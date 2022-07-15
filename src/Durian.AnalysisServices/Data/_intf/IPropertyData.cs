// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="BasePropertyDeclarationSyntax"/>.
	/// </summary>
	public interface IPropertyData : IMemberData, ISymbolOrMember<IPropertySymbol, IPropertyData>
	{
		/// <summary>
		/// Kind of the auto-property.
		/// </summary>
		AutoPropertyKind AutoPropertyKind { get; }

		/// <summary>
		/// Backing field of the property or <see langword="null"/> if not an auto-property.
		/// </summary>
		ISymbolOrMember<IFieldSymbol, IFieldData>? BackingField { get; }

		/// <summary>
		/// Target <see cref="BasePropertyDeclarationSyntax"/>.
		/// </summary>
		new BasePropertyDeclarationSyntax Declaration { get; }

		/// <inheritdoc cref="IMemberData.OverriddenSymbols"/>
		new ISymbolContainer<IPropertySymbol, IPropertyData> OverriddenSymbols { get; }

		/// <summary>
		/// <see cref="ISymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		new IPropertySymbol Symbol { get; }
	}
}
