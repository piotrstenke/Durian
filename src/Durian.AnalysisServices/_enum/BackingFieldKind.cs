namespace Durian.Analysis;

/// <summary>
/// Defines all possible backing field kinds.
/// </summary>
public enum BackingFieldKind
{
	/// <summary>
	/// The field is not a backing field.
	/// </summary>
	None = 0,

	/// <summary>
	/// The field is a backing field of a property.
	/// </summary>
	Property = 1,

	/// <summary>
	/// The field is a backing field of an event.
	/// </summary>
	Event = 2
}
