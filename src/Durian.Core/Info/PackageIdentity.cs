using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Durian.Info
{
	/// <summary>
	/// Contains basic information about a Durian package.
	/// </summary>
	[DebuggerDisplay("Name = {Name}, Version = {Version}")]
	public sealed record PackageIdentity
	{
		private ModuleIdentity? _module;

		/// <summary>
		/// Name of the package.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Version of the package.
		/// </summary>
		public string Version { get; }

		/// <summary>
		/// Module this package is part of.
		/// </summary>
		public ModuleIdentity Module => _module!;

		/// <summary>
		/// Type of this package.
		/// </summary>
		public PackageType Type { get; }

		/// <summary>
		/// Enum value of <see cref="DurianPackage"/> that corresponds with this <see cref="PackageIdentity"/>.
		/// </summary>
		public DurianPackage EnumValue { get; }

		internal PackageIdentity(DurianPackage enumValue, string version, PackageType type)
		{
			EnumValue = enumValue;
			Name = EnumToString(enumValue);
			Version = version;
			Type = type;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Name}, {Version}";
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int hashCode = -726504116;
			hashCode = (hashCode * -1521134295) + EnumValue.GetHashCode();
			hashCode = (hashCode * -1521134295) + Version.GetHashCode();
			hashCode = (hashCode * -1521134295) + Type.GetHashCode();
			hashCode = (hashCode * -1521134295) + Module.GetHashCode();
			return hashCode;
		}

		internal void SetModule(ModuleIdentity module)
		{
			_module = module;
		}

		/// <summary>
		/// Checks, if the calling <see cref="Assembly"/> references the specified Durian <paramref name="package"/>.
		/// </summary>
		/// <param name="package"><see cref="PackageIdentity"/> of Durian module to check for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="package"/> is <see langword="null"/>.</exception>
		public static bool HasReference(PackageIdentity package)
		{
			if (package is null)
			{
				throw new ArgumentNullException(nameof(package));
			}

			return HasReference_Internal(package.Name, Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Checks, if the specified <paramref name="assembly"/> references the specified Durian <paramref name="package"/>.
		/// </summary>
		/// <param name="package"><see cref="PackageIdentity"/> of Durian module to check for.</param>
		/// <param name="assembly"><see cref="Assembly"/> to check if contains the reference.</param>
		/// <exception cref="ArgumentNullException"><paramref name="package"/> is <see langword="null"/>. -or- <paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static bool HasReference(PackageIdentity package, Assembly assembly)
		{
			if (package is null)
			{
				throw new ArgumentNullException(nameof(package));
			}

			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			return HasReference_Internal(package.Name, assembly);
		}

		/// <summary>
		/// Checks, if the calling <see cref="Assembly"/> references the specified Durian <paramref name="package"/>.
		/// </summary>
		/// <param name="package"><see cref="DurianPackage"/> to check for.</param>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianPackage"/> value detected.</exception>
		public static bool HasReference(DurianPackage package)
		{
			CheckIsValidPackageEnum(package);
			string packageName = EnumToString(package);
			return HasReference_Internal(packageName, Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Checks, if the specified <paramref name="assembly"/> references the specified Durian <paramref name="package"/>.
		/// </summary>
		/// <param name="package"><see cref="DurianPackage"/> to check for.</param>
		/// <param name="assembly"><see cref="Assembly"/> to check if contains the reference.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianPackage"/> value detected.</exception>
		public static bool HasReference(DurianPackage package, Assembly assembly)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			CheckIsValidPackageEnum(package);
			string packageName = EnumToString(package);
			return HasReference_Internal(packageName, assembly);
		}

		/// <summary>
		/// Checks, if the calling <see cref="Assembly"/> references a Durian package with the specified <paramref name="packageName"/>.
		/// </summary>
		/// <param name="packageName">Name of the Durian package to check for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="packageName"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Unknown Durian package name: <paramref name="packageName"/>.</exception>
		public static bool HasReference(string packageName)
		{
			return HasReference(packageName, Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Checks, if the specified <paramref name="assembly"/> references a Durian package with the specified <paramref name="packageName"/>.
		/// </summary>
		/// <param name="packageName">Name of the Durian package to check for.</param>
		/// <param name="assembly"><see cref="Assembly"/> to check if contains the reference.</param>
		/// <exception cref="ArgumentNullException"><paramref name="packageName"/> is <see langword="null"/>. -or- <paramref name="assembly"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Unknown Durian package name: <paramref name="packageName"/>.</exception>
		public static bool HasReference(string packageName, Assembly assembly)
		{
			if (packageName is null)
			{
				throw new ArgumentNullException(nameof(packageName));
			}

			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			ParsePackge(packageName);
			return HasReference_Internal(packageName, assembly);
		}

		/// <summary>
		/// Returns an array of <see cref="PackageIdentity"/> of all existing Durian packages.
		/// </summary>
		public static PackageIdentity[] GetAllPackages()
		{
			return new PackageIdentity[]
			{
				PackageRepository.Core,
				PackageRepository.CoreAnalyzer,
				PackageRepository.AnalysisServices,
				PackageRepository.TestServices,
				PackageRepository.DefaultParam,
			};
		}

		/// <summary>
		/// Returns an array of <see cref="DurianPackage"/> values representing all of the existing Durian packages.
		/// </summary>
		public static DurianPackage[] GetAllPackagesAsEnums()
		{
			return new DurianPackage[]
			{
				DurianPackage.Core,
				DurianPackage.CoreAnalyzer,
				DurianPackage.AnalysisServices,
				DurianPackage.TestServices,
				DurianPackage.DefaultParam,
			};
		}

		/// <summary>
		/// Returns an array of <see cref="PackageIdentity"/> of all Durian packages referenced by the calling <see cref="Assembly"/>.
		/// </summary>
		public static PackageIdentity[] GetReferencedPackages()
		{
			return GetReferencedPackages(Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Returns an array of <see cref="PackageIdentity"/> of all Durian packages referenced by the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the references Durian packages of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static PackageIdentity[] GetReferencedPackages(Assembly assembly)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			PackageIdentity[] allPackages = GetAllPackages();

			if (allPackages.Length == 0)
			{
				return allPackages;
			}

			List<PackageIdentity> referenced = new(allPackages.Length);

			foreach (PackageIdentity package in allPackages)
			{
				if (HasReference_Internal(package.Name, assembly))
				{
					referenced.Add(package);
				}
			}

			return referenced.ToArray();
		}

		/// <summary>
		/// Returns an array of <see cref="PackageIdentity"/> of all Durian packages referenced by the calling <see cref="Assembly"/>. Only <see cref="PackageIdentity"/> that are present in the given array of <paramref name="packages"/> are included.
		/// </summary>
		/// <param name="packages">Array of <see cref="PackageIdentity"/> to pick the referenced packages from.</param>
		public static PackageIdentity[] GetReferencedPackages(PackageIdentity[]? packages)
		{
			return GetReferencedPackages(Assembly.GetCallingAssembly(), packages);
		}

		/// <summary>
		/// Returns an array of <see cref="PackageIdentity"/> of all Durian packages referenced by the specified <paramref name="assembly"/>. Only <see cref="PackageIdentity"/> that are present in the given array of <paramref name="packages"/> are included.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the references Durian packages of.</param>
		/// <param name="packages">Array of <see cref="PackageIdentity"/> to pick the referenced packages from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static PackageIdentity[] GetReferencedPackages(Assembly assembly, PackageIdentity[]? packages)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (packages is null || packages.Length == 0)
			{
				return Array.Empty<PackageIdentity>();
			}

			HashSet<PackageIdentity> set = new(PackageEnumEqualityComparer.Instance);

			foreach (PackageIdentity package in packages)
			{
				if (HasReference_Internal(package.Name, assembly))
				{
					set.Add(package);
				}
			}

			PackageIdentity[] array = new PackageIdentity[set.Count];
			set.CopyTo(array);
			return array;
		}

		/// <summary>
		/// Returns an array of <see cref="PackageIdentity"/> of all Durian packages referenced by the calling <see cref="Assembly"/>. Only <see cref="DurianPackage"/>s that are present in the given array of <paramref name="packages"/> are included.
		/// </summary>
		/// <param name="packages">Array of <see cref="DurianPackage"/>s to pick the referenced packages from.</param>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianPackage"/> value detected.</exception>
		public static PackageIdentity[] GetReferencedPackages(DurianPackage[]? packages)
		{
			return GetReferencedPackages(Assembly.GetCallingAssembly(), packages);
		}

		/// <summary>
		/// Returns an array of <see cref="PackageIdentity"/> of all Durian packages referenced by the specified <paramref name="assembly"/>. Only <see cref="DurianPackage"/>s that are present in the given array of <paramref name="packages"/> are included.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the references Durian packages of.</param>
		/// <param name="packages">Array of <see cref="DurianPackage"/>s to pick the referenced packages from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianPackage"/> value detected.</exception>
		public static PackageIdentity[] GetReferencedPackages(Assembly assembly, DurianPackage[]? packages)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
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
				string packageName = EnumToString(package);

				if (HasReference_Internal(packageName, assembly) && set.Add(package))
				{
					list.Add(GetPackage(package));
				}
			}

			return list.ToArray();
		}

		/// <summary>
		/// Returns an array of <see cref="DurianPackage"/>s of all Durian packages referenced by the calling <see cref="Assembly"/>.
		/// </summary>
		public static DurianPackage[] GetReferencedPackagesAsEnums()
		{
			return GetReferencedPackagesAsEnums(Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Returns an array of <see cref="DurianPackage"/>s of all Durian packages referenced by the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the references Durian packages of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static DurianPackage[] GetReferencedPackagesAsEnums(Assembly assembly)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			DurianPackage[] allPackages = GetAllPackagesAsEnums();

			if (allPackages.Length == 0)
			{
				return allPackages;
			}

			List<DurianPackage> referenced = new(allPackages.Length);

			foreach (DurianPackage package in allPackages)
			{
				string packageName = EnumToString(package);

				if (HasReference_Internal(packageName, assembly))
				{
					referenced.Add(package);
				}
			}

			return referenced.ToArray();
		}

		/// <summary>
		/// Returns an array of <see cref="DurianPackage"/>s of all Durian packages referenced by the calling <see cref="Assembly"/>. Only <see cref="PackageIdentity"/> that are present in the given array of <paramref name="packages"/> are included.
		/// </summary>
		/// <param name="packages">Array of <see cref="PackageIdentity"/> to pick the referenced packages from.</param>
		public static DurianPackage[] GetReferencedPackagesAsEnums(PackageIdentity[]? packages)
		{
			return GetReferencedPackagesAsEnums(Assembly.GetCallingAssembly(), packages);
		}

		/// <summary>
		/// Returns an array of <see cref="DurianPackage"/>s of all Durian packages referenced by the specified <paramref name="assembly"/>. Only <see cref="PackageIdentity"/> that are present in the given array of <paramref name="packages"/> are included.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the references Durian packages of.</param>
		/// <param name="packages">Array of <see cref="PackageIdentity"/> to pick the referenced packages from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static DurianPackage[] GetReferencedPackagesAsEnums(Assembly assembly, PackageIdentity[]? packages)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (packages is null || packages.Length == 0)
			{
				return Array.Empty<DurianPackage>();
			}

			HashSet<DurianPackage> set = new();

			foreach (PackageIdentity package in packages)
			{
				if (HasReference_Internal(package.Name, assembly))
				{
					set.Add(package.EnumValue);
				}
			}

			DurianPackage[] array = new DurianPackage[set.Count];
			set.CopyTo(array);
			return array;
		}

		/// <summary>
		/// Returns an array of <see cref="DurianPackage"/>s of all Durian packages referenced by the calling <see cref="Assembly"/>. Only <see cref="DurianPackage"/>s that are present in the given array of <paramref name="packages"/> are included.
		/// </summary>
		/// <param name="packages">Array of <see cref="DurianPackage"/>s to pick the referenced packages from.</param>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianPackage"/> value detected.</exception>
		public static DurianPackage[] GetReferencedPackagesAsEnums(DurianPackage[]? packages)
		{
			return GetReferencedPackagesAsEnums(Assembly.GetCallingAssembly(), packages);
		}

		/// <summary>
		/// Returns an array of <see cref="DurianPackage"/>s of all Durian packages referenced by the specified <paramref name="assembly"/>. Only <see cref="DurianPackage"/>s that are present in the given array of <paramref name="packages"/> are included.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the references Durian packages of.</param>
		/// <param name="packages">Array of <see cref="DurianPackage"/>s to pick the referenced packages from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianPackage"/> value detected.</exception>
		public static DurianPackage[] GetReferencedPackagesAsEnums(Assembly assembly, DurianPackage[]? packages)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (packages is null || packages.Length == 0)
			{
				return Array.Empty<DurianPackage>();
			}

			HashSet<DurianPackage> set = new();

			foreach (DurianPackage package in packages)
			{
				CheckIsValidPackageEnum(package);
				string packageName = EnumToString(package);

				if (HasReference_Internal(packageName, assembly))
				{
					set.Add(package);
				}
			}

			DurianPackage[] array = new DurianPackage[set.Count];
			set.CopyTo(array);
			return array;
		}

		/// <summary>
		/// Returns an array of <see cref="PackageIdentity"/> of all Durian packages that are not referenced by the calling <see cref="Assembly"/>.
		/// </summary>
		public static PackageIdentity[] GetNotReferencedPackages()
		{
			return GetNotReferencedPackages(Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Returns an array of <see cref="PackageIdentity"/> of all Durian packages that are not referenced by the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the not references Durian packages of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static PackageIdentity[] GetNotReferencedPackages(Assembly assembly)
		{
			return GetNotReferencedPackages(assembly, GetAllPackages());
		}

		/// <summary>
		/// Returns an array of <see cref="PackageIdentity"/> of all Durian packages that are not referenced by the calling <see cref="Assembly"/>. Only <see cref="PackageIdentity"/> that are present in the given array of <paramref name="packages"/> are included.
		/// </summary>
		/// <param name="packages">Array of <see cref="PackageIdentity"/> to pick the not referenced packages from.</param>
		public static PackageIdentity[] GetNotReferencedPackages(PackageIdentity[]? packages)
		{
			return GetNotReferencedPackages(Assembly.GetCallingAssembly(), packages);
		}

		/// <summary>
		/// Returns an array of <see cref="PackageIdentity"/> of all Durian packages that are not referenced by the specified <paramref name="assembly"/>. Only <see cref="PackageIdentity"/> that are present in the given array of <paramref name="packages"/> are included.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the not references Durian packages of.</param>
		/// <param name="packages">Array of <see cref="PackageIdentity"/> to pick the not referenced packages from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static PackageIdentity[] GetNotReferencedPackages(Assembly assembly, PackageIdentity[]? packages)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (packages is null || packages.Length == 0)
			{
				return Array.Empty<PackageIdentity>();
			}

			PackageIdentity[] references = GetReferencedPackages(assembly, packages);

			if (packages.Length == references.Length)
			{
				return Array.Empty<PackageIdentity>();
			}

			PackageEnumEqualityComparer comparer = PackageEnumEqualityComparer.Instance;

			if (references.Length == 0)
			{
				return packages.Distinct(comparer).ToArray();
			}

			return packages
				.Except(references, comparer)
				.Distinct(comparer)
				.ToArray();
		}

		/// <summary>
		/// Returns an array of <see cref="PackageIdentity"/> of all Durian packages that are not referenced by the calling <see cref="Assembly"/>. Only <see cref="DurianPackage"/>s that are present in the given array of <paramref name="packages"/> are included.
		/// </summary>
		/// <param name="packages">Array of <see cref="DurianPackage"/>s to pick the not referenced packages from.</param>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianPackage"/> value detected.</exception>
		public static PackageIdentity[] GetNotReferencedPackages(DurianPackage[]? packages)
		{
			return GetNotReferencedPackages(Assembly.GetCallingAssembly(), packages);
		}

		/// <summary>
		/// Returns an array of <see cref="PackageIdentity"/> of all Durian packages that are not referenced by the specified <paramref name="assembly"/>. Only <see cref="DurianPackage"/>s that are present in the given array of <paramref name="packages"/> are included.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the not references Durian packages of.</param>
		/// <param name="packages">Array of <see cref="DurianPackage"/>s to pick the not referenced packages from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianPackage"/> value detected.</exception>
		public static PackageIdentity[] GetNotReferencedPackages(Assembly assembly, DurianPackage[]? packages)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
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

			PackageIdentity[] references = GetReferencedPackages(assembly, allPackages);

			if (packages.Length == references.Length)
			{
				return Array.Empty<PackageIdentity>();
			}

			PackageEnumEqualityComparer comparer = PackageEnumEqualityComparer.Instance;

			if (references.Length == 0)
			{
				return allPackages.Distinct(comparer).ToArray();
			}

			return allPackages
				.Except(references, comparer)
				.Distinct(comparer)
				.ToArray();
		}

		/// <summary>
		/// Returns an array of <see cref="DurianPackage"/>s of all Durian packages that are not referenced by the calling <see cref="Assembly"/>.
		/// </summary>
		public static DurianPackage[] GetNotReferencedPackagesAsEnums()
		{
			return GetNotReferencedPackagesAsEnums(Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Returns an array of <see cref="DurianPackage"/>s of all Durian packages that are not referenced by the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the not references Durian packages of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static DurianPackage[] GetNotReferencedPackagesAsEnums(Assembly assembly)
		{
			return GetNotReferencedPackagesAsEnums(assembly, GetAllPackages());
		}

		/// <summary>
		/// Returns an array of <see cref="DurianPackage"/>s of all Durian packages that are not referenced by the calling <see cref="Assembly"/>. Only <see cref="PackageIdentity"/> that are present in the given array of <paramref name="packages"/> are included.
		/// </summary>
		/// <param name="packages">Array of <see cref="PackageIdentity"/> to pick the not referenced packages from.</param>
		public static DurianPackage[] GetNotReferencedPackagesAsEnums(PackageIdentity[]? packages)
		{
			return GetNotReferencedPackagesAsEnums(Assembly.GetCallingAssembly(), packages);
		}

		/// <summary>
		/// Returns an array of <see cref="DurianPackage"/>s of all Durian packages that are not referenced by the specified <paramref name="assembly"/>. Only <see cref="PackageIdentity"/> that are present in the given array of <paramref name="packages"/> are included.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the not references Durian packages of.</param>
		/// <param name="packages">Array of <see cref="PackageIdentity"/> to pick the not referenced packages from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static DurianPackage[] GetNotReferencedPackagesAsEnums(Assembly assembly, PackageIdentity[]? packages)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (packages is null || packages.Length == 0)
			{
				return Array.Empty<DurianPackage>();
			}

			PackageIdentity[] references = GetReferencedPackages(assembly, packages);

			if (packages.Length == references.Length)
			{
				return Array.Empty<DurianPackage>();
			}

			PackageEnumEqualityComparer comparer = PackageEnumEqualityComparer.Instance;

			if (references.Length == 0)
			{
				return packages
					.Distinct(comparer)
					.Select(m => m.EnumValue)
					.ToArray();
			}

			return packages
				.Except(references, comparer)
				.Distinct(comparer)
				.Select(m => m.EnumValue)
				.ToArray();
		}

		/// <summary>
		/// Returns an array of <see cref="DurianPackage"/>s of all Durian packages that are not referenced by the calling <see cref="Assembly"/>. Only <see cref="DurianPackage"/>s that are present in the given array of <paramref name="packages"/> are included.
		/// </summary>
		/// <param name="packages">Array of <see cref="DurianPackage"/>s to pick the not referenced packages from.</param>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianPackage"/> value detected.</exception>
		public static DurianPackage[] GetNotReferencedPackagesAsEnums(DurianPackage[]? packages)
		{
			return GetNotReferencedPackagesAsEnums(Assembly.GetCallingAssembly(), packages);
		}

		/// <summary>
		/// Returns an array of <see cref="DurianPackage"/>s of all Durian packages that are not referenced by the specified <paramref name="assembly"/>. Only <see cref="DurianPackage"/>s that are present in the given array of <paramref name="packages"/> are included.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the not references Durian packages of.</param>
		/// <param name="packages">Array of <see cref="DurianPackage"/>s to pick the not referenced packages from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianPackage"/> value detected.</exception>
		public static DurianPackage[] GetNotReferencedPackagesAsEnums(Assembly assembly, DurianPackage[]? packages)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
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

			PackageIdentity[] references = GetReferencedPackages(assembly, allPackages);

			if (packages.Length == references.Length)
			{
				return Array.Empty<DurianPackage>();
			}

			PackageEnumEqualityComparer comparer = PackageEnumEqualityComparer.Instance;

			if (references.Length == 0)
			{
				return allPackages
					.Distinct(comparer)
					.Select(m => m.EnumValue)
					.ToArray();
			}

			return allPackages
				.Except(references, comparer)
				.Distinct(comparer)
				.Select(m => m.EnumValue)
				.ToArray();
		}

		/// <summary>
		/// Returns a new instance of <see cref="PackageIdentity"/> corresponding with the specified <see cref="DurianPackage"/>.
		/// </summary>
		/// <param name="package"><see cref="DurianPackage"/> to get the <see cref="PackageIdentity"/> of.</param>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianPackage"/> value detected.</exception>
		public static PackageIdentity GetPackage(DurianPackage package)
		{
			return package switch
			{
				DurianPackage.AnalysisServices => PackageRepository.AnalysisServices,
				DurianPackage.Core => PackageRepository.Core,
				DurianPackage.CoreAnalyzer => PackageRepository.CoreAnalyzer,
				DurianPackage.DefaultParam => PackageRepository.DefaultParam,
				DurianPackage.TestServices => PackageRepository.TestServices,
				_ => throw new InvalidOperationException($"Unknown {nameof(DurianPackage)} value: {package}!")
			};
		}

		/// <summary>
		/// Returns a new instance of <see cref="PackageIdentity"/> of Durian package with the specified <paramref name="packageName"/>.
		/// </summary>
		/// <param name="packageName">Name of the Durian package to get the <see cref="PackageIdentity"/> of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="packageName"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Unknown Durian package name: <paramref name="packageName"/>.</exception>
		public static PackageIdentity GetPackage(string packageName)
		{
			DurianPackage package = ParsePackge(packageName);
			return GetPackage(package);
		}

		internal static void CheckIsValidPackageEnum(DurianPackage package)
		{
			if (package < DurianPackage.Core || package > DurianPackage.DefaultParam)
			{
				throw new InvalidOperationException($"Unknown {nameof(DurianPackage)} value: {package}!");
			}
		}

		internal static DurianPackage ParsePackge(string packageName)
		{
			if (packageName is null)
			{
				throw new ArgumentNullException(nameof(packageName));
			}

			string name = packageName.Replace("Durian.", "").Replace(".", "");

			if (Enum.TryParse(name, out DurianPackage package))
			{
				return package;
			}

			throw new ArgumentException($"Unknown Durian package name: {packageName}", nameof(packageName));
		}

		internal static string EnumToString(DurianPackage package)
		{
			return package switch
			{
				DurianPackage.CoreAnalyzer => "Durian.Core.Analyzer",
				_ => $"Durian.{package}"
			};
		}

		private static bool HasReference_Internal(string packageName, Assembly assembly)
		{
			return assembly.GetReferencedAssemblies().Any(a => a.Name == packageName);
		}
	}
}
