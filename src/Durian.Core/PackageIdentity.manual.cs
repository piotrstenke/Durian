using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Durian.Info;

public partial class PackageIdentity
{
	private const int _maxPackageType = (int)PackageType.CodeFixLibrary;

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
			DurianPackage.FriendClass => PackageRepository.FriendClass,
			DurianPackage.InterfaceTargets => PackageRepository.InterfaceTargets,
			DurianPackage.CopyFrom => PackageRepository.CopyFrom,
			_ => throw new InvalidOperationException($"Unknown {nameof(DurianPackage)} value: {package}!")
		};
	}

	/// <summary>
	/// Returns the <see cref="PackageType"/> associated with the specified <paramref name="package"/>.
	/// </summary>
	/// <param name="package"><see cref="DurianPackage"/> to get the <see cref="PackageType"/> associated with.</param>
	/// <exception cref="ArgumentException">Unknown <see cref="DurianPackage"/> value: <paramref name="package"/>.</exception>
	public static PackageType GetPackageType(DurianPackage package)
	{
		EnsureIsValidPackageEnum(package);

		return package switch
		{
			DurianPackage.Main => PackageType.Unspecified,
			DurianPackage.Core => PackageType.Library,
			DurianPackage.CoreAnalyzer => PackageType.Analyzer,
			DurianPackage.AnalysisServices => PackageType.Library,
			DurianPackage.TestServices => PackageType.Library,
			DurianPackage.DefaultParam => PackageType.Analyzer | PackageType.StaticGenerator | PackageType.SyntaxBasedGenerator | PackageType.CodeFixLibrary,
			DurianPackage.FriendClass => PackageType.Analyzer | PackageType.StaticGenerator | PackageType.CodeFixLibrary,
			DurianPackage.InterfaceTargets => PackageType.Analyzer | PackageType.StaticGenerator,
			DurianPackage.CopyFrom => PackageType.Analyzer | PackageType.StaticGenerator | PackageType.SyntaxBasedGenerator | PackageType.CodeFixLibrary,
			_ => PackageType.Unspecified,
		};
	}

	/// <summary>
	/// Determines whether the given Durian <paramref name="package"/> is of the specified <paramref name="type"/>.
	/// </summary>
	/// <param name="package"><see cref="DurianPackage"/> to check whether is of the specified <paramref name="type"/>.</param>
	/// <param name="type">Type of the package.</param>
	public static bool IsPackageType(DurianPackage package, PackageType type)
	{
		if (type <= PackageType.Unspecified)
		{
			return false;
		}

		if (IsSingleFlag(type))
		{
			return IsFlag(package, type);
		}

		IEnumerable<PackageType> flags = Enum.GetValues(typeof(PackageType))
			.Cast<PackageType>()
			.Skip(1)
			.Where(flag => type.HasFlag(flag));

		foreach (PackageType packageType in flags)
		{
			if (!IsFlag(package, packageType))
			{
				return false;
			}
		}

		return true;

		static bool IsSingleFlag(PackageType type)
		{
			int x = (int)type;

			return (x & (x - 1)) == 0 && x <= _maxPackageType;
		}

		static bool IsFlag(DurianPackage package, PackageType type)
		{
			return type switch
			{
				PackageType.Library => package is
					DurianPackage.Core or
					DurianPackage.AnalysisServices or
					DurianPackage.TestServices,

				PackageType.Analyzer => package is
					DurianPackage.CoreAnalyzer or
					DurianPackage.DefaultParam or
					DurianPackage.InterfaceTargets or
					DurianPackage.FriendClass or
					DurianPackage.CopyFrom,

				PackageType.StaticGenerator => package is
					DurianPackage.DefaultParam or
					DurianPackage.InterfaceTargets or
					DurianPackage.FriendClass or
					DurianPackage.CopyFrom,

				PackageType.SyntaxBasedGenerator => package is
					DurianPackage.DefaultParam or
					DurianPackage.CopyFrom,

				//PackageType.FileBasedGenerator => package is

				PackageType.CodeFixLibrary => package is
					DurianPackage.DefaultParam or
					DurianPackage.CopyFrom or
					DurianPackage.FriendClass,

				_ => false
			};
		}
	}

	/// <summary>
	/// Attempts to return a name of the specified <paramref name="package"/>.
	/// </summary>
	/// <param name="package"><see cref="DurianPackage"/> to get the name of.</param>
	/// <param name="packageName">Name of the package.</param>
	public static bool TryGetName(DurianPackage package, [NotNullWhen(true)] out string? packageName)
	{
		packageName = package switch
		{
			DurianPackage.Main => PackageNames.Main,
			DurianPackage.Core => PackageNames.Core,
			DurianPackage.CoreAnalyzer => PackageNames.CoreAnalyzer,
			DurianPackage.AnalysisServices => PackageNames.AnalysisServices,
			DurianPackage.TestServices => PackageNames.TestServices,
			DurianPackage.DefaultParam => PackageNames.DefaultParam,
			DurianPackage.FriendClass => PackageNames.FriendClass,
			DurianPackage.InterfaceTargets => PackageNames.InterfaceTargets,
			DurianPackage.CopyFrom => PackageNames.CopyFrom,
			_ => null
		};

		return packageName is not null;
	}

	/// <summary>
	/// Attempts to convert the specified <paramref name="packageName"/> into a value of the <see cref="DurianPackage"/> enum.
	/// </summary>
	/// <param name="packageName"><see cref="string"/> to convert to a value of the <see cref="DurianPackage"/> enum.</param>
	/// <param name="package">Value of the <see cref="DurianPackage"/> enum created from the <paramref name="packageName"/>.</param>
	public static bool TryParse([NotNullWhen(true)] string? packageName, out DurianPackage package)
	{
		if (string.IsNullOrWhiteSpace(packageName))
		{
			package = default;
			return false;
		}

		string name = Utilities.GetParsableIdentityName(packageName!);

		// Switch expression gives a compilation error here, weird.

		if (name.Equals(PackageNames.Main, StringComparison.OrdinalIgnoreCase))
		{
			package = DurianPackage.Main;
			return true;
		}

		if (name.Equals(PackageNames.Core, StringComparison.OrdinalIgnoreCase))
		{
			package = DurianPackage.Core;
			return true;
		}

		if (name.Equals(PackageNames.CoreAnalyzer, StringComparison.OrdinalIgnoreCase))
		{
			package = DurianPackage.CoreAnalyzer;
			return true;
		}

		if (name.Equals(PackageNames.AnalysisServices, StringComparison.OrdinalIgnoreCase))
		{
			package = DurianPackage.AnalysisServices;
			return true;
		}

		if (name.Equals(PackageNames.TestServices, StringComparison.OrdinalIgnoreCase))
		{
			package = DurianPackage.TestServices;
			return true;
		}

		if (name.Equals(PackageNames.DefaultParam, StringComparison.OrdinalIgnoreCase))
		{
			package = DurianPackage.DefaultParam;
			return true;
		}

		if (name.Equals(PackageNames.FriendClass, StringComparison.OrdinalIgnoreCase))
		{
			package = DurianPackage.FriendClass;
			return true;
		}

		if (name.Equals(PackageNames.InterfaceTargets, StringComparison.OrdinalIgnoreCase))
		{
			package = DurianPackage.InterfaceTargets;
			return true;
		}

		if (name.Equals(PackageNames.CopyFrom, StringComparison.OrdinalIgnoreCase))
		{
			package = DurianPackage.CopyFrom;
			return true;
		}

		package = default;
		return false;
	}
}
