// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Logging
{
	/// <summary>
	/// Provides a method that creates a hint name for a specified <see cref="ISymbol"/>.
	/// </summary>
	public interface IHintNameProvider
	{
		/// <summary>
		/// Returns a name of the log file for the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the file name for.</param>
		string GetFileName(ISymbol symbol);

		/// <summary>
		/// Resets the <see cref="IHintNameProvider"/> to its original state.
		/// </summary>
		void Reset();

		/// <summary>
		/// A callback method that changes the state of the current <see cref="IHintNameProvider"/>.
		/// </summary>
		void Success();
	}
}