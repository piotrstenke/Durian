using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Durian.Info
{
	public partial class PackageIdentity
	{
		private static DurianPackage[]? _allPackages;

		private static DurianPackage[] AllPackages
		{
			get => _allPackages ??= Enum.GetValues(typeof(DurianPackage))
				.Cast<DurianPackage>()
				.Skip(1)
				.ToArray();
		}

		/// <summary>
		/// Deallocates all cached instances of <see cref="PackageIdentity"/>.
		/// </summary>
		public static void Deallocate()
		{
			IdentityPool.Packages.Clear();
			_allPackages = default;
		}

		/// <summary>
		/// Throws an <see cref="InvalidOperationException"/> if the specified <paramref name="package"/> is not a valid <see cref="DurianPackage"/> value.
		/// </summary>
		/// <param name="package"><see cref="DurianPackage"/> to ensure that is valid.</param>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianPackage"/> value: <paramref name="package"/>.</exception>
		public static void EnsureIsValidPackageEnum(DurianPackage package)
		{
			if (!GlobalInfo.IsValidPackageValue(package))
			{
				throw new ArgumentException($"Unknown {nameof(DurianPackage)} value: {package}!");
			}
		}

		/// <summary>
		/// Returns a collection of all existing Durian packages.
		/// </summary>
		/// <returns>A new instance of <see cref="PackageContainer"/> that contains all the existing Durian packages.</returns>
		public static PackageContainer GetAllPackages()
		{
			List<DurianPackage> packages = new(AllPackages);

			return new PackageContainer(packages);
		}

		/// <summary>
		/// Returns a name of the specified <paramref name="package"/>.
		/// </summary>
		/// <param name="package"><see cref="DurianPackage"/> to get the name of.</param>
		/// <exception cref="ArgumentException">Unknown <see cref="DurianPackage"/> value: <paramref name="package"/>.</exception>
		public static string GetName(DurianPackage package)
		{
			if (!TryGetName(package, out string? packageName))
			{
				throw new ArgumentException($"Unknown {nameof(DurianPackage)} value: {package}", nameof(package));
			}

			return packageName;
		}

		/// <summary>
		/// Returns a collection of all Durian packages that are not referenced by the calling <see cref="Assembly"/>.
		/// </summary>
		/// <returns>A new instance of <see cref="PackageContainer"/> that contains the not referenced Durian packages.</returns>
		public static PackageContainer GetNotReferencedPackages()
		{
			return Assembly.GetCallingAssembly().GetNotReferencedPackages();
		}

		/// <summary>
		/// Returns a collection of all Durian packages present in the provided array of <paramref name="references"/> that are not referenced by the calling <see cref="Assembly"/>.
		/// </summary>
		/// <param name="references">Array of <see cref="PackageReference"/>s to pick from.</param>
		/// <returns>A new instance of <see cref="PackageContainer"/> that contains the not referenced Durian packages.</returns>
		public static PackageContainer GetNotReferencedPackages(params PackageReference[]? references)
		{
			return Assembly.GetCallingAssembly().GetNotReferencedPackages(references);
		}

		/// <summary>
		/// Returns a collection of all Durian packages present in the provided collection of <paramref name="packages"/> that are not referenced by the calling <see cref="Assembly"/>.
		/// </summary>
		/// <param name="packages"><see cref="PackageContainer"/> that provides a collection of Durian packages to pick from.</param>
		/// <returns>A new instance of <see cref="PackageContainer"/> that contains the not referenced Durian packages.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="packages"/> is <see langword="null"/>.</exception>
		public static PackageContainer GetNotReferencedPackages(PackageContainer packages)
		{
			return Assembly.GetCallingAssembly().GetNotReferencedPackages(packages);
		}

		/// <summary>
		/// Returns a collection of all Durian packages present in the provided array of <paramref name="packages"/> that are not referenced by the calling <see cref="Assembly"/>.
		/// </summary>
		/// <param name="packages">Array of <see cref="PackageIdentity"/>s to pick from.</param>
		/// <returns>A new instance of <see cref="PackageContainer"/> that contains the not referenced Durian packages.</returns>
		public static PackageContainer GetNotReferencedPackages(params PackageIdentity[]? packages)
		{
			return Assembly.GetCallingAssembly().GetNotReferencedPackages(packages);
		}

		/// <summary>
		/// Returns a collection of all Durian packages present in the provided array of <paramref name="packages"/> that are not referenced by the calling <see cref="Assembly"/>.
		/// </summary>
		/// <param name="packages">Array of <see cref="DurianPackage"/>s to pick the not referenced packages from.</param>
		/// <returns>A new instance of <see cref="PackageContainer"/> that contains the not referenced Durian packages.</returns>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianPackage"/> value detected.</exception>
		public static PackageContainer GetNotReferencedPackages(params DurianPackage[]? packages)
		{
			return Assembly.GetCallingAssembly().GetNotReferencedPackages(packages);
		}

		/// <summary>
		/// Returns a new instance of <see cref="PackageIdentity"/> of Durian package with the specified <paramref name="packageName"/>.
		/// </summary>
		/// <param name="packageName">Name of the Durian package to get the <see cref="PackageIdentity"/> of.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="packageName"/> is <see langword="null"/> or empty. -or-
		/// Unknown Durian package name: <paramref name="packageName"/>.
		/// </exception>
		public static PackageIdentity GetPackage(string packageName)
		{
			DurianPackage package = Parse(packageName);
			return GetPackage(package);
		}

		/// <summary>
		/// Returns the <see cref="PackageType"/> associated with a <see cref="DurianPackage"/> with the specified <paramref name="packageName"/>.
		/// </summary>
		/// <param name="packageName">Name of <see cref="DurianPackage"/> to get the <see cref="PackageType"/> associated with.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="packageName"/> is <see langword="null"/> or empty. -or-
		/// Unknown Durian package name: <paramref name="packageName"/>.
		/// </exception>
		public static PackageType GetPackageType(string packageName)
		{
			DurianPackage package = Parse(packageName);
			return GetPackageType(package);
		}

		/// <summary>
		/// Returns a new instance of <see cref="PackageReference"/> that represents an in-direct reference to a <see cref="PackageIdentity"/>.
		/// </summary>
		/// <param name="packageName">Name of the Durian package to get a <see cref="PackageReference"/> to.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="packageName"/> is <see langword="null"/> or empty. -or-
		/// Unknown Durian package name: <paramref name="packageName"/>.
		/// </exception>
		public static PackageReference GetReference(string packageName)
		{
			DurianPackage package = Parse(packageName);
			return new PackageReference(package);
		}

		/// <summary>
		/// Returns a new instance of <see cref="PackageReference"/> that represents an in-direct reference to a <see cref="PackageIdentity"/>.
		/// </summary>
		/// <param name="package"><see cref="DurianPackage"/> to get a <see cref="PackageReference"/> to.</param>
		/// <exception cref="ArgumentException">Unknown <see cref="DurianPackage"/> value detected.</exception>
		public static PackageReference GetReference(DurianPackage package)
		{
			return new PackageReference(package);
		}

		/// <summary>
		/// Returns a collection of all Durian packages that are referenced by the calling <see cref="Assembly"/>.
		/// </summary>
		/// <returns>A new instance of <see cref="PackageContainer"/> that contains the referenced Durian packages.</returns>
		public static PackageContainer GetReferencedPackages()
		{
			return Assembly.GetCallingAssembly().GetReferencedPackages();
		}

		/// <summary>
		/// Returns a collection of all Durian packages present in the provided collection of <paramref name="packages"/> that are referenced by the calling <see cref="Assembly"/>.
		/// </summary>
		/// <param name="packages"><see cref="PackageContainer"/> that provides a collection of Durian packages to pick from.</param>
		/// <returns>A new instance of <see cref="PackageContainer"/> that contains the referenced Durian packages.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="packages"/> is <see langword="null"/>.</exception>
		public static PackageContainer GetReferencedPackages(PackageContainer packages)
		{
			return Assembly.GetCallingAssembly().GetReferencedPackages(packages);
		}

		/// <summary>
		/// Returns a collection of all Durian packages present in the provided array of <paramref name="packages"/> that are referenced by the calling <see cref="Assembly"/>.
		/// </summary>
		/// <param name="packages">Array of <see cref="PackageIdentity"/>s to pick from.</param>
		/// <returns>A new instance of <see cref="PackageContainer"/> that contains the referenced Durian packages.</returns>
		public static PackageContainer GetReferencedPackages(params PackageIdentity[]? packages)
		{
			return Assembly.GetCallingAssembly().GetReferencedPackages(packages);
		}

		/// <summary>
		/// Returns a collection of all Durian packages present in the provided array of <paramref name="references"/> that are referenced by the calling <see cref="Assembly"/>.
		/// </summary>
		/// <param name="references">Array of <see cref="PackageReference"/>s to pick from.</param>
		/// <returns>A new instance of <see cref="PackageContainer"/> that contains the referenced Durian packages.</returns>
		public static PackageContainer GetReferencedPackages(params PackageReference[]? references)
		{
			return Assembly.GetCallingAssembly().GetReferencedPackages(references);
		}

		/// <summary>
		/// Returns a collection of all Durian packages present in the provided array of <paramref name="packages"/> that are referenced by the calling <see cref="Assembly"/>.
		/// </summary>
		/// <param name="packages">Array of <see cref="DurianPackage"/>s to pick the referenced packages from.</param>
		/// <returns>A new instance of <see cref="PackageContainer"/> that contains the referenced Durian packages.</returns>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianPackage"/> value detected.</exception>
		public static PackageContainer GetReferencedPackages(params DurianPackage[]? packages)
		{
			return Assembly.GetCallingAssembly().GetReferencedPackages(packages);
		}

		/// <summary>
		/// Determines whether the calling <see cref="Assembly"/> references the provided <paramref name="package"/>.
		/// </summary>
		/// <param name="package"><see cref="PackageReference"/> of Durian package to check for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="package"/> is <see langword="null"/>.</exception>
		public static bool HasReference(PackageReference package)
		{
			return Assembly.GetCallingAssembly().HasReference(package);
		}

		/// <summary>
		/// Determines whether the specified calling <see cref="Assembly"/> references the provided <paramref name="package"/>.
		/// </summary>
		/// <param name="package"><see cref="PackageIdentity"/> representing a Durian package to check for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="package"/> is <see langword="null"/>.</exception>
		public static bool HasReference(PackageIdentity package)
		{
			return Assembly.GetCallingAssembly().HasReference(package);
		}

		/// <summary>
		/// Determines whether the calling <see cref="Assembly"/> references the provided <paramref name="package"/>.
		/// </summary>
		/// <param name="package"><see cref="DurianPackage"/> representing a Durian package to check for.</param>
		/// <exception cref="ArgumentException">Unknown <see cref="DurianPackage"/> value: <paramref name="package"/>.</exception>
		public static bool HasReference(DurianPackage package)
		{
			return Assembly.GetCallingAssembly().HasReference(package);
		}

		/// <summary>
		/// Determines whether the calling <see cref="Assembly"/> references a Durian package with the given <paramref name="packageName"/>.
		/// </summary>
		/// <param name="packageName">Name of the Durian package to check for.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="packageName"/> is <see langword="null"/> or empty. -or-
		/// Unknown Durian package name: <paramref name="packageName"/>.
		/// </exception>
		public static bool HasReference(string packageName)
		{
			return Assembly.GetCallingAssembly().HasReference(packageName);
		}

		/// <summary>
		/// Determines whether the  specified <paramref name="package"/> is a Roslyn-based analyzer package (either <see cref="PackageType.Analyzer"/>,
		/// <see cref="PackageType.StaticGenerator"/>, <see cref="PackageType.SyntaxBasedGenerator"/> or <see cref="PackageType.FileBasedGenerator"/>).
		/// </summary>
		/// <exception cref="ArgumentException">Unknown <see cref="DurianPackage"/> value: <paramref name="package"/>.</exception>
		public static bool IsAnalyzerPackage(DurianPackage package)
		{
			PackageType type = GetPackageType(package);
			return IsAnalyzerPackage(type);
		}

		/// <summary>
		/// Determines whether a <see cref="DurianPackage"/> with the specified <paramref name="packageName"/> is a Roslyn-based analyzer package (either <see cref="PackageType.Analyzer"/>,
		/// <see cref="PackageType.StaticGenerator"/>, <see cref="PackageType.SyntaxBasedGenerator"/> or <see cref="PackageType.FileBasedGenerator"/>).
		/// </summary>
		/// <param name="packageName">Name of the Durian package to check.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="packageName"/> is <see langword="null"/> or empty. -or-
		/// Unknown Durian package name: <paramref name="packageName"/>.
		/// </exception>
		public static bool IsAnalyzerPackage(string packageName)
		{
			PackageType type = GetPackageType(packageName);
			return IsAnalyzerPackage(type);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> represents a Roslyn-based analyzer package (either <see cref="PackageType.Analyzer"/>,
		/// <see cref="PackageType.StaticGenerator"/>, <see cref="PackageType.SyntaxBasedGenerator"/> or <see cref="PackageType.FileBasedGenerator"/>).
		/// </summary>
		/// <param name="type"><see cref="PackageType"/> to check.</param>
		public static bool IsAnalyzerPackage(PackageType type)
		{
			return
				type.HasFlag(PackageType.Analyzer) ||
				type.HasFlag(PackageType.StaticGenerator) ||
				type.HasFlag(PackageType.SyntaxBasedGenerator) ||
				type.HasFlag(PackageType.FileBasedGenerator);
		}

		/// <summary>
		/// Determines whether <see cref="DurianPackage"/> with the given <paramref name="packageName"/> is of the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="packageName">Name of the Durian package to check whether is of the specified <paramref name="type"/>.</param>
		/// <param name="type">Type of the package.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="packageName"/> is <see langword="null"/> or empty. -or-
		/// Unknown Durian package name: <paramref name="packageName"/>.
		/// </exception>
		public static bool IsPackageType(string packageName, PackageType type)
		{
			DurianPackage package = Parse(packageName);
			return IsPackageType(package, type);
		}

		/// <summary>
		/// Converts the specified <paramref name="packageName"/> into a value of the <see cref="DurianPackage"/> enum.
		/// </summary>
		/// <param name="packageName"><see cref="string"/> to convert to a value of the <see cref="DurianPackage"/> enum.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="packageName"/> is <see langword="null"/> or empty. -or-
		/// Unknown Durian package name: <paramref name="packageName"/>.
		/// </exception>
		public static DurianPackage Parse(string packageName)
		{
			if (!TryParse(packageName, out DurianPackage package))
			{
				if (string.IsNullOrWhiteSpace(packageName))
				{
					throw new ArgumentException($"$'{nameof(packageName)}' cannot be null or empty", nameof(packageName));
				}

				throw new ArgumentException($"Unknown Durian package name: {packageName}", nameof(packageName));
			}

			return package;
		}

		/// <summary>
		/// Attempts to return a <see cref="PackageIdentity"/> of Durian package with the specified <paramref name="package"/>.
		/// </summary>
		/// <param name="packageName">Name of the Durian package to get the <see cref="PackageIdentity"/> of.</param>
		/// <param name="package"><see cref="PackageIdentity"/> that was returned.</param>
		public static bool TryGetPackage([NotNullWhen(true)] string? packageName, [NotNullWhen(true)] out PackageIdentity? package)
		{
			if (!TryParse(packageName, out DurianPackage p))
			{
				package = null;
				return false;
			}

			package = GetPackage(p);
			return true;
		}

		/// <summary>
		/// Attempts to return a new instance of <see cref="PackageReference"/> that represents an in-direct reference to a <see cref="PackageIdentity"/>.
		/// </summary>
		/// <param name="packageName">Name of the Durian module to get a <see cref="PackageReference"/> to.</param>
		/// <param name="reference">Newly-created <see cref="PackageReference"/>.</param>
		public static bool TryGetReference([NotNullWhen(true)] string? packageName, [NotNullWhen(true)] out PackageReference? reference)
		{
			if (!TryParse(packageName, out DurianPackage package))
			{
				reference = null;
				return false;
			}

			reference = new PackageReference(package);
			return true;
		}

		internal static void EnsureIsValidPackageEnum_InvOp(DurianPackage package)
		{
			if (package == DurianPackage.None)
			{
				throw new InvalidOperationException($"{nameof(DurianPackage)}.{nameof(DurianPackage.None)} is not a valid Durian package");
			}

			if (!GlobalInfo.IsValidPackageValue(package))
			{
				throw new InvalidOperationException($"Unknown {nameof(DurianPackage)} value: {package}");
			}
		}
	}
}
