using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.Generator.Core.DurianDiagnostics;

namespace Durian.Generator.Core
{
	/// <summary>
	/// Analyzer that checks if the current compilation references the <c>Durian.Core</c> package.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class CoreProjectAnalyzer : DurianAnalyzer
	{
		/// <inheritdoc/>
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			DUR0001_ProjectMustReferenceDurianCore
		);

		/// <summary>
		/// Initializes a new instance of the <see cref="CoreProjectAnalyzer"/> class.
		/// </summary>
		public CoreProjectAnalyzer()
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
			foreach (AssemblyIdentity assembly in context.Compilation.ReferencedAssemblyNames)
			{
				if (assembly.Name == "Durian.Core")
				{
					return;
				}
			}

			context.ReportDiagnostic(Diagnostic.Create(DUR0001_ProjectMustReferenceDurianCore, Location.None));
		}
	}
}
