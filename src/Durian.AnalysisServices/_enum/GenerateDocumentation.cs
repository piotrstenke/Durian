// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis
{
	/// <summary>
	/// Determines when to generate XML documentation.
	/// </summary>
	public enum GenerateDocumentation
	{
		/// <summary>
		/// The XML documentation is never generated.
		/// </summary>
		Never,

		/// <summary>
		/// The XML documentation is always generated, even if the source symbol has no documentation.
		/// </summary>
		Always,

		/// <summary>
		/// The XML documentation is generated only if the source symbol has documentation.
		/// </summary>
		WhenPossible
	};
}
