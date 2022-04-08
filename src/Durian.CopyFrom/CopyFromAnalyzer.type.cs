﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
		/// <inheritdoc cref="Analyze(in CopyFromTypeContext, IDiagnosticReceiver)"/>
		public static bool Analyze(in CopyFromTypeContext context)
		{
			if (context.Compilation.HasErrors || !ShouldAnalyze(context.Symbol))
			{
				return false;
			}

			AttributeData[] attributes = context.Symbol.GetAttributes(context.Compilation.CopyFromTypeAttribute!).ToArray();

			if (attributes.Length == 0 || !EnsureIsInPartialContext(context.Symbol))
			{
				return false;
			}

			return ValidateTargetTypes(in context, attributes, out _, true);
		}

		/// <summary>
		/// Performs analysis of a <see cref="INamedTypeSymbol"/> contained within the specified <paramref name="context"/>.
		/// </summary>
		/// <param name="context"><see cref="CopyFromTypeContext"/> that contains all data needed to perform analysis.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		public static bool Analyze(in CopyFromTypeContext context, IDiagnosticReceiver diagnosticReceiver)
		{
			if (context.Compilation.HasErrors || !ShouldAnalyze(context.Symbol))
			{
				return false;
			}

			bool isValid = AnalyzeTypeWithoutPattern(
				in context,
				out ImmutableArray<AttributeData> attributes,
				out List<TargetData>? targetTypes,
				diagnosticReceiver
			);

			AnalyzePattern(context.Symbol, context.Compilation, attributes, targetTypes?.Count > 0, diagnosticReceiver);
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

		private static void AnalyzeTypeAttribute(SyntaxNodeAnalysisContext context, CopyFromCompilationData compilation, AttributeSyntax attr, CSharpSyntaxNode member)
		{
			if (context.SemanticModel.GetDeclaredSymbol(member) is INamedTypeSymbol typeSymbol && ShouldAnalyze(typeSymbol))
			{
				DiagnosticReceiver.Contextual<SyntaxNodeAnalysisContext> diagnosticReceiver = DiagnosticReceiver.Factory.SyntaxNode(context);
				AnalyzeTypeWithoutPattern(new CopyFromTypeContext(compilation, context.SemanticModel, typeSymbol, cancellationToken: context.CancellationToken), attr, diagnosticReceiver);
			}
		}

		internal static bool AnalyzeTypeWithoutPattern(
			in CopyFromTypeContext context,
			out ImmutableArray<AttributeData> attributes,
			[NotNullWhen(true)] out List<TargetData>? targetTypes,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			AttributeData[]? copyFroms = GetAttributes(context.Symbol, context.Compilation, out attributes);

			if (copyFroms is null)
			{
				targetTypes = default;
				return false;
			}

			bool isValid = EnsureIsInPartialContext(context.Symbol, diagnosticReceiver);
			isValid &= ValidateTargetTypes(context, copyFroms, out targetTypes, diagnosticReceiver);

			return isValid;
		}

		internal static bool AnalyzeTypeWithoutPattern(
			in CopyFromTypeContext context,
			out ImmutableArray<AttributeData> attributes,
			[NotNullWhen(true)] out List<TargetData>? targetTypes
		)
		{
			AttributeData[]? copyFroms = GetAttributes(context.Symbol, context.Compilation, out attributes);

			if (copyFroms is null)
			{
				targetTypes = default;
				return false;
			}

			if (EnsureIsInPartialContext(context.Symbol))
			{
				return ValidateTargetTypes(in context, copyFroms, out targetTypes, true);
			}

			targetTypes = default;
			return false;
		}

		private static bool AnalyzeTypeWithoutPattern(
			in CopyFromTypeContext context,
			AttributeSyntax syntax,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			AttributeData[]? copyFroms = GetAttributes(context.Symbol, context.Compilation, out _);

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

			ValidateTargetTypes(in context, targets, out List<TargetData>? targetTypes, false);

			bool isValid = EnsureIsInPartialContext(context.Symbol, diagnosticReceiver);
			isValid &= ValidateTargetType(in context, copyFroms[index], targetTypes!);

			return isValid;
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

		private static bool IsInDifferentAssembly(INamedTypeSymbol type, INamedTypeSymbol target)
		{
			return !SymbolEqualityComparer.Default.Equals(type.ContainingAssembly, target.ContainingAssembly);
		}

		private static bool IsValidTarget(INamedTypeSymbol symbol)
		{
			return symbol.TypeKind is TypeKind.Class or TypeKind.Struct or TypeKind.Interface;
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

		private static bool ValidatePartialNameAndAddTarget(
			in CopyFromTypeContext context,
			AttributeData attribute,
			INamedTypeSymbol target,
			List<TargetData> copyFromTypes,
			bool isValid,
			ref Location? location,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			if (TryGetPartialPartName(attribute, out string? partialPartName))
			{
				if (!TryGetPartialPart(target, context.Compilation, partialPartName, out TypeDeclarationSyntax? partialPart))
				{
					location ??= attribute.GetLocation();
					diagnosticReceiver.ReportDiagnostic(DUR0218_UnknownPartialPartName, location, context.Symbol);
					return false;
				}

				if (copyFromTypes.Any(t => SymbolEqualityComparer.Default.Equals(t.Symbol, target) && (t.PartialPartName is null || t.PartialPartName == partialPartName)))
				{
					location ??= attribute.GetLocation();
					diagnosticReceiver.ReportDiagnostic(DUR0206_EquivalentTarget, location, context.Symbol);
				}
				else if (isValid)
				{
					copyFromTypes.Add(CreateTargetData(attribute, target, partialPart, partialPartName));
				}
			}
			else if (copyFromTypes.Any(t => SymbolEqualityComparer.Default.Equals(t.Symbol, target)))
			{
				location ??= attribute.GetLocation();
				diagnosticReceiver.ReportDiagnostic(DUR0206_EquivalentTarget, location, context.Symbol);
			}
			else if (isValid)
			{
				copyFromTypes.Add(CreateTargetData(attribute, target));
			}

			return isValid;
		}

		private static bool ValidateTargetType(
			in CopyFromTypeContext context,
			AttributeData attribute,
			List<TargetData> copyFromTypes
		)
		{
			if (GetTargetType(attribute, context.SemanticModel, out _, out _) is not INamedTypeSymbol target ||
				CopiesFromItself(context.Symbol, target) ||
				IsInDifferentAssembly(context.Symbol, target) ||
				!HasValidTypeArguments(target, out _))
			{
				return false;
			}

			if (TryGetPartialPartName(attribute, out string? partialPartName))
			{
				if (!TryGetPartialPart(target, context.Compilation, partialPartName, out TypeDeclarationSyntax? partialPart))
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
			in CopyFromTypeContext context,
			AttributeData attribute,
			List<TargetData> copyFromTypes,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			if (GetTargetType(attribute, context.SemanticModel, out DiagnosticDescriptor? diagnostic, out object? value) is not INamedTypeSymbol target)
			{
				if (diagnostic is not null)
				{
					diagnosticReceiver.ReportDiagnostic(diagnostic, attribute.GetLocation(), context.Symbol, value);
				}

				return false;
			}

			bool isValid = true;
			Location? location = default;

			if (CopiesFromItself(context.Symbol, target))
			{
				location ??= attribute.GetLocation();
				diagnosticReceiver.ReportDiagnostic(DUR0207_MemberCannotCopyFromItselfOrItsParent, location, context.Symbol);
				isValid = false;
			}

			if (IsInDifferentAssembly(context.Symbol, target))
			{
				location ??= attribute.GetLocation();
				diagnosticReceiver.ReportDiagnostic(DUR0205_ImplementationNotAccessible, location, context.Symbol, target);
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
						diagnosticReceiver.ReportDiagnostic(DUR0217_TypeParameterIsNotValid, location, context.Symbol, typeArgument);
					}
				}
			}

			return ValidatePartialNameAndAddTarget(in context, attribute, target, copyFromTypes, isValid, ref location, diagnosticReceiver);
		}

		private static bool ValidateTargetTypes(
			in CopyFromTypeContext context,
			AttributeData[] attributes,
			[NotNullWhen(true)] out List<TargetData>? targetTypes,
			bool returnIfInvalid
		)
		{
			List<TargetData> copyFromTypes = new();
			bool isValid = true;

			foreach (AttributeData attribute in attributes)
			{
				if (!ValidateTargetType(in context, attribute, copyFromTypes))
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
			in CopyFromTypeContext context,
			AttributeData[] attributes,
			[NotNullWhen(true)] out List<TargetData>? targetTypes,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			List<TargetData> copyFromTypes = new();

			bool isValid = true;

			foreach (AttributeData attribute in attributes)
			{
				if (!ValidateTargetType(in context, attribute, copyFromTypes, diagnosticReceiver))
				{
					isValid = false;
				}
			}

			targetTypes = isValid ? copyFromTypes : default;
			return isValid;
		}
	}
}
