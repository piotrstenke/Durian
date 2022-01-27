// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.TestServices
{
	/// <summary>
	/// Represents a result of a <see cref="ISourceGenerator"/> test.
	/// </summary>
	public interface IGeneratorTestResult
	{
		/// <summary>
		/// A collection of <see cref="Diagnostic"/>s that was produced during the <see cref="ISourceGenerator"/> pass.
		/// </summary>
		/// <value></value>
		ImmutableArray<Diagnostic> Diagnostics { get; }

		/// <summary>
		/// <see cref="System.Exception"/> that was thrown during the <see cref="ISourceGenerator"/> pass.
		/// </summary>
		Exception? Exception { get; }

		/// <summary>
		/// The <see cref="ISourceGenerator"/> that was being tested.
		/// </summary>
		ISourceGenerator Generator { get; }

		/// <summary>
		/// <see cref="CSharpCompilation"/> that represents the input of the <see cref="ISourceGenerator"/>> pass.
		/// </summary>
		CSharpCompilation InputCompilation { get; }

		/// <summary>
		/// Indicates whether the <see cref="ISourceGenerator"/> didn't produce any fatal errors and successfully generated the requested sources.
		/// </summary>
		bool IsGenerated { get; }

		/// <summary>
		/// <see cref="CSharpCompilation"/> that represents the output of the <see cref="ISourceGenerator"/> pass.
		/// </summary>
		CSharpCompilation? OutputCompilation { get; }

		/// <summary>
		/// Checks if the provided <paramref name="result"/> is equivalent to the <see cref="GeneratorDriverRunResult"/> created by the <see cref="ISourceGenerator"/> pass.
		/// </summary>
		/// <param name="result"><see cref="GeneratorDriverRunResult"/> to compare.</param>
		bool Compare(GeneratorDriverRunResult result);
	}
}