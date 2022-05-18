// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis.CodeGeneration
{
	/// <summary>
	/// Defines all possible accessor kinds.
	/// </summary>
	public enum Accessor
	{
		/// <summary>
		/// Member is not an accessor.
		/// </summary>
		None = 0,

		/// <summary>
		/// Represents the <see langword="get"/> accessor.
		/// </summary>
		Get = 1,

		/// <summary>
		/// Represents the <see langword="set"/> accessor.
		/// </summary>
		Set = 2,

		/// <summary>
		/// Represents the <see langword="init"/> accessor.
		/// </summary>
		Init = 3,

		/// <summary>
		/// Represents the <see langword="add"/> accessor.
		/// </summary>
		Add = 4,

		/// <summary>
		/// Represents the <see langword="remove"/> accessor.
		/// </summary>
		Remove = 5
	}
}
