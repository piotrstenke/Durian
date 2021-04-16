using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Durian.Tests
{
	/// <summary>
	/// A builder of the <see cref="GeneratorDriverRunResult"/> class.
	/// </summary>
	public sealed class GeneratorDriverRunResultBuilder
	{
		private readonly List<GeneratorRunResult> _results;
		private readonly List<GeneratorRunResultBuilder> _childBuilders;

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneratorRunResultBuilder"/> class.
		/// </summary>
		public GeneratorDriverRunResultBuilder()
		{
			_results = new List<GeneratorRunResult>();
			_childBuilders = new();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneratorRunResultBuilder"/> class.
		/// </summary>
		/// <param name="results">A collection of <see cref="GeneratorRunResult"/>s to be used when creating the <see cref="GeneratorDriverRunResult"/>.</param>
		public GeneratorDriverRunResultBuilder(IEnumerable<GeneratorRunResult>? results)
		{
			if (results is null)
			{
				_results = new();
			}
			else
			{
				_results = new(results);
			}

			_childBuilders = new();
		}

		/// <summary>
		/// Adds a new <see cref="GeneratorRunResult"/> to the <see cref="GeneratorDriverRunResult.Results"/> collection.
		/// </summary>
		/// <param name="result">A <see cref="GeneratorRunResult"/> to be added to the <see cref="GeneratorDriverRunResult.Results"/> collection.</param>
		/// <returns>This <see cref="GeneratorDriverRunResultBuilder"/>.</returns>
		public GeneratorDriverRunResultBuilder AddResult(in GeneratorRunResult result)
		{
			_results.Add(result);

			return this;
		}

		/// <summary>
		/// Adds a new <see cref="GeneratorRunResult"/> created from the provided data to the <see cref="GeneratorDriverRunResult.Results"/> collection.
		/// </summary>
		/// <param name="generator">A <see cref="ISourceGenerator"/> to be set to the <see cref="GeneratorRunResult.Generator"/> property.</param>
		/// <param name="generatedSources">A collection of <see cref="GeneratedSourceResult"/>s to be set to the <see cref="GeneratorRunResult.GeneratedSources"/> property.</param>
		/// <param name="diagnostics">A collection of <see cref="Diagnostic"/>s to be set to the <see cref="GeneratorRunResult.Diagnostics"/> property.</param>
		/// <param name="exception">An <see cref="Exception"/> to be set to the <see cref="GeneratorRunResult.Exception"/> property.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> was <c>null</c>.</exception>
		/// <returns>This <see cref="GeneratorDriverRunResultBuilder"/>.</returns>
		public GeneratorDriverRunResultBuilder AddResult(ISourceGenerator generator, IEnumerable<GeneratedSourceResult>? generatedSources, IEnumerable<Diagnostic>? diagnostics, Exception? exception)
		{
			_results.Add(GeneratorResultFactory.CreateGeneratorResult(generator, generatedSources, diagnostics, exception));

			return this;
		}

		/// <summary>
		/// Begins building a new <see cref="GeneratorRunResult"/>.
		/// </summary>
		public GeneratorRunResultBuilder BeginResult()
		{
			return new()
			{
				_parent = this
			};
		}

		/// <summary>
		/// Adds a range of <see cref="GeneratorRunResult"/>s to the <see cref="GeneratorDriverRunResult.Results"/> collection.
		/// </summary>
		/// <param name="generatedSources">A range <see cref="GeneratorRunResult"/>s to be added to the <see cref="GeneratorDriverRunResult.Results"/> collection.</param>
		/// <returns>This <see cref="GeneratorDriverRunResultBuilder"/>.</returns>
		public GeneratorDriverRunResultBuilder AddResults(IEnumerable<GeneratorRunResult>? generatedSources)
		{
			if (generatedSources is not null)
			{
				_results.AddRange(generatedSources);
			}

			return this;
		}

		/// <summary>
		/// Assigns a new collection of <see cref="GeneratorRunResult"/>s to the <see cref="GeneratorDriverRunResult.Results"/> property.
		/// </summary>
		/// <param name="results">A collection of <see cref="GeneratorRunResult"/>s to be set to the <see cref="GeneratorDriverRunResult.Results"/> property.</param>
		/// <returns>This <see cref="GeneratorDriverRunResultBuilder"/>.</returns>
		public GeneratorDriverRunResultBuilder WithResults(IEnumerable<GeneratorRunResult>? results)
		{
			if (results is not null)
			{
				_results.AddRange(results);
			}

			return this;
		}

		/// <summary>
		/// Actually creates the <see cref="GeneratorDriverRunResult"/>.
		/// </summary>
		public GeneratorDriverRunResult Build()
		{
			return GeneratorResultFactory.CreateDriverResult(_results);
		}

		/// <summary>
		/// Resets the builder.
		/// </summary>
		public void Reset()
		{
			_results.Clear();

			foreach (GeneratorRunResultBuilder child in _childBuilders)
			{
				child._parent = null;
			}

			_childBuilders.Clear();
		}
	}
}
