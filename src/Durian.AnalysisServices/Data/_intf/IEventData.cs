// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.SymbolContainers;
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
		/// Returns the <see cref="Declaration"/> as a <see cref="EventFieldDeclarationSyntax"/>.
		/// </summary>
		EventFieldDeclarationSyntax? AsField { get; }

		/// <summary>
		/// Returns the <see cref="Declaration"/> as a <see cref="EventDeclarationSyntax"/>.
		/// </summary>
		EventDeclarationSyntax? AsProperty { get; }

		/// <summary>
		/// Backing field of the event or <see langword="null"/> if not a field event.
		/// </summary>
		ISymbolOrMember<IFieldSymbol, IFieldData>? BackingField { get; }

		/// <summary>
		/// Target <see cref="MemberDeclarationSyntax"/>.
		/// </summary>
		new MemberDeclarationSyntax Declaration { get; }

		/// <inheritdoc cref="IMemberData.OverriddenSymbols"/>
		new ISymbolContainer<IEventSymbol, IEventData> OverriddenSymbols { get; }

		/// <summary>
		/// <see cref="IEventSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		new IEventSymbol Symbol { get; }
	}
}
