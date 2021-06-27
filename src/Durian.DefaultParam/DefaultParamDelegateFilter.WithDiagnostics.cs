// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Durian.Analysis.Cache;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Durian.Analysis.DefaultParam.DefaultParamDelegateAnalyzer.WithDiagnostics;

namespace Durian.Analysis.DefaultParam
{
	public partial class DefaultParamDelegateFilter
	{
		/// <summary>
		/// Contains <see langword="static"/> methods that validate <see cref="DelegateDeclarationSyntax"/>es and report <see cref="Diagnostic"/>s for the invalid ones.
		/// </summary>
		public static class WithDiagnostics
		{
			/// <summary>
			/// Enumerates through all the <see cref="DelegateDeclarationSyntax"/>es collected by the <paramref name="syntaxReceiver"/> and returns an array of <see cref="DefaultParamDelegateData"/>s created from the valid ones.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="syntaxReceiver"><see cref="DefaultParamSyntaxReceiver"/> that collected the <see cref="DelegateDeclarationSyntax"/>es.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			public static DefaultParamDelegateData[] GetValidDelegates(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				DefaultParamSyntaxReceiver syntaxReceiver,
				CancellationToken cancellationToken = default
			)
			{
				if (compilation is null || diagnosticReceiver is null || syntaxReceiver is null || syntaxReceiver.CandidateDelegates.Count == 0)
				{
					return Array.Empty<DefaultParamDelegateData>();
				}

				return GetValidDelegates_Internal(diagnosticReceiver, compilation, syntaxReceiver.CandidateDelegates.ToArray(), cancellationToken);
			}

			/// <summary>
			/// Enumerates through all the <see cref="DelegateDeclarationSyntax"/>es collected by the <paramref name="syntaxReceiver"/> and returns an array of <see cref="DefaultParamDelegateData"/>s created from the valid ones. If the target <see cref="DefaultParamDelegateData"/> already exists in the specified <paramref name="cache"/>, includes it instead.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="syntaxReceiver"><see cref="DefaultParamSyntaxReceiver"/> that collected the <see cref="DelegateDeclarationSyntax"/>es.</param>
			/// <param name="cache">Container of cached <see cref="DefaultParamDelegateData"/>s.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			public static DefaultParamDelegateData[] GetValidDelegates(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				DefaultParamSyntaxReceiver syntaxReceiver,
				in CachedData<DefaultParamDelegateData> cache,
				CancellationToken cancellationToken = default
			)
			{
				if (compilation is null || diagnosticReceiver is null || syntaxReceiver is null || syntaxReceiver.CandidateDelegates.Count == 0)
				{
					return Array.Empty<DefaultParamDelegateData>();
				}

				return GetValidDelegates_Internal(diagnosticReceiver, compilation, syntaxReceiver.CandidateDelegates.ToArray(), cancellationToken, in cache);
			}

