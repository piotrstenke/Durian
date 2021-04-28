using System;
using Microsoft.CodeAnalysis;
using System.Diagnostics;

namespace Durian.Logging
{
	/// <summary>
	/// A <see cref="IFileNameProvider"/> that returns the name of the specified <see cref="ISymbol"/>.
	/// </summary>
	public sealed class SymbolNameToFile : IFileNameProvider
	{
		/// <summary>
		/// A singleton instance of the <see cref="SymbolNameToFile"/> class.
		/// </summary>
		public static SymbolNameToFile Instance { get; } = new();

		private SymbolNameToFile()
		{
		}

		/// <inheritdoc/>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public string GetFileName(ISymbol symbol)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			return symbol.Name;
		}

		void IFileNameProvider.Success()
		{
			// Do nothing.
		}

		void IFileNameProvider.Reset()
		{
			// Do nothing.
		}
	}
}
