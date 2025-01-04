// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.SyntaxVisitors
{
	/// <summary>
	/// Removes nodes of the specified type from the syntax tree.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="SyntaxNode"/> to remove.</typeparam>
	public class NodeRemover<T> : CSharpSyntaxRewriter where T : SyntaxNode
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NodeRemover{T}"/> class.
		/// </summary>
		/// <param name="visitIntoStructuredTrivia">Determines whether to visit nodes that are part of a structured trivia.</param>
		public NodeRemover(bool visitIntoStructuredTrivia = false) : base(visitIntoStructuredTrivia)
		{
		}

		/// <inheritdoc/>
		public override SyntaxNode? Visit(SyntaxNode? node)
		{
			if (node is T)
			{
				return base.Visit(node);
			}

			return null;
		}
	}
}
