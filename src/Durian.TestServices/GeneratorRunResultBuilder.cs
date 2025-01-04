using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Durian.TestServices;

/// <summary>
/// A builder of the <see cref="GeneratorRunResult"/> struct.
/// </summary>
public sealed class GeneratorRunResultBuilder
{
	internal GeneratorDriverRunResultBuilder? _parent;

	private readonly List<Diagnostic> _diagnostics;

	private readonly List<GeneratedSourceResult> _sources;

	private Exception? _exception;

	private ISourceGenerator? _generator;

	/// <inheritdoc cref="GeneratorRunResultBuilder(IEnumerable{GeneratedSourceResult})"/>
	public GeneratorRunResultBuilder()
	{
		_sources = new List<GeneratedSourceResult>();
		_diagnostics = new List<Diagnostic>();
	}

	/// <inheritdoc cref="GeneratorRunResultBuilder(IEnumerable{GeneratedSourceResult})"/>
	public GeneratorRunResultBuilder(ISourceGenerator? generator)
	{
		_sources = new List<GeneratedSourceResult>();
		_diagnostics = new List<Diagnostic>();
		_generator = generator;
	}

	/// <inheritdoc cref="GeneratorRunResultBuilder(IEnumerable{GeneratedSourceResult})"/>
	public GeneratorRunResultBuilder(IEnumerable<GeneratedSourceResult>? sources) : this(null, sources)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="GeneratorRunResultBuilder"/> class.
	/// </summary>
	/// <param name="generator">A <see cref="ISourceGenerator"/> to be set to the <see cref="GeneratorRunResult.Generator"/> property.</param>
	/// <param name="sources">A collection of <see cref="GeneratedSourceResult"/>s to be set to the <see cref="GeneratorRunResult.GeneratedSources"/> property.</param>
	public GeneratorRunResultBuilder(ISourceGenerator? generator, IEnumerable<GeneratedSourceResult>? sources)
	{
		if (sources is null)
		{
			_sources = new List<GeneratedSourceResult>();
		}
		else
		{
			_sources = new List<GeneratedSourceResult>(sources);
		}

		_diagnostics = new List<Diagnostic>();
		_generator = generator;
	}

	/// <summary>
	/// Adds a new <see cref="Diagnostic"/> to the <see cref="GeneratorRunResult.Diagnostics"/> collection.
	/// </summary>
	/// <param name="diagnostic">A <see cref="Diagnostic"/> to be added to the <see cref="GeneratorRunResult.Diagnostics"/> collection.</param>
	/// <returns>This <see cref="GeneratorRunResultBuilder"/>.</returns>
	public GeneratorRunResultBuilder AddDiagnostic(Diagnostic? diagnostic)
	{
		if (diagnostic is not null)
		{
			_diagnostics.Add(diagnostic);
		}

		return this;
	}

	/// <summary>
	/// Adds a range of <see cref="Diagnostic"/>s to the <see cref="GeneratorRunResult.Diagnostics"/> collection.
	/// </summary>
	/// <param name="diagnostics">A range <see cref="Diagnostic"/>s to be added to the <see cref="GeneratorRunResult.Diagnostics"/> collection.</param>
	/// <returns>This <see cref="GeneratorRunResultBuilder"/>.</returns>
	public GeneratorRunResultBuilder AddDiagnostics(IEnumerable<Diagnostic>? diagnostics)
	{
		if (diagnostics is not null)
		{
			_diagnostics.AddRange(diagnostics);
		}

		return this;
	}

	/// <summary>
	/// Adds a new <see cref="GeneratedSourceResult"/> to the <see cref="GeneratorRunResult.GeneratedSources"/> collection.
	/// </summary>
	/// <param name="source">A <see cref="GeneratedSourceResult"/> to be added to the <see cref="GeneratorRunResult.GeneratedSources"/> collection.</param>
	/// <returns>This <see cref="GeneratorRunResultBuilder"/>.</returns>
	public GeneratorRunResultBuilder AddSource(in GeneratedSourceResult source)
	{
		_sources.Add(source);

		return this;
	}

	/// <inheritdoc cref="AddSource(SyntaxTree, string)"/>
	public GeneratorRunResultBuilder AddSource(SyntaxTree? syntaxTree)
	{
		if (syntaxTree is not null)
		{
			_sources.Add(GeneratorResultFactory.CreateSourceResult(syntaxTree));
		}

		return this;
	}

	/// <inheritdoc cref="AddSource(SyntaxTree, SourceText, string)"/>
	/// <exception cref="ArgumentException">A valid <see cref="SourceText"/> couldn't be properly retrieved from the <paramref name="syntaxTree"/>.</exception>
	public GeneratorRunResultBuilder AddSource(SyntaxTree? syntaxTree, string? hintName)
	{
		if (syntaxTree is not null)
		{
			_sources.Add(GeneratorResultFactory.CreateSourceResult(syntaxTree, hintName));
		}

		return this;
	}

	/// <summary>
	/// Adds a new <see cref="GeneratedSourceResult"/> created from the specified <paramref name="syntaxTree"/> to the <see cref="GeneratorRunResult.GeneratedSources"/> collection.
	/// </summary>
	/// <param name="syntaxTree">A <see cref="SyntaxTree"/> to be set to the <see cref="GeneratedSourceResult.SyntaxTree"/> property.</param>
	/// <param name="sourceText">A <see cref="SourceText"/> to be set to the <see cref="GeneratedSourceResult.SourceText"/> property.</param>
	/// <param name="hintName">A <see cref="string"/> value to be set to the <see cref="GeneratedSourceResult.HintName"/> property.</param>
	/// <returns>This <see cref="GeneratorRunResultBuilder"/>.</returns>
	public GeneratorRunResultBuilder AddSource(SyntaxTree? syntaxTree, SourceText? sourceText, string? hintName)
	{
		if (syntaxTree is not null && sourceText is not null)
		{
			_sources.Add(GeneratorResultFactory.CreateSourceResult(syntaxTree, sourceText, hintName));
		}

		return this;
	}

