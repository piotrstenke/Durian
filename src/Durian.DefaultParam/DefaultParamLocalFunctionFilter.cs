// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Durian.Analysis.Cache;
using Durian.Analysis.Data;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.DefaultParam
{
	/// <summary>
	/// Reports <see cref="Diagnostic"/> s for <see cref="LocalFunctionStatementSyntax"/> with the
	/// <c>Durian.DefaultParamAttribute</c> applied.
	/// </summary>
	public partial class DefaultParamLocalFunctionFilter : IDefaultParamFilter<IDefaultParamTarget>, IDefaultParamFilter, INodeProvider<LocalFunctionStatementSyntax>
	{
		/// <inheritdoc/>
		public DefaultParamGenerator Generator { get; }

		/// <inheritdoc/>
		public IHintNameProvider HintNameProvider { get; }

		/// <inheritdoc/>
		public bool IncludeGeneratedSymbols { get; }

		/// <summary>
		/// <see cref="FilterMode"/> of this <see cref="DefaultParamDelegateFilter"/>.
		/// </summary>
		public FilterMode Mode => Generator.LoggingConfiguration.CurrentFilterMode;

		IDurianSourceGenerator IGeneratorSyntaxFilter.Generator => Generator;

		/// <inheritdoc cref="DefaultParamLocalFunctionFilter(DefaultParamGenerator, IHintNameProvider)"/>
		public DefaultParamLocalFunctionFilter(DefaultParamGenerator generator) : this(generator, new SymbolNameToFile())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamLocalFunctionFilter"/> class.
		/// </summary>
		/// <param name="generator"><see cref="DefaultParamGenerator"/> that created this filter.</param>
		/// <param name="hintNameProvider">
		/// <see cref="IHintNameProvider"/> that is used to create a hint name for the generated source.
		/// </param>
		public DefaultParamLocalFunctionFilter(DefaultParamGenerator generator, IHintNameProvider hintNameProvider)
		{
			Generator = generator;
			HintNameProvider = hintNameProvider;
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/> s for a single <see cref="LocalFunctionStatementSyntax"/>.
		/// </summary>
		/// <param name="diagnosticReceiver">
		/// <see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/> s.
		/// </param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="localFunction">
		/// <see cref="LocalFunctionStatementSyntax"/> to report <see cref="Diagnostic"/> for.
		/// </param>
		/// <param name="cancellationToken">
		/// <see cref="CancellationToken"/> that specifies if the operation should be canceled.
		/// </param>
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

			if (HasDefaultParamAttribute(localFunction, semanticModel, compilation.DefaultParamAttribute!))
			{
				ISymbol? symbol = semanticModel.GetDeclaredSymbol(localFunction, cancellationToken);

				if (symbol is not IMethodSymbol s)
				{
					return;
				}

				DefaultParamMethodAnalyzer.WithDiagnostics.ReportDiagnosticForInvalidMethodType(diagnosticReceiver, s);
			}
		}

		/// <summary>
		/// Reports <see cref="Diagnostic"/> s for <see cref="LocalFunctionStatementSyntax"/> es
		/// returned by the <see cref="GetCandidateLocalFunctions()"/> method.
		/// </summary>
		/// <param name="diagnosticReceiver">
		/// <see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/> s.
		/// </param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="collectedLocalFunctions">
		/// A collection of <see cref="LocalFunctionStatementSyntax"/> to report the <see
		/// cref="Diagnostic"/> s for.
		/// </param>
		/// <param name="cancellationToken">
		/// <see cref="CancellationToken"/> that specifies if the operation should be canceled.
		/// </param>
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
		/// Reports <see cref="Diagnostic"/> s for <see cref="LocalFunctionStatementSyntax"/> es
		/// returned by the <see cref="GetCandidateLocalFunctions()"/> method.
		/// </summary>
		/// <param name="diagnosticReceiver">
		/// <see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/> s.
		/// </param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="syntaxReceiver">
		/// <see cref="DefaultParamSyntaxReceiver"/> that collected the <see
		/// cref="LocalFunctionStatementSyntax"/> to report <see cref="Diagnostic"/> for.
		/// </param>
		/// <param name="cancellationToken">
		/// <see cref="CancellationToken"/> that specifies if the operation should be canceled.
		/// </param>
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
		/// Returns an array of <see cref="LocalFunctionStatementSyntax"/> s collected by the <see
		/// cref="Generator"/>'s <see cref="DefaultParamSyntaxReceiver"/> that can be filtrated by
		/// this filter.
		/// </summary>
		public LocalFunctionStatementSyntax[] GetCandidateLocalFunctions()
		{
			if (Mode == FilterMode.None)
			{
				return Array.Empty<LocalFunctionStatementSyntax>();
			}

			return Generator.SyntaxReceiver?.CandidateLocalFunctions?.ToArray() ?? Array.Empty<LocalFunctionStatementSyntax>();
		}

		private static bool HasDefaultParamAttribute(
			LocalFunctionStatementSyntax decl,
			SemanticModel semanticModel,
			INamedTypeSymbol attributeType
		)
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

		IEnumerable<IMemberData> IGeneratorSyntaxFilter.Filtrate(in GeneratorExecutionContext context)
		{
			if (Generator.SyntaxReceiver is not null &&
				Generator.TargetCompilation is not null &&
				Generator.SyntaxReceiver.CandidateLocalFunctions is not null &&
				Generator.SyntaxReceiver.CandidateLocalFunctions.Count > 0
			)
			{
				DefaultParamUtilities.IterateFilter(this);
			}

			return Array.Empty<IMemberData>();
		}

		IEnumerable<IMemberData> ISyntaxFilter.Filtrate(
			ICompilationData compilation,
			IDurianSyntaxReceiver syntaxReceiver,
			CancellationToken cancellationToken
		)
		{
			return Array.Empty<IMemberData>();
		}

		IEnumerable<IMemberData> ISyntaxFilter.Filtrate(
			ICompilationData compilation,
			IEnumerable<CSharpSyntaxNode> collectedNodes,
			CancellationToken cancellationToken
		)
		{
			return Array.Empty<IMemberData>();
		}

		IEnumerable<IMemberData> ISyntaxFilterWithDiagnostics.Filtrate(
			IDiagnosticReceiver diagnosticReceiver,
			ICompilationData compilation,
			IDurianSyntaxReceiver syntaxReceiver,
			CancellationToken cancellationToken
		)
		{
			ReportDiagnosticsForLocalFunctions(diagnosticReceiver, (DefaultParamCompilationData)compilation, (DefaultParamSyntaxReceiver)syntaxReceiver, cancellationToken);
			return Array.Empty<IMemberData>();
		}

		IEnumerable<IMemberData> ISyntaxFilterWithDiagnostics.Filtrate(
			IDiagnosticReceiver diagnosticReceiver,
			ICompilationData compilation,
			IEnumerable<CSharpSyntaxNode> collectedNodes,
			CancellationToken cancellationToken)
		{
			ReportDiagnosticsForLocalFunctions(diagnosticReceiver, (DefaultParamCompilationData)compilation, collectedNodes.OfType<LocalFunctionStatementSyntax>(), cancellationToken);
			return Array.Empty<IMemberData>();
		}

		IEnumerable<IMemberData> ICachedGeneratorSyntaxFilter<IDefaultParamTarget>.Filtrate(in CachedGeneratorExecutionContext<IDefaultParamTarget> context)
		{
			return Array.Empty<IMemberData>();
		}

		IEnumerator<IMemberData> IGeneratorSyntaxFilter.GetEnumerator()
		{
			return DefaultParamUtilities.GetFilterEnumerator(this);
		}

		IEnumerator<IMemberData> ICachedGeneratorSyntaxFilter<IDefaultParamTarget>.GetEnumerator(in CachedGeneratorExecutionContext<IDefaultParamTarget> context)
		{
			ref readonly CachedData<IDefaultParamTarget> cache = ref context.GetCachedData();
			return DefaultParamUtilities.GetFilterEnumerator(this, in cache);
		}

		IEnumerable<CSharpSyntaxNode> INodeProvider.GetNodes()
		{
			return GetCandidateLocalFunctions();
		}

		IEnumerable<LocalFunctionStatementSyntax> INodeProvider<LocalFunctionStatementSyntax>.GetNodes()
		{
			return GetCandidateLocalFunctions();
		}

		bool IDefaultParamFilter<IDefaultParamTarget>.GetValidationData(
			CSharpSyntaxNode node,
			DefaultParamCompilationData compilation,
			[NotNullWhen(true)] out SemanticModel? semanticModel,
			[NotNullWhen(true)] out ISymbol? symbol,
			out TypeParameterContainer typeParameters,
			CancellationToken cancellationToken
		)
		{
			semanticModel = null!;
			symbol = null!;
			typeParameters = default;
			return true;
		}

		bool INodeValidator<IDefaultParamTarget>.GetValidationData(
			CSharpSyntaxNode node,
			ICompilationData compilation,
			[NotNullWhen(true)] out SemanticModel? semanticModel,
			[NotNullWhen(true)] out ISymbol? symbol,
			CancellationToken cancellationToken
		)
		{
			semanticModel = null!;
			symbol = null!;
			return true;
		}

		bool IDefaultParamFilter<IDefaultParamTarget>.ValidateAndCreate(
			DefaultParamCompilationData compilation,
			CSharpSyntaxNode node,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken
		)
		{
			data = null;
			return false;
		}

		bool IDefaultParamFilter<IDefaultParamTarget>.ValidateAndCreate(
			CSharpSyntaxNode node,
			DefaultParamCompilationData compilation,
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

		bool INodeValidator<IDefaultParamTarget>.ValidateAndCreate(
			CSharpSyntaxNode node,
			ICompilationData compilation,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken
		)
		{
			data = null;
			return false;
		}

		bool INodeValidator<IDefaultParamTarget>.ValidateAndCreate(
			CSharpSyntaxNode node,
			ICompilationData compilation,
			SemanticModel semanticModel,
			ISymbol symbol,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken
		)
		{
			data = null;
			return false;
		}

		bool IDefaultParamFilter<IDefaultParamTarget>.ValidateAndCreateWithDiagnostics(
			IDiagnosticReceiver diagnosticReceiver,
			CSharpSyntaxNode node,
			DefaultParamCompilationData compilation,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken
		)
		{
			data = null;

			if (node is LocalFunctionStatementSyntax f)
			{
				ReportDiagnosticsForLocalFunction(diagnosticReceiver, compilation, f, cancellationToken);
			}

			return false;
		}

		bool IDefaultParamFilter<IDefaultParamTarget>.ValidateAndCreateWithDiagnostics(
			IDiagnosticReceiver diagnosticReceiver,
			CSharpSyntaxNode node,
			DefaultParamCompilationData compilation,
			SemanticModel semanticModel,
			ISymbol symbol,
			in TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken
		)
		{
			data = null;

			if (node is LocalFunctionStatementSyntax f)
			{
				ReportDiagnosticsForLocalFunction(diagnosticReceiver, compilation, f, cancellationToken);
			}

			return false;
		}

		bool INodeValidatorWithDiagnostics<IDefaultParamTarget>.ValidateAndCreateWithDiagnostics(
			IDiagnosticReceiver diagnosticReceiver,
			CSharpSyntaxNode node,
			ICompilationData compilation,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken
		)
		{
			data = null;

			if (node is LocalFunctionStatementSyntax f)
			{
				ReportDiagnosticsForLocalFunction(diagnosticReceiver, (DefaultParamCompilationData)compilation, f, cancellationToken);
			}

			return false;
		}

		bool INodeValidatorWithDiagnostics<IDefaultParamTarget>.ValidateAndCreateWithDiagnostics(
			IDiagnosticReceiver diagnosticReceiver,
			CSharpSyntaxNode node,
			ICompilationData compilation,
			SemanticModel semanticModel,
			ISymbol symbol,
			[NotNullWhen(true)] out IDefaultParamTarget? data,
			CancellationToken cancellationToken
		)
		{
			data = null;

			if (node is LocalFunctionStatementSyntax f)
			{
				ReportDiagnosticsForLocalFunction(diagnosticReceiver, (DefaultParamCompilationData)compilation, f, cancellationToken);
			}

			return false;
		}

		#endregion -Interface Implementations-
	}
}