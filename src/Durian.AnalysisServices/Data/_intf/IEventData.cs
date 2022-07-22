// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="IEventSymbol"/>.
	/// </summary>
	public interface IEventData : IMemberData, IVariableDeclarator<EventFieldDeclarationSyntax>, ISymbolOrMember<IEventSymbol, IEventData>
	{
		/// <summary>
		/// The <see langword="add"/> accessor of the event.
		/// </summary>
		ISymbolOrMember<IMethodSymbol, IMethodData>? AddMethod { get; }

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

		/// <summary>
		/// Interface methods this <see cref="IEventSymbol"/> explicily implements.
		/// </summary>
		ISymbolOrMember<IEventSymbol, IEventData>? ExplicitInterfaceImplementation { get; }

		/// <summary>
		/// Interface methods this <see cref="IEventSymbol"/> implicitly implements
		/// </summary>
		ISymbolContainer<IEventSymbol, IEventData> ImplicitInterfaceImplementations { get; }

		/// <summary>
		/// Determines whether this event is a default interface implementation.
		/// </summary>
		bool IsDefaultImplementation { get; }

		/// <summary>
		/// Event overridden by this event.
		/// </summary>
		ISymbolOrMember<IEventSymbol, IEventData>? OverriddenEvent { get; }

		/// <summary>
		/// All events overridden by this event.
		/// </summary>
		ISymbolContainer<IEventSymbol, IEventData> OverriddenEvents { get; }

		/// <summary>
		/// The <see langword="remove"/> accessor of the event.
		/// </summary>
		ISymbolOrMember<IMethodSymbol, IMethodData>? RemoveMethod { get; }

		/// <summary>
		/// <see cref="IEventSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		new IEventSymbol Symbol { get; }

		/// <summary>
		/// Returns a collection of <see cref="IEventData"/>s of all variables defined in the <see cref="IVariableDeclarator{T}.Declaration"/>.
		/// </summary>
		IEnumerable<ISymbolOrMember<IEventSymbol, IEventData>> GetUnderlayingEvents();
	}
}
