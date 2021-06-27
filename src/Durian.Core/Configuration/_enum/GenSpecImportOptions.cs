// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Configuration
{
	/// <summary>
	/// Configures how the generator should behave when creating a generic specialization for a class outside of the current assembly.
	/// </summary>
	public enum GenSpecImportOptions
	{
		/// <summary>
		/// Prevent creating generic specializations for imported classes.
		/// </summary>
		Prevent = 0,

		/// <summary>
		/// Allows to create generic specialization for imported classes that are marked with the <see cref="AllowSpecializationAttribute"/>.
		/// </summary>
		OverrideMarked = 1,

		/// <summary>
		/// Allows to create generic specialization for imported classes that are not placed in the <c>System</c> namespace or any of its children namespaces.
		/// </summary>
		OverrideNonSystem = 2,

		/// <summary>
		/// Allows to create generic specialization for imported classes that are placed in the <c>System</c> namespace or any of its children namespaces.
		/// </summary>
		OverrideSystem = 3,

		/// <summary>
		/// Allows to create generic specialization for any imported class, regardless of its origin assembly or namespace.
		/// </summary>
		OverrideAny = 4,
	}
}
