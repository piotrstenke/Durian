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
		/// Base types of the current type.
		/// </summary>
		ISymbolContainer<INamedTypeSymbol, ITypeData> BaseTypes { get; }

		/// <summary>
		/// Parameterless constructor of this type.
		/// </summary>
		ISymbolOrMember<IMethodSymbol, IMethodData>? ParameterlessConstructor { get; }

		/// <summary>
		/// All partial declarations of the type (including <see cref="IMemberData.Declaration"/>).
		/// </summary>
		ImmutableArray<TypeDeclarationSyntax> PartialDeclarations { get; }

		/// <summary>
		/// <see cref="ITypeSymbol"/> associated with the <see cref="IMemberData.Declaration"/>.
		/// </summary>
		new ITypeSymbol Symbol { get; }

		/// <summary>
		/// Returns all <see cref="IEventSymbol"/>s contained within this type.
		/// </summary>
		/// <param name="members">Range of members to include.</param>
		ISymbolContainer<IEventSymbol, IEventData> GetEvents(IncludedMembers members);

		/// <summary>
		/// Returns all <see cref="IFieldSymbol"/>s contained within this type.
		/// </summary>
		/// <param name="members">Range of members to include.</param>
		ISymbolContainer<IFieldSymbol, IFieldData> GetFields(IncludedMembers members);

		/// <summary>
		/// Returns all <see cref="INamedTypeSymbol"/>s contained within this type.
		/// </summary>
		/// <param name="members">Range of members to include.</param>
		ISymbolContainer<INamedTypeSymbol, ITypeData> GetInnerTypes(IncludedMembers members);

		/// <summary>
		/// Returns all <see cref="ISymbol"/>s contained within this type.
		/// </summary>
		/// <param name="members">Range of members to include.</param>
		ISymbolContainer<ISymbol, IMemberData> GetMembers(IncludedMembers members);

		/// <summary>
		/// Returns all <see cref="IMethodSymbol"/>s contained within this type.
		/// </summary>
		/// <param name="members">Range of members to include.</param>
		ISymbolContainer<IMethodSymbol, IMethodData> GetMethods(IncludedMembers members);

		/// <summary>
		/// Returns all <see cref="IPropertySymbol"/>s contained within this type.
		/// </summary>
		/// <param name="members">Range of members to include.</param>
		ISymbolContainer<IPropertySymbol, IPropertyData> GetProperties(IncludedMembers members);

		/// <summary>
		/// Converts the current <see cref="ITypeData"/> to a <see cref="INamespaceOrTypeData"/>.
		/// </summary>
		INamespaceOrTypeData ToNamespaceOrType();
	}
}