	/// <inheritdoc cref="AddSource(string, string)"/>
	public GeneratorRunResultBuilder AddSource(string source)
	{
		if (!string.IsNullOrWhiteSpace(source))
		{
			_sources.Add(GeneratorResultFactory.CreateSourceResult(source));
		}

		return this;
	}

	/// <summary>
	/// Adds a new <see cref="GeneratedSourceResult"/> created from the specified <paramref name="source"/> to the <see cref="GeneratorRunResult.GeneratedSources"/> collection.
	/// </summary>
	/// <param name="source">A <see cref="string"/> value to be parsed and converted to a <see cref="SyntaxTree"/> that will be used to set the <see cref="GeneratedSourceResult.SyntaxTree"/> property.</param>
	/// <param name="hintName">A <see cref="string"/> value to be set to the <see cref="GeneratedSourceResult.HintName"/> property.</param>
	/// <exception cref="ArgumentException">
	/// A <see cref="SyntaxTree"/> couldn't be created from the specified <paramref name="source"/>. -or-
	/// A valid <see cref="SourceText"/> couldn't be properly retrieved from the created <see cref="SyntaxTree"/>.
	/// </exception>
	public GeneratorRunResultBuilder AddSource(string source, string? hintName)
	{
		if (!string.IsNullOrWhiteSpace(source))
		{
			_sources.Add(GeneratorResultFactory.CreateSourceResult(source, hintName));
		}

		return this;
	}

	/// <summary>
	/// Adds a range of <see cref="GeneratedSourceResult"/>s to the <see cref="GeneratorRunResult.GeneratedSources"/> collection.
	/// </summary>
	/// <param name="sources">A range <see cref="GeneratedSourceResult"/>s to be added to the <see cref="GeneratorRunResult.GeneratedSources"/> collection.</param>
	/// <returns>This <see cref="GeneratorRunResultBuilder"/>.</returns>
	public GeneratorRunResultBuilder AddSources(IEnumerable<GeneratedSourceResult>? sources)
	{
		if (sources is not null)
		{
			_sources.AddRange(sources);
		}

		return this;
	}

	/// <summary>
	/// Actually creates the <see cref="GeneratorRunResult"/>.
	/// </summary>
	/// <exception cref="ArgumentException">The <see cref="GeneratorRunResult.Generator"/> property must be set before calling the <see cref="Build"/> method.</exception>
	public GeneratorRunResult Build()
	{
		if (_generator is null)
		{
			throw new InvalidOperationException($"The {nameof(GeneratorRunResult)}.{nameof(GeneratorRunResult.Generator)} property must be set before calling the {nameof(Build)} method!");
		}

		GeneratorRunResult result = GeneratorResultFactory.CreateGeneratorResult(_generator, _sources, _diagnostics, _exception);

		_parent?.AddResult(in result);

		return result;
	}

	/// <summary>
	/// Resets the builder.
	/// </summary>
	public void Reset()
	{
		_generator = null;
		_exception = null;
		_diagnostics.Clear();
		_sources.Clear();
	}

	/// <summary>
	/// Assigns a new collection of <see cref="Diagnostic"/>s to the <see cref="GeneratorRunResult.Diagnostics"/> property.
	/// </summary>
	/// <param name="diagnostics">A collection of <see cref="Diagnostic"/>s to be set to the <see cref="GeneratorRunResult.Diagnostics"/> property.</param>
	/// <returns>This <see cref="GeneratorRunResultBuilder"/>.</returns>
	public GeneratorRunResultBuilder WithDiagnostics(IEnumerable<Diagnostic>? diagnostics)
	{
		if (diagnostics is not null)
		{
			_diagnostics.Clear();
			_diagnostics.AddRange(diagnostics);
		}

		return this;
	}

	/// <summary>
	/// Assigns new <see cref="Exception"/> to the <see cref="GeneratorRunResult.Exception"/> property.
	/// </summary>
	/// <param name="exception">An <see cref="Exception"/> to be set to the <see cref="GeneratorRunResult.Exception"/> property.</param>
	/// <returns>This <see cref="GeneratorRunResultBuilder"/>.</returns>
	public GeneratorRunResultBuilder WithException(Exception? exception)
	{
		_exception = exception;

		return this;
	}

	/// <summary>
	/// Assigns new <see cref="ISourceGenerator"/> to the <see cref="GeneratorRunResult.Generator"/> property.
	/// </summary>
	/// <param name="generator">A <see cref="ISourceGenerator"/> to be set to the <see cref="GeneratorRunResult.Generator"/> property.</param>
	/// <returns>This <see cref="GeneratorRunResultBuilder"/>.</returns>
	public GeneratorRunResultBuilder WithGenerator(ISourceGenerator? generator)
	{
		_generator = generator;

		return this;
	}

	/// <summary>
	/// Assigns a new collection of <see cref="GeneratedSourceResult"/>s to the <see cref="GeneratorRunResult.GeneratedSources"/> property.
	/// </summary>
	/// <param name="sources">A collection of <see cref="Diagnostic"/>s to be set to the <see cref="GeneratorRunResult.GeneratedSources"/> property.</param>
	/// <returns>This <see cref="GeneratorRunResultBuilder"/>.</returns>
	public GeneratorRunResultBuilder WithSources(IEnumerable<GeneratedSourceResult>? sources)
	{
		if (sources is not null)
		{
			_sources.AddRange(sources);
		}

		return this;
	}
}
