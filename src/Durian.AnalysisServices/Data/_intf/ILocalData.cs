// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="ILocalSymbol"/>.
	/// </summary>
	public interface ILocalData : IMemberData, IDeclarator<LocalDeclarationStatementSyntax>, ISymbolOrMember<ILocalSymbol, ILocalData>
	{
		/// <summary>
		/// <see cref="ILocalSymbol"/> associated with the <see cref="IMemberData.Declaration"/>.
		/// </summary>
		new ILocalSymbol Symbol { get; }
	}
}
