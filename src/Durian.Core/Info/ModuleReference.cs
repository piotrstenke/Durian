using System;

namespace Durian.Info
{
	/// <summary>
	/// Represents a reference to a <see cref="ModuleIdentity"/> of a specific <see cref="DurianModule"/>.
	/// </summary>
	/// <remarks>
	/// Creating a new instance of <see cref="ModuleIdentity"/> is very costly performance-wise because of array allocations.
	/// This class should be used in cases when there is no need for direct reference to a <see cref="ModuleIdentity"/>
	/// <para>NOTE: This class implements the <see cref="IEquatable{T}"/> - two values are compared by their values, not references.</para></remarks>
	public sealed class ModuleReference : IEquatable<ModuleReference>
	{
		private ModuleIdentity? _module;

		/// <summary>
		/// The module this <see cref="ModuleReference"/> references.
		/// </summary>
		public DurianModule EnumValue { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ModuleReference"/> class.
		/// </summary>
		/// <param name="module">The module this <see cref="ModuleReference"/> references.</param>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public ModuleReference(DurianModule module)
		{
			ModuleIdentity.CheckIsValidModuleEnum(module);
			EnumValue = module;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ModuleReference"/> class.
		/// </summary>
		/// <param name="module">The module this <see cref="ModuleReference"/> references.</param>
		/// <exception cref="ArgumentNullException"><paramref name="module"/> is <see langword="null"/>.</exception>
		public ModuleReference(ModuleIdentity module)
		{
			if(module is null)
			{
				throw new ArgumentNullException(nameof(module));
			}

			_module = module;
			EnumValue = module.Module;
		}

		/// <summary>
		/// Returns the <see cref="ModuleIdentity"/> this <see cref="ModuleReference"/> references.
		/// </summary>
		/// <remarks>NOTE: This method uses lazy initialization.</remarks>
		public ModuleIdentity GetModule()
		{
			return _module ??= ModuleIdentity.GetModule(EnumValue);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return EnumValue.ToString();
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if(obj is not ModuleReference r)
			{
				return false;
			}

			return r == this;
		}

		/// <inheritdoc/>
		public bool Equals(ModuleReference other)
		{
			return other == this;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		/// <inheritdoc/>
		public static bool operator ==(ModuleReference a, ModuleReference b)
		{
			return a.EnumValue == b.EnumValue;
		}

		/// <inheritdoc/>
		public static bool operator !=(ModuleReference a, ModuleReference b)
		{
			return !(a == b);
		}
	}
}
