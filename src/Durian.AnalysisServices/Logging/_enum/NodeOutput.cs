// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Logging
{
	/// <summary>
	/// Determines what to output when a <see cref="SyntaxNode"/> is being logged.
	/// </summary>
	public enum NodeOutput
	{
		/// <summary>
		/// Uses the default value specified in configuration.
		/// </summary>
		Default,

		/// <summary>
		/// Outputs only the target <see cref="SyntaxNode"/>.
		/// </summary>
		Node,

		/// <summary>
		/// Outputs containing node of the <see cref="SyntaxNode"/>.
		/// </summary>
		Containing,

		/// <summary>
		/// Outputs whole <see cref="Microsoft.CodeAnalysis.SyntaxTree"/> associated with the <see cref="SyntaxNode"/>.
		/// </summary>
		SyntaxTree
	}
}