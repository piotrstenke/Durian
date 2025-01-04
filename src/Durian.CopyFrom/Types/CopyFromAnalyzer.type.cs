using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Durian.Analysis.CopyFrom.Types;
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

			List<INamedTypeSymbol> dependencies = new();

			return ValidateTargetTypes(in context, attributes, dependencies, out _, true);
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
				out _,
				out List<TargetTypeData>? targetTypes,
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

		internal static bool AnalyzeTypeWithoutPattern(
			in CopyFromTypeContext context,
			out ImmutableArray<AttributeData> attributes,
			[NotNullWhen(true)] out List<INamedTypeSymbol>? dependencies,
			[NotNullWhen(true)] out List<TargetTypeData>? targetTypes,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			AttributeData[]? copyFroms = GetCopyFromAttributes(context.Symbol, context.Compilation, out attributes);

			if (copyFroms is null || !HasCopyFromsOnCurrentDeclaration(context.Node!, copyFroms))
			{
				targetTypes = default;
				dependencies = default;
				return false;
			}

			List<INamedTypeSymbol> deps = new();

			bool isValid = EnsureIsInPartialContext(context.Symbol, diagnosticReceiver);
			isValid &= ValidateTargetTypes(context, copyFroms, deps, out targetTypes, diagnosticReceiver);

			dependencies = isValid ? deps : default;

			return isValid;
		}

		internal static bool AnalyzeTypeWithoutPattern(
			in CopyFromTypeContext context,
			out ImmutableArray<AttributeData> attributes,
			[NotNullWhen(true)] out List<INamedTypeSymbol>? dependencies,
			[NotNullWhen(true)] out List<TargetTypeData>? targetTypes
		)
		{
			AttributeData[]? copyFroms = GetCopyFromAttributes(context.Symbol, context.Compilation, out attributes);

			if (copyFroms is null || !HasCopyFromsOnCurrentDeclaration(context.Node!, copyFroms))
			{
				targetTypes = default;
				dependencies = default;
				return false;
			}

			if (EnsureIsInPartialContext(context.Symbol))
			{
				List<INamedTypeSymbol> deps = new();
				bool isValid = ValidateTargetTypes(in context, copyFroms, deps, out targetTypes, true);
				dependencies = isValid ? deps : default;
				return isValid;
			}

			targetTypes = default;
			dependencies = default;
			return false;
		}

		private static void AnalyzeTypeAttribute(SyntaxNodeAnalysisContext context, CopyFromCompilationData compilation, AttributeSyntax attr, CSharpSyntaxNode member)
		{
			if (context.SemanticModel.GetDeclaredSymbol(member) is INamedTypeSymbol typeSymbol && ShouldAnalyze(typeSymbol))
			{
				DiagnosticReceiver.Contextual<SyntaxNodeAnalysisContext> diagnosticReceiver = DiagnosticReceiver.Factory.SyntaxNode(context);
				AnalyzeTypeWithoutPattern(new CopyFromTypeContext(compilation, context.SemanticModel, typeSymbol, cancellationToken: context.CancellationToken), attr, diagnosticReceiver);
			}
		}

		private static bool AnalyzeTypeWithoutPattern(
			in CopyFromTypeContext context,
			AttributeSyntax syntax,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			AttributeData[]? copyFroms = GetCopyFromAttributes(context.Symbol, context.Compilation, out _);

			if (copyFroms is null || !HasCopyFromsOnCurrentDeclaration(context.Node!, copyFroms))
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

			List<INamedTypeSymbol> dependencies = new();

			ValidateTargetTypes(in context, targets, dependencies, out List<TargetTypeData>? targetTypes, false);

			bool isValid = EnsureIsInPartialContext(context.Symbol, diagnosticReceiver);
			isValid &= ValidateTargetType(in context, copyFroms[index], targetTypes!, dependencies);

			return isValid;
		}

		private static bool CopiesFromItself(INamedTypeSymbol type, INamedTypeSymbol target)
		{
			if (SymbolEqualityComparer.Default.Equals(type, target))
			{
				return true;
			}

			if (type.Inherits(target) || target.Inherits(type))
			{
				return true;
			}

			if (target.ContainsSymbol(type))
			{
				return true;
			}

			return false;
		}

		private static TargetTypeData CreateTargetTypeData(
			AttributeData attribute,
			INamedTypeSymbol target,
			AdditionalNodes additionalNodes,
			string[]? usings
		)
		{
			return CreateTargetTypeData(attribute, target, additionalNodes, usings, default, default);
		}

		private static TargetTypeData CreateTargetTypeData(
			AttributeData attribute,
			INamedTypeSymbol target,
			AdditionalNodes additionalNodes,
			string[]? usings,
			TypeDeclarationSyntax? partialPart,
			string? partialPartName
		)
		{
			int order = GetOrder(attribute);
			bool handleSpecialMembers = ShouldHandleSpecialMembers(attribute);

			return new(target, order, additionalNodes, usings, partialPart, partialPartName, handleSpecialMembers);
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

		private static AttributeData[]? GetCopyFromAttributes(INamedTypeSymbol type, CopyFromCompilationData compilation, out ImmutableArray<AttributeData> allAttributes)
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

		private static bool HasValidTypeArguments(INamedTypeSymbol type, out List<ITypeSymbol>? invalidArguments)
		{
			if (type.IsUnboundGenericType)
			{
				invalidArguments = default;
				return true;
			}

			return HasValidTypeArguments(type.TypeParameters, type.TypeArguments, out invalidArguments);
		}

		private static bool IsCircularDependency(in CopyFromTypeContext context, INamedTypeSymbol target, List<INamedTypeSymbol> dependencies)
		{
			foreach (AttributeData attribute in target.GetAttributes(context.Compilation.CopyFromTypeAttribute!))
			{
				if (GetTargetType(attribute, context.SemanticModel, out _, out _) is not INamedTypeSymbol type)
				{
					continue;
				}

				if (SymbolEqualityComparer.Default.Equals(context.Symbol, type) || IsCircularDependency(in context, type, dependencies))
				{
					return true;
				}

				dependencies.Add(target);
			}

			return false;
		}

		private static bool IsInDifferentAssembly(INamedTypeSymbol type, INamedTypeSymbol target)
		{
			return !SymbolEqualityComparer.Default.Equals(type.ContainingAssembly, target.ContainingAssembly);
		}

		private static bool IsValidTarget(INamedTypeSymbol symbol)
		{
			return symbol.TypeKind is TypeKind.Class or TypeKind.Struct or TypeKind.Interface;
		}

		private static AdditionalNodes RemoveInvalidFlags(AdditionalNodes additionalNodes, INamedTypeSymbol symbol)
		{
			if (additionalNodes.HasFlag(AdditionalNodes.Constraints) && (!symbol.IsGenericType || symbol.HasConstraints()))
			{
				RemoveFlag(ref additionalNodes, AdditionalNodes.Constraints);
			}

			if (additionalNodes.HasFlag(AdditionalNodes.Documentation) && symbol.HasDocumentation())
			{
				RemoveFlag(ref additionalNodes, AdditionalNodes.Documentation);
			}

			if (additionalNodes.HasFlag(AdditionalNodes.BaseType) && (symbol.HasExplicitBaseType() || !symbol.SupportsExplicitBaseType()))
			{
				RemoveFlag(ref additionalNodes, AdditionalNodes.BaseType);
			}

			return additionalNodes;
		}

		private static AdditionalNodes RetrieveNonStandardTypeConfig(AttributeData attribute, INamedTypeSymbol symbol)
		{
			AdditionalNodes additionalNodes = GetAdditionalNodesConfig(attribute);

			if (additionalNodes == AdditionalNodes.None)
			{
				return additionalNodes;
			}

			return RemoveInvalidFlags(additionalNodes, symbol);
		}

		private static AdditionalNodes RetrieveNonStandardTypeConfig(
			AttributeData attribute,
			INamedTypeSymbol symbol,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			AdditionalNodes additionalNodes = GetAdditionalNodesConfig(attribute);

			if (additionalNodes == AdditionalNodes.None)
			{
				return additionalNodes;
			}

			if (additionalNodes == AdditionalNodes.All)
			{
				return RemoveInvalidFlags(additionalNodes, symbol);
			}

			Location? location = default;

			if (additionalNodes.HasFlag(AdditionalNodes.Constraints))
			{
				if (!symbol.IsGenericType)
				{
					location ??= attribute.GetNamedArgumentLocation(CopyFromTypeAttributeProvider.AdditionalNodes);
					diagnosticReceiver.ReportDiagnostic(DUR0224_CannotCopyConstraintsForMethodOrNonGenericMember, location, symbol);
					RemoveFlag(ref additionalNodes, AdditionalNodes.Constraints);
				}
				else if (symbol.HasConstraints())
				{
					location ??= attribute.GetNamedArgumentLocation(CopyFromTypeAttributeProvider.AdditionalNodes);
					diagnosticReceiver.ReportDiagnostic(DUR0223_MemberAlreadyHasConstraints, location, symbol);
					RemoveFlag(ref additionalNodes, AdditionalNodes.Constraints);
				}
			}

			if (additionalNodes.HasFlag(AdditionalNodes.Documentation) && symbol.HasDocumentation())
			{
				location ??= attribute.GetNamedArgumentLocation(CopyFromTypeAttributeProvider.AdditionalNodes);
				diagnosticReceiver.ReportDiagnostic(DUR0222_MemberAlreadyHasDocumentation, location, symbol);
				RemoveFlag(ref additionalNodes, AdditionalNodes.Documentation);
			}

			if (additionalNodes.HasFlag(AdditionalNodes.BaseType))
			{
				if (symbol.HasExplicitBaseType())
				{
					location ??= attribute.GetNamedArgumentLocation(CopyFromTypeAttributeProvider.AdditionalNodes);
					diagnosticReceiver.ReportDiagnostic(DUR0225_BaseTypeAlreadySpecified, location, symbol);
					RemoveFlag(ref additionalNodes, AdditionalNodes.BaseType);
				}
				else if (!symbol.SupportsExplicitBaseType())
				{
					location ??= attribute.GetNamedArgumentLocation(CopyFromTypeAttributeProvider.AdditionalNodes);
					diagnosticReceiver.ReportDiagnostic(DUR0226_CannotApplyBaseType, location, symbol);
					RemoveFlag(ref additionalNodes, AdditionalNodes.BaseType);
				}
			}

			return additionalNodes;
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
			List<TargetTypeData> copyFromTypes,
			INamedTypeSymbol target,
			bool isValid,
			ref Location? location,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			AdditionalNodes additionalNodes = RetrieveNonStandardTypeConfig(attribute, context.Symbol, diagnosticReceiver);
			string[]? usings = RetrieveUsings(attribute, context.Node!, additionalNodes, context.Symbol, diagnosticReceiver);

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
					copyFromTypes.Add(CreateTargetTypeData(attribute, target, additionalNodes, usings, partialPart, partialPartName));
				}
			}
			else if (copyFromTypes.Any(t => SymbolEqualityComparer.Default.Equals(t.Symbol, target)))
			{
				location ??= attribute.GetLocation();
				diagnosticReceiver.ReportDiagnostic(DUR0206_EquivalentTarget, location, context.Symbol);
			}
			else if (isValid)
			{
				copyFromTypes.Add(CreateTargetTypeData(attribute, target, additionalNodes, usings));
			}

			return isValid;
		}

		private static bool ValidateTargetType(
			in CopyFromTypeContext context,
			AttributeData attribute,
			List<TargetTypeData> copyFromTypes,
			List<INamedTypeSymbol> dependencies
		)
		{
			if (GetTargetType(attribute, context.SemanticModel, out _, out _) is not INamedTypeSymbol target ||
				CopiesFromItself(context.Symbol, target) ||
				IsInDifferentAssembly(context.Symbol, target) ||
				IsCircularDependency(in context, target, dependencies) ||
				!HasValidTypeArguments(target, out _))
			{
				return false;
			}

			AdditionalNodes additionalNodes = RetrieveNonStandardTypeConfig(attribute, context.Symbol);
			string[]? usings = RetrieveUsings(attribute, context.Node!, additionalNodes);

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

				copyFromTypes.Add(CreateTargetTypeData(attribute, target, additionalNodes, usings, partialPart, partialPartName));
				return true;
			}

			if (!copyFromTypes.Any(t => SymbolEqualityComparer.Default.Equals(t.Symbol, target)))
			{
				copyFromTypes.Add(CreateTargetTypeData(attribute, target, additionalNodes, usings));
			}

			return true;
		}

		private static bool ValidateTargetType(
			in CopyFromTypeContext context,
			AttributeData attribute,
			List<TargetTypeData> copyFromTypes,
			List<INamedTypeSymbol> dependencies,
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
			else if (IsInDifferentAssembly(context.Symbol, target))
			{
				location ??= attribute.GetLocation();
				diagnosticReceiver.ReportDiagnostic(DUR0205_ImplementationNotAccessible, location, context.Symbol, target);
				isValid = false;
			}
			else if (IsCircularDependency(in context, target, dependencies))
			{
				location ??= attribute.GetLocation();
				diagnosticReceiver.ReportDiagnostic(DUR0221_CircularDependency, location, context.Symbol);
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

			return ValidatePartialNameAndAddTarget(
				in context,
				attribute,
				copyFromTypes,
				target,
				isValid,
				ref location,
				diagnosticReceiver
			);
		}

		private static bool ValidateTargetTypes(
			in CopyFromTypeContext context,
			AttributeData[] attributes,
			List<INamedTypeSymbol> dependencies,
			[NotNullWhen(true)] out List<TargetTypeData>? targetTypes,
			bool returnIfInvalid
		)
		{
			List<TargetTypeData> copyFromTypes = new();
			bool isValid = true;

			foreach (AttributeData attribute in attributes)
			{
				if (!ValidateTargetType(in context, attribute, copyFromTypes, dependencies))
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
			List<INamedTypeSymbol> dependencies,
			[NotNullWhen(true)] out List<TargetTypeData>? targetTypes,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			List<TargetTypeData> copyFromTypes = new();

			bool isValid = true;

			foreach (AttributeData attribute in attributes)
			{
				if (!ValidateTargetType(in context, attribute, copyFromTypes, dependencies, diagnosticReceiver))
				{
					isValid = false;
				}
			}

			targetTypes = isValid ? copyFromTypes : default;
			return isValid;
		}
	}
}
