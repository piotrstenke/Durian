// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.Analysis.DurianDiagnostics;

namespace Durian.Analysis
{
#pragma warning disable RS1001 // Missing diagnostic analyzer attribute.

	/// <summary>
	/// Analyzes if the user added custom types to the <c>Durian.Analysis</c> namespace.
	/// </summary>
#if !MAIN_PACKAGE

	[DiagnosticAnalyzer(LanguageNames.CSharp)]
#endif

	public sealed class CustomTypesInGeneratorNamespaceAnalyzer : DurianAnalyzer
#pragma warning restore RS1001 // Missing diagnostic analyzer attribute.
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
			if (!Analyze(context.Node))
			{
				context.ReportDiagnostic(Diagnostic.Create(DUR0005_DoNotAddTypesToGeneratorNamespace, context.Node.GetLocation()));
			}
		}

		private static bool Analyze(SyntaxNode node)
		{
			if (node is not NamespaceDeclarationSyntax n)
			{
				return true;
			}

			return n.Name.ToString() != DurianStrings.GeneratorNamespace;
		}
	}
}
