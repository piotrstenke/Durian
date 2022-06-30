// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="IEventSymbol"/>.
	/// </summary>
	public interface IEventData : IMemberData, IDeclarator<EventFieldDeclarationSyntax>, ISymbolOrMember<IEventSymbol, IEventData>
	{
		/// <summary>
		/// Target <see cref="MemberDeclarationSyntax"/>.
		/// </summary>
		new MemberDeclarationSyntax Declaration { get; }

		/// <summary>
		/// <see cref="IEventSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		new IEventSymbol Symbol { get; }
	}
}
