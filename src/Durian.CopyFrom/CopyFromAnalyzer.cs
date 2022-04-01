// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using static Durian.Analysis.CopyFrom.CopyFromDiagnostics;

namespace Durian.Analysis.CopyFrom
{
	/// <summary>
	/// Analyzes members marked with either <c>Durian.CopyFromTypeAttribute</c> or <c>Durian.CopyFromMethodAttribute</c>.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class CopyFromAnalyzer : DurianAnalyzer<CopyFromCompilationData>
	{
		private enum TargetAccessor
		{
			None,
			GetOrRemove,
			SetOrAdd
		}

		/// <inheritdoc/>
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			DUR0201_ContainingTypeMustBePartial,
			DUR0202_MemberMustBePartial,
			DUR0203_MemberCannotBeResolved,
			DUR0204_WrongTargetMemberKind,
			DUR0205_ImplementationNotAccessible,
			DUR0206_EquivalentTarget,
			DUR0207_MemberCannotCopyFromItselfOrItsParent,
			DUR0208_MemberConflict,
			DUR0209_CannotCopyFromMethodWithoutImplementation,
			DUR0210_InvalidMethodKind,
			DUR0211_MethodAlreadyHasImplementation,
			DUR0212_TargetDoesNotHaveReturnType,
			DUR0213_TargetCannotHaveReturnType,
			DUR0214_InvalidPatternAttributeSpecified,
			DUR0215_RedundantPatternAttribute,
			DUR0216_EquivalentPatternAttribute,
			DUR0217_TypeParameterIsNotValid,
			DUR0218_UnknownPartialPartName,
			DUR0219_PatternOnDifferentDeclaration,
			DUR0220_UsingAlreadySpecified
		);

		/// <summary>
		/// Initializes a new instance of the <see cref="CopyFromAnalyzer"/> class.
		/// </summary>
		public CopyFromAnalyzer()
		{
		}

		/// <inheritdoc cref="Analyze(IMethodSymbol, CopyFromCompilationData, SemanticModel, IDiagnosticReceiver, MethodDeclarationSyntax)"/>
		public static bool Analyze(
			IMethodSymbol method,
			CopyFromCompilationData compilation,
			SemanticModel semanticModel,
			MethodDeclarationSyntax? declaration = default
		)
		{
			if (compilation.HasErrors || !ShouldAnalyze(method))
			{
				return false;
			}

			AttributeData? attribute = method.GetAttribute(compilation.CopyFromMethodAttribute!);

			if (attribute is null || !ValidateMarkedMethod(method, ref declaration))
			{
				return false;
			}

			return ValidateTargetMethod(method, semanticModel, compilation, declaration, attribute, out _);
		}

		/// <inheritdoc cref="Analyze(INamedTypeSymbol, CopyFromCompilationData, SemanticModel, IDiagnosticReceiver)"/>
		public static bool Analyze(INamedTypeSymbol type, CopyFromCompilationData compilation, SemanticModel semanticModel)
		{
			if (compilation.HasErrors || !ShouldAnalyze(type))
			{
				return false;
			}

			AttributeData[] attributes = type.GetAttributes(compilation.CopyFromTypeAttribute!).ToArray();

			if (attributes.Length == 0 || !EnsureIsInPartialContext(type))
			{
				return false;
			}

			return ValidateTargetTypes(type, semanticModel, compilation, attributes, out _, true);
		}

		/// <summary>
		/// Performs analysis of the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to analyze.</param>
		/// <param name="compilation">Current <see cref="CopyFromCompilationData"/>.</param>
		/// <param name="semanticModel">Current <see cref="SemanticModel"/>.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		public static bool Analyze(
			INamedTypeSymbol type,
			CopyFromCompilationData compilation,
			SemanticModel semanticModel,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			if (compilation.HasErrors || !ShouldAnalyze(type))
			{
				return false;
			}

			bool isValid = AnalyzeTypeWithoutPattern(
				type,
				compilation,
				semanticModel,
				out ImmutableArray<AttributeData> attributes,
				out List<TargetData>? targetTypes,
				diagnosticReceiver
			);

