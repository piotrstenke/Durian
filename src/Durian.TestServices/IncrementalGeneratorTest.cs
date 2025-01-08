using System;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.TestServices;

/// <summary>
/// An abstract class that provides methods to test <see cref="IIncrementalGenerator"/>s.
/// </summary>
public abstract class IncrementalGeneratorTest
{
	/// <summary>
	/// An <see cref="IIncrementalGenerator"/> that is being tested.
	/// </summary>
	public IIncrementalGenerator UnderlayingGenerator { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="SourceGeneratorTest"/> class.
	/// </summary>
	protected IncrementalGeneratorTest()
	{
		UnderlayingGenerator = CreateGenerator();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="IncrementalGeneratorTest"/> class.
	/// </summary>
	/// <param name="enableDiagnostics">Determines whether to enable diagnostics for the created <see cref="IIncrementalGenerator"/> if it supports any.</param>
	protected IncrementalGeneratorTest(bool enableDiagnostics)
	{
		IIncrementalGenerator generator = CreateGenerator();

		if (enableDiagnostics && generator is ILoggableSourceGenerator g && g.LogHandler is not null)
		{
			g.LogHandler.EnableDiagnosticsIfSupported();
		}

		UnderlayingGenerator = generator;
	}

	/// <summary>
	/// Returns a <see cref="SingleGeneratorTestResult"/> created by performing a test on the target <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="IIncrementalGenerator"/> to perform the test on.</param>
	/// <param name="input">Input for the generator.</param>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
	public static SingleGeneratorTestResult RunGenerator(IIncrementalGenerator generator, string? input)
	{
		return generator.RunTest(SingleGeneratorTestResult.Create, input);
	}

	/// <summary>
	/// Returns a <see cref="SingleGeneratorTestResult"/> created by performing a test on the target <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="IIncrementalGenerator"/> to perform the test on.</param>
	/// <param name="input">Input for the generator.</param>
	/// <param name="index">Index of the source in the generator's output.</param>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
	public static SingleGeneratorTestResult RunGenerator(IIncrementalGenerator generator, string? input, int index)
	{
		return generator.RunTest(
			(CSharpGeneratorDriver generatorDriver, CSharpCompilation inputCompilation, CSharpCompilation outputCompilation)
				=> SingleGeneratorTestResult.Create(generatorDriver, inputCompilation, outputCompilation, index), input);
	}

	/// <summary>
	/// Returns a <see cref="SingleGeneratorTestResult"/> created by performing a test on the target <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="IIncrementalGenerator"/> to perform the test on.</param>
	/// <param name="input">Input for the generator.</param>
	/// <param name="external">Code in external assembly that is referenced by assembly containing the <paramref name="input"/> text.</param>
	public static SingleGeneratorTestResult RunGeneratorWithDependency(IIncrementalGenerator generator, string? input, string? external)
	{
		MetadataReference reference = RoslynUtilities.CreateReference(external);
		return generator.RunTest(SingleGeneratorTestResult.Create, input, reference);
	}

	/// <summary>
	/// Returns a <see cref="SingleGeneratorTestResult"/> created by performing a test on the target <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="IIncrementalGenerator"/> to perform the test on.</param>
	/// <param name="input">Input for the generator.</param>
	/// <param name="external">Code in external assembly that is referenced by assembly containing the <paramref name="input"/> text.</param>
	/// <param name="index">Index of the source in the generator's output.</param>
	public static SingleGeneratorTestResult RunGeneratorWithDependency(IIncrementalGenerator generator, string? input, string? external, int index)
	{
		MetadataReference reference = RoslynUtilities.CreateReference(external);
		return generator.RunTest((CSharpGeneratorDriver generatorDriver, CSharpCompilation inputCompilation, CSharpCompilation outputCompilation)
				=> SingleGeneratorTestResult.Create(generatorDriver, inputCompilation, outputCompilation, index), input, reference);
	}

	/// <summary>
	/// Returns a <see cref="MultipleGeneratorTestResult"/> created by performing a test on the target <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="IIncrementalGenerator"/> to perform the test on.</param>
	/// <param name="input">Input for the generator.</param>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
	public static MultipleGeneratorTestResult RunGeneratorWithMultipleOutputs(IIncrementalGenerator generator, string? input)
	{
		return generator.RunTest(MultipleGeneratorTestResult.Create, input);
	}

	/// <summary>
	/// Returns a <see cref="MultipleGeneratorTestResult"/> created by performing a test on the target <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="IIncrementalGenerator"/> to perform the test on.</param>
	/// <param name="input">Input for the generator.</param>
	/// <param name="startIndex">Number of generated sources to skip.</param>
	/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
	public static MultipleGeneratorTestResult RunGeneratorWithMultipleOutputs(IIncrementalGenerator generator, string? input, int startIndex)
	{
		return generator.RunTest(
			(CSharpGeneratorDriver generatorDriver, CSharpCompilation inputCompilation, CSharpCompilation outputCompilation)
				=> MultipleGeneratorTestResult.Create(generatorDriver, inputCompilation, outputCompilation, startIndex), input);
	}

	/// <summary>
	/// Returns a <see cref="SingleGeneratorTestResult"/> created by performing a test on the target <see cref="UnderlayingGenerator"/>.
	/// </summary>
	/// <param name="input">Input for the generator.</param>
	public virtual SingleGeneratorTestResult RunGenerator(string? input)
	{
		return RunGenerator(UnderlayingGenerator, input);
	}

	/// <summary>
	/// Returns a <see cref="SingleGeneratorTestResult"/> created by performing a test on the target <see cref="UnderlayingGenerator"/>.
	/// </summary>
	/// <param name="input">Input for the generator.</param>
	/// <param name="index">Index of the source in the generator's output.</param>
	public virtual SingleGeneratorTestResult RunGenerator(string? input, int index)
	{
		return RunGenerator(UnderlayingGenerator, input, index);
	}

	/// <summary>
	/// Returns a <see cref="SingleGeneratorTestResult"/> created by performing a test on the target <see cref="UnderlayingGenerator"/>.
	/// </summary>
	/// <param name="input">Input for the generator.</param>
	/// <param name="external">Code in external assembly that is referenced by assembly containing the <paramref name="input"/> text.</param>
	public virtual SingleGeneratorTestResult RunGeneratorWithDependency(string? input, string? external)
	{
		return RunGeneratorWithDependency(UnderlayingGenerator, input, external);
	}

	/// <summary>
	/// Returns a <see cref="SingleGeneratorTestResult"/> created by performing a test on the target <see cref="UnderlayingGenerator"/>.
	/// </summary>
	/// <param name="input">Input for the generator.</param>
	/// <param name="external">Code in external assembly that is referenced by assembly containing the <paramref name="input"/> text.</param>
	/// <param name="index">Index of the source in the generator's output.</param>
	public virtual SingleGeneratorTestResult RunGeneratorWithDependency(string? input, string? external, int index)
	{
		return RunGeneratorWithDependency(UnderlayingGenerator, input, external, index);
	}

	/// <summary>
	/// Returns a <see cref="MultipleGeneratorTestResult"/> created by performing a test on the target <see cref="UnderlayingGenerator"/>.
	/// </summary>
	/// <param name="input">Input for the generator.</param>
	public virtual MultipleGeneratorTestResult RunGeneratorWithMultipleOutputs(string? input)
	{
		return RunGeneratorWithMultipleOutputs(UnderlayingGenerator, input);
	}

	/// <summary>
	/// Returns a <see cref="MultipleGeneratorTestResult"/> created by performing a test on the target <see cref="UnderlayingGenerator"/>.
	/// </summary>
	/// <param name="input">Input for the generator.</param>
	/// <param name="startIndex">Number of generated sources to skip.</param>
	public virtual MultipleGeneratorTestResult RunGeneratorWithMultipleOutputs(string? input, int startIndex)
	{
		return RunGeneratorWithMultipleOutputs(UnderlayingGenerator, input, startIndex);
	}

	/// <summary>
	/// Creates a new instance of <see cref="IIncrementalGenerator"/> for the current test.
	/// </summary>
	protected abstract IIncrementalGenerator CreateGenerator();
}
