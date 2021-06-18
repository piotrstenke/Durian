// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Durian.Info.PackageIdentity;

namespace Durian.Analysis
{
	/// <summary>
	/// Utility class that contains static methods similar to those <see cref="PackageIdentity"/>, but with <see cref="CSharpCompilation"/> as arguments instead of <see cref="Assembly"/>.
	/// </summary>
	public static class PackageUtilities
	{
		/// <summary>
		/// Returns an array of <see cref="PackageIdentity"/> of all Durian packages that are not referenced by the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the not references Durian packages of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static PackageIdentity[] GetNotReferencedPackages(CSharpCompilation compilation)
		{
			return GetNotReferencedPackages(compilation, GetAllPackages());
		}

		/// <summary>
		/// Returns an array of <see cref="PackageIdentity"/> of all Durian packages that are not referenced by the specified <paramref name="compilation"/>. Only <see cref="PackageIdentity"/> that are present in the given array of <paramref name="packages"/> are included.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the not references Durian packages of.</param>
		/// <param name="packages">Array of <see cref="PackageIdentity"/> to pick the not referenced packages from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static PackageIdentity[] GetNotReferencedPackages(CSharpCompilation compilation, PackageIdentity[]? packages)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (packages is null || packages.Length == 0)
			{
				return Array.Empty<PackageIdentity>();
			}

			PackageIdentity[] references = GetReferencedPackages(compilation, packages);

			if (packages.Length == references.Length)
			{
				return Array.Empty<PackageIdentity>();
			}

			if (references.Length == 0)
			{
				return packages;
			}

			PackageEnumEqualityComparer comparer = PackageEnumEqualityComparer.Instance;

			return packages
				.Except(references, comparer)
				.Distinct(comparer)
				.ToArray();
		}

		/// <summary>
		/// Returns an array of <see cref="PackageIdentity"/> of all Durian packages that are not referenced by the specified <paramref name="compilation"/>. Only <see cref="DurianPackage"/>s that are present in the given array of <paramref name="packages"/> are included.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the not references Durian packages of.</param>
		/// <param name="packages">Array of <see cref="DurianPackage"/>s to pick the not referenced packages from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianPackage"/> value detected.</exception>
		public static PackageIdentity[] GetNotReferencedPackages(CSharpCompilation compilation, DurianPackage[]? packages)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (packages is null || packages.Length == 0)
			{
				return Array.Empty<PackageIdentity>();
			}

			int length = packages.Length;
			PackageIdentity[] allPackages = new PackageIdentity[length];

			for (int i = 0; i < length; i++)
			{
				allPackages[i] = GetPackage(packages[i]);
			}

			PackageIdentity[] references = GetReferencedPackages(compilation, allPackages);

			if (packages.Length == references.Length)
			{
				return Array.Empty<PackageIdentity>();
			}

			if (references.Length == 0)
			{
				return allPackages;
			}

			PackageEnumEqualityComparer comparer = PackageEnumEqualityComparer.Instance;

