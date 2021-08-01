// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Info
{
	/// <summary>
	/// Contains information about the Durian project.
	/// </summary>
	public static class GlobalInfo
	{
		/// <summary>
		/// Specifies the maximal valid value of the <see cref="DurianModule"/> enum.
		/// </summary>
		public const DurianModule ModuleMax = DurianModule.Manager;

		/// <summary>
		/// Specifies the minimal valid value of the <see cref="DurianModule"/> enum.
		/// </summary>
		public const DurianModule ModuleMin = DurianModule.Core;

		/// <summary>
		/// Number of published Durian analyzer or source generator packages.
		/// </summary>
		public const int NumAnalyzerPackages = 5;

		/// <summary>
		/// Number of published Durian modules.
		/// </summary>
		public const int NumModules = 5;

		/// <summary>
		/// Number of published Durian packages.
		/// </summary>
		public const int NumPackages = 9;

		/// <summary>
		/// Specifies the maximal valid value of the <see cref="DurianPackage"/> enum.
		/// </summary>
		public const DurianPackage PackageMax = DurianPackage.Manager;

		/// <summary>
		/// Specifies the minimal valid value of the <see cref="DurianPackage"/> enum.
		/// </summary>
		public const DurianPackage PackageMin = DurianPackage.Main;

		/// <summary>
		/// A <see cref="string"/> that represents the three-letter prefix of each diagnostic id.
		/// </summary>
		public static string IdPrefix => "DUR";

		/// <summary>
		/// Link to the Durian repository.
		/// </summary>
		public static string Repository => "https://github.com/piotrstenke/Durian";

		/// <summary>
		/// Determines whether the specified <paramref name="module"/> is a valid <see cref="DurianModule"/> value.
		/// </summary>
		/// <param name="module"><see cref="DurianModule"/> to check.</param>
		public static bool IsValidModuleValue(DurianModule module)
		{
			return module >= ModuleMin && module <= ModuleMax;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="package"/> is a valid <see cref="DurianPackage"/> value.
		/// </summary>
		/// <param name="package"><see cref="DurianPackage"/> to check.</param>
		public static bool IsValidPackageValue(DurianPackage package)
		{
			return package >= PackageMin && package <= PackageMax;
		}
	}
}
