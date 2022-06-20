// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers
{
	/// <summary>
	/// Provides methods for returning symbol representations using either <see cref="ISymbol"/>s or <see cref="IMemberData"/>s.
	/// </summary>
	/// <typeparam name="TSymbol">Type of returned <see cref="ISymbol"/>s.</typeparam>
	public interface ISymbolContainer<out TSymbol> : ISymbolContainer where TSymbol : class, ISymbol
	{
		/// <summary>
		/// Parent <see cref="ICompilationData"/> of the current container.
		/// </summary>
		ICompilationData? ParentCompilation { get; }

		/// <summary>
		/// Returns the first member according to the current <see cref="ISymbolContainer.Order"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException">Container does not contain any symbols.</exception>
		ISymbolOrMember<TSymbol> First();

		/// <summary>
		/// Returns the first member according to the specified <paramref name="order"/>.
		/// </summary>
		/// <param name="order"><see cref="ReturnOrder"/> by which to return the first member.</param>
		/// <exception cref="InvalidOperationException">Container does not contain any symbols.</exception>
		ISymbolOrMember<TSymbol> First(ReturnOrder order);

		/// <summary>
		/// Returns the last member according to the current <see cref="ISymbolContainer.Order"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException">Container does not contain any symbols.</exception>
		ISymbolOrMember<TSymbol> Last();

		/// <summary>
		/// Returns the first member according to the specified <paramref name="order"/>.
		/// </summary>
		/// <param name="order"><see cref="ReturnOrder"/> by which to return the last member.</param>
		/// <exception cref="InvalidOperationException">Container does not contain any symbols.</exception>
		ISymbolOrMember<TSymbol> Last(ReturnOrder order);
	}

	/// <summary>
	/// Provides methods for returning symbol representations using either <see cref="ISymbol"/>s or <see cref="IMemberData"/>s.
	/// </summary>
	/// <typeparam name="TSymbol">Type of returned <see cref="ISymbol"/>s.</typeparam>
	/// <typeparam name="TData">Type of returned <see cref="IMemberData"/>s.</typeparam>
	public interface ISymbolContainer<out TSymbol, out TData> : ISymbolContainer<TSymbol>
		where TSymbol : class, ISymbol
		where TData : class, IMemberData
	{
		/// <inheritdoc cref="ISymbolContainer{TSymbol}.First()"/>
		new ISymbolOrMember<TSymbol, TData> First();

		/// <inheritdoc cref="ISymbolContainer{TSymbol}.First(ReturnOrder)"/>
		new ISymbolOrMember<TSymbol, TData> First(ReturnOrder order);

		/// <inheritdoc cref="ISymbolContainer{TSymbol}.Last()"/>
		new ISymbolOrMember<TSymbol, TData> Last();

		/// <inheritdoc cref="ISymbolContainer{TSymbol}.Last(ReturnOrder)"/>
		new ISymbolOrMember<TSymbol, TData> Last(ReturnOrder order);
	}
}
