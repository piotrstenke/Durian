// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Durian.Analysis;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.TestServices
{
	/// <summary>
	/// An abstract class that provides methods to test <see cref="ISourceGenerator"/>s.
	/// </summary>
	public abstract class GeneratorTest
	{
		/// <summary>
		/// An <see cref="ISourceGenerator"/> that is being tested.
		/// </summary>
		public ISourceGenerator UnderlayingGenerator { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneratorTest"/> class.
		/// </summary>
		protected GeneratorTest()
		{
			UnderlayingGenerator = CreateGenerator();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneratorTest"/> class.
		/// </summary>
		/// <param name="enableDiagnostics">Determines whether to enable diagnostics for the created <see cref="ISourceGenerator"/> if it supports any.</param>
		protected GeneratorTest(bool enableDiagnostics)
		{
			ISourceGenerator generator = CreateGenerator();

			if (enableDiagnostics && generator is IDurianGenerator g && g.LogHandler is not null)
			{
				g.LogHandler.EnableDiagnosticsIfSupported();
			}

			UnderlayingGenerator = generator;
		}

		/// <summary>
		/// Returns a <see cref="SingleGeneratorTestResult"/> created by performing a test on the target <paramref name="generator"/>.
		/// </summary>
		/// <param name="generator"><see cref="ISourceGenerator"/> to perform the test on.</param>
		/// <param name="input">Input for the generator.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		public static SingleGeneratorTestResult RunGenerator(ISourceGenerator generator, string? input)
		{
			return generator.RunTest(SingleGeneratorTestResult.Create, input);
		}

		/// <summary>
		/// Returns a <see cref="SingleGeneratorTestResult"/> created by performing a test on the target <paramref name="generator"/>.
		/// </summary>
		/// <param name="generator"><see cref="ISourceGenerator"/> to perform the test on.</param>
		/// <param name="input">Input for the generator.</param>
		/// <param name="index">Index of the source in the generator's output.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		public static SingleGeneratorTestResult RunGenerator(ISourceGenerator generator, string? input, int index)
		{
			return generator.RunTest(
				(CSharpGeneratorDriver generatorDriver, CSharpCompilation inputCompilation, CSharpCompilation outputCompilation)
					=> SingleGeneratorTestResult.Create(generatorDriver, inputCompilation, outputCompilation, index), input);
		}

		/// <summary>
		/// Returns a <see cref="MultipleGeneratorTestResult"/> created by performing a test on the target <paramref name="generator"/>.
		/// </summary>
		/// <param name="generator"><see cref="ISourceGenerator"/> to perform the test on.</param>
		/// <param name="input">Input for the generator.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		public static MultipleGeneratorTestResult RunGeneratorWithMultipleOutputs(ISourceGenerator generator, string? input)
		{
			return generator.RunTest(MultipleGeneratorTestResult.Create, input);
		}

		/// <summary>
		/// Returns a <see cref="MultipleGeneratorTestResult"/> created by performing a test on the target <paramref name="generator"/>.
		/// </summary>
		/// <param name="generator"><see cref="ISourceGenerator"/> to perform the test on.</param>
		/// <param name="input">Input for the generator.</param>
		/// <param name="startIndex">Number of generated sources to skip.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		public static MultipleGeneratorTestResult RunGeneratorWithMultipleOutputs(ISourceGenerator generator, string? input, int startIndex)
		{
			return generator.RunTest(
				(CSharpGeneratorDriver generatorDriver, CSharpCompilation inputCompilation, CSharpCompilation outputCompilation)
					=> MultipleGeneratorTestResult.Create(generatorDriver, inputCompilation, outputCompilation, startIndex), input);
		}

		/// <summary>
		/// Returns a <see cref="SingleGeneratorTestResult"/> created by performing a test on the target <paramref name="generator"/>.
		/// </summary>
		/// <param name="generator"><see cref="ISourceGenerator"/> to perform the test on.</param>
		/// <param name="input">Input for the generator.</param>
		/// <param name="external">Code in external assembly that is referenced by assembly containing the <paramref name="input"/> text.</param>
		public static SingleGeneratorTestResult RunGeneratorWithDependency(ISourceGenerator generator, string? input, string? external)
		{
			MetadataReference reference = RoslynUtilities.CreateReference(external);
			return generator.RunTest(SingleGeneratorTestResult.Create, input, reference);
		}

		/// <summary>
		/// Returns a <see cref="SingleGeneratorTestResult"/> created by performing a test on the target <paramref name="generator"/>.
		/// </summary>
		/// <param name="generator"><see cref="ISourceGenerator"/> to perform the test on.</param>
		/// <param name="input">Input for the generator.</param>
		/// <param name="external">Code in external assembly that is referenced by assembly containing the <paramref name="input"/> text.</param>
		/// <param name="index">Index of the source in the generator's output.</param>
		public static SingleGeneratorTestResult RunGeneratorWithDependency(ISourceGenerator generator, string? input, string? external, int index)
		{
			MetadataReference reference = RoslynUtilities.CreateReference(external);
			return generator.RunTest((CSharpGeneratorDriver generatorDriver, CSharpCompilation inputCompilation, CSharpCompilation outputCompilation)
					=> SingleGeneratorTestResult.Create(generatorDriver, inputCompilation, outputCompilation, index), input, reference);
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
		/// Creates a new instance of <see cref="ISourceGenerator"/> for the current test.
		/// </summary>
		protected abstract ISourceGenerator CreateGenerator();
	}
}
