// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Analysis
{
	/// <summary>
	/// Base class for Durian analyzers.
	/// </summary>
	/// <typeparam name="TCompilation">Type of <see cref="ICompilationData"/> this <see cref="DurianAnalyzer"/> uses.</typeparam>
	public abstract class DurianAnalyzer<TCompilation> : DurianAnalyzer where TCompilation : class, ICompilationData
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DurianAnalyzer{TCompilation}"/> class.
		/// </summary>
		protected DurianAnalyzer()
		{
		}

		/// <inheritdoc/>
		public sealed override void Initialize(AnalysisContext context)
		{
			if (Concurrent)
			{
				context.EnableConcurrentExecution();
			}

			context.ConfigureGeneratedCodeAnalysis(AllowGenerated ? GeneratedCodeAnalysisFlags.Analyze : GeneratedCodeAnalysisFlags.None);

			context.RegisterCompilationStartAction(context =>
			{
				if (context.Compilation is not CSharpCompilation compilation)
				{
					return;
				}

				DiagnosticBag diagnosticReceiver = DiagnosticReceiver.Factory.Bag();
				TCompilation data = CreateCompilation(compilation, diagnosticReceiver);
				IDurianAnalysisContext durianContext = new DurianCompilationStartAnalysisContext(context);

				if (!data.HasErrors)
				{
					Register(durianContext, data);
				}

				ReportInitializationDiagnostics(durianContext, diagnosticReceiver);
			});
		}

		/// <inheritdoc/>
		[Obsolete("Implementation of this method was removed - use Register(IDurianAnalysisContext, TCompilation) instead.")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		public sealed override void Register(IDurianAnalysisContext context)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
		{
			// Do nothing
		}

		/// <summary>
		/// Performs the analysis using the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="context"><see cref="IDurianAnalysisContext"/> to register the actions to.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to be used during the analysis.</param>
		public abstract void Register(IDurianAnalysisContext context, TCompilation compilation);

		/// <summary>
		/// Creates a new <see cref="ICompilationData"/> based on the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to create the <see cref="ICompilationData"/> from.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> to report diagnostics to.</param>
		protected abstract TCompilation CreateCompilation(CSharpCompilation compilation, IDiagnosticReceiver diagnosticReceiver);

		/// <inheritdoc/>
		protected sealed override void Register(IDurianAnalysisContext context, CSharpCompilation compilation)
		{
			DiagnosticBag diagnosticReceiver = DiagnosticReceiver.Factory.Bag();
			TCompilation data = CreateCompilation(compilation, diagnosticReceiver);

			if (!data.HasErrors)
			{
				Register(context, data);
			}

			ReportInitializationDiagnostics(context, diagnosticReceiver);
		}

		private static void ReportInitializationDiagnostics(IDurianAnalysisContext context, DiagnosticBag diagnosticReceiver)
		{
			if (diagnosticReceiver.Count > 0)
			{
				context.RegisterCompilationAction(context =>
				{
					foreach (Diagnostic diag in diagnosticReceiver)
					{
						context.ReportDiagnostic(diag);
					}
				});
			}
		}
	}
}