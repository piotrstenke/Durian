using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.SyntaxVisitors;

/// <summary>
/// Replaces <see cref="SyntaxTrivia"/>s of the specified type with the provided <see cref="Replacement"/>.
/// </summary>
public abstract class TriviaReplacer : CSharpSyntaxRewriter
{
	/// <summary>
	/// <see cref="SyntaxTrivia"/> that is the replacement.
	/// </summary>
	public SyntaxTrivia Replacement { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="TriviaReplacer"/> class.
	/// </summary>
	/// <param name="visitIntoStructuredTrivia">Determines whether to visit nodes that are part of a structured trivia.</param>
	protected TriviaReplacer(bool visitIntoStructuredTrivia = false) : base(visitIntoStructuredTrivia)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="TriviaReplacer"/> class.
	/// </summary>
	/// <param name="replacement"><see cref="SyntaxTrivia"/> that is the replacement.</param>
	/// <param name="visitIntoStructuredTrivia">Determines whether to visit nodes that are part of a structured trivia.</param>
	protected TriviaReplacer(in SyntaxTrivia replacement, bool visitIntoStructuredTrivia = false) : base(visitIntoStructuredTrivia)
	{
		Replacement = replacement;
	}
}
