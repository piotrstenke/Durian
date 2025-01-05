using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Durian.Analysis.Data;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.Analysis.DurianDiagnostics;

namespace Durian.Analysis;

/// <summary>
/// Analyzes if the Durian types used by the user are properly imported.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TypeImportAnalyzer : DurianAnalyzer<CompilationWithImportedTypes>
{
	/// <inheritdoc/>
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
		DUR0002_ModuleOfTypeIsNotImported,
		DUR0003_DoNotUseTypeFromDurianGeneratorNamespace
	);

	/// <summary>
	/// Initializes a new instance of the <see cref="TypeImportAnalyzer"/> class.
	/// </summary>
	public TypeImportAnalyzer()
	{
	}

	/// <inheritdoc/>
	protected override void Register(IDurianAnalysisContext context, CompilationWithImportedTypes compilation)
	{
		context.RegisterSyntaxNodeAction(c => Analyze(c, compilation), SyntaxKind.IdentifierName, SyntaxKind.GenericName);
	}

	/// <inheritdoc/>
	protected override CompilationWithImportedTypes CreateCompilation(CSharpCompilation compilation, IDiagnosticReceiver diagnosticReceiver)
	{
		return new CompilationWithImportedTypes(compilation);
	}

	private static void Analyze(SyntaxNodeAnalysisContext context, CompilationWithImportedTypes compilation)
	{
		if (!IsValidForAnalysis(context.Node, context.SemanticModel, context.CancellationToken, out INamedTypeSymbol? type))
		{
			return;
		}

		if (compilation.IsDisabled(type, out DurianModule[]? modules))
		{
			context.ReportDiagnostic(Diagnostic.Create(DUR0002_ModuleOfTypeIsNotImported, context.Node.GetLocation(), type, modules[0]));
		}
		else if (modules is not null)
		{
			// Every type in the 'Durian.Generator' namespace is located in the Core module.

			if (Array.IndexOf(modules, DurianModule.Core) > -1 && string.Join(".", type.GetContainingNamespaces().Select(n => n.Name)) == "Durian.Generator")
			{
				context.ReportDiagnostic(Diagnostic.Create(DUR0003_DoNotUseTypeFromDurianGeneratorNamespace, context.Node.GetLocation()));
			}
		}
	}

	private static bool IsValidForAnalysis(SyntaxNode node, SemanticModel semanticModel, CancellationToken cancellationToken, [NotNullWhen(true)] out INamedTypeSymbol? type)
	{
		if (node is not SimpleNameSyntax name)
		{
			type = null;
			return false;
		}

		SymbolInfo info = semanticModel.GetSymbolInfo(name, cancellationToken);

		if (info.Symbol is null)
		{
			type = null;
			return false;
		}

		if (info.Symbol is not INamedTypeSymbol t)
		{
			if (!name.Ancestors().Any(a => a is AttributeSyntax))
			{
				type = null;
				return false;
			}

			t = info.Symbol!.ContainingType;

			if (t is null)
			{
				type = null;
				return false;
			}
		}

		type = t;
		return true;
	}
}