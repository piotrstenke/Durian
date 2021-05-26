using System.Collections.Immutable;
using System.Linq;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.Generator.DurianDiagnostics;

namespace Durian.Generator
{
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
		protected override CompilationWithImportedTypes CreateCompilation(CSharpCompilation compilation)
		{
			return new CompilationWithImportedTypes(compilation);
		}

		/// <inheritdoc/>
		protected override void Register(CompilationStartAnalysisContext context, CompilationWithImportedTypes compilation)
		{
			context.RegisterSyntaxNodeAction(c => Analyze(c, compilation), SyntaxKind.IdentifierName, SyntaxKind.GenericName);
		}

		private static void Analyze(SyntaxNodeAnalysisContext context, CompilationWithImportedTypes compilation)
		{
			if (context.Node is not SimpleNameSyntax node)
			{
				return;
			}

			SymbolInfo info = context.SemanticModel.GetSymbolInfo(node, context.CancellationToken);

			if (info.Symbol is null)
			{
				return;
			}

			if (info.Symbol is not INamedTypeSymbol type)
			{
				if (!node.Ancestors().Any(a => a is AttributeSyntax))
				{
					return;
				}

				type = info.Symbol!.ContainingType;
			}

			(bool isDurianType, bool isDisabled) = compilation.IsDisabledDurianType(type, out ModuleIdentity? module);

			if (isDurianType)
			{
				if (module!.Module == DurianModule.Core)
				{
					context.ReportDiagnostic(Diagnostic.Create(DUR0003_DoNotUseTypeFromDurianGeneratorNamespace, node.GetLocation()));
				}
				else if (isDisabled && module.Module != DurianModule.None)
				{
					context.ReportDiagnostic(Diagnostic.Create(DUR0002_ModuleOfTypeIsNotImported, node.GetLocation(), type, module.Module));
				}
			}
		}
	}
}
