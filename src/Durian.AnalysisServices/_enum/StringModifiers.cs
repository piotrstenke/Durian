using System;

namespace Durian.Analysis;

/// <summary>
/// Modifiers of a <see cref="string"/> literal.
/// </summary>
[Flags]
public enum StringModifiers
{
	/// <summary>
	/// No <see cref="string"/> modifiers specified.
	/// </summary>
	None = 0,

	/// <summary>
	/// The <see cref="string"/> is marked with the verbatim '@' modifier.
	/// </summary>
	Verbatim = 1,

	/// <summary>
	/// The <see cref="string"/> is marked with the interpolation '$' modifier.
	/// </summary>
	Interpolation = 2
}
