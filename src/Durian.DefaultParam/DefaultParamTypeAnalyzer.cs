// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

#if !MAIN_PACKAGE

using Microsoft.CodeAnalysis.Diagnostics;

#endif

namespace Durian.Analysis.DefaultParam
{
	/// <summary>
	/// Analyzes types with type parameters marked by the <c>Durian.DefaultParamAttribute</c>.
	/// </summary>
#pragma warning disable RS1001 // Missing diagnostic analyzer attribute.
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

		/// <inheritdoc cref="DefaultParamDelegateAnalyzer.Analyze(INamedTypeSymbol, DefaultParamCompilationData, CancellationToken)"/>
		public static bool Analyze(
			INamedTypeSymbol symbol,
			DefaultParamCompilationData compilation,
			CancellationToken cancellationToken = default
		)
		{
			return DefaultParamDelegateAnalyzer.Analyze(symbol, compilation, cancellationToken);
		}

		/// <inheritdoc cref="WithDiagnostics.AnalyzeAgainstPartial(IDiagnosticReceiver, INamedTypeSymbol, CancellationToken)"/>
		public static bool AnalyzeAgainstPartial(INamedTypeSymbol symbol, CancellationToken cancellationToken = default)
		{
			TypeDeclarationSyntax[] syntaxes = symbol.DeclaringSyntaxReferences.Select(r => r.GetSyntax(cancellationToken)).OfType<TypeDeclarationSyntax>().ToArray();

			return syntaxes.Length <= 1 && !syntaxes[0].Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
		}

		/// <inheritdoc cref="WithDiagnostics.AnalyzeCollidingMembers(IDiagnosticReceiver, INamedTypeSymbol, in TypeParameterContainer, DefaultParamCompilationData, string, out HashSet{int}?, IEnumerable{AttributeData}, INamedTypeSymbol[], CancellationToken)"/>
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

		/// <inheritdoc cref="WithDiagnostics.AnalyzeCollidingMembers(IDiagnosticReceiver, INamedTypeSymbol, in TypeParameterContainer, DefaultParamCompilationData, string, out HashSet{int}?, bool, CancellationToken)"/>
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
			return DefaultParamDelegateAnalyzer.AnalyzeCollidingMembers(symbol, in typeParameters, compilation, targetNamespace, out applyNew, allowsNewModifier, cancellationToken);
		}

		/// <summary>
		/// Returns a collection of all supported diagnostics of <see cref="DefaultParamDelegateAnalyzer"/>.
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
				MemberNames.Config_TypeConvention,
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
				MemberNames.Config_TypeConvention,
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
		public override void Analyze(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			WithDiagnostics.Analyze(diagnosticReceiver, (INamedTypeSymbol)symbol, compilation, cancellationToken);
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