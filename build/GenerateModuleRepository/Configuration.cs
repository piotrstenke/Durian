internal class Configuration
{
	private readonly PackageDefinition _def;

	public string ModuleName => _def.ModuleName;
	public string PackageName => _def.PackageName;
	public string PackageType => _def.PackageType;
	public string Version => _def.Version;
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
