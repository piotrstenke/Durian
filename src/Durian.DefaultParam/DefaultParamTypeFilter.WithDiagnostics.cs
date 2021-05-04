using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Durian.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Durian.DefaultParam.DefaultParamAnalyzer.WithDiagnostics;

namespace Durian.DefaultParam
{
	public partial class DefaultParamTypeFilter
	{
		public static class WithDiagnostics
		{
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

			public static bool ValidateAndCreate(
				IDiagnosticReceiver diagnosticReceiver,
				DefaultParamCompilationData compilation,
				TypeDeclarationSyntax declaration,
				[NotNullWhen(true)] out DefaultParamTypeData? data,
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
				TypeDeclarationSyntax declaration,
				SemanticModel semanticModel,
				INamedTypeSymbol symbol,
				ref TypeParameterContainer typeParameters,
				[NotNullWhen(true)] out DefaultParamTypeData? data
			)
			{
				bool isValid = AnalyzeAgaintsProhibitedAttributes(diagnosticReceiver, symbol, compilation, out AttributeData[]? attributes);
				isValid &= AnalyzeContainingTypes(diagnosticReceiver, symbol, compilation, out ITypeData[]? containingTypes);
				isValid &= AnalyzeTypeParameters(diagnosticReceiver, in typeParameters);

				if (isValid)
				{
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
						typeParameters
					);

					return true;
				}

				data = null;
				return false;
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
		}
	}
}
