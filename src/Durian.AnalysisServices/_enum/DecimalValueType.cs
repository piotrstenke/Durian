namespace Durian.Analysis;

/// <summary>
/// Defines all existing floating-point numeric types.
/// </summary>
public enum DecimalValueType
{
	/// <summary>
	/// Type is not a floating-point numeric.
	/// </summary>
	None = 0,

	/// <summary>
	/// Type represents a <see cref="float"/>.
	/// </summary>
	Float = 1,

	/// <summary>
	/// Type represents a <see cref="double"/>.
	/// </summary>
	Double = 2,

	/// <summary>
	/// Type represents a <see cref="decimal"/>.
	/// </summary>
	Decimal = 3
}
