namespace Durian.Analysis;

/// <summary>
/// Specifies kind of an attribute target specifier.
/// </summary>
public enum AttributeTargetKind
{
	/// <summary>
	/// Kind not applicable.
	/// </summary>
	None = 0,

	/// <summary>
	/// The attribute targets the member itself. Applicable targets:
	/// <list type="bullet">
	/// <item><see langword="field"/> for fields.</item>
	/// <item><see langword="method"/> for methods and lambdas.</item>
	/// <item><see langword="type"/> for types.</item>
	/// <item><see langword="typevar"/> for type parameters.</item>
	/// <item><see langword="property"/> for properties.</item>
	/// <item><see langword="event"/> for events.</item>
	/// <item><see langword="param"/> for parameters.</item>
	/// <item><see langword="assembly"/> for assemblies.</item>
	/// <item><see langword="module"/> for modules.</item>
	/// </list>
	/// </summary>
	This = 1,

	/// <summary>
	/// The attribute targets a the actual value of the member. Applicable targets:
	/// <list type="bullet">
	/// <item><see langword="field"/> for properties and events.</item>
	/// <item><see langword="return"/> for methods, delegates and accessors.</item>
	/// </list>
	/// </summary>
	Value = 2,

	/// <summary>
	/// The attribute targets a generated handler member. Applicable targets:
	/// <list type="bullet">
	/// <item><see langword="method"/> for events.</item>
	/// <item><see langword="param"/> for property setters and event accessors.</item>
	/// </list>
	/// </summary>
	Handler = 3,
}
