// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.CompilerServices;

namespace Durian.Configuration
{
	/// <summary>
	/// Configures how friend classes of the target <see cref="Type"/> are handled.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
	public sealed class FriendClassConfigurationAttribute : Attribute
	{
		/// <summary>
		/// Determines whether <see langword="internal"/> members of the current <see cref="Type"/> can be accessed outside of the current assembly if an appropriate <see cref="InternalsVisibleToAttribute"/> is present. Defaults to <see langword="true"/>.
		/// </summary>
		public bool AllowInternalsVisibleTo { get; set; } = true;

		/// <summary>
		/// Determines whether the type itself is inaccessible outside of its friend types. Defaults to <see langword="false"/>.
		/// </summary>
		public bool ApplyToType { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="FriendClassConfigurationAttribute"/>.
		/// </summary>
		public FriendClassConfigurationAttribute()
		{
		}
	}
}
