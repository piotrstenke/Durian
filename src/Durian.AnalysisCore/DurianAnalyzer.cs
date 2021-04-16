using Durian.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian
{
	/// <summary>
	/// Base class for Durian analyzers.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public abstract class DurianAnalyzer : DiagnosticAnalyzer
	{
		/// <inheritdoc/>
		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		}
	}

	/// <summary>
	/// Base class for Durian analyzers.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="ICompilationData"/> this <see cref="DurianAnalyzer"/> uses.</typeparam>
	public abstract class DurianAnalyzer<T> : DurianAnalyzer where T : class, ICompilationData
	{
		/// <inheritdoc/>
		public override void Initialize(AnalysisContext context)
		{
			base.Initialize(context);

			context.RegisterCompilationStartAction(c =>
			{
				if (c.Compilation is not CSharpCompilation compilation)
				{
					return;
				}

				T data = CreateCompilation(compilation);

				if (!data.HasErrors)
				{
					Register(c, data);
				}
			});
		}

		/// <summary>
		/// Performs the analysis using the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="context"><see cref="CompilationStartAnalysisContext"/> to register the actions to.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to be used during the analysis.</param>
		protected abstract void Register(CompilationStartAnalysisContext context, T compilation);

		/// <summary>
		/// Creates a new <see cref="ICompilationData"/> based on the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to create the <see cref="ICompilationData"/> from.</param>
		protected abstract T CreateCompilation(CSharpCompilation compilation);
	}
}
