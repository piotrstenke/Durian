using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Durian.Generator.Data;
using Durian.Generator.Extensions;
using Microsoft.CodeAnalysis;

namespace Durian.Generator.DefaultParam
{
	public partial class DefaultParamTypeAnalyzer
	{
		/// <summary>
		/// Contains static methods that analyze types with type parameters marked using the <see cref="DefaultParamAttribute"/> and report <see cref="Diagnostic"/>s for the invalid ones.
		/// </summary>
		public static new class WithDiagnostics
		{
			/// <inheritdoc cref="DefaultParamDelegateAnalyzer.WithDiagnostics.Analyze(IDiagnosticReceiver, INamedTypeSymbol, DefaultParamCompilationData, CancellationToken)"/>
			public static bool Analyze(IDiagnosticReceiver diagnosticReceiver, INamedTypeSymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
			{
				TypeParameterContainer typeParameters = TypeParameterContainer.CreateFrom(symbol, compilation, cancellationToken);

				if (!typeParameters.HasDefaultParams)
				{
					return false;
				}

				bool isValid = AnalyzeAgaintsProhibitedAttributes(diagnosticReceiver, symbol, compilation);
				isValid &= AnalyzeContainingTypes(diagnosticReceiver, symbol, cancellationToken);
				isValid &= AnalyzeTypeParameters(diagnosticReceiver, in typeParameters);

				ImmutableArray<AttributeData> attributes = symbol.GetAttributes();
				INamedTypeSymbol[] containingTypes = symbol.GetContainingTypeSymbols().ToArray();

				if (isValid)
				{
					isValid &= AnalyzeCollidingMembers(diagnosticReceiver, symbol, in typeParameters, compilation, attributes, containingTypes, out _, cancellationToken);
				}

				ShouldInheritInsteadOfCopying(diagnosticReceiver, symbol, compilation, attributes, containingTypes);

				return isValid;
			}

			/// <inheritdoc cref="DefaultParamDelegateAnalyzer.WithDiagnostics.AnalyzeCollidingMembers(IDiagnosticReceiver, INamedTypeSymbol, in TypeParameterContainer, DefaultParamCompilationData, out HashSet{int}?, CancellationToken)"/>
			public static bool AnalyzeCollidingMembers(
				IDiagnosticReceiver diagnosticReceiver,
				INamedTypeSymbol symbol,
				in TypeParameterContainer typeParameters,
				DefaultParamCompilationData compilation,
				out HashSet<int>? applyNew,
				CancellationToken cancellationToken = default
			)
			{
				return DefaultParamDelegateAnalyzer.WithDiagnostics.AnalyzeCollidingMembers(diagnosticReceiver, symbol, in typeParameters, compilation, out applyNew, cancellationToken);
			}

			/// <inheritdoc cref="DefaultParamDelegateAnalyzer.WithDiagnostics.AnalyzeCollidingMembers(IDiagnosticReceiver, INamedTypeSymbol, in TypeParameterContainer, DefaultParamCompilationData, IEnumerable{AttributeData}, INamedTypeSymbol[], out HashSet{int}?, CancellationToken)"/>
			public static bool AnalyzeCollidingMembers(
				IDiagnosticReceiver diagnosticReceiver,
				INamedTypeSymbol symbol,
				in TypeParameterContainer typeParameters,
				DefaultParamCompilationData compilation,
				IEnumerable<AttributeData> attributes,
				INamedTypeSymbol[] containingTypes,
				out HashSet<int>? applyNew,
				CancellationToken cancellationToken = default
			)
			{
				return DefaultParamDelegateAnalyzer.WithDiagnostics.AnalyzeCollidingMembers(diagnosticReceiver, symbol, in typeParameters, compilation, attributes, containingTypes, out applyNew, cancellationToken);
			}

			/// <inheritdoc cref="DefaultParamDelegateAnalyzer.WithDiagnostics.AnalyzeCollidingMembers(IDiagnosticReceiver, INamedTypeSymbol, in TypeParameterContainer, DefaultParamCompilationData, bool, out HashSet{int}?, CancellationToken)"/>
			public static bool AnalyzeCollidingMembers(
				IDiagnosticReceiver diagnosticReceiver,
				INamedTypeSymbol symbol,
				in TypeParameterContainer typeParameters,
				DefaultParamCompilationData compilation,
				bool allowsNewModifier,
				out HashSet<int>? applyNew,
				CancellationToken cancellationToken = default
			)
			{
				return DefaultParamDelegateAnalyzer.WithDiagnostics.AnalyzeCollidingMembers(diagnosticReceiver, symbol, in typeParameters, compilation, allowsNewModifier, out applyNew, cancellationToken);
			}

			/// <inheritdoc cref="ShouldInheritInsteadOfCopying(IDiagnosticReceiver, INamedTypeSymbol, DefaultParamCompilationData, IEnumerable{AttributeData}, INamedTypeSymbol[])"/>
			public static bool ShouldInheritInsteadOfCopying(IDiagnosticReceiver diagnosticReceiver, INamedTypeSymbol symbol, DefaultParamCompilationData compilation)
			{
				return ShouldInheritInsteadOfCopying(diagnosticReceiver, symbol, compilation, symbol.GetAttributes(), symbol.GetContainingTypeSymbols().ToArray());
			}

			/// <summary>
			/// Determines, whether the <see cref="DefaultParamGenerator"/> should inherit the target <paramref name="symbol"/> instead of copying its contents.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="attributes">A collection of <see cref="INamedTypeSymbol"/>' attributes.</param>
			/// <param name="containingTypes">An array of <see cref="INamedTypeSymbol"/>s of the target <see cref="INamedTypeSymbol"/>.</param>
			/// <returns><see langword="true"/> if the configuration of the <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
			public static bool ShouldInheritInsteadOfCopying(IDiagnosticReceiver diagnosticReceiver, INamedTypeSymbol symbol, DefaultParamCompilationData compilation, IEnumerable<AttributeData> attributes, INamedTypeSymbol[] containingTypes)
			{
				if ((symbol.TypeKind == TypeKind.Struct || symbol.IsSealed) && HasInheritConvention(attributes, containingTypes, compilation))
				{
					diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0122_ApplyCopyTypeConventionOnStructOrSealedType, symbol);
					return false;
				}

				return true;
			}

			/// <inheritdoc cref="DefaultParamAnalyzer.WithDiagnostics.AnalyzeAgaintsProhibitedAttributes(IDiagnosticReceiver, ISymbol, DefaultParamCompilationData)"/>
			public static bool AnalyzeAgaintsProhibitedAttributes(IDiagnosticReceiver diagnosticReceiver, INamedTypeSymbol symbol, DefaultParamCompilationData compialation)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeAgaintsProhibitedAttributes(diagnosticReceiver, symbol, compialation);
			}

