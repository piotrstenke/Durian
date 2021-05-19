using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Generator.DefaultParam
{
	/// <summary>
	/// Analyzes types with type parameters marked by the <see cref="DefaultParamAttribute"/>.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public partial class DefaultParamTypeAnalyzer : DefaultParamAnalyzer
	{
		/// <inheritdoc/>
		public override SymbolKind SupportedSymbolKind => SymbolKind.NamedType;

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamTypeAnalyzer"/> class.
		/// </summary>
		public DefaultParamTypeAnalyzer()
		{
		}

		/// <inheritdoc/>
		protected override IEnumerable<DiagnosticDescriptor> GetAnalyzerSpecificDiagnostics()
		{
			return new DiagnosticDescriptor[]
			{
				DefaultParamDiagnostics.DUR0122_ApplyCopyTypeConventionOnStructOrSealedType
			};
		}

		/// <inheritdoc/>
		public override void Analyze(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			if(symbol is not INamedTypeSymbol t)
			{
				return;
			}

			TypeKind kind = t.TypeKind;

			if(kind is not TypeKind.Class and not TypeKind.Struct and not TypeKind.Interface)
			{
				return;
			}

			WithDiagnostics.Analyze(diagnosticReceiver, t, compilation, cancellationToken);
		}
	}
}
