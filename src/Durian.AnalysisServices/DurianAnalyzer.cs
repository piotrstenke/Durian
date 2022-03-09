﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;

namespace Durian.Analysis
{
	/// <summary>
	/// Base class for all Durian analyzers.
	/// </summary>
	public abstract class DurianAnalyzer : DiagnosticAnalyzer, IDurianAnalyzer
	{
		/// <inheritdoc/>
		public virtual bool AllowGenerated => false;

		/// <inheritdoc/>
		public virtual bool Concurrent => true;

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianAnalyzer"/> class.
		/// </summary>
		protected DurianAnalyzer()
		{
		}

		/// <summary>
		/// Calls the <see cref="AnalysisContext.EnableConcurrentExecution"/> and <see cref="AnalysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags)"/> methods on the <paramref name="context"/>.
		/// </summary>
		/// <param name="context">Target <see cref="AnalysisContext"/>.</param>
		public static void EnableConcurrentAndDisableGeneratedCodeAnalysis(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		}

		/// <inheritdoc/>
		public override void Initialize(AnalysisContext context)
		{
			if (Concurrent)
			{
				context.EnableConcurrentExecution();
			}

			context.ConfigureGeneratedCodeAnalysis(AllowGenerated ? GeneratedCodeAnalysisFlags.Analyze : GeneratedCodeAnalysisFlags.None);
			IDurianAnalysisContext c = new DurianAnalysisContext(context);
			Register(c);
		}

		/// <inheritdoc cref="IDurianAnalyzer.Register(IDurianAnalysisContext, CSharpCompilation)"/>
		public abstract void Register(IDurianAnalysisContext context);

		IEnumerable<DiagnosticDescriptor> IDurianAnalyzer.GetSupportedDiagnostics()
		{
			return SupportedDiagnostics;
		}

		void IDurianAnalyzer.Register(IDurianAnalysisContext context, CSharpCompilation compilation)
		{
			Register(context, compilation);
		}

		/// <inheritdoc cref="IDurianAnalyzer.Register(IDurianAnalysisContext, CSharpCompilation)"/>
		protected virtual void Register(IDurianAnalysisContext context, CSharpCompilation compilation)
		{
			Register(context);
		}
	}
}