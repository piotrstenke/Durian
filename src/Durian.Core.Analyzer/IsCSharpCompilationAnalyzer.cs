using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.Generator.Core.DurianDiagnostics;

namespace Durian.Generator.Core
{
	/// <summary>
	/// Analyzes if the current compilation is <see cref="CSharpCompilation"/>.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.FSharp, LanguageNames.VisualBasic)]
	public sealed class IsCSharpCompilationAnalyzer : DurianAnalyzer
	{
		/// <inheritdoc/>
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			DUR0004_DurianModulesAreValidOnlyInCSharp,
			DUR0006_ProjectMustUseCSharp9
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
			if (context.Compilation is not CSharpCompilation c)
			{
				context.ReportDiagnostic(Diagnostic.Create(DUR0004_DurianModulesAreValidOnlyInCSharp, Location.None));
				return;
			}

			if (c.LanguageVersion < LanguageVersion.CSharp9)
			{
				context.ReportDiagnostic(Diagnostic.Create(DUR0006_ProjectMustUseCSharp9, Location.None));
			}
		}
	}
}
