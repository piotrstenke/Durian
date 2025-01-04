using System;

namespace Durian.Info;

/// <summary>
/// Represents a reference to a <see cref="ModuleIdentity"/> of a specific <see cref="DurianModule"/>.
/// </summary>
/// <remarks>
/// Creating a new instance of <see cref="ModuleIdentity"/> is very costly performance-wise because of array allocations.
/// This class should be used in cases when there is no need for direct reference to a <see cref="ModuleIdentity"/>
/// <para>This class implements the <see cref="IEquatable{T}"/> interface - two instances are compared by their values, not references</para></remarks>
public sealed class ModuleReference : IDurianReference, IEquatable<ModuleReference>
{
	private readonly PackageIdentity? _targetIdentity;

	private ModuleIdentity? _module;

	/// <summary>
	/// The module this <see cref="ModuleReference"/> references.
	/// </summary>
	public DurianModule EnumValue { get; }

	/// <summary>
	/// Determines whether the <see cref="PackageIdentity"/> object has been allocated.
	/// </summary>
	public bool IsAllocated => _module is not null;

	/// <summary>
	/// Initializes a new instance of the <see cref="ModuleReference"/> class.
	/// </summary>
	/// <param name="module">The module this <see cref="ModuleReference"/> references.</param>
	/// <exception cref="ArgumentException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
	public ModuleReference(DurianModule module)
	{
		ModuleIdentity.EnsureIsValidModuleEnum(module);
		EnumValue = module;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ModuleReference"/> class.
	/// </summary>
	/// <param name="module">The module this <see cref="ModuleReference"/> references.</param>
	/// <exception cref="ArgumentNullException"><paramref name="module"/> is <see langword="null"/>.</exception>
	public ModuleReference(ModuleIdentity module)
	{
		if (module is null)
		{
			throw new ArgumentNullException(nameof(module));
		}

		_module = module;
		EnumValue = module.Module;
	}

	internal ModuleReference(DurianModule module, PackageIdentity targetIdentity)
	{
		EnumValue = module;
		_targetIdentity = targetIdentity;
	}

	/// <inheritdoc/>
	public static bool operator !=(ModuleReference? a, ModuleReference? b)
	{
		return !(a == b);
	}

	/// <inheritdoc/>
	public static bool operator ==(ModuleReference? a, ModuleReference? b)
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
	/// Sets the specified <paramref name="module"/> as the identity to be pointed to.
	/// </summary>
	/// <param name="module"><see cref="ModuleIdentity"/> to set as the identity to be pointed to.</param>
	/// <exception cref="ArgumentNullException"><paramref name="module"/> is <see langword="null"/>.</exception>
	/// <exception cref="InvalidOperationException"><paramref name="module"/> represents different <see cref="DurianModule"/> than the reference.</exception>
	public void Accept(ModuleIdentity module)
	{
		if (module is null)
		{
			throw new ArgumentNullException(nameof(module));
		}

		if (module.Module != EnumValue)
		{
			throw new InvalidOperationException($"Provided identity represents the '{module.Module}' module, but the reference points to '{EnumValue}'!");
		}

		_module = module;
	}

	/// <summary>
	/// Allocates the <see cref="ModuleIdentity"/> this <see cref="ModuleReference"/> references.
	/// </summary>
	public void Allocate()
	{
		if (_module is null)
		{
			Reallocate();
		}
	}

	/// <summary>
	/// Creates a new object that is a copy of the current instance.
	/// </summary>
	/// <returns>A new object that is a copy of this instance.</returns>
	public ModuleReference Clone()
	{
		if (_module is null)
		{
			return new(EnumValue);
		}

		return new(_module);
	}

	/// <summary>
	/// Removes from memory the <see cref="ModuleIdentity"/> this <see cref="ModuleReference"/> references.
	/// </summary>
	public void Deallocate()
	{
		_module = null;
	}

	/// <inheritdoc/>
	public override bool Equals(object? obj)
	{
		if (obj is not ModuleReference r)
		{
			return false;
		}

		return r == this;
	}

	/// <inheritdoc/>
	public bool Equals(ModuleReference? other)
	{
		return other == this;
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		return EnumValue.GetHashCode();
	}

	/// <summary>
	/// Returns the <see cref="ModuleIdentity"/> this <see cref="ModuleReference"/> references.
	/// </summary>
	/// <remarks>This method uses lazy initialization.</remarks>
	public ModuleIdentity GetModule()
	{
		Allocate();
		return _module!;
	}

	/// <summary>
	/// Allocates new or overrides existing <see cref="ModuleIdentity"/> this <see cref="ModuleReference"/> references.
	/// </summary>
	public void Reallocate()
	{
		_module = ModuleIdentity.GetModule(EnumValue);

		if (_targetIdentity is not null)
		{
			_module.SetPackage(_targetIdentity);
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
		return GetModule();
	}
}
