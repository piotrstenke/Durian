// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="BasePropertyDeclarationSyntax"/>.
	/// </summary>
	public interface IPropertyData : IMemberData
	{
		/// <summary>
		/// Target <see cref="BasePropertyDeclarationSyntax"/>.
		/// </summary>
		new BasePropertyDeclarationSyntax Declaration { get; }

		/// <summary>
		/// <see cref="ISymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		new IPropertySymbol Symbol { get; }

		/// <summary>
		/// Backing field of the property or <see langword="null"/> if not an auto-property.
		/// </summary>
		ISymbolOrMember<IFieldSymbol, IMemberData>? BackingField { get; }

		/// <summary>
		/// Kind of the auto-property.
		/// </summary>
		AutoPropertyKind AutoPropertyKind { get; }
	}
}
