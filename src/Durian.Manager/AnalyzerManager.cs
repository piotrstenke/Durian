// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Durian.Analysis;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Manager
{
	/// <summary>
	/// Defines a set of analyzers to execute.
	/// </summary>
	public abstract class AnalyzerManager : DiagnosticAnalyzer, IDurianAnalyzer
	{
		/// <inheritdoc/>
		public virtual bool AllowGenerated => false;

		/// <inheritdoc/>
		public virtual bool Concurrent => true;

		/// <inheritdoc/>
		public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ((IDurianAnalyzer)this).GetSupportedDiagnostics().ToImmutableArray();

		/// <summary>
		/// Determines which Durian modules have to be enabled for this manager to be included in code analysis.
		/// </summary>
		public abstract ImmutableArray<DurianModule> RequiredModules { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="AnalyzerManager"/> class.
		/// </summary>
		protected AnalyzerManager()
		{
		}

		/// <inheritdoc/>
		public sealed override void Initialize(AnalysisContext context)
		{
			if (Concurrent)
			{
				context.EnableConcurrentExecution();
			}

			if (AllowGenerated)
			{
				context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.ReportDiagnostics | GeneratedCodeAnalysisFlags.Analyze);
			}
			else
			{
				context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			}

			context.RegisterCompilationStartAction(OnCompilationStart);
		}

		/// <summary>
		/// Returns a collection of child <see cref="IDurianAnalyzer"/>s to be executed.
		/// </summary>
		public abstract IEnumerable<IDurianAnalyzer> GetAnalyzers();

		/// <summary>
		/// Registers actions to be performed by this <see cref="AnalyzerManager"/>.
		/// </summary>
		/// <param name="context"><see cref="IDurianAnalysisContext"/> to register the actions to.</param>
		/// <param name="compilation">Current <see cref="Compilation"/>.</param>
		public virtual void Register(IDurianAnalysisContext context, CSharpCompilation compilation)
		{
			foreach (IDurianAnalyzer analyzer in GetAnalyzers())
			{
				analyzer.Register(context, compilation);
			}
		}

		/// <summary>
		/// Determines whether the specified <see cref="CSharpCompilation"/> is valid for analysis.
		/// </summary>
		/// <param name="compilation">Current <see cref="CSharpCompilation"/>.</param>
		public virtual bool ShouldAnalyze(CSharpCompilation compilation)
		{
			return DisabledModuleAnalyzer.AreEnabled(RequiredModules.ToArray());
		}

		IEnumerable<DiagnosticDescriptor> IDurianAnalyzer.GetSupportedDiagnostics()
		{
			return GetAnalyzers().SelectMany(a => a.GetSupportedDiagnostics()).Distinct();
		}

		/// <summary>
		/// Method executed when compilation is started.
		/// </summary>
		/// <param name="context"><see cref="CompilationStartAnalysisContext"/> to register actions to.</param>
		protected virtual void OnCompilationStart(CompilationStartAnalysisContext context)
		{
			if (context.Compilation is CSharpCompilation compilation && ShouldAnalyze(compilation))
			{
				IDurianAnalysisContext durianContext = new DurianCompilationStartAnalysisContext(context);
				Register(durianContext, compilation);
			}
		}
	}
}