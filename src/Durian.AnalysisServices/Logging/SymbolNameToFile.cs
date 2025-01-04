using System;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Logging
{
	/// <summary>
	/// A <see cref="IHintNameProvider"/> that returns the name of the specified <see cref="ISymbol"/>.
	/// </summary>
	public sealed class SymbolNameToFile : IHintNameProvider
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SymbolNameToFile"/> class.
		/// </summary>
		public SymbolNameToFile()
		{
		}

		/// <inheritdoc/>
		public string GetHintName(ISymbol symbol)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			string name = symbol.ToString()
				.Replace('<', '{')
				.Replace('>', '}');

			return name;
		}

		/// <inheritdoc/>
		public void Initialize()
		{
			// Do nothing.
		}

		/// <inheritdoc/>
		public void Reset()
		{
			// Do nothing.
		}

		/// <inheritdoc/>
		public void Success()
		{
			// Do nothing.
		}
	}
}
