//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the GenerateModuleRepository project.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Durian.Info
{
	/// <summary>
	/// Factory class of <see cref="PackageIdentity"/> for all available Durian packages.
	/// </summary>
	public static class PackageRepository
	{
		/// <summary>
		/// Returns a <see cref="PackageIdentity"/> for the <see cref="DurianPackage.AnalysisServices"/> package.
		/// </summary>
		public static PackageIdentity AnalysisServices => new(
			enumValue: DurianPackage.AnalysisServices,
			version: "1.0.0",
			type: PackageType.Library
		);

		/// <summary>
		/// Returns a <see cref="PackageIdentity"/> for the <see cref="DurianPackage.Core"/> package.
		/// </summary>
		public static PackageIdentity Core => new(
			enumValue: DurianPackage.Core,
			version: "1.0.0",
			type: PackageType.Library
		);

		/// <summary>
		/// Returns a <see cref="PackageIdentity"/> for the <see cref="DurianPackage.CoreAnalyzer"/> package.
		/// </summary>
		public static PackageIdentity CoreAnalyzer => new(
			enumValue: DurianPackage.CoreAnalyzer,
			version: "1.0.0",
			type: PackageType.Analyzer
		);

		/// <summary>
		/// Returns a <see cref="PackageIdentity"/> for the <see cref="DurianPackage.DefaultParam"/> package.
		/// </summary>
		public static PackageIdentity DefaultParam => new(
			enumValue: DurianPackage.DefaultParam,
			version: "1.0.0",
			type: PackageType.SyntaxBasedGenerator | PackageType.Analyzer
		);

		/// <summary>
		/// Returns a <see cref="PackageIdentity"/> for the <see cref="DurianPackage.TestServices"/> package.
		/// </summary>
		public static PackageIdentity TestServices => new(
			enumValue: DurianPackage.TestServices,
			version: "1.0.0",
			type: PackageType.Library
		);
	}
}
