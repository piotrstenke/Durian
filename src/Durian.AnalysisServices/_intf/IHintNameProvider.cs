﻿using Microsoft.CodeAnalysis;
using System;

namespace Durian.Analysis;

/// <summary>
/// Provides a method that creates a hint name for a specified <see cref="ISymbol"/>.
/// </summary>
public interface IHintNameProvider
{
	/// <summary>
	/// Returns the default hint name.
	/// </summary>
	string GetHintName();

	/// <summary>
	/// Returns a hint name for the specified <paramref name="symbol"/>.
	/// </summary>
	/// <param name="symbol"><see cref="ISymbol"/> to get the hint name for.</param>
	/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
	string GetHintName(ISymbol symbol);

	/// <summary>
	/// Returns a hint name for the specified <paramref name="symbolName"/>.
	/// </summary>
	/// <param name="symbolName">Symbol name.</param>
	/// <exception cref="ArgumentException"><paramref name="symbolName"/> is <see langword="null"/> or empty.</exception>
	string GetHintName(string symbolName);

	/// <summary>
	/// Initializes the provider.
	/// </summary>
	void Initialize();

	/// <summary>
	/// Resets the provider to its original state.
	/// </summary>
	void Reset();

	/// <summary>
	/// A callback method that changes the state of the current provider.
	/// </summary>
	void Success();
}
