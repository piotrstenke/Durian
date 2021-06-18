// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Logging
{
	/// <summary>
	/// A <see cref="IHintNameProvider"/> that returns the name of the specified <see cref="ISymbol"/>.
	/// </summary>
	public sealed class SymbolNameToFile : IHintNameProvider
	{
		private readonly StringBuilder _builder;
		private bool _isCleared;
		private ISymbol? _previousSymbol;

		/// <inheritdoc cref="SymbolNameToFile(StringBuilder)"/>
		public SymbolNameToFile()
		{
			_builder = new();
			_isCleared = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SymbolNameToFile"/> class.
		/// </summary>
		/// <param name="stringBuilder"><see cref="StringBuilder"/> that is used to create the file name.</param>
		public SymbolNameToFile(StringBuilder? stringBuilder)
		{
			if (stringBuilder is null)
			{
				_isCleared = true;
				_builder = new();
			}
			else
			{
				_builder = stringBuilder;
			}
		}

		/// <inheritdoc/>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public string GetFileName(ISymbol symbol)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			if (!_isCleared)
			{
				if (SymbolEqualityComparer.Default.Equals(symbol, _previousSymbol))
				{
					return _builder.ToString();
				}

				_builder.Clear();
			}

			string name = symbol.ToString()
				.Replace('<', '{')
				.Replace('>', '}');

			_previousSymbol = symbol;
			return name;
		}

		/// <inheritdoc/>
		public void Reset()
		{
			_builder.Clear();
			_isCleared = true;
		}

		/// <inheritdoc/>
		public void Success()
		{
			_builder.Clear();
			_isCleared = true;
		}
	}
}
