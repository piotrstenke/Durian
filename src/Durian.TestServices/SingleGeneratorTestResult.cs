using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Durian.Analysis.SyntaxVisitors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Durian.TestServices
{
	/// <summary>
	/// A <see cref="IGeneratorTestResult"/> that represents a single <see cref="SyntaxTree"/> retrieved from a tested <see cref="ISourceGenerator"/>.
	/// </summary>
	public readonly struct SingleGeneratorTestResult : IGeneratorTestResult
	{
		private readonly GeneratorRunResult _runResult;

		private readonly GeneratedSourceResult _sourceResult;

		/// <summary>
		/// A collection of <see cref="Diagnostic"/>s that were reported during the generator pass.
		/// </summary>
		public ImmutableArray<Diagnostic> Diagnostics => _runResult.Diagnostics;

		/// <inheritdoc/>
		public Exception? Exception => _runResult.Exception;

		/// <inheritdoc/>
		public ISourceGenerator Generator => _runResult.Generator;

		/// <summary>
		/// An identifier provided by the generator that identifies the added <see cref="SourceText"/>
		/// </summary>
		public string? HintName => _sourceResult.HintName;

		/// <inheritdoc/>
		public CSharpCompilation InputCompilation { get; }

		/// <summary>
		/// Determines whether the <see cref="ISourceGenerator"/> actually generated any <see cref="SyntaxTree"/>.
		/// </summary>
		[MemberNotNullWhen(true, nameof(SourceText), nameof(SyntaxTree), nameof(HintName))]
		public bool IsGenerated { get; }

		/// <inheritdoc/>
		public CSharpCompilation OutputCompilation { get; }

		/// <summary>
		/// The <see cref="Microsoft.CodeAnalysis.Text.SourceText"/> that was added by the generator.
		/// </summary>
		public SourceText? SourceText => _sourceResult.SourceText;

		/// <summary>
		/// A <see cref="SyntaxTree"/> that was generated during the generator pass.
		/// </summary>
		public SyntaxTree? SyntaxTree => _sourceResult.SyntaxTree;

		private SingleGeneratorTestResult(
			GeneratorRunResult runResult,
			GeneratedSourceResult sourceResult,
			CSharpCompilation inputCompilation,
			CSharpCompilation outputCompilation,
			bool isGenerated
		)
		{
			_runResult = runResult;
			_sourceResult = sourceResult;
			InputCompilation = inputCompilation;
			OutputCompilation = outputCompilation;
			IsGenerated = isGenerated;
		}

		/// <inheritdoc cref="Create(CSharpGeneratorDriver, CSharpCompilation, CSharpCompilation, int)"/>
		public static SingleGeneratorTestResult Create(CSharpGeneratorDriver generatorDriver, CSharpCompilation inputCompilation, CSharpCompilation outputCompilation)
		{
			return Create(generatorDriver, inputCompilation, outputCompilation, 0);
		}

		/// <summary>
		/// Creates a new <see cref="SingleGeneratorTestResult"/> from the specified <paramref name="generatorDriver"/>.
		/// </summary>
		/// <param name="generatorDriver"><see cref="CSharpGeneratorDriver"/> that was used to run the generator test.</param>
		/// <param name="inputCompilation"><see cref="CSharpCompilation"/> that was passed as input to the <paramref name="generatorDriver"/>.</param>
		/// <param name="outputCompilation"><see cref="CSharpCompilation"/> that was returned by the <paramref name="generatorDriver"/>.</param>
		/// <param name="sourceIndex">Index of generated syntax tree to include in the result.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="generatorDriver"/> is <see langword="null"/>. -or-
		/// <paramref name="inputCompilation"/> is <see langword="null"/>. -or-
		/// <paramref name="outputCompilation"/> is <see langword="null"/>.
		/// </exception>
		public static SingleGeneratorTestResult Create(
			CSharpGeneratorDriver generatorDriver,
			CSharpCompilation inputCompilation,
			CSharpCompilation outputCompilation,
			int sourceIndex
		)
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
			bool isGenerated = runResult.Exception is null && runResult.GeneratedSources.Length > sourceIndex;

			if (isGenerated)
			{
				GeneratedSourceResult sourceResult = runResult.GeneratedSources[sourceIndex];

				return new SingleGeneratorTestResult(runResult, sourceResult, inputCompilation, outputCompilation, true);
			}

			return new SingleGeneratorTestResult(runResult, default, inputCompilation, outputCompilation, false);
		}

		/// <summary>
		/// Checks if the <paramref name="expected"/> <see cref="SyntaxTree"/> is equivalent to the <see cref="SyntaxTree"/> created by the <see cref="ISourceGenerator"/>.
		/// </summary>
		/// <param name="expected">A <see cref="SyntaxTree"/> that was expected to be generated.</param>
		/// <param name="includeStructuredTrivia">Determines whether to include structured trivia in the comparison.</param>
		public bool Compare(SyntaxTree? expected, bool includeStructuredTrivia = false)
		{
			if (expected is null)
			{
				return false;
			}

			if (SyntaxTree is null)
			{
				return false;
			}

			return Compare_Internal(expected, includeStructuredTrivia);
		}

		/// <summary>
		/// Checks if the <see cref="SyntaxTree"/> created from the <paramref name="expected"/> source is equivalent to the <see cref="SyntaxTree"/> created by the <see cref="ISourceGenerator"/>.
		/// </summary>
		/// <param name="expected">A <see cref="string"/> that represents a <see cref="SyntaxTree"/> that was expected to be generated.</param>
		/// <param name="includeStructuredTrivia">Determines whether to include structured trivia in the comparison.</param>
		public bool Compare(string? expected, bool includeStructuredTrivia = false)
		{
			if (expected is null)
			{
				return false;
			}

			if (SyntaxTree is null)
			{
				return false;
			}

			if (CSharpSyntaxTree.ParseText(expected, encoding: Encoding.UTF8) is not SyntaxTree tree)
			{
				return false;
			}

			return Compare_Internal(tree, includeStructuredTrivia);
		}

		bool IGeneratorTestResult.Compare(GeneratorDriverRunResult result, bool includeStructuredTrivia)
		{
			if (result is null || result.GeneratedTrees.IsDefaultOrEmpty)
			{
				return false;
			}

			return Compare(result.GeneratedTrees[0], includeStructuredTrivia);
		}

		private bool Compare_Internal(SyntaxTree expected, bool includeStructuredTrivia)
		{
			if (!includeStructuredTrivia)
			{
				return SyntaxTree!.IsEquivalentTo(expected);
			}

			StructuredTriviaPreserver preserver = new();
			SyntaxNode root = SyntaxTree!.GetRoot();
			root = preserver.Visit(root);

			SyntaxNode exp = preserver.Visit(expected.GetRoot());

			return root.IsEquivalentTo(exp);
		}
	}
}
