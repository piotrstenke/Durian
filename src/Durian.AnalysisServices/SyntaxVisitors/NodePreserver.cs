// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.SyntaxVisitors
{
	/// <summary>
	/// Removes all nodes from the syntax tree except for nodes of the specified type.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="SyntaxNode"/> to preserve</typeparam>
	public class NodePreserver<T> : CSharpSyntaxRewriter where T : SyntaxNode
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NodePreserver{T}"/> class.
		/// </summary>
		/// <param name="visitIntoStructedTrivia">Determines whether to visit nodes that are part of a structured trivia.</param>
		public NodePreserver(bool visitIntoStructedTrivia = false) : base(visitIntoStructedTrivia)
		{
		}

		/// <inheritdoc/>
		public override SyntaxNode? Visit(SyntaxNode? node)
		{
			if (node is not T)
			{
				return base.Visit(node);
			}

			return null;
		}
	}
}
