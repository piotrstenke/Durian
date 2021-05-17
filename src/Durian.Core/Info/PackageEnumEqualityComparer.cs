using System.Collections.Generic;

namespace Durian.Info
{
	/// <summary>
	/// Compares the <see cref="PackageIdentity.EnumValue"/> of two <see cref="PackageIdentity"/> instances.
	/// </summary>
	public sealed class PackageEnumEqualityComparer : IEqualityComparer<PackageIdentity>
	{
		/// <summary>
		/// Returns a shared instance of <see cref="PackageEnumEqualityComparer"/>.
		/// </summary>
		public static PackageEnumEqualityComparer Instance { get; } = new();

		/// <summary>
		/// Initializes a new instance of the <see cref="PackageEnumEqualityComparer"/> class.
		/// </summary>
		public PackageEnumEqualityComparer()
		{
		}

		/// <inheritdoc/>
		public bool Equals(PackageIdentity x, PackageIdentity y)
		{
			return x.EnumValue == y.EnumValue;
		}

		/// <inheritdoc/>
		public int GetHashCode(PackageIdentity obj)
		{
			return obj.EnumValue.GetHashCode();
		}
	}
}
