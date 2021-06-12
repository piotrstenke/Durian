// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Generator.Data;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Generator
{
	/// <summary>
	/// Base class for Durian analyzers.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="ICompilationData"/> this <see cref="DurianAnalyzer"/> uses.</typeparam>
	public abstract class DurianAnalyzer<T> : DurianAnalyzer where T : class, ICompilationData
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DurianAnalyzer{T}"/> class.
		/// </summary>
		protected DurianAnalyzer()
		{
		}

		/// <inheritdoc/>
		public sealed override void Register(IDurianAnalysisContext context)
		{
			// Do nothing
		}

		/// <summary>
		/// Performs the analysis using the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="context"><see cref="IDurianAnalysisContext"/> to register the actions to.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to be used during the analysis.</param>
		public abstract void Register(IDurianAnalysisContext context, T compilation);

		/// <summary>
		/// Creates a new <see cref="ICompilationData"/> based on the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to create the <see cref="ICompilationData"/> from.</param>
		protected abstract T CreateCompilation(CSharpCompilation compilation);

		private protected sealed override void InitializeCore(AnalysisContext context)
		{
			if (Concurrent)
			{
				context.EnableConcurrentExecution();
			}

			context.ConfigureGeneratedCodeAnalysis(AllowGenerated ? GeneratedCodeAnalysisFlags.Analyze : GeneratedCodeAnalysisFlags.None);

			context.RegisterCompilationStartAction(c =>
			{
				if (c.Compilation is not CSharpCompilation compilation)
				{
					return;
				}

				T data = CreateCompilation(compilation);

				if (!data.HasErrors)
				{
					IDurianAnalysisContext durianContext = new DurianCompilationStartAnalysisContext(c);
					Register(durianContext, data);
				}
			});
		}

		private protected sealed override void Register(IDurianAnalysisContext context, CSharpCompilation compilation)
		{
			T c = CreateCompilation(compilation);
			Register(context, c);
		}
	}
}
