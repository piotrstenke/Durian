namespace Durian.Info;

/// <summary>
/// Contains information about the Durian project.
/// </summary>
public static class GlobalInfo
{
	/// <summary>
	/// A <see cref="string"/> that represents the three-letter prefix of each diagnostic id.
	/// </summary>
	public static string IdPrefix => "DUR";

	/// <summary>
	/// Specifies the maximal valid value of the <see cref="DurianModule"/> enum.
	/// </summary>
	public static DurianModule ModuleMax => DurianModule.CopyFrom;

	/// <summary>
	/// Specifies the minimal valid value of the <see cref="DurianModule"/> enum.
	/// </summary>
	public static DurianModule ModuleMin => DurianModule.Core;

	/// <summary>
	/// Number of published Durian analyzer or source generator packages, excluding <c>Durian.Manager</c>.
	/// </summary>
	public static int NumAnalyzerPackages => 6;

	/// <summary>
	/// Number of published Durian modules.
	/// </summary>
	public static int NumModules => 6;

	/// <summary>
	/// Number of published Durian packages.
	/// </summary>
	public static int NumPackages => 10;

	/// <summary>
	/// Specifies the maximal valid value of the <see cref="DurianPackage"/> enum.
	/// </summary>
	public static DurianPackage PackageMax => DurianPackage.CopyFrom;

	/// <summary>
	/// Specifies the minimal valid value of the <see cref="DurianPackage"/> enum.
	/// </summary>
	public static DurianPackage PackageMin => DurianPackage.Main;

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
