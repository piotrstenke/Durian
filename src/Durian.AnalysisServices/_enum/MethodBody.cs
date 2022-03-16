// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis
{
	/// <summary>
	/// Defines type of a method body.
	/// </summary>
	public enum MethodBody
	{
		/// <summary>
		/// Method does not have a body.
		/// </summary>
		None,

		/// <summary>
		/// The method body is a code block.
		/// </summary>
		Block,

		/// <summary>
		/// The method body is an arrow expression.
		/// </summary>
		Expression
	}
}
