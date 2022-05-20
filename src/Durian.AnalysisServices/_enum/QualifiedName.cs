// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis
{
	/// <summary>
	/// Determines format of a qualified name.
	/// </summary>
	public enum QualifiedName
	{
		/// <summary>
		/// The qualified name uses the same format is in code.
		/// </summary>
		Code = 0,

		/// <summary>
		/// The qualified name uses metadata format.
		/// </summary>
		Metadata = 1,

		/// <summary>
		/// The qualified name uses XML-supported format.
		/// </summary>
		Xml = 2
	}
}
