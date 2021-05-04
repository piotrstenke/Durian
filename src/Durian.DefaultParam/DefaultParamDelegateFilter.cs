using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Durian.Data;
using Durian.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Durian.DefaultParam.DefaultParamAnalyzer;

namespace Durian.DefaultParam
{
	public partial class DefaultParamDelegateFilter : IDefaultParamFilter
	{
		public DefaultParamGenerator Generator { get; }
		public IFileNameProvider FileNameProvider { get; }
		public FilterMode Mode => Generator.LoggingConfiguration.CurrentFilterMode;
		public bool IncludeGeneratedSymbols { get; }
		IDurianSourceGenerator IGeneratorSyntaxFilter.Generator => Generator;

		public DefaultParamDelegateFilter(DefaultParamGenerator generator) : this(generator, new SymbolNameToFile())
		{
		}

		public DefaultParamDelegateFilter(DefaultParamGenerator generator, IFileNameProvider fileNameProvider)
		{
			Generator = generator;
			FileNameProvider = fileNameProvider;
		}

		public DelegateDeclarationSyntax[] GetCandidateDelegates()
		{
			return Generator.SyntaxReceiver?.CandidateDelegates?.ToArray() ?? Array.Empty<DelegateDeclarationSyntax>();
		}

		public DefaultParamDelegateData[] GetValidDelegates()
		{
			if (Generator.SyntaxReceiver is null || Generator.TargetCompilation is null || Generator.SyntaxReceiver.CandidateDelegates.Count == 0)
			{
				return Array.Empty<DefaultParamDelegateData>();
			}

			return DefaultParamUtilities.IterateFilter<DefaultParamDelegateData>(this);
		}

		public static bool GetValidationData(
			DefaultParamCompilationData compilation,
			DelegateDeclarationSyntax declaration,
			[NotNullWhen(true)] out SemanticModel? semanticModel,
			out TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out INamedTypeSymbol? symbol,
			CancellationToken cancellationToken
		)
		{
			semanticModel = compilation.Compilation.GetSemanticModel(declaration.SyntaxTree);
			typeParameters = GetParameters(declaration, semanticModel, compilation, cancellationToken);

			if (!typeParameters.HasDefaultParams)
			{
				symbol = null!;
				return false;
			}

			symbol = semanticModel.GetDeclaredSymbol(declaration, cancellationToken)!;

			return symbol is not null;
		}

		public static DefaultParamDelegateData[] GetValidDelegates(
			DefaultParamCompilationData compilation,
			IEnumerable<DelegateDeclarationSyntax> collectedDelegates,
			CancellationToken cancellationToken = default
		)
		{
			if (compilation is null || collectedDelegates is null || compilation is null)
			{
				return Array.Empty<DefaultParamDelegateData>();
			}

			DelegateDeclarationSyntax[] collected = collectedDelegates.ToArray();

			if (collected.Length == 0)
			{
				return Array.Empty<DefaultParamDelegateData>();
			}

			return GetValidDelegates_Internal(compilation, collected, cancellationToken);
		}

		public static DefaultParamDelegateData[] GetValidDelegates(
			DefaultParamCompilationData compilation,
			DefaultParamSyntaxReceiver syntaxReceiver,
			CancellationToken cancellationToken = default
		)
		{
			if (compilation is null || syntaxReceiver is null || syntaxReceiver.CandidateDelegates.Count == 0)
			{
				return Array.Empty<DefaultParamDelegateData>();
			}

			return GetValidDelegates_Internal(compilation, syntaxReceiver.CandidateDelegates.ToArray(), cancellationToken);
		}

		public static bool ValidateAndCreate(
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

			return ValidateAndCreate(compilation, declaration, semanticModel, symbol, ref typeParameters, out data);
		}

		private static bool ValidateAndCreate(
			DefaultParamCompilationData compilation,
			DelegateDeclarationSyntax declaration,
			SemanticModel semanticModel,
			INamedTypeSymbol symbol,
			ref TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out DefaultParamDelegateData? data
		)
		{
			if (AnalyzeAgaintsProhibitedAttributes(symbol, compilation, out AttributeData[]? attributes) &&
				AnalyzeContainingTypes(symbol, compilation, out ITypeData[]? containingTypes) &&
				AnalyzeTypeParameters(in typeParameters))
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

		private static TypeParameterContainer GetParameters(
			DelegateDeclarationSyntax declaration,
			SemanticModel semanticModel,
			DefaultParamCompilationData compilation,
			CancellationToken cancellationToken
		)
		{
			TypeParameterListSyntax? parameters = declaration.TypeParameterList;

			if (parameters is null)
			{
				return new TypeParameterContainer(null);
			}

			return new TypeParameterContainer(parameters.Parameters.Select(p => TypeParameterData.CreateFrom(p, semanticModel, compilation, cancellationToken)));
		}

		private static DefaultParamDelegateData[] GetValidDelegates_Internal(
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

				if (ValidateAndCreate(compilation, decl, out DefaultParamDelegateData? data, cancellationToken))
				{
					list.Add(data!);
				}
			}

			return list.ToArray();
		}

