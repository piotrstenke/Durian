// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Immutable;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="SyntaxNode"/>.
	/// </summary>
	public interface IMemberData : ISymbolOrMember
	{
		/// <summary>
		/// Target <see cref="SyntaxNode"/>.
		/// </summary>
		SyntaxNode Declaration { get; }

		/// <summary>
		/// Determines whether the current member is declared using the <see langword="new"/> keyword.
		/// </summary>
		bool IsNew { get; }

		/// <summary>
		/// Determines whether the current member is declared using the <see langword="partial"/> keyword.
		/// </summary>
		bool IsPartial { get; }

		/// <summary>
		/// Determines whether the current member is declared using the <see langword="unsafe"/> keyword.
		/// </summary>
		bool IsUnsafe { get; }

		/// <summary>
		/// <see cref="Analysis.Virtuality"/> of the member.
		/// </summary>
		Virtuality Virtuality { get; }

		/// <summary>
		/// Member this member hides using the <see langword="new"/> keyword.
		/// </summary>
		ISymbolOrMember? HiddenSymbol { get; }

		/// <summary>
		/// Location of the member.
		/// </summary>
		Location? Location { get; }

		/// <summary>
		/// Name of the underlaying symbol including the verbatim identifier '@' token.
		/// </summary>
		string Name { get; }

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
		new ISymbol Symbol { get; }

		/// <summary>
		/// Data of all attributes applied to the <see cref="Symbol"/>.
		/// </summary>
		ImmutableArray<AttributeData> Attributes { get; }

		/// <summary>
		/// All <see cref="INamespaceSymbol"/>s that contain the <see cref="Symbol"/>.
		/// </summary>
		IWrittableSymbolContainer<INamespaceSymbol> ContainingNamespaces { get; }

		/// <summary>
		/// All <see cref="ITypeData"/>s that contain the <see cref="Symbol"/>.
		/// </summary>
		IWrittableSymbolContainer<INamedTypeSymbol> ContainingTypes { get; }

		/// <summary>
		/// All modifiers of the current symbol.
		/// </summary>
		string[] Modifiers { get; }
	}
}
