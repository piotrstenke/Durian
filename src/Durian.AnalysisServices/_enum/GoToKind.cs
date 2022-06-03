// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis
{
	/// <summary>
	/// Defines all possible kinds of a <see langword="goto"/> statement.
	/// </summary>
	public enum GoToKind
	{
		/// <summary>
		/// Not a <see langword="goto"/> statement.
		/// </summary>
		None = 0,

		/// <summary>
		/// Represents a <c><see langword="goto"/> X</c> statement.
		/// </summary>
		Label = 1,

		/// <summary>
		/// Represents a <c><see langword="goto"/> <see langword="case"/> X</c> statement.
		/// </summary>
		Case = 2,

		/// <summary>
		/// Represents a <c><see langword="goto"/> <see langword="default"/></c> statement.
		/// </summary>
		DefaultCase = 3
	}
}
