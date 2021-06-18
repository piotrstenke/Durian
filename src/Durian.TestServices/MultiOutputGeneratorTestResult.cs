// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.TestServices
{
	/// <summary>
	/// A <see cref="IGeneratorTestResult"/> that represents multiple sources that were created during the generator pass.
	/// </summary>
	public readonly struct MultiOutputGeneratorTestResult : IGeneratorTestResult
	{
		private readonly GeneratorRunResult _runResult;

		/// <inheritdoc/>
		public readonly CSharpCompilation InputCompilation { get; }

		/// <inheritdoc/>
		public readonly bool IsGenerated => Exception is null && GeneratedSources.Length > 0;

		/// <inheritdoc/>
		public readonly CSharpCompilation OutputCompilation { get; }

		/// <inheritdoc/>
		public readonly ImmutableArray<Diagnostic> Diagnostics => _runResult.Diagnostics;

		/// <inheritdoc/>
		public readonly Exception? Exception => _runResult.Exception;

		/// <summary>
		/// A collection of <see cref="GeneratedSourceResult"/> that act as the output of the <see cref="Generator"/>.
		/// </summary>
		public readonly ImmutableArray<GeneratedSourceResult> GeneratedSources => _runResult.GeneratedSources;

		/// <inheritdoc/>
		public readonly ISourceGenerator Generator => _runResult.Generator;

		/// <summary>
		/// Number of <see cref="GeneratedSourceResult"/> that were created by the <see cref="Generator"/>.
		/// </summary>
		public readonly int Length => GeneratedSources.Length;

		/// <summary>
		/// Initializes a new instance of the <see cref="MultiOutputGeneratorTestResult"/> struct.
		/// </summary>
		/// <param name="generatorDriver">A <see cref="CSharpGeneratorDriver"/> that was used to perform the test.</param>
		/// <param name="inputCompilation">A <see cref="CSharpCompilation"/> that represent an input for the tested <see cref="ISourceGenerator"/>.</param>
		/// <param name="outputCompilation">A <see cref="CSharpCompilation"/> that was created by the tested <see cref="ISourceGenerator"/>.</param>
		public MultiOutputGeneratorTestResult(GeneratorDriver generatorDriver, CSharpCompilation inputCompilation, CSharpCompilation outputCompilation)
		{
			InputCompilation = inputCompilation;
			OutputCompilation = outputCompilation;
			_runResult = generatorDriver.GetRunResult().Results[0];
		}

		/// <inheritdoc cref="MultiOutputGeneratorTestResult(GeneratorDriver, CSharpCompilation, CSharpCompilation)"/>
		public static MultiOutputGeneratorTestResult Create(GeneratorDriver generatorDriver, CSharpCompilation inputCompilation, CSharpCompilation outputCompilation)
		{
			return new MultiOutputGeneratorTestResult(generatorDriver, inputCompilation, outputCompilation);
		}

		/// <summary>
		/// Checks if the <see cref="CSharpSyntaxTree"/> at the specified <paramref name="index"/> in <see cref="GeneratedSources"/> is equivalent to a <see cref="CSharpSyntaxTree"/> created from the given <paramref name="expected"/> source.
		/// </summary>
		/// <param name="index">Index at which the <see cref="CSharpSyntaxTree"/> to compare is located at.</param>
		/// <param name="expected">A <see cref="string"/> that represents a <see cref="CSharpSyntaxTree"/> that was expected to be generated.</param>
		public readonly bool Compare(int index, string? expected)
		{
			if (index < 0 && index > Length)
			{
				return false;
			}

			if (expected is null)
			{
				return false;
			}

			return Compare_Internal(index, CSharpSyntaxTree.ParseText(expected, encoding: Encoding.UTF8));
		}

		/// <summary>
		/// Checks if the <see cref="CSharpSyntaxTree"/> at the specified <paramref name="index"/> in <see cref="GeneratedSources"/> is equivalent to the given <paramref name="syntaxTree"/>.
		/// </summary>
		/// <param name="index">Index at which the <see cref="CSharpSyntaxTree"/> to compare is located at.</param>
		/// <param name="syntaxTree"><see cref="CSharpSyntaxTree"/> to compare.</param>
		public readonly bool Compare(int index, CSharpSyntaxTree? syntaxTree)
		{
			if (index < 0 && index > Length)
			{
				return false;
			}

			return Compare_Internal(index, syntaxTree);
		}

		readonly bool IGeneratorTestResult.Compare(GeneratorDriverRunResult result)
		{
			if (result.GeneratedTrees.Length != Length)
			{
				return false;
			}

			int length = Length;

			for (int i = 0; i < length; i++)
			{
				if (!result.GeneratedTrees[i].IsEquivalentTo(GeneratedSources[i].SyntaxTree))
				{
					return false;
				}
			}

			return true;
		}

		private readonly bool Compare_Internal(int index, SyntaxTree? syntaxTree)
		{
			if (syntaxTree is null)
			{
				return false;
			}

			return GeneratedSources[index].SyntaxTree.IsEquivalentTo(syntaxTree);
		}
	}
}
