using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Durian.Info;

/// <summary>
/// A collection of Durian modules represented as either <see cref="ModuleReference"/>, direct <see cref="ModuleIdentity"/> or a value of <see cref="DurianModule"/>.
/// </summary>
/// <remarks>This class implements the <see cref="IEquatable{T}"/> interface - two instances are compared by their values, not references.</remarks>
[DebuggerDisplay("Count = {_references?.Count ?? 0}, nq")]
public sealed class ModuleContainer : IDurianContainer, ICollection<ModuleReference>
{
	// Both lists must have the same length.

	private readonly List<DurianModule> _enums;
	private readonly List<ModuleReference?> _references;

	/// <summary>
	/// Number of referenced Durian modules.
	/// </summary>
	public int Count => _references.Count;

	/// <summary>
	/// Determines whether the current <see cref="ModuleContainer"/> does not contain any Durian modules.
	/// </summary>
	public bool IsEmpty => Count == 0;

	bool ICollection<ModuleReference>.IsReadOnly => false;

	/// <summary>
	/// Initializes a new instance of the <see cref="ModuleContainer"/> class.
	/// </summary>
	public ModuleContainer()
	{
		_references = new List<ModuleReference?>();
		_enums = new List<DurianModule>();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ModuleContainer"/> class.
	/// </summary>
	/// <param name="references">A collection of <see cref="ModuleReference"/>s that the current instance should include.</param>
	/// <remarks>Values equal to <see langword="null"/> are omitted.</remarks>
	/// <exception cref="ArgumentNullException"><paramref name="references"/> is <see langword="null"/>.</exception>
	public ModuleContainer(IEnumerable<ModuleReference> references)
	{
		if (references is null)
		{
			throw new ArgumentNullException(nameof(references));
		}

		ModuleReference[] modules = references.ToArray();

		_references = new List<ModuleReference?>(modules.Length);
		_enums = new List<DurianModule>(modules.Length);

		foreach (ModuleReference r in modules)
		{
			if (r is null)
			{
				continue;
			}

			_references.Add(r);
			_enums.Add(r.EnumValue);
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ModuleContainer"/> class.
	/// </summary>
	/// <param name="modules">A collection of <see cref="ModuleIdentity"/>s that the current instance should include.</param>
	/// <remarks>Values equal to <see langword="null"/> are omitted.</remarks>
	/// <exception cref="ArgumentNullException"><paramref name="modules"/> is <see langword="null"/>.</exception>
	public ModuleContainer(IEnumerable<ModuleIdentity> modules)
	{
		if (modules is null)
		{
			throw new ArgumentNullException(nameof(modules));
		}

		ModuleIdentity[] identities = modules.ToArray();

		_references = new List<ModuleReference?>(identities.Length);
		_enums = new List<DurianModule>(identities.Length);

		foreach (ModuleIdentity m in identities)
		{
			if (m is null)
			{
				continue;
			}

			_references.Add(new ModuleReference(m));
			_enums.Add(m.Module);
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ModuleContainer"/> class.
	/// </summary>
	/// <param name="modules">A collection of <see cref="DurianModule"/>s that the current instance should include.</param>
	/// <exception cref="ArgumentNullException"><paramref name="modules"/> is <see langword="null"/>.</exception>
	/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected.</exception>
	public ModuleContainer(IEnumerable<DurianModule> modules)
	{
		if (modules is null)
		{
			throw new ArgumentNullException(nameof(modules));
		}

		List<DurianModule> enums = modules.ToList();

		foreach (DurianModule m in modules)
		{
			ModuleIdentity.EnsureIsValidModuleEnum_InvOp(m);
		}

		_enums = enums;
		_references = new List<ModuleReference?>(_enums.Count);

		for (int i = 0; i < _enums.Count; i++)
		{
			_references.Add(null);
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ModuleContainer"/> class.
	/// </summary>
	/// <param name="capacity">Initial number of modules the current instance can store without resizing of a internal container.</param>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than <c>0</c>.</exception>
	public ModuleContainer(int capacity)
	{
		_references = new List<ModuleReference?>(capacity);
		_enums = new List<DurianModule>(capacity);
	}

	internal ModuleContainer(List<DurianModule> enums)
	{
		_enums = enums;
		_references = new List<ModuleReference?>(_enums.Count);

		for (int i = 0; i < _enums.Count; i++)
		{
			_references.Add(null);
		}
	}

	private ModuleContainer(List<DurianModule> enums, List<ModuleReference?> references)
	{
		_enums = enums;
		_references = references;
	}

	/// <summary>
	/// Returns all elements in the container as values of <see cref="DurianModule"/>.
	/// </summary>
	public DurianModule[] AsEnums()
	{
		if (Count == 0)
		{
			return Array.Empty<DurianModule>();
		}

		DurianModule[] array = new DurianModule[Count];

		for (int i = 0; i < array.Length; i++)
		{
			array[i] = _enums[i];
		}

		return array;
	}

	/// <summary>
	/// Returns all elements in the container as <see cref="ModuleIdentity"/>s.
	/// </summary>
	public ModuleIdentity[] AsIdentities()
	{
		if (Count == 0)
		{
			return Array.Empty<ModuleIdentity>();
		}

		ModuleIdentity[] array = new ModuleIdentity[Count];

		for (int i = 0; i < array.Length; i++)
		{
			ModuleReference reference = GetReferenceAt(i);

			array[i] = reference.GetModule();
		}

		return array;
	}

	/// <summary>
	/// Returns all elements in the container as <see cref="ModuleReference"/>s.
	/// </summary>
	public ModuleReference[] AsReferences()
	{
		if (Count == 0)
		{
			return Array.Empty<ModuleReference>();
		}

		ModuleReference[] array = new ModuleReference[Count];

		for (int i = 0; i < array.Length; i++)
		{
			array[i] = GetReferenceAt(i);
		}

		return array;
	}

	/// <summary>
	/// Removes all referenced Durian modules from the container.
	/// </summary>
	public void Clear()
	{
		_references.Clear();
	}

	/// <summary>
	/// Creates a new <see cref="ModuleContainer"/> that is a copy of the current instance.
	/// </summary>
	/// <param name="sharedReference">Determines whether to share internal list of modules between both instances.</param>
	public ModuleContainer Clone(bool sharedReference = true)
	{
		List<DurianModule> enums;
		List<ModuleReference?> references;

		if (sharedReference)
		{
			references = _references;
			enums = _enums;
		}
		else
		{
			references = new List<ModuleReference?>(Count);
			enums = new List<DurianModule>(Count);

			references.AddRange(_references);
		}

		return new ModuleContainer(enums, references);
	}

	/// <summary>
	/// Determines whether the specified <paramref name="module"/> is included withing this instance.
	/// </summary>
	/// <param name="module"><see cref="DurianModule"/> to check.</param>
	public bool Contains(DurianModule module)
	{
		return _enums.Contains(module);
	}

	/// <summary>
	/// Determines whether any Durian module in this container is compliant with the specified <paramref name="predicate"/>.
	/// </summary>
	/// <param name="predicate">Function that picks <see cref="ModuleReference"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
	public bool Contains(Predicate<ModuleReference> predicate)
	{
		if (predicate is null)
		{
			throw new ArgumentNullException(nameof(predicate));
		}

		for (int i = 0; i < Count; i++)
		{
			ModuleReference reference = GetReferenceAt(i);

			if (predicate(reference))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Removes all duplicate module entries from the container.
	/// </summary>
	public void Distinct()
	{
		List<int> duplicates = new(Count);

		for (int i = Count - 1; i > -1; i++)
		{
			for (int j = i - 1; j > -1; j++)
			{
				if (_enums[i] == _enums[j])
				{
					duplicates.Add(i);
				}
			}
		}

		foreach (int dup in duplicates)
		{
			_enums.RemoveAt(dup);
			_references.RemoveAt(dup);
		}
	}

	/// <summary>
	/// Returns a collection of all <see cref="ModuleIdentity"/>s that are compliant with the specified <paramref name="predicate"/>.
	/// </summary>
	/// <param name="predicate">Function that determines whether to include a specific <see cref="ModuleIdentity"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
	public IEnumerable<ModuleIdentity> GetAllModules(Predicate<ModuleReference> predicate)
	{
		if (predicate is null)
		{
			throw new ArgumentNullException(nameof(predicate));
		}

		List<ModuleIdentity> modules = new(Count);

		for (int i = 0; i < Count; i++)
		{
			ModuleReference reference = GetReferenceAt(i);

			if (predicate(reference))
			{
				modules.Add(reference.GetModule());
			}
		}

		return modules;
	}

	/// <summary>
	/// Returns a collection of all <see cref="ModuleIdentity"/>s that represent the specified <paramref name="module"/>.
	/// </summary>
	/// <param name="module"><see cref="DurianModule"/> to get the <see cref="ModuleIdentity"/>s for.</param>
	public IEnumerable<ModuleIdentity> GetAllModules(DurianModule module)
	{
		List<ModuleIdentity> modules = new(Count);

		for (int i = 0; i < Count; i++)
		{
			if (_enums[i] == module)
			{
				ModuleReference reference = GetReferenceAt(i);

				modules.Add(reference.GetModule());
			}
		}

		return modules;
	}

	/// <summary>
	/// Returns a collection of all <see cref="ModuleReference"/>s that are compliant with the specified <paramref name="predicate"/>.
	/// </summary>
	/// <param name="predicate">Function that determines whether to include a specific <see cref="ModuleReference"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
	public IEnumerable<ModuleReference> GetAllReferences(Predicate<ModuleReference> predicate)
	{
		if (predicate is null)
		{
			throw new ArgumentNullException(nameof(predicate));
		}

		List<ModuleReference> modules = new(Count);

		for (int i = 0; i < Count; i++)
		{
			ModuleReference reference = GetReferenceAt(i);

			if (predicate(reference))
			{
				modules.Add(reference);
			}
		}

		return modules;
	}

	/// <summary>
	/// Returns a collection of all <see cref="ModuleReference"/>s that point to the specified <paramref name="module"/>.
	/// </summary>
	/// <param name="module"><see cref="DurianModule"/> to get the <see cref="ModuleReference"/>s for.</param>
	public IEnumerable<ModuleReference> GetAllReferences(DurianModule module)
	{
		List<ModuleReference> modules = new(Count);

		for (int i = 0; i < Count; i++)
		{
			if (_enums[i] == module)
			{
				ModuleReference reference = GetReferenceAt(i);

				modules.Add(reference);
			}
		}

		return modules;
	}

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	public List<ModuleReference>.Enumerator GetEnumerator()
	{
		InitializeReferences();
		return _references.GetEnumerator()!;
	}

	/// <summary>
	/// Returns the first <see cref="ModuleIdentity"/> that represents the specified <paramref name="module"/>.
	/// </summary>
	/// <param name="module"><see cref="DurianModule"/> to get the <see cref="ModuleIdentity"/> for.</param>
	/// <exception cref="ArgumentException"><paramref name="module"/> not registered.</exception>
	public ModuleIdentity GetModule(DurianModule module)
	{
		if (!TryGetModule(module, out ModuleIdentity? identity))
		{
			throw new ArgumentException($"Module '{module}' not registered", nameof(module));
		}

		return identity;
	}

	/// <summary>
	/// Returns the first <see cref="ModuleIdentity"/> that is compliant with the specified <paramref name="predicate"/>.
	/// </summary>
	/// <param name="predicate">Function that determines whether to return a specific <see cref="ModuleReference"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException">No module satisfies the specified condition.</exception>
	public ModuleIdentity GetModule(Predicate<ModuleReference> predicate)
	{
		if (!TryGetModule(predicate, out ModuleIdentity? identity))
		{
			throw new ArgumentException("No module satisfies the specified condition", nameof(predicate));
		}

		return identity;
	}

	/// <summary>
	/// Returns the first <see cref="ModuleReference"/> that is compliant with the specified <paramref name="predicate"/>.
	/// </summary>
	/// <param name="predicate">Function that determines whether to return a specific <see cref="ModuleReference"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException">No module satisfies the specified condition.</exception>
	public ModuleReference GetReference(Predicate<ModuleReference> predicate)
	{
		if (!TryGetReference(predicate, out ModuleReference? reference))
		{
			throw new ArgumentException("No module satisfies the specified condition", nameof(predicate));
		}

		return reference;
	}

	/// <summary>
	/// Returns the first <see cref="ModuleReference"/> that references the specified <paramref name="module"/>.
	/// </summary>
	/// <param name="module"><see cref="DurianModule"/> to get the <see cref="ModuleIdentity"/> for.</param>
	/// <exception cref="ArgumentException"><paramref name="module"/> not registered.</exception>
	public ModuleReference GetReference(DurianModule module)
	{
		if (!TryGetReference(module, out ModuleReference? reference))
		{
			throw new ArgumentException($"Module '{module}' not registered", nameof(module));
		}

		return reference;
	}

	/// <summary>
	/// Includes the specified <paramref name="module"/> in the container.
	/// </summary>
	/// <param name="module"><see cref="DurianModule"/> to include.</param>
	/// <exception cref="ArgumentException"><paramref name="module"/> is not a valid <see cref="DurianModule"/> value.</exception>
	public void Include(DurianModule module)
	{
		if (!GlobalInfo.IsValidModuleValue(module))
		{
			throw new ArgumentException($"Value '{module}' is not a valid {nameof(DurianModule)} value!", nameof(module));
		}

		_enums.Add(module);
		_references.Add(null);
	}

	/// <summary>
	/// Includes the specified <paramref name="module"/> in the container.
	/// </summary>
	/// <param name="module"><see cref="ModuleIdentity"/> to include.</param>
	/// <exception cref="ArgumentNullException"><paramref name="module"/> is <see langword="null"/>.</exception>
	public void Include(ModuleIdentity module)
	{
		if (module is null)
		{
			throw new ArgumentNullException(nameof(module));
		}

		_enums.Add(module.Module);
		_references.Add(new ModuleReference(module));
	}

	/// <summary>
	/// Includes the specified <paramref name="reference"/> in the container.
	/// </summary>
	/// <param name="reference"><see cref="ModuleReference"/> to include.</param>
	/// <exception cref="ArgumentNullException"><paramref name="reference"/> is <see langword="null"/>.</exception>
	public void Include(ModuleReference reference)
	{
		if (reference is null)
		{
			throw new ArgumentNullException(nameof(reference));
		}

		_enums.Add(reference.EnumValue);
		_references.Add(reference);
	}

	/// <summary>
	/// Includes all the specified <paramref name="modules"/> in the container.
	/// </summary>
	/// <param name="modules">A collection of <see cref="DurianModule"/>s to include.</param>
	/// <exception cref="ArgumentNullException"><paramref name="modules"/> is <see langword="null"/>.</exception>
	/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected.</exception>
	public void IncludeAll(IEnumerable<DurianModule> modules)
	{
		if (modules is null)
		{
			throw new ArgumentNullException(nameof(modules));
		}

		foreach (DurianModule module in modules)
		{
			ModuleIdentity.EnsureIsValidModuleEnum_InvOp(module);

			_enums.Add(module);
			_references.Add(null);
		}
	}

	/// <summary>
	/// Includes all the specified <paramref name="modules"/> in the container.
	/// </summary>
	/// <param name="modules">A collection of <see cref="ModuleIdentity"/>s to include.</param>
	/// <remarks>Values equal to <see langword="null"/> are omitted.</remarks>
	/// <exception cref="ArgumentNullException"><paramref name="modules"/> is <see langword="null"/>.</exception>
	public void IncludeAll(IEnumerable<ModuleIdentity> modules)
	{
		if (modules is null)
		{
			throw new ArgumentNullException(nameof(modules));
		}

		foreach (ModuleIdentity module in modules)
		{
			if (module is null)
			{
				continue;
			}

			_enums.Add(module.Module);
			_references.Add(new ModuleReference(module));
		}
	}

	/// <summary>
	/// Includes all the specified <paramref name="references"/> in the container.
	/// </summary>
	/// <param name="references">A collection of <see cref="ModuleReference"/>s to include.</param>
	/// <remarks>Values equal to <see langword="null"/> are omitted.</remarks>
	/// <exception cref="ArgumentNullException"><paramref name="references"/> is <see langword="null"/>.</exception>
	public void IncludeAll(IEnumerable<ModuleReference> references)
	{
		if (references is null)
		{
			throw new ArgumentNullException(nameof(references));
		}

		foreach (ModuleReference reference in references)
		{
			if (reference is null)
			{
				continue;
			}

			_enums.Add(reference.EnumValue);
			_references.Add(reference);
		}
	}

	/// <summary>
	/// Removes first occurrence of the specified <paramref name="module"/> from the container.
	/// </summary>
	/// <param name="module"><see cref="DurianModule"/> to remove.</param>
	/// <returns><see langword="true"/> if the <paramref name="module"/> has been successfully removed, <see langword="false"/> otherwise.</returns>
	public bool Remove(DurianModule module)
	{
		int index = _enums.IndexOf(module);

		if (index == -1)
		{
			return false;
		}

		_enums.RemoveAt(index);
		_references.RemoveAt(index);

		return true;
	}

	/// <summary>
	/// Removes first occurrence of the specified <paramref name="module"/> from the container.
	/// </summary>
	/// <param name="module"><see cref="DurianModule"/> to remove.</param>
	/// <param name="reference">Reference to a module that was removed from the container.</param>
	/// <returns><see langword="true"/> if the <paramref name="module"/> has been successfully removed, <see langword="false"/> otherwise.</returns>
	public bool Remove(DurianModule module, [NotNullWhen(true)] out ModuleReference? reference)
	{
		int index = _enums.IndexOf(module);

		if (index == -1)
		{
			reference = null;
			return false;
		}

		ModuleReference? r = _references[index] ?? new ModuleReference(module);

		_enums.RemoveAt(index);
		_references.RemoveAt(index);

		reference = r;

		return true;
	}

	/// <summary>
	/// Removes first occurrence of the a Durian module that is compliant with the specified <paramref name="predicate"/>.
	/// </summary>
	/// <param name="predicate">Function that determines whether to remove the given Durian module.</param>
	/// <returns><see langword="true"/> if a module has been successfully removed, <see langword="false"/> otherwise.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
	public bool Remove(Predicate<ModuleReference> predicate)
	{
		if (predicate is null)
		{
			throw new ArgumentNullException(nameof(predicate));
		}

		for (int i = 0; i < Count; i++)
		{
			ModuleReference reference = GetReferenceAt(i);

			if (predicate(reference))
			{
				_enums.RemoveAt(i);
				_references.RemoveAt(i);

				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Removes all occurrences of the specified Durian <paramref name="module"/> from the container.
	/// </summary>
	/// <returns><see langword="true"/> if any <see cref="DurianModule"/>s has been successfully removed, <see langword="false"/> otherwise.</returns>
	/// <param name="module"><see cref="DurianModule"/> to remove all occurrences of.</param>
	public bool RemoveAll(DurianModule module)
	{
		List<int> duplicates = new(Count);

		for (int i = 0; i < Count; i++)
		{
			if (_enums[i] == module)
			{
				duplicates.Add(i);
			}
		}

		if (duplicates.Count == 0)
		{
			return false;
		}

		for (int i = duplicates.Count - 1; i > -1; i++)
		{
			int dup = duplicates[i];

			_references.RemoveAt(dup);
			_enums.RemoveAt(dup);
		}

		return true;
	}

	/// <summary>
	/// Removes all Durian modules that are compliant with the specified <paramref name="predicate"/> from the container.
	/// </summary>
	/// <returns><see langword="true"/> if any Durian modules has been successfully removed, <see langword="false"/> otherwise.</returns>
	/// <param name="predicate">Function that determines which Durian modules to remove.</param>
	/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
	public bool RemoveAll(Predicate<ModuleReference> predicate)
	{
		if (predicate is null)
		{
			throw new ArgumentNullException(nameof(predicate));
		}

		List<int> duplicates = new(Count);

		for (int i = 0; i < Count; i++)
		{
			ModuleReference reference = GetReferenceAt(i);

			if (predicate(reference))
			{
				duplicates.Add(i);
			}
		}

		if (duplicates.Count == 0)
		{
			return false;
		}

		for (int i = duplicates.Count - 1; i > -1; i++)
		{
			int dup = duplicates[i];

			_enums.RemoveAt(dup);
			_references.RemoveAt(dup);
		}

		return true;
	}

	/// <summary>
	/// Attempts to return <see cref="ModuleIdentity"/> that represents the specified <paramref name="module"/>.
	/// </summary>
	/// <returns><see langword="true"/> if an appropriate <see cref="ModuleIdentity"/> has been found, <see langword="false"/> otherwise.</returns>
	/// <param name="module"><see cref="DurianModule"/> to get the <see cref="ModuleIdentity"/> for.</param>
	/// <param name="identity"><see cref="ModuleIdentity"/> that represents the specified <paramref name="module"/>.</param>
	public bool TryGetModule(DurianModule module, [NotNullWhen(true)] out ModuleIdentity? identity)
	{
		for (int i = 0; i < Count; i++)
		{
			if (_enums[i] == module)
			{
				ModuleReference reference = GetReferenceAt(i);
				identity = reference.GetModule();
				return true;
			}
		}

		identity = default;
		return false;
	}

	/// <summary>
	/// Attempts to return <see cref="ModuleIdentity"/> that is compliant with the specified <paramref name="predicate"/>.
	/// </summary>
	/// <param name="predicate">Function that determines whether to return a specific <see cref="ModuleReference"/>.</param>
	/// <param name="identity"><see cref="ModuleIdentity"/> that is compliant with the specified <paramref name="predicate"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
	public bool TryGetModule(Predicate<ModuleReference> predicate, [NotNullWhen(true)] out ModuleIdentity? identity)
	{
		if (predicate is null)
		{
			throw new ArgumentNullException(nameof(predicate));
		}

		for (int i = 0; i < Count; i++)
		{
			ModuleReference reference = GetReferenceAt(i);

			if (predicate(reference))
			{
				identity = reference.GetModule();
				return true;
			}
		}

		identity = default;
		return false;
	}

	/// <summary>
	/// Attempts to return <see cref="ModuleReference"/> that refers to the specified <paramref name="module"/>.
	/// </summary>
	/// <returns><see langword="true"/> if an appropriate <see cref="ModuleReference"/> has been found, <see langword="false"/> otherwise.</returns>
	/// <param name="module"><see cref="DurianModule"/> to get the <see cref="ModuleReference"/> for.</param>
	/// <param name="reference"><see cref="ModuleReference"/> that represents the specified <paramref name="module"/>.</param>
	public bool TryGetReference(DurianModule module, [NotNullWhen(true)] out ModuleReference? reference)
	{
		for (int i = 0; i < Count; i++)
		{
			if (_enums[i] == module)
			{
				reference = GetReferenceAt(i);
				return true;
			}
		}

		reference = default;
		return false;
	}

	/// <summary>
	/// Attempts to return <see cref="ModuleReference"/> that is compliant with the specified <paramref name="predicate"/>.
	/// </summary>
	/// <param name="predicate">Function that determines whether to return a specific <see cref="ModuleReference"/>.</param>
	/// <param name="reference"><see cref="ModuleReference"/> that is compliant with the specified <paramref name="predicate"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
	public bool TryGetReference(Predicate<ModuleReference> predicate, [NotNullWhen(true)] out ModuleReference? reference)
	{
		if (predicate is null)
		{
			throw new ArgumentNullException(nameof(predicate));
		}

		for (int i = 0; i < Count; i++)
		{
			ModuleReference target = GetReferenceAt(i);

			if (predicate(target))
			{
				reference = target;
				return true;
			}
		}

		reference = default;
		return false;
	}

	/// <summary>
	/// Includes the specified <paramref name="reference"/> in the container unless the <see cref="DurianModule"/> it is referring to is already included.
	/// </summary>
	/// <param name="reference"><see cref="ModuleReference"/> to include.</param>
	/// <exception cref="ArgumentNullException"><paramref name="reference"/> is <see langword="null"/>.</exception>
	public bool TryInclude(ModuleReference reference)
	{
		if (reference is null)
		{
			throw new ArgumentNullException(nameof(reference));
		}

		if (!Contains(reference.EnumValue))
		{
			_enums.Add(reference.EnumValue);
			_references.Add(reference);
			return true;
		}

		return false;
	}

	/// <summary>
	/// Includes the specified <paramref name="module"/> in the container unless the <see cref="DurianModule"/> it represents is already included.
	/// </summary>
	/// <param name="module"><see cref="ModuleIdentity"/> to include.</param>
	/// <exception cref="ArgumentNullException"><paramref name="module"/> is <see langword="null"/>.</exception>
	public bool TryInclude(ModuleIdentity module)
	{
		if (module is null)
		{
			throw new ArgumentNullException(nameof(module));
		}

		if (!Contains(module.Module))
		{
			_enums.Add(module.Module);
			_references.Add(new ModuleReference(module));
			return true;
		}

		return false;
	}

	/// <summary>
	/// Includes the specified <paramref name="module"/> in the container unless it is already included.
	/// </summary>
	/// <param name="module"><see cref="DurianModule"/> to include.</param>
	/// <exception cref="ArgumentException"><paramref name="module"/> is not a valid <see cref="DurianModule"/> value.</exception>
	public bool TryInclude(DurianModule module)
	{
		if (!Contains(module))
		{
			Include(module);
			return true;
		}

		return false;
	}

	void ICollection<ModuleReference>.Add(ModuleReference item)
	{
		Include(item);
	}

	IEnumerable<int> IDurianContainer.AsEnums()
	{
		DurianModule[] modules = AsEnums();
		int[] ints = new int[modules.Length];
		modules.CopyTo(ints, 0);
		return ints;
	}

	IEnumerable<IDurianIdentity> IDurianContainer.AsIdentities()
	{
		return AsIdentities();
	}

	IEnumerable<IDurianReference> IDurianContainer.AsReferences()
	{
		return AsReferences();
	}

	IDurianContainer IDurianContainer.Clone(bool sharedReference)
	{
		return Clone(sharedReference);
	}

	object ICloneable.Clone()
	{
		return Clone(false);
	}

	bool ICollection<ModuleReference>.Contains(ModuleReference item)
	{
		return Contains(item.EnumValue);
	}

	void ICollection<ModuleReference>.CopyTo(ModuleReference[] array, int arrayIndex)
	{
		InitializeReferences();
		_references.CopyTo(array, arrayIndex);
	}

	bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
	{
		if (other is null)
		{
			return false;
		}

		if (ReferenceEquals(this, other))
		{
			return true;
		}

		if (other is not ModuleContainer c || c.Count != Count)
		{
			return false;
		}

		List<DurianModule> enums = c._enums;

		for (int i = 0; i < Count; i++)
		{
			if (enums[i] != _enums[i])
			{
				return false;
			}
		}

		return true;
	}

	IEnumerator<ModuleReference> IEnumerable<ModuleReference>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
	{
		if (comparer is null)
		{
			throw new ArgumentNullException(nameof(comparer));
		}

		int hashCode = -507560740;

		foreach (DurianModule module in _enums)
		{
			hashCode = (hashCode * -1521134295) + module.GetHashCode();
		}

		return hashCode;
	}

	bool ICollection<ModuleReference>.Remove(ModuleReference item)
	{
		return Remove(item.EnumValue);
	}

	private ModuleReference GetReferenceAt(int index)
	{
		ModuleReference? reference = _references[index];

		if (reference is null)
		{
			reference = new ModuleReference(_enums[index]);
			_references[index] = reference;
		}

		return reference;
	}

	private void InitializeReferences()
	{
		for (int i = 0; i < Count; i++)
		{
			if (_references[i] is null)
			{
				_references[i] = new ModuleReference(_enums[i]);
			}
		}
	}
}
