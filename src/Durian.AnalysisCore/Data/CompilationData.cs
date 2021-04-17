using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Data
{
	/// <summary>
	/// Basic implementation of the <see cref="ICompilationData"/> interface.
	/// </summary>
	public class CompilationData : ICompilationData
	{
		/// <inheritdoc/>
		public CSharpCompilation Compilation { get; private set; }

		/// <inheritdoc/>
		public virtual bool HasErrors => false;

		/// <summary>
		/// Initializes a new instance of the <see cref="CompilationData"/> class.
		/// </summary>
		/// <param name="compilation">Current <see cref="Microsoft.CodeAnalysis.Compilation"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <c>null</c>.</exception>
		public CompilationData(CSharpCompilation compilation)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			Compilation = compilation;
		}

		/// <inheritdoc/>
		public void UpdateCompilation(CSharpSyntaxTree? tree)
		{
			if (tree is not null)
			{
				Compilation = Compilation.AddSyntaxTrees(tree);
			}
		}

		/// <inheritdoc/>
		public void UpdateCompilation(CSharpSyntaxTree? original, CSharpSyntaxTree? updated)
		{
			if (original is null || updated is null)
			{
				return;
			}

			Compilation = Compilation.ReplaceSyntaxTree(original, updated);
		}

		/// <inheritdoc/>
		public void UpdateCompilation(IEnumerable<CSharpSyntaxTree>? trees)
		{
			if (trees is not null)
			{
				Compilation = Compilation.AddSyntaxTrees(trees);
			}
		}

		/// <inheritdoc/>
		public void UpdateCompilation(MetadataReference? reference)
		{
			if (reference is not null)
			{
				Compilation = Compilation.AddReferences(reference);
			}
		}

		/// <inheritdoc/>
		public void UpdateCompilation(MetadataReference? original, MetadataReference? updated)
		{
			if (original is null || updated is null)
			{
				return;
			}

			Compilation = Compilation.ReplaceReference(original, updated);
		}

		/// <inheritdoc/>
		public void UpdateCompilation(IEnumerable<MetadataReference>? references)
		{
			if (references is not null)
			{
				Compilation = Compilation.AddReferences(references);
			}
		}
	}
}
