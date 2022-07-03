// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers
{
	/// <summary>
	/// <see cref="ISymbolContainer{TSymbol, TData}"/> that allows to share a single symbol collection between multiple sub-set containers.
	/// </summary>
	/// <typeparam name="TSymbol">Type of returned <see cref="ISymbol"/>s.</typeparam>
	/// <typeparam name="TData">Type of returned <see cref="IMemberData"/>s.</typeparam>
	public interface ILeveledSymbolContainer<TSymbol, TData> : ISymbolContainer<TSymbol, TData>, IReturnOrderEnumerable<ISymbolOrMember<TSymbol, TData>>, ISealable
		where TSymbol : class, ISymbol
		where TData : class, IMemberData
	{
		/// <summary>
		/// Current nesting level of <see cref="ISymbolContainer"/>s or <c>-1</c> if no internal container is initialized.
		/// </summary>
		int CurrentLevel { get; }

		/// <summary>
		/// Number of possible nesting levels of <see cref="ISymbolContainer"/>s.
		/// </summary>
		int NumLevels { get; }

		/// <summary>
		/// Registers a new nesting level.
		/// </summary>
		/// <param name="function">Function used to retrieve the ?<typeparamref name="TSymbol"/>s.</param>
		/// <exception cref="ArgumentNullException"><paramref name="function"/> is <see langword="null"/>.</exception>
		/// <exception cref="SealedObjectException">Cannot register new level to a sealed container.</exception>
		void RegisterLevel(Func<TSymbol, IEnumerable<TSymbol>> function);

		/// <summary>
		/// Registers a new nesting level.
		/// </summary>
		/// <param name="function">Function used to retrieve the ?<typeparamref name="TSymbol"/>s.</param>
		/// <exception cref="ArgumentNullException"><paramref name="function"/> is <see langword="null"/>.</exception>
		/// <exception cref="SealedObjectException">Cannot register new level to a sealed container.</exception>
		void RegisterLevel(Func<TSymbol, IEnumerable<ISymbolOrMember<TSymbol, TData>>> function);

		/// <summary>
		/// Registers a new nesting level.
		/// </summary>
		/// <param name="function">Function used to retrieve the ?<typeparamref name="TSymbol"/>s.</param>
		/// <exception cref="ArgumentNullException"><paramref name="function"/> is <see langword="null"/>.</exception>
		/// <exception cref="SealedObjectException">Cannot register new level to a sealed container.</exception>
		void RegisterLevel(Func<ISymbolOrMember<TSymbol, TData>, IEnumerable<ISymbolOrMember<TSymbol, TData>>> function);

		/// <summary>
		/// Resolves a <see cref="ISymbolContainer{TSymbol, TData}"/> at the specified nesting level.
		/// </summary>
		/// <param name="level">Nesting level of the container.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="level"/> must be greater than <c>0</c> and less than <see cref="NumLevels"/>.</exception>
		ISymbolContainer<TSymbol, TData> ResolveLevel(int level);

		/// <summary>
		/// Removes the cached data of the specified <paramref name="level"/> and all levels after it.
		/// </summary>
		/// <param name="level">Level to clear the cached data of.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="level"/> must be greater than <c>0</c> and less than <see cref="NumLevels"/>.</exception>
		void ClearLevel(int level);
	}
}
