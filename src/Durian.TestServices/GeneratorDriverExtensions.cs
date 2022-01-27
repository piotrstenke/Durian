// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.TestServices
{
	/// <summary>
	/// Contains various extensions methods for the <see cref="CSharpGeneratorDriver"/> class.
	/// </summary>
	public static class GeneratorDriverExtensions
	{
		/// <summary>
		/// Performs a test on all <see cref="ISourceGenerator"/>s registered in the provided <paramref name="generatorDriver"/>.
		/// </summary>
		/// <param name="generatorDriver"><see cref="CSharpGeneratorDriver"/> to run a tests on all registered <see cref="ISourceGenerator"/> of.</param>
		/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generatorDriver"/>.</param>
		/// <param name="source">A <see cref="string"/> that will be parsed as a <see cref="CSharpSyntaxTree"/> and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generatorDriver"/>.</param>
		/// <param name="assemblies">An array of <see cref="Assembly"/> instances to be referenced by the input <see cref="CSharpCompilation"/> of the tested <paramref name="generatorDriver"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generatorDriver"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
		public static IGeneratorTestResult RunTest(this CSharpGeneratorDriver generatorDriver, GeneratorTestResultProvider resultProvider, string? source, params Assembly[]? assemblies)
		{
			return RunTest(generatorDriver, resultProvider, RoslynUtilities.CreateCompilationWithAssemblies(source, assemblies));
		}

		/// <summary>
		/// Performs a test on all <see cref="ISourceGenerator"/>s registered in the provided <paramref name="generatorDriver"/>.
		/// </summary>
		/// <param name="generatorDriver"><see cref="CSharpGeneratorDriver"/> to run a tests on all registered <see cref="ISourceGenerator"/> of.</param>
		/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generatorDriver"/>.</param>
		/// <param name="sources">A collection of <see cref="string"/>s that will be parsed as <see cref="CSharpSyntaxTree"/>s and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generatorDriver"/>.</param>
		/// <param name="assemblies">An array of <see cref="Assembly"/> instances to be referenced by the input <see cref="CSharpCompilation"/> of the tested <paramref name="generatorDriver"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generatorDriver"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
		public static IGeneratorTestResult RunTest(this CSharpGeneratorDriver generatorDriver, GeneratorTestResultProvider resultProvider, IEnumerable<string>? sources, params Assembly[]? assemblies)
		{
			return RunTest(generatorDriver, resultProvider, RoslynUtilities.CreateCompilationWithAssemblies(sources, assemblies));
		}

		/// <summary>
		/// Performs a test on all <see cref="ISourceGenerator"/>s registered in the provided <paramref name="generatorDriver"/>.
		/// </summary>
		/// <param name="generatorDriver"><see cref="CSharpGeneratorDriver"/> to run a tests on all registered <see cref="ISourceGenerator"/> of.</param>
		/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generatorDriver"/>.</param>
		/// <param name="compilation">A <see cref="CSharpCompilation"/> to be used a input of the tested <paramref name="generatorDriver"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generatorDriver"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static IGeneratorTestResult RunTest(this CSharpGeneratorDriver generatorDriver, GeneratorTestResultProvider resultProvider, CSharpCompilation compilation)
		{
			if (generatorDriver is null)
			{
				throw new ArgumentNullException(nameof(generatorDriver));
			}

			if (resultProvider is null)
			{
				throw new ArgumentNullException(nameof(resultProvider));
			}

			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			CSharpGeneratorDriver driver = generatorDriver;
			driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation outputCompilation, out _);
			return resultProvider(driver, compilation, (CSharpCompilation)outputCompilation);
		}

		/// <summary>
		/// Performs a test on all <see cref="ISourceGenerator"/>s registered in the provided <paramref name="generatorDriver"/>.
		/// </summary>
		/// <param name="generatorDriver"><see cref="CSharpGeneratorDriver"/> to run a tests on all registered <see cref="ISourceGenerator"/> of.</param>
		/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generatorDriver"/>.</param>
		/// <param name="source">A <see cref="string"/> that will be parsed as a <see cref="CSharpSyntaxTree"/> and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generatorDriver"/>.</param>
		/// <param name="assemblies">An array of <see cref="Assembly"/> instances to be referenced by the input <see cref="CSharpCompilation"/> of the tested <paramref name="generatorDriver"/>.</param>
		/// <typeparam name="TResult">Type of <see cref="IGeneratorTestResult"/> to create after the test is run.</typeparam>
		/// <exception cref="ArgumentNullException"><paramref name="generatorDriver"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
		public static TResult RunTest<TResult>(this CSharpGeneratorDriver generatorDriver, GeneratorTestResultProvider<TResult> resultProvider, string? source, params Assembly[]? assemblies) where TResult : IGeneratorTestResult
		{
			return RunTest(generatorDriver, resultProvider, RoslynUtilities.CreateCompilationWithAssemblies(source, assemblies));
		}

		/// <summary>
		/// Performs a test on all <see cref="ISourceGenerator"/>s registered in the provided <paramref name="generatorDriver"/>.
		/// </summary>
		/// <param name="generatorDriver"><see cref="CSharpGeneratorDriver"/> to run a tests on all registered <see cref="ISourceGenerator"/> of.</param>
		/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generatorDriver"/>.</param>
		/// <param name="sources">A collection of <see cref="string"/>s that will be parsed as <see cref="CSharpSyntaxTree"/>s and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generatorDriver"/>.</param>
		/// <param name="assemblies">An array of <see cref="Assembly"/> instances to be referenced by the input <see cref="CSharpCompilation"/> of the tested <paramref name="generatorDriver"/>.</param>
		/// <typeparam name="TResult">Type of <see cref="IGeneratorTestResult"/> to create after the test is run.</typeparam>
		/// <exception cref="ArgumentNullException"><paramref name="generatorDriver"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
		public static TResult RunTest<TResult>(this CSharpGeneratorDriver generatorDriver, GeneratorTestResultProvider<TResult> resultProvider, IEnumerable<string>? sources, params Assembly[]? assemblies) where TResult : IGeneratorTestResult
		{
			return RunTest(generatorDriver, resultProvider, RoslynUtilities.CreateCompilationWithAssemblies(sources, assemblies));
		}

		/// <summary>
		/// Performs a test on all <see cref="ISourceGenerator"/>s registered in the provided <paramref name="generatorDriver"/>.
		/// </summary>
		/// <param name="generatorDriver"><see cref="CSharpGeneratorDriver"/> to run a tests on all registered <see cref="ISourceGenerator"/> of.</param>
		/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generatorDriver"/>.</param>
		/// <param name="compilation">A <see cref="CSharpCompilation"/> to be used a input of the tested <paramref name="generatorDriver"/>.</param>
		/// <typeparam name="TResult">Type of <see cref="IGeneratorTestResult"/> to create after the test is run.</typeparam>
		/// <exception cref="ArgumentNullException"><paramref name="generatorDriver"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static TResult RunTest<TResult>(this CSharpGeneratorDriver generatorDriver, GeneratorTestResultProvider<TResult> resultProvider, CSharpCompilation compilation) where TResult : IGeneratorTestResult
		{
			if (generatorDriver is null)
			{
				throw new ArgumentNullException(nameof(generatorDriver));
			}

			if (resultProvider is null)
			{
				throw new ArgumentNullException(nameof(resultProvider));
			}

			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			CSharpGeneratorDriver driver = generatorDriver;
			driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation outputCompilation, out _);
			return resultProvider(driver, compilation, (CSharpCompilation)outputCompilation);
		}
	}
}