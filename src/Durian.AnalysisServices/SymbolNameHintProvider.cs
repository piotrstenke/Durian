using System;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis;

/// <summary>
/// A <see cref="IHintNameProvider"/> that returns the name of the specified <see cref="ISymbol"/>.
/// </summary>
public sealed class SymbolNameHintProvider : IHintNameProvider
{
	/// <summary>
	/// Initializes a new instance of the <see cref="SymbolNameHintProvider"/> class.
	/// </summary>
	public SymbolNameHintProvider()
	{
	}

	/// <inheritdoc/>
	public string GetHintName(ISymbol symbol)
	{
		if (symbol is null)
		{
			throw new ArgumentNullException(nameof(symbol));
		}

		return symbol.ToString()
			.Replace('<', '{')
			.Replace('>', '}');
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

	string IHintNameProvider.GetHintName()
	{
		throw new NotImplementedException($"{nameof(SymbolNameHintProvider)} does not support default hint names");
	}

	string IHintNameProvider.GetHintName(string symbolName)
	{
		return symbolName;
	}
}
