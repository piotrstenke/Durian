using System.Collections.Generic;
using Durian.Data;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian
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
		/// Decides, which <see cref="CSharpSyntaxNode"/>s collected by the <see cref="IDurianSourceGenerator.SyntaxReceiver"/> of the <see cref="Generator"/> are valid and returns a collection of <see cref="IMemberData"/> based on those <see cref="CSharpSyntaxNode"/>s.
		/// </summary>
		IEnumerable<IMemberData> Filtrate();
	}
}
