using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Tests
{
	/// <inheritdoc cref="GeneratorTest"/>
	/// <typeparam name="TGenerator">Type of <see cref="ISourceGenerator"/> to test.</typeparam>
	public abstract class GeneratorTest<TGenerator> : GeneratorTest where TGenerator : ISourceGenerator, new()
	{
		/// <inheritdoc cref="GeneratorTest.Generator"/>
		public new TGenerator Generator => (TGenerator)base.Generator;

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneratorTest{TGenerator}"/> class.
		/// </summary>
		protected GeneratorTest(TGenerator generator) : base(generator)
		{
		}
	}

	/// <summary>
	/// An abstract class that provides methods to test <see cref="ISourceGenerator"/>s.
	/// </summary>
	public abstract class GeneratorTest
	{
		/// <summary>
		/// An <see cref="ISourceGenerator"/> that is being tested.
		/// </summary>
		public ISourceGenerator Generator { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneratorTest"/> class.
		/// </summary>
		/// <param name="generator"><see cref="ISourceGenerator"/> to test.</param>
		protected GeneratorTest(ISourceGenerator generator)
		{
			Generator = generator;
		}

		/// <summary>
		/// Returns a <see cref="SingletonGeneratorTestResult"/> created by performing a test on the target <see cref="Generator"/>.
		/// </summary>
		/// <param name="input">Input for the generator.</param>
		public virtual SingletonGeneratorTestResult RunGenerator(string? input)
		{
			return RunGenerator(input, Generator);
		}

		/// <summary>
		/// Returns a <see cref="SingletonGeneratorTestResult"/> created by performing a test on the target <see cref="Generator"/>.
		/// </summary>
		/// <param name="input">Input for the generator.</param>
		/// <param name="index">Index of the source in the generator's output.</param>
		public virtual SingletonGeneratorTestResult RunGenerator(string? input, int index)
		{
			return RunGenerator(input, Generator, index);
		}

		/// <summary>
		/// Returns a <see cref="SingletonGeneratorTestResult"/> created by performing a test on the target <paramref name="sourceGenerator"/>.
		/// </summary>
		/// <param name="input">Input for the generator.</param>
		/// <param name="sourceGenerator"><see cref="ISourceGenerator"/> to perform the test on.</param>
		public static SingletonGeneratorTestResult RunGenerator(string? input, ISourceGenerator sourceGenerator)
		{
			if (sourceGenerator is null)
			{
				return default;
			}

			return sourceGenerator.RunTest(SingletonGeneratorTestResult.Create, input);
		}

		/// <summary>
		/// Returns a <see cref="SingletonGeneratorTestResult"/> created by performing a test on the target <paramref name="sourceGenerator"/>.
		/// </summary>
		/// <param name="input">Input for the generator.</param>
		/// <param name="sourceGenerator"><see cref="ISourceGenerator"/> to perform the test on.</param>
		/// <param name="index">Index of the source in the generator's output.</param>
		public static SingletonGeneratorTestResult RunGenerator(string? input, ISourceGenerator sourceGenerator, int index)
		{
			if (sourceGenerator is null)
			{
				return default;
			}

			return sourceGenerator.RunTest(
				(CSharpGeneratorDriver generatorDriver, CSharpCompilation inputCompilation, CSharpCompilation outputCompilation)
					=> new SingletonGeneratorTestResult(generatorDriver, inputCompilation, outputCompilation, index), input);
		}
	}
}
