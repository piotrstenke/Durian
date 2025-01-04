using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.Analysis.DurianDiagnostics;

namespace Durian.Analysis;

/// <summary>
/// Analyzes if the user added custom types to the <c>Durian.Analysis</c> namespace.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CustomTypesInGeneratorNamespaceAnalyzer : DurianAnalyzer
{
	/// <inheritdoc/>
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
		DUR0005_DoNotAddTypesToGeneratorNamespace
	);

	/// <summary>
	/// Initializes a new instance of the <see cref="CustomTypesInGeneratorNamespaceAnalyzer"/> class.
	/// </summary>
	public CustomTypesInGeneratorNamespaceAnalyzer()
	{
	}

	/// <inheritdoc/>
	public override void Register(IDurianAnalysisContext context)
	{
		context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.NamespaceDeclaration);
	}

	private static void Analyze(SyntaxNodeAnalysisContext context)
	{
		if (context.Node is not NamespaceDeclarationSyntax n)
		{
			return;
		}

		if (n.Name.ToString() == DurianStrings.GeneratorNamespace)
		{
			context.ReportDiagnostic(Diagnostic.Create(DUR0005_DoNotAddTypesToGeneratorNamespace, n.Name.GetLocation()));
		}
	}
}