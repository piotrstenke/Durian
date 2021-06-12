// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Durian.Generator.Cache;
using Durian.Generator.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Durian.Generator.DefaultParam.DefaultParamMethodAnalyzer;
using static Durian.Generator.DefaultParam.DefaultParamMethodAnalyzer.WithDiagnostics;

namespace Durian.Generator.DefaultParam
{
	public partial class DefaultParamMethodFilter
	{
		/// <summary>
		/// Contains <see langword="static"/> methods that validate <see cref="DefaultParamMethodData"/>es and report <see cref="Diagnostic"/>s for the invalid ones.
		/// </summary>
		public static class WithDiagnostics
		{
			/// <summary>
			/// Enumerates through all the <see cref="MethodDeclarationSyntax"/>es collected by the <paramref name="syntaxReceiver"/> and returns an array of <see cref="DefaultParamMethodData"/>s created from the valid ones.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="syntaxReceiver"><see cref="DefaultParamSyntaxReceiver"/> that collected the <see cref="MethodDeclarationSyntax"/>es.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			public static DefaultParamMethodData[] GetValidMethods(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				DefaultParamSyntaxReceiver syntaxReceiver,
				CancellationToken cancellationToken = default
			)
			{
				if (compilation is null || diagnosticReceiver is null || syntaxReceiver is null || syntaxReceiver.CandidateMethods.Count == 0)
				{
					return Array.Empty<DefaultParamMethodData>();
				}

				return GetValidMethods_Internal(diagnosticReceiver, compilation, syntaxReceiver.CandidateMethods.ToArray(), cancellationToken);
			}

			/// <summary>
			/// Enumerates through all the <see cref="MethodDeclarationSyntax"/>es collected by the <paramref name="syntaxReceiver"/> and returns an array of <see cref="DefaultParamMethodData"/>s created from the valid ones. If the target <see cref="DefaultParamMethodData"/> already exists in the specified <paramref name="cache"/>, includes it instead.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="syntaxReceiver"><see cref="DefaultParamSyntaxReceiver"/> that collected the <see cref="MethodDeclarationSyntax"/>es.</param>
			/// <param name="cache">Container of cached <see cref="DefaultParamMethodData"/>s.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			public static DefaultParamMethodData[] GetValidMethods(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				DefaultParamSyntaxReceiver syntaxReceiver,
				in CachedData<DefaultParamMethodData> cache,
				CancellationToken cancellationToken = default
			)
			{
				if (compilation is null || diagnosticReceiver is null || syntaxReceiver is null || syntaxReceiver.CandidateMethods.Count == 0)
				{
					return Array.Empty<DefaultParamMethodData>();
				}

				return GetValidMethods_Internal(diagnosticReceiver, compilation, syntaxReceiver.CandidateMethods.ToArray(), cancellationToken, in cache);
			}

