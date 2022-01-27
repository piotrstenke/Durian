// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian.Analysis.InterfaceTargets
{
	/// <summary>
	/// Specifies possible targets of an <see langword="interface"/>.
	/// </summary>
	[Flags]
	public enum IntfTargets
	{
		// *********************************************************************************************
		// Values of this enum have to be exactly the same as Durian.InterfaceTargets.
		// *********************************************************************************************

		/// <summary>
		/// Interface cannot be implemented in code, only through reflection.
		/// </summary>
		ReflectionOnly = 0,

		/// <summary>
		/// Interface cannot be implemented in code. This value is the same as <see cref="ReflectionOnly"/>.
		/// </summary>
		None = ReflectionOnly,

		/// <summary>
		/// Interface can be implemented by normal C# classes.
		/// </summary>
		Class = 1,

		/// <summary>
		/// Interface can be implemented by record classes.
		/// </summary>
		RecordClass = 2,

		/// <summary>
		/// Interface can be a base for other interface.
		/// </summary>
		Interface = 4,

		/// <summary>
		/// Interface can be implemented by normal C# structs.
		/// </summary>
		Struct = 8,

		/// <summary>
		/// Interface can be implemented by record structs.
		/// </summary>
		RecordStruct = 16,

		/// <summary>
		/// Interface can be implemented by all valid member kinds.
		/// </summary>
		All = Class | RecordClass | Struct | RecordStruct | Interface
	}
}