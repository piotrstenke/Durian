// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis
{
	/// <summary>
	/// Defines all possible kinds of a <see langword="using"/> directive.
	/// </summary>
	public enum UsingKind
	{
		/// <summary>
		/// Not a <see langword="using"/> directive.
		/// </summary>
		None = 0,

		/// <summary>
		/// Represents a <c><see langword="using"/> X</c> directive.
		/// </summary>
		Ordinary = 1,

		/// <summary>
		/// Represents a <c><see langword="using"/> <see langword="static"/> X</c> directive.
		/// </summary>
		Static = 2,

		/// <summary>
		/// Represents a <c><see langword="using"/> X = Y</c> directive.
		/// </summary>
		Alias = 3
	}
}