			/// <summary>
			/// Enumerates through all the <paramref name="collectedMethods"/> and returns an array of <see cref="DefaultParamMethodData"/>s created from the valid ones.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="collectedMethods">A collection of <see cref="MethodDeclarationSyntax"/>es to validate.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			public static DefaultParamMethodData[] GetValidMethods(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				IEnumerable<MethodDeclarationSyntax> collectedMethods,
				CancellationToken cancellationToken = default
			)
			{
				if (compilation is null || diagnosticReceiver is null || collectedMethods is null)
				{
					return Array.Empty<DefaultParamMethodData>();
				}

				MethodDeclarationSyntax[] array = collectedMethods.ToArray();

				if (array.Length == 0)
				{
					return Array.Empty<DefaultParamMethodData>();
				}

				return GetValidMethods_Internal(diagnosticReceiver, compilation, array, cancellationToken);
			}

			/// <summary>
			/// Enumerates through all the <paramref name="collectedMethods"/> and returns an array of <see cref="DefaultParamMethodData"/>s created from the valid ones. If the target <see cref="DefaultParamMethodData"/> already exists in the specified <paramref name="cache"/>, includes it instead.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="collectedMethods">A collection of <see cref="MethodDeclarationSyntax"/>es to validate.</param>
			/// <param name="cache">Container of cached <see cref="DefaultParamMethodData"/>s.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			public static DefaultParamMethodData[] GetValidMethods(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				IEnumerable<MethodDeclarationSyntax> collectedMethods,
				in CachedData<DefaultParamMethodData> cache,
				CancellationToken cancellationToken = default
			)
			{
				if (compilation is null || diagnosticReceiver is null || collectedMethods is null)
				{
					return Array.Empty<DefaultParamMethodData>();
				}

				MethodDeclarationSyntax[] array = collectedMethods.ToArray();

				if (array.Length == 0)
				{
					return Array.Empty<DefaultParamMethodData>();
				}

				return GetValidMethods_Internal(diagnosticReceiver, compilation, array, cancellationToken, in cache);
			}

			/// <summary>
			/// Validates the specified <paramref name="declaration"/> and returns a new instance of <see cref="DefaultParamMethodData"/> if the validation was a success.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="declaration"><see cref="DefaultParamMethodData"/> to validate.</param>
			/// <param name="data">Newly-created instance of <see cref="DefaultParamMethodData"/>.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			public static bool ValidateAndCreate(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				MethodDeclarationSyntax declaration,
				[NotNullWhen(true)] out DefaultParamMethodData? data,
				CancellationToken cancellationToken = default
			)
			{
				if (!GetValidationData(compilation, declaration, out SemanticModel? semanticModel, out IMethodSymbol? symbol, out TypeParameterContainer typeParameters, cancellationToken))
				{
					data = null;
					return false;
				}

				return ValidateAndCreate(diagnosticReceiver, compilation, declaration, semanticModel, symbol, in typeParameters, out data, cancellationToken);
			}

			/// <summary>
			/// Validates the specified <paramref name="declaration"/> and returns a new instance of <see cref="DefaultParamMethodData"/> if the validation was a success. If the target <see cref="DefaultParamMethodData"/> already exists in the specified <paramref name="cache"/>, includes it instead.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="declaration"><see cref="DefaultParamMethodData"/> to validate.</param>
			/// <param name="cache">Container of cached <see cref="DefaultParamMethodData"/>s.</param>
			/// <param name="data">Newly-created instance of <see cref="DefaultParamMethodData"/>.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			public static bool ValidateAndCreate(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				MethodDeclarationSyntax declaration,
				in CachedData<DefaultParamMethodData> cache,
				[NotNullWhen(true)] out DefaultParamMethodData? data,
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
			/// Validates the specified <paramref name="declaration"/> and returns a new instance of <see cref="DefaultParamMethodData"/> if the validation was a success.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="declaration"><see cref="DefaultParamMethodData"/> to validate.</param>
			/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
			/// <param name="symbol"><see cref="INamedTypeSymbol"/> created from the <paramref name="declaration"/>.</param>
			/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains the <paramref name="declaration"/>'s type parameters.</param>
			/// <param name="data">Newly-created instance of <see cref="DefaultParamMethodData"/>.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			public static bool ValidateAndCreate(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				MethodDeclarationSyntax declaration,
				SemanticModel semanticModel,
				IMethodSymbol symbol,
				in TypeParameterContainer typeParameters,
				[NotNullWhen(true)] out DefaultParamMethodData? data,
				CancellationToken cancellationToken = default
			)
			{
				if (!ShouldBeAnalyzed(diagnosticReceiver, symbol, in typeParameters, compilation, out TypeParameterContainer combinedTypeParameters, cancellationToken))
				{
					data = null;
					return false;
				}

				bool isValid = AnalyzeAgainstInvalidMethodType(diagnosticReceiver, symbol);
				isValid &= AnalyzeAgainstPartialOrExtern(diagnosticReceiver, symbol, declaration);
				isValid &= AnalyzeAgainstProhibitedAttributes(diagnosticReceiver, symbol, compilation, out AttributeData[]? attributes);
				isValid &= AnalyzeContainingTypes(diagnosticReceiver, symbol, compilation, out ITypeData[]? containingTypes);

				if (isValid)
				{
					TypeParameterContainer combinedParameters = typeParameters;

					if (AnalyzeBaseMethodAndTypeParameters(diagnosticReceiver, symbol, ref combinedParameters, in combinedTypeParameters))
					{
						INamedTypeSymbol[] symbols = DefaultParamUtilities.TypeDatasToTypeSymbols(containingTypes!);

						if (AnalyzeMethodSignature(diagnosticReceiver, symbol, in combinedParameters, compilation, attributes!, symbols, out HashSet<int>? newModifiers, cancellationToken))
						{
							bool call = ShouldCallInsteadOfCopying(symbol, compilation, attributes!, symbols);
							string targetNamespace = DefaultParamAnalyzer.GetTargetNamespace(symbol, attributes!, symbols, compilation);

							data = new(
								declaration,
								compilation,
								symbol,
								semanticModel,
								containingTypes,
								null,
								attributes,
								in combinedParameters,
								newModifiers,
								call,
								targetNamespace
							);

							return true;
						}
					}
				}

				data = null;
				return false;
			}

			/// <summary>
			/// Validates the specified <paramref name="declaration"/> and returns a new instance of <see cref="DefaultParamMethodData"/> if the validation was a success. If the target <see cref="DefaultParamMethodData"/> already exists in the specified <paramref name="cache"/>, includes it instead.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="declaration"><see cref="DefaultParamMethodData"/> to validate.</param>
			/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
			/// <param name="symbol"><see cref="IMethodSymbol"/> created from the <paramref name="declaration"/>.</param>
			/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains the <paramref name="declaration"/>'s type parameters.</param>
			/// <param name="cache">Container of cached <see cref="DefaultParamMethodData"/>s.</param>
			/// <param name="data">Newly-created instance of <see cref="DefaultParamMethodData"/>.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			public static bool ValidateAndCreate(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				MethodDeclarationSyntax declaration,
				SemanticModel semanticModel,
				IMethodSymbol symbol,
				in TypeParameterContainer typeParameters,
				in CachedData<DefaultParamMethodData> cache,
				[NotNullWhen(true)] out DefaultParamMethodData? data,
				CancellationToken cancellationToken = default
			)
			{
				if (cache.TryGetCachedValue(declaration.GetLocation().GetLineSpan(), out data))
				{
					return true;
				}

				return ValidateAndCreate(diagnosticReceiver, compilation, declaration, semanticModel, symbol, in typeParameters, out data, cancellationToken);
			}

			private static DefaultParamMethodData[] GetValidMethods_Internal(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				MethodDeclarationSyntax[] collectedMethods,
				CancellationToken cancellationToken
			)
			{
				List<DefaultParamMethodData> list = new(collectedMethods.Length);

				foreach (MethodDeclarationSyntax decl in collectedMethods)
				{
					if (decl is null)
					{
						continue;
					}

					if (ValidateAndCreate(diagnosticReceiver, compilation, decl, out DefaultParamMethodData? data, cancellationToken))
					{
						list.Add(data!);
					}
				}

				return list.ToArray();
			}

			private static DefaultParamMethodData[] GetValidMethods_Internal(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				MethodDeclarationSyntax[] collectedMethods,
				CancellationToken cancellationToken,
				in CachedData<DefaultParamMethodData> cache
			)
			{
				List<DefaultParamMethodData> list = new(collectedMethods.Length);

				foreach (MethodDeclarationSyntax decl in collectedMethods)
				{
					if (decl is null)
					{
						continue;
					}

					if (cache.TryGetCachedValue(decl.GetLocation().GetLineSpan(), out DefaultParamMethodData? data) ||
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
