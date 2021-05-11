﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Durian.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Durian.DefaultParam.DefaultParamMethodAnalyzer;
using static Durian.DefaultParam.DefaultParamMethodAnalyzer.WithDiagnostics;

namespace Durian.DefaultParam
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
				bool isValid = AnalyzeAgaintsPartialOrExtern(diagnosticReceiver, symbol, declaration);
				isValid &= AnalyzeAgaintsProhibitedAttributes(diagnosticReceiver, symbol, compilation, out AttributeData[]? attributes);
				isValid &= AnalyzeContainingTypes(diagnosticReceiver, symbol, compilation, out ITypeData[]? containingTypes);

				if(isValid)
				{
					TypeParameterContainer combinedParameters = typeParameters;

					if (AnalyzeOverrideMethod(diagnosticReceiver, symbol, ref combinedParameters, compilation, cancellationToken) &&
						AnalyzeMethodSignature(diagnosticReceiver, symbol, in combinedParameters, compilation, out HashSet<int>? newModifiers, cancellationToken))
					{
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
							CheckShouldCallInsteadOfCopying(attributes!, compilation)
						);

						return true;
					}
				}

				data = null;
				return false;
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
		}
	}
}
