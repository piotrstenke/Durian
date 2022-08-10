// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis.CodeGeneration
{
	/// <summary>
	/// Specifies the available prefixes of a numeric literal.
	/// </summary>
	public enum NumericLiteralPrefix
	{
		/// <summary>
		/// No prefix specified. The value is in the decimal (base-10) format.
		/// </summary>
		None = 0,

		/// <summary>
		/// The hexadecimal '0x' prefix. The value is in the hexadecimal (base-16) format.
		/// </summary>
		HexadecimalLower = 1,

		/// <summary>
		/// The hexadecimal '0X' prefix. The value is in the hexadecimal (base-16) format.
		/// </summary>
		HexadecimalUpper = 2,

		/// <summary>
		/// The binary '0b' prefix. The value is in the binary (base-16) format.
		/// </summary>
		BinaryLower = 3,

		/// <summary>
		/// The binary '0B' prefix. The value is in the binary (base-16) format.
		/// </summary>
		BinaryUpper = 4,
	}
}
