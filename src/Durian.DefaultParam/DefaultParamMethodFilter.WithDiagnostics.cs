using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Durian.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Durian.DefaultParam.DefaultParamAnalyzer;
using static Durian.DefaultParam.DefaultParamMethodAnalyzer;

namespace Durian.DefaultParam
{
	public partial class DefaultParamMethodFilter
	{
		public static class WithDiagnostics
		{
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

			public static bool ValidateAndCreate(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				MethodDeclarationSyntax declaration,
				[NotNullWhen(true)] out DefaultParamMethodData? data,
				CancellationToken cancellationToken = default
			)
			{
				if (!GetValidationData(compilation, declaration, out SemanticModel? semanticModel, out TypeParameterContainer typeParameters, out IMethodSymbol? symbol, cancellationToken))
				{
					data = null;
					return false;
				}

				return ValidateAndCreate(diagnosticReceiver, compilation, declaration, semanticModel, symbol, ref typeParameters, out data, cancellationToken);
			}

			public static bool ValidateAndCreate(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				MethodDeclarationSyntax declaration,
				SemanticModel semanticModel,
				IMethodSymbol symbol,
				ref TypeParameterContainer typeParameters,
				[NotNullWhen(true)] out DefaultParamMethodData? data,
				CancellationToken cancellationToken = default
			)
			{
				bool isValid = AnalyzeAgaintsPartialOrExternWithDiagnostics(diagnosticReceiver, symbol, declaration);
				isValid &= AnalyzeAgainstGeneratedCodeAttributeWithDiagnostics(diagnosticReceiver, symbol, compilation, out AttributeData[]? attributes);
				isValid &= AnalyzeContainingTypesWithDiagnostics(diagnosticReceiver, symbol, compilation, out ITypeData[]? containingTypes);

				bool hasValidTypeParameters;

				if (IsOverride(symbol, out IMethodSymbol? baseMethod))
				{
					isValid &= AnalyzeOverrideMethodWithDiagnostics(diagnosticReceiver, symbol, baseMethod, ref typeParameters, compilation, cancellationToken, out hasValidTypeParameters);
				}
				else
				{
					hasValidTypeParameters = AnalyzeTypeParametersWithDiagnostics(diagnosticReceiver, in typeParameters);
					isValid &= hasValidTypeParameters;
				}

				if (hasValidTypeParameters)
				{
					isValid &= AnalyzeMethodSignatureWithDiagnostics(diagnosticReceiver, symbol, typeParameters, compilation, out HashSet<int>? newModifiers, cancellationToken);

					if (isValid)
					{
						data = new(
							declaration,
							compilation,
							symbol,
							semanticModel,
							containingTypes,
							null,
							attributes,
							in typeParameters,
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