			AnalyzePattern(type, attributes, compilation, targetTypes?.Count > 0, diagnosticReceiver);
			return isValid;
		}

		/// <summary>
		/// Performs analysis of the specified <paramref name="method"/>.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to analyze.</param>
		/// <param name="compilation">Current <see cref="CopyFromCompilationData"/>.</param>
		/// <param name="semanticModel">Current <see cref="SemanticModel"/>.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <param name="declaration"><see cref="MethodDeclarationSyntax"/> associated with the <paramref name="method"/>.</param>
		/// <exception cref="InvalidOperationException"><paramref name="method"/> is not associated with a <see cref="MethodDeclarationSyntax"/>.</exception>
		public static bool Analyze(
			IMethodSymbol method,
			CopyFromCompilationData compilation,
			SemanticModel semanticModel,
			IDiagnosticReceiver diagnosticReceiver,
			MethodDeclarationSyntax? declaration = default
		)
		{
			if (compilation.HasErrors || !ShouldAnalyze(method))
			{
				return false;
			}

			bool isValid = AnalyzeMethodWithoutPattern(
				method,
				compilation,
				semanticModel,
				declaration,
				out ImmutableArray<AttributeData> attributes,
				out IMethodSymbol? targetMethod,
				diagnosticReceiver
			);

			AnalyzePattern(method, attributes, compilation, targetMethod is not null, diagnosticReceiver);
			return isValid;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> should be a subject of analysis.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if should be analyzed.</param>
		public static bool ShouldAnalyze(INamedTypeSymbol symbol)
		{
			return IsValidTarget(symbol);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> should be a subject of analysis.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if should be analyzed.</param>
		public static bool ShouldAnalyze(IMethodSymbol symbol)
		{
			return symbol.MethodKind is
				not MethodKind.BuiltinOperator and
				not MethodKind.Constructor and
				not MethodKind.DelegateInvoke and
				not MethodKind.EventRaise and
				not MethodKind.FunctionPointerSignature and
				not MethodKind.ReducedExtension;
		}

		/// <inheritdoc/>
		public override void Register(IDurianAnalysisContext context, CopyFromCompilationData compilation)
		{
			context.RegisterSyntaxNodeAction(c => AnalyzeAttributeSyntax(c, compilation), SyntaxKind.Attribute);
		}

		/// <inheritdoc/>
		protected override CopyFromCompilationData CreateCompilation(CSharpCompilation compilation, IDiagnosticReceiver diagnosticReceiver)
		{
			return new CopyFromCompilationData(compilation);
		}

		internal static bool AnalyzeMethodWithoutPattern(
			IMethodSymbol method,
			CopyFromCompilationData compilation,
			SemanticModel semanticModel,
			MethodDeclarationSyntax? declaration,
			out ImmutableArray<AttributeData> attributes,
			[NotNullWhen(true)] out IMethodSymbol? targetMethod,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			AttributeData? copyFrom = GetAttribute(method, compilation, out attributes);

			if (copyFrom is null)
			{
				targetMethod = default;
				return false;
			}

			bool isValid = ValidateMarkedMethod(method, ref declaration, diagnosticReceiver);

			if (declaration is null)
			{
				targetMethod = default;
			}
			else
			{
				isValid &= ValidateTargetMethod(method, semanticModel, compilation, declaration, copyFrom, out targetMethod, diagnosticReceiver);
			}

			return isValid;
		}

		internal static bool AnalyzeMethodWithoutPattern(
			IMethodSymbol method,
			CopyFromCompilationData compilation,
			SemanticModel semanticModel,
			MethodDeclarationSyntax? declaration,
			out ImmutableArray<AttributeData> attributes,
			[NotNullWhen(true)] out IMethodSymbol? targetMethod
		)
		{
			AttributeData? copyFrom = GetAttribute(method, compilation, out attributes);

			if (copyFrom is null)
			{
				targetMethod = default;
				return false;
			}

			if (ValidateMarkedMethod(method, ref declaration))
			{
				return ValidateTargetMethod(method, semanticModel, compilation, declaration, copyFrom, out targetMethod);
			}

			targetMethod = default;
			return false;
		}

		internal static bool AnalyzePattern(
			ISymbol symbol,
			AttributeData patternAttribute,
			HashSet<string> patterns,
			bool hasTarget,
			[NotNullWhen(true)] out string? pattern,
			[NotNullWhen(true)] out string? replacement,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			Location? location = null;
			bool isValid = true;

			if (!hasTarget)
			{
				location = patternAttribute.GetLocation();
				diagnosticReceiver.ReportDiagnostic(DUR0215_RedundantPatternAttribute, location, symbol);
				isValid = false;
			}

			if (HasValidRegexPattern(patternAttribute, out pattern, out replacement))
			{
				if (!patterns.Add(pattern))
				{
					diagnosticReceiver.ReportDiagnostic(DUR0216_EquivalentPatternAttribute, location ?? patternAttribute.GetLocation(), symbol);
					isValid = false;
				}
			}
			else
			{
				diagnosticReceiver.ReportDiagnostic(DUR0214_InvalidPatternAttributeSpecified, location ?? patternAttribute.GetLocation(), symbol);
				isValid = false;
			}

			return isValid;
		}

		internal static bool AnalyzeTypeWithoutPattern(
			INamedTypeSymbol type,
			CopyFromCompilationData compilation,
			SemanticModel semanticModel,
			out ImmutableArray<AttributeData> attributes,
			[NotNullWhen(true)] out List<TargetData>? targetTypes,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			AttributeData[]? copyFroms = GetAttributes(type, compilation, out attributes);

			if (copyFroms is null)
			{
				targetTypes = default;
				return false;
			}

			bool isValid = EnsureIsInPartialContext(type, diagnosticReceiver);
			isValid &= ValidateTargetTypes(type, semanticModel, compilation, copyFroms, out targetTypes, diagnosticReceiver);

			return isValid;
		}

		internal static bool AnalyzeTypeWithoutPattern(
			INamedTypeSymbol type,
			CopyFromCompilationData compilation,
			SemanticModel semanticModel,
			out ImmutableArray<AttributeData> attributes,
			[NotNullWhen(true)] out List<TargetData>? targetTypes
		)
		{
			AttributeData[]? copyFroms = GetAttributes(type, compilation, out attributes);

			if (copyFroms is null)
			{
				targetTypes = default;
				return false;
			}

			if (EnsureIsInPartialContext(type))
			{
				return ValidateTargetTypes(type, semanticModel, compilation, copyFroms, out targetTypes, true);
			}

			targetTypes = default;
			return false;
		}

		internal static bool HasValidRegexPattern(
			AttributeData attribute,
			[NotNullWhen(true)] out string? pattern,
			[NotNullWhen(true)] out string? replacement
		)
		{
			ImmutableArray<TypedConstant> arguments = attribute.ConstructorArguments;

			if (arguments.Length < 2 || arguments[0].Value is not string p || string.IsNullOrEmpty(p) || arguments[1].Value is not string r)
			{
				pattern = default;
				replacement = default;
				return false;
			}

			pattern = p;
			replacement = r;
			return true;
		}

		internal static void ReportEquivalentPattern(IDiagnosticReceiver diagnosticReceiver, Location? location, ISymbol symbol)
		{
			diagnosticReceiver.ReportDiagnostic(DUR0216_EquivalentPatternAttribute, location, symbol);
		}

		private static void AnalyzeAttributeSyntax(SyntaxNodeAnalysisContext context, CopyFromCompilationData compilation)
		{
			if (context.Node is not AttributeSyntax attr || attr.ArgumentList is null || attr.Parent?.Parent is not CSharpSyntaxNode member)
			{
				return;
			}

			if (member is
				not MemberDeclarationSyntax and
				not AccessorDeclarationSyntax and
				not LocalFunctionStatementSyntax and
				not LambdaExpressionSyntax
			)
			{
				return;
			}

			if (context.SemanticModel.GetSymbolInfo(attr).Symbol is not IMethodSymbol attributeSymbol)
			{
				return;
			}

			if (SymbolEqualityComparer.Default.Equals(attributeSymbol.ContainingType, compilation.CopyFromTypeAttribute))
			{
				AnalyzeTypeAttribute(context, compilation, attr, member);
			}
			else if (SymbolEqualityComparer.Default.Equals(attributeSymbol.ContainingType, compilation.CopyFromMethodAttribute))
			{
				AnalyzeMethodAttribute(context, compilation, member);
			}
			else if (SymbolEqualityComparer.Default.Equals(attributeSymbol.ContainingType, compilation.PatternAttribute))
			{
				AnalyzePatternAttribute(context, compilation, attr, member);
			}
		}

		private static void AnalyzeMethodAttribute(SyntaxNodeAnalysisContext context, CopyFromCompilationData compilation, CSharpSyntaxNode member)
		{
			IMethodSymbol? methodSymbol = member is LambdaExpressionSyntax
				? context.SemanticModel.GetSymbolInfo(member).Symbol as IMethodSymbol
				: context.SemanticModel.GetDeclaredSymbol(member) as IMethodSymbol;

			if (methodSymbol is not null && ShouldAnalyze(methodSymbol))
			{
				DiagnosticReceiver.Contextual<SyntaxNodeAnalysisContext> diagnosticReceiver = DiagnosticReceiver.Factory.SyntaxNode(context);
				AnalyzeMethodWithoutPattern(methodSymbol, compilation, context.SemanticModel, member as MethodDeclarationSyntax, out _, out _, diagnosticReceiver);
			}
		}

		private static void AnalyzePattern(
			ISymbol symbol,
			ImmutableArray<AttributeData> attributes,
			CopyFromCompilationData compilation,
			bool hasTarget,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			AttributeData[] patternAttributes = GetAttributes(attributes, compilation.PatternAttribute!);

			if (patternAttributes.Length == 0)
			{
				return;
			}

			HashSet<string> set = new();

			foreach (AttributeData pattern in patternAttributes)
			{
				AnalyzePattern(symbol, pattern, set, hasTarget, out _, out _, diagnosticReceiver);
			}
		}

		private static bool AnalyzePattern(
			ISymbol symbol,
			AttributeSyntax attrSyntax,
			CopyFromCompilationData compilation,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			AttributeData? currentData = null;
			HashSet<string> patterns = new();
			bool hasCopyFrom = false;

			foreach (AttributeData attribute in symbol.GetAttributes())
			{
				if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, compilation.PatternAttribute))
				{
					if (currentData is null && attribute.ApplicationSyntaxReference?.Span == attrSyntax.Span)
					{
						currentData = attribute;
					}
					else if (attribute.ConstructorArguments[0].Value is string pattern && !string.IsNullOrEmpty(pattern))
					{
						patterns.Add(pattern);
					}
				}
				else if (
					!hasCopyFrom &&
					(SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, compilation.CopyFromMethodAttribute) ||
					SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, compilation.CopyFromTypeAttribute)))
				{
					hasCopyFrom = true;
				}
			}

			if (currentData is null)
			{
				return false;
			}

			return AnalyzePattern(symbol, currentData, patterns, hasCopyFrom, out _, out _, diagnosticReceiver);
		}

		private static void AnalyzePatternAttribute(SyntaxNodeAnalysisContext context, CopyFromCompilationData compilation, AttributeSyntax attr, CSharpSyntaxNode member)
		{
			if (context.SemanticModel.GetDeclaredSymbol(member) is ISymbol symbol)
			{
				DiagnosticReceiver.Contextual<SyntaxNodeAnalysisContext> diagnosticReceiver = DiagnosticReceiver.Factory.SyntaxNode(context);
				AnalyzePattern(symbol, attr, compilation, diagnosticReceiver);
			}
		}

		private static void AnalyzeTypeAttribute(SyntaxNodeAnalysisContext context, CopyFromCompilationData compilation, AttributeSyntax attr, CSharpSyntaxNode member)
		{
			if (context.SemanticModel.GetDeclaredSymbol(member) is INamedTypeSymbol typeSymbol && ShouldAnalyze(typeSymbol))
			{
				DiagnosticReceiver.Contextual<SyntaxNodeAnalysisContext> diagnosticReceiver = DiagnosticReceiver.Factory.SyntaxNode(context);
				AnalyzeTypeWithoutPattern(typeSymbol, compilation, context.SemanticModel, attr, diagnosticReceiver);
			}
		}

		private static bool AnalyzeTypeWithoutPattern(
			INamedTypeSymbol type,
			CopyFromCompilationData compilation,
			SemanticModel semanticModel,
			AttributeSyntax syntax,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			AttributeData[]? copyFroms = GetAttributes(type, compilation, out _);

			if (copyFroms is null)
			{
				return false;
			}

			int index = Array.FindIndex(copyFroms, a => a.ApplicationSyntaxReference?.Span == syntax.Span);

			if (index == -1)
			{
				return false;
			}

			AttributeData[] targets = new AttributeData[copyFroms.Length - 1];
			Array.Copy(copyFroms, 0, targets, 0, index);
			Array.Copy(copyFroms, index + 1, targets, index, targets.Length - index);

			ValidateTargetTypes(type, semanticModel, compilation, targets, out List<TargetData>? targetTypes, false);

			bool isValid = EnsureIsInPartialContext(type, diagnosticReceiver);
			isValid &= ValidateTargetType(type, semanticModel, copyFroms[index], compilation, targetTypes!);

			return isValid;
		}

		private static bool CopiesFromItself(IMethodSymbol method, IMethodSymbol target)
		{
			if (SymbolEqualityComparer.Default.Equals(method, target))
			{
				return true;
			}

			if (target.ConstructedFrom is not null)
			{
				if (SymbolEqualityComparer.Default.Equals(method, target.ConstructedFrom))
				{
					return true;
				}

				target = target.ConstructedFrom;
			}

			return method.Overrides(target) || target.Overrides(method);
		}

		private static bool CopiesFromItself(INamedTypeSymbol type, INamedTypeSymbol target)
		{
			if (SymbolEqualityComparer.Default.Equals(type, target))
			{
				return true;
			}

			if (type.InheritsFrom(target) || target.InheritsFrom(type))
			{
				return true;
			}

			if (target.ContainsSymbol(type))
			{
				return true;
			}

			return false;
		}

		private static TargetData CreateTargetData(AttributeData attribute, INamedTypeSymbol target)
		{
			return CreateTargetData(attribute, target, default, default);
		}

		private static TargetData CreateTargetData(AttributeData attribute, INamedTypeSymbol target, TypeDeclarationSyntax? partialPart, string? partialPartName)
		{
			int order = GetOrder(attribute);
			string[] usings = GetUsings(attribute);
			bool copyUsings = ShouldCopyUsings(attribute);
			bool handleSpecialMembers = ShouldHandleSpecialMembers(attribute);

			return new(target, order, partialPart, partialPartName, copyUsings, usings, handleSpecialMembers);
		}

		private static bool EnsureIsInPartialContext(IMethodSymbol method, [NotNullWhen(true)] ref MethodDeclarationSyntax? declaration)
		{
			if (!method.IsPartialDefinition || method.PartialImplementationPart is not null)
			{
				return false;
			}

			if (declaration is null && !method.TryGetSyntax(out declaration))
			{
				return false;
			}

			return method.IsPartialContext(declaration);
		}

		private static bool EnsureIsInPartialContext(IMethodSymbol method, [NotNullWhen(true)] ref MethodDeclarationSyntax? declaration, IDiagnosticReceiver diagnosticReceiver)
		{
			if (method.MethodKind != MethodKind.Ordinary)
			{
				return false;
			}

			bool success = true;

			if (method.IsPartialDefinition)
			{
				if (method.PartialImplementationPart is not null)
				{
					diagnosticReceiver.ReportDiagnostic(DUR0211_MethodAlreadyHasImplementation);
					success = false;
				}
			}
			else
			{
				if (method.PartialDefinitionPart is null)
				{
					if (declaration is null && !method.TryGetSyntax(out declaration))
					{
						return false;
					}

					if (!method.IsPartial(declaration))
					{
						diagnosticReceiver.ReportDiagnostic(DUR0202_MemberMustBePartial, method);
					}
					else
					{
						diagnosticReceiver.ReportDiagnostic(DUR0211_MethodAlreadyHasImplementation);
					}
				}
				else
				{
					diagnosticReceiver.ReportDiagnostic(DUR0211_MethodAlreadyHasImplementation);
				}

				success = false;
			}

			foreach (INamedTypeSymbol baseType in method.GetContainingTypes())
			{
				if (!baseType.IsPartial())
				{
					diagnosticReceiver.ReportDiagnostic(DUR0201_ContainingTypeMustBePartial, baseType);
					success = false;
				}
			}

			return success;
		}

		private static bool EnsureIsInPartialContext(INamedTypeSymbol type)
		{
			return type.IsPartialContext();
		}

		private static bool EnsureIsInPartialContext(INamedTypeSymbol type, IDiagnosticReceiver diagnosticReceiver)
		{
			bool success = true;

			if (!type.IsPartial())
			{
				diagnosticReceiver.ReportDiagnostic(DUR0202_MemberMustBePartial, type);
				success = false;
			}

			foreach (INamedTypeSymbol baseType in type.GetContainingTypes())
			{
				if (!baseType.IsPartial())
				{
					diagnosticReceiver.ReportDiagnostic(DUR0201_ContainingTypeMustBePartial, baseType);
					success = false;
				}
			}

			return success;
		}

		private static bool EnsureIsValidMethodType(IMethodSymbol method, [NotNullWhen(false)] out DiagnosticDescriptor? diagnostic)
		{
			if (!IsValidMethodType(method))
			{
				diagnostic = DUR0210_InvalidMethodKind;
				return false;
			}

			diagnostic = null;
			return true;
		}

		private static IMethodSymbol? EnsureIsValidTarget(
			IMethodSymbol currentMethod,
			ISymbol symbol,
			TargetAccessor accessor,
			out DiagnosticDescriptor? diagnostic
		)
		{
			switch (symbol)
			{
				case IMethodSymbol method:

					if (SymbolEqualityComparer.Default.Equals(currentMethod, method))
					{
						diagnostic = DUR0207_MemberCannotCopyFromItselfOrItsParent;
						return null;
					}

					if (IsValidTarget(method) && !method.HasImplementation())
					{
						diagnostic = DUR0209_CannotCopyFromMethodWithoutImplementation;
						return null;
					}

					if (currentMethod.ReturnsVoid)
					{
						if (!method.ReturnsVoid)
						{
							diagnostic = DUR0213_TargetCannotHaveReturnType;
							return null;
						}
					}
					else if (method.ReturnsVoid)
					{
						diagnostic = DUR0212_TargetDoesNotHaveReturnType;
						return null;
					}

					diagnostic = null;
					return method;

				case IPropertySymbol property:

					if (property.IsAutoProperty())
					{
						diagnostic = DUR0209_CannotCopyFromMethodWithoutImplementation;
						return null;
					}

					if (accessor != TargetAccessor.None)
					{
						if (accessor == TargetAccessor.SetOrAdd)
						{
							if (property.SetMethod is null)
							{
								diagnostic = DUR0203_MemberCannotBeResolved;
								return null;
							}
						}
						else if (property.GetMethod is null)
						{
							diagnostic = DUR0203_MemberCannotBeResolved;
							return null;
						}
					}

					if (currentMethod.ReturnsVoid)
					{
						if (property.SetMethod is null)
						{
							diagnostic = DUR0213_TargetCannotHaveReturnType;
							return null;
						}
						else
						{
							diagnostic = null;
							return property.SetMethod;
						}
					}
					else if (property.GetMethod is null)
					{
						diagnostic = DUR0212_TargetDoesNotHaveReturnType;
						return null;
					}
					else
					{
						diagnostic = null;
						return property.GetMethod;
					}

				case IEventSymbol @event:

					if (accessor != TargetAccessor.None)
					{
						if (!currentMethod.ReturnsVoid)
						{
							diagnostic = DUR0212_TargetDoesNotHaveReturnType;
							return null;
						}

						if (accessor == TargetAccessor.SetOrAdd)
						{
							if (@event.AddMethod is not null)
							{
								diagnostic = null;
								return @event.AddMethod;
							}
						}
						else if (@event.RemoveMethod is not null)
						{
							diagnostic = null;
							return @event.RemoveMethod;
						}
					}

					break;
			}

			diagnostic = DUR0204_WrongTargetMemberKind;
			return null;
		}

		private static AttributeData? GetAttribute(IMethodSymbol method, CopyFromCompilationData compilation, out ImmutableArray<AttributeData> allAttributes)
		{
			allAttributes = method.GetAttributes();

			if (allAttributes.Length == 0)
			{
				return default;
			}

			return allAttributes.FirstOrDefault(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, compilation.CopyFromMethodAttribute));
		}

		private static AttributeData[]? GetAttributes(INamedTypeSymbol type, CopyFromCompilationData compilation, out ImmutableArray<AttributeData> allAttributes)
		{
			allAttributes = type.GetAttributes();

			if (allAttributes.Length == 0)
			{
				return default;
			}

			AttributeData[] copyFroms = GetAttributes(allAttributes, compilation.CopyFromTypeAttribute!);

			if (copyFroms.Length == 0)
			{
				return default;
			}

			return copyFroms;
		}

		private static AttributeData[] GetAttributes(ImmutableArray<AttributeData> attributes, INamedTypeSymbol attrSymbol)
		{
			return attributes.Where(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attrSymbol)).ToArray();
		}

		private static int GetOrder(AttributeData attribute)
		{
			return attribute.GetNamedArgumentValue<int>(CopyFromTypeAttributeProvider.Order);
		}

		private static SymbolInfo GetSymbolFromCrefSyntax(
			string text,
			CSharpCompilation compilation,
			MemberDeclarationSyntax declaration,
			out CSharpSyntaxNode cref
		)
		{
			const string annotation = "copyFrom - inheritdoc";

			text = AnalysisUtilities.ToXmlCompatible(text);
			string parse = $"/// <inheritdoc cref=\"{text}\"/>";

			SyntaxNode root = CSharpSyntaxTree.ParseText(parse, encoding: Encoding.UTF8).GetRoot();
			DocumentationCommentTriviaSyntax trivia = root.DescendantNodes(descendIntoTrivia: true).OfType<DocumentationCommentTriviaSyntax>().First();

			trivia = trivia.WithAdditionalAnnotations(new SyntaxAnnotation(annotation));

			root = declaration.SyntaxTree.GetRoot();
			root = root.ReplaceNode(declaration, declaration.WithLeadingTrivia(SyntaxFactory.Trivia(trivia)));
			compilation = compilation.ReplaceSyntaxTree(declaration.SyntaxTree, root.SyntaxTree);

#pragma warning disable RS1030 // Do not invoke Compilation.GetSemanticModel() method within a diagnostic analyzer
			SemanticModel createdSemanticModel = compilation.GetSemanticModel(root.SyntaxTree, true);
#pragma warning restore RS1030 // Do not invoke Compilation.GetSemanticModel() method within a diagnostic analyzer

			trivia = (DocumentationCommentTriviaSyntax)root.GetAnnotatedNodes(annotation).First();

			cref = trivia.DescendantNodes().OfType<CrefSyntax>().First();
			return createdSemanticModel.GetSymbolInfo(cref);
		}

		private static TargetAccessor GetTargetAccessor(ref string text)
		{
			if (text.Length < 5)
			{
				return TargetAccessor.None;
			}

			if (text[text.Length - 4] == '_')
			{
				if (text.EndsWith("get"))
				{
					text = text.Substring(0, text.Length - 4);
					return TargetAccessor.GetOrRemove;
				}

				if (text.EndsWith("set") || text.EndsWith("add"))
				{
					text = text.Substring(0, text.Length - 4);
					return TargetAccessor.SetOrAdd;
				}
			}
			else if (text.EndsWith("_remove"))
			{
				text = text.Substring(0, text.Length - 7);
				return TargetAccessor.GetOrRemove;
			}

			return TargetAccessor.None;
		}

		private static IMethodSymbol? GetTargetMethod(
			IMethodSymbol currentMethod,
			AttributeData attribute,
			SemanticModel semanticModel,
			CSharpCompilation compilation,
			MemberDeclarationSyntax declaration,
			out DiagnosticDescriptor? diagnostic,
			out object? value
		)
		{
			if (attribute.ApplicationSyntaxReference is null)
			{
				diagnostic = DUR0203_MemberCannotBeResolved;
				value = default;
				return null;
			}

			TypedConstant constant = attribute.GetConstructorArgument(0);

			if (constant.IsNull || constant.Value is not string text)
			{
				diagnostic = DUR0203_MemberCannotBeResolved;
				value = default;
				return null;
			}

			text = text.Trim();
			TargetAccessor accessor = GetTargetAccessor(ref text);
			value = text;

			SymbolInfo info;

			if (TryParseName(text, out CSharpSyntaxNode? syntax))
			{
				info = semanticModel.GetSpeculativeSymbolInfo(declaration.SpanStart, syntax, SpeculativeBindingOption.BindAsExpression);
			}
			else
			{
				info = GetSymbolFromCrefSyntax(text, compilation, declaration, out _);
			}

			if (info.Symbol is not null)
			{
				return EnsureIsValidTarget(currentMethod, info.Symbol, accessor, out diagnostic);
			}
			else if (info.CandidateReason is CandidateReason.OverloadResolutionFailure or CandidateReason.Inaccessible)
			{
				if (info.CandidateSymbols.Length == 1)
				{
					return EnsureIsValidTarget(currentMethod, info.CandidateSymbols[0], accessor, out diagnostic);
				}
				else
				{
					diagnostic = DUR0208_MemberConflict;
					return null;
				}
			}
			else if (info.CandidateReason is CandidateReason.Ambiguous or CandidateReason.MemberGroup)
			{
				diagnostic = DUR0208_MemberConflict;
				return null;
			}

			diagnostic = DUR0203_MemberCannotBeResolved;
			return null;
		}

		private static INamedTypeSymbol? GetTargetType(
			AttributeData attribute,
			SemanticModel semanticModel,
			out DiagnosticDescriptor? diagnostic,
			out object? value
		)
		{
			TypedConstant constant = attribute.GetConstructorArgument(0);

			if (constant.IsNull)
			{
				diagnostic = DUR0203_MemberCannotBeResolved;
				value = default;
				return null;
			}

			if (constant.Value is ITypeSymbol target)
			{
				if (target is not INamedTypeSymbol named)
				{
					diagnostic = DUR0204_WrongTargetMemberKind;
					value = target;
					return null;
				}

				if (!IsValidTarget(named))
				{
					if (named is IErrorTypeSymbol error)
					{
						if (error.CandidateReason is CandidateReason.Ambiguous)
						{
							diagnostic = DUR0208_MemberConflict;
							value = error;
							return null;
						}

						if (error.CandidateReason is CandidateReason.Inaccessible && error.CandidateSymbols[0] is INamedTypeSymbol t && IsValidTarget(t))
						{
							diagnostic = null;
							value = t;
							return t;
						}
					}

					diagnostic = DUR0204_WrongTargetMemberKind;
					value = target;
					return null;
				}

				diagnostic = null;
				value = named;
				return named;
			}

			if (constant.Value is string text)
			{
				value = text;
				return GetTargetTypeFromString(text, attribute, semanticModel, out diagnostic);
			}

			diagnostic = DUR0203_MemberCannotBeResolved;
			value = default;
			return null;
		}

		private static INamedTypeSymbol? GetTargetTypeFromString(
			string text,
			AttributeData attribute,
			SemanticModel semanticModel,
			out DiagnosticDescriptor? diagnostic
		)
		{
			if (attribute.ApplicationSyntaxReference is null)
			{
				diagnostic = DUR0203_MemberCannotBeResolved;
				return null;
			}

			TypeSyntax syntax = SyntaxFactory.ParseTypeName(text);

			SymbolInfo symbol = semanticModel.GetSpeculativeSymbolInfo(attribute.ApplicationSyntaxReference!.Span.Start, syntax, SpeculativeBindingOption.BindAsTypeOrNamespace);

			if (symbol.Symbol is not null)
			{
				if (symbol.Symbol is INamedTypeSymbol named && IsValidTarget(named))
				{
					diagnostic = null;
					return named;
				}

				diagnostic = DUR0204_WrongTargetMemberKind;
				return null;
			}
			else if (symbol.CandidateReason == CandidateReason.Inaccessible)
			{
				diagnostic = DUR0205_ImplementationNotAccessible;
				return null;
			}
			else if (symbol.CandidateReason == CandidateReason.Ambiguous)
			{
				diagnostic = DUR0208_MemberConflict;
				return null;
			}

			diagnostic = DUR0203_MemberCannotBeResolved;
			return null;
		}

		private static string[] GetUsings(AttributeData attribute)
		{
			return attribute.GetNamedArgumentArrayValue<string>(CopyFromTypeAttributeProvider.AddUsings).ToArray();
		}

		private static bool HasValidTypeArguments(INamedTypeSymbol type, out List<ITypeSymbol>? invalidArguments)
		{
			if (type.IsUnboundGenericType)
			{
				invalidArguments = default;
				return true;
			}

			return HasValidTypeArguments(type.TypeParameters, type.TypeArguments, out invalidArguments);
		}

		private static bool HasValidTypeArguments(IMethodSymbol method, out List<ITypeSymbol>? invalidArguments)
		{
			return HasValidTypeArguments(method.TypeParameters, method.TypeArguments, out invalidArguments);
		}

		private static bool HasValidTypeArguments(
			ImmutableArray<ITypeParameterSymbol> typeParameters,
			ImmutableArray<ITypeSymbol> typeArguments,
			out List<ITypeSymbol>? invalidArguments
		)
		{
			if (typeParameters.Length == 0)
			{
				invalidArguments = default;
				return true;
			}

			if (typeParameters.Length != typeArguments.Length)
			{
				invalidArguments = default;
				return false;
			}

			List<ITypeSymbol> invalid = new(typeArguments.Length);

			for (int i = 0; i < typeParameters.Length; i++)
			{
				ITypeSymbol arg = typeArguments[i];
				ITypeParameterSymbol param = typeParameters[i];

				if (arg.Name != param.Name && !arg.IsValidForTypeParameter(param))
				{
					invalid.Add(arg);
				}
			}

			if (invalid.Count > 0)
			{
				invalidArguments = invalid;
				return false;
			}

			invalidArguments = default;
			return true;
		}

		private static bool IsInDifferentAssembly(INamedTypeSymbol type, INamedTypeSymbol target)
		{
			return !SymbolEqualityComparer.Default.Equals(type.ContainingAssembly, target.ContainingAssembly);
		}

		private static bool IsInDifferentAssembly(IMethodSymbol target, CopyFromCompilationData compilation)
		{
			return compilation.Compilation.Assembly.GetTypeByMetadataName(target.ContainingType.ToString()) is null;
		}

		private static bool IsValidMethodType(IMethodSymbol method)
		{
			return !method.IsAbstract && !method.IsExtern && method.MethodKind == MethodKind.Ordinary;
		}

		private static bool IsValidTarget(INamedTypeSymbol symbol)
		{
			return symbol.TypeKind is TypeKind.Class or TypeKind.Struct or TypeKind.Interface;
		}

		private static bool IsValidTarget(IMethodSymbol symbol)
		{
			return symbol.MethodKind is
				MethodKind.Ordinary or
				MethodKind.Constructor or
				MethodKind.UserDefinedOperator or
				MethodKind.Conversion;
		}

		private static bool ShouldCopyUsings(AttributeData attribute)
		{
			if (attribute.TryGetNamedArgumentValue(CopyFromTypeAttributeProvider.CopyUsings, out bool value))
			{
				return value;
			}

			return true;
		}

		private static bool ShouldHandleSpecialMembers(AttributeData attribute)
		{
			if (attribute.TryGetNamedArgumentValue(CopyFromTypeAttributeProvider.HandleSpecialMembers, out bool value))
			{
				return value;
			}

			return true;
		}

		private static bool TryGetPartialPart(
			INamedTypeSymbol target,
			CopyFromCompilationData compilation,
			string? partialPartName,
			[NotNullWhen(true)] out TypeDeclarationSyntax? partialPart
		)
		{
			partialPart = default;

			if (partialPartName is null)
			{
				return false;
			}

			foreach (AttributeData attr in target.GetAttributes(compilation.PartialNameAttribute!))
			{
				if (attr.GetConstructorArgumentValue<string>(0) == partialPartName)
				{
					partialPart = attr.ApplicationSyntaxReference?.GetSyntax()?.Parent?.Parent as TypeDeclarationSyntax;
					break;
				}
			}

			return partialPart is not null;
		}

		private static bool TryGetPartialPartName(AttributeData attribute, out string? partialPartName)
		{
			return attribute.TryGetNamedArgumentValue(CopyFromTypeAttributeProvider.PartialPart, out partialPartName);
		}

		private static bool TryParseName(string text, [NotNullWhen(true)] out CSharpSyntaxNode? name)
		{
			if (text.IndexOf('(') == -1 && text.IndexOf('[') == -1)
			{
				name = SyntaxFactory.ParseName(text);
				return true;
			}

			name = default;
			return false;
		}

		private static bool ValidateMarkedMethod(IMethodSymbol method, [NotNullWhen(true)] ref MethodDeclarationSyntax? declaration, IDiagnosticReceiver diagnosticReceiver)
		{
			bool isValid = EnsureIsValidMethodType(method, out DiagnosticDescriptor? diagnostic);

			if (!isValid)
			{
				diagnosticReceiver.ReportDiagnostic(diagnostic!, method);

				if (method.MethodKind != MethodKind.Ordinary)
				{
					return false;
				}
			}

			isValid &= EnsureIsInPartialContext(method, ref declaration, diagnosticReceiver);

			return isValid;
		}

		private static bool ValidateMarkedMethod(IMethodSymbol method, [NotNullWhen(true)] ref MethodDeclarationSyntax? declaration)
		{
			if (!IsValidMethodType(method))
			{
				return false;
			}

			if (!EnsureIsInPartialContext(method, ref declaration))
			{
				return false;
			}

			return true;
		}

		private static bool ValidatePartialNameAndAddTarget(
			AttributeData attribute,
			INamedTypeSymbol type,
			INamedTypeSymbol target,
			CopyFromCompilationData compilation,
			List<TargetData> copyFromTypes,
			bool isValid,
			ref Location? location,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			if (TryGetPartialPartName(attribute, out string? partialPartName))
			{
				if (!TryGetPartialPart(target, compilation, partialPartName, out TypeDeclarationSyntax? partialPart))
				{
					location ??= attribute.GetLocation();
					diagnosticReceiver.ReportDiagnostic(DUR0218_UnknownPartialPartName, location, type);
					return false;
				}

				if (copyFromTypes.Any(t => SymbolEqualityComparer.Default.Equals(t.Symbol, target) && (t.PartialPartName is null || t.PartialPartName == partialPartName)))
				{
					location ??= attribute.GetLocation();
					diagnosticReceiver.ReportDiagnostic(DUR0206_EquivalentTarget, location, type);
				}
				else if (isValid)
				{
					copyFromTypes.Add(CreateTargetData(attribute, target, partialPart, partialPartName));
				}
			}
			else if (copyFromTypes.Any(t => SymbolEqualityComparer.Default.Equals(t.Symbol, target)))
			{
				location ??= attribute.GetLocation();
				diagnosticReceiver.ReportDiagnostic(DUR0206_EquivalentTarget, location, type);
			}
			else if (isValid)
			{
				copyFromTypes.Add(CreateTargetData(attribute, target));
			}

			return isValid;
		}

		private static bool ValidateTargetMethod(
			IMethodSymbol method,
			SemanticModel semanticModel,
			CopyFromCompilationData compilation,
			MemberDeclarationSyntax declaration,
			AttributeData attribute,
			[NotNullWhen(true)] out IMethodSymbol? targetMethod
		)
		{
			if (GetTargetMethod(method, attribute, semanticModel, compilation.Compilation, declaration, out _, out _) is IMethodSymbol target &&
				!CopiesFromItself(method, target) &&
				!IsInDifferentAssembly(method, compilation) &&
				HasValidTypeArguments(target, out _)
			)
			{
				targetMethod = target;
				return true;
			}

			targetMethod = default;
			return false;
		}

		private static bool ValidateTargetMethod(
			IMethodSymbol method,
			SemanticModel semanticModel,
			CopyFromCompilationData compilation,
			MemberDeclarationSyntax declaration,
			AttributeData attribute,
			[NotNullWhen(true)] out IMethodSymbol? targetMethod,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			if (GetTargetMethod(method, attribute, semanticModel, compilation.Compilation, declaration, out DiagnosticDescriptor? diagnostic, out object? value) is IMethodSymbol target)
			{
				bool isValid = true;
				Location? location = default;

				if (CopiesFromItself(method, target))
				{
					isValid = false;
					location ??= attribute.GetLocation();
					diagnosticReceiver.ReportDiagnostic(DUR0207_MemberCannotCopyFromItselfOrItsParent, location, method);
				}

				if (IsInDifferentAssembly(target, compilation))
				{
					isValid = false;
					location ??= attribute.GetLocation();
					diagnosticReceiver.ReportDiagnostic(DUR0205_ImplementationNotAccessible, location, method, target);
				}

				if (!HasValidTypeArguments(target, out List<ITypeSymbol>? invalidArguments))
				{
					isValid = false;

					if (invalidArguments is not null)
					{
						location ??= attribute.GetLocation();

						foreach (ITypeSymbol type in invalidArguments)
						{
							diagnosticReceiver.ReportDiagnostic(DUR0217_TypeParameterIsNotValid, location, method, type);
						}
					}
				}

				if (isValid)
				{
					targetMethod = target;
					return true;
				}
			}
			else if (diagnostic is not null)
			{
				diagnosticReceiver.ReportDiagnostic(diagnostic, attribute.GetLocation(), method, value);
			}

			targetMethod = default;
			return false;
		}

		private static bool ValidateTargetType(
			INamedTypeSymbol type,
			SemanticModel semanticModel,
			AttributeData attribute,
			CopyFromCompilationData compilation,
			List<TargetData> copyFromTypes
		)
		{
			if (GetTargetType(attribute, semanticModel, out _, out _) is not INamedTypeSymbol target ||
				CopiesFromItself(type, target) ||
				IsInDifferentAssembly(type, target) ||
				!HasValidTypeArguments(target, out _))
			{
				return false;
			}

			if (TryGetPartialPartName(attribute, out string? partialPartName))
			{
				if (!TryGetPartialPart(target, compilation, partialPartName, out TypeDeclarationSyntax? partialPart))
				{
					return false;
				}

				if (copyFromTypes.Any(t => SymbolEqualityComparer.Default.Equals(t.Symbol, target) && t.PartialPartName == partialPartName))
				{
					return true;
				}

				copyFromTypes.Add(CreateTargetData(attribute, target, partialPart, partialPartName));
				return true;
			}

			if (!copyFromTypes.Any(t => SymbolEqualityComparer.Default.Equals(t.Symbol, target)))
			{
				copyFromTypes.Add(CreateTargetData(attribute, target));
			}

			return true;
		}

		private static bool ValidateTargetType(
			INamedTypeSymbol type,
			SemanticModel semanticModel,
			AttributeData attribute,
			CopyFromCompilationData compilation,
			List<TargetData> copyFromTypes,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			if (GetTargetType(attribute, semanticModel, out DiagnosticDescriptor? diagnostic, out object? value) is not INamedTypeSymbol target)
			{
				if (diagnostic is not null)
				{
					diagnosticReceiver.ReportDiagnostic(diagnostic, attribute.GetLocation(), type, value);
				}

				return false;
			}

			bool isValid = true;
			Location? location = default;

			if (CopiesFromItself(type, target))
			{
				location ??= attribute.GetLocation();
				diagnosticReceiver.ReportDiagnostic(DUR0207_MemberCannotCopyFromItselfOrItsParent, location, type);
				isValid = false;
			}

			if (IsInDifferentAssembly(type, target))
			{
				location ??= attribute.GetLocation();
				diagnosticReceiver.ReportDiagnostic(DUR0205_ImplementationNotAccessible, location, type, target);
				isValid = false;
			}

			if (!HasValidTypeArguments(target, out List<ITypeSymbol>? invalidTypeArguments))
			{
				isValid = false;

				if (invalidTypeArguments is not null)
				{
					location ??= attribute.GetLocation();

					foreach (ITypeSymbol typeArgument in invalidTypeArguments)
					{
						diagnosticReceiver.ReportDiagnostic(DUR0217_TypeParameterIsNotValid, location, type, typeArgument);
					}
				}
			}

			return ValidatePartialNameAndAddTarget(attribute, type, target, compilation, copyFromTypes, isValid, ref location, diagnosticReceiver);
		}

		private static bool ValidateTargetTypes(
			INamedTypeSymbol type,
			SemanticModel semanticModel,
			CopyFromCompilationData compilation,
			AttributeData[] attributes,
			[NotNullWhen(true)] out List<TargetData>? targetTypes,
			bool returnIfInvalid
		)
		{
			List<TargetData> copyFromTypes = new();
			bool isValid = true;

			foreach (AttributeData attribute in attributes)
			{
				if (!ValidateTargetType(type, semanticModel, attribute, compilation, copyFromTypes))
				{
					if (returnIfInvalid)
					{
						targetTypes = default;
						return false;
					}

					isValid = false;
				}
			}

			targetTypes = copyFromTypes;
			return isValid;
		}

		private static bool ValidateTargetTypes(
			INamedTypeSymbol type,
			SemanticModel semanticModel,
			CopyFromCompilationData compilation,
			AttributeData[] attributes,
			[NotNullWhen(true)] out List<TargetData>? targetTypes,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			List<TargetData> copyFromTypes = new();

			bool isValid = true;

			foreach (AttributeData attribute in attributes)
			{
				if (!ValidateTargetType(type, semanticModel, attribute, compilation, copyFromTypes, diagnosticReceiver))
				{
					isValid = false;
				}
			}

			targetTypes = isValid ? copyFromTypes : default;
			return isValid;
		}
	}
}
