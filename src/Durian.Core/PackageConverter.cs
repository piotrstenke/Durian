using System;
using System.Collections.Generic;
using System.Linq;

namespace Durian.Info;

/// <summary>
/// Contains conversion methods for collections of <see cref="PackageReference"/>s, <see cref="PackageIdentity"/>s and <see cref="DurianPackage"/>s.
/// </summary>
public static class PackageConverter
{
	/// <summary>
	/// Converts a collection of <see cref="PackageReference"/>s into an array of <see cref="DurianPackage"/>s.
	/// </summary>
	/// <param name="references">A collection of <see cref="PackageReference"/>s to convert.</param>
	/// <exception cref="ArgumentNullException"><paramref name="references"/> is <see langword="null"/>.</exception>
	/// <exception cref="InvalidOperationException">Null <see cref="PackageReference"/> detected.</exception>
	public static DurianPackage[] ToEnums(IEnumerable<PackageReference> references)
	{
		if (references is null)
		{
			throw new ArgumentNullException(nameof(references));
		}

		PackageReference[] array = references.ToArray();

		if (array.Length == 0)
		{
			return Array.Empty<DurianPackage>();
		}

		DurianPackage[] enums = new DurianPackage[array.Length];

		for (int i = 0; i < enums.Length; i++)
		{
			PackageReference? reference = array[i];

			if (reference is null)
			{
				throw new InvalidOperationException($"Null package reference at index: {i}");
			}

			enums[i] = reference.EnumValue;
		}

		return enums;
	}

	/// <summary>
	/// Converts a collection of <see cref="PackageIdentity"/>s into an array of <see cref="DurianPackage"/>s.
	/// </summary>
	/// <param name="packages">A collection of <see cref="PackageIdentity"/>s to convert.</param>
	/// <exception cref="ArgumentNullException"><paramref name="packages"/> is <see langword="null"/>.</exception>
	/// <exception cref="InvalidOperationException">Null <see cref="PackageIdentity"/> detected.</exception>
	public static DurianPackage[] ToEnums(IEnumerable<PackageIdentity> packages)
	{
		if (packages is null)
		{
			throw new ArgumentNullException(nameof(packages));
		}

		PackageIdentity[] array = packages.ToArray();

		if (array.Length == 0)
		{
			return Array.Empty<DurianPackage>();
		}

		DurianPackage[] enums = new DurianPackage[array.Length];

		for (int i = 0; i < enums.Length; i++)
		{
			PackageIdentity? identity = array[i];

			if (identity is null)
			{
				throw new InvalidOperationException($"Null package identity at index: {i}");
			}

			enums[i] = identity.EnumValue;
		}

		return enums;
	}

	/// <summary>
	/// Converts a collection of <see cref="DurianPackage"/>s into an array of <see cref="PackageIdentity"/>s.
	/// </summary>
	/// <param name="packages">A collection of <see cref="DurianPackage"/>s to convert.</param>
	/// <exception cref="ArgumentNullException"><paramref name="packages"/> is <see langword="null"/>.</exception>
	/// <exception cref="InvalidOperationException">Unknown <see cref="DurianPackage"/> value detected.</exception>
	public static PackageIdentity[] ToIdentities(IEnumerable<DurianPackage> packages)
	{
		if (packages is null)
		{
			throw new ArgumentNullException(nameof(packages));
		}

		DurianPackage[] array = packages.ToArray();

		if (array.Length == 0)
		{
			return Array.Empty<PackageIdentity>();
		}

		PackageIdentity[] identities = new PackageIdentity[array.Length];

		for (int i = 0; i < identities.Length; i++)
		{
			identities[i] = PackageIdentity.GetPackage(array[i]);
		}

		return identities;
	}

	/// <summary>
	/// Converts a collection of <see cref="PackageReference"/>s into an array of <see cref="PackageIdentity"/>s.
	/// </summary>
	/// <param name="references">A collection of <see cref="PackageReference"/>s to convert.</param>
	/// <exception cref="ArgumentNullException"><paramref name="references"/> is <see langword="null"/>.</exception>
	/// <exception cref="InvalidOperationException">Null <see cref="PackageReference"/> detected.</exception>
	public static PackageIdentity[] ToIdentities(IEnumerable<PackageReference> references)
	{
		if (references is null)
		{
			throw new ArgumentNullException(nameof(references));
		}

		PackageReference[] array = references.ToArray();

		if (array.Length == 0)
		{
			return Array.Empty<PackageIdentity>();
		}

		PackageIdentity[] identities = new PackageIdentity[array.Length];

		for (int i = 0; i < identities.Length; i++)
		{
			PackageReference? reference = array[i];

			if (reference is null)
			{
				throw new InvalidOperationException($"Null package reference at index: {i}");
			}

			identities[i] = reference.GetPackage();
		}

		return identities;
	}

	/// <summary>
	/// Converts a collection of <see cref="PackageIdentity"/>s into an array of <see cref="PackageReference"/>s.
	/// </summary>
	/// <param name="packages">A collection of <see cref="PackageIdentity"/>s to convert.</param>
	/// <exception cref="ArgumentNullException"><paramref name="packages"/> is <see langword="null"/>.</exception>
	/// <exception cref="InvalidOperationException">Null <see cref="PackageIdentity"/> detected.</exception>
	public static PackageReference[] ToReferences(IEnumerable<PackageIdentity> packages)
	{
		if (packages is null)
		{
			throw new ArgumentNullException(nameof(packages));
		}

		PackageIdentity[] array = packages.ToArray();

		if (array.Length == 0)
		{
			return Array.Empty<PackageReference>();
		}

		PackageReference[] references = new PackageReference[array.Length];

		for (int i = 0; i < references.Length; i++)
		{
			PackageIdentity? identity = array[i];

			if (identity is null)
			{
				throw new InvalidOperationException($"Null package identity at index: {i}");
			}

			references[i] = new PackageReference(identity);
		}

		return references;
	}

	/// <summary>
	/// Converts a collection of <see cref="DurianPackage"/>s into an array of <see cref="PackageReference"/>s.
	/// </summary>
	/// <param name="packages">A collection of <see cref="DurianPackage"/>s to convert.</param>
	/// <exception cref="ArgumentNullException"><paramref name="packages"/> is <see langword="null"/>.</exception>
	/// <exception cref="InvalidOperationException">Unknown <see cref="DurianPackage"/> value detected.</exception>
	public static PackageReference[] ToReferences(IEnumerable<DurianPackage> packages)
	{
		if (packages is null)
		{
			throw new ArgumentNullException(nameof(packages));
		}

		DurianPackage[] array = packages.ToArray();

		if (array.Length == 0)
		{
			return Array.Empty<PackageReference>();
		}

		PackageReference[] references = new PackageReference[array.Length];

		for (int i = 0; i < references.Length; i++)
		{
			references[i] = new PackageReference(array[i]);
		}

		return references;
	}
}
