// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="MemberDeclarationSyntax"/>.
	/// </summary>
	public interface IMemberData
	{
		/// <summary>
		/// Target <see cref="MemberDeclarationSyntax"/>.
		/// </summary>
		MemberDeclarationSyntax Declaration { get; }

		/// <summary>
		/// Parent compilation of this <see cref="IMemberData"/>.
		/// </summary>
		ICompilationData ParentCompilation { get; }

		/// <summary>
		/// <see cref="Microsoft.CodeAnalysis.SemanticModel"/> of the <see cref="Declaration"/>.
		/// </summary>
		SemanticModel SemanticModel { get; }

		/// <summary>
		/// <see cref="ISymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		ISymbol Symbol { get; }

		/// <summary>
		/// Returns data of all attributes applied to the <see cref="Symbol"/>.
		/// </summary>
		ImmutableArray<AttributeData> GetAttributes();

		/// <summary>
		/// Returns all <see cref="INamespaceSymbol"/>s that contain the <see cref="Symbol"/>.
		/// </summary>
		IEnumerable<INamespaceSymbol> GetContainingNamespaces();

		/// <summary>
		/// Returns all <see cref="ITypeData"/>s that contain the <see cref="Symbol"/>.
		/// </summary>
		IEnumerable<ITypeData> GetContainingTypes();
	}
}
