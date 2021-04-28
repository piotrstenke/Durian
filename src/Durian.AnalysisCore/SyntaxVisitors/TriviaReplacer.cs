﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.SyntaxVisitors
{
	/// <summary>
	/// Replaces <see cref="SyntaxTrivia"/>s of the specified type with the provided <see cref="Replacement"/>.
	/// </summary>
	public abstract class TriviaReplacer : CSharpSyntaxRewriter
	{
		/// <summary>
		/// <see cref="SyntaxTrivia"/> that is the replacement.
		/// </summary>
		public virtual SyntaxTrivia Replacement { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="TriviaReplacer"/> class.
		/// </summary>
		protected TriviaReplacer()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TriviaReplacer"/> class.
		/// </summary>
		/// <param name="replacement"><see cref="SyntaxTrivia"/> that is the replacement.</param>
		protected TriviaReplacer(in SyntaxTrivia replacement)
		{
			Replacement = replacement;
		}
	}
}
