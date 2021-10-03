// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian
{
	/// <summary>
	/// Defines accessibility that can be applied to a generated type.
	/// </summary>
	public enum GeneratedTypeAccess
	{
		/// <summary>
		/// Applies the default accessibility defined by a given generator.
		/// </summary>
		Default,

		/// <summary>
		/// Accessibility is not explicitly specified.
		/// </summary>
		Unspecified,

		/// <summary>
		/// Generated type is <see langword="public"/>.
		/// </summary>
		Public,

		/// <summary>
		/// Generated type is <see langword="internal"/>.
		/// </summary>
		Internal
	}
}
