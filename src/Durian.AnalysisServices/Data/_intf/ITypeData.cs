// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Immutable;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="INamedTypeSymbol"/>.
	/// </summary>
	public interface ITypeData : IGenericMemberData, INamespaceOrTypeData, ISymbolOrMember<INamedTypeSymbol, ITypeData>
	{
		/// <summary>
		/// <see cref="ITypeSymbol"/> associated with the <see cref="IMemberData.Declaration"/>.
		/// </summary>
		new INamedTypeSymbol Symbol { get; }

		/// <summary>
		/// All partial declarations of the type (including <see cref="IMemberData.Declaration"/>).
		/// </summary>
		ImmutableArray<TypeDeclarationSyntax> PartialDeclarations { get; }

		/// <summary>
		/// Member <see cref="ISymbol"/>s of the current type and its parent types.
		/// </summary>
		ISymbolContainer<ISymbol, IMemberData> AllMembers { get; }

		/// <summary>
		/// Member <see cref="ISymbol"/>s of the current type.
		/// </summary>
		ISymbolContainer<ISymbol, IMemberData> Members { get; }

		/// <summary>
		/// Base types of the current type.
		/// </summary>
		ISymbolContainer<INamedTypeSymbol, ITypeData> BaseTypes { get; }

		/// <summary>
		/// Inner types of the current type and its parent types.
		/// </summary>
		ISymbolContainer<INamedTypeSymbol, ITypeData> AllInnerTypes { get; }

		/// <summary>
		/// Inner types of the current type.
		/// </summary>
		ISymbolContainer<INamedTypeSymbol, ITypeData> InnerTypes { get; }

		/// <summary>
		/// Parameterless constructor of this type.
		/// </summary>
		ISymbolOrMember<IMethodSymbol, IMethodData>? ParameterlessConstructor { get; }


		ISymbolContainer<ISymbol, IMemberData> GetMembers(IncludedMembers members);

		ISymbolContainer<INamedTypeSymbol, ITypeData> GetInnerTypes(IncludedMembers members);
	}
}