			/// <summary>
			/// Enumerates through all the <paramref name="collectedDelegates"/> and returns an array of <see cref="DefaultParamDelegateData"/>s created from the valid ones.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="collectedDelegates">A collection of <see cref="DelegateDeclarationSyntax"/>es to validate.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			public static DefaultParamDelegateData[] GetValidDelegates(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				IEnumerable<DelegateDeclarationSyntax> collectedDelegates,
				CancellationToken cancellationToken = default
			)
			{
				if (compilation is null || diagnosticReceiver is null || collectedDelegates is null)
				{
					return Array.Empty<DefaultParamDelegateData>();
				}

				DelegateDeclarationSyntax[] collected = collectedDelegates.ToArray();

				if (collected.Length == 0)
				{
					return Array.Empty<DefaultParamDelegateData>();
				}

				return GetValidDelegates_Internal(diagnosticReceiver, compilation, collected, cancellationToken);
			}

			/// <summary>
			/// Enumerates through all the <paramref name="collectedDelegates"/> and returns an array of <see cref="DefaultParamDelegateData"/>s created from the valid ones. If the target <see cref="DefaultParamDelegateData"/> already exists in the specified <paramref name="cache"/>, includes it instead.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="collectedDelegates">A collection of <see cref="DelegateDeclarationSyntax"/>es to validate.</param>
			/// <param name="cache">Container of cached <see cref="DefaultParamDelegateData"/>s.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			public static DefaultParamDelegateData[] GetValidDelegates(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				IEnumerable<DelegateDeclarationSyntax> collectedDelegates,
				in CachedData<DefaultParamDelegateData> cache,
				CancellationToken cancellationToken = default
			)
			{
				if (compilation is null || diagnosticReceiver is null || collectedDelegates is null)
				{
					return Array.Empty<DefaultParamDelegateData>();
				}

				DelegateDeclarationSyntax[] array = collectedDelegates.ToArray();

				if (array.Length == 0)
				{
					return Array.Empty<DefaultParamDelegateData>();
				}

				return GetValidDelegates_Internal(diagnosticReceiver, compilation, array, cancellationToken, in cache);
			}

			/// <summary>
			/// Validates the specified <paramref name="declaration"/> and returns a new instance of <see cref="DefaultParamDelegateData"/> if the validation was a success.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="declaration"><see cref="DefaultParamDelegateData"/> to validate.</param>
			/// <param name="data">Newly-created instance of <see cref="DefaultParamDelegateData"/>.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			public static bool ValidateAndCreate(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				DelegateDeclarationSyntax declaration,
				[NotNullWhen(true)] out DefaultParamDelegateData? data,
				CancellationToken cancellationToken = default
			)
			{
				if (!GetValidationData(compilation, declaration, out SemanticModel? semanticModel, out INamedTypeSymbol? symbol, out TypeParameterContainer typeParameters, cancellationToken))
				{
					data = null;
					return false;
				}

				return ValidateAndCreate(diagnosticReceiver, compilation, declaration, semanticModel, symbol, in typeParameters, out data, cancellationToken);
			}

			/// <summary>
			/// Validates the specified <paramref name="declaration"/> and returns a new instance of <see cref="DefaultParamDelegateData"/> if the validation was a success. If the target <see cref="DefaultParamDelegateData"/> already exists in the specified <paramref name="cache"/>, includes it instead.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="declaration"><see cref="DefaultParamDelegateData"/> to validate.</param>
			/// <param name="cache">Container of cached <see cref="DefaultParamDelegateData"/>s.</param>
			/// <param name="data">Newly-created instance of <see cref="DefaultParamDelegateData"/>.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			public static bool ValidateAndCreate(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				DelegateDeclarationSyntax declaration,
				in CachedData<DefaultParamDelegateData> cache,
				[NotNullWhen(true)] out DefaultParamDelegateData? data,
				CancellationToken cancellationToken = default
			)
			{
				if (cache.TryGetCachedValue(declaration.GetLocation().GetLineSpan(), out data))
				{
					return true;
				}

				return ValidateAndCreate(diagnosticReceiver, compilation, declaration, out data, cancellationToken);
			}

			/// <summary>
			/// Validates the specified <paramref name="declaration"/> and returns a new instance of <see cref="DefaultParamDelegateData"/> if the validation was a success.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="declaration"><see cref="DefaultParamDelegateData"/> to validate.</param>
			/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
			/// <param name="symbol"><see cref="INamedTypeSymbol"/> created from the <paramref name="declaration"/>.</param>
			/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains the <paramref name="declaration"/>'s type parameters.</param>
			/// <param name="data">Newly-created instance of <see cref="DefaultParamDelegateData"/>.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			public static bool ValidateAndCreate(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				DelegateDeclarationSyntax declaration,
				SemanticModel semanticModel,
				INamedTypeSymbol symbol,
				in TypeParameterContainer typeParameters,
				[NotNullWhen(true)] out DefaultParamDelegateData? data,
				CancellationToken cancellationToken = default
			)
			{
				bool isValid = AnalyzeAgainstProhibitedAttributes(diagnosticReceiver, symbol, compilation, out AttributeData[]? attributes);
				isValid &= AnalyzeContainingTypes(diagnosticReceiver, symbol, compilation, out ITypeData[]? containingTypes);
				isValid &= AnalyzeTypeParameters(diagnosticReceiver, symbol, in typeParameters);

				if (isValid)
				{
					INamedTypeSymbol[] symbols = DefaultParamUtilities.TypeDatasToTypeSymbols(containingTypes!);
					string targetNamespace = DefaultParamAnalyzer.GetTargetNamespace(symbol, compilation, attributes!, symbols);

					if (AnalyzeCollidingMembers(diagnosticReceiver, symbol, in typeParameters, compilation, targetNamespace, out HashSet<int>? applyNewModifiers, attributes!, symbols, cancellationToken))
					{
						data = new DefaultParamDelegateData(
							declaration,
							compilation,
							symbol,
							semanticModel,
							containingTypes,
							null,
							attributes,
							typeParameters,
							applyNewModifiers,
							targetNamespace
						);

						return true;
					}
				}

				data = null;
				return false;
			}

			/// <summary>
			/// Validates the specified <paramref name="declaration"/> and returns a new instance of <see cref="DefaultParamDelegateData"/> if the validation was a success. If the target <see cref="DefaultParamDelegateData"/> already exists in the specified <paramref name="cache"/>, includes it instead.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="declaration"><see cref="DefaultParamDelegateData"/> to validate.</param>
			/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
			/// <param name="symbol"><see cref="INamedTypeSymbol"/> created from the <paramref name="declaration"/>.</param>
			/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains the <paramref name="declaration"/>'s type parameters.</param>
			/// <param name="cache">Container of cached <see cref="DefaultParamDelegateData"/>s.</param>
			/// <param name="data">Newly-created instance of <see cref="DefaultParamDelegateData"/>.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			public static bool ValidateAndCreate(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				DelegateDeclarationSyntax declaration,
				SemanticModel semanticModel,
				INamedTypeSymbol symbol,
				in TypeParameterContainer typeParameters,
				in CachedData<DefaultParamDelegateData> cache,
				[NotNullWhen(true)] out DefaultParamDelegateData? data,
				CancellationToken cancellationToken = default
			)
			{
				if (cache.TryGetCachedValue(declaration.GetLocation().GetLineSpan(), out data))
				{
					return true;
				}

				return ValidateAndCreate(diagnosticReceiver, compilation, declaration, semanticModel, symbol, in typeParameters, out data, cancellationToken);
			}

			private static DefaultParamDelegateData[] GetValidDelegates_Internal(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				DelegateDeclarationSyntax[] collectedDelegates,
				CancellationToken cancellationToken
			)
			{
				List<DefaultParamDelegateData> list = new(collectedDelegates.Length);

				foreach (DelegateDeclarationSyntax decl in collectedDelegates)
				{
					if (decl is null)
					{
						continue;
					}

					if (ValidateAndCreate(diagnosticReceiver, compilation, decl, out DefaultParamDelegateData? data, cancellationToken))
					{
						list.Add(data!);
					}
				}

				return list.ToArray();
			}

			private static DefaultParamDelegateData[] GetValidDelegates_Internal(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				DelegateDeclarationSyntax[] collectedDelegates,
				CancellationToken cancellationToken,
				in CachedData<DefaultParamDelegateData> cache
			)
			{
				List<DefaultParamDelegateData> list = new(collectedDelegates.Length);

				foreach (DelegateDeclarationSyntax decl in collectedDelegates)
				{
					if (decl is null)
					{
						continue;
					}

					if (cache.TryGetCachedValue(decl.GetLocation().GetLineSpan(), out DefaultParamDelegateData? data) ||
						ValidateAndCreate(diagnosticReceiver, compilation, decl, out data, cancellationToken))
					{
						list.Add(data!);
					}
				}

				return list.ToArray();
			}
		}
	}
}
