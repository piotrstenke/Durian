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

namespace Durian.DefaultParam
{
	public class DefaultParamLocalFunctionFilter : IDefaultParamFilter
	{
		public DefaultParamGenerator Generator { get; }
		public IFileNameProvider FileNameProvider { get; }
		public FilterMode Mode => Generator.LoggingConfiguration.CurrentFilterMode;
		public bool IncludeGeneratedSymbols { get; }
		IDurianSourceGenerator IGeneratorSyntaxFilter.Generator => Generator;

		public DefaultParamLocalFunctionFilter(DefaultParamGenerator generator) : this(generator, new SymbolNameToFile())
		{
		}

		public DefaultParamLocalFunctionFilter(DefaultParamGenerator generator, IFileNameProvider fileNameProvider)
		{
			Generator = generator;
			FileNameProvider = fileNameProvider;
		}

		public LocalFunctionStatementSyntax[] GetCandidateLocalFunctions()
		{
			if (Mode == FilterMode.None)
			{
				return Array.Empty<LocalFunctionStatementSyntax>();
			}

			return Generator.SyntaxReceiver?.CandidateLocalFunctions?.ToArray() ?? Array.Empty<LocalFunctionStatementSyntax>();
		}

		public void ReportDiagnosticsForLocalFunctions()
		{
			if (Generator.SyntaxReceiver is null ||
				Generator.TargetCompilation is null ||
				Generator.SyntaxReceiver.CandidateLocalFunctions is null ||
				Generator.SyntaxReceiver.CandidateLocalFunctions.Count == 0
			)
			{
				return;
			}

			DefaultParamUtilities.IterateFilter<IDefaultParamTarget>(this);
		}

		public static void ReportDiagnosticsForLocalFunctions(
			IDiagnosticReceiver diagnosticReceiver,
			DefaultParamCompilationData compilation,
			IEnumerable<LocalFunctionStatementSyntax> collectedLocalFunctions,
			CancellationToken cancellationToken = default
		)
		{
			if (collectedLocalFunctions is null || compilation is null || diagnosticReceiver is null)
			{
				return;
			}

			foreach (LocalFunctionStatementSyntax fn in collectedLocalFunctions)
			{
				ReportDiagnosticsForLocalFunction(diagnosticReceiver, compilation, fn, cancellationToken);
			}
		}

		public static void ReportDiagnosticsForLocalFunctions(
			IDiagnosticReceiver diagnosticReceiver,
			DefaultParamCompilationData compilation,
			DefaultParamSyntaxReceiver syntaxReceiver,
			CancellationToken cancellationToken = default
		)
		{
			if (diagnosticReceiver is null || compilation is null || syntaxReceiver is null || syntaxReceiver.CandidateLocalFunctions is null || syntaxReceiver.CandidateLocalFunctions.Count == 0)
			{
				return;
			}

			foreach (LocalFunctionStatementSyntax fn in syntaxReceiver.CandidateLocalFunctions)
			{
				ReportDiagnosticsForLocalFunction(diagnosticReceiver, compilation, fn, cancellationToken);
			}
		}

		public static void ReportDiagnosticsForLocalFunction(
			IDiagnosticReceiver diagnosticReceiver,
			DefaultParamCompilationData compilation,
			LocalFunctionStatementSyntax localFunction,
			CancellationToken cancellationToken = default
		)
		{
			if (localFunction is null)
			{
				return;
			}

			SemanticModel semanticModel = compilation.Compilation.GetSemanticModel(localFunction.SyntaxTree);

			if (HasDefaultParamAttrbiute(localFunction, semanticModel, compilation.AttributeConstructor!))
			{
				ISymbol? symbol = semanticModel.GetDeclaredSymbol(localFunction, cancellationToken);

				if (symbol is not IMethodSymbol s)
				{
					return;
				}

				DefaultParamMethodAnalyzer.WithDiagnostics.ReportDiagnosticForLocalFunction(diagnosticReceiver, s);
			}
		}

		private static bool HasDefaultParamAttrbiute(LocalFunctionStatementSyntax decl, SemanticModel semanticModel, IMethodSymbol attrCtor)
		{
			if (decl.TypeParameterList is null)
			{
				return false;
			}

			return decl.TypeParameterList.Parameters
				.SelectMany(p => p.AttributeLists)
				.SelectMany(attr => attr.Attributes)
				.Any(attr =>
				{
					SymbolInfo info = semanticModel.GetSymbolInfo(attr);
					return info.Symbol is IMethodSymbol m && SymbolEqualityComparer.Default.Equals(m, attrCtor);
				});
		}

		#region -Interface Implementations-

		IEnumerator<IMemberData> IGeneratorSyntaxFilter.GetEnumerator()
		{
			return DefaultParamUtilities.GetFilterEnumerator(this);
		}

		IMemberData[] IGeneratorSyntaxFilter.Filtrate()
		{
			ReportDiagnosticsForLocalFunctions();
			return Array.Empty<IMemberData>();
		}

		IEnumerable<IMemberData> ISyntaxFilter.Filtrate(ICompilationData compilation, IDurianSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken)
		{
			return Array.Empty<IMemberData>();
		}

		IEnumerable<IMemberData> ISyntaxFilter.Filtrate(ICompilationData compilation, IEnumerable<CSharpSyntaxNode> collectedNodes, CancellationToken cancellationToken)
		{
			return Array.Empty<IMemberData>();
		}

		IEnumerable<IMemberData> ISyntaxFilterWithDiagnostics.Filtrate(IDiagnosticReceiver diagnosticReceiver, ICompilationData compilation, IDurianSyntaxReceiver syntaxReceiver, CancellationToken cancellationToken)
		{
			ReportDiagnosticsForLocalFunctions(diagnosticReceiver, (DefaultParamCompilationData)compilation, (DefaultParamSyntaxReceiver)syntaxReceiver, cancellationToken);
			return Array.Empty<IMemberData>();
		}

		IEnumerable<IMemberData> ISyntaxFilterWithDiagnostics.Filtrate(IDiagnosticReceiver diagnosticReceiver, ICompilationData compilation, IEnumerable<CSharpSyntaxNode> collectedNodes, CancellationToken cancellationToken)
		{
			ReportDiagnosticsForLocalFunctions(diagnosticReceiver, (DefaultParamCompilationData)compilation, collectedNodes.OfType<LocalFunctionStatementSyntax>(), cancellationToken);
			return Array.Empty<IMemberData>();
		}

		CSharpSyntaxNode[] IDefaultParamFilter.GetCandidateNodes()
		{
			return GetCandidateLocalFunctions();
		}

		bool IDefaultParamFilter.ValidateAndCreate(
			DefaultParamCompilationData compilation,
			CSharpSyntaxNode node,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken
		)
		{
			data = null;
			return false;
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
			data = null;
			return false;
		}

		bool IDefaultParamFilter.ValidateAndCreateWithDiagnostics(
			IDiagnosticReceiver diagnosticReceiver,
			DefaultParamCompilationData compilation,
			CSharpSyntaxNode node,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken
		)
		{
			ReportDiagnosticsForLocalFunction(diagnosticReceiver, compilation, (LocalFunctionStatementSyntax)node, cancellationToken);

			data = null;
			return false;
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
			ReportDiagnosticsForLocalFunction(diagnosticReceiver, compilation, (LocalFunctionStatementSyntax)node, cancellationToken);

			data = null;
			return false;
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
			typeParameters = default;
			semanticModel = null!;
			symbol = null!;
			return true;
		}

		#endregion
	}
}
