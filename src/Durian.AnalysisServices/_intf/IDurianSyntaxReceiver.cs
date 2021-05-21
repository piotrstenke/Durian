using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Generator
{
	/// <summary>
	/// <see cref="ISyntaxReceiver"/> that provides an additional method for checking if any <see cref="CSharpSyntaxNode"/>s were collected.
	/// </summary>
	public interface IDurianSyntaxReceiver : ISyntaxReceiver
	{
		/// <summary>
		/// Determines whether the <see cref="ISyntaxReceiver"/> is empty, i.e. it didn't collect any <see cref="CSharpSyntaxNode"/>s.
		/// </summary>
		bool IsEmpty();

		/// <summary>
		/// Returns a collection of <see cref="CSharpSyntaxNode"/>s that were collected by this <see cref="IDurianSyntaxReceiver"/>.
		/// </summary>
		IEnumerable<CSharpSyntaxNode> GetCollectedNodes();
	}
}
