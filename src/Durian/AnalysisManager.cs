// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Generator.Manager
{
	/// <summary>
	/// Defines basic behavior of an analysis manager.
	/// </summary>
	public abstract class AnalysisManager : DiagnosticAnalyzer
	{
		/// <summary>
		/// Determines whether analyzers in this manager should analyze generated code.
		/// </summary>
		public virtual bool AllowGenerated => false;

		/// <summary>
		/// Determines whether analyzers in this manager can be run concurrently.
		/// </summary>
		public virtual bool Concurrent => true;

		/// <summary>
		/// Initializes a new instance of the <see cref="AnalysisManager"/> class.
		/// </summary>
		protected AnalysisManager()
		{
		}

		/// <inheritdoc/>
		public override void Initialize(AnalysisContext context)
		{
			if (Concurrent)
			{
				context.EnableConcurrentExecution();
			}

			context.ConfigureGeneratedCodeAnalysis(AllowGenerated ? GeneratedCodeAnalysisFlags.Analyze : GeneratedCodeAnalysisFlags.None);

			context.RegisterCompilationStartAction(context =>
			{
				bool isValid = ShouldAnalyze(context.Compilation, out Diagnostic[]? diagnostics);

				if (diagnostics is not null && diagnostics.Length > 0)
				{
					context.RegisterCompilationEndAction(context =>
					{
						foreach (Diagnostic diagnostic in diagnostics)
						{
							context.ReportDiagnostic(diagnostic);
						}
					});
				}

				if (isValid)
				{
					RegisterActions(context);
				}
			});
		}

		/// <summary>
		/// Registers actions to be executed during analysis.
		/// </summary>
		/// <param name="context"><see cref="CompilationStartAnalyzerAction"/> to register the actions to.</param>
#pragma warning disable RS1012 // Start action has no registered actions.

		protected virtual void RegisterActions(CompilationStartAnalysisContext context)
#pragma warning restore RS1012 // Start action has no registered actions.
		{
			// Do nothing.
		}

		/// <summary>
		/// Determines whether analysis should be performed for the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to check.</param>
		/// <param name="diagnostics">An array of diagnostics this <see cref="Compilation"/> produced.</param>
		protected virtual bool ShouldAnalyze(Compilation compilation, out Diagnostic[]? diagnostics)
		{
			diagnostics = Array.Empty<Diagnostic>();
			return true;
		}
	}
}
