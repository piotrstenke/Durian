// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Durian.TestServices
{
	/// <summary>
	/// A <see cref="IGeneratorTestResult"/> that represents a single <see cref="CSharpSyntaxTree"/> retrieved from a tested <see cref="ISourceGenerator"/>.
	/// </summary>
	public readonly struct SingletonGeneratorTestResult : IGeneratorTestResult
	{
		private readonly GeneratorRunResult _runResult;
		private readonly GeneratedSourceResult _sourceResult;
		private readonly CSharpSyntaxTree? _tree;

		/// <inheritdoc/>
		public readonly CSharpCompilation InputCompilation { get; }

		/// <summary>
		/// Determines whether the <see cref="ISourceGenerator"/> actually generated any <see cref="CSharpSyntaxTree"/>.
		/// </summary>
		[MemberNotNullWhen(true, "SourceText", "SyntaxTree", "HintName")]
		public readonly bool IsGenerated { get; }

		/// <inheritdoc/>
		public readonly CSharpCompilation OutputCompilation { get; }

		/// <summary>
		/// A collection of <see cref="Diagnostic"/>s that were reported during the generator pass.
		/// </summary>
		public readonly ImmutableArray<Diagnostic> Diagnostics => _runResult.Diagnostics;

		/// <inheritdoc/>
		public readonly Exception? Exception => _runResult.Exception;

		/// <inheritdoc/>
		public readonly ISourceGenerator Generator => _runResult.Generator;

		/// <summary>
		/// An identifier provided by the generator that identifies the added <see cref="SourceText"/>
		/// </summary>
		public readonly string? HintName => _sourceResult.HintName;

		/// <summary>
		/// The <see cref="Microsoft.CodeAnalysis.Text.SourceText"/> that was added by the generator.
		/// </summary>
		public readonly SourceText? SourceText => _sourceResult.SourceText;

		/// <summary>
		/// A <see cref="CSharpSyntaxTree"/> that was generated during the generator pass.
		/// </summary>
		public readonly CSharpSyntaxTree? SyntaxTree => _sourceResult.SyntaxTree as CSharpSyntaxTree;

		/// <inheritdoc cref="SingletonGeneratorTestResult(CSharpGeneratorDriver, CSharpCompilation, CSharpCompilation, int)"/>
		public SingletonGeneratorTestResult(CSharpGeneratorDriver generatorDriver, CSharpCompilation inputCompilation, CSharpCompilation outputCompilation) : this(generatorDriver, inputCompilation, outputCompilation, 0)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SingletonGeneratorTestResult"/> struct.
		/// </summary>
		/// <param name="generatorDriver">A <see cref="CSharpGeneratorDriver"/> that was used to perform the test.</param>
		/// <param name="inputCompilation">A <see cref="CSharpCompilation"/> that represent an input for the tested <see cref="ISourceGenerator"/>.</param>
		/// <param name="outputCompilation">A <see cref="CSharpCompilation"/> that was created by the tested <see cref="ISourceGenerator"/>.</param>
		/// <param name="sourceIndex">Index of the target <see cref="CSharpSyntaxTree"/> in the generator's output.</param>
		public SingletonGeneratorTestResult(CSharpGeneratorDriver generatorDriver, CSharpCompilation inputCompilation, CSharpCompilation outputCompilation, int sourceIndex)
		{
			InputCompilation = inputCompilation;
			OutputCompilation = outputCompilation;
			_runResult = generatorDriver.GetRunResult().Results[0];
			bool isGenerated = _runResult.Exception is null && _runResult.GeneratedSources.Length > sourceIndex;

			if (isGenerated)
			{
				_sourceResult = _runResult.GeneratedSources[sourceIndex];
				_tree = _sourceResult.SyntaxTree as CSharpSyntaxTree;
				isGenerated = _tree is not null;
			}
			else
			{
				_sourceResult = default;
				_tree = null;
			}

			IsGenerated = isGenerated;
		}

		/// <inheritdoc cref="SingletonGeneratorTestResult(CSharpGeneratorDriver, CSharpCompilation, CSharpCompilation, int)"/>
		public static SingletonGeneratorTestResult Create(CSharpGeneratorDriver generatorDriver, CSharpCompilation inputCompilation, CSharpCompilation outputCompilation)
		{
			return new SingletonGeneratorTestResult(generatorDriver, inputCompilation, outputCompilation);
		}

		/// <inheritdoc cref="SingletonGeneratorTestResult(CSharpGeneratorDriver, CSharpCompilation, CSharpCompilation, int)"/>
		public static SingletonGeneratorTestResult Create(CSharpGeneratorDriver generatorDriver, CSharpCompilation inputCompilation, CSharpCompilation outputCompilation, int sourceIndex)
		{
			return new SingletonGeneratorTestResult(generatorDriver, inputCompilation, outputCompilation, sourceIndex);
		}

		/// <summary>
		/// Checks if the <paramref name="expected"/> <see cref="CSharpSyntaxTree"/> is equivalent to the <see cref="CSharpSyntaxTree"/> created by the <see cref="ISourceGenerator"/>.
		/// </summary>
		/// <param name="expected">A <see cref="CSharpSyntaxTree"/> that was expected to be generated.</param>
		public readonly bool Compare(CSharpSyntaxTree? expected)
		{
			if (expected is null)
			{
				return false;
			}

			return SyntaxTree?.IsEquivalentTo(expected) ?? false;
		}

		/// <summary>
		/// Checks if the <see cref="CSharpSyntaxTree"/> created from the <paramref name="expected"/> source is equivalent to the <see cref="CSharpSyntaxTree"/> created by the <see cref="ISourceGenerator"/>.
		/// </summary>
		/// <param name="expected">A <see cref="string"/> that represents a <see cref="CSharpSyntaxTree"/> that was expected to be generated.</param>
		public readonly bool Compare(string? expected)
		{
			if (expected is null)
			{
				return false;
			}

			if (CSharpSyntaxTree.ParseText(expected, encoding: Encoding.UTF8) is not CSharpSyntaxTree tree)
			{
				return false;
			}

			return SyntaxTree?.IsEquivalentTo(tree) ?? false;
		}

		readonly bool IGeneratorTestResult.Compare(GeneratorDriverRunResult result)
		{
			if (result is null || result.GeneratedTrees.IsDefaultOrEmpty)
			{
				return false;
			}

			if (result.GeneratedTrees[0] is not CSharpSyntaxTree tree)
			{
				return false;
			}

			return SyntaxTree?.IsEquivalentTo(tree) ?? false;
		}
	}
}
