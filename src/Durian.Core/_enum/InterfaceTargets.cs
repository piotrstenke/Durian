// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian
{
	/// <summary>
	/// Specifies possible targets of an <see langword="interface"/>.
	/// </summary>
	[Flags]
	public enum InterfaceTargets
	{
		/// <summary>
		/// Interface cannot be implemented in code, only through reflection.
		/// </summary>
		ReflectionOnly = 0,

		/// <summary>
		/// Interface cannot be implemented in code. This value is the same as <see cref="ReflectionOnly"/>.
		/// </summary>
		None = ReflectionOnly,

		/// <summary>
		/// Interface can be implemented by classes.
		/// </summary>
		Class = 1,

		/// <summary>
		/// Interface can be implemented by structs.
		/// </summary>
		Struct = 2,

		/// <summary>
		/// Interface can be a base for other interface.
		/// </summary>
		Interface = 4,

		/// <summary>
		/// Interface can be implemented by records.
		/// </summary>
		Record = 8,

		/// <summary>
		/// Interface can be implemented by all valid member kinds.
		/// </summary>
		All = Class | Struct | Interface | Record
	}
}
