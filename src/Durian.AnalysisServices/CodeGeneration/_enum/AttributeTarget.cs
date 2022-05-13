// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis.CodeGeneration
{
	/// <summary>
	/// Specifies all available targets for an attribute.
	/// </summary>
	public enum AttributeTarget
	{
		/// <summary>
		/// No attribute target specified.
		/// </summary>
		None = 0,

		/// <summary>
		/// The attribute targets a method.
		/// </summary>
		Method = 1,

		/// <summary>
		/// The attribute targets a method return type.
		/// </summary>
		Return = 2,

		/// <summary>
		/// The attribute targets a property.
		/// </summary>
		Property = 3,

		/// <summary>
		/// The attribute targets a field.
		/// </summary>
		Field = 4,

		/// <summary>
		/// The attribute targets an event.
		/// </summary>
		Event = 5,

		/// <summary>
		/// The attribute targets a type.
		/// </summary>
		Type = 6,

		/// <summary>
		/// The attribute targets a type parameter.
		/// </summary>
		TypeVar = 7,

		/// <summary>
		/// The attribute targets a parameter.
		/// </summary>
		Param = 8,

		/// <summary>
		/// The attribute targets a module.
		/// </summary>
		Module = 9,

		/// <summary>
		/// The attribute targets an assembly.
		/// </summary>
		Assembly = 10
	}
}
