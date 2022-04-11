// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Threading;

namespace Durian.Analysis.Filters
{
	/// <summary>
	/// Data that is passed to the <see cref="IValidationContextProvider{T}.TryGetContext(in ValidationDataContext, out T)"/> method.
	/// </summary>
	public readonly struct ValidationDataContext
	{
		/// <summary>
		/// <see cref="System.Threading.CancellationToken"/> that specifies if the operation should be canceled.
		/// </summary>
		public CancellationToken CancellationToken { get; }

		/// <summary>
		/// <see cref="CSharpSyntaxNode"/> to get the data of.
		/// </summary>
		public CSharpSyntaxNode Node { get; }

		/// <summary>
		/// Parent <see cref="ICompilationData"/> of the target <see cref="Node"/>.
		/// </summary>
		public ICompilationData TargetCompilation { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ValidationDataContext"/> structure.
		/// </summary>
		/// <param name="node"><see cref="CSharpSyntaxNode"/> to get the data of.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of the target <paramref name="node"/>.</param>
		/// <param name="cancellationToken"><see cref="System.Threading.CancellationToken"/> that specifies if the operation should be canceled.</param>
		public ValidationDataContext(CSharpSyntaxNode node, ICompilationData compilation, CancellationToken cancellationToken = default)
		{
			Node = node;
			TargetCompilation = compilation;
			CancellationToken = cancellationToken;
		}

		/// <summary>
		/// Converts the current <see cref="ValidationDataContext"/> to a <see cref="SyntaxValidationContext"/>.
		/// </summary>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the current <see cref="Node"/>.</param>
		/// <param name="symbol"><see cref="ISymbol"/> that is represented by the current <see cref="Node"/>.</param>
		public SyntaxValidationContext ToSyntaxContext(SemanticModel semanticModel, ISymbol symbol)
		{
			return new(TargetCompilation, semanticModel, symbol, Node, CancellationToken);
		}
	}
}
