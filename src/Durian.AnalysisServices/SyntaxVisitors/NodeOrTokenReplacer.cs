using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.SyntaxVisitors;

/// <summary>
/// Replaces node of the specified type with the provided <see cref="SyntaxNodeOrToken"/>.
/// </summary>
public abstract class NodeOrTokenReplacer : CSharpSyntaxRewriter
{
	/// <summary>
	/// <see cref="SyntaxNodeOrToken"/> that is the replacement.
	/// </summary>
	public SyntaxNodeOrToken Replacement { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="NodeOrTokenReplacer"/> class.
	/// </summary>
	/// <param name="visitIntoStructuredTrivia">Determines whether to visit nodes that are part of a structured trivia.</param>
	protected NodeOrTokenReplacer(bool visitIntoStructuredTrivia = false) : base(visitIntoStructuredTrivia)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="NodeOrTokenReplacer"/> class.
	/// </summary>
	/// <param name="replacement"><see cref="SyntaxNodeOrToken"/> that is the replacement.</param>
	/// <param name="visitIntoStructuredTrivia">Determines whether to visit nodes that are part of a structured trivia.</param>
	protected NodeOrTokenReplacer(SyntaxNodeOrToken replacement, bool visitIntoStructuredTrivia = false) : base(visitIntoStructuredTrivia)
	{
		Replacement = replacement;
	}
}
