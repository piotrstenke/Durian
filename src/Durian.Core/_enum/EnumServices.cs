// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian
{
	/// <summary>
	/// Specifies extension methods that can be generated for an <see langword="enum"/>.
	/// </summary>
	[Flags]
	public enum EnumServices
	{
		/// <summary>
		/// None.
		/// </summary>
		None = 0,

		/// <summary>
		/// Reflection-less implementation of ToString().
		/// </summary>
		ToString = 1,

		/// <summary>
		/// Boxing-less implementation of Equals(other).
		/// </summary>
		Equals = 1 << 1,

		/// <summary>
		/// Boxing-less implementation of GetHashCode().
		/// </summary>
		GetHashCode = 1 << 2,

		/// <summary>
		/// Boxing-less implementation of HasFlag(flag). Only enums with the <see cref="FlagsAttribute"/> are supported.
		/// </summary>
		HasFlag = 1 << 3,

		/// <summary>
		/// Boxing-less implementation of CompareTo(other).
		/// </summary>
		CompareTo = 1 << 4,

		/// <summary>
		/// Method that determines whether the current value is defined in the <see langword="enum"/>.
		/// </summary>
		IsDefined = 1 << 5,

		/// <summary>
		/// Method that returns all flags that the current value contains.
		/// Only enums with the <see cref="FlagsAttribute"/> are supported.
		/// </summary>
		GetFlags = 1 << 6,

		/// <summary>
		/// Method that returns all flags that the current value contains as <see cref="string"/>s.
		/// Only enums with the <see cref="FlagsAttribute"/> are supported.
		/// </summary>
		GetStringFlags = 1 << 7,

		/// <summary>
		/// All available methods are to be generated.
		/// </summary>
		All = ToString | Equals | GetHashCode | HasFlag | CompareTo | IsDefined | GetFlags | GetStringFlags,
	}
}
