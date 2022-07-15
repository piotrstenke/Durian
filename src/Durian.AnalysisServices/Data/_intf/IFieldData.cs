// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="IFieldSymbol"/>.
	/// </summary>
	public interface IFieldData : IMemberData, IDeclarator<FieldDeclarationSyntax>, ISymbolOrMember<IFieldSymbol, IFieldData>
	{
		/// <summary>
		/// The kind of the field if its a backing field.
		/// </summary>
		BackingFieldKind BackingFieldKind { get; }

		/// <summary>
		/// <see cref="IFieldSymbol"/> associated with the <see cref="IMemberData.Declaration"/>.
		/// </summary>
		new IFieldSymbol Symbol { get; }

		/// <summary>
		/// Returns a collection of <see cref="IFieldData"/>s of all variables defined in the <see cref="IDeclarator{T}.Declaration"/>.
		/// </summary>
		IEnumerable<IFieldData> GetUnderlayingFields();
	}
}
