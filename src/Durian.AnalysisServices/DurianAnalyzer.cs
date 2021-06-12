// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Generator
{
	/// <summary>
	/// Base class for all Durian analyzers.
	/// </summary>
	public abstract class DurianAnalyzer : DiagnosticAnalyzer, IAnalyzerInfo
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
		public sealed override void Initialize(AnalysisContext context)
		{
			InitializeCore(context);
		}

		/// <inheritdoc cref="IAnalyzerInfo.Register(IDurianAnalysisContext, CSharpCompilation)"/>
		public abstract void Register(IDurianAnalysisContext context);

		void IAnalyzerInfo.Register(IDurianAnalysisContext context, CSharpCompilation compilation)
		{
			Register(context, compilation);
		}

		private protected virtual void InitializeCore(AnalysisContext context)
		{
			if (Concurrent)
			{
				context.EnableConcurrentExecution();
			}

			context.ConfigureGeneratedCodeAnalysis(AllowGenerated ? GeneratedCodeAnalysisFlags.Analyze : GeneratedCodeAnalysisFlags.None);
			IDurianAnalysisContext c = new DurianAnalysisContext(context);
			Register(c);
		}

		/// <inheritdoc cref="IAnalyzerInfo.Register(IDurianAnalysisContext, CSharpCompilation)"/>
		private protected virtual void Register(IDurianAnalysisContext context, CSharpCompilation compilation)
		{
			Register(context);
		}
	}
}
