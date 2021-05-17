using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Generator
{
	/// <summary>
	/// Base class for Durian analyzers.
	/// </summary>
	public abstract class DurianAnalyzer : DiagnosticAnalyzer
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DurianAnalyzer"/> class.
		/// </summary>
		protected DurianAnalyzer()
		{
		}

		/// <inheritdoc/>
		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		}
	}
}
