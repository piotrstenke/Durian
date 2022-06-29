// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers
{
	/// <summary>
	/// Provides methods for returning symbol representations using either <see cref="ISymbol"/>s or <see cref="IMemberData"/>s.
	/// </summary>
	/// <typeparam name="TSymbol">Type of returned <see cref="ISymbol"/>s.</typeparam>
	/// <typeparam name="TData">Type of returned <see cref="IMemberData"/>s.</typeparam>
	public interface ISymbolContainer<out TSymbol, out TData> : ISymbolContainer
		where TSymbol : class, ISymbol
		where TData : class, IMemberData
	{
		/// <summary>
		/// <see cref="ICompilationData"/> that is used to create <typeparamref name="TData"/>s from <typeparamref name="TSymbol"/>s.
		/// </summary>
		ICompilationData? ParentCompilation { get; }

		/// <summary>
		/// Returns the first member according to the current <see cref="ISymbolContainer.Order"/>.
		/// </summary>
		/// <exception cref="EmptyContainerException">Container does not contain any symbols.</exception>
		ISymbolOrMember<TSymbol, TData> First();

		/// <summary>
		/// Returns the first member according to the specified <paramref name="order"/>.
		/// </summary>
		/// <param name="order"><see cref="ReturnOrder"/> by which to return the first member.</param>
		/// <exception cref="EmptyContainerException">Container does not contain any symbols.</exception>
		ISymbolOrMember<TSymbol, TData> First(ReturnOrder order);

		/// <summary>
		/// Returns the last member according to the current <see cref="ISymbolContainer.Order"/>.
		/// </summary>
		/// <exception cref="EmptyContainerException">Container does not contain any symbols.</exception>
		ISymbolOrMember<TSymbol, TData> Last();

		/// <summary>
		/// Returns the first member according to the specified <paramref name="order"/>.
		/// </summary>
		/// <param name="order"><see cref="ReturnOrder"/> by which to return the last member.</param>
		/// <exception cref="EmptyContainerException">Container does not contain any symbols.</exception>
		ISymbolOrMember<TSymbol, TData> Last(ReturnOrder order);
	}
}
