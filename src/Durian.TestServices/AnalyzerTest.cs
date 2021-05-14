﻿using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Tests
{
	/// <summary>
	/// An abstract class that provides methods to test <see cref="DiagnosticAnalyzer"/>s.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="DiagnosticAnalyzer"/> this <see cref="AnalyzerTest{T}"/> supports.</typeparam>
	public abstract class AnalyzerTest<T> : CompilationTest where T : DiagnosticAnalyzer
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
		protected AnalyzerTest(TestableCompilationData compilation) : base(compilation)
		{
		}

		/// <summary>
		/// Creates a new instance of the <typeparamref name="T"/> class.
		/// </summary>
		protected abstract T CreateAnalyzer();

		/// <summary>
		/// Asynchronously executes the analyzer created using the <see cref="CreateAnalyzer"/> method.
		/// </summary>
		/// <param name="input">Input for the analyzer.</param>
		/// <param name="addToCompilation">Determines whether to add the <see cref="CSharpSyntaxTree"/> created from the <paramref name="input"/> to the <see cref="CompilationTest.Compilation"/>.</param>
		public async Task<ImmutableArray<Diagnostic>> RunAnalyzerAsync(string? input, bool addToCompilation = true)
		{
			if (input is null)
			{
				return ImmutableArray.Create<Diagnostic>();
			}

			T analyzer = CreateAnalyzer();
			return await RunAnalyzerAsync(analyzer, input, addToCompilation).ConfigureAwait(false);
		}

		/// <summary>
		/// Asynchronously executed the specified <paramref name="analyzer"/>.
		/// </summary>
		/// <param name="analyzer"><see cref="DiagnosticAnalyzer"/> to execute.</param>
		/// <param name="input">Input for the analyzer.</param>
		/// <param name="addToCompilation">Determines whether to add the <see cref="CSharpSyntaxTree"/> created from the <paramref name="input"/> to the <see cref="CompilationTest.Compilation"/>.</param>
		public async Task<ImmutableArray<Diagnostic>> RunAnalyzerAsync(T analyzer, string? input, bool addToCompilation = true)
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
	}
}