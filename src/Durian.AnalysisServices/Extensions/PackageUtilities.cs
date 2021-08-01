// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;
using Durian.Info;
using Microsoft.CodeAnalysis;
using static Durian.Info.PackageIdentity;

namespace Durian.Analysis
{
	/// <summary>
	/// Contains various <see cref="PackageIdentity"/>-related extension methods for the <see cref="Compilation"/> class.
	/// </summary>
	public static class PackageUtilities
	{
		/// <summary>
		/// Returns a collection of all Durian packages that are not referenced by the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get all the not referenced Durian packages of.</param>
		/// <returns>A new instance of <see cref="PackageContainer"/> that contains the not referenced Durian packages.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static PackageContainer GetNotReferencedPackages(this Compilation compilation)
		{
			PackageContainer all = GetAllPackages();

			return GetNotReferencedPackages(compilation, all);
		}

		/// <summary>
		/// Returns a collection of all Durian packages present in the provided collection of <paramref name="packages"/> that are not referenced by the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the not referenced Durian packages from.</param>
		/// <param name="packages"><see cref="PackageContainer"/> that provides a collection of Durian packages to pick from.</param>
		/// <returns>A new instance of <see cref="PackageContainer"/> that contains the not referenced Durian packages.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="packages"/> is <see langword="null"/>.</exception>
		public static PackageContainer GetNotReferencedPackages(this Compilation compilation, PackageContainer packages)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (packages is null)
			{
				throw new ArgumentNullException(nameof(packages));
			}

			if (packages.Count == 0)
			{
				return new PackageContainer();
			}

			return compilation.GetNotReferencedPackages(packages.AsEnums());
		}

		/// <summary>
		/// Returns a collection of all Durian packages present in the provided array of <paramref name="references"/> that are not referenced by the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the not referenced Durian packages of.</param>
		/// <param name="references">Array of <see cref="PackageReference"/>s to pick from.</param>
		/// <returns>A new instance of <see cref="PackageContainer"/> that contains the not referenced Durian packages.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static PackageContainer GetNotReferencedPackages(this Compilation compilation, params PackageReference[]? references)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (references is null || references.Length == 0)
			{
				return new PackageContainer();
			}

			AssemblyIdentity[] assemblies = compilation.ReferencedAssemblyNames.ToArray();

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
		/// Returns a collection of all Durian packages present in the provided array of <paramref name="packages"/> that are not referenced by the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the not referenced Durian packages of.</param>
		/// <param name="packages">Array of <see cref="PackageIdentity"/>s to pick from.</param>
		/// <returns>A new instance of <see cref="PackageContainer"/> that contains the not referenced Durian packages.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static PackageContainer GetNotReferencedPackages(this Compilation compilation, params PackageIdentity[]? packages)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (packages is null || packages.Length == 0)
			{
				return new PackageContainer();
			}

			AssemblyIdentity[] references = compilation.ReferencedAssemblyNames.ToArray();

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
		/// Returns a collection of all Durian packages present in the provided array of <paramref name="packages"/> that are not referenced by the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the not referenced Durian packages of.</param>
		/// <param name="packages">Array of <see cref="DurianPackage"/>s to pick the not referenced packages from.</param>
		/// <returns>A new instance of <see cref="PackageContainer"/> that contains the not referenced Durian packages.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianPackage"/> value detected.</exception>
		public static PackageContainer GetNotReferencedPackages(this Compilation compilation, params DurianPackage[]? packages)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (packages is null || packages.Length == 0)
			{
				return new PackageContainer();
			}

			AssemblyIdentity[] references = compilation.ReferencedAssemblyNames.ToArray();

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
		/// Returns a collection of all Durian packages present in the provided array of <paramref name="references"/> that are referenced by the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the referenced Durian packages of.</param>
		/// <param name="references">Array of <see cref="PackageReference"/>s to pick from.</param>
		/// <returns>A new instance of <see cref="PackageContainer"/> that contains the referenced Durian packages.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static PackageContainer GetReferencedPackages(this Compilation compilation, params PackageReference[]? references)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (references is null || references.Length == 0)
			{
				return new PackageContainer();
			}

			AssemblyIdentity[] assemblies = compilation.ReferencedAssemblyNames.ToArray();

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

