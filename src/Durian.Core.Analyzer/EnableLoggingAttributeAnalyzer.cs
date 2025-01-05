using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.Analysis.DurianDiagnostics;

namespace Durian.Analysis;

/// <summary>
/// Analyzes whether the <see cref="EnableLoggingAttribute"/> is used properly.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EnableLoggingAttributeAnalyzer : DurianAnalyzer
{
	/// <inheritdoc/>
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
		DUR0010_DuplicateEnableLogging
	);

	/// <summary>
	/// Initializes a new instance of the <see cref="EnableLoggingAttributeAnalyzer"/> class.
	/// </summary>
	public EnableLoggingAttributeAnalyzer()
	{
	}

	/// <inheritdoc/>
	protected override void Register(IDurianAnalysisContext context)
	{
		context.RegisterCompilationStartAction(context =>
		{
			INamedTypeSymbol? enableLoggingAttribute = context.Compilation.GetTypeByMetadataName(typeof(EnableLoggingAttribute).ToString());

			if (enableLoggingAttribute is null)
			{
				return;
			}

			context.RegisterCompilationEndAction(context => Analyze(context, enableLoggingAttribute));
		});
	}

	private static void Analyze(CompilationAnalysisContext context, INamedTypeSymbol enableLoggingAttribute)
	{
		AttributeData[] attributes = context.Compilation.Assembly.GetAttributes(enableLoggingAttribute).ToArray();

		if (attributes.Length == 0)
		{
			return;
		}

		HashSet<string> modules = new();

		foreach (AttributeData attr in attributes)
		{
			TypedConstant argument = attr.GetConstructorArgument(0);

			if (!AnalysisUtilities.TryRetrieveModuleName(argument, out string? moduleName))
			{
				continue;
			}

			if (!modules.Add(moduleName))
			{
				context.ReportDiagnostic(Diagnostic.Create(DUR0010_DuplicateEnableLogging, attr.GetLocation()));
			}
		}
	}
}