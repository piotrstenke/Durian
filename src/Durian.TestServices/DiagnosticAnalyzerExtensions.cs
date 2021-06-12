// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Tests
{
	/// <summary>
	/// Contains various extensions methods for the <see cref="DiagnosticAnalyzer"/> class.
	/// </summary>
	public static class DiagnosticAnalyzerExtensions
	{
		/// <summary>
		/// Runs the specified <paramref name="analyzer"/> and checks if it produces <see cref="Diagnostic"/> described by the specified <paramref name="descriptor"/>.
		/// </summary>
		/// <param name="analyzer"><see cref="DiagnosticAnalyzer"/> to run and check.</param>
		/// <param name="compilation"><see cref="Compilation"/> that is the input for the <paramref name="analyzer"/>.</param>
		/// <param name="descriptor"><see cref="DiagnosticDescriptor"/> of the target <see cref="Diagnostic"/>.</param>
		public static Task<bool> ProducesDiagnostic(this DiagnosticAnalyzer analyzer, Compilation compilation, DiagnosticDescriptor descriptor)
		{
			return ProducesDiagnostic(analyzer, compilation, descriptor.Id);
		}

		/// <summary>
		/// Runs the specified <paramref name="analyzer"/> and checks if it produces <see cref="Diagnostic"/> with the specified <paramref name="id"/>.
		/// </summary>
		/// <param name="analyzer"><see cref="DiagnosticAnalyzer"/> to run and check.</param>
		/// <param name="compilation"><see cref="Compilation"/> that is the input for the <paramref name="analyzer"/>.</param>
		/// <param name="id">Id of <see cref="Diagnostic"/> to check for.</param>
		public static Task<bool> ProducesDiagnostic(this DiagnosticAnalyzer analyzer, Compilation compilation, string id)
		{
			return RunAnalyzer(analyzer, compilation).ContinueWith(task => task.Result.Any(d => d.Id == id));
		}

		/// <summary>
		/// Runs the specified <paramref name="analyzer"/> and returns an <see cref="ImmutableArray{T}"/> of produced <see cref="Diagnostic"/>s.
		/// </summary>
		/// <param name="analyzer"><see cref="DiagnosticAnalyzer"/> to run.</param>
		/// <param name="compilation"><see cref="Compilation"/> that is the input for the <paramref name="analyzer"/>.</param>
		public static Task<ImmutableArray<Diagnostic>> RunAnalyzer(this DiagnosticAnalyzer analyzer, Compilation compilation)
		{
			return compilation
				.WithAnalyzers(ImmutableArray.Create(analyzer))
				.GetAnalyzerDiagnosticsAsync();
		}

		/// <summary>
		/// Runs the specified <paramref name="analyzer"/> and returns an <see cref="ImmutableArray{T}"/> of produced <see cref="Diagnostic"/>s.
		/// </summary>
		/// <param name="analyzer"><see cref="DiagnosticAnalyzer"/> to run.</param>
		/// <param name="source">A <see cref="string"/> representing a <see cref="CSharpSyntaxTree"/> the analysis should be performed on.</param>
		public static Task<ImmutableArray<Diagnostic>> RunAnalyzer(this DiagnosticAnalyzer analyzer, string? source)
		{
			CSharpCompilation compilation = RoslynUtilities.CreateBaseCompilation();

			if (!string.IsNullOrWhiteSpace(source))
			{
				compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(source!));
			}

			return RunAnalyzer(analyzer, compilation);
		}

		/// <summary>
		/// Runs the specified <paramref name="analyzer"/> and returns an <see cref="ImmutableArray{T}"/> of produced <see cref="Diagnostic"/>s.
		/// </summary>
		/// <param name="analyzer"><see cref="DiagnosticAnalyzer"/> to run.</param>
		/// <param name="tree">A <see cref="CSharpSyntaxTree"/> the analysis should be performed on.</param>
		public static Task<ImmutableArray<Diagnostic>> RunAnalyzer(this DiagnosticAnalyzer analyzer, CSharpSyntaxTree? tree)
		{
			CSharpCompilation compilation = RoslynUtilities.CreateBaseCompilation();

			if (tree is not null)
			{
				compilation = compilation.AddSyntaxTrees(tree);
			}

			return RunAnalyzer(analyzer, compilation);
		}
	}
}
