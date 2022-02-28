// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using static Durian.Analysis.CopyFrom.CopyFromDiagnostics;

namespace Durian.Analysis.CopyFrom
{
	/// <summary>
	/// Analyzes members marked with either <c>Durian.CopyFromTypeAttribute</c> or <c>Durian.CopyFromMethodAttribute</c>.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class CopyFromAnalyzer : DurianAnalyzer<CopyFromCompilationData>
	{
		private const string _annotation = "copyFrom - inheritdoc";

		/// <inheritdoc/>
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			DUR0201_ContainingTypeMustBePartial,
			DUR0202_MemberMustBePartial,
			DUR0203_MemberCannotBeResolved,
			DUR0204_WrongTargetMemberKind,
			DUR0205_ImplementationNotAccessible,
			DUR0206_EquivalentAttributes,
			DUR0207_MemberCannotCopyFromItselfOrItsParent,
			DUR0208_MemberConflict,
			DUR0209_CannotCopyFromMethodWithoutImplementation,
			DUR0210_InvalidMethodKind,
			DUR0211_MethodAlreadyHasImplementation,
			DUR0212_TargetDoesNotHaveReturnType,
			DUR0213_TargetCannotHaveReturnType,
			DUR0214_InvalidPatternAttributeSpecified,
			DUR0215_RedundantPatternAttribute
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
			AttributeData[] attributes = method.GetAttributes(compilation.CopyFromMethodAttribute!).ToArray();

			if (attributes.Length == 0 || !ValidateMarkedMethod(method, ref declaration))
			{
				return false;
			}

			return ValidateTargetMethods(method, semanticModel, compilation, declaration, attributes, out _);
		}

		/// <inheritdoc cref="Analyze(INamedTypeSymbol, CopyFromCompilationData, SemanticModel, IDiagnosticReceiver)"/>
		public static bool Analyze(INamedTypeSymbol type, CopyFromCompilationData compilation, SemanticModel semanticModel)
		{
			AttributeData[] attributes = type.GetAttributes(compilation.CopyFromTypeAttribute!).ToArray();

			if (attributes.Length == 0 || !EnsureIsInPartialContext(type))
			{
				return false;
			}

			return ValidateTargetTypes(type, semanticModel, attributes, out _);
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
			if (compilation.HasErrors)
			{
				return false;
			}

			AttributeData[]? attributes = type.GetAttributes(compilation.CopyFromTypeAttribute).ToArray();

			if (attributes is null || attributes.Length == 0)
			{
				return false;
			}

			bool isValid = EnsureIsInPartialContext(type, diagnosticReceiver);
			isValid &= ValidateTargetTypes(type, semanticModel, attributes, out _, diagnosticReceiver);

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
			if (compilation.HasErrors)
			{
				return false;
			}

			AttributeData[]? attributes = method.GetAttributes(compilation.CopyFromMethodAttribute).ToArray();

			if (attributes is null || attributes.Length == 0)
			{
				return false;
			}

			bool isValid = ValidateMarkedMethod(method, ref declaration, diagnosticReceiver);

			if (declaration is not null)
			{
				isValid &= ValidateTargetMethods(method, semanticModel, compilation, declaration, attributes, out _, diagnosticReceiver);
			}

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
				if (context.SemanticModel.GetDeclaredSymbol(member) is INamedTypeSymbol typeSymbol && ShouldAnalyze(typeSymbol))
				{
					DiagnosticReceiver.Contextual<SyntaxNodeAnalysisContext> diagnosticReceiver = DiagnosticReceiver.Factory.SyntaxNode(context);
					Analyze(typeSymbol, compilation, context.SemanticModel, diagnosticReceiver);
				}
			}
			else if (SymbolEqualityComparer.Default.Equals(attributeSymbol.ContainingType, compilation.CopyFromMethodAttribute))
			{
				IMethodSymbol? methodSymbol = member is LambdaExpressionSyntax
					? context.SemanticModel.GetSymbolInfo(member).Symbol as IMethodSymbol
					: context.SemanticModel.GetDeclaredSymbol(member) as IMethodSymbol;

				if (methodSymbol is not null && ShouldAnalyze(methodSymbol))
				{
					DiagnosticReceiver.Contextual<SyntaxNodeAnalysisContext> diagnosticReceiver = DiagnosticReceiver.Factory.SyntaxNode(context);
					Analyze(methodSymbol, compilation, context.SemanticModel, diagnosticReceiver, member as MethodDeclarationSyntax);
				}
			}
			else if(SymbolEqualityComparer.Default.Equals(attributeSymbol.ContainingType, compilation.PatternAttribute))
            {
				if(context.SemanticModel.GetDeclaredSymbol(member) is ISymbol symbol)
                {
					DiagnosticReceiver.Contextual<SyntaxNodeAnalysisContext> diagnosticReceiver = DiagnosticReceiver.Factory.SyntaxNode(context);
					AnalyzePattern(symbol, attr, compilation, diagnosticReceiver);
                }
            }
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

