// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Immutable;
using System.Text;

namespace Durian.TestServices
{
	/// <summary>
	/// A <see cref="IGeneratorTestResult"/> that represents multiple sources that were created during the generator pass.
	/// </summary>
	public readonly struct MultiOutputGeneratorTestResult : IGeneratorTestResult
	{
		private readonly GeneratorRunResult _runResult;

		/// <inheritdoc/>
		public ImmutableArray<Diagnostic> Diagnostics => _runResult.Diagnostics;

		/// <inheritdoc/>
		public Exception? Exception => _runResult.Exception;

		/// <summary>
		/// A collection of <see cref="GeneratedSourceResult"/> that act as the output of the <see cref="Generator"/>.
		/// </summary>
		public ImmutableArray<GeneratedSourceResult> GeneratedSources { get; }

		/// <inheritdoc/>
		public ISourceGenerator Generator => _runResult.Generator;

		/// <inheritdoc/>
		public CSharpCompilation InputCompilation { get; }

		/// <inheritdoc/>
		public bool IsGenerated => Exception is null && GeneratedSources.Length > 0;

		/// <summary>
		/// Number of <see cref="GeneratedSourceResult"/> that were created by the <see cref="Generator"/>.
		/// </summary>
		public int Length => GeneratedSources.Length;

		/// <inheritdoc/>
		public CSharpCompilation OutputCompilation { get; }

		/// <summary>
		/// Returns a <see cref="GeneratedSourceResult"/> at the specified <paramref name="index"/>.
		/// </summary>
		/// <param name="index">Index to get the <see cref="GeneratedSourceResult"/> at.</param>
		/// <exception cref="IndexOutOfRangeException">The specified index is not in the array.</exception>
		public GeneratedSourceResult this[int index] => GeneratedSources[index];

		private MultiOutputGeneratorTestResult(
			GeneratorRunResult runResult,
			ImmutableArray<GeneratedSourceResult> generatesSources,
			CSharpCompilation inputCompilation,
			CSharpCompilation outputCompilation
		)
		{
			_runResult = runResult;
			GeneratedSources = generatesSources;
			InputCompilation = inputCompilation;
			OutputCompilation = outputCompilation;
		}

		/// <inheritdoc cref="Create(GeneratorDriver, CSharpCompilation, CSharpCompilation, int)"/>
		public static MultiOutputGeneratorTestResult Create(GeneratorDriver generatorDriver, CSharpCompilation inputCompilation, CSharpCompilation outputCompilation)
		{
			return Create(generatorDriver, inputCompilation, outputCompilation, 0);
		}

		/// <summary>
		/// Creates a new <see cref="MultiOutputGeneratorTestResult"/> from the specified <paramref name="generatorDriver"/>.
		/// </summary>
		/// <param name="generatorDriver"><see cref="CSharpGeneratorDriver"/> that was used to run the generator test.</param>
		/// <param name="inputCompilation"><see cref="CSharpCompilation"/> that was passed as input to the <paramref name="generatorDriver"/>.</param>
		/// <param name="outputCompilation"><see cref="CSharpCompilation"/> that was returned by the <paramref name="generatorDriver"/>.</param>
		/// <param name="startIndex">Number of generated sources to skip.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="generatorDriver"/> is <see langdword="null"/>. -or-
		/// <paramref name="inputCompilation"/> is <see langword="null"/>. -or-
		/// <paramref name="outputCompilation"/> is <see langword="null"/>.
		/// </exception>
		public static MultiOutputGeneratorTestResult Create(GeneratorDriver generatorDriver, CSharpCompilation inputCompilation, CSharpCompilation outputCompilation, int startIndex)
		{
			if (generatorDriver is null)
			{
				throw new ArgumentNullException(nameof(generatorDriver));
			}

			if (inputCompilation is null)
			{
				throw new ArgumentNullException(nameof(inputCompilation));
			}

			if (outputCompilation is null)
			{
				throw new ArgumentNullException(nameof(outputCompilation));
			}

			GeneratorRunResult runResult = generatorDriver.GetRunResult().Results[0];
			ImmutableArray<GeneratedSourceResult> generatedSources = runResult.GeneratedSources;

			if(startIndex >= generatedSources.Length)
			{
				generatedSources = ImmutableArray.Create<GeneratedSourceResult>();
			}
			else if(startIndex > 0)
			{
				generatedSources = generatedSources.RemoveRange(0, startIndex);
			}

			return new MultiOutputGeneratorTestResult(runResult, generatedSources, inputCompilation, outputCompilation);
		}

		/// <summary>
		/// Checks if the <see cref="CSharpSyntaxTree"/>s in <see cref="GeneratedSources"/> are equivalent to the given array of <paramref name="expected"/>.
		/// </summary>
		/// <remarks>
		/// If <paramref name="expected"/> is <see langword="null"/>, empty, or it's length is not equal to that of <see cref="GeneratedSources"/>, <see langword="false"/> is returned.
		/// </remarks>
		/// <param name="expected">Array of <see cref="string"/>s representing <see cref="CSharpSyntaxTree"/>s that were expected to be generated.</param>
		public bool Compare(params string[]? expected)
		{
			if (expected is null || expected.Length == 0 || expected.Length != Length)
			{
				return false;
			}

			ImmutableArray<GeneratedSourceResult> generatedSources = GeneratedSources;

			for (int i = 0; i < expected.Length; i++)
			{
				if (!generatedSources[0].SyntaxTree.IsEquivalentTo(CSharpSyntaxTree.ParseText(expected[0], encoding: Encoding.UTF8)))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Checks if the <see cref="CSharpSyntaxTree"/>s in <see cref="GeneratedSources"/> are equivalent to the given array of <paramref name="syntaxTrees"/>.
		/// </summary>
		/// <remarks>
		/// If <paramref name="syntaxTrees"/> is <see langword="null"/>, empty, or it's length is not equal to that of <see cref="GeneratedSources"/>, <see langword="false"/> is returned.
		/// </remarks>
		/// <param name="syntaxTrees">Array of <see cref="CSharpSyntaxTree"/>s to compare.</param>
		public bool Compare(params CSharpSyntaxTree[]? syntaxTrees)
		{
			if (syntaxTrees is null || syntaxTrees.Length == 0 || syntaxTrees.Length != Length)
			{
				return false;
			}

			ImmutableArray<GeneratedSourceResult> generatedSources = GeneratedSources;

			for (int i = 0; i < syntaxTrees.Length; i++)
			{
				if(!generatedSources[0].SyntaxTree.IsEquivalentTo(syntaxTrees[0]))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Checks if the <see cref="CSharpSyntaxTree"/> at the specified <paramref name="index"/> in <see cref="GeneratedSources"/> is equivalent to a <see cref="CSharpSyntaxTree"/> created from the given <paramref name="expected"/> source.
		/// </summary>
		/// <param name="index">Index at which the <see cref="CSharpSyntaxTree"/> to compare is located at.</param>
		/// <param name="expected">A <see cref="string"/> that represents a <see cref="CSharpSyntaxTree"/> that was expected to be generated.</param>
		public bool Compare(int index, string? expected)
		{
			if (index < 0 && index > Length)
			{
				return false;
			}

			if (expected is null)
			{
				return false;
			}

			return Compare_Internal(CSharpSyntaxTree.ParseText(expected, encoding: Encoding.UTF8), index);
		}

		/// <summary>
		/// Checks if the <see cref="CSharpSyntaxTree"/> at the specified <paramref name="index"/> in <see cref="GeneratedSources"/> is equivalent to the given <paramref name="syntaxTree"/>.
		/// </summary>
		/// <param name="index">Index at which the <see cref="CSharpSyntaxTree"/> to compare is located at.</param>
		/// <param name="syntaxTree"><see cref="CSharpSyntaxTree"/> to compare.</param>
		public bool Compare(int index, CSharpSyntaxTree? syntaxTree)
		{
			if (index < 0 && index > Length)
			{
				return false;
			}

			return Compare_Internal(syntaxTree, index);
		}

		bool IGeneratorTestResult.Compare(GeneratorDriverRunResult result)
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

		private bool Compare_Internal(SyntaxTree? syntaxTree, int index)
		{
			if (syntaxTree is null)
			{
				return false;
			}

			return GeneratedSources[index].SyntaxTree.IsEquivalentTo(syntaxTree);
		}
	}
}