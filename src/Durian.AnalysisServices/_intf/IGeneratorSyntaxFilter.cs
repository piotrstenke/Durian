using System.Collections.Generic;
using Durian.Generator.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Generator
{
	/// <summary>
	/// <see cref="ISyntaxFilter"/> that filtrates <see cref="CSharpSyntaxNode"/>s for the specified <see cref="IDurianSourceGenerator"/>.
	/// </summary>
	public interface IGeneratorSyntaxFilter : ISyntaxFilter
	{
		/// <summary>
		/// <see cref="IDurianSourceGenerator"/> this <see cref="IGeneratorSyntaxFilter"/> takes the <see cref="CSharpSyntaxNode"/>s from.
		/// </summary>
		IDurianSourceGenerator Generator { get; }

		/// <summary>
		/// Determines whether <see cref="ISymbol"/>s should be generated right after a <see cref="IMemberData"/> is created instead of waiting for all the <see cref="IMemberData"/>s to be returned.
		/// </summary>
		public bool IncludeGeneratedSymbols { get; }

		/// <summary>
		/// Decides, which <see cref="CSharpSyntaxNode"/>s collected by the <see cref="IDurianSourceGenerator.SyntaxReceiver"/> of the <see cref="Generator"/> are valid and returns a collection of <see cref="IMemberData"/> based on those <see cref="CSharpSyntaxNode"/>s.
		/// </summary>
		IMemberData[] Filtrate();

		/// <summary>
		/// Returns an <see cref="IEnumerator{T}"/> that allows to manually iterate through the filtrated <see cref="IMemberData"/>s.
		/// </summary>
		IEnumerator<IMemberData> GetEnumerator();
	}
}
