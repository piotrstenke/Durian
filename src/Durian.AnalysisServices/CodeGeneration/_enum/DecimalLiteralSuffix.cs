namespace Durian.Analysis.CodeGeneration;

/// <summary>
/// Specifies the available suffixes of a floating-point numeric literal.
/// </summary>
public enum DecimalLiteralSuffix
{
	/// <summary>
	/// No suffix specified.
	/// </summary>
	None = 0,

	/// <summary>
	/// The float 'f' suffix.
	/// </summary>
	FloatLower = 1,

	/// <summary>
	/// The float 'F' suffix.
	/// </summary>
	FloatUpper = 2,

	/// <summary>
	/// The double 'd' suffix.
	/// </summary>
	DoubleLower = 3,

	/// <summary>
	/// The double 'D' suffix.
	/// </summary>
	DoubleUpper = 4,

	/// <summary>
	/// The decimal 'm' suffix.
	/// </summary>
	DecimalLower = 5,

	/// <summary>
	/// The decimal 'M' suffix.
	/// </summary>
	DecimalUpper = 6,
}
