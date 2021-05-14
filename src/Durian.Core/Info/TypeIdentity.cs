using System;

namespace Durian.Info
{
	/// <summary>
	/// Contains basic information about a type contained within a Durian module.
	/// </summary>
	public sealed record TypeIdentity : IEquatable<TypeIdentity>
	{
		private ModuleIdentity? _module;

		/// <summary>
		/// Module this <see cref="TypeIdentity"/> is part of.
		/// </summary>
		public ModuleIdentity Module => _module!;

		/// <summary>
		/// Name of the type.
		/// </summary>
		public string Name { get; }

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
		/// Namespace where the type is to be found.
		/// </summary>
		public string Namespace { get; }

		internal TypeIdentity(string name, string @namespace)
		{
			Name = name;
			Namespace = @namespace;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return FullyQualifiedName;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int hashCode = -1246297765;
			hashCode = (hashCode * -1521134295) + _module!.GetHashCode();
			hashCode = (hashCode * -1521134295) + Name.GetHashCode();
			hashCode = (hashCode * -1521134295) + Namespace.GetHashCode();
			return hashCode;
		}

		internal void SetModule(ModuleIdentity module)
		{
			_module = module;
		}
	}
}
