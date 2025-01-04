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
		/// Clears the registry.
		/// </summary>
		public void Clear()
		{
			_symbols.Clear();
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> is registered.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to check if is registered.</param>
		public bool IsRegistered(ISymbol symbol)
		{
			return _symbols.ContainsKey(symbol.ToString());
		}

		/// <summary>
		/// Registers the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to register.</param>
		public void Register(ISymbol symbol)
		{
			_symbols.TryAdd(symbol.ToString(), default);
		}
	}
}
