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
	public partial class DefaultParamTypeFilter : IDefaultParamFilter
	{
		public DefaultParamGenerator Generator { get; }
		public IFileNameProvider FileNameProvider { get; }
		public FilterMode Mode => Generator.LoggingConfiguration.CurrentFilterMode;
		public bool IncludeGeneratedSymbols { get; }
		IDurianSourceGenerator IGeneratorSyntaxFilter.Generator => Generator;

		public DefaultParamTypeFilter(DefaultParamGenerator generator) : this(generator, new SymbolNameToFile())
		{
		}

		public DefaultParamTypeFilter(DefaultParamGenerator generator, IFileNameProvider fileNameProvider)
		{
			Generator = generator;
			FileNameProvider = fileNameProvider;
		}

		public TypeDeclarationSyntax[] GetCandidateTypes()
		{
			return Generator.SyntaxReceiver?.CandidateTypes?.ToArray() ?? Array.Empty<TypeDeclarationSyntax>();
		}

		public DefaultParamTypeData[] GetValidTypes()
		{
			if (Generator.SyntaxReceiver is null || Generator.TargetCompilation is null || Generator.SyntaxReceiver.CandidateTypes.Count == 0)
			{
				return Array.Empty<DefaultParamTypeData>();
			}

			return DefaultParamUtilities.IterateFilter<DefaultParamTypeData>(this);
		}

		public static DefaultParamTypeData[] GetValidTypes(
			DefaultParamCompilationData compilation,
			IEnumerable<TypeDeclarationSyntax> collectedTypes,
			CancellationToken cancellationToken = default
		)
		{
			if (compilation is null || collectedTypes is null || compilation is null)
			{
				return Array.Empty<DefaultParamTypeData>();
			}

			TypeDeclarationSyntax[] collected = collectedTypes.ToArray();

			if (collected.Length == 0)
			{
				return Array.Empty<DefaultParamTypeData>();
			}

			return GetValidTypes_Internal(compilation, collected, cancellationToken);
		}

		public static DefaultParamTypeData[] GetValidTypes(
			DefaultParamCompilationData compilation,
			DefaultParamSyntaxReceiver syntaxReceiver,
			CancellationToken cancellationToken = default
		)
		{
			if (compilation is null || syntaxReceiver is null || syntaxReceiver.CandidateTypes.Count == 0)
			{
				return Array.Empty<DefaultParamTypeData>();
			}

			return GetValidTypes_Internal(compilation, syntaxReceiver.CandidateTypes.ToArray(), cancellationToken);
		}

		public static bool ValidateAndCreate(
			DefaultParamCompilationData compilation,
			TypeDeclarationSyntax declaration,
			[NotNullWhen(true)] out DefaultParamTypeData? data,
			CancellationToken cancellationToken
		)
		{
			if (!GetValidationData(compilation, declaration, out SemanticModel? semanticModel, out TypeParameterContainer typeParameters, out INamedTypeSymbol? symbol, cancellationToken))
			{
				data = null;
				return false;
			}

			return ValidateAndCreate(compilation, declaration, semanticModel, symbol, ref typeParameters, out data);
		}

		public static bool ValidateAndCreate(
			DefaultParamCompilationData compilation,
			TypeDeclarationSyntax declaration,
			SemanticModel semanticModel,
			INamedTypeSymbol symbol,
			ref TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out DefaultParamTypeData? data
		)
		{
			if (AnalyzeAgainstGeneratedCodeAttribute(symbol, compilation, out AttributeData[]? attributes) &&
				AnalyzeContainingTypes(symbol, compilation, out ITypeData[]? containingTypes) &&
				AnalyzeTypeParameters(in typeParameters))
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

		public static bool GetValidationData(
			DefaultParamCompilationData compilation,
			TypeDeclarationSyntax declaration,
			[NotNullWhen(true)] out SemanticModel? semanticModel,
			out TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out INamedTypeSymbol? symbol,
			CancellationToken cancellationToken = default
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

		private static DefaultParamTypeData[] GetValidTypes_Internal(
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

				if (ValidateAndCreate(compilation, decl, out DefaultParamTypeData? data, cancellationToken))
				{
					list.Add(data!);
				}
			}

			return list.ToArray();
		}

		private static TypeParameterContainer GetParameters(
			TypeDeclarationSyntax declaration,
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

		#region -Interface Implementations

		CSharpSyntaxNode[] IDefaultParamFilter.GetCandidateNodes()
		{
			return GetCandidateTypes();
		}

		IEnumerator<IMemberData> IGeneratorSyntaxFilter.GetEnumerator()
		{
			return DefaultParamUtilities.GetFilterEnumerator(this);
		}

		IMemberData[] IGeneratorSyntaxFilter.Filtrate()
		{
			return GetValidTypes();
		}

		IEnumerable<IMemberData> ISyntaxFilter.Filtrate(ICompilationData compilation, IDurianSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken)
		{
			return GetValidTypes((DefaultParamCompilationData)compilation, (DefaultParamSyntaxReceiver)syntaxReceiver, cancellationToken);
		}

		IEnumerable<IMemberData> ISyntaxFilter.Filtrate(ICompilationData compilation, IEnumerable<CSharpSyntaxNode> collectedNodes, CancellationToken cancellationToken)
		{
			return GetValidTypes((DefaultParamCompilationData)compilation, collectedNodes.OfType<TypeDeclarationSyntax>(), cancellationToken);
		}

		IEnumerable<IMemberData> ISyntaxFilterWithDiagnostics.Filtrate(IDiagnosticReceiver diagnosticReceiver, ICompilationData compilation, IDurianSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken)
		{
			return WithDiagnostics.GetValidTypes(diagnosticReceiver, (DefaultParamCompilationData)compilation, (DefaultParamSyntaxReceiver)syntaxReceiver, cancellationToken);
		}

		IEnumerable<IMemberData> ISyntaxFilterWithDiagnostics.Filtrate(IDiagnosticReceiver diagnosticReceiver, ICompilationData compilation, IEnumerable<CSharpSyntaxNode> collectedNodes, CancellationToken cancellationToken)
		{
			return WithDiagnostics.GetValidTypes(diagnosticReceiver, (DefaultParamCompilationData)compilation, collectedNodes.OfType<TypeDeclarationSyntax>(), cancellationToken);
		}

		bool IDefaultParamFilter.ValidateAndCreate(
			DefaultParamCompilationData compilation,
			CSharpSyntaxNode node,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken
		)
		{
			bool isValid = ValidateAndCreate(compilation, (TypeDeclarationSyntax)node, out DefaultParamTypeData? d, cancellationToken);
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
			bool isValid = ValidateAndCreate(compilation, (TypeDeclarationSyntax)node, semanticModel, (INamedTypeSymbol)symbol, ref typeParameters, out DefaultParamTypeData? d);
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
			bool isValid = WithDiagnostics.ValidateAndCreate(diagnosticReceiver, compilation, (TypeDeclarationSyntax)node, out DefaultParamTypeData? d, cancellationToken);
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
			bool isValid = WithDiagnostics.ValidateAndCreate(diagnosticReceiver, compilation, (TypeDeclarationSyntax)node, semanticModel, (INamedTypeSymbol)symbol, ref typeParameters, out DefaultParamTypeData? d);
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
			bool isValid = GetValidationData(compilation, (TypeDeclarationSyntax)node, out semanticModel, out typeParameters, out INamedTypeSymbol? s, cancellationToken);
			symbol = s;
			return isValid;
		}
		#endregion
	}
}
