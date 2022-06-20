// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Immutable;
using Durian.Analysis.SymbolContainers;
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
		/// Target <see cref="BaseTypeDeclarationSyntax"/>.
		/// </summary>
		new BaseTypeDeclarationSyntax Declaration { get; }

		/// <summary>
		/// <see cref="ITypeSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		new ITypeSymbol Symbol { get; }

		/// <summary>
		/// All partial declarations of the type (including <see cref="Declaration"/>).
		/// </summary>
		ImmutableArray<BaseTypeDeclarationSyntax> PartialDeclarations { get; }

		/// <summary>
		/// Member <see cref="ISymbol"/>s of the current type and its parent types.
		/// </summary>
		ISymbolContainer<ISymbol> AllMembers { get; }

		/// <summary>
		/// Member <see cref="ISymbol"/>s of the current type.
		/// </summary>
		ISymbolContainer<ISymbol> Members { get; }

		/// <summary>
		/// Base types of the current type.
		/// </summary>
		ISymbolContainer<INamedTypeSymbol> BaseTypes { get; }

		/// <summary>
		/// Inner types of the current type and its parent types.
		/// </summary>
		ISymbolContainer<INamedTypeSymbol> AllInnerTypes { get; }

		/// <summary>
		/// Inner types of the current type.
		/// </summary>
		ISymbolContainer<INamedTypeSymbol> InnerTypes { get; }

		/// <summary>
		/// Parameterless constructor of this type.
		/// </summary>
		ISymbolOrMember<IMethodSymbol, IMethodData>? ParameterlessConstructor { get; }

		/// <summary>
		/// Type parameters of this type.
		/// </summary>
		ISymbolContainer<ITypeParameterSymbol> TypeParameters { get; }

		/// <summary>
		/// Type arguments of this type.
		/// </summary>
		ISymbolContainer<ITypeSymbol> TypeArguments { get; }
	}
}
