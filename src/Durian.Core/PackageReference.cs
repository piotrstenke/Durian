// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian.Info
{
	/// <summary>
	/// Represents a reference to a <see cref="PackageIdentity"/> of a specific <see cref="DurianPackage"/>.
	/// </summary>
	/// <remarks>
	/// Creating a new instance of <see cref="PackageIdentity"/> is very costly performance-wise because of array allocations.
	/// This class should be used in cases when there is no need for direct reference to a <see cref="PackageIdentity"/>
	/// <para>This class implements the <see cref="IEquatable{T}"/> interface - two instances are compared by their values, not references.</para></remarks>
	public sealed class PackageReference : IDurianReference, IEquatable<PackageReference>
	{
		private readonly ModuleIdentity? _targetIdentity;
		private PackageIdentity? _package;

		/// <summary>
		/// The package this <see cref="PackageReference"/> references.
		/// </summary>
		public DurianPackage EnumValue { get; }

		/// <summary>
		/// Determines whether the <see cref="PackageIdentity"/> object has been allocated.
		/// </summary>
		public bool IsAllocated => _package is not null;

		/// <summary>
		/// Initializes a new instance of the <see cref="PackageReference"/> class.
		/// </summary>
		/// <param name="package">The package this <see cref="PackageReference"/> references.</param>
		/// <exception cref="ArgumentException">Unknown <see cref="DurianPackage"/> value detected.</exception>
		public PackageReference(DurianPackage package)
		{
			PackageIdentity.EnsureIsValidPackageEnum(package);
			EnumValue = package;
			_package = null;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PackageReference"/> class.
		/// </summary>
		/// <param name="package">The package this <see cref="PackageReference"/> references.</param>
		/// <exception cref="ArgumentNullException"><paramref name="package"/> is <see langword="null"/>.</exception>
		public PackageReference(PackageIdentity package)
		{
			if (package is null)
			{
				throw new ArgumentNullException(nameof(package));
			}

			_package = package;
			EnumValue = package.EnumValue;
		}

		internal PackageReference(DurianPackage package, ModuleIdentity targetIdentity)
		{
			EnumValue = package;
			_targetIdentity = targetIdentity;
		}

		/// <inheritdoc/>
		public static bool operator !=(PackageReference? a, PackageReference? b)
		{
			return !(a == b);
		}

		/// <inheritdoc/>
		public static bool operator ==(PackageReference? a, PackageReference? b)
		{
			if (a is null)
			{
				return b is null;
			}

			if (b is null)
			{
				return false;
			}

			return a.EnumValue == b.EnumValue;
		}

		/// <summary>
		/// Sets the specified <paramref name="package"/> as the identity to be pointed to.
		/// </summary>
		/// <param name="package"><see cref="PackageIdentity"/> to be set as the identity to be pointed to.</param>
		/// <exception cref="ArgumentNullException"><paramref name="package"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><paramref name="package"/> represents different <see cref="DurianPackage"/> than the reference.</exception>
		public void Accept(PackageIdentity package)
		{
			if (package is null)
			{
				throw new ArgumentNullException(nameof(package));
			}

			if (package.EnumValue != EnumValue)
			{
				throw new InvalidOperationException($"Provided identity represents the '{package.EnumValue}' package, but the reference points to '{EnumValue}'!");
			}

			_package = package;
		}

		/// <summary>
		/// Allocates the <see cref="PackageIdentity"/> this <see cref="PackageReference"/> references.
		/// </summary>
		public void Allocate()
		{
			if (_package is null)
			{
				Reallocate();
			}
		}

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>A new object that is a copy of this instance.</returns>
		public PackageReference Clone()
		{
			if (_package is null)
			{
				return new(EnumValue);
			}

			return new(_package);
		}

		/// <summary>
		/// Removes from memory the <see cref="PackageIdentity"/> this <see cref="PackageReference"/> references.
		/// </summary>
		public void Deallocate()
		{
			_package = null;
		}

		/// <inheritdoc/>
		public override bool Equals(object? obj)
		{
			if (obj is not PackageReference r)
			{
				return false;
			}

			return r == this;
		}

		/// <inheritdoc/>
		public bool Equals(PackageReference? other)
		{
			return other == this;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return EnumValue.GetHashCode();
		}

		/// <summary>
		/// Returns the <see cref="PackageIdentity"/> this <see cref="PackageReference"/> references.
		/// </summary>
		/// <remarks>This method uses lazy initialization.</remarks>
		public PackageIdentity GetPackage()
		{
			Allocate();
			return _package!;
		}

		/// <summary>
		/// Allocates new or overrides existing <see cref="PackageIdentity"/> this <see cref="PackageReference"/> references.
		/// </summary>
		public void Reallocate()
		{
			_package = PackageIdentity.GetPackage(EnumValue);

			if (_targetIdentity is not null)
			{
				_package.SetModule(_targetIdentity);
			}
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return EnumValue.ToString();
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		IDurianReference IDurianReference.Clone()
		{
			return Clone();
		}

		object? IDurianReference.GetAllocatedValue()
		{
			return GetPackage();
		}
	}
}
