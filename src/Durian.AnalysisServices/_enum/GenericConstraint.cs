using System;

namespace Durian.Analysis
{
	/// <summary>
	/// Represents all types of generic constraints that can be applied to a generic parameter.
	/// </summary>
	[Flags]
	public enum GenericConstraint
	{
		/// <summary>
		/// No constraint.
		/// </summary>
		None = 0,

		/// <summary>
		/// The parameter is constrained to inherit or implement a specific type.
		/// </summary>
		Type = 1,

		/// <summary>
		/// The parameter is constrained to have a parameterless constructor ('new()' constraint).
		/// </summary>
		New = 2,

		/// <summary>
		/// The parameter is constrained to be a reference type ('class' constraint).
		/// </summary>
		Class = 4,

		/// <summary>
		/// The parameter is constrained to be a value type ('struct' constraint).
		/// </summary>
		Struct = 8,

		/// <summary>
		/// The parameter is constrained to be an unmanaged value type ('unmanaged' constraint).
		/// </summary>
		Unmanaged = 16,

		/// <summary>
		/// The parameter is constrained to be non-nullable ('notnull' constraint).
		/// </summary>
		NotNull = 32,

		/// <summary>
		/// The parameter is constrained using the 'default' constraint.
		/// </summary>
		Default = 64
	}
}
