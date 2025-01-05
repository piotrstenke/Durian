using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.Analysis.DurianDiagnostics;

namespace Durian.Analysis;

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

	/// <summary>
	/// Analyzes the specified <paramref name="compilation"/>.
	/// </summary>
	/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
	/// <param name="compilation"><see cref="Compilation"/> to analyze.</param>
	public static bool Analyze(IDiagnosticReceiver diagnosticReceiver, Compilation compilation)
	{
		bool value = Analyze(compilation);

		if (!value)
		{
			diagnosticReceiver.ReportDiagnostic(DUR0004_DurianModulesAreValidOnlyInCSharp);
		}

		return value;
	}

	/// <summary>
	/// Analyzes the specified <paramref name="compilation"/>.
	/// </summary>
	/// <param name="compilation"><see cref="Compilation"/> to analyze.</param>
	public static bool Analyze(Compilation compilation)
	{
		return compilation is CSharpCompilation;
	}

	/// <summary>
	/// Creates a <see cref="Diagnostic"/> indicating that the current compilation is not a C# compilation.
	/// </summary>
	public static Diagnostic GetNotCSharpCompilationDiagnostic()
	{
		return Diagnostic.Create(DUR0004_DurianModulesAreValidOnlyInCSharp, Location.None);
	}

	/// <inheritdoc/>
	protected override void Register(IDurianAnalysisContext context)
	{
		context.RegisterCompilationAction(Analyze);
	}

	private static void Analyze(CompilationAnalysisContext context)
	{
		if (!Analyze(context.Compilation))
		{
			context.ReportDiagnostic(Diagnostic.Create(DUR0004_DurianModulesAreValidOnlyInCSharp, Location.None));
		}
	}
}