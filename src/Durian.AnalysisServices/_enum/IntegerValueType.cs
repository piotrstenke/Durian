// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis
{
	/// <summary>
	/// Defines all existing integral numeric types, excluding native-sized integers (<see langword="nint"/> and <see langword="nuint"/>).
	/// </summary>
	public enum IntegerValueType
	{
		/// <summary>
		/// Type is not an integer.
		/// </summary>
		None = 0,

		/// <summary>
		/// Type represents a <see cref="short"/>.
		/// </summary>
		Int16 = 1,

		/// <summary>
		/// Type represents an <see cref="int"/>.
		/// </summary>
		Int32 = 2,

		/// <summary>
		/// Type represents a <see cref="long"/>.
		/// </summary>
		Int64 = 3,

		/// <summary>
		/// Type represents an <see cref="ushort"/>.
		/// </summary>
		UInt16 = 4,

		/// <summary>
		/// Type represents an <see cref="uint"/>.
		/// </summary>
		UInt32 = 5,

		/// <summary>
		/// Type represents an <see cref="ulong"/>.
		/// </summary>
		UInt64 = 6,

		/// <summary>
		/// Type represents a <see cref="byte"/>.
		/// </summary>
		Byte = 7,

		/// <summary>
		/// Type represents a <see cref="sbyte"/>.
		/// </summary>
		SByte = 8
	}
}
