// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian.Analysis.CodeGeneration
{
	/// <summary>
	/// Determines how enum declarations are written.
	/// </summary>
	[Flags]
	public enum EnumStyle
	{
		/// <summary>
		/// No special format applied.
		/// </summary>
		None = 0,

		/// <summary>
		/// Writes explicit values of the enum's fields.
		/// </summary>
		ExplicitValues = 1,

		/// <summary>
		/// Writes the default base type (<see cref="int"/>) of the enum.
		/// </summary>
		ExplicitInt32 = 2
	}
}
