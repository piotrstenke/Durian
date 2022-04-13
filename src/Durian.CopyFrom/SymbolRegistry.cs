// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.CopyFrom
{
	/// <summary>
	/// Registers <see cref="ISymbol"/> that were already generated.
	/// </summary>
	public sealed class SymbolRegistry
	{
		private readonly ConcurrentDictionary<string, byte> _symbols;

		/// <summary>
		/// Initializes a new instance of the <see cref="SymbolRegistry"/> class.
		/// </summary>
		public SymbolRegistry()
		{
			_symbols = new();
		}

		/// <summary>
		/// Registers the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to register.</param>
		public void Register(ISymbol symbol)
		{
			_symbols.TryAdd(symbol.ToString(), default);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> is registered.
		/// </summary>
		/// <param name="symbol"></param>
		public bool IsRegistered(ISymbol symbol)
		{
			return _symbols.ContainsKey(symbol.ToString());
		}

		/// <summary>
		/// Clears the registry.
		/// </summary>
		public void Clear()
		{
			_symbols.Clear();
		}
	}
}
