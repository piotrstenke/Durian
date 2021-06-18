// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Durian.Analysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Manager
{
	/// <summary>
	/// Defines basic behavior of an analysis manager.
	/// </summary>
	public abstract class AnalysisManager : DiagnosticAnalyzer, IAnalyzerInfo
	{
		private protected IAnalyzerInfo[] _analyzers;

		/// <inheritdoc/>
		public virtual bool AllowGenerated => false;

		/// <inheritdoc/>
		public virtual bool Concurrent => true;

		/// <inheritdoc/>
		public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ((IAnalyzerInfo)this).GetSupportedDiagnostics().ToImmutableArray();

		/// <summary>
		/// Initializes a new instance of the <see cref="AnalysisManager"/> class.
		/// </summary>
		protected AnalysisManager()
		{
			_analyzers = GetAnalyzersCore();
		}

		/// <summary>
		/// Returns a collection of child <see cref="IAnalyzerInfo"/>s of this <see cref="AnalysisManager"/>.
		/// </summary>
		public IEnumerable<IAnalyzerInfo> GetAnalyzers()
		{
			IAnalyzerInfo[] analyzers = new IAnalyzerInfo[_analyzers.Length];
			Array.Copy(_analyzers, analyzers, _analyzers.Length);
			return analyzers;
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

			context.RegisterCompilationStartAction(context =>
			{
				IDurianAnalysisContext c = new DurianCompilationStartAnalysisContext(context);

				if (ShouldAnalyze(context.Compilation) && context.Compilation is CSharpCompilation com)
				{
					Register(c, com);
				}

				PostRegister(c, context.Compilation);
			});
		}

		/// <summary>
		/// Registers actions to be performed by this <see cref="AnalysisManager"/>.
		/// </summary>
		/// <param name="context"><see cref="IDurianAnalysisContext"/> to register the actions to.</param>
		/// <param name="compilation">Current <see cref="Compilation"/>.</param>
		public virtual void Register(IDurianAnalysisContext context, Compilation compilation)
		{
			CSharpCompilation c = (CSharpCompilation)compilation;

			foreach (IAnalyzerInfo analyzer in _analyzers)
			{
				analyzer.Register(context, c);
			}
		}

		/// <summary>
		/// Resets the internal collection of child <see cref="IAnalyzerInfo"/>s.
		/// </summary>
		public virtual void Reset()
		{
			_analyzers = GetAnalyzersCore();
		}

		/// <summary>
		/// Determines whether the specified <see cref="Compilation"/> is valid for analysis.
		/// </summary>
		/// <param name="compilation">Current <see cref="Compilation"/>.</param>
		public virtual bool ShouldAnalyze(Compilation compilation)
		{
			return true;
		}

		IEnumerable<DiagnosticDescriptor> IAnalyzerInfo.GetSupportedDiagnostics()
		{
			List<DiagnosticDescriptor> diagnostics = new(_analyzers.Length * 4);

			foreach (IAnalyzerInfo analyzer in _analyzers)
			{
				diagnostics.AddRange(analyzer.GetSupportedDiagnostics());
			}

			return diagnostics;
		}

		void IAnalyzerInfo.Register(IDurianAnalysisContext context, CSharpCompilation compilation)
		{
			Register(context, compilation);
		}

		/// <summary>
		/// Returns an array of child <see cref="IAnalyzerInfo"/>s of this <see cref="AnalysisManager"/>.
		/// </summary>
		protected abstract IAnalyzerInfo[] GetAnalyzersCore();

		/// <summary>
		/// Executed after the <see cref="Register(IDurianAnalysisContext, Compilation)"/> method, even if <see cref="ShouldAnalyze(Compilation)"/> returns <see langword="false"/>.
		/// </summary>
		/// <param name="context">Current <see cref="IDurianAnalysisContext"/>.</param>
		/// <param name="compilation">Current <see cref="Compilation"/>.</param>
		protected virtual void PostRegister(IDurianAnalysisContext context, Compilation compilation)
		{
			// Do nothing.
		}
	}
}
