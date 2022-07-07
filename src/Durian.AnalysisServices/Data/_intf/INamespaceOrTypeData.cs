// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="INamespaceOrTypeData"/>.
	/// </summary>
	public interface INamespaceOrTypeData : IMemberData, ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>
	{
		/// <summary>
		/// <see cref="INamespaceOrTypeSymbol"/> associated with the <see cref="IMemberData.Declaration"/>.
		/// </summary>
		new INamespaceOrTypeSymbol Symbol { get; }

		/// <summary>
		/// Returns all <see cref="INamedTypeSymbol"/>s contained within this namespace.
		/// </summary>
		/// <param name="members">Range of members to include.</param>
		ISymbolContainer<INamedTypeSymbol, ITypeData> GetTypes(IncludedMembers members);

		/// <summary>
		/// Converts the current <see cref="INamespaceOrTypeData"/> to an actual <see cref="INamespaceData"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException">Current member is not a <see cref="INamespaceData"/>.</exception>
		INamespaceData ToNamespace();

		/// <summary>
		/// Converts the current <see cref="INamespaceOrTypeData"/> to an actual <see cref="ITypeData"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException">Current member is not a <see cref="ITypeData"/>.</exception>
		ITypeData ToType();
	}
}
