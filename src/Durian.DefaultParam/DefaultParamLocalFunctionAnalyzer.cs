// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Durian.Analysis.DefaultParam
{
	/// <summary>
	/// Analyzes local functions with type parameters marked by the <c>Durian.DefaultParamAttribute</c>.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class DefaultParamLocalFunctionAnalyzer : DefaultParamAnalyzer
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
			return GetBaseDiagnostics().Concat(new[]
			{
				DefaultParamDiagnostics.DUR0103_DefaultParamIsNotValidOnThisTypeOfMethod
			});
		}

		/// <inheritdoc/>
		public override void Register(IDurianAnalysisContext context, DefaultParamCompilationData compilation)
		{
			context.RegisterSyntaxNodeAction(c => FindAndAnalyzeLocalFunction(c, compilation), SyntaxKind.LocalFunctionStatement);
		}

		/// <inheritdoc/>
		protected override IEnumerable<DiagnosticDescriptor> GetAnalyzerSpecificDiagnostics()
		{
			return new[]
			{
				DefaultParamDiagnostics.DUR0103_DefaultParamIsNotValidOnThisTypeOfMethod
			};
		}

		/// <inheritdoc/>
		protected override bool ShouldAnalyze(ISymbol symbol, DefaultParamCompilationData compilation)
		{
			return symbol is IMethodSymbol m && m.MethodKind == MethodKind.LocalFunction;
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

			if (typeParameters.Any(t => t.HasAttribute(compilation.DefaultParamAttribute!)))
			{
				DiagnosticDescriptor d = DefaultParamDiagnostics.DUR0103_DefaultParamIsNotValidOnThisTypeOfMethod;
				context.ReportDiagnostic(Diagnostic.Create(d, l.GetLocation(), m));
			}
		}
	}
}
