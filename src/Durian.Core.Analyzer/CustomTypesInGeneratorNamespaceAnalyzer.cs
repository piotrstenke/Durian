using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.Generator.DurianDiagnostics;

namespace Durian.Generator
{
	/// <summary>
	/// Analyzes if the user added custom types to the <c>Durian.Generator</c> namespace.
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
		public override void Initialize(AnalysisContext context)
		{
			base.Initialize(context);
			context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.NamespaceDeclaration);
		}

		private void Analyze(SyntaxNodeAnalysisContext context)
		{
			if (context.Node is not NamespaceDeclarationSyntax node)
			{
				return;
			}

			if (node.Name.ToString() == DurianStrings.GeneratorNamespace)
			{
				context.ReportDiagnostic(Diagnostic.Create(DUR0005_DoNotAddTypesToGeneratorNamespace, node.GetLocation()));
			}
		}
	}
}
