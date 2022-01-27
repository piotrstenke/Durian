// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

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
				throw new InvalidOperationException($"Unknown {nameof(DurianPackage)} value: {package}!");
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
		/// Returns a new instance of <see cref="PackageIdentity"/> corresponding with the specified <see cref="DurianPackage"/>.
		/// </summary>
		/// <param name="package"><see cref="DurianPackage"/> to get the <see cref="PackageIdentity"/> of.</param>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianPackage"/> value detected.</exception>
		public static PackageIdentity GetPackage(DurianPackage package)
		{
			return package switch
			{
				DurianPackage.Main => PackageRepository.Main,
				DurianPackage.AnalysisServices => PackageRepository.AnalysisServices,
				DurianPackage.Core => PackageRepository.Core,
				DurianPackage.CoreAnalyzer => PackageRepository.CoreAnalyzer,
				DurianPackage.DefaultParam => PackageRepository.DefaultParam,
				DurianPackage.TestServices => PackageRepository.TestServices,
				DurianPackage.Info => PackageRepository.Info,
				DurianPackage.FriendClass => PackageRepository.FriendClass,
				DurianPackage.InterfaceTargets => PackageRepository.InterfaceTargets,
				DurianPackage.Manager => PackageRepository.Manager,
				_ => throw new InvalidOperationException($"Unknown {nameof(DurianPackage)} value: {package}!")
			};
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
			DurianPackage package = ParsePackage(packageName);
			return GetPackage(package);
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
			DurianPackage package = ParsePackage(packageName);
			return new PackageReference(package);
		}

		/// <summary>
		/// Returns a new instance of <see cref="PackageReference"/> that represents an in-direct reference to a <see cref="PackageIdentity"/>.
		/// </summary>
		/// <param name="package"><see cref="DurianPackage"/> to get a <see cref="PackageReference"/> to.</param>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianPackage"/> value detected.</exception>
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
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianPackage"/> value detected.</exception>
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
		/// Determines whether the specified <paramref name="package"/> is a Roslyn-based analyzer package.
		/// </summary>
		/// <param name="package"><see cref="DurianPackage"/> to check.</param>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianPackage"/> value detected.</exception>
		public static bool IsAnalyzerPackage(DurianPackage package)
		{
			PackageIdentity identity = GetPackage(package);
			return IsAnalyzerPackage(identity.Type);
		}

		/// <summary>
		/// Determines whether <see cref="DurianPackage"/> with the specified <paramref name="packageName"/> is a Roslyn-based analyzer package.
		/// </summary>
		/// <param name="packageName">Name of the Durian package to check.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="packageName"/> is <see langword="null"/> or empty. -or-
		/// Unknown Durian package name: <paramref name="packageName"/>.
		/// </exception>
		public static bool IsAnalyzerPackage(string packageName)
		{
			PackageIdentity identity = GetPackage(packageName);
			return IsAnalyzerPackage(identity.Type);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> represents a Roslyn-based analyzer package (either <see cref="PackageType.Analyzer"/>, <see cref="PackageType.StaticGenerator"/>, <see cref="PackageType.SyntaxBasedGenerator"/> or <see cref="PackageType.FileBasedGenerator"/>).
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
		/// Converts the specified <paramref name="package"/> value into a <see cref="string"/> value.
		/// </summary>
		/// <param name="package"><see cref="DurianPackage"/> to convert into a <see cref="string"/>.</param>
		/// <exception cref="ArgumentException">Unknown <see cref="DurianPackage"/> value: <paramref name="package"/>.</exception>
		public static string PackageToString(DurianPackage package)
		{
			EnsureIsValidPackageEnum(package);

			return package switch
			{
				DurianPackage.CoreAnalyzer => "Durian.Core.Analyzer",
				DurianPackage.Main => "Durian",
				_ => $"Durian.{package}"
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="packageName"/> into a value of the <see cref="DurianPackage"/> enum.
		/// </summary>
		/// <param name="packageName"><see cref="string"/> to convert to a value of the <see cref="DurianPackage"/> enum.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="packageName"/> is <see langword="null"/> or empty. -or-
		/// Unknown Durian package name: <paramref name="packageName"/>.
		/// </exception>
		public static DurianPackage ParsePackage(string packageName)
		{
			if (!TryParsePackage(packageName, out DurianPackage package))
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
			if (!TryParsePackage(packageName, out DurianPackage p))
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
			if (!TryParsePackage(packageName, out DurianPackage package))
			{
				reference = null;
				return false;
			}

			reference = new PackageReference(package);
			return true;
		}

		/// <summary>
		/// Attempts to convert the specified <paramref name="packageName"/> into a value of the <see cref="DurianPackage"/> enum.
		/// </summary>
		/// <param name="packageName"><see cref="string"/> to convert to a value of the <see cref="DurianPackage"/> enum.</param>
		/// <param name="package">Value of the <see cref="DurianPackage"/> enum created from the <paramref name="packageName"/>.</param>
		public static bool TryParsePackage([NotNullWhen(true)]string? packageName, out DurianPackage package)
		{
			if (string.IsNullOrWhiteSpace(packageName))
			{
				package = default;
				return false;
			}

			string name = Utilities.DurianRegex.Replace(packageName, "");

			return Enum.TryParse(name, true, out package);
		}
	}
}