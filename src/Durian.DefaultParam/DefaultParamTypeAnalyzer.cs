// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Durian.Analysis.Extensions;
using Durian.Configuration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

#if !MAIN_PACKAGE

using Microsoft.CodeAnalysis.Diagnostics;

#endif

namespace Durian.Analysis.DefaultParam
{
#pragma warning disable RS1001 // Missing diagnostic analyzer attribute.

	/// <summary>
	/// Analyzes types with type parameters marked by the <see cref="DefaultParamAttribute"/>.
	/// </summary>
#if !MAIN_PACKAGE

	[DiagnosticAnalyzer(LanguageNames.CSharp)]
#endif

	public sealed partial class DefaultParamTypeAnalyzer : DefaultParamAnalyzer
#pragma warning restore RS1001 // Missing diagnostic analyzer attribute.
	{
		/// <inheritdoc/>
		public override SymbolKind SupportedSymbolKind => SymbolKind.NamedType;

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamTypeAnalyzer"/> class.
		/// </summary>
		public DefaultParamTypeAnalyzer()
		{
		}

		/// <inheritdoc cref="DefaultParamDelegateAnalyzer.AllowsNewModifier(INamedTypeSymbol, DefaultParamCompilationData)"/>
		public static bool AllowsNewModifier(INamedTypeSymbol symbol, DefaultParamCompilationData compilation)
		{
			return AllowsNewModifier(symbol.GetAttributes(), symbol.GetContainingTypeSymbols().ToArray(), compilation);
		}

		/// <inheritdoc cref="DefaultParamDelegateAnalyzer.AllowsNewModifier(IEnumerable{AttributeData}, INamedTypeSymbol[], DefaultParamCompilationData)"/>
		public static bool AllowsNewModifier(IEnumerable<AttributeData> attributes, INamedTypeSymbol[] containingTypes, DefaultParamCompilationData compilation)
		{
			return DefaultParamUtilities.AllowsNewModifier(attributes, containingTypes, compilation);
		}

		/// <inheritdoc cref="DefaultParamDelegateAnalyzer.Analyze(INamedTypeSymbol, DefaultParamCompilationData, CancellationToken)"/>
		public static bool Analyze(INamedTypeSymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			return DefaultParamDelegateAnalyzer.Analyze(symbol, compilation, cancellationToken);
		}

		/// <inheritdoc cref="WithDiagnostics.AnalyzeAgainstPartial(IDiagnosticReceiver, INamedTypeSymbol, CancellationToken)"/>
		public static bool AnalyzeAgainstPartial(INamedTypeSymbol symbol, CancellationToken cancellationToken = default)
		{
			TypeDeclarationSyntax[] syntaxes = symbol.DeclaringSyntaxReferences.Select(r => r.GetSyntax(cancellationToken)).OfType<TypeDeclarationSyntax>().ToArray();

			return syntaxes.Length <= 1 && !syntaxes[0].Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
		}

		/// <inheritdoc cref="WithDiagnostics.AnalyzeCollidingMembers(IDiagnosticReceiver, INamedTypeSymbol, in TypeParameterContainer, DefaultParamCompilationData, string, IEnumerable{AttributeData}, INamedTypeSymbol[], out HashSet{int}?, CancellationToken)"/>
		public static bool AnalyzeCollidingMembers(
			INamedTypeSymbol symbol,
			in TypeParameterContainer typeParameters,
			DefaultParamCompilationData compilation,
			string targetNamespace,
			out HashSet<int>? applyNew,
			CancellationToken cancellationToken = default
		)
		{
			return AnalyzeCollidingMembers(
				symbol,
				in typeParameters,
				compilation,
				targetNamespace,
				symbol.GetAttributes(),
				symbol.GetContainingTypeSymbols().ToArray(),
				out applyNew,
				cancellationToken
			);
		}

		/// <inheritdoc cref="WithDiagnostics.AnalyzeCollidingMembers(IDiagnosticReceiver, INamedTypeSymbol, in TypeParameterContainer, DefaultParamCompilationData, string, IEnumerable{AttributeData}, INamedTypeSymbol[], out HashSet{int}?, CancellationToken)"/>
		public static bool AnalyzeCollidingMembers(
			INamedTypeSymbol symbol,
			in TypeParameterContainer typeParameters,
			DefaultParamCompilationData compilation,
			string targetNamespace,
			IEnumerable<AttributeData> attributes,
			INamedTypeSymbol[] containingTypes,
			out HashSet<int>? applyNew,
			CancellationToken cancellationToken = default
		)
		{
			bool allowsNew = AllowsNewModifier(attributes, containingTypes, compilation);

			return AnalyzeCollidingMembers(symbol, in typeParameters, compilation, targetNamespace, allowsNew, out applyNew, cancellationToken);
		}

		/// <inheritdoc cref="WithDiagnostics.AnalyzeCollidingMembers(IDiagnosticReceiver, INamedTypeSymbol, in TypeParameterContainer, DefaultParamCompilationData, string, bool, out HashSet{int}?, CancellationToken)"/>
		public static bool AnalyzeCollidingMembers(
			INamedTypeSymbol symbol,
			in TypeParameterContainer typeParameters,
			DefaultParamCompilationData compilation,
			string targetNamespace,
			bool allowsNewModifier,
			out HashSet<int>? applyNew,
			CancellationToken cancellationToken = default
		)
		{
			return DefaultParamDelegateAnalyzer.AnalyzeCollidingMembers(symbol, in typeParameters, compilation, targetNamespace, allowsNewModifier, out applyNew, cancellationToken);
		}

		/// <summary>
		/// Returns a collection of all supported diagnostics of <see cref="DefaultParamDelegateAnalyzer"/>.
		/// </summary>
		public static IEnumerable<DiagnosticDescriptor> GetSupportedDiagnostics()
		{
			return GetBaseDiagnostics().Concat(GetAnalyzerSpecificDiagnosticsAsArray());
		}

		/// <summary>
		/// Checks if the target <paramref name="symbol"/> has the <see cref="DPTypeConvention.Inherit"/> applied, either directly or by one of the containing types.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		public static bool HasInheritConvention(INamedTypeSymbol symbol, DefaultParamCompilationData compilation)
		{
			return HasInheritConvention(symbol.GetAttributes(), symbol.GetContainingTypeSymbols().ToArray(), compilation);
		}

		/// <summary>
		/// Checks if the target <see cref="INamedTypeSymbol"/> has the <see cref="DPTypeConvention.Inherit"/> applied, either directly or by one of the <paramref name="containingTypes"/>.
		/// </summary>
		/// <param name="attributes">Attributes of the target <see cref="INamedTypeSymbol"/>.</param>
		/// <param name="containingTypes">Types that contain target <see cref="INamedTypeSymbol"/>.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		public static bool HasInheritConvention(IEnumerable<AttributeData> attributes, INamedTypeSymbol[] containingTypes, DefaultParamCompilationData compilation)
		{
			return DefaultParamUtilities.GetConfigurationEnumValue(nameof(DefaultParamConfigurationAttribute.TypeConvention), attributes, containingTypes, compilation, (int)compilation.Configuration.TypeConvention) == (int)DPTypeConvention.Inherit;
		}

		/// <summary>
		/// Checks if one of containing types of the target <paramref name="symbol"/> has the <see cref="DPTypeConvention.Inherit"/> applied.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		public static bool HasInheritConventionOnContainingTypes(INamedTypeSymbol symbol, DefaultParamCompilationData compilation)
		{
			return HasInheritConventionOnContainingTypes(symbol.GetContainingTypeSymbols().ToArray(), compilation);
		}

		/// <summary>
		/// Checks if one of <paramref name="containingTypes"/> of the target <see cref="INamedTypeSymbol"/> has the <see cref="DPTypeConvention.Inherit"/> applied.
		/// </summary>
		/// <param name="containingTypes">Types that contain target <see cref="INamedTypeSymbol"/>.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		public static bool HasInheritConventionOnContainingTypes(INamedTypeSymbol[] containingTypes, DefaultParamCompilationData compilation)
		{
			return DefaultParamUtilities.GetConfigurationEnumValueOnContainingTypes(nameof(DefaultParamConfigurationAttribute.TypeConvention), containingTypes, compilation, (int)compilation.Configuration.TypeConvention) == (int)DPTypeConvention.Inherit;
		}

		/// <summary>
		/// Determines, whether the <see cref="DefaultParamGenerator"/> should inherit the target <paramref name="symbol"/> instead of copying its contents.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		public static bool ShouldInheritInsteadOfCopying(INamedTypeSymbol symbol, DefaultParamCompilationData compilation)
		{
			return ShouldInheritInsteadOfCopying(symbol, compilation, symbol.GetAttributes(), symbol.GetContainingTypeSymbols().ToArray());
		}

		/// <summary>
		/// Determines, whether the <see cref="DefaultParamGenerator"/> should inherit the target <paramref name="symbol"/> instead of copying its contents.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="attributes">A collection of <see cref="INamedTypeSymbol"/>' attributes.</param>
		/// <param name="containingTypes">An array of <see cref="INamedTypeSymbol"/>s of the target <see cref="INamedTypeSymbol"/>.</param>
		public static bool ShouldInheritInsteadOfCopying(INamedTypeSymbol symbol, DefaultParamCompilationData compilation, IEnumerable<AttributeData> attributes, INamedTypeSymbol[] containingTypes)
		{
			if (symbol.TypeKind == TypeKind.Struct ||
				symbol.IsStatic ||
				symbol.IsSealed ||
				(symbol.TypeKind == TypeKind.Class && !symbol.InstanceConstructors.Any(ctor => ctor.DeclaredAccessibility >= Accessibility.Protected))
			)
			{
				return false;
			}

			return HasInheritConvention(attributes, containingTypes, compilation);
		}

		/// <inheritdoc/>
		public override void Analyze(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			if (symbol is not INamedTypeSymbol t)
			{
				return;
			}

			TypeKind kind = t.TypeKind;

			if (kind is not TypeKind.Class and not TypeKind.Struct and not TypeKind.Interface)
			{
				return;
			}

			WithDiagnostics.Analyze(diagnosticReceiver, t, compilation, cancellationToken);
		}

		/// <inheritdoc/>
		protected override IEnumerable<DiagnosticDescriptor> GetAnalyzerSpecificDiagnostics()
		{
			return GetAnalyzerSpecificDiagnosticsAsArray();
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
