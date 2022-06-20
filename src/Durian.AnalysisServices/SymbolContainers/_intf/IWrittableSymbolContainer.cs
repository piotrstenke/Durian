// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Text;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers
{
	/// <summary>
	/// <see cref="ISymbolContainer{TSymbol}"/> that writes name of the contained <see cref="ISymbol"/>s into a <see cref="StringBuilder"/>.
	/// </summary>
	public interface IWrittableSymbolContainer : ISymbolContainer
	{
		/// <summary>
		/// Writes the contents of this container to the specified <paramref name="builder"/>.
		/// </summary>
		/// <param name="builder"><see cref="StringBuilder"/> to write the contents of this container to.</param>
		void WriteTo(StringBuilder builder);

		/// <summary>
		/// Returns a <see cref="string"/> representing all the members in the current container.
		/// </summary>
		string ToString();
	}

	/// <inheritdoc cref="IWrittableSymbolContainer"/>
	/// <typeparam name="TSymbol">Type of target <see cref="ISymbol"/>.</typeparam>
	public interface IWrittableSymbolContainer<out TSymbol> : ISymbolContainer<TSymbol>, IWrittableSymbolContainer where TSymbol : class, ISymbol
	{
	}

	/// <inheritdoc cref="IWrittableSymbolContainer"/>
	/// <typeparam name="TSymbol">Type of target <see cref="ISymbol"/>.</typeparam>
	/// <typeparam name="TData">Type of target <see cref="IMemberData"/>.</typeparam>
	public interface IWrittableSymbolContainer<out TSymbol, out TData> : IWrittableSymbolContainer<TSymbol>, ISymbolContainer<TSymbol, TData>
		where TSymbol : class, ISymbol
		where TData : class, IMemberData
	{
	}
}
