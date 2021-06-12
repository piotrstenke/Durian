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
using static Durian.Generator.DefaultParam.DefaultParamTypeAnalyzer.WithDiagnostics;

namespace Durian.Generator.DefaultParam
{
	public partial class DefaultParamTypeFilter
	{
		/// <summary>
		/// Contains <see langword="static"/> methods that validate <see cref="TypeDeclarationSyntax"/>es and report <see cref="Diagnostic"/>s for the invalid ones.
		/// </summary>
		public static class WithDiagnostics
		{
			/// <summary>
			/// Enumerates through all the <see cref="TypeDeclarationSyntax"/>es collected by the <paramref name="syntaxReceiver"/> and returns an array of <see cref="DefaultParamTypeData"/>s created from the valid ones.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="syntaxReceiver"><see cref="DefaultParamSyntaxReceiver"/> that collected the <see cref="TypeDeclarationSyntax"/>es.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			public static DefaultParamTypeData[] GetValidTypes(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				DefaultParamSyntaxReceiver syntaxReceiver,
				CancellationToken cancellationToken = default
			)
			{
				if (compilation is null || diagnosticReceiver is null || syntaxReceiver is null || syntaxReceiver.CandidateTypes.Count == 0)
				{
					return Array.Empty<DefaultParamTypeData>();
				}

				return GetValidTypes_Internal(diagnosticReceiver, compilation, syntaxReceiver.CandidateTypes.ToArray(), cancellationToken);
			}

			/// <summary>
			/// Enumerates through all the <see cref="TypeDeclarationSyntax"/>es collected by the <paramref name="syntaxReceiver"/> and returns an array of <see cref="DefaultParamTypeData"/>s created from the valid ones. If the target <see cref="DefaultParamTypeData"/> already exists in the specified <paramref name="cache"/>, includes it instead.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="syntaxReceiver"><see cref="DefaultParamSyntaxReceiver"/> that collected the <see cref="TypeDeclarationSyntax"/>es.</param>
			/// <param name="cache">Container of cached <see cref="DefaultParamTypeData"/>s.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			public static DefaultParamTypeData[] GetValidTypes(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				DefaultParamSyntaxReceiver syntaxReceiver,
				in CachedData<DefaultParamTypeData> cache,
				CancellationToken cancellationToken = default
			)
			{
				if (compilation is null || diagnosticReceiver is null || syntaxReceiver is null || syntaxReceiver.CandidateTypes.Count == 0)
				{
					return Array.Empty<DefaultParamTypeData>();
				}

				return GetValidTypes_Internal(diagnosticReceiver, compilation, syntaxReceiver.CandidateTypes.ToArray(), cancellationToken, in cache);
			}

