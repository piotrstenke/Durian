using System;
using System.Collections.Generic;
using Durian.Analysis;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.TestServices;

/// <summary>
/// <see cref="ITestableGenerator"/> implementation that supports <see cref="IIncrementalGenerator"/>s.
/// </summary>
public class TestableIncrementalGenerator : DurianIncrementalGenerator, ITestableGenerator
{
	/// <inheritdoc cref="ITestableGenerator.UnderlayingGenerator"/>
	public DurianIncrementalGenerator UnderlayingGenerator { get; }

	/// <inheritdoc/>
	public override string GeneratorName => UnderlayingGenerator.GeneratorName!;

	/// <inheritdoc/>
	public override string GeneratorVersion => UnderlayingGenerator.GeneratorVersion!;

	/// <inheritdoc/>
	public override int NumStaticTrees => UnderlayingGenerator.NumStaticTrees;

	/// <inheritdoc/>
	public string TestName { get; }

	IDurianGenerator ITestableGenerator.UnderlayingGenerator => UnderlayingGenerator;

	/// <summary>
	/// Initializes a new instance of the <see cref="TestableIncrementalGenerator"/> class.
	/// </summary>
	/// <param name="generator">Underlaying generator.</param>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
	public TestableIncrementalGenerator(DurianIncrementalGenerator generator) : this(generator, null)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="TestableIncrementalGenerator"/> class.
	/// </summary>
	/// <param name="generator">Underlaying generator.</param>
	/// <param name="testName">Name of the test that is currently running.</param>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
	public TestableIncrementalGenerator(DurianIncrementalGenerator generator, string? testName)
	{
		if (generator is null)
		{
			throw new ArgumentNullException(nameof(generator));
		}

		UnderlayingGenerator = generator;
		TestName = string.IsNullOrWhiteSpace(testName) ? TestUtilities.DefaultTestName : testName!;

		UnderlayingGenerator.HintNameProvider = new TestHintNameProvider(TestName);
	}

	/// <inheritdoc/>
	protected internal override IEnumerable<ISourceTextProvider>? GetInitialSources()
	{
		return UnderlayingGenerator.GetInitialSources();
	}

	/// <inheritdoc/>
	protected internal override void Register(IncrementalValueProvider<Compilation> compilation, IncrementalGeneratorInitializationContext context)
	{
		UnderlayingGenerator.Register(compilation, context);
	}

	/// <inheritdoc/>
	protected internal override DurianModule[] GetRequiredModules()
	{
		return UnderlayingGenerator.GetRequiredModules();
	}

	/// <inheritdoc/>
	protected internal override bool ValidateCompilation(CSharpCompilation compilation, Action<Diagnostic> reportDiagnostic)
	{
		return UnderlayingGenerator.ValidateCompilation(compilation, reportDiagnostic);
	}
}
