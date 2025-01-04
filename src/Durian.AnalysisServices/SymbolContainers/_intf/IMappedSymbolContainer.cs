using System;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers;

/// <summary>
/// <see cref="ILeveledSymbolContainer"/> that maps <see cref="int"/> values to appropriate <typeparamref name="TLevel"/> levels.
/// </summary>
/// <typeparam name="TLevel">Type of mapped value.</typeparam>
public interface IMappedSymbolContainer<TLevel> : ILeveledSymbolContainer
{
	/// <summary>
	/// Current nesting level of <see cref="ISymbolContainer"/>s.
	/// </summary>
	new TLevel CurrentLevel { get; }

	/// <summary>
	/// Resolves a <see cref="ISymbolContainer"/> at the specified nesting level.
	/// </summary>
	/// <param name="level">Nesting level of the container.</param>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="level"/> must be greater than <c>0</c> and less than <see cref="ILeveledSymbolContainer.NumLevels"/>.</exception>
	ISymbolContainer ResolveLevel(TLevel level);

	/// <summary>
	/// Removes the cached data of the specified <paramref name="level"/> and all levels after it.
	/// </summary>
	/// <param name="level">Level to clear the cached data of.</param>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="level"/> must be greater than <c>0</c> and less than <see cref="ILeveledSymbolContainer.NumLevels"/>.</exception>
	void ClearLevel(TLevel level);
}

/// <summary>
/// <see cref="ILeveledSymbolContainer{TSymbol, TData}"/> that maps <see cref="int"/> values to appropriate <typeparamref name="TLevel"/> levels.
/// </summary>
/// <typeparam name="TSymbol">Type of returned <see cref="ISymbol"/>s.</typeparam>
/// <typeparam name="TData">Type of returned <see cref="IMemberData"/>s.</typeparam>
/// <typeparam name="TLevel">Type of mapped value.</typeparam>
public interface IMappedSymbolContainer<TSymbol, TData, TLevel> : IMappedSymbolContainer<TLevel>, ILeveledSymbolContainer<TSymbol, TData>
	where TSymbol : class, ISymbol
	where TData : class, IMemberData
{
	/// <summary>
	/// Resolves a <see cref="ISymbolContainer{TSymbol, TData}"/> at the specified nesting level.
	/// </summary>
	/// <param name="level">Nesting level of the container.</param>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="level"/> must be greater than <c>0</c> and less than <see cref="ILeveledSymbolContainer.NumLevels"/>.</exception>
	new ISymbolContainer<TSymbol, TData> ResolveLevel(TLevel level);
}