			/// <inheritdoc cref="DefaultParamAnalyzer.WithDiagnostics.AnalyzeAgaintsProhibitedAttributes(IDiagnosticReceiver, ISymbol, DefaultParamCompilationData, out AttributeData[])"/>
			public static bool AnalyzeAgaintsProhibitedAttributes(IDiagnosticReceiver diagnosticReceiver, INamedTypeSymbol symbol, DefaultParamCompilationData compilation, [NotNullWhen(true)] out AttributeData[]? attributes)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeAgaintsProhibitedAttributes(diagnosticReceiver, symbol, compilation, out attributes);
			}

			/// <inheritdoc cref="DefaultParamAnalyzer.WithDiagnostics.AnalyzeContainingTypes(IDiagnosticReceiver, ISymbol, CancellationToken)"/>
			public static bool AnalyzeContainingTypes(IDiagnosticReceiver diagnosticReceiver, INamedTypeSymbol symbol, CancellationToken cancellationToken = default)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeContainingTypes(diagnosticReceiver, symbol, cancellationToken);
			}

			/// <inheritdoc cref="DefaultParamAnalyzer.WithDiagnostics.AnalyzeContainingTypes(IDiagnosticReceiver, ISymbol, DefaultParamCompilationData, out ITypeData[])"/>
			public static bool AnalyzeContainingTypes(IDiagnosticReceiver diagnosticReceiver, INamedTypeSymbol symbol, DefaultParamCompilationData compilation, [NotNullWhen(true)] out ITypeData[]? containingTypes)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeContainingTypes(diagnosticReceiver, symbol, compilation, out containingTypes);
			}

			/// <inheritdoc cref="DefaultParamAnalyzer.WithDiagnostics.AnalyzeTypeParameters(IDiagnosticReceiver, in TypeParameterContainer)"/>
			public static bool AnalyzeTypeParameters(IDiagnosticReceiver diagnosticReceiver, in TypeParameterContainer typeParameters)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeTypeParameters(diagnosticReceiver, in typeParameters);
			}
		}
	}
}