			foreach (INamedTypeSymbol baseType in method.GetContainingTypeSymbols())
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

			foreach (INamedTypeSymbol baseType in type.GetContainingTypeSymbols())
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

		private static IMethodSymbol? EnsureIsValidTarget(IMethodSymbol currentMethod, ISymbol symbol, bool? accessor, out DiagnosticDescriptor? diagnostic)
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

					if (accessor.HasValue)
					{
						if (accessor.Value)
						{
							if (property.SetMethod is null)
							{
								diagnostic = DUR0203_MemberCannotBeResolved;
								return null;
							}
						}
						else
						{
							if (property.GetMethod is null)
							{
								diagnostic = DUR0203_MemberCannotBeResolved;
								return null;
							}
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
					else
					{
						if (property.GetMethod is null)
						{
							diagnostic = DUR0212_TargetDoesNotHaveReturnType;
							return null;
						}
						else
						{
							diagnostic = null;
							return property.GetMethod;
						}
					}

				case IEventSymbol @event:

					if (accessor.HasValue)
					{
						if (!currentMethod.ReturnsVoid)
						{
							diagnostic = DUR0212_TargetDoesNotHaveReturnType;
							return null;
						}

						if (accessor.Value)
						{
							if (@event.AddMethod is not null)
							{
								diagnostic = null;
								return @event.AddMethod;
							}
						}
						else
						{
							if (@event.RemoveMethod is not null)
							{
								diagnostic = null;
								return @event.RemoveMethod;
							}
						}
					}

					break;
			}

			diagnostic = DUR0204_WrongTargetMemberKind;
			return null;
		}

