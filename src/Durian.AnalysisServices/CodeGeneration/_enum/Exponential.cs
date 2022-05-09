﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis.CodeGeneration
{
	/// <summary>
	/// Specifies type of exponential used to write a decimal value.
	/// </summary>
	public enum Exponential
	{
		/// <summary>
		/// No exponential specified.
		/// </summary>
		None = 0,

		/// <summary>
		/// The exponential is specified using the 'e' character.
		/// </summary>
		Lowercase = 1,

		/// <summary>
		/// The exponential is specified using the 'E' character.
		/// </summary>
		Uppercase = 2
	}
}
