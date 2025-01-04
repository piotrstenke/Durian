using System;

namespace Durian.Analysis.CodeGeneration;

/// <summary>
/// Determines how record declarations are written.
/// </summary>
[Flags]
public enum RecordStyle
{
	/// <summary>
	/// No special format applied.
	/// </summary>
	None = 0,

	/// <summary>
	/// Writes the <see langword="class"/> keyword when the record is a reference type.
	/// </summary>
	ExplicitClass = 1,

	/// <summary>
	/// Writes a primary constructor.
	/// </summary>
	PrimaryConstructor = 2
}
