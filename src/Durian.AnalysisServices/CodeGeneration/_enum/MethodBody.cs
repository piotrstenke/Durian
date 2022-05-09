// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis.CodeGeneration
{
	/// <summary>
	/// Defines available kinds of a method body.
	/// </summary>
	public enum MethodBody
	{
		/// <summary>
		/// Method does not have a body.
		/// </summary>
		None = 0,

		/// <summary>
		/// The method uses a code block as a body.
		/// </summary>
		Block = 1,

		/// <summary>
		/// The method uses an arrow expression '=>' as a body.
		/// </summary>
		Expression = 2
	}
}
