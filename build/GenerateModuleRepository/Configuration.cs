internal readonly struct Configuration
{
	public string ModuleType { get; }
	public string Version { get; }
	public string Id { get; }
	public string ModuleName { get; }
	public string[] ExternalDiagnostics { get; }
	public string[] IncludedTypes { get; }
	public string[] DiagnosticFiles { get; }

	public Configuration(string moduleName, string moduleType, string version, string? id, string[] externalDiagnostics, string[] includedTypes, string[] diagnosticFiles)
	{
		ModuleName = moduleName;
		ModuleType = moduleType;
		Version = version;
		Id = id ?? "default";
		ExternalDiagnostics = externalDiagnostics;
		IncludedTypes = includedTypes;
		DiagnosticFiles = diagnosticFiles;
	}
}
