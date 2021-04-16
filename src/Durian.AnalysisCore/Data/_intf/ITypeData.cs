using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="TypeDeclarationSyntax"/>.
	/// </summary>
	public interface ITypeData : IMemberData
	{
		/// <summary>
		/// <see cref="INamedTypeSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		new INamedTypeSymbol Symbol { get; }

		/// <summary>
		/// Target <see cref="TypeDeclarationSyntax"/>.
		/// </summary>
		new TypeDeclarationSyntax Declaration { get; }

		/// <summary>
		/// All modifiers of the <see cref="Symbol"/>.
		/// </summary>
		SyntaxToken[] Modifiers { get; }

		/// <summary>
		/// If the type is partial, returns all declarations of the type (including <see cref="IMemberData.Declaration"/>), otherwise returns only <see cref="IMemberData.Declaration"/>.
		/// </summary>
		IEnumerable<TypeDeclarationSyntax> GetPartialDeclarations();

		/// <summary>
		/// Returns all <see cref="ITypeData"/>s that contain the <see cref="Symbol"/>.
		/// </summary>
		/// <param name="includeSelf">Determines whether the <see cref="ITypeData"/> should return itself as well.</param>
		IEnumerable<ITypeData> GetContainingTypes(bool includeSelf);
	}
}
