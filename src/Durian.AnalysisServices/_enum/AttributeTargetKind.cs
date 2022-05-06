// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis
{
	/// <summary>
	/// Specifies kind of an attribute target specifier.
	/// </summary>
	public enum AttributeTargetKind
	{
		/// <summary>
		/// The attribute targets the member itself.
		/// </summary>
		This = 0,

		/// <summary>
		/// The attribute targets a field behind the member or its return type.
		/// </summary>
		FieldOrReturn = 1,

		/// <summary>
		/// The attribute targets a method behind the member or its parameter.
		/// </summary>
		MethodOrParam = 2,
	}
}
