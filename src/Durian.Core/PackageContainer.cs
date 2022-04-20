// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Durian.Info
{
	/// <summary>
	/// A collection of Durian packages represented as either <see cref="PackageReference"/>, direct <see cref="PackageIdentity"/> or a value of <see cref="DurianPackage"/>.
	/// </summary>
	/// <remarks>This class implements the <see cref="IEquatable{T}"/> interface - two instances are compared by their values, not references.</remarks>
	[DebuggerDisplay("Count = {_references?.Count ?? 0}, nq")]
	public sealed class PackageContainer : IDurianContainer, ICollection<PackageReference>, ICloneable
	{
		// Both lists must have the same length.

		private readonly List<DurianPackage> _enums;
		private readonly List<PackageReference?> _references;

		/// <summary>
		/// Number of referenced Durian packages.
		/// </summary>
		public int Count => _references.Count;

		/// <summary>
		/// Determines whether the current <see cref="PackageContainer"/> does not contain any Durian packages.
		/// </summary>
		public bool IsEmpty => Count == 0;

		bool ICollection<PackageReference>.IsReadOnly => false;

		/// <summary>
		/// Initializes a new instance of the <see cref="PackageContainer"/> class.
		/// </summary>
		public PackageContainer()
		{
			_references = new List<PackageReference?>();
			_enums = new List<DurianPackage>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PackageContainer"/> class.
		/// </summary>
		/// <param name="references">A collection of <see cref="PackageReference"/>s that the current instance should include.</param>
		/// <remarks>Values equal to <see langword="null"/> are omitted.</remarks>
		/// <exception cref="ArgumentNullException"><paramref name="references"/> is <see langword="null"/>.</exception>
		public PackageContainer(IEnumerable<PackageReference> references)
		{
			if (references is null)
			{
				throw new ArgumentNullException(nameof(references));
			}

			PackageReference[] packages = references.ToArray();

			_references = new List<PackageReference?>(packages.Length);
			_enums = new List<DurianPackage>(packages.Length);

			foreach (PackageReference r in packages)
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
		/// Initializes a new instance of the <see cref="PackageContainer"/> class.
		/// </summary>
		/// <param name="packages">A collection of <see cref="PackageIdentity"/>s that the current instance should include.</param>
		/// <remarks>Values equal to <see langword="null"/> are omitted.</remarks>
		/// <exception cref="ArgumentNullException"><paramref name="packages"/> is <see langword="null"/>.</exception>
		public PackageContainer(IEnumerable<PackageIdentity> packages)
		{
			if (packages is null)
			{
				throw new ArgumentNullException(nameof(packages));
			}

			PackageIdentity[] identities = packages.ToArray();

			_references = new List<PackageReference?>(identities.Length);
			_enums = new List<DurianPackage>(identities.Length);

			foreach (PackageIdentity p in identities)
			{
				if (p is null)
				{
					continue;
				}

				_references.Add(new PackageReference(p));
				_enums.Add(p.EnumValue);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PackageContainer"/> class.
		/// </summary>
		/// <param name="packages">A collection of <see cref="DurianPackage"/>s that the current instance should include.</param>
		/// <exception cref="ArgumentNullException"><paramref name="packages"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianPackage"/> value detected.</exception>
		public PackageContainer(IEnumerable<DurianPackage> packages)
		{
			if (packages is null)
			{
				throw new ArgumentNullException(nameof(packages));
			}

			List<DurianPackage> enums = packages.ToList();

			foreach (DurianPackage p in packages)
			{
				PackageIdentity.EnsureIsValidPackageEnum_InvOp(p);
			}

			_enums = enums;
			_references = new List<PackageReference?>(_enums.Count);

			for (int i = 0; i < _enums.Count; i++)
			{
				_references.Add(null);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PackageContainer"/> class.
		/// </summary>
		/// <param name="capacity">Initial number of packages the current instance can store without resizing of a internal container.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than <c>0</c>.</exception>
		public PackageContainer(int capacity)
		{
			_references = new List<PackageReference?>(capacity);
			_enums = new List<DurianPackage>(capacity);
		}

		internal PackageContainer(List<DurianPackage> enums)
		{
			_enums = enums;
			_references = new List<PackageReference?>(_enums.Count);

			for (int i = 0; i < _enums.Count; i++)
			{
				_references.Add(null);
			}
		}

		private PackageContainer(List<DurianPackage> enums, List<PackageReference?> references)
		{
			_enums = enums;
			_references = references;
		}

		void ICollection<PackageReference>.Add(PackageReference item)
		{
			Include(item);
		}

		/// <summary>
		/// Returns all elements in the container as values of <see cref="DurianPackage"/>.
		/// </summary>
		public DurianPackage[] AsEnums()
		{
			if (Count == 0)
			{
				return Array.Empty<DurianPackage>();
			}

			DurianPackage[] array = new DurianPackage[Count];

			for (int i = 0; i < array.Length; i++)
			{
				array[i] = _enums[i];
			}

			return array;
		}

		IEnumerable<int> IDurianContainer.AsEnums()
		{
			DurianPackage[] packages = AsEnums();
			int[] ints = new int[packages.Length];
			packages.CopyTo(ints, 0);
			return ints;
		}

		/// <summary>
		/// Returns all elements in the container as <see cref="PackageIdentity"/>s.
		/// </summary>
		public PackageIdentity[] AsIdentities()
		{
			if (Count == 0)
			{
				return Array.Empty<PackageIdentity>();
			}

			PackageIdentity[] array = new PackageIdentity[Count];

			for (int i = 0; i < array.Length; i++)
			{
				PackageReference reference = GetReference(i);

				array[i] = reference.GetPackage();
			}

			return array;
		}

		IEnumerable<IDurianIdentity> IDurianContainer.AsIdentities()
		{
			return AsIdentities();
		}

		/// <summary>
		/// Returns all elements in the container as <see cref="PackageReference"/>s.
		/// </summary>
		public PackageReference[] AsReferences()
		{
			if (Count == 0)
			{
				return Array.Empty<PackageReference>();
			}

			PackageReference[] array = new PackageReference[Count];

			for (int i = 0; i < array.Length; i++)
			{
				array[i] = GetReference(i);
			}

			return array;
		}

		IEnumerable<IDurianReference> IDurianContainer.AsReferences()
		{
			return AsReferences();
		}

		/// <summary>
		/// Removes all referenced Durian packages from the container.
		/// </summary>
		public void Clear()
		{
			_references.Clear();
		}

		/// <summary>
		/// Creates a new <see cref="PackageContainer"/> that is a copy of the current instance.
		/// </summary>
		/// <param name="sharedReference">Determines whether to share internal list of packages between both instances.</param>
		public PackageContainer Clone(bool sharedReference = true)
		{
			List<DurianPackage> enums;
			List<PackageReference?> references;

			if (sharedReference)
			{
				references = _references;
				enums = _enums;
			}
			else
			{
				references = new List<PackageReference?>(Count);
				enums = new List<DurianPackage>(Count);

				references.AddRange(_references);
			}

			return new PackageContainer(enums, references);
		}

		IDurianContainer IDurianContainer.Clone(bool sharedReference)
		{
			return Clone(sharedReference);
		}

		object ICloneable.Clone()
		{
			return Clone(false);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="package"/> is included withing this instance.
		/// </summary>
		/// <param name="package"><see cref="DurianPackage"/> to check.</param>
		public bool Contains(DurianPackage package)
		{
			return _enums.Contains(package);
		}

		/// <summary>
		/// Determines whether any Durian package in this container is compliant with the specified <paramref name="predicate"/>.
		/// </summary>
		/// <param name="predicate">Function that picks <see cref="PackageReference"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
		public bool Contains(Predicate<PackageReference> predicate)
		{
			if (predicate is null)
			{
				throw new ArgumentNullException(nameof(predicate));
			}

			for (int i = 0; i < Count; i++)
			{
				PackageReference reference = GetReference(i);

				if (predicate(reference))
				{
					return true;
				}
			}

			return false;
		}

		bool ICollection<PackageReference>.Contains(PackageReference item)
		{
			return Contains(item.EnumValue);
		}

		void ICollection<PackageReference>.CopyTo(PackageReference[] array, int arrayIndex)
		{
			InitializeReferences();
			_references.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Removes all duplicate package entries from the container.
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

			if (other is not PackageContainer c || c.Count != Count)
			{
				return false;
			}

			List<DurianPackage> enums = c._enums;

			for (int i = 0; i < Count; i++)
			{
				if (enums[i] != _enums[i])
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Returns a collection of all <see cref="PackageIdentity"/>s that are compliant with the specified <paramref name="predicate"/>.
		/// </summary>
		/// <param name="predicate">Function that determines whether to include a specific <see cref="PackageIdentity"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
		public IEnumerable<PackageIdentity> GetAllPackages(Predicate<PackageReference> predicate)
		{
			if (predicate is null)
			{
				throw new ArgumentNullException(nameof(predicate));
			}

			List<PackageIdentity> packages = new(Count);

			for (int i = 0; i < Count; i++)
			{
				PackageReference reference = GetReference(i);

				if (predicate(reference))
				{
					packages.Add(reference.GetPackage());
				}
			}

			return packages;
		}

		/// <summary>
		/// Returns a collection of all <see cref="PackageIdentity"/>s that represent the specified <paramref name="package"/>.
		/// </summary>
		/// <param name="package"><see cref="DurianPackage"/> to get the <see cref="PackageIdentity"/>s for.</param>
		public IEnumerable<PackageIdentity> GetAllPackages(DurianPackage package)
		{
			List<PackageIdentity> packages = new(Count);

			for (int i = 0; i < Count; i++)
			{
				if (_enums[i] == package)
				{
					PackageReference reference = GetReference(i);

					packages.Add(reference.GetPackage());
				}
			}

			return packages;
		}

		/// <summary>
		/// Returns a collection of all <see cref="PackageReference"/>s that are compliant with the specified <paramref name="predicate"/>.
		/// </summary>
		/// <param name="predicate">Function that determines whether to include a specific <see cref="PackageReference"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
		public IEnumerable<PackageReference> GetAllReferences(Predicate<PackageReference> predicate)
		{
			if (predicate is null)
			{
				throw new ArgumentNullException(nameof(predicate));
			}

			List<PackageReference> packages = new(Count);

			for (int i = 0; i < Count; i++)
			{
				PackageReference reference = GetReference(i);

				if (predicate(reference))
				{
					packages.Add(reference);
				}
			}

			return packages;
		}

		/// <summary>
		/// Returns a collection of all <see cref="PackageReference"/>s that point to the specified <paramref name="package"/>.
		/// </summary>
		/// <param name="package"><see cref="DurianPackage"/> to get the <see cref="PackageReference"/>s for.</param>
		public IEnumerable<PackageReference> GetAllReferences(DurianPackage package)
		{
			List<PackageReference> packages = new(Count);

			for (int i = 0; i < Count; i++)
			{
				if (_enums[i] == package)
				{
					PackageReference reference = GetReference(i);

					packages.Add(reference);
				}
			}

			return packages;
		}

		/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
		public List<PackageReference>.Enumerator GetEnumerator()
		{
			InitializeReferences();
			return _references.GetEnumerator()!;
		}

		IEnumerator<PackageReference> IEnumerable<PackageReference>.GetEnumerator()
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

			foreach (DurianPackage package in _enums)
			{
				hashCode = (hashCode * -1521134295) + package.GetHashCode();
			}

			return hashCode;
		}

		/// <summary>
		/// Returns the first <see cref="PackageIdentity"/> that represents the specified <paramref name="package"/>.
		/// </summary>
		/// <param name="package"><see cref="DurianPackage"/> to get the <see cref="PackageIdentity"/> for.</param>
		/// <exception cref="ArgumentException"><paramref name="package"/> not registered.</exception>
		public PackageIdentity? GetPackage(DurianPackage package)
		{
			if (!TryGetPackage(package, out PackageIdentity? identity))
			{
				throw new ArgumentException($"Package '{package}' not registered", nameof(package));
			}

			return identity;
		}

		/// <summary>
		/// Returns the first <see cref="PackageIdentity"/> that is compliant with the specified <paramref name="predicate"/>.
		/// </summary>
		/// <param name="predicate">Function that determines whether to return a specific <see cref="PackageReference"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">No package satisfies the specified condition.</exception>
		public PackageIdentity? GetPackage(Predicate<PackageReference> predicate)
		{
			if (!TryGetPackage(predicate, out PackageIdentity? identity))
			{
				throw new ArgumentException("No package satisfies the specified condition", nameof(predicate));
			}

			return identity;
		}

		/// <summary>
		/// Returns the first <see cref="PackageReference"/> that is compliant with the specified <paramref name="predicate"/>.
		/// </summary>
		/// <param name="predicate">Function that determines whether to return a specific <see cref="PackageReference"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">No package satisfies the specified condition.</exception>
		public PackageReference? GetReference(Predicate<PackageReference> predicate)
		{
			if (!TryGetReference(predicate, out PackageReference? reference))
			{
				throw new ArgumentException("No package satisfies the specified condition", nameof(predicate));
			}

			return reference;
		}

		/// <summary>
		/// Returns the first <see cref="PackageReference"/> that references the specified <paramref name="package"/>.
		/// </summary>
		/// <param name="package"><see cref="DurianPackage"/> to get the <see cref="PackageIdentity"/> for.</param>
		/// <exception cref="ArgumentException"><paramref name="package"/> not registered.</exception>
		public PackageReference? GetReference(DurianPackage package)
		{
			if (!TryGetReference(package, out PackageReference? reference))
			{
				throw new ArgumentException($"Package '{package}' not registered", nameof(package));
			}

			return reference;
		}

		/// <summary>
		/// Includes the specified <paramref name="package"/> in the container.
		/// </summary>
		/// <param name="package"><see cref="DurianPackage"/> to include.</param>
		/// <exception cref="ArgumentException"><paramref name="package"/> is not a valid <see cref="DurianPackage"/> value.</exception>
		public void Include(DurianPackage package)
		{
			if (!GlobalInfo.IsValidPackageValue(package))
			{
				throw new ArgumentException($"Value '{package}' is not a valid {nameof(DurianPackage)} value!", nameof(package));
			}

			_enums.Add(package);
			_references.Add(null);
		}

		/// <summary>
		/// Includes the specified <paramref name="package"/> in the container.
		/// </summary>
		/// <param name="package"><see cref="PackageIdentity"/> to include.</param>
		/// <exception cref="ArgumentNullException"><paramref name="package"/> is <see langword="null"/>.</exception>
		public void Include(PackageIdentity package)
		{
			if (package is null)
			{
				throw new ArgumentNullException(nameof(package));
			}

			_enums.Add(package.EnumValue);
			_references.Add(new PackageReference(package));
		}

		/// <summary>
		/// Includes the specified <paramref name="reference"/> in the container.
		/// </summary>
		/// <param name="reference"><see cref="PackageReference"/> to include.</param>
		/// <exception cref="ArgumentNullException"><paramref name="reference"/> is <see langword="null"/>.</exception>
		public void Include(PackageReference reference)
		{
			if (reference is null)
			{
				throw new ArgumentNullException(nameof(reference));
			}

			_enums.Add(reference.EnumValue);
			_references.Add(reference);
		}

		/// <summary>
		/// Includes all the specified <paramref name="packages"/> in the container.
		/// </summary>
		/// <param name="packages">A collection of <see cref="DurianPackage"/>s to include.</param>
		/// <exception cref="ArgumentNullException"><paramref name="packages"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianPackage"/> value detected.</exception>
		public void IncludeAll(IEnumerable<DurianPackage> packages)
		{
			if (packages is null)
			{
				throw new ArgumentNullException(nameof(packages));
			}

			foreach (DurianPackage package in packages)
			{
				PackageIdentity.EnsureIsValidPackageEnum_InvOp(package);

				_enums.Add(package);
				_references.Add(null);
			}
		}

		/// <summary>
		/// Includes all the specified <paramref name="packages"/> in the container.
		/// </summary>
		/// <param name="packages">A collection of <see cref="PackageIdentity"/>s to include.</param>
		/// <remarks>Values equal to <see langword="null"/> are omitted.</remarks>
		/// <exception cref="ArgumentNullException"><paramref name="packages"/> is <see langword="null"/>.</exception>
		public void IncludeAll(IEnumerable<PackageIdentity> packages)
		{
			if (packages is null)
			{
				throw new ArgumentNullException(nameof(packages));
			}

			foreach (PackageIdentity package in packages)
			{
				if (package is null)
				{
					continue;
				}

				_enums.Add(package.EnumValue);
				_references.Add(new PackageReference(package));
			}
		}

		/// <summary>
		/// Includes all the specified <paramref name="references"/> in the container.
		/// </summary>
		/// <param name="references">A collection of <see cref="PackageReference"/>s to include.</param>
		/// <remarks>Values equal to <see langword="null"/> are omitted.</remarks>
		/// <exception cref="ArgumentNullException"><paramref name="references"/> is <see langword="null"/>.</exception>
		public void IncludeAll(IEnumerable<PackageReference> references)
		{
			if (references is null)
			{
				throw new ArgumentNullException(nameof(references));
			}

			foreach (PackageReference reference in references)
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
		/// Removes first occurrence of the specified <paramref name="package"/> from the container.
		/// </summary>
		/// <param name="package"><see cref="DurianPackage"/> to remove.</param>
		/// <returns><see langword="true"/> if the <paramref name="package"/> has been successfully removed, <see langword="false"/> otherwise.</returns>
		public bool Remove(DurianPackage package)
		{
			int index = _enums.IndexOf(package);

			if (index == -1)
			{
				return false;
			}

			_enums.RemoveAt(index);
			_references.RemoveAt(index);

			return true;
		}

		/// <summary>
		/// Removes first occurrence of the specified <paramref name="package"/> from the container.
		/// </summary>
		/// <param name="package"><see cref="DurianPackage"/> to remove.</param>
		/// <param name="reference">Reference to a package that was removed from the container.</param>
		/// <returns><see langword="true"/> if the <paramref name="package"/> has been successfully removed, <see langword="false"/> otherwise.</returns>
		public bool Remove(DurianPackage package, [NotNullWhen(true)] out PackageReference? reference)
		{
			int index = _enums.IndexOf(package);

			if (index == -1)
			{
				reference = null;
				return false;
			}

			PackageReference? r = _references[index] ?? new PackageReference(package);

			_enums.RemoveAt(index);
			_references.RemoveAt(index);

			reference = r;

			return true;
		}

		/// <summary>
		/// Removes first occurrence of the a Durian package that is compliant with the specified <paramref name="predicate"/>.
		/// </summary>
		/// <param name="predicate">Function that determines whether to remove the given Durian package.</param>
		/// <returns><see langword="true"/> if a package has been successfully removed, <see langword="false"/> otherwise.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
		public bool Remove(Predicate<PackageReference> predicate)
		{
			if (predicate is null)
			{
				throw new ArgumentNullException(nameof(predicate));
			}

			for (int i = 0; i < Count; i++)
			{
				PackageReference reference = GetReference(i);

				if (predicate(reference))
				{
					_enums.RemoveAt(i);
					_references.RemoveAt(i);

					return true;
				}
			}

			return false;
		}

		bool ICollection<PackageReference>.Remove(PackageReference item)
		{
			return Remove(item.EnumValue);
		}

		/// <summary>
		/// Removes all occurrences of the specified Durian <paramref name="package"/> from the container.
		/// </summary>
		/// <returns><see langword="true"/> if any <see cref="DurianPackage"/>s has been successfully removed, <see langword="false"/> otherwise.</returns>
		/// <param name="package"><see cref="DurianPackage"/> to remove all occurrences of.</param>
		public bool RemoveAll(DurianPackage package)
		{
			List<int> duplicates = new(Count);

			for (int i = 0; i < Count; i++)
			{
				if (_enums[i] == package)
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
		/// Removes all Durian packages that are compliant with the specified <paramref name="predicate"/> from the container.
		/// </summary>
		/// <returns><see langword="true"/> if any Durian packages has been successfully removed, <see langword="false"/> otherwise.</returns>
		/// <param name="predicate">Function that determines which Durian packages to remove.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
		public bool RemoveAll(Predicate<PackageReference> predicate)
		{
			if (predicate is null)
			{
				throw new ArgumentNullException(nameof(predicate));
			}

			List<int> duplicates = new(Count);

			for (int i = 0; i < Count; i++)
			{
				PackageReference reference = GetReference(i);

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
		/// Attempts to return <see cref="PackageIdentity"/> that represents the specified <paramref name="package"/>.
		/// </summary>
		/// <returns><see langword="true"/> if an appropriate <see cref="PackageIdentity"/> has been found, <see langword="false"/> otherwise.</returns>
		/// <param name="package"><see cref="DurianPackage"/> to get the <see cref="PackageIdentity"/> for.</param>
		/// <param name="identity"><see cref="PackageIdentity"/> that represents the specified <paramref name="package"/>.</param>
		public bool TryGetPackage(DurianPackage package, [NotNullWhen(true)] out PackageIdentity? identity)
		{
			for (int i = 0; i < Count; i++)
			{
				if (_enums[i] == package)
				{
					PackageReference reference = GetReference(i);
					identity = reference.GetPackage();
					return true;
				}
			}

			identity = default;
			return false;
		}

		/// <summary>
		/// Attempts to return <see cref="PackageIdentity"/> that is compliant with the specified <paramref name="predicate"/>.
		/// </summary>
		/// <param name="predicate">Function that determines whether to return a specific <see cref="PackageReference"/>.</param>
		/// <param name="identity"><see cref="PackageIdentity"/> that is compliant with the specified <paramref name="predicate"/>.</param>
		/// <returns><see langword="true"/> if an appropriate <see cref="PackageIdentity"/> has been found, <see langword="false"/> otherwise.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
		public bool TryGetPackage(Predicate<PackageReference> predicate, [NotNullWhen(true)] out PackageIdentity? identity)
		{
			if (predicate is null)
			{
				throw new ArgumentNullException(nameof(predicate));
			}

			for (int i = 0; i < Count; i++)
			{
				PackageReference target = GetReference(i);

				if (predicate(target))
				{
					identity = target.GetPackage();
					return true;
				}
			}

			identity = default;
			return false;
		}

		/// <summary>
		/// Attempts to return <see cref="PackageReference"/> that refers to the specified <paramref name="package"/>.
		/// </summary>
		/// <returns><see langword="true"/> if an appropriate <see cref="PackageReference"/> has been found, <see langword="false"/> otherwise.</returns>
		/// <param name="package"><see cref="DurianPackage"/> to get the <see cref="PackageReference"/> for.</param>
		/// <param name="reference"><see cref="PackageReference"/> that represents the specified <paramref name="package"/>.</param>
		public bool TryGetReference(DurianPackage package, [NotNullWhen(true)] out PackageReference? reference)
		{
			for (int i = 0; i < Count; i++)
			{
				if (_enums[i] == package)
				{
					reference = GetReference(i);
					return true;
				}
			}

			reference = default;
			return false;
		}

		/// <summary>
		/// Attempts to return <see cref="PackageReference"/> that is compliant with the specified <paramref name="predicate"/>.
		/// </summary>
		/// <param name="predicate">Function that determines whether to return a specific <see cref="PackageReference"/>.</param>
		/// <param name="reference"><see cref="PackageReference"/> that is compliant with the specified <paramref name="predicate"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
		public bool TryGetReference(Predicate<PackageReference> predicate, [NotNullWhen(true)] out PackageReference? reference)
		{
			if (predicate is null)
			{
				throw new ArgumentNullException(nameof(predicate));
			}

			for (int i = 0; i < Count; i++)
			{
				PackageReference target = GetReference(i);

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
		/// Includes the specified <paramref name="reference"/> in the container unless the <see cref="DurianPackage"/> it is referring to is already included.
		/// </summary>
		/// <param name="reference"><see cref="PackageReference"/> to include.</param>
		/// <exception cref="ArgumentNullException"><paramref name="reference"/> is <see langword="null"/>.</exception>
		public bool TryInclude(PackageReference reference)
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
		/// Includes the specified <paramref name="package"/> in the container unless the <see cref="DurianPackage"/> it represents is already included.
		/// </summary>
		/// <param name="package"><see cref="PackageIdentity"/> to include.</param>
		/// <exception cref="ArgumentNullException"><paramref name="package"/> is <see langword="null"/>.</exception>
		public bool TryInclude(PackageIdentity package)
		{
			if (package is null)
			{
				throw new ArgumentNullException(nameof(package));
			}

			if (!Contains(package.EnumValue))
			{
				_enums.Add(package.EnumValue);
				_references.Add(new PackageReference(package));
				return true;
			}

			return false;
		}

		/// <summary>
		/// Includes the specified <paramref name="package"/> in the container unless it is already included.
		/// </summary>
		/// <param name="package"><see cref="DurianPackage"/> to include.</param>
		/// <exception cref="ArgumentException"><paramref name="package"/> is not a valid <see cref="DurianPackage"/> value.</exception>
		public bool TryInclude(DurianPackage package)
		{
			if (!Contains(package))
			{
				Include(package);
				return true;
			}

			return false;
		}

		private PackageReference GetReference(int index)
		{
			PackageReference? reference = _references[index];

			if (reference is null)
			{
				reference = new PackageReference(_enums[index]);
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
					_references[i] = new PackageReference(_enums[i]);
				}
			}
		}
	}
}
