// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian.Analysis.CodeGeneration
{
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
}
