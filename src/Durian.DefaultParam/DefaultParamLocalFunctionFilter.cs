using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Durian.Generator.Data;
using Durian.Generator.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Generator.DefaultParam
{
	/// <summary>
	/// Reports <see cref="Diagnostic"/>s for <see cref="LocalFunctionStatementSyntax"/> with the <see cref="DefaultParamAttribute"/> applied.
	/// </summary>
	public class DefaultParamLocalFunctionFilter : IDefaultParamFilter
	{
		/// <inheritdoc/>
		public DefaultParamGenerator Generator { get; }

		/// <inheritdoc/>
		public IFileNameProvider FileNameProvider { get; }

		/// <summary>
		/// <see cref="FilterMode"/> of this <see cref="DefaultParamDelegateFilter"/>.
		/// </summary>
		public FilterMode Mode => Generator.LoggingConfiguration.CurrentFilterMode;

		/// <inheritdoc/>
		public bool IncludeGeneratedSymbols { get; }
		IDurianSourceGenerator IGeneratorSyntaxFilter.Generator => Generator;

		/// <inheritdoc cref="DefaultParamLocalFunctionFilter(DefaultParamGenerator, IFileNameProvider)"/>
		public DefaultParamLocalFunctionFilter(DefaultParamGenerator generator) : this(generator, new SymbolNameToFile())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamLocalFunctionFilter"/> class.
		/// </summary>
		/// <param name="generator"><see cref="DefaultParamGenerator"/> that created this filter.</param>
		/// <param name="fileNameProvider"><see cref="IFileNameProvider"/> that is used to create a hint name for the generated source.</param>
		public DefaultParamLocalFunctionFilter(DefaultParamGenerator generator, IFileNameProvider fileNameProvider)
		{
			Generator = generator;
			FileNameProvider = fileNameProvider;
		}

		/// <summary>
		/// Returns an array of <see cref="LocalFunctionStatementSyntax"/>s collected by the <see cref="Generator"/>'s <see cref="DefaultParamSyntaxReceiver"/> that can be filtrated by this filter.
		/// </summary>
		public LocalFunctionStatementSyntax[] GetCandidateLocalFunctions()
		{
			if (Mode == FilterMode.None)
			{
				return Array.Empty<LocalFunctionStatementSyntax>();
			}

			return Generator.SyntaxReceiver?.CandidateLocalFunctions?.ToArray() ?? Array.Empty<LocalFunctionStatementSyntax>();
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s for <see cref="LocalFunctionStatementSyntax"/> returned by the <see cref="GetCandidateLocalFunctions()"/> method.
		/// </summary>
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

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s for <see cref="LocalFunctionStatementSyntax"/>es returned by the <see cref="GetCandidateLocalFunctions()"/> method.
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="collectedLocalFunctions">A collection of <see cref="LocalFunctionStatementSyntax"/> to report the <see cref="Diagnostic"/>s for.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
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

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s for <see cref="LocalFunctionStatementSyntax"/>es returned by the <see cref="GetCandidateLocalFunctions()"/> method.
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="syntaxReceiver"><see cref="DefaultParamSyntaxReceiver"/> that collected the <see cref="LocalFunctionStatementSyntax"/> to report <see cref="Diagnostic"/> for.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
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

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s for a single <see cref="LocalFunctionStatementSyntax"/>.
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="localFunction"><see cref="LocalFunctionStatementSyntax"/> to report <see cref="Diagnostic"/> for.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
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

			if (HasDefaultParamAttribute(localFunction, semanticModel, compilation.MainAttribute!))
			{
				ISymbol? symbol = semanticModel.GetDeclaredSymbol(localFunction, cancellationToken);

				if (symbol is not IMethodSymbol s)
				{
					return;
				}

				DefaultParamMethodAnalyzer.WithDiagnostics.ReportDiagnosticForLocalFunction(diagnosticReceiver, s);
			}
		}

		private static bool HasDefaultParamAttribute(LocalFunctionStatementSyntax decl, SemanticModel semanticModel, INamedTypeSymbol attributeType)
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
					return info.Symbol is IMethodSymbol m && SymbolEqualityComparer.Default.Equals(m.ContainingType, attributeType);
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
			in TypeParameterContainer typeParameters,
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
			in TypeParameterContainer typeParameters,
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
			[NotNullWhen(true)] out ISymbol? symbol,
			out TypeParameterContainer typeParameters,
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
