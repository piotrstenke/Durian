using System;

namespace Durian.Analysis.CodeGeneration;

/// <summary>
/// Determines how interface members are written.
/// </summary>
[Flags]
public enum InterfaceMemberStyle
{
	/// <summary>
	/// No format applied.
	/// </summary>
	None = 0,

	/// <summary>
	/// Use explicit <see langword="virtual"/> and <see langword="abstract"/> modifiers.
	/// </summary>
	ExplicitVirtual = 1,

	/// <summary>
	/// Use explicit <see langword="public"/> modifier.
	/// </summary>
	ExplicitAccess = 2
}