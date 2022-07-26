// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="INamedTypeSymbol"/> representing an interface.
	/// </summary>
	public interface IInterfaceData : ITypeData, ISymbolOrMember<INamedTypeSymbol, IInterfaceData>
	{
		/// <summary>
		/// Target <see cref="InterfaceDeclarationSyntax"/>.
		/// </summary>
		new InterfaceDeclarationSyntax Declaration { get; }

		/// <summary>
		/// Members of the interface that are default-implemented.
		/// </summary>
		ISymbolContainer<ISymbol, IMemberData> DefaultImplementations { get; }

		/// <summary>
		/// Creates a shallow copy of the current data.
		/// </summary>
		new IInterfaceData Clone();
	}
}