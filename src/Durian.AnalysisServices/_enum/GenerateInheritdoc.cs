// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis
{
	/// <summary>
	/// Determines when to generate the 'inheritdoc' tag.
	/// </summary>
	public enum GenerateInheritdoc
	{
		/// <summary>
		/// The 'inheritdoc' tag is not generated.
		/// </summary>
		Never,

		/// <summary>
		/// The 'inheridoc' tag is always generated, even if the source symbol has no documentation.
		/// </summary>
		Always,

		/// <summary>
		/// The 'inheritdoc' tag is generated only if the source symbol has documentation.
		/// </summary>
		WhenPossible
	};
}
