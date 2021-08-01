// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Durian.Configuration;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.FriendClass
{
	/// <summary>
	/// Represents a single friend type specified by the <see cref="FriendClassAttribute"/>.
	/// </summary>
	public readonly struct FriendClass : IEquatable<FriendClass>
	{
		/// <summary>
		/// Default value of the <see cref="FriendClassAttribute.AllowInherit"/> property.
		/// </summary>
		public const bool AllowInheritDefaultValue = false;

		/// <summary>
		/// Default value of the <see cref="FriendClassConfigurationAttribute.AllowInternalsVisibleTo"/> property.
		/// </summary>
		public const bool AllowInternalsVisibleToDefaultValue = true;

		/// <summary>
		/// Default value of the <see cref="FriendClassConfigurationAttribute.ApplyToType"/> property.
		/// </summary>
		public const bool ApplyToTypeDefaultValue = false;

		/// <summary>
		/// Determines whether child <see cref="System.Type"/>s of the target <see cref="Type"/> are also friend <see cref="System.Type"/>s.
		/// </summary>
		public readonly bool AllowInherit { get; }

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> representing the target <see cref="System.Type"/>.
		/// </summary>
		public readonly INamedTypeSymbol Type { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="FriendClass"/> struct.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> representing the target <see cref="System.Type"/>.</param>
		/// <param name="allowInherit">Determines whether child <see cref="System.Type"/>s of the target <see cref="Type"/> are also friend <see cref="System.Type"/>s.</param>
		public FriendClass(INamedTypeSymbol type, bool allowInherit)
		{
			Type = type;
			AllowInherit = allowInherit;
		}

		/// <inheritdoc/>
		public static bool operator !=(FriendClass left, FriendClass right)
		{
			return !(left == right);
		}

		/// <inheritdoc/>
		public static bool operator ==(FriendClass left, FriendClass right)
		{
			return left.AllowInherit == right.AllowInherit && SymbolEqualityComparer.Default.Equals(left.Type, right.Type);
		}

		/// <inheritdoc/>
		public override readonly bool Equals(object obj)
		{
			if (obj is FriendClass f)
			{
				return f == this;
			}

			return false;
		}

		/// <inheritdoc/>
		public readonly bool Equals(FriendClass other)
		{
			return other == this;
		}

		/// <inheritdoc/>
		public override readonly int GetHashCode()
		{
			int hashCode = 542482862;
			hashCode = (hashCode * -1521134295) + AllowInherit.GetHashCode();

			if (Type is not null)
			{
				hashCode = (hashCode * -1521134295) + SymbolEqualityComparer.Default.GetHashCode(Type);
			}

			return hashCode;
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"{Type}, Inherit = {AllowInherit}";
		}
	}
}
