// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis;
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
        public ISourceGenerator Generator { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratorTest"/> class.
        /// </summary>
        protected GeneratorTest()
        {
            Generator = CreateGenerator();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratorTest"/> class.
        /// </summary>
        /// <param name="enableDiagnostics">Determines whether to enable diagnostics for the created <see cref="ISourceGenerator"/> if it supports any.</param>
        protected GeneratorTest(bool enableDiagnostics)
        {
            ISourceGenerator generator = CreateGenerator();

            if (enableDiagnostics && generator is IDurianGenerator g && g.SupportsDiagnostics)
            {
                g.EnableDiagnostics = true;
            }

            Generator = generator;
        }

        /// <summary>
        /// Returns a <see cref="SingletonGeneratorTestResult"/> created by performing a test on the target <paramref name="generator"/>.
        /// </summary>
        /// <param name="generator"><see cref="ISourceGenerator"/> to perform the test on.</param>
        /// <param name="input">Input for the generator.</param>
        public static SingletonGeneratorTestResult RunGenerator(ISourceGenerator generator, string? input)
        {
            if (generator is null)
            {
                return default;
            }

            return generator.RunTest(SingletonGeneratorTestResult.Create, input);
        }

        /// <summary>
        /// Returns a <see cref="SingletonGeneratorTestResult"/> created by performing a test on the target <paramref name="generator"/>.
        /// </summary>
        /// <param name="generator"><see cref="ISourceGenerator"/> to perform the test on.</param>
        /// <param name="input">Input for the generator.</param>
        /// <param name="index">Index of the source in the generator's output.</param>
        public static SingletonGeneratorTestResult RunGenerator(ISourceGenerator generator, string? input, int index)
        {
            if (generator is null)
            {
                return default;
            }

            return generator.RunTest(
                (CSharpGeneratorDriver generatorDriver, CSharpCompilation inputCompilation, CSharpCompilation outputCompilation)
                    => new SingletonGeneratorTestResult(generatorDriver, inputCompilation, outputCompilation, index), input);
        }

        /// <summary>
        /// Returns a <see cref="SingletonGeneratorTestResult"/> created by performing a test on the target <paramref name="generator"/>.
        /// </summary>
        /// <param name="generator"><see cref="ISourceGenerator"/> to perform the test on.</param>
        /// <param name="input">Input for the generator.</param>
        /// <param name="external">Code in external assembly that is referenced by assembly containing the <paramref name="input"/> text.</param>
        public static SingletonGeneratorTestResult RunGeneratorWithDependency(ISourceGenerator generator, string? input, string? external)
        {
            if (generator is null)
            {
                return default;
            }

            MetadataReference reference = RoslynUtilities.CreateReference(external);
            return generator.RunTest(SingletonGeneratorTestResult.Create, input, reference);
        }

        /// <summary>
        /// Returns a <see cref="SingletonGeneratorTestResult"/> created by performing a test on the target <paramref name="generator"/>.
        /// </summary>
        /// <param name="generator"><see cref="ISourceGenerator"/> to perform the test on.</param>
        /// <param name="input">Input for the generator.</param>
        /// <param name="external">Code in external assembly that is referenced by assembly containing the <paramref name="input"/> text.</param>
        /// <param name="index">Index of the source in the generator's output.</param>
        public static SingletonGeneratorTestResult RunGeneratorWithDependency(ISourceGenerator generator, string? input, string? external, int index)
        {
            if (generator is null)
            {
                return default;
            }

            MetadataReference reference = RoslynUtilities.CreateReference(external);
            return generator.RunTest((CSharpGeneratorDriver generatorDriver, CSharpCompilation inputCompilation, CSharpCompilation outputCompilation)
                    => new SingletonGeneratorTestResult(generatorDriver, inputCompilation, outputCompilation, index), input, reference);
        }

        /// <summary>
        /// Returns a <see cref="SingletonGeneratorTestResult"/> created by performing a test on the target <see cref="Generator"/>.
        /// </summary>
        /// <param name="input">Input for the generator.</param>
        public virtual SingletonGeneratorTestResult RunGenerator(string? input)
        {
            return RunGenerator(Generator, input);
        }

        /// <summary>
        /// Returns a <see cref="SingletonGeneratorTestResult"/> created by performing a test on the target <see cref="Generator"/>.
        /// </summary>
        /// <param name="input">Input for the generator.</param>
        /// <param name="index">Index of the source in the generator's output.</param>
        public virtual SingletonGeneratorTestResult RunGenerator(string? input, int index)
        {
            return RunGenerator(Generator, input, index);
        }

        /// <summary>
        /// Returns a <see cref="SingletonGeneratorTestResult"/> created by performing a test on the target <see cref="Generator"/>.
        /// </summary>
        /// <param name="input">Input for the generator.</param>
        /// <param name="external">Code in external assembly that is referenced by assembly containing the <paramref name="input"/> text.</param>
        public virtual SingletonGeneratorTestResult RunGeneratorWithDependency(string? input, string? external)
        {
            return RunGeneratorWithDependency(Generator, input, external);
        }

        /// <summary>
        /// Returns a <see cref="SingletonGeneratorTestResult"/> created by performing a test on the target <see cref="Generator"/>.
        /// </summary>
        /// <param name="input">Input for the generator.</param>
        /// <param name="external">Code in external assembly that is referenced by assembly containing the <paramref name="input"/> text.</param>
        /// <param name="index">Index of the source in the generator's output.</param>
        public virtual SingletonGeneratorTestResult RunGeneratorWithDependency(string? input, string? external, int index)
        {
            return RunGeneratorWithDependency(Generator, input, external, index);
        }

        /// <summary>
        /// Creates a new instance of <see cref="ISourceGenerator"/> for the current test.
        /// </summary>
        protected abstract ISourceGenerator CreateGenerator();
    }
}