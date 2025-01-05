using System.Collections.Generic;
using System.Collections.Immutable;
using Durian.Analysis.Logging;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.GlobalScope;

/// <summary>
/// Generates syntax tree of types required by the <c>GlobalScope</c> module.
/// </summary>
[Generator(LanguageNames.CSharp)]
[LoggingConfiguration(
	SupportedLogs = GeneratorLogs.All,
	LogDirectory = "GlobalScope",
	SupportsDiagnostics = true,
	RelativeToGlobal = true,
	EnableExceptions = true)]
public sealed class GlobalScopeGenerator : DurianIncrementalGenerator
{
	/// <inheritdoc/>
	public override string GeneratorName => "GlobalScope";

	/// <inheritdoc/>
	public override string GeneratorVersion => "1.0.0";

	/// <summary>
	/// Initializes a new instance of the <see cref="GlobalScopeGenerator"/> class.
	/// </summary>
	public GlobalScopeGenerator()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="GlobalScopeGenerator"/> class.
	/// </summary>
	/// <param name="context">Configures how this <see cref="GlobalScopeGenerator"/> is initialized.</param>
	public GlobalScopeGenerator(in GeneratorLogCreationContext context) : base(in context)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="GlobalScopeGenerator"/> class.
	/// </summary>
	/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
	public GlobalScopeGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
	{
	}

	/// <summary>
	/// Returns a collection of <see cref="ISourceTextProvider"/> used by this generator to create initial sources.
	/// </summary>
	public static IEnumerable<ISourceTextProvider> GetSourceProviders()
	{
		return new ISourceTextProvider[]
		{
			new GlobalScopeAttributeProvider()
		};
	}

	/// <inheritdoc/>
	protected override void Register(IncrementalValueProvider<Compilation> compilation, IncrementalGeneratorInitializationContext context)
	{
		IncrementalValuesProvider<Data> potential = context.SyntaxProvider.ForAttributeWithMetadataName(
			GlobalScopeAttributeProvider.FullName,
			static (node, _) => node is ClassDeclarationSyntax,
			static (context, _) => new Data(context.TargetSymbol.GetFullyQualifiedName(), GlobalScopeDeclarationAnalyzer.Analyze(context.TargetSymbol))
		);

		IncrementalValueProvider<ImmutableArray<Data>> valid = potential
			.Where(x => x.IsValidTarget)
			.Collect();

		context.RegisterSourceOutput(valid, (context, data) =>
		{
			CodeBuilder builder = new();

			foreach (Data target in data)
			{
				builder.UsingStatic(target.FullName, true);
			}

			AddSource("_DurianGlobalScopes.cs", builder.ToString(), context);
		});
	}

	/// <inheritdoc/>
	protected internal override IEnumerable<ISourceTextProvider>? GetInitialSources()
	{
		return GetSourceProviders();
	}

	/// <inheritdoc/>
	protected internal override DurianModule[] GetRequiredModules()
	{
		return new DurianModule[] { DurianModule.GlobalScope };
	}

	private record struct Data(string FullName, bool IsValidTarget);
}
