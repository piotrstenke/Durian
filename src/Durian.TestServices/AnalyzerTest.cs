// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.TestServices
{
	/// <summary>
	/// An abstract class that provides methods to test <see cref="DiagnosticAnalyzer"/>s.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="DiagnosticAnalyzer"/> this <see cref="AnalyzerTest{T}"/> supports.</typeparam>
	public abstract class AnalyzerTest<T> : CompilationTest where T : DiagnosticAnalyzer, new()
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AnalyzerTest{T}"/> class.
		/// </summary>
		protected AnalyzerTest()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AnalyzerTest{T}"/> class.
		/// </summary>
		/// <param name="sources">An array of <see cref="string"/>s to be used as initial sources of <see cref="CSharpSyntaxTree"/>s for the <see cref="Compilation"/>.</param>
		protected AnalyzerTest(params string[]? sources) : base(sources)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AnalyzerTest{T}"/> class.
		/// </summary>
		/// <param name="compilation">An instance of <see cref="TestableCompilationData"/> to share between all tests in this class.</param>
		/// <param name="addInitialSources">Determines whether to add sources created using the <see cref="CompilationTest.GetInitialSources()"/> method to the <paramref name="compilation"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		protected AnalyzerTest(TestableCompilationData compilation, bool addInitialSources = true) : base(compilation, addInitialSources)
		{
		}

		/// <summary>
		/// Asynchronously executes a new instance of <typeparamref name="T"/>.
		/// </summary>
		/// <param name="input">Input for the analyzer.</param>
		/// <param name="addToCompilation">Determines whether to add the <see cref="CSharpSyntaxTree"/> created from the <paramref name="input"/> to the <see cref="CompilationTest.Compilation"/>.</param>
		protected async Task<ImmutableArray<Diagnostic>> RunAnalyzer(string? input, bool addToCompilation = false)
		{
			if (input is null)
			{
				return ImmutableArray.Create<Diagnostic>();
			}

			T analyzer = new();
			return await RunAnalyzer(analyzer, input, addToCompilation).ConfigureAwait(false);
		}

		/// <summary>
		/// Asynchronously executed the specified <paramref name="analyzer"/>.
		/// </summary>
		/// <param name="analyzer"><see cref="DiagnosticAnalyzer"/> to execute.</param>
		/// <param name="input">Input for the analyzer.</param>
		/// <param name="addToCompilation">Determines whether to add the <see cref="CSharpSyntaxTree"/> created from the <paramref name="input"/> to the <see cref="CompilationTest.Compilation"/>.</param>
		protected async Task<ImmutableArray<Diagnostic>> RunAnalyzer(T analyzer, string? input, bool addToCompilation = false)
		{
			if (analyzer is null || input is null)
			{
				return ImmutableArray.Create<Diagnostic>();
			}

			ImmutableArray<DiagnosticAnalyzer> collection = ImmutableArray.Create<DiagnosticAnalyzer>(analyzer);
			CSharpCompilation compilation;

			if (addToCompilation)
			{
				Compilation.UpdateCompilation(input);
				compilation = Compilation.CurrentCompilation;
			}
			else
			{
				compilation = Compilation.CurrentCompilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(input, encoding: Encoding.UTF8));
			}

			AnalysisResult result = await compilation.WithAnalyzers(collection).GetAnalysisResultAsync(default).ConfigureAwait(false);
			return result.GetAllDiagnostics(analyzer);
		}

		/// <summary>
		/// Performs analysis on a <see cref="CSharpCompilation"/> containing the specified <paramref name="input"/> text and a reference to assembly containing  the given <paramref name="external"/> text.
		/// </summary>
		/// <param name="input">Input for the analyzer.</param>
		/// <param name="external">Text representing code in an external assembly.</param>
		/// <exception cref="InvalidOperationException">Emit failed.</exception>
		protected async Task<ImmutableArray<Diagnostic>> RunAnalyzerWithDependency(string input, string external)
		{
			CSharpCompilation dependency = RoslynUtilities.CreateCompilation(external);

			AddInitialSources(ref dependency);

			CSharpCompilation current = RoslynUtilities.CreateCompilationWithDependency(input, dependency);

			AddInitialSources(ref current);

			T analyzer = new();

			AnalysisResult result = await current
				.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer))
				.GetAnalysisResultAsync(default)
				.ConfigureAwait(false);

			return result.GetAllDiagnostics(analyzer);
		}
	}
}
