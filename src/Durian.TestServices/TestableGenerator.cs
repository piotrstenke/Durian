using System;
using System.Collections.Generic;
using Durian.Analysis;
using Durian.Analysis.Data;
using Durian.Analysis.Filtering;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.TestServices;

/// <summary>
/// <see cref="ITestableGenerator"/> implementation that supports <see cref="DurianGeneratorWithContext{TContext}"/>.
/// </summary>
/// <typeparam name="TContext">Type of <see cref="IGeneratorPassContext"/> this generator uses.</typeparam>
public class TestableGenerator<TContext> : DurianGeneratorWithContext<TContext>, ITestableGenerator where TContext : GeneratorPassContext
{
	private volatile int _analyzerCounter;
	private volatile int _generatorCounter;

	/// <inheritdoc/>
	public override string GeneratorName => UnderlayingGenerator.GeneratorName!;

	/// <inheritdoc/>
	public override string GeneratorVersion => UnderlayingGenerator.GeneratorVersion!;

	/// <inheritdoc/>
	public override int NumStaticTrees => UnderlayingGenerator.NumStaticTrees;

	/// <inheritdoc/>
	public string TestName { get; }

	/// <inheritdoc cref="ITestableGenerator.UnderlayingGenerator"/>
	public DurianGeneratorWithContext<TContext> UnderlayingGenerator { get; }

	IDurianGenerator ITestableGenerator.UnderlayingGenerator => UnderlayingGenerator;

	/// <inheritdoc cref="TestableGenerator(DurianGeneratorWithContext{TContext}, string?)"/>
	public TestableGenerator(DurianGeneratorWithContext<TContext> generator) : this(generator, default)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="TestableGenerator{TContext}"/> class.
	/// </summary>
	/// <param name="generator"><see cref="ISourceGenerator"/> that is used to actually generate sources.</param>
	/// <param name="testName">Name of the test that is currently running.</param>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
	public TestableGenerator(DurianGeneratorWithContext<TContext> generator, string? testName) : base(generator.LoggingConfiguration)
	{
		if (generator is null)
		{
			throw new ArgumentNullException(nameof(generator));
		}

		UnderlayingGenerator = generator;
		TestName = string.IsNullOrWhiteSpace(testName) ? TestUtilities.DefaultTestName : testName!;
	}

	/// <inheritdoc/>
	public override IReadOnlyFilterContainer<IGeneratorSyntaxFilter>? GetFilters(TContext context)
	{
		return UnderlayingGenerator.GetFilters(context);
	}

	/// <inheritdoc/>
	protected internal override IDurianSyntaxReceiver CreateSyntaxReceiver()
	{
		return UnderlayingGenerator.CreateSyntaxReceiver();
	}

	/// <inheritdoc/>
	protected internal override IEnumerable<ISourceTextProvider>? GetInitialSources()
	{
		return UnderlayingGenerator.GetInitialSources();
	}

	/// <inheritdoc/>
	protected internal override DurianModule[] GetRequiredModules()
	{
		return UnderlayingGenerator.GetRequiredModules();
	}

	/// <inheritdoc/>
	public override void Initialize(GeneratorInitializationContext context)
	{
		UnderlayingGenerator.Initialize(context);
	}

	/// <inheritdoc/>
	protected internal override void AddSourceCore(string hintName, SyntaxTree tree, GeneratorExecutionContext context)
	{
		UnderlayingGenerator.AddSourceCore(hintName, tree, context);
	}

	/// <inheritdoc/>
	protected internal override void AddSourceCore(string hintName, SyntaxTree tree, TContext context)
	{
		UnderlayingGenerator.AddSourceCore(hintName, tree, context);
	}

	/// <inheritdoc/>
	protected internal override void AfterExecution(TContext context)
	{
		UnderlayingGenerator.AfterExecution(context);
	}

	/// <inheritdoc/>
	protected internal override void AfterExecutionOfGroup(IReadOnlyFilterGroup<IGeneratorSyntaxFilter> filterGroup, TContext context)
	{
		UnderlayingGenerator.AfterExecutionOfGroup(filterGroup, context);

		SetAnalyzerMode(context);
	}

	/// <inheritdoc/>
	protected internal override void BeforeExecution(TContext context)
	{
		UnderlayingGenerator.BeforeExecution(context);
	}

	/// <inheritdoc/>
	protected internal override void BeforeExecutionOfGroup(IReadOnlyFilterGroup<IGeneratorSyntaxFilter> filterGroup, TContext context)
	{
		UnderlayingGenerator.BeforeExecutionOfGroup(filterGroup, context);

		SetGeneratorMode(context);
	}

	/// <inheritdoc/>
	protected internal override void BeforeFiltersWithGeneratedSymbols(TContext context)
	{
		UnderlayingGenerator.BeforeFiltersWithGeneratedSymbols(context);
	}

	/// <inheritdoc/>
	protected internal override void BeforeFiltrationOfGeneratedSymbols(IReadOnlyFilterGroup<IGeneratorSyntaxFilter> filterGroup, TContext context)
	{
		UnderlayingGenerator.BeforeFiltrationOfGeneratedSymbols(filterGroup, context);
	}

	/// <inheritdoc/>
	protected internal override void BeforeFilteringOfGroup(IReadOnlyFilterGroup<IGeneratorSyntaxFilter> filterGroup, TContext context)
	{
		UnderlayingGenerator.BeforeFilteringOfGroup(filterGroup, context);

		SetAnalyzerMode(context);
	}

	/// <inheritdoc/>
	protected internal override TContext? CreateCurrentPassContext(CSharpCompilation currentCompilation, GeneratorExecutionContext context)
	{
		TContext? newContext = UnderlayingGenerator.CreateCurrentPassContext(currentCompilation, context);

		if (newContext is not null)
		{
			newContext.FileNameProvider = new TestHintNameProvider(TestName);
		}

		return newContext;
	}

	/// <inheritdoc/>
	protected internal override bool Generate(IMemberData data, string hintName, TContext context)
	{
		return UnderlayingGenerator.Generate(data, hintName, context);
	}

	/// <inheritdoc/>
	protected internal override void IterateThroughFilter(IGeneratorSyntaxFilter filter, TContext context)
	{
		IEnumerator<IMemberData> iter = filter.GetEnumerator(context);

		SetAnalyzerMode(context);

		while (iter.MoveNext())
		{
			SetGeneratorMode(context);
			UnderlayingGenerator.GenerateFromData(iter.Current, context);
			SetAnalyzerMode(context);
		}
	}

	/// <inheritdoc/>
	protected internal override void OnException(Exception e, TContext context, bool allowLog)
	{
		UnderlayingGenerator.OnException(e, context, false);

		if (allowLog)
		{
			UnderlayingGenerator.LogHandler.LogException(e, TestName);
		}
	}

	/// <inheritdoc/>
	protected internal override bool ValidateCompilation(CSharpCompilation compilation, Action<Diagnostic> reportDiagnostic)
	{
		return UnderlayingGenerator.ValidateCompilation(compilation, reportDiagnostic);
	}

	private void SetAnalyzerMode(TContext context)
	{
		if (context.FileNameProvider is not TestHintNameProvider t)
		{
			return;
		}

		_generatorCounter = t.Counter;
		t.Counter = _analyzerCounter;
	}

	private void SetGeneratorMode(TContext context)
	{
		if (context.FileNameProvider is not TestHintNameProvider t)
		{
			return;
		}

		_analyzerCounter = t.Counter;
		t.Counter = _generatorCounter;
	}
}
