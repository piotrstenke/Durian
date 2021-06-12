// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Info
{
	/// <summary>
	/// Contains information about the Durian project.
	/// </summary>
	public static class DurianInfo
	{
		/// <summary>
		/// Specifies the maximal valid value of the <see cref="DurianModule"/> enum.
		/// </summary>
		public const DurianModule ModuleMax = DurianModule.DefaultParam;

		/// <summary>
		/// Specifies the minimal valid value of the <see cref="DurianModule"/> enum.
		/// </summary>
		public const DurianModule ModuleMin = DurianModule.Core;

		/// <summary>
		/// Number of Durian modules published.
		/// </summary>
		public const int NumModules = 2;

		/// <summary>
		/// Number of Durian packages published.
		/// </summary>
		public const int NumPackages = 5;

		/// <summary>
		/// Specifies the maximal valid value of the <see cref="DurianPackage"/> enum.
		/// </summary>
		public const DurianPackage PackageMax = DurianPackage.DefaultParam;

		/// <summary>
		/// Specifies the minimal valid value of the <see cref="DurianPackage"/> enum.
		/// </summary>
		public const DurianPackage PackageMin = DurianPackage.Core;

		/// <summary>
		/// A <see cref="string"/> that represents the three-letter prefix of each diagnostic id.
		/// </summary>
		public static string IdPrefix => "DUR";

		/// <summary>
		/// Link to the Durian repository.
		/// </summary>
		public static string Repository => "https://github.com/piotrstenke/Durian";
	}
}