			return allPackages
				.Except(references, comparer)
				.Distinct(comparer)
				.ToArray();
		}

		/// <summary>
		/// Returns an array of <see cref="DurianPackage"/>s of all Durian packages that are not referenced by the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the not references Durian packages of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static DurianPackage[] GetNotReferencedPackagesAsEnums(CSharpCompilation compilation)
		{
			return GetNotReferencedPackagesAsEnums(compilation, GetAllPackages());
		}

		/// <summary>
		/// Returns an array of <see cref="DurianPackage"/>s of all Durian packages that are not referenced by the specified <paramref name="compilation"/>. Only <see cref="PackageIdentity"/> that are present in the given array of <paramref name="packages"/> are included.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the not references Durian packages of.</param>
		/// <param name="packages">Array of <see cref="PackageIdentity"/> to pick the not referenced packages from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static DurianPackage[] GetNotReferencedPackagesAsEnums(CSharpCompilation compilation, PackageIdentity[]? packages)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (packages is null || packages.Length == 0)
			{
				return Array.Empty<DurianPackage>();
			}

			PackageIdentity[] references = GetReferencedPackages(compilation, packages);

			if (packages.Length == references.Length)
			{
				return Array.Empty<DurianPackage>();
			}

			PackageEnumEqualityComparer comparer = PackageEnumEqualityComparer.Instance;

			if (references.Length == 0)
			{
				return packages
					.Distinct(comparer)
					.Select(p => p.EnumValue)
					.ToArray();
			}

			return packages
				.Except(references, comparer)
				.Distinct(comparer)
				.Select(p => p.EnumValue)
				.ToArray();
		}

		/// <summary>
		/// Returns an array of <see cref="DurianPackage"/>s of all Durian packages that are not referenced by the specified <paramref name="compilation"/>. Only <see cref="DurianPackage"/>s that are present in the given array of <paramref name="packages"/> are included.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the not references Durian packages of.</param>
		/// <param name="packages">Array of <see cref="DurianPackage"/>s to pick the not referenced packages from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianPackage"/> value detected.</exception>
		public static DurianPackage[] GetNotReferencedPackagesAsEnums(CSharpCompilation compilation, DurianPackage[]? packages)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (packages is null || packages.Length == 0)
			{
				return Array.Empty<DurianPackage>();
			}

			int length = packages.Length;
			PackageIdentity[] allPackages = new PackageIdentity[length];

			for (int i = 0; i < length; i++)
			{
				allPackages[i] = GetPackage(packages[i]);
			}

			PackageIdentity[] references = GetReferencedPackages(compilation, allPackages);

			if (packages.Length == references.Length)
			{
				return Array.Empty<DurianPackage>();
			}

			PackageEnumEqualityComparer comparer = PackageEnumEqualityComparer.Instance;

			if (references.Length == 0)
			{
				return allPackages
					.Distinct(comparer)
					.Select(p => p.EnumValue)
					.ToArray();
			}

			return allPackages
				.Except(references, comparer)
				.Distinct(comparer)
				.Select(p => p.EnumValue)
				.ToArray();
		}

		/// <summary>
		/// Returns an array of <see cref="PackageIdentity"/> of all Durian packages referenced by the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the references Durian packages of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static PackageIdentity[] GetRefencedPackages(CSharpCompilation compilation)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			PackageIdentity[] allPackages = GetAllPackages();

			if (allPackages.Length == 0)
			{
				return allPackages;
			}

			List<PackageIdentity> referenced = new(allPackages.Length);

			foreach (PackageIdentity package in allPackages)
			{
				if (HasReference_Internal(package.Name, compilation))
				{
					referenced.Add(package);
				}
			}

			return referenced.ToArray();
		}

		/// <summary>
		/// Returns an array of <see cref="DurianPackage"/>s of all Durian packages referenced by the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the references Durian packages of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static DurianPackage[] GetRefencedPackagesAsEnums(CSharpCompilation compilation)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			DurianPackage[] allPackages = GetAllPackagesAsEnums();

			if (allPackages.Length == 0)
			{
				return allPackages;
			}

			List<DurianPackage> referenced = new(allPackages.Length);

			foreach (DurianPackage package in allPackages)
			{
				string packageName = PackageToString(package);

				if (HasReference_Internal(packageName, compilation))
				{
					referenced.Add(package);
				}
			}

			return referenced.ToArray();
		}

		/// <summary>
		/// Returns an array of <see cref="DurianPackage"/>s of all Durian packages referenced by the specified <paramref name="compilation"/>. Only <see cref="PackageIdentity"/> that are present in the given array of <paramref name="packages"/> are included.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the references Durian packages of.</param>
		/// <param name="packages">Array of <see cref="PackageIdentity"/> to pick the referenced packages from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static DurianPackage[] GetRefencedPackagesAsEnums(CSharpCompilation compilation, PackageIdentity[]? packages)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (packages is null || packages.Length == 0)
			{
				return Array.Empty<DurianPackage>();
			}

			HashSet<DurianPackage> set = new();

			foreach (PackageIdentity package in packages)
			{
				if (HasReference_Internal(package.Name, compilation))
				{
					set.Add(package.EnumValue);
				}
			}

			DurianPackage[] array = new DurianPackage[set.Count];
			set.CopyTo(array);
			return array;
		}

		/// <summary>
		/// Returns an array of <see cref="DurianPackage"/>s of all Durian packages referenced by the specified <paramref name="compilation"/>. Only <see cref="DurianPackage"/>s that are present in the given array of <paramref name="packages"/> are included.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the references Durian packages of.</param>
		/// <param name="packages">Array of <see cref="DurianPackage"/>s to pick the referenced packages from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianPackage"/> value detected.</exception>
		public static DurianPackage[] GetRefencedPackagesAsEnums(CSharpCompilation compilation, DurianPackage[]? packages)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (packages is null || packages.Length == 0)
			{
				return Array.Empty<DurianPackage>();
			}

			HashSet<DurianPackage> set = new();

			foreach (DurianPackage package in packages)
			{
				CheckIsValidPackageEnum(package);
				string packageName = PackageToString(package);

				if (HasReference_Internal(packageName, compilation))
				{
					set.Add(package);
				}
			}

			DurianPackage[] array = new DurianPackage[set.Count];
			set.CopyTo(array);
			return array;
		}

		/// <summary>
		/// Returns an array of <see cref="PackageIdentity"/> of all Durian packages referenced by the specified <paramref name="compilation"/>. Only <see cref="PackageIdentity"/> that are present in the given array of <paramref name="packages"/> are included.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the references Durian packages of.</param>
		/// <param name="packages">Array of <see cref="PackageIdentity"/> to pick the referenced packages from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static PackageIdentity[] GetReferencedPackages(CSharpCompilation compilation, PackageIdentity[]? packages)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (packages is null || packages.Length == 0)
			{
				return Array.Empty<PackageIdentity>();
			}

			HashSet<PackageIdentity> set = new(PackageEnumEqualityComparer.Instance);

			foreach (PackageIdentity package in packages)
			{
				if (HasReference_Internal(package.Name, compilation))
				{
					set.Add(package);
				}
			}

			PackageIdentity[] array = new PackageIdentity[set.Count];
			set.CopyTo(array);
			return array;
		}

		/// <summary>
		/// Returns an array of <see cref="PackageIdentity"/> of all Durian packages referenced by the specified <paramref name="compilation"/>. Only <see cref="DurianPackage"/>s that are present in the given array of <paramref name="packages"/> are included.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the references Durian packages of.</param>
		/// <param name="packages">Array of <see cref="DurianPackage"/>s to pick the referenced packages from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianPackage"/> value detected.</exception>
		public static PackageIdentity[] GetReferencedPackages(CSharpCompilation compilation, DurianPackage[]? packages)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (packages is null || packages.Length == 0)
			{
				return Array.Empty<PackageIdentity>();
			}

			HashSet<DurianPackage> set = new();
			List<PackageIdentity> list = new(packages.Length);

			foreach (DurianPackage package in packages)
			{
				CheckIsValidPackageEnum(package);
				string packageName = PackageToString(package);

				if (HasReference_Internal(packageName, compilation) && set.Add(package))
				{
					list.Add(GetPackage(package));
				}
			}

			return list.ToArray();
		}

		/// <summary>
		/// Checks, if the specified <paramref name="compilation"/> references the specified Durian <paramref name="package"/>.
		/// </summary>
		/// <param name="package"><see cref="DurianPackage"/> to check for.</param>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to check if contains the reference.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianPackage"/> value detected.</exception>
		public static bool HasReference(DurianPackage package, CSharpCompilation compilation)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			CheckIsValidPackageEnum(package);
			string packageName = PackageToString(package);
			return HasReference_Internal(packageName, compilation);
		}

		/// <summary>
		/// Checks, if the specified <paramref name="compilation"/> references the specified Durian <paramref name="package"/>.
		/// </summary>
		/// <param name="package"><see cref="PackageIdentity"/> of Durian module to check for.</param>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to check if contains the reference.</param>
		/// <exception cref="ArgumentNullException"><paramref name="package"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static bool HasReference(PackageIdentity package, CSharpCompilation compilation)
		{
			if (package is null)
			{
				throw new ArgumentNullException(nameof(package));
			}

			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			return HasReference_Internal(package.Name, compilation);
		}

		/// <summary>
		/// Checks, if the specified <paramref name="compilation"/> references a Durian package with the specified <paramref name="packageName"/>.
		/// </summary>
		/// <param name="packageName">Name of the Durian package to check for.</param>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to check if contains the reference.</param>
		/// <exception cref="ArgumentNullException"><paramref name="packageName"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Unknown Durian package name: <paramref name="packageName"/>.</exception>
		public static bool HasReference(string packageName, CSharpCompilation compilation)
		{
			if (packageName is null)
			{
				throw new ArgumentNullException(nameof(packageName));
			}

			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			ParsePackge(packageName);
			return HasReference_Internal(packageName, compilation);
		}

		private static bool HasReference_Internal(string packageName, CSharpCompilation compilation)
		{
			return compilation.ReferencedAssemblyNames.Any(assembly => assembly.Name == packageName);
		}
	}
}
