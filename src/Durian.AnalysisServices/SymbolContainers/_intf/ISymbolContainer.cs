// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections;
using System.Collections.Immutable;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers
{
	/// <summary>
	/// Provides methods for returning symbol representations using either <see cref="ISymbol"/>s or <see cref="IMemberData"/>s.
	/// </summary>
	public interface ISymbolContainer : IEnumerable
	{
		/// <summary>
		/// Number of elements in the container.
		/// </summary>
		int Count { get; }

		/// <summary>
		/// Order of elements in the container.
		/// </summary>
		ReturnOrder Order { get; }

		/// <summary>
		/// Returns the <see cref="IMemberData"/>s contained within this instance.
		/// </summary>
		ImmutableArray<IMemberData> GetData();

		/// <summary>
		/// Returns the names of symbols contained within this instance.
		/// </summary>
		ImmutableArray<string> GetNames();

		/// <summary>
		/// Returns the <see cref="ISymbol"/>s contained within this instance.
		/// </summary>
		ImmutableArray<ISymbol> GetSymbols();

		/// <summary>
		/// Reverses the contents of the container.
		/// </summary>
		void Reverse();
	}
}
