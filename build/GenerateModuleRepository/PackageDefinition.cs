internal readonly struct PackageDefinition
{
	public string PackageType { get; }
	public string Version { get; }
	public string PackageName { get; }
	public string[] Modules { get; }
	public string[] ExternalDiagnostics { get; }
	public string[] IncludedTypes { get; }
	public string[] DiagnosticFiles { get; }

	public PackageDefinition(string packageName, string packageType, string version, string[] modules, string[] externalDiagnostics, string[] includedTypes, string[] diagnosticFiles)
	{
		Modules = modules;
		PackageType = packageType;
		PackageName = packageName;
		Version = version;
		ExternalDiagnostics = externalDiagnostics;
		IncludedTypes = includedTypes;
		DiagnosticFiles = diagnosticFiles;
	}
}
