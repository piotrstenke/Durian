// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.Generator.Core.DurianDiagnostics;

namespace Durian.Generator.Core
{
	/// <summary>
	/// Analyzes if the Durian types used by the user are properly imported.
	/// </summary>
#if !MAIN_PACKAGE

	[DiagnosticAnalyzer(LanguageNames.CSharp)]
#endif
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
		public override void Register(IDurianAnalysisContext context, CompilationWithImportedTypes compilation)
		{
			context.RegisterSyntaxNodeAction(c => Analyze(c, compilation), SyntaxKind.IdentifierName, SyntaxKind.GenericName);
		}

		/// <inheritdoc/>
		protected override CompilationWithImportedTypes CreateCompilation(CSharpCompilation compilation)
		{
			return new CompilationWithImportedTypes(compilation);
		}

		private static void Analyze(SyntaxNodeAnalysisContext context, CompilationWithImportedTypes compilation)
		{
			if (!IsValidForAnalysis(context.Node, context.SemanticModel, context.CancellationToken, out INamedTypeSymbol? type))
			{
				return;
			}

			(bool isDurianType, bool isDisabled) = compilation.IsDisabledDurianType(type!, out DurianModule module);

			if (isDurianType)
			{
				if (module == DurianModule.Core)
				{
					context.ReportDiagnostic(Diagnostic.Create(DUR0003_DoNotUseTypeFromDurianGeneratorNamespace, context.Node.GetLocation()));
				}
				else if (isDisabled && module != DurianModule.None)
				{
					context.ReportDiagnostic(Diagnostic.Create(DUR0002_ModuleOfTypeIsNotImported, context.Node.GetLocation(), type, module));
				}
			}
		}

		private static bool IsValidForAnalysis(SyntaxNode node, SemanticModel semanticModel, CancellationToken cancellationToken, out INamedTypeSymbol? type)
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
}
