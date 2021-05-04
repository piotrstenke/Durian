namespace Durian.Generator
{
	/// <summary>
	/// Factory class of <see cref="PackageIdentity"/> for all available Durian modules.
	/// </summary>
	public static class PackageFactory
	{
		/// <summary>
		/// Returns a <see cref="PackageIdentity"/> for the <c>Durian.Core</c> module.
		/// </summary>
		public static PackageIdentity Core => new("Durian.Core", "1.0.0", DurianModule.Core, ModuleType.Library);

		/// <summary>
		/// Returns a <see cref="PackageIdentity"/> for the <c>Durian.AnalysisCore</c> module.
		/// </summary>
		public static PackageIdentity AnalysisCore => new("Durian.AnalysisCore", "1.0.0", DurianModule.AnalysisCore, ModuleType.Library);

		/// <summary>
		/// Returns a <see cref="PackageIdentity"/> for the <c>Durian.DefaultParam</c> module.
		/// </summary>
		public static PackageIdentity DefaultParam => new("Durian.DefaultParam", "1.0.0", DurianModule.DefaultParam, ModuleType.SyntaxBasedGenerator, new[]
		{
			new StaticTreeIdentity("DefaultParamAttribute", "Durian", StaticTreeType.Attribute),
			new StaticTreeIdentity("DefaultParamConfigurationAttribute", "Durian.Configuration", StaticTreeType.Attribute),
			new StaticTreeIdentity("DefaultParamMethodConfigurationAttribute", "Durian.Configuration", StaticTreeType.Attribute)
		});
	}
}
