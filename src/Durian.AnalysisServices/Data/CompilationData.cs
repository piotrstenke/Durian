// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Basic implementation of the <see cref="ICompilationData"/> interface.
	/// </summary>
	public class CompilationData : ICompilationData
	{
		/// <inheritdoc/>
		public CSharpCompilation Compilation { get; private set; }

		/// <inheritdoc/>
		public virtual bool HasErrors { get; protected set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CompilationData"/> class.
		/// </summary>
		/// <param name="compilation">Current <see cref="CSharpCompilation"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
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
				CSharpCompilation compilation = Compilation;
				Compilation = Compilation.AddSyntaxTrees(tree);
				OnUpdate(compilation);
			}
		}

		/// <inheritdoc/>
		public void UpdateCompilation(CSharpSyntaxTree? original, CSharpSyntaxTree? updated)
		{
			if (original is null || updated is null)
			{
				return;
			}

			CSharpCompilation compilation = Compilation;
			Compilation = Compilation.ReplaceSyntaxTree(original, updated);
			OnUpdate(compilation);
		}

		/// <inheritdoc/>
		public void UpdateCompilation(IEnumerable<CSharpSyntaxTree>? trees)
		{
			if (trees is not null)
			{
				CSharpCompilation compilation = Compilation;
				Compilation = Compilation.AddSyntaxTrees(trees);
				OnUpdate(compilation);
			}
		}

		/// <inheritdoc/>
		public void UpdateCompilation(MetadataReference? reference)
		{
			if (reference is not null)
			{
				CSharpCompilation compilation = Compilation;
				Compilation = Compilation.AddReferences(reference);
				OnUpdate(compilation);
			}
		}

		/// <inheritdoc/>
		public void UpdateCompilation(MetadataReference? original, MetadataReference? updated)
		{
			if (original is null || updated is null)
			{
				return;
			}

			CSharpCompilation compilation = Compilation;
			Compilation = Compilation.ReplaceReference(original, updated);
			OnUpdate(compilation);
		}

		/// <inheritdoc/>
		public void UpdateCompilation(IEnumerable<MetadataReference>? references)
		{
			if (references is not null)
			{
				CSharpCompilation compilation = Compilation;
				Compilation = Compilation.AddReferences(references);
				OnUpdate(compilation);
			}
		}

		/// <summary>
		/// Methods executed when the <see cref="Compilation"/> has changed using one of the <c>Update...</c> methods.
		/// </summary>
		/// <param name="oldCompilation"><see cref="CSharpCompilation"/> that was updated.</param>
		protected virtual void OnUpdate(CSharpCompilation oldCompilation)
		{
			// Do nothing by default.
		}
	}
}