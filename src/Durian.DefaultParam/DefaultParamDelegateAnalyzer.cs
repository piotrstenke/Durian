using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Generator.DefaultParam
{
	/// <summary>
	/// Analyzes delegates with type parameters marked by the <see cref="DefaultParamAttribute"/>.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class DefaultParamDelegateAnalyzer : DefaultParamAnalyzer
	{
		/// <inheritdoc/>
		public override SymbolKind SupportedSymbolKind => SymbolKind.NamedType;

		/// <inheritdoc/>
		public override void Analyze(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			if (symbol is not INamedTypeSymbol t || t.TypeKind != TypeKind.Delegate)
			{
				return;
			}

			WithDiagnostics.DefaultAnalyze(diagnosticReceiver, symbol, compilation, cancellationToken);
		}

		/// <inheritdoc/>
		protected override IEnumerable<DiagnosticDescriptor> GetAnalyzerSpecificDiagnostics()
		{
			return Array.Empty<DiagnosticDescriptor>();
		}
	}
}
