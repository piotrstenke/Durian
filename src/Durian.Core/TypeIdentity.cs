// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Immutable;

namespace Durian.Info
{
	/// <summary>
	/// Contains basic information about a type contained within a Durian module.
	/// </summary>
	/// <remarks>This class implements the <see cref="IEquatable{T}"/> interface - two instances are compared by their values, not references.
	/// <para>This class implements the <see cref="IDisposable"/> interface - instance should be disposed using the <see cref="Dispose"/> method if its no longer needed.</para></remarks>
	public sealed partial class TypeIdentity : IDurianIdentity, IEquatable<TypeIdentity>, IDisposable
	{
		private bool _disposed;

		/// <summary>
		/// Fully qualified name of the type.
		/// </summary>
		public string FullyQualifiedName
		{
			get
			{
				string n = Namespace;

				if (string.IsNullOrWhiteSpace(n))
				{
					return Name;
				}

				return $"{n}.{Name}";
			}
		}

		/// <summary>
		/// Modules this <see cref="TypeIdentity"/> is part of.
		/// </summary>
		public ImmutableArray<ModuleReference> Modules { get; }

		/// <summary>
		/// Name of the type.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Namespace where the type is to be found.
		/// </summary>
		public string Namespace { get; }

		internal TypeIdentity(string name, string @namespace, DurianModule[] modules)
		{
			Name = name;
			Namespace = @namespace;

			int length = modules.Length;
			ImmutableArray<ModuleReference>.Builder b = ImmutableArray.CreateBuilder<ModuleReference>(length);

			for (int i = 0; i < length; i++)
			{
				b.Add(new ModuleReference(modules[i]));
			}

			Modules = b.ToImmutable();

			IdentityPool.Types.TryAdd(Name, this);
		}

		private TypeIdentity(string name, string @namespace, ImmutableArray<ModuleReference> modules)
		{
			Name = name;
			Namespace = @namespace;
			Modules = modules;

			// This constructor is called only when a clone is created.
			// Since this instance is a clone, it shouldn't have access to the IdentityPool.
			_disposed = true;
		}

		/// <inheritdoc/>
		public static bool operator !=(TypeIdentity? a, TypeIdentity? b)
		{
			return !(a == b);
		}

		/// <inheritdoc/>
		public static bool operator ==(TypeIdentity? a, TypeIdentity? b)
		{
			if (a is null)
			{
				return b is null;
			}

			if (b is null)
			{
				return false;
			}

			return
				a.Name == b.Name &&
				b.Namespace == b.Namespace &&
				Utilities.CompareImmutableArrays(a.Modules, b.Modules);
		}

		/// <inheritdoc cref="ICloneable.Clone"/>
		public TypeIdentity Clone()
		{
			return new TypeIdentity(Name, Namespace, Modules);
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			if (!_disposed)
			{
				IdentityPool.Types.TryRemove(Name, out _);
				_disposed = true;
			}
		}

		/// <inheritdoc/>
		public override bool Equals(object? obj)
		{
			if (obj is not TypeIdentity other)
			{
				return false;
			}

			return other == this;
		}

		/// <inheritdoc/>
		public bool Equals(TypeIdentity? other)
		{
			return other == this;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int hashCode = -1246297765;
			hashCode = (hashCode * -1521134295) + Name.GetHashCode();
			hashCode = (hashCode * -1521134295) + Namespace.GetHashCode();
			hashCode = (hashCode * -1521134295) + Utilities.GetHashCodeOfImmutableArray(Modules);

			return hashCode;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return FullyQualifiedName;
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		IDurianIdentity IDurianIdentity.Clone()
		{
			return Clone();
		}

		internal void SetModule(ModuleIdentity module)
		{
			foreach (ModuleReference m in Modules)
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
