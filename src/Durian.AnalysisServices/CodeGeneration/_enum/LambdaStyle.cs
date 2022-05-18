// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis.CodeGeneration
{
	/// <summary>
	/// Defines available kinds of an anonymous function body.
	/// </summary>
	public enum LambdaStyle
	{
		/// <summary>
		/// The anonymous function does not have a body.
		/// </summary>
		None = 0,

		/// <summary>
		/// The anonymous function uses a code block as a body.
		/// </summary>
		Block = 1,

		/// <summary>
		/// The anonymous function uses an arrow expression '=>' as a body.
		/// </summary>
		Expression = 2,

		/// <summary>
		/// The anonymous function uses a code block with the <see langword="delegate"/> keyword as a body.
		/// </summary>
		Method = 3
	}
}
