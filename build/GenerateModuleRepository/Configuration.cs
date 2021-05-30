internal class Configuration
{
	internal readonly PackageDefinition _def;

	public string PackageName => _def.PackageName;
	public string PackageType => _def.PackageType;
	public string Version => _def.Version;
	public string[] Modules => _def.Modules;
	public DiagnosticData[] Diagnostics { get; }
	public IncludedType[] IncludedTypes { get; }
	public DiagnosticData[]? ExternalDiagnostics { get; set; }

	public ref readonly PackageDefinition Definition => ref _def;

	public Configuration(in PackageDefinition definition, DiagnosticData[] diagnostics, IncludedType[] includedTypes)
	{
		_def = definition;
		Diagnostics = diagnostics;
		IncludedTypes = includedTypes;
	}
}
