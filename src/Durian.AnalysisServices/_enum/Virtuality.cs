// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis
{
	/// <summary>
	/// Specifies all possible steps of virtuality of a member.
	/// </summary>
	public enum Virtuality
	{
		/// <summary>
		/// Member is not <see langword="virtual"/>.
		/// </summary>
		NotVirtual = 0,

		/// <summary>
		/// The member is <see langword="sealed"/>.
		/// </summary>
		Sealed = 1,

		/// <summary>
		/// The member is <see langword="virtual"/>.
		/// </summary>
		Virtual = 2,

		/// <summary>
		/// The member is <see langword="abstract"/>.
		/// </summary>
		Abstract = 3
	}
}
