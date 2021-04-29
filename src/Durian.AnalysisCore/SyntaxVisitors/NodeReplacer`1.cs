using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.SyntaxVisitors
{
	/// <summary>
	/// Replaces <see cref="CSharpSyntaxNode"/>s of the specified type with the provided <see cref="Replacement"/>.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="CSharpSyntaxNode"/> this <see cref="NodeReplacer{T}"/> accepts as replacement.</typeparam>
	public abstract class NodeReplacer<T> : CSharpSyntaxRewriter where T : notnull, CSharpSyntaxNode
	{
		/// <summary>
		/// <see cref="CSharpSyntaxNode"/> that is the replacement.
		/// </summary>
		[MaybeNull]
		public virtual T Replacement { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="NodeReplacer{T}"/> class.
		/// </summary>
		protected NodeReplacer()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NodeReplacer{T}"/> class.
		/// </summary>
		/// <param name="replacement"><see cref="CSharpSyntaxNode"/> that is the replacement.</param>
		protected NodeReplacer(T replacement)
		{
			Replacement = replacement;
		}
	}
}
