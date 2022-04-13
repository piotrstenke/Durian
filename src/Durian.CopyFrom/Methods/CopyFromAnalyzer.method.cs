﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Durian.Analysis.CopyFrom.Methods;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.Analysis.CopyFrom.CopyFromDiagnostics;

namespace Durian.Analysis.CopyFrom
{
	public partial class CopyFromAnalyzer
	{
		private enum TargetAccessor
		{
			None,
			GetOrRemove,
			SetOrAdd
		}

		/// <inheritdoc cref="Analyze(ref CopyFromMethodContext, IDiagnosticReceiver)"/>
		public static bool Analyze(ref CopyFromMethodContext context)
		{
			if (context.Compilation.HasErrors || !ShouldAnalyze(context.Symbol))
			{
				return false;
			}

			AttributeData? attribute = context.Symbol.GetAttribute(context.Compilation.CopyFromMethodAttribute!);

			if (attribute is null || !ValidateMarkedMethod(ref context))
			{
				return false;
			}

			List<IMethodSymbol> dependencies = new();
			return ValidateTargetMethod(in context, attribute, dependencies, out _);
		}

		/// <summary>
		/// Performs analysis of a <see cref="IMethodSymbol"/> contained within the specified <paramref name="context"/>.
		/// </summary>
		/// <param name="context"><see cref="CopyFromMethodContext"/> that contains all data needed to perform analysis.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <exception cref="InvalidOperationException"><see cref="IMethodSymbol"/> is not associated with a <see cref="MethodDeclarationSyntax"/>.</exception>
		public static bool Analyze(ref CopyFromMethodContext context, IDiagnosticReceiver diagnosticReceiver)
		{
			if (context.Compilation.HasErrors || !ShouldAnalyze(context.Symbol))
			{
				return false;
			}

			bool isValid = AnalyzeMethodWithoutPattern(
				ref context,
				out ImmutableArray<AttributeData> attributes,
				out _,
				out IMethodSymbol? targetMethod,
				diagnosticReceiver
			);

			AnalyzePattern(context.Symbol, context.Compilation, attributes, targetMethod is not null, diagnosticReceiver);
			return isValid;
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

		internal static bool AnalyzeMethodWithoutPattern(
			ref CopyFromMethodContext context,
			out ImmutableArray<AttributeData> attributes,
			[NotNullWhen(true)] out List<IMethodSymbol>? dependencies,
			[NotNullWhen(true)] out IMethodSymbol? targetMethod,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			AttributeData? copyFrom = GetAttribute(context.Symbol, context.Compilation, out attributes);

			if (copyFrom is null)
			{
				targetMethod = default;
				dependencies = default;
				return false;
			}

			List<IMethodSymbol> deps = new();

			bool isValid = ValidateMarkedMethod(ref context, diagnosticReceiver);
			isValid &= ValidateTargetMethod(in context, copyFrom, deps, out targetMethod, diagnosticReceiver);

			dependencies = isValid ? deps : default;
			return isValid;
		}

		internal static bool AnalyzeMethodWithoutPattern(
			ref CopyFromMethodContext context,
			out ImmutableArray<AttributeData> attributes,
			[NotNullWhen(true)] out List<IMethodSymbol>? dependencies,
			[NotNullWhen(true)] out IMethodSymbol? targetMethod
		)
		{
			AttributeData? copyFrom = GetAttribute(context.Symbol, context.Compilation, out attributes);

			if (copyFrom is null)
			{
				targetMethod = default;
				dependencies = default;
				return false;
			}

			if (ValidateMarkedMethod(ref context))
			{
				List<IMethodSymbol> deps = new();
				bool isValid = ValidateTargetMethod(in context, copyFrom, deps, out targetMethod);
				dependencies = isValid ? deps : default;
				return isValid;
			}

			targetMethod = default;
			dependencies = default;
			return false;
		}

		private static void AnalyzeMethodAttribute(SyntaxNodeAnalysisContext context, CopyFromCompilationData compilation, CSharpSyntaxNode member)
		{
			IMethodSymbol? methodSymbol = member is LambdaExpressionSyntax
				? context.SemanticModel.GetSymbolInfo(member).Symbol as IMethodSymbol
				: context.SemanticModel.GetDeclaredSymbol(member) as IMethodSymbol;

			if (methodSymbol is not null && ShouldAnalyze(methodSymbol))
			{
				DiagnosticReceiver.Contextual<SyntaxNodeAnalysisContext> diagnosticReceiver = DiagnosticReceiver.Factory.SyntaxNode(context);
				CopyFromMethodContext c = new(compilation, context.SemanticModel, methodSymbol, member as MethodDeclarationSyntax);
				AnalyzeMethodWithoutPattern(ref c, out _, out _, out _, diagnosticReceiver);
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

		private static bool EnsureIsInPartialContext(ref CopyFromMethodContext context)
		{
			if (!context.Symbol.IsPartialDefinition || context.Symbol.PartialImplementationPart is not null)
			{
				return false;
			}

			if (context.Node is null && !context.TryInitNode(out context))
			{
				return false;
			}

			return context.Symbol.IsPartialContext(context.Node);
		}

		private static bool EnsureIsInPartialContext(ref CopyFromMethodContext context, IDiagnosticReceiver diagnosticReceiver)
		{
			if (context.Symbol.MethodKind != MethodKind.Ordinary)
			{
				return false;
			}

			bool success = true;

			if (context.Symbol.IsPartialDefinition)
			{
				if (context.Symbol.PartialImplementationPart is not null)
				{
					diagnosticReceiver.ReportDiagnostic(DUR0211_MethodAlreadyHasImplementation);
					success = false;
				}
			}
			else
			{
				if (context.Symbol.PartialDefinitionPart is null)
				{
					if (context.Node is null && !context.TryInitNode(out context))
					{
						return false;
					}

					if (!context.Symbol.IsPartial(context.Node))
					{
						diagnosticReceiver.ReportDiagnostic(DUR0202_MemberMustBePartial, context.Symbol);
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

			foreach (INamedTypeSymbol baseType in context.Symbol.GetContainingTypes())
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

		private static bool IsCircularDependency(in CopyFromMethodContext context, IMethodSymbol target, List<IMethodSymbol> dependencies)
		{
			AttributeData? attribute = target.GetAttribute(context.Compilation.CopyFromMethodAttribute!);

			if(attribute is null || GetTargetMethod(in context, attribute, out _, out _) is not IMethodSymbol method)
			{
				return false;
			}

			if (SymbolEqualityComparer.Default.Equals(context.Symbol, method) || IsCircularDependency(in context, method, dependencies))
			{
				return true;
			}

			dependencies.Add(method);
			return false;
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

		private static SymbolInfo GetSymbolFromCrefSyntax(
			in CopyFromMethodContext context,
			string text,
			out CSharpSyntaxNode cref
		)
		{
			const string annotation = "copyFrom - inheritdoc";

			text = AnalysisUtilities.ToXmlCompatible(text);
			string parse = $"/// <inheritdoc cref=\"{text}\"/>";

			SyntaxNode root = CSharpSyntaxTree.ParseText(parse, encoding: Encoding.UTF8).GetRoot();
			DocumentationCommentTriviaSyntax trivia = root.DescendantNodes(descendIntoTrivia: true).OfType<DocumentationCommentTriviaSyntax>().First();

			trivia = trivia.WithAdditionalAnnotations(new SyntaxAnnotation(annotation));

			root = context.Node!.SyntaxTree.GetRoot();
			root = root.ReplaceNode(context.Node, context.Node.WithLeadingTrivia(SyntaxFactory.Trivia(trivia)));
			CSharpCompilation compilation = context.Compilation.Compilation.ReplaceSyntaxTree(context.Node.SyntaxTree, root.SyntaxTree);

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
			in CopyFromMethodContext context,
			AttributeData attribute,
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
				info = context.SemanticModel.GetSpeculativeSymbolInfo(context.Node!.SpanStart, syntax, SpeculativeBindingOption.BindAsExpression);
			}
			else
			{
				info = GetSymbolFromCrefSyntax(in context, text, out _);
			}

			if (info.Symbol is not null)
			{
				return EnsureIsValidTarget(context.Symbol, info.Symbol, accessor, out diagnostic);
			}
			else if (info.CandidateReason is CandidateReason.OverloadResolutionFailure or CandidateReason.Inaccessible)
			{
				if (info.CandidateSymbols.Length == 1)
				{
					return EnsureIsValidTarget(context.Symbol, info.CandidateSymbols[0], accessor, out diagnostic);
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

		private static bool HasValidTypeArguments(IMethodSymbol method, out List<ITypeSymbol>? invalidArguments)
		{
			return HasValidTypeArguments(method.TypeParameters, method.TypeArguments, out invalidArguments);
		}

		private static bool IsInDifferentAssembly(IMethodSymbol target, CopyFromCompilationData compilation)
		{
			return compilation.Compilation.Assembly.GetTypeByMetadataName(target.ContainingType.ToString()) is null;
		}

		private static bool IsValidMethodType(IMethodSymbol method)
		{
			return !method.IsAbstract && !method.IsExtern && method.MethodKind == MethodKind.Ordinary;
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

		private static bool ValidateMarkedMethod(ref CopyFromMethodContext context, IDiagnosticReceiver diagnosticReceiver)
		{
			bool isValid = EnsureIsValidMethodType(context.Symbol, out DiagnosticDescriptor? diagnostic);

			if (!isValid)
			{
				diagnosticReceiver.ReportDiagnostic(diagnostic!, context.Symbol);

				if (context.Symbol.MethodKind != MethodKind.Ordinary)
				{
					return false;
				}
			}

			isValid &= EnsureIsInPartialContext(ref context, diagnosticReceiver);

			return isValid;
		}

		private static bool ValidateMarkedMethod(ref CopyFromMethodContext context)
		{
			if (!IsValidMethodType(context.Symbol))
			{
				return false;
			}

			if (!EnsureIsInPartialContext(ref context))
			{
				return false;
			}

			return true;
		}

		private static bool ValidateTargetMethod(
			in CopyFromMethodContext context,
			AttributeData attribute,
			List<IMethodSymbol> dependencies,
			[NotNullWhen(true)] out IMethodSymbol? targetMethod
		)
		{
			if (GetTargetMethod(in context, attribute, out _, out _) is IMethodSymbol target &&
				!CopiesFromItself(context.Symbol, target) &&
				!IsInDifferentAssembly(context.Symbol, context.Compilation) &&
				!IsCircularDependency(in context, target, dependencies) &&
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
			in CopyFromMethodContext context,
			AttributeData attribute,
			List<IMethodSymbol> dependencies,
			[NotNullWhen(true)] out IMethodSymbol? targetMethod,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			if (GetTargetMethod(in context, attribute, out DiagnosticDescriptor? diagnostic, out object? value) is IMethodSymbol target)
			{
				bool isValid = true;
				Location? location = default;

				if (CopiesFromItself(context.Symbol, target))
				{
					isValid = false;
					location ??= attribute.GetLocation();
					diagnosticReceiver.ReportDiagnostic(DUR0207_MemberCannotCopyFromItselfOrItsParent, location, context.Symbol);
				}
				else if (IsInDifferentAssembly(target, context.Compilation))
				{
					isValid = false;
					location ??= attribute.GetLocation();
					diagnosticReceiver.ReportDiagnostic(DUR0205_ImplementationNotAccessible, location, context.Symbol, target);
				}
				else if(IsCircularDependency(in context, target, dependencies))
				{
					isValid = false;
					location ??= attribute.GetLocation();
					diagnosticReceiver.ReportDiagnostic(DUR0221_CircularDependency, location, context.Symbol);
				}

				if (!HasValidTypeArguments(target, out List<ITypeSymbol>? invalidArguments))
				{
					isValid = false;

					if (invalidArguments is not null)
					{
						location ??= attribute.GetLocation();

						foreach (ITypeSymbol type in invalidArguments)
						{
							diagnosticReceiver.ReportDiagnostic(DUR0217_TypeParameterIsNotValid, location, context.Symbol, type);
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
				diagnosticReceiver.ReportDiagnostic(diagnostic, attribute.GetLocation(), context.Symbol, value);
			}

			targetMethod = default;
			return false;
		}
	}
}
