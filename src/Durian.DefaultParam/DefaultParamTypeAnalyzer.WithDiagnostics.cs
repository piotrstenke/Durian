// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.DefaultParam
{
#pragma warning disable RS1001 // Missing diagnostic analyzer attribute.

	public partial class DefaultParamTypeAnalyzer
#pragma warning restore RS1001 // Missing diagnostic analyzer attribute.
	{
		/// <summary>
		/// Contains static methods that analyze types with type parameters marked using the <c>Durian.DefaultParamAttribute</c> and report <see cref="Diagnostic"/>s for the invalid ones.
		/// </summary>
		public static new class WithDiagnostics
		{
			/// <inheritdoc cref="DefaultParamDelegateAnalyzer.WithDiagnostics.Analyze(IDiagnosticReceiver, INamedTypeSymbol, DefaultParamCompilationData, CancellationToken)"/>
			public static bool Analyze(
				IDiagnosticReceiver diagnosticReceiver,
				INamedTypeSymbol symbol,
				DefaultParamCompilationData compilation,
				CancellationToken cancellationToken = default
			)
			{
				TypeParameterContainer typeParameters = TypeParameterContainer.CreateFrom(symbol, compilation, cancellationToken);

				if (!typeParameters.HasDefaultParams)
				{
					return false;
				}

				ImmutableArray<AttributeData> attributes = symbol.GetAttributes();
				INamedTypeSymbol[] containingTypes = symbol.GetContainingTypeSymbols().ToArray();

				bool isValid = AnalyzeAgainstProhibitedAttributes(diagnosticReceiver, symbol, compilation, attributes);
				isValid &= AnalyzeContainingTypes(diagnosticReceiver, symbol, compilation, containingTypes, cancellationToken);
				isValid &= AnalyzeAgainstPartial(diagnosticReceiver, symbol, cancellationToken);
				isValid &= AnalyzeTypeParameters(diagnosticReceiver, symbol, in typeParameters);

				if (isValid)
				{
					string targetNamespace = GetTargetNamespace(symbol, compilation, attributes, containingTypes);

					isValid &= AnalyzeCollidingMembers(diagnosticReceiver, symbol, in typeParameters, compilation, targetNamespace, out _, attributes, containingTypes, cancellationToken);
				}

				ShouldInheritInsteadOfCopying(diagnosticReceiver, symbol, compilation, attributes, containingTypes);

				return isValid;
			}

			/// <summary>
			/// Analyzes if the specified <paramref name="symbol"/> is partial.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="symbol"><see cref="INamedTypeSymbol"/> to analyze.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			/// <returns><see langword="true"/> if the <paramref name="symbol"/> is not partial, <see langword="false"/> otherwise.</returns>
			public static bool AnalyzeAgainstPartial(
				IDiagnosticReceiver diagnosticReceiver,
				INamedTypeSymbol symbol,
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

			/// <inheritdoc cref="DefaultParamAnalyzer.WithDiagnostics.AnalyzeAgainstProhibitedAttributes(IDiagnosticReceiver, ISymbol, DefaultParamCompilationData, IEnumerable{AttributeData}?)"/>
			public static bool AnalyzeAgainstProhibitedAttributes(
				IDiagnosticReceiver diagnosticReceiver,
				ISymbol symbol,
				DefaultParamCompilationData compilation,
				IEnumerable<AttributeData>? attributes = null
			)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeAgainstProhibitedAttributes(
					diagnosticReceiver,
					symbol,
					compilation,
					attributes
				);
			}

			/// <inheritdoc cref="DefaultParamAnalyzer.WithDiagnostics.AnalyzeAgainstProhibitedAttributes(IDiagnosticReceiver, ISymbol, DefaultParamCompilationData, out AttributeData[])"/>
			public static bool AnalyzeAgainstProhibitedAttributes(
				IDiagnosticReceiver diagnosticReceiver,
				INamedTypeSymbol symbol,
				DefaultParamCompilationData compilation,
				[NotNullWhen(true)] out AttributeData[]? attributes
			)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeAgainstProhibitedAttributes(
					diagnosticReceiver,
					symbol,
					compilation,
					out attributes
				);
			}

			/// <inheritdoc cref="DefaultParamDelegateAnalyzer.WithDiagnostics.AnalyzeCollidingMembers(IDiagnosticReceiver, INamedTypeSymbol, in TypeParameterContainer, DefaultParamCompilationData, string, out HashSet{int}?, IEnumerable{AttributeData}?, INamedTypeSymbol[], CancellationToken)"/>
			public static bool AnalyzeCollidingMembers(
				IDiagnosticReceiver diagnosticReceiver,
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
				InitializeAttributes(ref attributes, symbol);
				InitializeContainingTypes(ref containingTypes, symbol);

				bool allowsNew = AllowsNewModifier(symbol, compilation, attributes, containingTypes);

				return AnalyzeCollidingMembers(
					diagnosticReceiver,
					symbol,
					in typeParameters,
					compilation,
					targetNamespace,
					out applyNew,
					allowsNew,
					cancellationToken
				);
			}

			/// <inheritdoc cref="DefaultParamDelegateAnalyzer.WithDiagnostics.AnalyzeCollidingMembers(IDiagnosticReceiver, INamedTypeSymbol, in TypeParameterContainer, DefaultParamCompilationData, string, out HashSet{int}?, bool, CancellationToken)"/>
			public static bool AnalyzeCollidingMembers(
				IDiagnosticReceiver diagnosticReceiver,
				INamedTypeSymbol symbol,
				in TypeParameterContainer typeParameters,
				DefaultParamCompilationData compilation,
				string targetNamespace,
				out HashSet<int>? applyNew,
				bool allowsNewModifier,
				CancellationToken cancellationToken = default
			)
			{
				return DefaultParamDelegateAnalyzer.WithDiagnostics.AnalyzeCollidingMembers(
					diagnosticReceiver,
					symbol,
					in typeParameters,
					compilation,
					targetNamespace,
					out applyNew,
					allowsNewModifier,
					cancellationToken
				);
			}

			/// <inheritdoc cref="DefaultParamAnalyzer.WithDiagnostics.AnalyzeContainingTypes(IDiagnosticReceiver, ISymbol, DefaultParamCompilationData, out INamedTypeSymbol[], CancellationToken)"/>
			public static bool AnalyzeContainingTypes(
				IDiagnosticReceiver diagnosticReceiver,
				ISymbol symbol,
				DefaultParamCompilationData compilation,
				[NotNullWhen(true)] out INamedTypeSymbol[]? containingTypes,
				CancellationToken cancellationToken = default
			)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeContainingTypes(
					diagnosticReceiver,
					symbol,
					compilation,
					out containingTypes,
					cancellationToken
				);
			}

			/// <inheritdoc cref="DefaultParamAnalyzer.WithDiagnostics.AnalyzeContainingTypes(IDiagnosticReceiver, ISymbol, DefaultParamCompilationData, INamedTypeSymbol[], CancellationToken)"/>
			public static bool AnalyzeContainingTypes(
				IDiagnosticReceiver diagnosticReceiver,
				ISymbol symbol,
				DefaultParamCompilationData compilation,
				INamedTypeSymbol[]? containingTypes = null,
				CancellationToken cancellationToken = default
			)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeContainingTypes(
					diagnosticReceiver,
					symbol,
					compilation,
					containingTypes,
					cancellationToken
				);
			}

			/// <inheritdoc cref="DefaultParamAnalyzer.WithDiagnostics.AnalyzeContainingTypes(IDiagnosticReceiver, ISymbol, DefaultParamCompilationData, out INamedTypeSymbol[], CancellationToken)"/>
			public static bool AnalyzeContainingTypes(
				IDiagnosticReceiver diagnosticReceiver,
				INamedTypeSymbol symbol,
				DefaultParamCompilationData compilation,
				[NotNullWhen(true)] out INamedTypeSymbol[]? containingTypes,
				CancellationToken cancellationToken = default
			)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeContainingTypes(
					diagnosticReceiver,
					symbol,
					compilation,
					out containingTypes,
					cancellationToken
				);
			}

			/// <inheritdoc cref="DefaultParamAnalyzer.WithDiagnostics.AnalyzeContainingTypes(IDiagnosticReceiver, ISymbol, DefaultParamCompilationData, out ITypeData[])"/>
			public static bool AnalyzeContainingTypes(
				IDiagnosticReceiver diagnosticReceiver,
				INamedTypeSymbol symbol,
				DefaultParamCompilationData compilation,
				[NotNullWhen(true)] out ITypeData[]? containingTypes
			)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeContainingTypes(
					diagnosticReceiver,
					symbol,
					compilation,
					out containingTypes
				);
			}

			/// <inheritdoc cref="DefaultParamAnalyzer.WithDiagnostics.AnalyzeTypeParameters(IDiagnosticReceiver, ISymbol, in TypeParameterContainer)"/>
			public static bool AnalyzeTypeParameters(
				IDiagnosticReceiver diagnosticReceiver,
				INamedTypeSymbol symbol,
				in TypeParameterContainer typeParameters
			)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeTypeParameters(diagnosticReceiver, symbol, in typeParameters);
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
			public static bool ShouldInheritInsteadOfCopying(
				IDiagnosticReceiver diagnosticReceiver,
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
		}
	}
}
