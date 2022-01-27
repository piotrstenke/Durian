// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian
{
	/// <summary>
	/// Specifies a <see cref="Type"/> that can use <see langword="internal"/> members of the current <see cref="Type"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
	public sealed class FriendClassAttribute : Attribute
	{
		/// <summary>
		/// Determines whether <see langword="internal"/> members of the current <see cref="Type"/> can be accessed by <see cref="Type"/>s that inherit the <see cref="FriendType"/>. Defaults to <see langword="false"/>.
		/// </summary>
		public bool AllowsFriendChildren { get; set; }

		/// <summary>
		/// Friend <see cref="Type"/> of the current <see cref="Type"/>.
		/// </summary>
		public Type FriendType { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="FriendClassAttribute"/> class.
		/// </summary>
		/// <param name="type">Friend <see cref="Type"/> of the current <see cref="Type"/>.</param>
		public FriendClassAttribute(Type type)
		{
			FriendType = type;
		}
	}
}