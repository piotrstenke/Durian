// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Analysis.DefaultParam.Types
{
	/// <summary>
	/// Analyzes types with type parameters marked by the <c>Durian.DefaultParamAttribute</c>.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class DefaultParamTypeAnalyzer : DefaultParamAnalyzer
	{
		/// <inheritdoc/>
		public override SymbolKind SupportedSymbolKind => SymbolKind.NamedType;

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamTypeAnalyzer"/> class.
		/// </summary>
		public DefaultParamTypeAnalyzer()
		{
		}

		/// <inheritdoc cref="Delegates.DefaultParamDelegateAnalyzer.Analyze(INamedTypeSymbol, DefaultParamCompilationData, IDiagnosticReceiver, CancellationToken)"/>
		public static bool Analyze(
			INamedTypeSymbol symbol,
			DefaultParamCompilationData compilation,
			IDiagnosticReceiver diagnosticReceiver,
			CancellationToken cancellationToken = default
		)
		{
			TypeParameterContainer typeParameters = TypeParameterContainer.CreateFrom(symbol, compilation, cancellationToken);

			if (!typeParameters.HasDefaultParams)
			{
				return false;
			}

			ImmutableArray<AttributeData> attributes = symbol.GetAttributes();
			INamedTypeSymbol[] containingTypes = symbol.GetContainingTypes().ToArray();

			bool isValid = AnalyzeAgainstProhibitedAttributes(symbol, compilation, diagnosticReceiver, attributes);
			isValid &= AnalyzeContainingTypes(symbol, compilation, diagnosticReceiver, containingTypes, cancellationToken);
			isValid &= AnalyzeAgainstPartial(symbol, diagnosticReceiver, cancellationToken);
			isValid &= AnalyzeTypeParameters(symbol, in typeParameters, diagnosticReceiver);

			if (isValid)
			{
				string targetNamespace = GetTargetNamespace(symbol, compilation, attributes, containingTypes);

				isValid &= AnalyzeCollidingMembers(symbol, in typeParameters, compilation, targetNamespace, out _, diagnosticReceiver, attributes, containingTypes, cancellationToken);
			}

			ShouldInheritInsteadOfCopying(symbol, compilation, diagnosticReceiver, attributes, containingTypes);

			return isValid;
		}

		/// <inheritdoc cref="Delegates.DefaultParamDelegateAnalyzer.Analyze(INamedTypeSymbol, DefaultParamCompilationData, CancellationToken)"/>
		public static bool Analyze(
			INamedTypeSymbol symbol,
			DefaultParamCompilationData compilation,
			CancellationToken cancellationToken = default
		)
		{
			return Delegates.DefaultParamDelegateAnalyzer.Analyze(symbol, compilation, cancellationToken);
		}

		/// <summary>
		/// Analyzes if the specified <paramref name="symbol"/> is partial.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to analyze.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is not partial, <see langword="false"/> otherwise.</returns>
		public static bool AnalyzeAgainstPartial(
			INamedTypeSymbol symbol,
			IDiagnosticReceiver diagnosticReceiver,
			CancellationToken cancellationToken = default
		)
		{
			TypeDeclarationSyntax[] syntaxes = symbol.DeclaringSyntaxReferences
				.Select(r => r.GetSyntax(cancellationToken))
				.OfType<TypeDeclarationSyntax>()
				.ToArray();

			if (syntaxes.Length > 1 || syntaxes[0].Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
			{
				diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0122_DoNotUseDefaultParamOnPartialType, symbol);
				return false;
			}

			return true;
		}

		/// <inheritdoc cref="AnalyzeAgainstPartial(INamedTypeSymbol, IDiagnosticReceiver, CancellationToken)"/>
		public static bool AnalyzeAgainstPartial(INamedTypeSymbol symbol, CancellationToken cancellationToken = default)
		{
			TypeDeclarationSyntax[] syntaxes = symbol.DeclaringSyntaxReferences.Select(r => r.GetSyntax(cancellationToken)).OfType<TypeDeclarationSyntax>().ToArray();

			return syntaxes.Length <= 1 && !syntaxes[0].Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
		}

		/// <inheritdoc cref="Delegates.DefaultParamDelegateAnalyzer.AnalyzeCollidingMembers(INamedTypeSymbol, in TypeParameterContainer, DefaultParamCompilationData, string, out HashSet{int}?, IDiagnosticReceiver, IEnumerable{AttributeData}?, INamedTypeSymbol[], CancellationToken)"/>
		public static bool AnalyzeCollidingMembers(
			INamedTypeSymbol symbol,
			in TypeParameterContainer typeParameters,
			DefaultParamCompilationData compilation,
			string targetNamespace,
			out HashSet<int>? applyNew,
			IDiagnosticReceiver diagnosticReceiver,
			IEnumerable<AttributeData>? attributes = null,
			INamedTypeSymbol[]? containingTypes = null,
			CancellationToken cancellationToken = default
		)
		{
			InitializeAttributes(ref attributes, symbol);
			InitializeContainingTypes(ref containingTypes, symbol);

			bool allowsNew = AllowsNewModifier(symbol, compilation, attributes, containingTypes);

			return AnalyzeCollidingMembers(
				symbol,
				in typeParameters,
				compilation,
				targetNamespace,
				out applyNew,
				allowsNew,
				diagnosticReceiver,
				cancellationToken
			);
		}

		/// <inheritdoc cref="Delegates.DefaultParamDelegateAnalyzer.AnalyzeCollidingMembers(INamedTypeSymbol, in TypeParameterContainer, DefaultParamCompilationData, string, out HashSet{int}?, bool, IDiagnosticReceiver, CancellationToken)"/>
		public static bool AnalyzeCollidingMembers(
			INamedTypeSymbol symbol,
			in TypeParameterContainer typeParameters,
			DefaultParamCompilationData compilation,
			string targetNamespace,
			out HashSet<int>? applyNew,
			bool allowsNewModifier,
			IDiagnosticReceiver diagnosticReceiver,
			CancellationToken cancellationToken = default
		)
		{
			return Delegates.DefaultParamDelegateAnalyzer.AnalyzeCollidingMembers(
				symbol,
				in typeParameters,
				compilation,
				targetNamespace,
				out applyNew,
				allowsNewModifier,
				diagnosticReceiver,
				cancellationToken
			);
		}

		/// <inheritdoc cref="AnalyzeCollidingMembers(INamedTypeSymbol, in TypeParameterContainer, DefaultParamCompilationData, string, out HashSet{int}?, IDiagnosticReceiver, IEnumerable{AttributeData}, INamedTypeSymbol[], CancellationToken)"/>
		public static bool AnalyzeCollidingMembers(
			INamedTypeSymbol symbol,
			in TypeParameterContainer typeParameters,
			DefaultParamCompilationData compilation,
			string targetNamespace,
			out HashSet<int>? applyNew,
			IEnumerable<AttributeData>? attributes = null,
			INamedTypeSymbol[]? containingTypes = null,
			CancellationToken cancellationToken = default
		)
		{
			bool allowsNew = AllowsNewModifier(symbol, compilation, attributes, containingTypes);

			return AnalyzeCollidingMembers(symbol, in typeParameters, compilation, targetNamespace, out applyNew, allowsNew, cancellationToken);
		}

		/// <inheritdoc cref="AnalyzeCollidingMembers(INamedTypeSymbol, in TypeParameterContainer, DefaultParamCompilationData, string, out HashSet{int}?, bool, IDiagnosticReceiver, CancellationToken)"/>
		public static bool AnalyzeCollidingMembers(
			INamedTypeSymbol symbol,
			in TypeParameterContainer typeParameters,
			DefaultParamCompilationData compilation,
			string targetNamespace,
			out HashSet<int>? applyNew,
			bool allowsNewModifier,
			CancellationToken cancellationToken = default
		)
		{
			return Delegates.DefaultParamDelegateAnalyzer.AnalyzeCollidingMembers(symbol, in typeParameters, compilation, targetNamespace, out applyNew, allowsNewModifier, cancellationToken);
		}

		/// <summary>
		/// Returns a collection of all supported diagnostics of <see cref="DefaultParamTypeAnalyzer"/>.
		/// </summary>
		public static IEnumerable<DiagnosticDescriptor> GetSupportedDiagnostics()
		{
			return GetBaseDiagnostics().Concat(GetAnalyzerSpecificDiagnosticsAsArray());
		}

		/// <summary>
		/// Checks if the target <paramref name="symbol"/> has the <see cref="TypeConvention.Inherit"/> applied, either directly or by one of the containing types.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="attributes">Attributes of the target <see cref="INamedTypeSymbol"/>.</param>
		/// <param name="containingTypes">Types that contain target <see cref="INamedTypeSymbol"/>.</param>
		public static bool HasInheritConvention(
			INamedTypeSymbol symbol,
			DefaultParamCompilationData compilation,
			IEnumerable<AttributeData>? attributes = null,
			INamedTypeSymbol[]? containingTypes = null
		)
		{
			InitializeAttributes(ref attributes, symbol);
			InitializeContainingTypes(ref containingTypes, symbol);

			int value = DefaultParamUtilities.GetConfigurationEnumValue(
				DefaultParamConfigurationAttributeProvider.TypeConvention,
				attributes,
				containingTypes,
				compilation,
				(int)compilation.GlobalConfiguration.TypeConvention
			);

			return value == (int)TypeConvention.Inherit;
		}

		/// <summary>
		/// Checks if one of containing types of the target <paramref name="symbol"/> has the <see cref="TypeConvention.Inherit"/> applied.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="containingTypes">Types that contain target <see cref="INamedTypeSymbol"/>.</param>
		public static bool HasInheritConventionOnContainingTypes(
			INamedTypeSymbol symbol,
			DefaultParamCompilationData compilation,
			INamedTypeSymbol[]? containingTypes = null
		)
		{
			InitializeContainingTypes(ref containingTypes, symbol);

			int value = DefaultParamUtilities.GetConfigurationEnumValueOnContainingTypes(
				DefaultParamConfigurationAttributeProvider.TypeConvention,
				containingTypes,
				compilation,
				(int)compilation.GlobalConfiguration.TypeConvention
			);

			return value == (int)TypeConvention.Inherit;
		}

		/// <summary>
		/// Determines, whether the <see cref="DefaultParamGenerator"/> should inherit the target <paramref name="symbol"/> instead of copying its contents.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <param name="attributes">A collection of <see cref="INamedTypeSymbol"/>' attributes.</param>
		/// <param name="containingTypes">An array of <see cref="INamedTypeSymbol"/>s of the target <see cref="INamedTypeSymbol"/>.</param>
		/// <returns><see langword="true"/> if the configuration of the <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
		public static bool ShouldInheritInsteadOfCopying(
			INamedTypeSymbol symbol,
			DefaultParamCompilationData compilation,
			IDiagnosticReceiver diagnosticReceiver,
			IEnumerable<AttributeData>? attributes = null,
			INamedTypeSymbol[]? containingTypes = null
		)
		{
			InitializeAttributes(ref attributes, symbol);
			InitializeContainingTypes(ref containingTypes, symbol);

			if (symbol.TypeKind == TypeKind.Struct ||
				symbol.IsStatic ||
				symbol.IsSealed ||
				(symbol.TypeKind == TypeKind.Class && !symbol.InstanceConstructors.Any(ctor => ctor.DeclaredAccessibility >= Accessibility.Protected))
			)
			{
				if (HasInheritConventionOnContainingTypes(symbol, compilation, containingTypes))
				{
					if (!DefaultParamUtilities.TryGetConfigurationPropertyValue(attributes, compilation.DefaultParamConfigurationAttribute!, nameof(DefaultParamConfiguration.TypeConvention), out int value) || value != (int)TypeConvention.Copy)
					{
						diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0118_ApplyCopyTypeConventionOnStructOrSealedTypeOrTypeWithNoPublicCtor, symbol);
					}
				}

				return false;
			}

			return HasInheritConvention(symbol, compilation, attributes, containingTypes);
		}

		/// <summary>
		/// Determines, whether the <see cref="DefaultParamGenerator"/> should inherit the target <paramref name="symbol"/> instead of copying its contents.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="attributes">A collection of <see cref="INamedTypeSymbol"/>' attributes.</param>
		/// <param name="containingTypes">An array of <see cref="INamedTypeSymbol"/>s of the target <see cref="INamedTypeSymbol"/>.</param>
		public static bool ShouldInheritInsteadOfCopying(
			INamedTypeSymbol symbol,
			DefaultParamCompilationData compilation,
			IEnumerable<AttributeData>? attributes = null,
			INamedTypeSymbol[]? containingTypes = null
		)
		{
			InitializeAttributes(ref attributes, symbol);
			InitializeContainingTypes(ref containingTypes, symbol);

			if (symbol.TypeKind == TypeKind.Struct ||
				symbol.IsStatic ||
				symbol.IsSealed ||
				(symbol.TypeKind == TypeKind.Class && !symbol.InstanceConstructors.Any(ctor => ctor.DeclaredAccessibility >= Accessibility.Protected))
			)
			{
				return false;
			}

			return HasInheritConvention(symbol, compilation, attributes, containingTypes);
		}

		/// <inheritdoc/>
		protected override void Analyze(
			ISymbol symbol,
			DefaultParamCompilationData compilation,
			IDiagnosticReceiver diagnosticReceiver,
			CancellationToken cancellationToken = default
		)
		{
			Analyze((INamedTypeSymbol)symbol, compilation, diagnosticReceiver, cancellationToken);
		}

		/// <inheritdoc/>
		protected override IEnumerable<DiagnosticDescriptor> GetAnalyzerSpecificDiagnostics()
		{
			return GetAnalyzerSpecificDiagnosticsAsArray();
		}

		/// <inheritdoc/>
		protected override bool ShouldAnalyze(ISymbol symbol, DefaultParamCompilationData compilation)
		{
			if (symbol is not INamedTypeSymbol t)
			{
				return false;
			}

			return t.TypeKind is TypeKind.Class or TypeKind.Struct or TypeKind.Interface;
		}

		private static DiagnosticDescriptor[] GetAnalyzerSpecificDiagnosticsAsArray()
		{
			return new DiagnosticDescriptor[]
			{
				DefaultParamDiagnostics.DUR0118_ApplyCopyTypeConventionOnStructOrSealedTypeOrTypeWithNoPublicCtor,
				DefaultParamDiagnostics.DUR0122_DoNotUseDefaultParamOnPartialType,
				DefaultParamDiagnostics.DUR0129_TargetNamespaceAlreadyContainsMemberWithName
			};
		}
	}
}
