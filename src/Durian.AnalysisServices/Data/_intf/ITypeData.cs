// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="TypeDeclarationSyntax"/>.
	/// </summary>
	public interface ITypeData : IMemberData
	{
		/// <summary>
		/// Target <see cref="TypeDeclarationSyntax"/>.
		/// </summary>
		new BaseTypeDeclarationSyntax Declaration { get; }

		/// <summary>
		/// All modifiers of the <see cref="Symbol"/>.
		/// </summary>
		SyntaxToken[] Modifiers { get; }

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		new INamedTypeSymbol Symbol { get; }

		/// <summary>
		/// Returns all <see cref="ITypeData"/>s that contain the <see cref="Symbol"/>.
		/// </summary>
		/// <param name="includeSelf">Determines whether the <see cref="ITypeData"/> should return itself as well.</param>
		IEnumerable<ITypeData> GetContainingTypes(bool includeSelf);

		/// <summary>
		/// If the type is partial, returns all declarations of the type (including <see cref="IMemberData.Declaration"/>), otherwise returns only <see cref="IMemberData.Declaration"/>.
		/// <para>If the type is not <see cref="TypeDeclarationSyntax"/>, an empty collection is returned instead.</para>
		/// </summary>
		IEnumerable<TypeDeclarationSyntax> GetPartialDeclarations();
	}
}
