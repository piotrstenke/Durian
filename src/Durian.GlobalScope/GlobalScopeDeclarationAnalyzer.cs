using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.Analysis.GlobalScope.GlobalScopeDiagnostics;

namespace Durian.Analysis.GlobalScope;

/// <summary>
/// Analyzes types marked with the <c>Durian.GlobalScopeAttribute</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GlobalScopeDeclarationAnalyzer : DurianAnalyzer<GlobalScopeCompilationData>
{
	/// <inheritdoc/>
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
		DUR0501_TypeIsNotStaticClass,
		DUR0502_TypeIsNotTopLevel
	);

	/// <summary>
	/// Initializes a new instance of the <see cref="GlobalScopeDeclarationAnalyzer"/> class.
	/// </summary>
	public GlobalScopeDeclarationAnalyzer()
	{
	}

	/// <inheritdoc/>
	protected override void Register(IDurianAnalysisContext context, GlobalScopeCompilationData compilation)
	{
		context.RegisterSymbolAction(context => Analyze(context, compilation), SymbolKind.NamedType);
	}

	/// <inheritdoc/>
	protected override GlobalScopeCompilationData CreateCompilation(CSharpCompilation compilation, IDiagnosticReceiver diagnosticReceiver)
	{
		return new(compilation);
	}

	internal static bool Analyze(ISymbol symbol)
	{
		return Analyze(symbol, null);
	}

	internal static bool Analyze(ISymbol symbol, Action<Diagnostic>? reportDiagnostic)
	{
		return symbol is INamedTypeSymbol type && Analyze(type, reportDiagnostic);
	}

	private static void Analyze(SymbolAnalysisContext context, GlobalScopeCompilationData compilation)
	{
		if(context.Symbol is not INamedTypeSymbol type || !type.IsClass())
		{
			// Handled by the AttributeUsage attribute.
			return;
		}

		if(!type.HasAttribute(compilation.GlobalScopeAttribute))
		{
			// Nothing to analyze.
			return;
		}

		Analyze(type, context.ReportDiagnostic);
	}

	private static bool Analyze(INamedTypeSymbol type, Action<Diagnostic>? reportDiagnostic)
	{
		Location? location = null;
		string? name = null;
		bool isValid = true;

		if (!type.IsStatic && !ReportDiagnostic(DUR0501_TypeIsNotStaticClass))
		{
			return false;
		}

		if (!type.IsTopLevel() && !ReportDiagnostic(DUR0502_TypeIsNotTopLevel))
		{
			return false;
		}

		return isValid;

		bool ReportDiagnostic(DiagnosticDescriptor descriptor)
		{
			isValid = false;

			if (reportDiagnostic is not null)
			{
				location ??= type.Locations.FirstOrDefault();
				name ??= type.GetFullyQualifiedName();

				reportDiagnostic.Invoke(Diagnostic.Create(descriptor, location, name));
				return true;
			}

			return false;
		}
	}
}
