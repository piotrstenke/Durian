// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian.Configuration
{
	/// <summary>
	/// Configures how friend classes of the target <see cref="Type"/> are handled.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
	public sealed class FriendClassConfigurationAttribute : Attribute
	{
		/// <summary>
		/// Determines whether sub-classes of the current type should be treated like friend types. Defaults to <see langword="false"/>.
		/// </summary>
		public bool AllowsChildren { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="FriendClassConfigurationAttribute"/>.
		/// </summary>
		public FriendClassConfigurationAttribute()
		{
		}
	}
}