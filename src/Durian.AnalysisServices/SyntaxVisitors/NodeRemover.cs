// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.SyntaxVisitors
{
	/// <summary>
	/// Removes nodes of the specified type from the syntax tree.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="CSharpSyntaxNode"/> to remove.</typeparam>
	public class NodeRemover<T> : CSharpSyntaxRewriter where T : CSharpSyntaxNode
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NodeRemover{T}"/> class.
		/// </summary>
		/// <param name="visitIntoStructedTrivia">Determines whether to visit nodes that are part of a structured trivia.</param>
		public NodeRemover(bool visitIntoStructedTrivia = false) : base(visitIntoStructedTrivia)
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
