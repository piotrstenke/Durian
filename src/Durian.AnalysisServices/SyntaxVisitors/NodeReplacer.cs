// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.SyntaxVisitors
{
	/// <summary>
	/// Replaces <see cref="CSharpSyntaxNode"/>s of the specified type with the provided <see cref="Replacement"/>.
	/// </summary>
	public abstract class NodeReplacer : CSharpSyntaxRewriter
	{
		/// <summary>
		/// <see cref="CSharpSyntaxNode"/> that is the replacement.
		/// </summary>
		public CSharpSyntaxNode? Replacement { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="NodeReplacer"/> class.
		/// </summary>
		/// <param name="visitIntoStructedTrivia">Determines whether to visit nodes that are part of a structured trivia.</param>
		protected NodeReplacer(bool visitIntoStructedTrivia = false) : base(visitIntoStructedTrivia)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NodeReplacer"/> class.
		/// </summary>
		/// <param name="replacement"><see cref="CSharpSyntaxNode"/> that is the replacement.</param>
		/// <param name="visitIntoStructedTrivia">Determines whether to visit nodes that are part of a structured trivia.</param>
		protected NodeReplacer(CSharpSyntaxNode? replacement, bool visitIntoStructedTrivia = false) : base(visitIntoStructedTrivia)
		{
			Replacement = replacement;
		}
	}
}
