using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.SyntaxVisitors
{
	/// <summary>
	/// Replaces <see cref="SyntaxNode"/>s of the specified type with the provided <see cref="Replacement"/>.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="SyntaxNode"/> this <see cref="NodeReplacer{T}"/> accepts as replacement.</typeparam>
	public abstract class NodeReplacer<T> : CSharpSyntaxRewriter where T : SyntaxNode
	{
		/// <summary>
		/// <see cref="SyntaxNode"/> that is the replacement.
		/// </summary>
		public T? Replacement { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="NodeReplacer{T}"/> class.
		/// </summary>
		/// <param name="visitIntoStructuredTrivia">Determines whether to visit nodes that are part of a structured trivia.</param>
		protected NodeReplacer(bool visitIntoStructuredTrivia = false) : base(visitIntoStructuredTrivia)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NodeReplacer{T}"/> class.
		/// </summary>
		/// <param name="replacement"><see cref="SyntaxNode"/> that is the replacement.</param>
		/// <param name="visitIntoStructuredTrivia">Determines whether to visit nodes that are part of a structured trivia.</param>
		protected NodeReplacer(T? replacement, bool visitIntoStructuredTrivia = false) : base(visitIntoStructuredTrivia)
		{
			Replacement = replacement;
		}
	}
}
