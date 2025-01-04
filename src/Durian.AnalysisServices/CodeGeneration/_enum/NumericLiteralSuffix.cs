namespace Durian.Analysis.CodeGeneration
{
	/// <summary>
	/// Specifies the available suffixes of a numeric literal.
	/// </summary>
	public enum NumericLiteralSuffix
	{
		/// <summary>
		/// No suffix specified.
		/// </summary>
		None = 0,

		/// <summary>
		/// The unsigned 'u' suffix.
		/// </summary>
		UnsignedLower = 1,

		/// <summary>
		/// The unsigned long 'ul' suffix.
		/// </summary>
		UnsignedLowerLongLower = 2,

		/// <summary>
		/// The unsigned long 'uL' suffix.
		/// </summary>
		UnsignedLowerLongUpper = 3,

		/// <summary>
		/// The unsigned 'U' suffix.
		/// </summary>
		UnsignedUpper = 4,

		/// <summary>
		/// The unsigned long 'Ul' suffix.
		/// </summary>
		UnsignedUpperLongLower = 5,

		/// <summary>
		/// The unsigned long 'UL' suffix.
		/// </summary>
		UnsignedUpperLongUpper = 6,

		/// <summary>
		/// The long 'l' suffix.
		/// </summary>
		LongLower = 7,

		/// <summary>
		/// The long unsigned 'lu' suffix.
		/// </summary>
		LongLowerUnsignedLower = 8,

		/// <summary>
		/// The long unsigned 'lU' suffix.
		/// </summary>
		LongLowerUnsignedUpper = 9,

		/// <summary>
		/// The long 'L' suffix.
		/// </summary>
		LongUpper = 10,

		/// <summary>
		/// The long unsigned 'Lu' suffix.
		/// </summary>
		LongUpperUnsignedLower = 11,

		/// <summary>
		/// The long unsigned 'LU' suffix.
		/// </summary>
		LongUpperUnsignedUpper = 12,

		/// <summary>
		/// The float 'f' suffix.
		/// </summary>
		FloatLower = 13,

		/// <summary>
		/// The float 'F' suffix.
		/// </summary>
		FloatUpper = 14,

		/// <summary>
		/// The double 'd' suffix.
		/// </summary>
		DoubleLower = 15,

		/// <summary>
		/// The double 'D' suffix.
		/// </summary>
		DoubleUpper = 16,

		/// <summary>
		/// The decimal 'm' suffix.
		/// </summary>
		DecimalLower = 17,

		/// <summary>
		/// The decimal 'M' suffix.
		/// </summary>
		DecimalUpper = 18,
	}
}
