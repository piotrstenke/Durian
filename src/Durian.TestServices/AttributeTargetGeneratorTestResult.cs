using System;
using System.Collections.Immutable;
using System.Text;
using Durian.Analysis.SyntaxVisitors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.TestServices;

/// <summary>
/// A <see cref="IGeneratorTestResult"/> that represents two <see cref="SyntaxTree"/>s - one for a generated <see cref="System.Attribute"/>
/// and one for a <see cref="SyntaxTree"/> that uses this <see cref="System.Attribute"/>.
/// </summary>
public readonly struct AttributeTargetGeneratorTestResult : IGeneratorTestResult
{
	private readonly GeneratorRunResult _runResult;

	/// <summary>
	/// A <see cref="GeneratedSourceResult"/> that represents the generated <see cref="System.Attribute"/>.
	/// </summary>
	public GeneratedSourceResult Attribute { get; }

	/// <inheritdoc/>
	public ImmutableArray<Diagnostic> Diagnostics => _runResult.Diagnostics;

	/// <inheritdoc/>
	public Exception? Exception => _runResult.Exception;

	/// <inheritdoc/>
	public ISourceGenerator Generator => _runResult.Generator;

	/// <inheritdoc/>
	public CSharpCompilation InputCompilation { get; }

	/// <inheritdoc/>
	public bool IsGenerated { get; }

	/// <inheritdoc/>
	public CSharpCompilation OutputCompilation { get; }

	/// <summary>
	/// A <see cref="GeneratedSourceResult"/> that represents the generated <see cref="SyntaxTree"/> that uses the generated <see cref="Attribute"/>.
	/// </summary>
	public GeneratedSourceResult Source { get; }

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

			IsGenerated = Attribute.SyntaxTree is SyntaxTree && Source.SyntaxTree is SyntaxTree;
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
	/// Checks if the <paramref name="expected"/> <see cref="SyntaxTree"/> is equivalent to the <see cref="SyntaxTree"/> of the <see cref="Attribute"/> created by the <see cref="ISourceGenerator"/>.
	/// </summary>
	/// <param name="expected">A <see cref="SyntaxTree"/> that was expected to be generated.</param>
	/// <param name="includeStructuredTrivia">Determines whether to include structured trivia in the comparison.</param>
	public bool CompareAttribute(SyntaxTree? expected, bool includeStructuredTrivia = false)
	{
		return Compare(Attribute.SyntaxTree, expected, includeStructuredTrivia);
	}

	/// <summary>
	/// Checks if the <see cref="SyntaxTree"/> created from the <paramref name="expected"/> source is equivalent to the <see cref="SyntaxTree"/> of the <see cref="Attribute"/> created by the <see cref="ISourceGenerator"/>.
	/// </summary>
	/// <param name="expected">A <see cref="string"/> that represents a <see cref="SyntaxTree"/> that was expected to be generated.</param>
	/// <param name="includeStructuredTrivia">Determines whether to include structured trivia in the comparison.</param>
	public bool CompareAttribute(string? expected, bool includeStructuredTrivia = false)
	{
		return Compare(Attribute.SyntaxTree, expected, includeStructuredTrivia);
	}

	/// <summary>
	/// Checks if the <paramref name="expected"/> <see cref="SyntaxTree"/> is equivalent to the <see cref="SyntaxTree"/> of the <see cref="Source"/> created by the <see cref="ISourceGenerator"/>.
	/// </summary>
	/// <param name="expected">A <see cref="SyntaxTree"/> that was expected to be generated.</param>
	/// <param name="includeStructuredTrivia">Determines whether to include structured trivia in the comparison.</param>
	public bool CompareSource(SyntaxTree? expected, bool includeStructuredTrivia = false)
	{
		return Compare(Source.SyntaxTree, expected, includeStructuredTrivia);
	}

	/// <summary>
	/// Checks if the <see cref="SyntaxTree"/> created from the <paramref name="expected"/> source is equivalent to the <see cref="SyntaxTree"/> of the <see cref="Source"/> created by the <see cref="ISourceGenerator"/>.
	/// </summary>
	/// <param name="expected">A <see cref="string"/> that represents a <see cref="SyntaxTree"/> that was expected to be generated.</param>
	/// <param name="includeStructuredTrivia">Determines whether to include structured trivia in the comparison.</param>
	public bool CompareSource(string? expected, bool includeStructuredTrivia = false)
	{
		return Compare(Source.SyntaxTree, expected, includeStructuredTrivia);
	}

	bool IGeneratorTestResult.Compare(GeneratorDriverRunResult result, bool includeStructuredTrivia)
	{
		if (result.GeneratedTrees.Length < 2)
		{
			return false;
		}

		if (!includeStructuredTrivia)
		{
			return
				result.GeneratedTrees[0].IsEquivalentTo(Attribute.SyntaxTree) &&
				result.GeneratedTrees[1].IsEquivalentTo(Source.SyntaxTree);
		}

		StructuredTriviaPreserver preserver = new();

		SyntaxNode node = preserver.Visit(Attribute.SyntaxTree.GetRoot());
		SyntaxNode other = preserver.Visit(result.GeneratedTrees[0].GetRoot());

		if (!node.IsEquivalentTo(other))
		{
			return false;
		}

		node = preserver.Visit(Source.SyntaxTree.GetRoot());
		other = preserver.Visit(result.GeneratedTrees[1].GetRoot());

		return node.IsEquivalentTo(other);
	}

	private static bool Compare(SyntaxTree first, string? second, bool includeStructuredTrivia)
	{
		if (second is null)
		{
			return false;
		}

		return Compare(first, CSharpSyntaxTree.ParseText(second, encoding: Encoding.UTF8), includeStructuredTrivia);
	}

	private static bool Compare(SyntaxTree first, SyntaxTree? second, bool includeStructuredTrivia)
	{
		if (second is null)
		{
			return false;
		}

		if (!includeStructuredTrivia)
		{
			return first.IsEquivalentTo(second);
		}

		StructuredTriviaPreserver preserver = new();

		SyntaxNode node = preserver.Visit(first.GetRoot());
		SyntaxNode other = preserver.Visit(second.GetRoot());

		return node.IsEquivalentTo(other);
	}
}
