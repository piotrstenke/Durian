namespace Durian.Analysis;

/// <summary>
/// Defines all possible auto-property kinds.
/// </summary>
public enum AutoPropertyKind
{
	/// <summary>
	/// Member is not an auto-property.
	/// </summary>
	None = 0,

	/// <summary>
	/// Auto-property with a <see langword="get"/> accessor only.
	/// </summary>
	GetOnly = 1,

	/// <summary>
	/// Auto-property with <see langword="get"/> and <see langword="set"/> accessors.
	/// </summary>
	GetSet = 2,

	/// <summary>
	/// Auto-property with <see langword="get"/> and <see langword="init"/> accessors.
	/// </summary>
	GetInit = 3,

	/// <summary>
	/// Auto-property with a <see langword="set"/> accessor only. Usable only in interfaces.
	/// </summary>
	SetOnly = 4,

	/// <summary>
	/// Auto-property with an <see langword="init"/> accessor only. Usable only in interfaces.
	/// </summary>
	InitOnly = 5,
}