			/// <summary>
			/// Enumerates through all the <paramref name="collectedTypes"/> and returns an array of <see cref="DefaultParamTypeData"/>s created from the valid ones.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="collectedTypes">A collection of <see cref="TypeDeclarationSyntax"/>es to validate.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			public static DefaultParamTypeData[] GetValidTypes(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				IEnumerable<TypeDeclarationSyntax> collectedTypes,
				CancellationToken cancellationToken = default
			)
			{
				if (compilation is null || diagnosticReceiver is null || collectedTypes is null)
				{
					return Array.Empty<DefaultParamTypeData>();
				}

				TypeDeclarationSyntax[] collected = collectedTypes.ToArray();

				if (collected.Length == 0)
				{
					return Array.Empty<DefaultParamTypeData>();
				}

				return GetValidTypes_Internal(diagnosticReceiver, compilation, collected, cancellationToken);
			}

			/// <summary>
			/// Enumerates through all the <paramref name="collectedTypes"/> and returns an array of <see cref="DefaultParamTypeData"/>s created from the valid ones. If the target <see cref="DefaultParamTypeData"/> already exists in the specified <paramref name="cache"/>, includes it instead.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="collectedTypes">A collection of <see cref="TypeDeclarationSyntax"/>es to validate.</param>
			/// <param name="cache">Container of cached <see cref="DefaultParamTypeData"/>s.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			public static DefaultParamTypeData[] GetValidTypes(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				IEnumerable<TypeDeclarationSyntax> collectedTypes,
				in CachedData<DefaultParamTypeData> cache,
				CancellationToken cancellationToken = default
			)
			{
				if (compilation is null || diagnosticReceiver is null || collectedTypes is null)
				{
					return Array.Empty<DefaultParamTypeData>();
				}

				TypeDeclarationSyntax[] array = collectedTypes.ToArray();

				if (array.Length == 0)
				{
					return Array.Empty<DefaultParamTypeData>();
				}

				return GetValidTypes_Internal(diagnosticReceiver, compilation, array, cancellationToken, in cache);
			}

			/// <summary>
			/// Validates the specified <paramref name="declaration"/> and returns a new instance of <see cref="DefaultParamTypeData"/> if the validation was a success.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="declaration"><see cref="DefaultParamTypeData"/> to validate.</param>
			/// <param name="data">Newly-created instance of <see cref="DefaultParamTypeData"/>.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			public static bool ValidateAndCreate(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				TypeDeclarationSyntax declaration,
				[NotNullWhen(true)] out DefaultParamTypeData? data,
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
			/// Validates the specified <paramref name="declaration"/> and returns a new instance of <see cref="DefaultParamTypeData"/> if the validation was a success. If the target <see cref="DefaultParamTypeData"/> already exists in the specified <paramref name="cache"/>, includes it instead.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="declaration"><see cref="DefaultParamTypeData"/> to validate.</param>
			/// <param name="cache">Container of cached <see cref="DefaultParamTypeData"/>s.</param>
			/// <param name="data">Newly-created instance of <see cref="DefaultParamTypeData"/>.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			public static bool ValidateAndCreate(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				TypeDeclarationSyntax declaration,
				in CachedData<DefaultParamTypeData> cache,
				[NotNullWhen(true)] out DefaultParamTypeData? data,
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
			/// Validates the specified <paramref name="declaration"/> and returns a new instance of <see cref="DefaultParamTypeData"/> if the validation was a success.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="declaration"><see cref="DefaultParamTypeData"/> to validate.</param>
			/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
			/// <param name="symbol"><see cref="INamedTypeSymbol"/> created from the <paramref name="declaration"/>.</param>
			/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains the <paramref name="declaration"/>'s type parameters.</param>
			/// <param name="data">Newly-created instance of <see cref="DefaultParamTypeData"/>.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			public static bool ValidateAndCreate(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				TypeDeclarationSyntax declaration,
				SemanticModel semanticModel,
				INamedTypeSymbol symbol,
				in TypeParameterContainer typeParameters,
				[NotNullWhen(true)] out DefaultParamTypeData? data,
				CancellationToken cancellationToken = default
			)
			{
				bool isValid = AnalyzeAgainstProhibitedAttributes(diagnosticReceiver, symbol, compilation, out AttributeData[]? attributes);
				isValid &= AnalyzeAgainstPartial(diagnosticReceiver, symbol, cancellationToken);
				isValid &= AnalyzeContainingTypes(diagnosticReceiver, symbol, compilation, out ITypeData[]? containingTypes);
				isValid &= AnalyzeTypeParameters(diagnosticReceiver, symbol, in typeParameters);

				if (isValid)
				{
					INamedTypeSymbol[] symbols = DefaultParamUtilities.TypeDatasToTypeSymbols(containingTypes);

					if (AnalyzeCollidingMembers(diagnosticReceiver, symbol, in typeParameters, compilation, attributes!, symbols, out HashSet<int>? applyNewModifiers, cancellationToken))
					{
						bool inherit = ShouldInheritInsteadOfCopying(diagnosticReceiver, symbol, compilation, attributes!, symbols);
						string targetNamespace = DefaultParamAnalyzer.GetTargetNamespace(symbol, attributes!, symbols, compilation);

						data = new DefaultParamTypeData(
							declaration,
							compilation,
							symbol,
							semanticModel,
							null,
							null,
							containingTypes,
							null,
							attributes,
							typeParameters,
							applyNewModifiers,
							inherit,
							targetNamespace
						);

						return true;
					}
				}
				else
				{
					ShouldInheritInsteadOfCopying(diagnosticReceiver, symbol, compilation);
				}

				data = null;
				return false;
			}

			/// <summary>
			/// Validates the specified <paramref name="declaration"/> and returns a new instance of <see cref="DefaultParamTypeData"/> if the validation was a success. If the target <see cref="DefaultParamTypeData"/> already exists in the specified <paramref name="cache"/>, includes it instead.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="declaration"><see cref="DefaultParamTypeData"/> to validate.</param>
			/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
			/// <param name="symbol"><see cref="INamedTypeSymbol"/> created from the <paramref name="declaration"/>.</param>
			/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains the <paramref name="declaration"/>'s type parameters.</param>
			/// <param name="cache">Container of cached <see cref="DefaultParamTypeData"/>s.</param>
			/// <param name="data">Newly-created instance of <see cref="DefaultParamTypeData"/>.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			public static bool ValidateAndCreate(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				TypeDeclarationSyntax declaration,
				SemanticModel semanticModel,
				INamedTypeSymbol symbol,
				in TypeParameterContainer typeParameters,
				in CachedData<DefaultParamTypeData> cache,
				[NotNullWhen(true)] out DefaultParamTypeData? data,
				CancellationToken cancellationToken = default
			)
			{
				if (cache.TryGetCachedValue(declaration.GetLocation().GetLineSpan(), out data))
				{
					return true;
				}

				return ValidateAndCreate(diagnosticReceiver, compilation, declaration, semanticModel, symbol, in typeParameters, out data, cancellationToken);
			}

			private static DefaultParamTypeData[] GetValidTypes_Internal(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				TypeDeclarationSyntax[] collectedTypes,
				CancellationToken cancellationToken
			)
			{
				List<DefaultParamTypeData> list = new(collectedTypes.Length);

				foreach (TypeDeclarationSyntax decl in collectedTypes)
				{
					if (decl is null)
					{
						continue;
					}

					if (ValidateAndCreate(diagnosticReceiver, compilation, decl, out DefaultParamTypeData? data, cancellationToken))
					{
						list.Add(data!);
					}
				}

				return list.ToArray();
			}

			private static DefaultParamTypeData[] GetValidTypes_Internal(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				TypeDeclarationSyntax[] collectedTypes,
				CancellationToken cancellationToken,
				in CachedData<DefaultParamTypeData> cache
			)
			{
				List<DefaultParamTypeData> list = new(collectedTypes.Length);

				foreach (TypeDeclarationSyntax decl in collectedTypes)
				{
					if (decl is null)
					{
						continue;
					}

					if (cache.TryGetCachedValue(decl.GetLocation().GetLineSpan(), out DefaultParamTypeData? data) ||
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
