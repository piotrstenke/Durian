using System;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis;

/// <summary>
/// <see cref="IHintNameProvider"/> implementation that always returns the same hint name.
/// </summary>
public sealed class StaticHintNameProvider : IHintNameProvider
{
	private readonly string _hintName;

	/// <summary>
	/// Initializes a new instance of the <see cref="StaticHintNameProvider"/> class.
	/// </summary>
	/// <param name="hintName">Hint name to always return.</param>
	public StaticHintNameProvider(string hintName)
	{
		if (string.IsNullOrWhiteSpace(hintName))
		{
			throw new ArgumentException("Value cannot be null or empty", nameof(hintName));
		}

		_hintName = hintName;
	}

	/// <summary>
	/// Returns the current hint name.
	/// </summary>
	public string GetHintName()
	{
		return _hintName;
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

	string IHintNameProvider.GetHintName(ISymbol symbol)
	{
		return GetHintName();
	}

	string IHintNameProvider.GetHintName(string symbolName)
	{
		return GetHintName();
	}
}
