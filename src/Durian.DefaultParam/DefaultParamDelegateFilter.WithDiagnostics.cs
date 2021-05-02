using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Durian.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Durian.DefaultParam.DefaultParamAnalyzer;

namespace Durian.DefaultParam
{
	public partial class DefaultParamDelegateFilter
	{
		public static class WithDiagnostics
		{
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

			public static bool ValidateAndCreate(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				DelegateDeclarationSyntax declaration,
				[NotNullWhen(true)] out DefaultParamDelegateData? data,
				CancellationToken cancellationToken = default
			)
			{
				if (!GetValidationData(compilation, declaration, out SemanticModel? semanticModel, out TypeParameterContainer typeParameters, out INamedTypeSymbol? symbol, cancellationToken))
				{
					data = null;
					return false;
				}

				return ValidateAndCreate(diagnosticReceiver, compilation, declaration, semanticModel, symbol, ref typeParameters, out data);
			}

			public static bool ValidateAndCreate(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				DelegateDeclarationSyntax declaration,
				SemanticModel semanticModel,
				INamedTypeSymbol symbol,
				ref TypeParameterContainer typeParameters,
				[NotNullWhen(true)] out DefaultParamDelegateData? data
			)
			{
				bool isValid = AnalyzeAgainstGeneratedCodeAttributeWithDiagnostics(diagnosticReceiver, symbol, compilation, out AttributeData[]? attributes);
				isValid &= AnalyzeContainingTypesWithDiagnostics(diagnosticReceiver, symbol, compilation, out ITypeData[]? containingTypes);
				isValid &= AnalyzeTypeParametersWithDiagnostics(diagnosticReceiver, in typeParameters);

				if (isValid)
				{
					data = new DefaultParamDelegateData(
						declaration,
						compilation,
						symbol,
						semanticModel,
						containingTypes,
						null,
						attributes,
						typeParameters
					);

					return true;
				}

				data = null;
				return false;
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
		}
	}
}
