// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
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
		/// Value applied to the <see cref="ConditionalAttribute"/> of this type.
		/// </summary>
		string? CompilerCondition { get; }

		/// <summary>
		/// Target <see cref="BaseTypeDeclarationSyntax"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException">Current symbol cannot be represented using a <see cref="BaseTypeDeclarationSyntax"/>.</exception>
		new BaseTypeDeclarationSyntax Declaration { get; }

		/// <summary>
		/// Equivalent to <see cref="Declaration"/>, but will never throw an exception. Used when a <see cref="INamedTypeSymbol"/> resolves to different node kind than <see cref="BaseTypeDeclarationSyntax"/>.
		/// </summary>
		SyntaxNode SafeDeclaration { get; }

		/// <summary>
		/// Determines whether the type is an attribute.
		/// </summary>
		bool IsAttribute { get; }

		/// <summary>
		/// Determines whether the type is an exception.
		/// </summary>
		bool IsException { get; }

		/// <summary>
		/// Parameterless constructor of this type.
		/// </summary>
		ISymbolOrMember<IMethodSymbol, IMethodData>? ParameterlessConstructor { get; }

		/// <summary>
		/// All partial declarations of the type (including <see cref="IMemberData.Declaration"/>).
		/// </summary>
		ImmutableArray<TypeDeclarationSyntax> PartialDeclarations { get; }

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> associated with the <see cref="IMemberData.Declaration"/>.
		/// </summary>
		new INamedTypeSymbol Symbol { get; }

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
		/// Creates a shallow copy of the current data.
		/// </summary>
		new ITypeData Clone();
	}
}
