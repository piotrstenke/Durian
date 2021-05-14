using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Generator.SyntaxVisitors
{
	/// <summary>
	/// Replaces node of the specified type with the provided <see cref="SyntaxNodeOrToken"/>.
	/// </summary>
	public abstract class SyntaxReplacer : CSharpSyntaxRewriter
	{
		/// <summary>
		/// <see cref="SyntaxNodeOrToken"/> that is the replacement.
		/// </summary>
		public SyntaxNodeOrToken Replacement { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="SyntaxReplacer"/> class.
		/// </summary>
		protected SyntaxReplacer()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SyntaxReplacer"/> class.
		/// </summary>
		/// <param name="replacement"><see cref="SyntaxNodeOrToken"/> that is the replacement.</param>
		protected SyntaxReplacer(SyntaxNodeOrToken replacement)
		{
			Replacement = replacement;
		}
	}
}
