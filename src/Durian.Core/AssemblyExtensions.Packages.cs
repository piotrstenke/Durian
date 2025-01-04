using System;
using System.Linq;
using System.Reflection;

namespace Durian.Info
{
	/// <summary>
	/// Contains various extension methods for the <see cref="Assembly"/> class.
	/// </summary>
	public static partial class AssemblyExtensions
	{
		/// <summary>
		/// Returns a collection of all Durian packages that are not referenced by the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get all the not referenced Durian packages of.</param>
		/// <returns>A new instance of <see cref="PackageContainer"/> that contains the not referenced Durian packages.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static PackageContainer GetNotReferencedPackages(this Assembly assembly)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			PackageContainer all = PackageIdentity.GetAllPackages();

			return assembly.GetNotReferencedPackages(all.AsEnums());
		}

		/// <summary>
		/// Returns a collection of all Durian packages present in the provided collection of <paramref name="packages"/> that are not referenced by the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the not referenced Durian packages from.</param>
		/// <param name="packages"><see cref="PackageContainer"/> that provides a collection of Durian packages to pick from.</param>
		/// <returns>A new instance of <see cref="PackageContainer"/> that contains the not referenced Durian packages.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>. -or- <paramref name="packages"/> is <see langword="null"/>.</exception>
		public static PackageContainer GetNotReferencedPackages(this Assembly assembly, PackageContainer packages)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (packages is null)
			{
				throw new ArgumentNullException(nameof(packages));
			}

			if (packages.Count == 0)
			{
				return new PackageContainer();
			}

