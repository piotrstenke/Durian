// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis
{
	/// <summary>
	/// Defines all types that have a built-in alias.
	/// </summary>
	public enum KeywordType
	{
		/// <summary>
		/// Type is not primitive.
		/// </summary>
		None = 0,

		/// <summary>
		/// Type represents a <see cref="short"/>.
		/// </summary>
		Short = 1,

		/// <summary>
		/// Type represents an <see cref="int"/>.
		/// </summary>
		Int = 2,

		/// <summary>
		/// Type represents a <see cref="long"/>.
		/// </summary>
		Long = 3,

		/// <summary>
		/// Type represents an <see cref="ushort"/>.
		/// </summary>
		UShort = 4,

		/// <summary>
		/// Type represents an <see cref="uint"/>.
		/// </summary>
		UInt = 5,

		/// <summary>
		/// Type represents an <see cref="ulong"/>.
		/// </summary>
		ULong = 6,

		/// <summary>
		/// Type represents a <see cref="byte"/>.
		/// </summary>
		Byte = 7,

		/// <summary>
		/// Type represents a <see cref="sbyte"/>.
		/// </summary>
		SByte = 8,

		/// <summary>
		/// Type represents a <see langword="nint"/>.
		/// </summary>
		NInt = 9,

		/// <summary>
		/// Type represents a <see langword="nuint"/>.
		/// </summary>
		NUInt = 10,

		/// <summary>
		/// Type represents a <see cref="float"/>.
		/// </summary>
		Float = 11,

		/// <summary>
		/// Type represents a <see cref="double"/>.
		/// </summary>
		Double = 12,

		/// <summary>
		/// Type represents a <see cref="decimal"/>.
		/// </summary>
		Decimal = 13,

		/// <summary>
		/// Type represents a <see cref="bool"/>.
		/// </summary>
		Bool = 14,

		/// <summary>
		/// Type represents a <see cref="char"/>.
		/// </summary>
		Char = 15,

		/// <summary>
		/// Type represents a <see cref="string"/>.
		/// </summary>
		String = 16,

		/// <summary>
		/// Type represents a <see cref="void"/>.
		/// </summary>
		Void = 17,

		/// <summary>
		/// Type represents a <see cref="string"/>.
		/// </summary>
		Object = 18,

		/// <summary>
		/// Type represents a <see langword="dynamic"/>.
		/// </summary>
		Dynamic = 19
	}
}