				if (!container.Contains(reference.EnumValue) && HasReference_Internal(assemblies, reference.EnumValue))
				{
					container.Include(reference);
				}
			}

			return container;
		}

		/// <summary>
		/// Returns a collection of all Durian packages present in the provided array of <paramref name="packages"/> that are referenced by the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the referenced Durian packages of.</param>
		/// <param name="packages">Array of <see cref="DurianPackage"/>s to pick the referenced packages from.</param>
		/// <returns>A new instance of <see cref="PackageContainer"/> that contains the referenced Durian packages.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianPackage"/> value detected.</exception>
		public static PackageContainer GetReferencedPackages(this Compilation compilation, params DurianPackage[]? packages)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (packages is null || packages.Length == 0)
			{
				return new PackageContainer();
			}

			AssemblyIdentity[] references = compilation.ReferencedAssemblyNames.ToArray();

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
		/// Returns a collection of all Durian packages that are referenced by the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get all the referenced Durian packages of.</param>
		/// <returns>A new instance of <see cref="PackageContainer"/> that contains the referenced Durian packages.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static PackageContainer GetReferencedPackages(this Compilation compilation)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			PackageContainer all = GetAllPackages();

			return compilation.GetReferencedPackages(all.AsEnums());
		}

		/// <summary>
		/// Returns a collection of all Durian packages present in the provided collection of <paramref name="packages"/> that are referenced by the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the referenced Durian packages from.</param>
		/// <param name="packages"><see cref="PackageContainer"/> that provides a collection of Durian packages to pick from.</param>
		/// <returns>A new instance of <see cref="PackageContainer"/> that contains the referenced Durian packages.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="packages"/> is <see langword="null"/>.</exception>
		public static PackageContainer GetReferencedPackages(this Compilation compilation, PackageContainer packages)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (packages is null)
			{
				throw new ArgumentNullException(nameof(packages));
			}

			if (packages.Count == 0)
			{
				return new PackageContainer();
			}

			return compilation.GetReferencedPackages(packages.AsEnums());
		}

		/// <summary>
		/// Returns a collection of all Durian packages present in the provided array of <paramref name="packages"/> that are referenced by the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the referenced Durian packages of.</param>
		/// <param name="packages">Array of <see cref="PackageIdentity"/>s to pick from.</param>
		/// <returns>A new instance of <see cref="PackageContainer"/> that contains the referenced Durian packages.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static PackageContainer GetReferencedPackages(this Compilation compilation, params PackageIdentity[]? packages)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (packages is null || packages.Length == 0)
			{
				return new PackageContainer();
			}

			AssemblyIdentity[] references = compilation.ReferencedAssemblyNames.ToArray();

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
		/// Determines whether the specified <paramref name="compilation"/> references the provided <paramref name="package"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to check if contains a reference to the provided <paramref name="package"/>.</param>
		/// <param name="package"><see cref="PackageReference"/> of Durian package to check for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="package"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static bool HasReference(this Compilation compilation, PackageReference package)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (package is null)
			{
				throw new ArgumentNullException(nameof(package));
			}

			return HasReference_Internal(compilation, package.EnumValue);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="compilation"/> references the provided <paramref name="package"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to check if contains a reference to the provided <paramref name="package"/>.</param>
		/// <param name="package"><see cref="PackageIdentity"/> representing a Durian package to check for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="package"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static bool HasReference(this Compilation compilation, PackageIdentity package)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (package is null)
			{
				throw new ArgumentNullException(nameof(package));
			}

			return HasReference_Internal(compilation, package.Name);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="compilation"/> references the provided <paramref name="package"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to check if contains a reference to the provided <paramref name="package"/>.</param>
		/// <param name="package"><see cref="DurianPackage"/> representing a Durian package to check for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianPackage"/> value detected.</exception>
		public static bool HasReference(this Compilation compilation, DurianPackage package)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			return HasReference_Internal(compilation, package);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="compilation"/> references a Durian package with the given <paramref name="packageName"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to check if contains the reference.</param>
		/// <param name="packageName">Name of the Durian package to check for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="packageName"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Unknown Durian package name: <paramref name="packageName"/>.</exception>
		public static bool HasReference(this Compilation compilation, string packageName)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			ParsePackage(packageName);
			return HasReference_Internal(compilation, packageName);
		}

		private static bool HasReference_Internal(Compilation compilation, string packageName)
		{
			return compilation.ReferencedAssemblyNames.Any(assembly => assembly.Name == packageName);
		}

		private static bool HasReference_Internal(Compilation compilation, DurianPackage package)
		{
			string packageName = PackageToString(package);
			return HasReference_Internal(compilation, packageName);
		}

		private static bool HasReference_Internal(AssemblyIdentity[] assemblies, DurianPackage package)
		{
			string packageName = PackageToString(package);
			return HasReference_Internal(assemblies, packageName);
		}

		private static bool HasReference_Internal(AssemblyIdentity[] assemblies, string packageName)
		{
			return assemblies.Any(assembly => assembly.Name == packageName);
		}
	}
}
