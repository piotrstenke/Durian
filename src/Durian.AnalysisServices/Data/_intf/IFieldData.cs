﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="IFieldSymbol"/>.
	/// </summary>
	public interface IFieldData : IMemberData, IVariableDeclarator<FieldDeclarationSyntax>, ISymbolOrMember<IFieldSymbol, IFieldData>
	{
		/// <summary>
		/// <see cref="ISymbol"/> associated with the field.s
		/// </summary>
		ISymbolOrMember<ISymbol, IMemberData>? AssociatedSymbol { get; }

		/// <summary>
		/// The kind of the field if its a backing field.
		/// </summary>
		BackingFieldKind BackingFieldKind { get; }

		/// <summary>
		/// Determines whether the field is thread static.
		/// </summary>
		bool IsThreadStatic { get; }

		/// <summary>
		/// Custom offset applied to this field or <c>-1</c> if no custom offset applied.
		/// </summary>
		int CustomOffset { get; }

		/// <summary>
		/// <see cref="IFieldSymbol"/> associated with the <see cref="IMemberData.Declaration"/>.
		/// </summary>
		new IFieldSymbol Symbol { get; }

		/// <summary>
		/// Returns a collection of <see cref="IFieldSymbol"/>s of all variables defined in the <see cref="IVariableDeclarator{T}.Declaration"/>.
		/// </summary>
		IEnumerable<ISymbolOrMember<IFieldSymbol, IFieldData>> GetUnderlayingFields();
	}
}
