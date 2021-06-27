// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Durian.Info
{
	/// <summary>
	/// Contains basic information about a Durian package.
	/// </summary>
	/// <remarks><para>NOTE: This class implements the <see cref="IEquatable{T}"/> - two values are compared by their values, not references.</para></remarks>
	[DebuggerDisplay("Name = {Name}, Version = {Version}")]
	public sealed partial class PackageIdentity : IEquatable<PackageIdentity>, ICloneable
	{
		private ImmutableArray<ModuleReference> _modules;

		/// <summary>
		/// Enum value of <see cref="DurianPackage"/> that corresponds with this <see cref="PackageIdentity"/>.
		/// </summary>
		public DurianPackage EnumValue { get; }

		/// <summary>
		/// Determines whether this package is a part of any Durian module.
		/// </summary>
		public bool IsPartOfAnyModule => Modules.Length == 0;

		/// <summary>
		/// Durian modules this package is part of.
		/// </summary>
		/// <remarks>Returns an empty <see cref="ImmutableArray{T}"/> if this package is not part of any Durian module.</remarks>
		public ImmutableArray<ModuleReference> Modules => _modules;

		/// <summary>
		/// Name of the package.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Type of this package.
		/// </summary>
		public PackageType Type { get; }

		/// <summary>
		/// Version of the package.
		/// </summary>
		public string Version { get; }

		internal PackageIdentity(DurianPackage enumValue, string version, PackageType type)
		{
			EnumValue = enumValue;
			Name = PackageToString(enumValue);
			Version = version;
			Type = type;
		}

		private PackageIdentity(DurianPackage enumValue, string name, string version, PackageType type, ref ImmutableArray<ModuleReference> modules)
		{
			EnumValue = enumValue;
			Name = name;
			Version = version;
			Type = type;
			_modules = modules;
		}

		/// <inheritdoc/>
		public static bool operator !=(PackageIdentity a, PackageIdentity b)
		{
			return !(a == b);
		}

		/// <inheritdoc/>
		public static bool operator ==(PackageIdentity a, PackageIdentity b)
		{
			return
				a.EnumValue == b.EnumValue &&
				a.Version == b.Version &&
				a.Name == b.Name &&
				a.Type == b.Type &&
				Utilities.CompareImmutableArrays(ref a._modules, ref b._modules);
		}

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>A new object that is a copy of this instance.</returns>
		public PackageIdentity Clone()
		{
			return new PackageIdentity(EnumValue, Name, Version, Type, ref _modules);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (obj is not PackageIdentity other)
			{
				return false;
			}

			return other == this;
		}

		/// <inheritdoc/>
		public bool Equals(PackageIdentity other)
		{
			return other == this;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int hashCode = -726504116;
			hashCode = (hashCode * -1521134295) + Name.GetHashCode();
			hashCode = (hashCode * -1521134295) + EnumValue.GetHashCode();
			hashCode = (hashCode * -1521134295) + Version.GetHashCode();
			hashCode = (hashCode * -1521134295) + Type.GetHashCode();
			hashCode = (hashCode * -1521134295) + Utilities.GetHashCodeOfImmutableArray(ref _modules);

			return hashCode;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Name}, {Version}";
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		internal void Initialize(DurianModule[]? modules)
		{
			if (modules is null)
			{
				_modules = ImmutableArray.Create<ModuleReference>();
				return;
			}

			int length = modules.Length;
			ImmutableArray<ModuleReference>.Builder b = ImmutableArray.CreateBuilder<ModuleReference>(length);

			for (int i = 0; i < length; i++)
			{
				b.Add(new ModuleReference(modules[i]));
			}

			_modules = b.ToImmutable();
		}
	}
}
