using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Generator.SyntaxVisitors
{
	/// <summary>
	/// Replaces <see cref="SyntaxToken"/>s of the specified type with the provided <see cref="Replacement"/>.
	/// </summary>
	public abstract class TokenReplacer : CSharpSyntaxRewriter
	{
		/// <summary>
		/// <see cref="SyntaxToken"/> that is the replacement.
		/// </summary>
		public virtual SyntaxToken Replacement { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="TokenReplacer"/> class.
		/// </summary>
		protected TokenReplacer()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TokenReplacer"/> class.
		/// </summary>
		/// <param name="replacement"><see cref="SyntaxToken"/> that is the replacement.</param>
		protected TokenReplacer(in SyntaxToken replacement)
		{
			Replacement = replacement;
		}
	}
}
