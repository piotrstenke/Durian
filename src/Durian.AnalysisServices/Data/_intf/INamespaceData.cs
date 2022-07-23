// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.CodeGeneration;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="INamespaceSymbol"/>.
	/// </summary>
	public interface INamespaceData : INamespaceOrTypeData, ISymbolOrMember<INamespaceSymbol, INamespaceData>
	{
		/// <summary>
		/// Target <see cref="BaseNamespaceDeclarationSyntax"/>.
		/// </summary>
		new BaseNamespaceDeclarationSyntax Declaration { get; }

		/// <summary>
		/// Style of this namespace declaration.
		/// </summary>
		NamespaceStyle DeclarationStyle { get; }

		/// <summary>
		/// <see cref="INamespaceSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		new INamespaceSymbol Symbol { get; }

		/// <summary>
		/// Returns all <see cref="INamespaceOrTypeSymbol"/>s contained within this namespace.
		/// </summary>
		/// <param name="members">Range of members to include.</param>
		ISymbolContainer<INamespaceOrTypeSymbol, INamespaceOrTypeData> GetMembers(IncludedMembers members);

		/// <summary>
		/// Returns all <see cref="INamespaceSymbol"/>s contained within this namespace.
		/// </summary>
		/// <param name="members">Range of members to include.</param>
		ISymbolContainer<INamespaceSymbol, INamespaceData> GetNamespaces(IncludedMembers members);
	}
}
