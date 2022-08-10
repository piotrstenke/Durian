// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis
{
	/// <summary>
	/// Defines all possible event accessor kinds.
	/// </summary>
	public enum EventAccessorKind
	{
		/// <summary>
		/// Member is not an accessor.
		/// </summary>
		None = 0,

		/// <summary>
		/// Represents the <see langword="add"/> accessor.
		/// </summary>
		Add = 1,

		/// <summary>
		/// Represents the <see langword="remove"/> accessor.
		/// </summary>
		Remove = 2
	}
}
