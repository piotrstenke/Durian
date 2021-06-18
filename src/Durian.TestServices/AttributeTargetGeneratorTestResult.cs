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
	/// A <see cref="IGeneratorTestResult"/> that represents two <see cref="CSharpSyntaxTree"/>s - one for a generated <see cref="System.Attribute"/>
	/// and one for a <see cref="CSharpSyntaxTree"/> that uses this <see cref="System.Attribute"/>.
	/// </summary>
	public readonly struct AttributeTargetGeneratorTestResult : IGeneratorTestResult
	{
		private readonly GeneratorRunResult _runResult;

		/// <summary>
		/// A <see cref="GeneratedSourceResult"/> that represents the generated <see cref="System.Attribute"/>.
		/// </summary>
		public readonly GeneratedSourceResult Attribute { get; }

		/// <inheritdoc/>
		public readonly CSharpCompilation InputCompilation { get; }

		/// <inheritdoc/>
		public readonly bool IsGenerated { get; }

		/// <inheritdoc/>
		public readonly CSharpCompilation OutputCompilation { get; }

		/// <summary>
		/// A <see cref="GeneratedSourceResult"/> that represents the generated <see cref="CSharpSyntaxTree"/> that uses the generated <see cref="Attribute"/>.
		/// </summary>
		public readonly GeneratedSourceResult Source { get; }

		/// <inheritdoc/>
		public readonly ImmutableArray<Diagnostic> Diagnostics => _runResult.Diagnostics;

		/// <inheritdoc/>
		public readonly Exception? Exception => _runResult.Exception;

		/// <inheritdoc/>
		public readonly ISourceGenerator Generator => _runResult.Generator;

		/// <summary>
		/// Initializes a new instance of the <see cref="AttributeTargetGeneratorTestResult"/> struct.
		/// </summary>
		/// <param name="generatorDriver">A <see cref="CSharpGeneratorDriver"/> that was used to perform the test.</param>
		/// <param name="inputCompilation">A <see cref="CSharpCompilation"/> that represent an input for the tested <see cref="ISourceGenerator"/>.</param>
		/// <param name="outputCompilation">A <see cref="CSharpCompilation"/> that was created by the tested <see cref="ISourceGenerator"/>.</param>
		public AttributeTargetGeneratorTestResult(GeneratorDriver generatorDriver, CSharpCompilation inputCompilation, CSharpCompilation outputCompilation)
		{
			InputCompilation = inputCompilation;
			OutputCompilation = outputCompilation;
			_runResult = generatorDriver.GetRunResult().Results[0];

			if (_runResult.Exception is null && _runResult.GeneratedSources.Length > 1)
			{
				Attribute = _runResult.GeneratedSources[0];
				Source = _runResult.GeneratedSources[1];

				IsGenerated = Attribute.SyntaxTree is CSharpSyntaxTree && Source.SyntaxTree is CSharpSyntaxTree;
			}
			else
			{
				IsGenerated = false;
				Source = default;
				Attribute = default;
			}
		}

		/// <inheritdoc cref="AttributeTargetGeneratorTestResult(GeneratorDriver, CSharpCompilation, CSharpCompilation)"/>
		public static AttributeTargetGeneratorTestResult Create(GeneratorDriver generatorDriver, CSharpCompilation inputCompilation, CSharpCompilation outputCompilation)
		{
			return new AttributeTargetGeneratorTestResult(generatorDriver, inputCompilation, outputCompilation);
		}

		/// <summary>
		/// Checks if the <paramref name="expected"/> <see cref="CSharpSyntaxTree"/> is equivalent to the <see cref="CSharpSyntaxTree"/> of the <see cref="Attribute"/> created by the <see cref="ISourceGenerator"/>.
		/// </summary>
		/// <param name="expected">A <see cref="CSharpSyntaxTree"/> that was expected to be generated.</param>
		public readonly bool CompareAttribute(CSharpSyntaxTree? expected)
		{
			return Compare((CSharpSyntaxTree)Attribute.SyntaxTree, expected);
		}

		/// <summary>
		/// Checks if the <see cref="CSharpSyntaxTree"/> created from the <paramref name="expected"/> source is equivalent to the <see cref="CSharpSyntaxTree"/> of the <see cref="Attribute"/> created by the <see cref="ISourceGenerator"/>.
		/// </summary>
		/// <param name="expected">A <see cref="string"/> that represents a <see cref="CSharpSyntaxTree"/> that was expected to be generated.</param>
		public readonly bool CompareAttribute(string? expected)
		{
			return Compare((CSharpSyntaxTree)Attribute.SyntaxTree, expected);
		}

		/// <summary>
		/// Checks if the <paramref name="expected"/> <see cref="CSharpSyntaxTree"/> is equivalent to the <see cref="CSharpSyntaxTree"/> of the <see cref="Source"/> created by the <see cref="ISourceGenerator"/>.
		/// </summary>
		/// <param name="expected">A <see cref="CSharpSyntaxTree"/> that was expected to be generated.</param>
		public readonly bool CompareSource(CSharpSyntaxTree? expected)
		{
			return Compare((CSharpSyntaxTree)Source.SyntaxTree, expected);
		}

		/// <summary>
		/// Checks if the <see cref="CSharpSyntaxTree"/> created from the <paramref name="expected"/> source is equivalent to the <see cref="CSharpSyntaxTree"/> of the <see cref="Source"/> created by the <see cref="ISourceGenerator"/>.
		/// </summary>
		/// <param name="expected">A <see cref="string"/> that represents a <see cref="CSharpSyntaxTree"/> that was expected to be generated.</param>
		public readonly bool CompareSource(string? expected)
		{
			return Compare((CSharpSyntaxTree)Source.SyntaxTree, expected);
		}

		readonly bool IGeneratorTestResult.Compare(GeneratorDriverRunResult result)
		{
			return
				result.GeneratedTrees.Length > 2 &&
				result.GeneratedTrees[0].IsEquivalentTo(Attribute.SyntaxTree) &&
				result.GeneratedTrees[1].IsEquivalentTo(Source.SyntaxTree);
		}

		private static bool Compare(CSharpSyntaxTree first, string? second)
		{
			if (second is null)
			{
				return false;
			}

			if (CSharpSyntaxTree.ParseText(second, encoding: Encoding.UTF8) is not CSharpSyntaxTree tree)
			{
				return false;
			}

			return first?.IsEquivalentTo(tree) ?? false;
		}

		private static bool Compare(CSharpSyntaxTree first, CSharpSyntaxTree? second)
		{
			if (second is null)
			{
				return false;
			}

			return first.IsEquivalentTo(second);
		}
	}
}
