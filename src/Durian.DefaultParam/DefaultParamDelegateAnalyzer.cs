using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Durian.Configuration;
using Durian.Generator.Extensions;
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
		protected override IEnumerable<DiagnosticDescriptor> GetAnalyzerSpecificDiagnostics()
		{
			return Array.Empty<DiagnosticDescriptor>();
		}

		/// <inheritdoc/>
		public override void Analyze(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			if (symbol is not INamedTypeSymbol t || t.TypeKind != TypeKind.Delegate)
			{
				return;
			}

			WithDiagnostics.DefaultAnalyze(diagnosticReceiver, symbol, compilation, cancellationToken);
		}

		/// <summary>
		/// Determines whether the 'new' modifier is allowed to be applied to the target <see cref="INamedTypeSymbol"/> according to the most specific <see cref="DefaultParamConfigurationAttribute"/> or <see cref="DefaultParamScopedConfigurationAttribute"/>.
		/// </summary>
		/// <param name="method"><see cref="INamedTypeSymbol"/> to check.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		public static bool AllowsNewModifier(INamedTypeSymbol method, DefaultParamCompilationData compilation)
		{
			return DefaultParamUtilities.AllowsNewModifier(method.GetAttributes(), method.GetContainingTypeSymbols().ToArray(), compilation);
		}

		/// <summary>
		/// Checks, if the collection of <see cref="AttributeData"/> and <paramref name="containingTypes"/> of a <see cref="INamedTypeSymbol"/> allow to apply the 'new' modifier.
		/// </summary>
		/// <param name="attributes">A collection of <see cref="AttributeData"/> representing attributes of a <see cref="INamedTypeSymbol"/>.</param>
		/// <param name="containingTypes">Containing types of the target <see cref="INamedTypeSymbol"/>.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		public static bool AllowsNewModifier(IEnumerable<AttributeData> attributes, INamedTypeSymbol[] containingTypes, DefaultParamCompilationData compilation)
		{
			return DefaultParamUtilities.AllowsNewModifier(attributes, containingTypes, compilation);
		}
	}
}
