// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="ISymbol"/>.
	/// </summary>
	public interface IMemberData : ISymbolOrMember
	{
		/// <summary>
		/// Data of all attributes applied to the <see cref="Symbol"/>.
		/// </summary>
		ImmutableArray<AttributeData> Attributes { get; }

		/// <summary>
		/// All <see cref="INamespaceSymbol"/>s that contain the <see cref="Symbol"/>.
		/// </summary>
		IWritableSymbolContainer<INamespaceSymbol, INamespaceData> ContainingNamespaces { get; }

		/// <summary>
		/// All <see cref="INamedTypeSymbol"/>s that contain the <see cref="Symbol"/>.
		/// </summary>
		IWritableSymbolContainer<INamedTypeSymbol, ITypeData> ContainingTypes { get; }

		/// <summary>
		/// Target <see cref="SyntaxNode"/>.
		/// </summary>
		SyntaxNode? Declaration { get; }

		/// <summary>
		/// Name of the underlaying symbol including the verbatim identifier '@' token and its type parameters.
		/// </summary>
		string GenericName { get; }

		/// <summary>
		/// Determines whether the current object represents a symbol that has an accessible declaration.
		/// </summary>
		[MemberNotNullWhen(true, nameof(Declaration))]
		bool HasDeclaration { get; }

		/// <summary>
		/// Member this member hides using the <see langword="new"/> keyword.
		/// </summary>
		ISymbolOrMember<ISymbol, IMemberData>? HiddenSymbol { get; }

		/// <summary>
		/// Determines whether the current member is declared using the <see langword="new"/> keyword.
		/// </summary>
		bool IsNew { get; }

		/// <summary>
		/// Determines whether the current member has the <see cref="ObsoleteAttribute"/> defined.
		/// </summary>
		bool IsObsolete { get; }

		/// <summary>
		/// Determines whether the current member is declared using the <see langword="partial"/> keyword.
		/// </summary>
		bool IsPartial { get; }

		/// <summary>
		/// Determines whether the current member is declared using the <see langword="unsafe"/> keyword.
		/// </summary>
		bool IsUnsafe { get; }

		/// <summary>
		/// Location of the member.
		/// </summary>
		Location? Location { get; }

		/// <summary>
		/// All modifiers of the current symbol.
		/// </summary>
		ImmutableArray<string> Modifiers { get; }

		/// <summary>
		/// Name of the underlaying symbol including the verbatim identifier '@' token.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// All <see cref="ISymbol"/>s overridden by this symbol.
		/// </summary>
		ISymbolContainer<ISymbol, IMemberData> OverriddenSymbols { get; }

		/// <summary>
		/// Parent compilation of this <see cref="IMemberData"/>.
		/// </summary>
		ICompilationData ParentCompilation { get; }

		/// <summary>
		/// <see cref="Microsoft.CodeAnalysis.SemanticModel"/> of the <see cref="Declaration"/>.
		/// </summary>
		SemanticModel SemanticModel { get; }

		/// <summary>
		/// Name of the underlaying symbol including the verbatim identifier '@' token and its type arguments.
		/// </summary>
		string SubstitutedName { get; }

		/// <summary>
		/// <see cref="ISymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		new ISymbol Symbol { get; }

		/// <summary>
		/// <see cref="Analysis.Virtuality"/> of the member.
		/// </summary>
		Virtuality Virtuality { get; }
	}
}