		private static bool AnalyzePattern(ISymbol symbol, AttributeSyntax attrSyntax, CopyFromCompilationData compilation, IDiagnosticReceiver diagnosticReceiver)
        {
			AttributeData? currentData = null;
			bool hasCopyFrom = false;

            foreach (AttributeData attribute in symbol.GetAttributes())
            {
				if (currentData is null &&
					SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, compilation.PatternAttribute) &&
					attribute.ApplicationSyntaxReference?.Span == attrSyntax.Span
				)
                {
					currentData = attribute;
                }

				if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, compilation.CopyFromMethodAttribute) ||
					SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, compilation.CopyFromTypeAttribute))
				{
					hasCopyFrom = true;

					if (currentData is not null)
                    {
						break;
                    }
				}
			}

			Location? location = null;
			bool isValid = true;

			if(!hasCopyFrom)
            {
				location = attrSyntax.GetLocation();
				diagnosticReceiver.ReportDiagnostic(DUR0215_RedundantPatternAttribute, location, symbol);
				isValid = false;
			}

			if(currentData is not null && !HasValidRegexPattern(currentData))
            {
				diagnosticReceiver.ReportDiagnostic(DUR0214_InvalidPatternAttributeSpecified, location ?? attrSyntax.GetLocation(), symbol);
				isValid = false;
			}

			return isValid;
        }

		private static SymbolInfo GetSymbolFromCrefSyntax(
			string text,
			CSharpCompilation compilation,
			MemberDeclarationSyntax declaration,
			out CSharpSyntaxNode cref
		)
		{
			text = AnalysisUtilities.ConvertFullyQualifiedNameToXml(text);
			string parse = $"/// <inheritdoc cref=\"{text}\"/>";

			SyntaxNode root = CSharpSyntaxTree.ParseText(parse, encoding: Encoding.UTF8).GetRoot();
			DocumentationCommentTriviaSyntax trivia = root.DescendantNodes(descendIntoTrivia: true).OfType<DocumentationCommentTriviaSyntax>().First();

			trivia = trivia.WithAdditionalAnnotations(new SyntaxAnnotation(_annotation));

			root = declaration.SyntaxTree.GetRoot();
			root = root.ReplaceNode(declaration, declaration.WithLeadingTrivia(SyntaxFactory.Trivia(trivia)));
			compilation = compilation.ReplaceSyntaxTree(declaration.SyntaxTree, root.SyntaxTree);

#pragma warning disable RS1030 // Do not invoke Compilation.GetSemanticModel() method within a diagnostic analyzer
			SemanticModel createdSemanticModel = compilation.GetSemanticModel(root.SyntaxTree, true);
#pragma warning restore RS1030 // Do not invoke Compilation.GetSemanticModel() method within a diagnostic analyzer

			trivia = (DocumentationCommentTriviaSyntax)root.GetAnnotatedNodes(_annotation).First();

			cref = trivia.DescendantNodes().OfType<CrefSyntax>().First();
			return createdSemanticModel.GetSymbolInfo(cref);
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
			bool? accessor = HasTargetAccessor(ref text);
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

		private static bool? HasTargetAccessor(ref string text)
		{
			// null <-- no accessor
			// false <-- get or remove
			// true <-- set or add

			if (text.Length < 5)
			{
				return null;
			}

			if (text[text.Length - 4] == '_')
			{
				if (text.EndsWith("get"))
				{
					text = text.Substring(0, text.Length - 4);
					return false;
				}

				if (text.EndsWith("set") || text.EndsWith("add"))
				{
					text = text.Substring(0, text.Length - 4);
					return true;
				}
			}
			else if (text.EndsWith("_remove"))
			{
				text = text.Substring(0, text.Length - 7);
				return false;
			}

			return null;
		}

		private static bool HasValidRegexPattern(AttributeData attribute)
		{
			ImmutableArray<TypedConstant> arguments = attribute.ConstructorArguments;

			if (arguments.Length < 2)
			{
				return false;
			}

			if (string.IsNullOrEmpty(arguments[0].Value as string))
			{
				return false;
			}

			if (arguments[1].Value is not string)
			{
				return false;
			}

			return true;
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

		private static bool ValidateTargetMethods(
			IMethodSymbol method,
			SemanticModel semanticModel,
			CopyFromCompilationData compilation,
			MemberDeclarationSyntax declaration,
			AttributeData[] attributes,
			[NotNullWhen(true)] out List<IMethodSymbol>? targetMethods
		)
		{
			List<IMethodSymbol> copyFromMethods = new();

			foreach (AttributeData attribute in attributes)
			{
				if (GetTargetMethod(method, attribute, semanticModel, compilation.Compilation, declaration, out _, out _) is IMethodSymbol target)
				{
					if (CopiesFromItself(method, target) || copyFromMethods.Contains(target, SymbolEqualityComparer.Default))
					{
						targetMethods = default;
						return false;
					}
					else
					{
						copyFromMethods.Add(target);
					}
				}
				else
				{
					targetMethods = default;
					return false;
				}
			}

			targetMethods = copyFromMethods;
			return true;
		}

		private static bool ValidateTargetMethods(
			IMethodSymbol method,
			SemanticModel semanticModel,
			CopyFromCompilationData compilation,
			MemberDeclarationSyntax declaration,
			AttributeData[] attributes,
			[NotNullWhen(true)] out List<IMethodSymbol>? targetMethods,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			List<IMethodSymbol> copyFromMethods = new();

			bool isValid = true;

			foreach (AttributeData attribute in attributes)
			{
				if (GetTargetMethod(method, attribute, semanticModel, compilation.Compilation, declaration, out DiagnosticDescriptor? diagnostic, out object? value) is IMethodSymbol target)
				{
					if (CopiesFromItself(method, target))
					{
						diagnosticReceiver.ReportDiagnostic(DUR0207_MemberCannotCopyFromItselfOrItsParent, attribute.GetLocation(), method);
						isValid = false;
					}
					else if (copyFromMethods.Contains(target, SymbolEqualityComparer.Default))
					{
						diagnosticReceiver.ReportDiagnostic(DUR0206_EquivalentAttributes, attribute.GetLocation(), method);
						isValid = false;
					}
					else if (compilation.Compilation.Assembly.GetTypeByMetadataName(target.ContainingType.ToString()) is null)
					{
						diagnosticReceiver.ReportDiagnostic(DUR0205_ImplementationNotAccessible, attribute.GetLocation(), method, target);
					}
					else
					{
						copyFromMethods.Add(target);
					}
				}
				else if (diagnostic is not null)
				{
					diagnosticReceiver.ReportDiagnostic(diagnostic, attribute.GetLocation(), method, value);
					isValid = false;
				}
			}

			targetMethods = isValid ? copyFromMethods : default;
			return isValid;
		}

		private static bool ValidateTargetTypes(
			INamedTypeSymbol type,
			SemanticModel semanticModel,
			AttributeData[] attributes,
			[NotNullWhen(true)] out List<INamedTypeSymbol>? targetTypes
		)
		{
			List<INamedTypeSymbol> copyFromTypes = new();

			foreach (AttributeData attribute in attributes)
			{
				if (GetTargetType(attribute, semanticModel, out _, out _) is INamedTypeSymbol target)
				{
					if (CopiesFromItself(type, target) || copyFromTypes.Contains(target, SymbolEqualityComparer.Default))
					{
						targetTypes = default;
						return false;
					}
					else
					{
						copyFromTypes.Add(target);
					}
				}
				else
				{
					targetTypes = default;
					return false;
				}
			}

			targetTypes = copyFromTypes;
			return true;
		}

		private static bool ValidateTargetTypes(
			INamedTypeSymbol type,
			SemanticModel semanticModel,
			AttributeData[] attributes,
			[NotNullWhen(true)] out List<INamedTypeSymbol>? targetTypes,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			List<INamedTypeSymbol> copyFromTypes = new();

			bool isValid = true;

			foreach (AttributeData attribute in attributes)
			{
				if (GetTargetType(attribute, semanticModel, out DiagnosticDescriptor? diagnostic, out object? value) is INamedTypeSymbol target)
				{
					if (CopiesFromItself(type, target))
					{
						diagnosticReceiver.ReportDiagnostic(DUR0207_MemberCannotCopyFromItselfOrItsParent, attribute.GetLocation(), type);
						isValid = false;
					}
					else if (copyFromTypes.Contains(target, SymbolEqualityComparer.Default))
					{
						diagnosticReceiver.ReportDiagnostic(DUR0206_EquivalentAttributes, attribute.GetLocation(), type);
						isValid = false;
					}
					else if (!SymbolEqualityComparer.Default.Equals(type.ContainingAssembly, target.ContainingAssembly))
					{
						diagnosticReceiver.ReportDiagnostic(DUR0205_ImplementationNotAccessible, attribute.GetLocation(), type, target);
					}
					else
					{
						copyFromTypes.Add(target);
					}
				}
				else if (diagnostic is not null)
				{
					diagnosticReceiver.ReportDiagnostic(diagnostic, attribute.GetLocation(), type, value);
					isValid = false;
				}
			}

			targetTypes = isValid ? copyFromTypes : default;
			return isValid;
		}
	}
}