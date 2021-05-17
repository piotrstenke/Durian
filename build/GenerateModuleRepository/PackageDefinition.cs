internal readonly struct PackageDefinition
{
	public string PackageType { get; }
	public string Version { get; }
	public string ModuleName { get; }
	public string PackageName { get; }
	public string[] ExternalDiagnostics { get; }
	public string[] IncludedTypes { get; }
	public string[] DiagnosticFiles { get; }

	public PackageDefinition(string moduleName, string packageName, string packageType, string version, string[] externalDiagnostics, string[] includedTypes, string[] diagnosticFiles)
	{
		ModuleName = moduleName;
		PackageType = packageType;
		PackageName = packageName;
		Version = version;
		ExternalDiagnostics = externalDiagnostics;
		IncludedTypes = includedTypes;
		DiagnosticFiles = diagnosticFiles;
	}
}
