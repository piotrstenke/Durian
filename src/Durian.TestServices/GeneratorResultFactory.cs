using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Durian.Tests
{
	/// <summary>
	/// Contains various factory methods for creating various generator run results.
	/// </summary>
	public static class GeneratorResultFactory
	{
		/// <inheritdoc cref="CreateSourceResult(string, string)"/>
		public static GeneratedSourceResult CreateSourceResult(string source)
		{
			return CreateSourceResult(source, null);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="GeneratedSourceResult"/> struct.
		/// </summary>
		/// <param name="source">A <see cref="string"/> value to be parsed and converted to a <see cref="CSharpSyntaxTree"/> that will be used to set the <see cref="GeneratedSourceResult.SyntaxTree"/> property.</param>
		/// <param name="hintName">A <see cref="string"/> value to be set to the <see cref="GeneratedSourceResult.HintName"/> property.</param>
		/// <exception cref="ArgumentException">
		/// A <see cref="CSharpSyntaxTree"/> couldn't be created from the specified <paramref name="source"/>. -or-
		/// A valid <see cref="SourceText"/> couldn't be properly retrieved from the created <see cref="CSharpSyntaxTree"/>.
		/// </exception>
		public static GeneratedSourceResult CreateSourceResult(string source, string? hintName)
		{
			if (string.IsNullOrWhiteSpace(source))
			{
				throw GetException();
			}

			if (CSharpSyntaxTree.ParseText(source) is not CSharpSyntaxTree tree)
			{
				throw GetException();
			}

			return CreateSourceResult(tree, hintName);

			static ArgumentException GetException()
			{
				return new ArgumentException($"A {nameof(CSharpSyntaxTree)} couldn't be created from the specified {nameof(source)}!");
			}
		}

		/// <inheritdoc cref="CreateSourceResult(CSharpSyntaxTree, string)"/>
		public static GeneratedSourceResult CreateSourceResult(CSharpSyntaxTree syntaxTree)
		{
			return CreateSourceResult(syntaxTree, string.Empty);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="GeneratedSourceResult"/> struct.
		/// </summary>
		/// <param name="syntaxTree">A <see cref="CSharpSyntaxTree"/> to be set to the <see cref="GeneratedSourceResult.SyntaxTree"/> property.</param>
		/// <param name="hintName">A <see cref="string"/> value to be set to the <see cref="GeneratedSourceResult.HintName"/> property.</param>
		/// <remarks>The needed <see cref="SourceText"/> for the <see cref="GeneratedSourceResult.SourceText"/> property is created by calling the <see cref="SyntaxTree.TryGetText(out SourceText?)"/> method.</remarks>
		/// <exception cref="ArgumentNullException"><paramref name="syntaxTree"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">A valid <see cref="SourceText"/> couldn't be properly retrieved from the <paramref name="syntaxTree"/>.</exception>
		public static GeneratedSourceResult CreateSourceResult(CSharpSyntaxTree syntaxTree, string? hintName)
		{
			if (syntaxTree is null)
			{
				throw new ArgumentNullException(nameof(syntaxTree));
			}

			if (!syntaxTree.TryGetText(out SourceText? sourceText))
			{
				throw new ArgumentException($"A valid {nameof(SourceText)} couldn't be properly retrieved from the {nameof(syntaxTree)}!");
			}

			return CreateSourceResult_Internal(syntaxTree, sourceText, hintName);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="GeneratedSourceResult"/> struct.
		/// </summary>
		/// <param name="syntaxTree">A <see cref="CSharpSyntaxTree"/> to be set to the <see cref="GeneratedSourceResult.SyntaxTree"/> property.</param>
		/// <param name="sourceText">A <see cref="SourceText"/> to be set to the <see cref="GeneratedSourceResult.SourceText"/> property.</param>
		/// <param name="hintName">A <see cref="string"/> value to be set to the <see cref="GeneratedSourceResult.HintName"/> property.</param>
		/// <exception cref="ArgumentNullException"><paramref name="syntaxTree"/> is <see langword="null"/>. -or <paramref name="sourceText"/> is <see langword="null"/>.</exception>
		public static GeneratedSourceResult CreateSourceResult(CSharpSyntaxTree syntaxTree, SourceText sourceText, string? hintName)
		{
			if (syntaxTree is null)
			{
				throw new ArgumentNullException(nameof(syntaxTree));
			}

			if (sourceText is null)
			{
				throw new ArgumentNullException(nameof(sourceText));
			}

			return CreateSourceResult_Internal(syntaxTree, sourceText, hintName);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="GeneratorRunResult"/> struct.
		/// </summary>
		/// <param name="generator">A <see cref="ISourceGenerator"/> to be set to the <see cref="GeneratorRunResult.Generator"/> property.</param>
		/// <param name="generatedSources">A collection of <see cref="GeneratedSourceResult"/>s to be set to the <see cref="GeneratorRunResult.GeneratedSources"/> property.</param>
		/// <param name="diagnostics">A collection of <see cref="Diagnostic"/>s to be set to the <see cref="GeneratorRunResult.Diagnostics"/> property.</param>
		/// <param name="exception">An <see cref="Exception"/> to be set to the <see cref="GeneratorRunResult.Exception"/> property.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		public static GeneratorRunResult CreateGeneratorResult(ISourceGenerator generator, IEnumerable<GeneratedSourceResult>? generatedSources, IEnumerable<Diagnostic>? diagnostics, Exception? exception)
		{
			if (generator is null)
			{
				throw new ArgumentNullException(nameof(generator));
			}

			object? obj = CreateInstance(
				typeof(GeneratorRunResult),
				generator,
				generatedSources?.ToImmutableArray() ?? ImmutableArray<GeneratedSourceResult>.Empty,
				diagnostics?.ToImmutableArray() ?? ImmutableArray<Diagnostic>.Empty,
				exception
			);

			if (obj is null)
			{
				return default;
			}

			return (GeneratorRunResult)obj;
		}

		/// <summary>
		/// Creates a new instance of the <see cref="GeneratorDriverRunResult"/> class.
		/// </summary>
		/// <param name="results">A collection of <see cref="GeneratorRunResult"/>s to be used when creating the <see cref="GeneratorDriverRunResult"/>.</param>
		public static GeneratorDriverRunResult CreateDriverResult(IEnumerable<GeneratorRunResult>? results)
		{
			return (GeneratorDriverRunResult)CreateInstance(typeof(GeneratorDriverRunResult), results?.ToImmutableArray() ?? ImmutableArray<GeneratorRunResult>.Empty)!;
		}

		private static GeneratedSourceResult CreateSourceResult_Internal(CSharpSyntaxTree syntaxTree, SourceText sourceText, string? hintName)
		{
			object? obj = CreateInstance(typeof(GeneratedSourceResult), syntaxTree, sourceText, hintName ?? string.Empty);

			if (obj is null)
			{
				return default;
			}

			return (GeneratedSourceResult)obj;
		}

		private static object? CreateInstance(Type type, params object?[] args)
		{
			return Activator.CreateInstance(
				type: type,
				bindingAttr: BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic,
				binder: null,
				args: args,
				culture: CultureInfo.InvariantCulture
			);
		}
	}
}
