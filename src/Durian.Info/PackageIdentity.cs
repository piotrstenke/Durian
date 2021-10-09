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
	/// <remarks>This class implements the <see cref="IEquatable{T}"/> interface - two instances are compared by their values, not references.
	/// <para>This class implements the <see cref="IDisposable"/> interface - instance should be disposed using the <see cref="Dispose"/> method if its no longer needed.</para></remarks>
	[DebuggerDisplay("Name = {Name}, Version = {Version}")]
	public sealed partial class PackageIdentity : IDurianIdentity, IEquatable<PackageIdentity>, IDisposable
	{
		private bool _disposed;
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

		internal PackageIdentity(DurianPackage enumValue, string version, PackageType type, DurianModule[]? modules)
		{
			EnumValue = enumValue;
			Name = PackageToString(enumValue);
			Version = version;
			Type = type;

			if (modules is null || modules.Length == 0)
			{
				_modules = ImmutableArray.Create<ModuleReference>();
			}
			else
			{
				int length = modules.Length;
				ImmutableArray<ModuleReference>.Builder b = ImmutableArray.CreateBuilder<ModuleReference>(length);

				for (int i = 0; i < length; i++)
				{
					b.Add(new ModuleReference(modules[i], this));
				}

				_modules = b.ToImmutable();
			}

			IdentityPool.Packages.TryAdd(Name, this);
		}

		private PackageIdentity(DurianPackage enumValue, string name, string version, PackageType type, ImmutableArray<ModuleReference> modules)
		{
			EnumValue = enumValue;
			Name = name;
			Version = version;
			Type = type;
			_modules = modules;

			// This constructor is called only when a clone is created.
			// Since this instance is a clone, it shouldn't have access to the IdentityPool.
			_disposed = true;
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
				Utilities.CompareImmutableArrays(a._modules, b._modules);
		}

		/// <inheritdoc cref="ICloneable.Clone"/>
		public PackageIdentity Clone()
		{
			return new PackageIdentity(EnumValue, Name, Version, Type, _modules);
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			if (!_disposed)
			{
				IdentityPool.Packages.TryRemove(Name, out _);
				_disposed = true;
			}
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
			hashCode = (hashCode * -1521134295) + Utilities.GetHashCodeOfImmutableArray(_modules);

			return hashCode;
		}

		/// <summary>
		/// Returns a new instance of <see cref="PackageReference"/> pointing to the this <see cref="PackageIdentity"/>.
		/// </summary>
		public PackageReference GetReference()
		{
			return new PackageReference(this);
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

		internal void SetModule(ModuleIdentity module)
		{
			foreach (ModuleReference m in _modules)
			{
				if (m.EnumValue == module.Module)
				{
					m.Accept(module);
					return;
				}
			}
		}
	}
}
