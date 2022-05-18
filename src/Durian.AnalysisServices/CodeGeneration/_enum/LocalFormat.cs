// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian.Analysis.CodeGeneration
{
	/// <summary>
	/// Specifies how local variables are written.
	/// </summary>
	[Flags]
	public enum LocalFormat
	{
		/// <summary>
		/// No format applied.
		/// </summary>
		None = 0,

		/// <summary>
		/// Use the <see langword="var"/> keyword instead of an explicit type.
		/// </summary>
		ImplicitType = 1,

		/// <summary>
		/// Skips the initializer character ('=') of the variable.
		/// </summary>
		SkipInitializer = 2
	}
}