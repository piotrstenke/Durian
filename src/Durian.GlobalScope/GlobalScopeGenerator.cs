using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Durian.Analysis.Logging;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.GlobalScope;

/// <summary>
/// Generates code for types marked with the <c>GlobalScope</c> attribute.
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

	/// <inheritdoc/>
	public override int NumStaticTrees => 2;

	/// <summary>
	/// Initializes a new instance of the <see cref="GlobalScopeGenerator"/> class.
	/// </summary>
	public GlobalScopeGenerator()
	{
		HintNameProvider = CreateHintNameProvider();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="GlobalScopeGenerator"/> class.
	/// </summary>
	/// <param name="context">Configures how this <see cref="GlobalScopeGenerator"/> is initialized.</param>
	public GlobalScopeGenerator(in GeneratorLogCreationContext context) : base(in context)
	{
		HintNameProvider = CreateHintNameProvider();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="GlobalScopeGenerator"/> class.
	/// </summary>
	/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
	public GlobalScopeGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
	{
		HintNameProvider = CreateHintNameProvider();
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
	protected internal override void Register(IncrementalValueProvider<Compilation> compilation, IncrementalGeneratorInitializationContext context)
	{
		if(LoggingConfiguration.EnableDiagnostics)
		{
			RegisterWithDiagnostics(context);
			return;
		}

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
			if (data.Length == 0)
			{
				return;
			}

			CodeBuilder builder = new();

			foreach (Data target in data)
			{
				Generate(builder, target.FullName);
			}

			AddSource(HintNameProvider.GetHintName(), builder.ToString(), context);
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

	private void RegisterWithDiagnostics(IncrementalGeneratorInitializationContext context)
	{
		IncrementalValueProvider<ImmutableArray<ISymbol>> targets = context.SyntaxProvider.ForAttributeWithMetadataName(
			GlobalScopeAttributeProvider.FullName,
			static (node, _) => node is ClassDeclarationSyntax,
			static (context, _) => context.TargetSymbol
		).Collect();

		context.RegisterSourceOutput(targets, (context, data) =>
		{
			if (data.Length == 0)
			{
				return;
			}

			CodeBuilder builder = new();

			bool isGenerated = false;

			foreach (ISymbol target in data)
			{
				if(!GlobalScopeDeclarationAnalyzer.Analyze(target, context.ReportDiagnostic))
				{
					continue;
				}

				isGenerated = true;
				Generate(builder, target.GetFullyQualifiedName());
			}

			if(isGenerated)
			{
				AddSource(HintNameProvider.GetHintName(), builder.ToString(), context);
			}
		});
	}

	private static void Generate(CodeBuilder builder, string target)
	{
		builder.UsingStatic(target, true);
	}

	private static StaticHintNameProvider CreateHintNameProvider()
	{
		return new StaticHintNameProvider("_DurianGlobalScopes.cs");
	}

	private record struct Data(string FullName, bool IsValidTarget);
}