		#region -Interface Implementations-

		CSharpSyntaxNode[] IDefaultParamFilter.GetCandidateNodes()
		{
			return GetCandidateDelegates();
		}

		IEnumerator<IMemberData> IGeneratorSyntaxFilter.GetEnumerator()
		{
			return DefaultParamUtilities.GetFilterEnumerator(this);
		}

		IMemberData[] IGeneratorSyntaxFilter.Filtrate()
		{
			return GetValidDelegates();
		}

		IEnumerable<IMemberData> ISyntaxFilter.Filtrate(ICompilationData compilation, IDurianSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken)
		{
			return GetValidDelegates((DefaultParamCompilationData)compilation, (DefaultParamSyntaxReceiver)syntaxReceiver, cancellationToken);
		}

		IEnumerable<IMemberData> ISyntaxFilter.Filtrate(ICompilationData compilation, IEnumerable<CSharpSyntaxNode> collectedNodes, CancellationToken cancellationToken)
		{
			return GetValidDelegates((DefaultParamCompilationData)compilation, collectedNodes.OfType<DelegateDeclarationSyntax>(), cancellationToken);
		}

		IEnumerable<IMemberData> ISyntaxFilterWithDiagnostics.Filtrate(IDiagnosticReceiver diagnosticReceiver, ICompilationData compilation, IDurianSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken)
		{
			return WithDiagnostics.GetValidDelegates(diagnosticReceiver, (DefaultParamCompilationData)compilation, (DefaultParamSyntaxReceiver)syntaxReceiver, cancellationToken);
		}

		IEnumerable<IMemberData> ISyntaxFilterWithDiagnostics.Filtrate(IDiagnosticReceiver diagnosticReceiver, ICompilationData compilation, IEnumerable<CSharpSyntaxNode> collectedNodes, CancellationToken cancellationToken)
		{
			return WithDiagnostics.GetValidDelegates(diagnosticReceiver, (DefaultParamCompilationData)compilation, collectedNodes.OfType<DelegateDeclarationSyntax>(), cancellationToken);
		}

		bool IDefaultParamFilter.ValidateAndCreate(
			DefaultParamCompilationData compilation,
			CSharpSyntaxNode node,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken
		)
		{
			bool isValid = ValidateAndCreate(compilation, (DelegateDeclarationSyntax)node, out DefaultParamDelegateData? d, cancellationToken);
			data = d;
			return isValid;
		}

		bool IDefaultParamFilter.ValidateAndCreate(
			DefaultParamCompilationData compilation,
			CSharpSyntaxNode node,
			SemanticModel semanticModel,
			ISymbol symbol,
			ref TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken
		)
		{
			bool isValid = ValidateAndCreate(compilation, (DelegateDeclarationSyntax)node, semanticModel, (INamedTypeSymbol)symbol, ref typeParameters, out DefaultParamDelegateData? d);
			data = d;
			return isValid;
		}

		bool IDefaultParamFilter.ValidateAndCreateWithDiagnostics(
			IDiagnosticReceiver diagnosticReceiver,
			DefaultParamCompilationData compilation,
			CSharpSyntaxNode node,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken
		)
		{
			bool isValid = WithDiagnostics.ValidateAndCreate(diagnosticReceiver, compilation, (DelegateDeclarationSyntax)node, out DefaultParamDelegateData? d, cancellationToken);
			data = d;
			return isValid;
		}

		bool IDefaultParamFilter.ValidateAndCreateWithDiagnostics(
			IDiagnosticReceiver diagnosticReceiver,
			DefaultParamCompilationData compilation,
			CSharpSyntaxNode node,
			SemanticModel semanticModel,
			ISymbol symbol,
			ref TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken
		)
		{
			bool isValid = WithDiagnostics.ValidateAndCreate(diagnosticReceiver, compilation, (DelegateDeclarationSyntax)node, semanticModel, (INamedTypeSymbol)symbol, ref typeParameters, out DefaultParamDelegateData? d);
			data = d;
			return isValid;
		}

		bool IDefaultParamFilter.GetValidationData(
			DefaultParamCompilationData compilation,
			CSharpSyntaxNode node,
			[NotNullWhen(true)] out SemanticModel? semanticModel,
			out TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out ISymbol? symbol,
			CancellationToken cancellationToken
		)
		{
			bool isValid = GetValidationData(compilation, (DelegateDeclarationSyntax)node, out semanticModel, out typeParameters, out INamedTypeSymbol? s, cancellationToken);
			symbol = s;
			return isValid;
		}

		#endregion
	}
}
