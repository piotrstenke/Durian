// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian.Info
{
	/// <summary>
	/// Defines type of a Durian package.
	/// </summary>
	[Flags]
	public enum PackageType
	{
		/// <summary>
		/// Unspecified package type.
		/// </summary>
		Unspecified = 0,

		/// <summary>
		/// Represents a library package. Libraries don't perform any actual code-related actions.
		/// </summary>
		Library = 1,

		/// <summary>
		/// Represents an analyzer package. Analyzers examine existing code and potentially provide code-fixes.
		/// </summary>
		Analyzer = 2,

		/// <summary>
		/// Represents a static generator package. Static generators always build the same code - for different inputs the output will be always the same.
		/// </summary>
		StaticGenerator = 4,

		/// <summary>
		/// Represents a syntax based generator package. Syntax based generators examine existing code and generate new nodes based on collected information.
		/// </summary>
		SyntaxBasedGenerator = 8,

		/// <summary>
		/// Represents a file based generator package. File based generators work on external files instead of actual C# source code.
		/// </summary>
		FileBasedGenerator = 16,

		/// <summary>
		/// Represents a code fix package. Code fix packages are basically libraries of non-abstract code fixes.
		/// </summary>
		CodeFixLibrary = 32
	}
}