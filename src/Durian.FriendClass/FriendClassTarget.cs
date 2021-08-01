// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Durian.Analysis.FriendClass
{
	/// <summary>
	/// Represents a <see cref="System.Type"/> with at least one <see cref="FriendClassAttribute"/> applied.
	/// </summary>
	public sealed class FriendClassTarget : IEquatable<FriendClassTarget>
	{
		/// <summary>
		/// Determines whether <see langword="internal"/> members of the target <see cref="Type"/> can be accessed outside of the current assembly if an appropriate <see cref="InternalsVisibleToAttribute"/> is present.
		/// </summary>
		public bool AllowInternalsVisibleTo { get; set; }

		/// <summary>
		/// Determines whether the target <see cref="Type"/> itself, not only its <see langword="internal"/> members, is inaccessible outside of its friend types.
		/// </summary>
		public bool ApplyToType { get; set; }

		/// <summary>
		/// A collection of friend types of the target <see cref="Type"/>.
		/// </summary>
		public ImmutableArray<FriendClass> FriendTypes { get; }

		/// <summary>
		/// <see cref="System.Type"/> the <see cref="FriendClassAttribute"/>s are applied to.
		/// </summary>
		public Type Type { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="FriendClassTarget"/> class.
		/// </summary>
		/// <param name="type"><see cref="System.Type"/> the <see cref="FriendClassAttribute"/>s are applied to.</param>
		/// <param name="friendTypes">A collection of friend types of the target <see cref="Type"/>.</param>
		public FriendClassTarget(Type type, IEnumerable<FriendClass> friendTypes)
		{
			Type = type;
			FriendTypes = friendTypes.ToImmutableArray();
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (obj is FriendClassTarget other)
			{
				return other == this;
			}

			return false;
		}

		/// <inheritdoc/>
		public bool Equals(FriendClassTarget other)
		{
			return other == this;
		}
	}
}
