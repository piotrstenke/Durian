// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian.Analysis.Extensions
{
	/// <summary>
	/// Configures how generic type parameters are substituted.
	/// </summary>
	[Flags]
	public enum GenericSubstitution
	{
		/// <summary>
		/// None additional changes are applied,
		/// </summary>
		None = 0,

		/// <summary>
		/// Type arguments are used instead of parameters.
		/// </summary>
		TypeArguments = 1,

		/// <summary>
		/// If the symbol is a method, its parameter list is included.
		/// </summary>
		ParameterList = 2,

		/// <summary>
		/// Includes variance of the type parameters.
		/// </summary>
		Variance = 4
	}
}
