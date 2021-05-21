using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.Generator.DurianDiagnostics;

namespace Durian.Generator
{
	/// <summary>
	/// Analyzes if the current compilation is <see cref="CSharpCompilation"/>.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.FSharp, LanguageNames.VisualBasic)]
	public sealed class IsCSharpCompilationAnalyzer : DurianAnalyzer
	{
		/// <inheritdoc/>
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			DUR0004_DurianModulesAreValidOnlyInCSharp
		);

		/// <summary>
		/// Initializes a new instance of the <see cref="IsCSharpCompilationAnalyzer"/> class.
		/// </summary>
		public IsCSharpCompilationAnalyzer()
		{
		}

		/// <inheritdoc/>
		public override void Initialize(AnalysisContext context)
		{
			base.Initialize(context);

			context.RegisterCompilationAction(Analyze);
		}

		private static void Analyze(CompilationAnalysisContext context)
		{
			if (context.Compilation is not CSharpCompilation)
			{
				context.ReportDiagnostic(Diagnostic.Create(DUR0004_DurianModulesAreValidOnlyInCSharp, Location.None));
			}
		}
	}
}
