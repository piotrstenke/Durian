﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Immutable;

namespace Durian.Info
{
	/// <summary>
	/// Contains basic information about a type contained within a Durian module.
	/// </summary>
	/// <remarks><para>NOTE: This class implements the <see cref="IEquatable{T}"/> - two values are compared by their values, not references.</para></remarks>
	public sealed partial class TypeIdentity : IEquatable<TypeIdentity>, ICloneable
	{
		private ImmutableArray<ModuleReference> _modules;

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
		public ImmutableArray<ModuleReference> Modules => _modules;

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

			_modules = b.ToImmutable();
		}

		private TypeIdentity(string name, string @namespace, ref ImmutableArray<ModuleReference> modules)
		{
			Name = name;
			Namespace = @namespace;
			_modules = modules;
		}

		/// <inheritdoc/>
		public static bool operator !=(TypeIdentity a, TypeIdentity b)
		{
			return !(a == b);
		}

		/// <inheritdoc/>
		public static bool operator ==(TypeIdentity a, TypeIdentity b)
		{
			return
				a.Name == b.Name &&
				b.Namespace == b.Namespace &&
				Utilities.CompareImmutableArrays(ref a._modules, ref b._modules);
		}

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>A new object that is a copy of this instance.</returns>
		public TypeIdentity Clone()
		{
			return new TypeIdentity(Name, Namespace, ref _modules);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (obj is not TypeIdentity other)
			{
				return false;
			}

			return other == this;
		}

		/// <inheritdoc/>
		public bool Equals(TypeIdentity other)
		{
			return other == this;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int hashCode = -1246297765;
			hashCode = (hashCode * -1521134295) + Name.GetHashCode();
			hashCode = (hashCode * -1521134295) + Namespace.GetHashCode();
			hashCode = (hashCode * -1521134295) + Utilities.GetHashCodeOfImmutableArray(ref _modules);

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
	}
}
