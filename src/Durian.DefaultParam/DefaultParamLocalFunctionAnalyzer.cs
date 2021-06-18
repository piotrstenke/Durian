// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Analysis.DefaultParam
{
#pragma warning disable RS1001 // Missing diagnostic analyzer attribute.

	/// <summary>
	/// Analyzes local functions with type parameters marked by the <see cref="DefaultParamAttribute"/>.
	/// </summary>
#if !MAIN_PACKAGE

	[DiagnosticAnalyzer(LanguageNames.CSharp)]
#endif

	public class DefaultParamLocalFunctionAnalyzer : DefaultParamAnalyzer
#pragma warning restore RS1001 // Missing diagnostic analyzer attribute.
	{
		/// <inheritdoc/>
		public override SymbolKind SupportedSymbolKind => SymbolKind.Method;

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamLocalFunctionAnalyzer"/> class.
		/// </summary>
		public DefaultParamLocalFunctionAnalyzer()
		{
		}

		/// <summary>
		/// Returns a collection of all supported diagnostics of <see cref="DefaultParamLocalFunctionAnalyzer"/>.
		/// </summary>
		public static IEnumerable<DiagnosticDescriptor> GetSupportedDiagnostics()
		{
			return GetBaseDiagnostics().Concat(GetAnalyzerSpecificDiagnosticsAsArray());
		}

		/// <inheritdoc/>
		public override void Register(IDurianAnalysisContext context, DefaultParamCompilationData compilation)
		{
			context.RegisterSyntaxNodeAction(c => FindAndAnalyzeLocalFunction(c, compilation), SyntaxKind.LocalFunctionStatement);
		}

		/// <inheritdoc/>
		protected override IEnumerable<DiagnosticDescriptor> GetAnalyzerSpecificDiagnostics()
		{
			return GetAnalyzerSpecificDiagnosticsAsArray();
		}

		private static void FindAndAnalyzeLocalFunction(SyntaxNodeAnalysisContext context, DefaultParamCompilationData compilation)
		{
			if (context.Node is not LocalFunctionStatementSyntax l)
			{
				return;
			}

			ISymbol? symbol = context.SemanticModel.GetDeclaredSymbol(l);

			if (symbol is not IMethodSymbol m)
			{
				return;
			}

			ITypeParameterSymbol[] typeParameters = m.TypeParameters.ToArray();

			if (typeParameters.Any(t => t.HasAttribute(compilation.MainAttribute!)))
			{
				DiagnosticDescriptor d = DefaultParamDiagnostics.DUR0103_DefaultParamIsNotOnThisTypeOfMethod;
				context.ReportDiagnostic(Diagnostic.Create(d, l.GetLocation(), m));
			}
		}

		private static DiagnosticDescriptor[] GetAnalyzerSpecificDiagnosticsAsArray()
		{
			return new[]
			{
				DefaultParamDiagnostics.DUR0103_DefaultParamIsNotOnThisTypeOfMethod
			};
		}
	}
}
