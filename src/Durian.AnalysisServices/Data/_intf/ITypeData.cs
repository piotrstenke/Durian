// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="BaseTypeDeclarationSyntax"/>.
	/// </summary>
	public interface ITypeData : IMemberData
	{
		/// <summary>
		/// Target <see cref="TypeDeclarationSyntax"/>.
		/// </summary>
		new BaseTypeDeclarationSyntax Declaration { get; }

		/// <summary>
		/// <see cref="ITypeSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		new ITypeSymbol Symbol { get; }

		/// <summary>
		/// If the type is partial, returns all declarations of the type (including <see cref="IMemberData.Declaration"/>), otherwise returns only <see cref="IMemberData.Declaration"/>.
		/// <para>If the type is not <see cref="BaseTypeDeclarationSyntax"/>, an empty collection is returned instead.</para>
		/// </summary>
		ImmutableArray<BaseTypeDeclarationSyntax> GetPartialDeclarations();
	}
}
