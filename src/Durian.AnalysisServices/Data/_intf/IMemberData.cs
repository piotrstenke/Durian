// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Collections.Immutable;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="MemberDeclarationSyntax"/>.
	/// </summary>
	public interface IMemberData
	{
		/// <summary>
		/// Target <see cref="CSharpSyntaxNode"/>.
		/// </summary>
		CSharpSyntaxNode Declaration { get; }

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
		/// Location of the member.
		/// </summary>
		Location Location { get; }

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
		ISymbol Symbol { get; }

		/// <summary>
		/// Returns data of all attributes applied to the <see cref="Symbol"/>.
		/// </summary>
		ImmutableArray<AttributeData> GetAttributes();

		/// <summary>
		/// Returns all <see cref="INamespaceSymbol"/>s that contain the <see cref="Symbol"/>.
		/// </summary>
		NamespaceContainer GetContainingNamespaces();

		/// <summary>
		/// Returns all modifiers of the current symbol.
		/// </summary>
		string[] GetModifiers();

		/// <summary>
		/// Returns all <see cref="ITypeData"/>s that contain the <see cref="Symbol"/>.
		/// </summary>
		TypeContainer GetContainingTypes();
	}
}