			return assembly.GetNotReferencedPackages(packages.AsEnums());
		}

		/// <summary>
		/// Returns a collection of all Durian packages present in the provided array of <paramref name="references"/> that are not referenced by the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the not referenced Durian packages of.</param>
		/// <param name="references">Array of <see cref="PackageReference"/>s to pick from.</param>
		/// <returns>A new instance of <see cref="PackageContainer"/> that contains the not referenced Durian packages.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static PackageContainer GetNotReferencedPackages(this Assembly assembly, params PackageReference[]? references)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (references is null || references.Length == 0)
			{
				return new PackageContainer();
			}

			AssemblyName[] assemblies = assembly.GetReferencedAssemblies();

			if (assemblies.Length == 0)
			{
				return new PackageContainer();
			}

			PackageContainer container = new(references.Length);

			foreach (PackageReference reference in references)
			{
				if (reference is null)
				{
					continue;
				}

				if (!container.Contains(reference.EnumValue) && !HasReference_Internal(assemblies, reference.EnumValue))
				{
					container.Include(reference);
				}
			}

			return container;
		}

		/// <summary>
		/// Returns a collection of all Durian packages present in the provided array of <paramref name="packages"/> that are not referenced by the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the not referenced Durian packages of.</param>
		/// <param name="packages">Array of <see cref="PackageIdentity"/>s to pick from.</param>
		/// <returns>A new instance of <see cref="PackageContainer"/> that contains the not referenced Durian packages.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static PackageContainer GetNotReferencedPackages(this Assembly assembly, params PackageIdentity[]? packages)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (packages is null || packages.Length == 0)
			{
				return new PackageContainer();
			}

			AssemblyName[] references = assembly.GetReferencedAssemblies();

			if (references.Length == 0)
			{
				return new PackageContainer();
			}

			PackageContainer container = new(packages.Length);

			foreach (PackageIdentity package in packages)
			{
				if (package is null)
				{
					continue;
				}

				if (!container.Contains(package.EnumValue) && !HasReference_Internal(references, package.Name))
				{
					container.Include(package);
				}
			}

			return container;
		}

		/// <summary>
		/// Returns a collection of all Durian packages present in the provided array of <paramref name="packages"/> that are not referenced by the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the not referenced Durian packages of.</param>
		/// <param name="packages">Array of <see cref="DurianPackage"/>s to pick the not referenced packages from.</param>
		/// <returns>A new instance of <see cref="PackageContainer"/> that contains the not referenced Durian packages.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianPackage"/> value detected.</exception>
		public static PackageContainer GetNotReferencedPackages(this Assembly assembly, params DurianPackage[]? packages)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (packages is null || packages.Length == 0)
			{
				return new PackageContainer();
			}

			foreach (DurianPackage package in packages)
			{
				PackageIdentity.EnsureIsValidPackageEnum_InvOp(package);
			}

			AssemblyName[] references = assembly.GetReferencedAssemblies();

			if (references.Length == 0)
			{
				return new PackageContainer();
			}

			PackageContainer container = new(packages.Length);

			foreach (DurianPackage package in packages)
			{
				if (!container.Contains(package) && !HasReference_Internal(references, package))
				{
					container.Include(package);
				}
			}

			return container;
		}

		/// <summary>
		/// Returns a collection of all Durian packages present in the provided array of <paramref name="references"/> that are referenced by the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the referenced Durian packages of.</param>
		/// <param name="references">Array of <see cref="PackageReference"/>s to pick from.</param>
		/// <returns>A new instance of <see cref="PackageContainer"/> that contains the referenced Durian packages.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static PackageContainer GetReferencedPackages(this Assembly assembly, params PackageReference[]? references)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (references is null || references.Length == 0)
			{
				return new PackageContainer();
			}

			AssemblyName[] assemblies = assembly.GetReferencedAssemblies();

			if (assemblies.Length == 0)
			{
				return new PackageContainer();
			}

			PackageContainer container = new(assemblies.Length);

			foreach (PackageReference reference in references)
			{
				if (reference is null)
				{
					continue;
				}

				if (!container.Contains(reference.EnumValue) && HasReference_Internal(assemblies, reference.EnumValue))
				{
					container.Include(reference);
				}
			}

			return container;
		}

		/// <summary>
		/// Returns a collection of all Durian packages present in the provided array of <paramref name="packages"/> that are referenced by the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the referenced Durian packages of.</param>
		/// <param name="packages">Array of <see cref="DurianPackage"/>s to pick the referenced packages from.</param>
		/// <returns>A new instance of <see cref="PackageContainer"/> that contains the referenced Durian packages.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianPackage"/> value detected.</exception>
		public static PackageContainer GetReferencedPackages(this Assembly assembly, params DurianPackage[]? packages)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (packages is null || packages.Length == 0)
			{
				return new PackageContainer();
			}

			foreach (DurianPackage package in packages)
			{
				PackageIdentity.EnsureIsValidPackageEnum_InvOp(package);
			}

			AssemblyName[] references = assembly.GetReferencedAssemblies();

			if (references.Length == 0)
			{
				return new PackageContainer();
			}

			PackageContainer container = new(packages.Length);

			foreach (DurianPackage package in packages)
			{
				if (!container.Contains(package) && HasReference_Internal(references, package))
				{
					container.Include(package);
				}
			}

			return container;
		}

		/// <summary>
		/// Returns a collection of all Durian packages that are referenced by the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get all the referenced Durian packages of.</param>
		/// <returns>A new instance of <see cref="PackageContainer"/> that contains the referenced Durian packages.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static PackageContainer GetReferencedPackages(this Assembly assembly)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			PackageContainer all = PackageIdentity.GetAllPackages();

			return assembly.GetReferencedPackages(all.AsEnums());
		}

		/// <summary>
		/// Returns a collection of all Durian packages present in the provided collection of <paramref name="packages"/> that are referenced by the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the referenced Durian packages from.</param>
		/// <param name="packages"><see cref="PackageContainer"/> that provides a collection of Durian packages to pick from.</param>
		/// <returns>A new instance of <see cref="PackageContainer"/> that contains the referenced Durian packages.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>. -or- <paramref name="packages"/> is <see langword="null"/>.</exception>
		public static PackageContainer GetReferencedPackages(this Assembly assembly, PackageContainer packages)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (packages is null)
			{
				throw new ArgumentNullException(nameof(packages));
			}

			if (packages.Count == 0)
			{
				return new PackageContainer();
			}

			return assembly.GetReferencedPackages(packages.AsEnums());
		}

		/// <summary>
		/// Returns a collection of all Durian packages present in the provided array of <paramref name="packages"/> that are referenced by the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the referenced Durian packages of.</param>
		/// <param name="packages">Array of <see cref="PackageIdentity"/>s to pick from.</param>
		/// <returns>A new instance of <see cref="PackageContainer"/> that contains the referenced Durian packages.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static PackageContainer GetReferencedPackages(this Assembly assembly, params PackageIdentity[]? packages)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (packages is null || packages.Length == 0)
			{
				return new PackageContainer();
			}

			AssemblyName[] references = assembly.GetReferencedAssemblies();

			if (references.Length == 0)
			{
				return new PackageContainer();
			}

			PackageContainer container = new(packages.Length);

			foreach (PackageIdentity package in packages)
			{
				if (package is null)
				{
					continue;
				}

				if (!container.Contains(package.EnumValue) && HasReference_Internal(references, package.Name))
				{
					container.Include(package);
				}
			}

			return container;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="assembly"/> references the provided <paramref name="package"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to check if contains a reference to the provided <paramref name="package"/>.</param>
		/// <param name="package"><see cref="PackageReference"/> of Durian package to check for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="package"/> is <see langword="null"/>. -or- <paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static bool HasReference(this Assembly assembly, PackageReference package)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (package is null)
			{
				throw new ArgumentNullException(nameof(package));
			}

			return HasReference_Internal(assembly, package.EnumValue);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="assembly"/> references the provided <paramref name="package"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to check if contains a reference to the provided <paramref name="package"/>.</param>
		/// <param name="package"><see cref="PackageIdentity"/> representing a Durian package to check for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="package"/> is <see langword="null"/>. -or- <paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static bool HasReference(this Assembly assembly, PackageIdentity package)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (package is null)
			{
				throw new ArgumentNullException(nameof(package));
			}

			return HasReference_Internal(assembly, package.Name);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="assembly"/> references the provided <paramref name="package"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to check if contains a reference to the provided <paramref name="package"/>.</param>
		/// <param name="package"><see cref="DurianPackage"/> representing a Durian package to check for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianPackage"/> value detected.</exception>
		public static bool HasReference(this Assembly assembly, DurianPackage package)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			return HasReference_Internal(assembly, package);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="assembly"/> references a Durian package with the given <paramref name="packageName"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to check if contains the reference.</param>
		/// <param name="packageName">Name of the Durian package to check for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="packageName"/> is <see langword="null"/>. -or- <paramref name="assembly"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Unknown Durian package name: <paramref name="packageName"/>.</exception>
		public static bool HasReference(this Assembly assembly, string packageName)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			PackageIdentity.Parse(packageName);
			return HasReference_Internal(assembly, packageName);
		}

		private static bool HasReference_Internal(Assembly assembly, string packageName)
		{
			AssemblyName[] references = assembly.GetReferencedAssemblies();
			return HasReference_Internal(references, packageName);
		}

		private static bool HasReference_Internal(Assembly assembly, DurianPackage package)
		{
			string packageName = PackageIdentity.GetName(package);
			return HasReference_Internal(assembly, packageName);
		}

		private static bool HasReference_Internal(AssemblyName[] references, string packageName)
		{
			return references.Any(r => r.Name == packageName);
		}

		private static bool HasReference_Internal(AssemblyName[] references, DurianPackage package)
		{
			string packageName = PackageIdentity.GetName(package);
			return HasReference_Internal(references, packageName);
		}
	}
}
