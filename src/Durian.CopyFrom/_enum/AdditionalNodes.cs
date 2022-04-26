// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian.Analysis.CopyFrom
{
	/// <summary>
	/// Specifies which non-standard nodes should also be copied.
	/// </summary>
	[Flags]
	public enum AdditionalNodes
	{
		// *********************************************************************************************
		// Values of this enum have to be exactly the same as Durian.CopyFromAdditionalNodes.
		// *********************************************************************************************

		/// <summary>
		/// No non-standard nodes are copied.
		/// </summary>
		None = 0,

		/// <summary>
		/// Specifies that all attribute lists of the target member should also be copied.
		/// </summary>
		Attributes = 1,

		/// <summary>
		/// Specifies that all generic constraints of the target member should also be copied.
		/// </summary>
		Constraints = 2,

		/// <summary>
		/// Specifies that the base type list of the target member should also be copied.
		/// </summary>
		BaseType = 4,

		/// <summary>
		/// Specifies that the base interface list of the target member should also be copied.
		/// </summary>
		BaseInterfaces = 8,

		/// <summary>
		/// Specifies that the documentation of the target member should also be copied.
		/// </summary>
		Documentation = 16,

		/// <summary>
		/// Specifies that all using directives in the file where target member is located should also be copied.
		/// </summary>
		Usings = 32,

		/// <summary>
		/// Specifies that all available non-standard nodes of the target member should also be copied.
		/// </summary>
		All = Attributes | Constraints | BaseType | BaseInterfaces | Documentation | Usings,

		/// <summary>
		/// Specifies that the default configuration should be used.
		/// </summary>
		Default = Usings
	}
}
