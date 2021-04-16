using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Durian.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Durian.DefaultParam.DefaultParamAnalyzer;

namespace Durian.DefaultParam
{
	public partial class DefaultParamTypeFilter : IDefaultParamFilter
	{
		private readonly Wrapper _wrapper;

		public DefaultParamTypeFilter()
		{
			_wrapper = new();
		}

		public static DefaultParamTypeData[] GetValidTypes(DefaultParamCompilationData compilation, IEnumerable<TypeDeclarationSyntax> collectedTypes, CancellationToken cancellationToken = default)
		{
			if (collectedTypes is null || compilation is null)
			{
				return Array.Empty<DefaultParamTypeData>();
			}

			TypeDeclarationSyntax[] collected = collectedTypes.ToArray();
			List<DefaultParamTypeData> list = new(collected.Length);

			foreach (TypeDeclarationSyntax decl in collected)
			{
				if (decl is null)
				{
					continue;
				}

				if (ValidateAndCreateWithoutDiagnostics(compilation, decl, out DefaultParamTypeData? data, cancellationToken))
				{
					list.Add(data!);
				}
			}

			return list.ToArray();
		}

		public static DefaultParamTypeData[] GetValidTypes(DefaultParamCompilationData compilation, DefaultParamSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken = default)
		{
			if (syntaxReceiver is null)
			{
				return Array.Empty<DefaultParamTypeData>();
			}

			return GetValidTypes(compilation, syntaxReceiver.CandidateTypes, cancellationToken);
		}

		public static DefaultParamTypeData[] GetValidTypes(
			IDiagnosticReceiver diagnosticReceiver,
			DefaultParamCompilationData compilation,
			DefaultParamSyntaxReceiver syntaxReceiver,
			CancellationToken cancellationToken = default
		)
		{
			if (syntaxReceiver is null)
			{
				return Array.Empty<DefaultParamTypeData>();
			}

			return GetValidTypes(diagnosticReceiver, compilation, syntaxReceiver.CandidateTypes, cancellationToken);
		}

		public static DefaultParamTypeData[] GetValidTypes(
			IDiagnosticReceiver diagnosticReceiver,
			DefaultParamCompilationData compilation,
			IEnumerable<TypeDeclarationSyntax> collectedTypes,
			CancellationToken cancellationToken = default
		)
		{
			if (collectedTypes is null || compilation is null)
			{
				return Array.Empty<DefaultParamTypeData>();
			}

			TypeDeclarationSyntax[] collected = collectedTypes.ToArray();
			List<DefaultParamTypeData> list = new(collected.Length);

			foreach (TypeDeclarationSyntax decl in collected)
			{
				if (decl is null)
				{
					continue;
				}

				if (ValidateAndCreateWithDiagnostics(diagnosticReceiver, compilation, decl, out DefaultParamTypeData? data, cancellationToken))
				{
					list.Add(data!);
				}
			}

			return list.ToArray();
		}

		public Wrapper GetWrapper(DefaultParamTypeData target, CancellationToken cancellationToken = default)
		{
			_wrapper.SetDataAndRemoveDefaultParamAttribute(target, cancellationToken);

			return _wrapper;
		}

		IDefaultParamTargetWrapper IDefaultParamFilter.GetWrapper(IDefaultParamTarget target, CancellationToken cancellationToken)
		{
			return GetWrapper((DefaultParamTypeData)target, cancellationToken);
		}

		IEnumerable<IMemberData> ISyntaxFilter.Filtrate(ICompilationData compilation, IDurianSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken)
		{
			return GetValidTypes((DefaultParamCompilationData)compilation, (DefaultParamSyntaxReceiver)syntaxReceiver, cancellationToken);
		}

		IEnumerable<IMemberData> ISyntaxFilter.Filtrate(ICompilationData compilation, IEnumerable<CSharpSyntaxNode> collectedNodes, CancellationToken cancellationToken)
		{
			return GetValidTypes((DefaultParamCompilationData)compilation, collectedNodes.Cast<TypeDeclarationSyntax>(), cancellationToken);
		}

		IEnumerable<IMemberData> ISyntaxFilterWithDiagnostics.Filtrate(IDiagnosticReceiver diagnosticReceiver, ICompilationData compilation, IDurianSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken)
		{
			return GetValidTypes(diagnosticReceiver, (DefaultParamCompilationData)compilation, (DefaultParamSyntaxReceiver)syntaxReceiver, cancellationToken);
		}

		IEnumerable<IMemberData> ISyntaxFilterWithDiagnostics.Filtrate(IDiagnosticReceiver diagnosticReceiver, ICompilationData compilation, IEnumerable<CSharpSyntaxNode> collectedNodes, CancellationToken cancellationToken)
		{
			return GetValidTypes(diagnosticReceiver, (DefaultParamCompilationData)compilation, collectedNodes.Cast<TypeDeclarationSyntax>(), cancellationToken);
		}

		private static bool ValidateAndCreateWithoutDiagnostics(
			DefaultParamCompilationData compilation,
			TypeDeclarationSyntax declaration,
			out DefaultParamTypeData? data,
			CancellationToken cancellationToken
		)
		{
			SemanticModel semanticModel = compilation.Compilation.GetSemanticModel(declaration.SyntaxTree);
			TypeParameterContainer typeParameters = GetParameters(declaration, semanticModel, compilation, cancellationToken);

			if (!typeParameters.HasDefaultParams)
			{
				data = null;
				return false;
			}

			INamedTypeSymbol? symbol = semanticModel.GetDeclaredSymbol(declaration, cancellationToken);

			if (symbol is null)
			{
				data = null;
				return false;
			}

			if (ValidateHasGeneratedCodeAttribute(symbol, compilation, out AttributeData[]? attributes) &&
				ValidateContainingTypes(symbol, compilation, out ITypeData[]? containingTypes) &&
				ValidateTypeParameters(in typeParameters))
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

		private static bool ValidateAndCreateWithDiagnostics(
			IDiagnosticReceiver diagnosticReceiver,
			DefaultParamCompilationData compilation,
			TypeDeclarationSyntax declaration,
			out DefaultParamTypeData? data,
			CancellationToken cancellationToken
		)
		{
			SemanticModel semanticModel = compilation.Compilation.GetSemanticModel(declaration.SyntaxTree);
			TypeParameterContainer typeParameters = GetParameters(declaration, semanticModel, compilation, cancellationToken);

			if (!typeParameters.HasDefaultParams)
			{
				data = null;
				return false;
			}

			INamedTypeSymbol? symbol = semanticModel.GetDeclaredSymbol(declaration, cancellationToken);

			if (symbol is null)
			{
				data = null;
				return false;
			}

			bool isValid = ValidateHasGeneratedCodeAttribute(diagnosticReceiver, symbol, compilation, out AttributeData[]? attributes);
			isValid &= ValidateContainingTypes(diagnosticReceiver, symbol, compilation, out ITypeData[]? containingTypes);
			isValid &= ValidateTypeParameters(diagnosticReceiver, in typeParameters);

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

		private static TypeParameterContainer GetParameters(TypeDeclarationSyntax declaration, SemanticModel semanticModel, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			TypeParameterListSyntax? parameters = declaration.TypeParameterList;

			if (parameters is null)
			{
				return new TypeParameterContainer(null);
			}

			return new TypeParameterContainer(parameters.Parameters.Select(p => TypeParameterData.CreateFrom(p, semanticModel, compilation, cancellationToken)));
		}
	}
}
