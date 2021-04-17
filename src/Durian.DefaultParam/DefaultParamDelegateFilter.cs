using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Durian.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Durian.DefaultParam.DefaultParamAnalyzer;

namespace Durian.DefaultParam
{
	public partial class DefaultParamDelegateFilter : IDefaultParamFilter
	{
		private readonly Wrapper _wrapper;

		public DefaultParamDelegateFilter()
		{
			_wrapper = new();
		}

		public static DefaultParamDelegateData[] GetValidDelegates(DefaultParamCompilationData compilation, DefaultParamSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken = default)
		{
			if (syntaxReceiver is null)
			{
				return Array.Empty<DefaultParamDelegateData>();
			}

			return GetValidDelegates(compilation, syntaxReceiver.CandidateDelegates, cancellationToken);
		}

		public static DefaultParamDelegateData[] GetValidDelegates(DefaultParamCompilationData compilation, IEnumerable<DelegateDeclarationSyntax> collectedDelegates, CancellationToken cancellationToken = default)
		{
			if (collectedDelegates is null || compilation is null)
			{
				return Array.Empty<DefaultParamDelegateData>();
			}

			DelegateDeclarationSyntax[] collected = collectedDelegates.ToArray();
			List<DefaultParamDelegateData> list = new(collected.Length);

			foreach (DelegateDeclarationSyntax decl in collected)
			{
				if (decl is null)
				{
					continue;
				}

				if (ValidateAndCreateWithoutDiagnostics(compilation, decl, out DefaultParamDelegateData? data, cancellationToken))
				{
					list.Add(data!);
				}
			}

			return list.ToArray();
		}

		public static DefaultParamDelegateData[] GetValidDelegates(
			IDiagnosticReceiver diagnosticReceiver,
			DefaultParamCompilationData compilation,
			DefaultParamSyntaxReceiver syntaxReceiver,
			CancellationToken cancellationToken = default
		)
		{
			if (syntaxReceiver is null)
			{
				return Array.Empty<DefaultParamDelegateData>();
			}

			return GetValidDelegates(diagnosticReceiver, compilation, syntaxReceiver.CandidateDelegates, cancellationToken);
		}

		public static DefaultParamDelegateData[] GetValidDelegates(
			IDiagnosticReceiver diagnosticReceiver,
			DefaultParamCompilationData compilation,
			IEnumerable<DelegateDeclarationSyntax> collectedDelegates,
			CancellationToken cancellationToken = default
		)
		{
			if (collectedDelegates is null || compilation is null)
			{
				return Array.Empty<DefaultParamDelegateData>();
			}

			DelegateDeclarationSyntax[] collected = collectedDelegates.ToArray();
			List<DefaultParamDelegateData> list = new(collected.Length);

			foreach (DelegateDeclarationSyntax decl in collected)
			{
				if (decl is null)
				{
					continue;
				}

				if (ValidateAndCreateWithDiagnostics(diagnosticReceiver, compilation, decl, out DefaultParamDelegateData? data, cancellationToken))
				{
					list.Add(data!);
				}
			}

			return list.ToArray();
		}

		public Wrapper GetWrapper(DefaultParamDelegateData target, CancellationToken cancellationToken = default)
		{
			_wrapper.SetDataAndRemoveDefaultParamAttribute(target, cancellationToken);

			return _wrapper;
		}

		IDefaultParamTargetWrapper IDefaultParamFilter.GetWrapper(IDefaultParamTarget target, CancellationToken cancellationToken)
		{
			return GetWrapper((DefaultParamDelegateData)target, cancellationToken);
		}

		IEnumerable<IMemberData> ISyntaxFilter.Filtrate(ICompilationData compilation, IDurianSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken)
		{
			return GetValidDelegates((DefaultParamCompilationData)compilation, (DefaultParamSyntaxReceiver)syntaxReceiver, cancellationToken);
		}

		IEnumerable<IMemberData> ISyntaxFilter.Filtrate(ICompilationData compilation, IEnumerable<CSharpSyntaxNode> collectedNodes, CancellationToken cancellationToken)
		{
			return GetValidDelegates((DefaultParamCompilationData)compilation, collectedNodes.Cast<DelegateDeclarationSyntax>(), cancellationToken);
		}

		IEnumerable<IMemberData> ISyntaxFilterWithDiagnostics.Filtrate(IDiagnosticReceiver diagnosticReceiver, ICompilationData compilation, IDurianSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken)
		{
			return GetValidDelegates(diagnosticReceiver, (DefaultParamCompilationData)compilation, (DefaultParamSyntaxReceiver)syntaxReceiver, cancellationToken);
		}

		IEnumerable<IMemberData> ISyntaxFilterWithDiagnostics.Filtrate(IDiagnosticReceiver diagnosticReceiver, ICompilationData compilation, IEnumerable<CSharpSyntaxNode> collectedNodes, CancellationToken cancellationToken)
		{
			return GetValidDelegates(diagnosticReceiver, (DefaultParamCompilationData)compilation, collectedNodes.Cast<DelegateDeclarationSyntax>(), cancellationToken);
		}

		private static bool ValidateAndCreateWithoutDiagnostics(
			DefaultParamCompilationData compilation,
			DelegateDeclarationSyntax declaration,
			out DefaultParamDelegateData? data,
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

		private static bool ValidateAndCreateWithDiagnostics(
			IDiagnosticReceiver diagnosticReceiver,
			DefaultParamCompilationData compilation,
			DelegateDeclarationSyntax declaration,
			out DefaultParamDelegateData? data,
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

		private static TypeParameterContainer GetParameters(DelegateDeclarationSyntax declaration, SemanticModel semanticModel, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
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
