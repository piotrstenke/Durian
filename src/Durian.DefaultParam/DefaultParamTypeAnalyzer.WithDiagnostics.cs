using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Durian.Configuration;
using Durian.Generator.Data;
using Durian.Generator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

				ImmutableArray<AttributeData> attributes = symbol.GetAttributes();
				INamedTypeSymbol[] containingTypes = symbol.GetContainingTypeSymbols().ToArray();

				bool isValid = AnalyzeAgainstProhibitedAttributes(diagnosticReceiver, symbol, compilation);
				isValid &= AnalyzeContainingTypes(diagnosticReceiver, symbol, compilation, cancellationToken);
				isValid &= AnalyzeAgainstPartial(diagnosticReceiver, symbol, cancellationToken);
				isValid &= AnalyzeTypeParameters(diagnosticReceiver, symbol, in typeParameters);

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
				return AnalyzeCollidingMembers(
					diagnosticReceiver,
					symbol,
					in typeParameters,
					compilation,
					symbol.GetAttributes(),
					symbol.GetContainingTypeSymbols().ToArray(),
					out applyNew,
					cancellationToken
				);
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
				bool allowsNew = AllowsNewModifier(attributes, containingTypes, compilation);

				return AnalyzeCollidingMembers(diagnosticReceiver, symbol, in typeParameters, compilation, allowsNew, out applyNew, cancellationToken);
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
				if (symbol.TypeKind == TypeKind.Struct ||
					symbol.IsStatic ||
					symbol.IsSealed ||
					(symbol.TypeKind == TypeKind.Class && !symbol.InstanceConstructors.Any(ctor => ctor.DeclaredAccessibility >= Accessibility.Protected))
				)
				{
					if (HasInheritConventionOnContainingTypes(containingTypes, compilation))
					{
						if (!DefaultParamUtilities.TryGetConfigurationPropertyValue(attributes, compilation.ConfigurationAttribute!, nameof(DefaultParamConfiguration.TypeConvention), out int value) || value != (int)DPTypeConvention.Copy)
						{
							diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0118_ApplyCopyTypeConventionOnStructOrSealedTypeOrTypeWithNoPublicCtor, symbol);
						}
					}

					return false;
				}

				return HasInheritConvention(attributes, containingTypes, compilation);
			}

			/// <inheritdoc cref="DefaultParamAnalyzer.WithDiagnostics.AnalyzeAgainstProhibitedAttributes(IDiagnosticReceiver, ISymbol, DefaultParamCompilationData)"/>
			public static bool AnalyzeAgainstProhibitedAttributes(IDiagnosticReceiver diagnosticReceiver, INamedTypeSymbol symbol, DefaultParamCompilationData compialation)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeAgainstProhibitedAttributes(diagnosticReceiver, symbol, compialation);
			}

			/// <inheritdoc cref="DefaultParamAnalyzer.WithDiagnostics.AnalyzeAgainstProhibitedAttributes(IDiagnosticReceiver, ISymbol, DefaultParamCompilationData, out AttributeData[])"/>
			public static bool AnalyzeAgainstProhibitedAttributes(IDiagnosticReceiver diagnosticReceiver, INamedTypeSymbol symbol, DefaultParamCompilationData compilation, [NotNullWhen(true)] out AttributeData[]? attributes)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeAgainstProhibitedAttributes(diagnosticReceiver, symbol, compilation, out attributes);
			}

			/// <inheritdoc cref="DefaultParamAnalyzer.WithDiagnostics.AnalyzeContainingTypes(IDiagnosticReceiver, ISymbol, DefaultParamCompilationData, CancellationToken)"/>
			public static bool AnalyzeContainingTypes(IDiagnosticReceiver diagnosticReceiver, INamedTypeSymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeContainingTypes(diagnosticReceiver, symbol, compilation, cancellationToken);
			}

			/// <inheritdoc cref="DefaultParamAnalyzer.WithDiagnostics.AnalyzeContainingTypes(IDiagnosticReceiver, ISymbol, DefaultParamCompilationData, out ITypeData[])"/>
			public static bool AnalyzeContainingTypes(IDiagnosticReceiver diagnosticReceiver, INamedTypeSymbol symbol, DefaultParamCompilationData compilation, [NotNullWhen(true)] out ITypeData[]? containingTypes)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeContainingTypes(diagnosticReceiver, symbol, compilation, out containingTypes);
			}

			/// <inheritdoc cref="DefaultParamAnalyzer.WithDiagnostics.AnalyzeTypeParameters(IDiagnosticReceiver, ISymbol, in TypeParameterContainer)"/>
			public static bool AnalyzeTypeParameters(IDiagnosticReceiver diagnosticReceiver, INamedTypeSymbol symbol, in TypeParameterContainer typeParameters)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeTypeParameters(diagnosticReceiver, symbol, in typeParameters);
			}

			/// <summary>
			/// Analyzes if the specified <paramref name="symbol"/> is partial.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="symbol"><see cref="INamedTypeSymbol"/> to analyze.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			/// <returns><see langword="true"/> if the <paramref name="symbol"/> is not partial, <see langword="false"/> otherwise.</returns>
			public static bool AnalyzeAgainstPartial(IDiagnosticReceiver diagnosticReceiver, INamedTypeSymbol symbol, CancellationToken cancellationToken = default)
			{
				TypeDeclarationSyntax[] syntaxes = symbol.DeclaringSyntaxReferences.Select(r => r.GetSyntax(cancellationToken)).OfType<TypeDeclarationSyntax>().ToArray();

				if (syntaxes.Length > 1 || syntaxes[0].Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
				{
					diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0122_DoNotUseDefaultParamOnPartialType, symbol);
					return false;
				}

				return true;
			}
		}
	}
}
