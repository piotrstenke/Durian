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
	public interface INamespaceData : IMemberData
	{
		/// <summary>
		/// Target <see cref="BaseNamespaceDeclarationSyntax"/>.
		/// </summary>
		new BaseNamespaceDeclarationSyntax Declaration { get; }

		/// <summary>
		/// <see cref="INamespaceSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		new INamespaceSymbol Symbol { get; }

		/// <summary>
		/// Style of this namespace declaration.
		/// </summary>
		NamespaceStyle DeclarationStyle { get; }

		/// <summary>
		/// All <see cref="INamespaceSymbol"/>s that are contained directly within this namespace.
		/// </summary>
		ISymbolContainer<INamespaceSymbol, INamespaceData> SubNamespaces { get; }

		/// <summary>
		/// All <see cref="INamespaceSymbol"/>s that are contained within this namespace or one if its sub namespaces.
		/// </summary>
		ISymbolContainer<INamespaceSymbol, INamespaceData> AllSubNamespaces { get; }

		/// <summary>
		/// All <see cref="INamedTypeSymbol"/>s that are contained directly within this namespace.
		/// </summary>
		ISymbolContainer<INamedTypeSymbol, ITypeData> Types { get; }

		/// <summary>
		/// All <see cref="INamedTypeSymbol"/> that are contained either within this namespace, one of its sub namespaces or one of its inner types.
		/// </summary>
		ISymbolContainer<INamedTypeSymbol, ITypeData> AllTypes { get; }
	}
}
