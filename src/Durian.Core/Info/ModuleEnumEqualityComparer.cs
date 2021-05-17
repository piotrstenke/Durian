using System.Collections.Generic;

namespace Durian.Info
{
	/// <summary>
	/// Compares the <see cref="ModuleIdentity.Module"/> enum value of two <see cref="ModuleIdentity"/> instances.
	/// </summary>
	public sealed class ModuleEnumEqualityComparer : IEqualityComparer<ModuleIdentity>
	{
		/// <summary>
		/// Returns a shared instance of <see cref="ModuleEnumEqualityComparer"/>.
		/// </summary>
		public static ModuleEnumEqualityComparer Instance { get; } = new();

		/// <summary>
		/// Initializes a new instance of the <see cref="ModuleEnumEqualityComparer"/> class.
		/// </summary>
		public ModuleEnumEqualityComparer()
		{
		}

		/// <inheritdoc/>
		public bool Equals(ModuleIdentity x, ModuleIdentity y)
		{
			return x.Module == y.Module;
		}

		/// <inheritdoc/>
		public int GetHashCode(ModuleIdentity obj)
		{
			return obj.Module.GetHashCode();
		}
	}
}
