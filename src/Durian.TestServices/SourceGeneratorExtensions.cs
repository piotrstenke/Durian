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
	/// Contains various extensions methods for the <see cref="ISourceGenerator"/> interface.
	/// </summary>
	public static class SourceGeneratorExtensions
	{
		/// <summary>
		/// Performs a test of the specified <paramref name="generator"/>.
		/// </summary>
		/// <param name="generator"><see cref="ISourceGenerator"/> to run a test of.</param>
		/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
		/// <param name="source">A <see cref="string"/> that will be parsed as a <see cref="CSharpSyntaxTree"/> and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
		/// <param name="assemblies">An array of <see cref="Assembly"/> instances to be referenced by the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
		public static IGeneratorTestResult RunTest(this ISourceGenerator generator, GeneratorTestResultProvider resultProvider, string? source, params Assembly[]? assemblies)
		{
			if (generator is null)
			{
				throw new ArgumentNullException(nameof(generator));
			}

			CSharpCompilation compilation = RoslynUtilities.CreateCompilationWithAssemblies(source, assemblies);
			return CSharpGeneratorDriver.Create(generator).RunTest(resultProvider, compilation);
		}

		/// <summary>
		/// Performs a test of the specified <paramref name="generator"/>.
		/// </summary>
		/// <param name="generator"><see cref="ISourceGenerator"/> to run a test of.</param>
		/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
		/// <param name="sources">A collection of <see cref="string"/>s that will be parsed as <see cref="CSharpSyntaxTree"/>s and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
		/// <param name="assemblies">An array of <see cref="Assembly"/> instances to be referenced by the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
		public static IGeneratorTestResult RunTest(this ISourceGenerator generator, GeneratorTestResultProvider resultProvider, IEnumerable<string>? sources, params Assembly[]? assemblies)
		{
			if (generator is null)
			{
				throw new ArgumentNullException(nameof(generator));
			}

			CSharpCompilation compilation = RoslynUtilities.CreateCompilationWithAssemblies(sources, assemblies);
			return CSharpGeneratorDriver.Create(generator).RunTest(resultProvider, compilation);
		}

		/// <summary>
		/// Performs a test of the specified <paramref name="generator"/>.
		/// </summary>
		/// <param name="generator"><see cref="ISourceGenerator"/> to run a test of.</param>
		/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
		/// <param name="compilation">A <see cref="CSharpCompilation"/> to be used a input of the tested <paramref name="generator"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static IGeneratorTestResult RunTest(this ISourceGenerator generator, GeneratorTestResultProvider resultProvider, CSharpCompilation compilation)
		{
			if (generator is null)
			{
				throw new ArgumentNullException(nameof(generator));
			}

			return CSharpGeneratorDriver.Create(generator).RunTest(resultProvider, compilation);
		}

		/// <summary>
		/// Performs a test of the specified <paramref name="generator"/>.
		/// </summary>
		/// <param name="generator"><see cref="ISourceGenerator"/> to run a test of.</param>
		/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
		/// <param name="source">A <see cref="string"/> that will be parsed as a <see cref="CSharpSyntaxTree"/> and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
		/// <param name="assemblies">An array of <see cref="Assembly"/> instances to be referenced by the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
		/// <typeparam name="TResult">Type of <see cref="IGeneratorTestResult"/> to create after the test is run.</typeparam>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
		public static TResult RunTest<TResult>(this ISourceGenerator generator, GeneratorTestResultProvider<TResult> resultProvider, string? source, params Assembly[]? assemblies) where TResult : IGeneratorTestResult
		{
			if (generator is null)
			{
				throw new ArgumentNullException(nameof(generator));
			}

			CSharpCompilation compilation = RoslynUtilities.CreateCompilationWithAssemblies(source, assemblies);
			return CSharpGeneratorDriver.Create(generator).RunTest(resultProvider, compilation);
		}

		/// <summary>
		/// Performs a test of the specified <paramref name="generator"/>.
		/// </summary>
		/// <param name="generator"><see cref="ISourceGenerator"/> to run a test of.</param>
		/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> of type <typeparamref name="TResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
		/// <param name="sources">A collection of <see cref="string"/>s that will be parsed as <see cref="CSharpSyntaxTree"/>s and added to the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
		/// <param name="assemblies">An array of <see cref="Assembly"/> instances to be referenced by the input <see cref="CSharpCompilation"/> of the tested <paramref name="generator"/>.</param>
		/// <typeparam name="TResult">Type of <see cref="IGeneratorTestResult"/> to create after the test is run.</typeparam>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>.</exception>
		public static TResult RunTest<TResult>(this ISourceGenerator generator, GeneratorTestResultProvider<TResult> resultProvider, IEnumerable<string>? sources, params Assembly[]? assemblies) where TResult : IGeneratorTestResult
		{
			if (generator is null)
			{
				throw new ArgumentNullException(nameof(generator));
			}

			CSharpCompilation compilation = RoslynUtilities.CreateCompilationWithAssemblies(sources, assemblies);
			return CSharpGeneratorDriver.Create(generator).RunTest(resultProvider, compilation);
		}

		/// <summary>
		/// Performs a test of the specified <paramref name="generator"/>.
		/// </summary>
		/// <param name="generator"><see cref="ISourceGenerator"/> to run a test of.</param>
		/// <param name="resultProvider">A delegate that creates the target <see cref="IGeneratorTestResult"/> of type <typeparamref name="TResult"/> from the data provided by the tested <paramref name="generator"/>.</param>
		/// <param name="compilation">A <see cref="CSharpCompilation"/> to be used a input of the tested <paramref name="generator"/>.</param>
		/// <typeparam name="TResult">Type of <see cref="IGeneratorTestResult"/> to create after the test is run.</typeparam>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="resultProvider"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static TResult RunTest<TResult>(this ISourceGenerator generator, GeneratorTestResultProvider<TResult> resultProvider, CSharpCompilation compilation) where TResult : IGeneratorTestResult
		{
			if (generator is null)
			{
				throw new ArgumentNullException(nameof(generator));
			}

			return CSharpGeneratorDriver.Create(generator).RunTest(resultProvider, compilation);
		}
	}
}
