// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace Durian.Info
{
	/// <summary>
	/// Compares the <see cref="PackageIdentity.EnumValue"/> of two <see cref="PackageIdentity"/> instances.
	/// </summary>
	public sealed class PackageEnumEqualityComparer : IEqualityComparer<PackageIdentity>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PackageEnumEqualityComparer"/> class.
		/// </summary>
		public PackageEnumEqualityComparer()
		{
		}

		/// <inheritdoc/>
		public bool Equals(PackageIdentity? x, PackageIdentity? y)
		{
			if (x is null)
			{
				return y is null;
			}

			if (y is null)
			{
				return false;
			}

			return x.EnumValue == y.EnumValue;
		}

		/// <inheritdoc/>
		public int GetHashCode(PackageIdentity obj)
		{
			if (obj is null)
			{
				throw new ArgumentNullException(nameof(obj));
			}

			return obj.EnumValue.GetHashCode();
		}
	}
}
