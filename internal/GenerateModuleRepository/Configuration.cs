// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

internal class Configuration
{
	internal readonly PackageDefinition _def;
	public ref readonly PackageDefinition Definition => ref _def;
	public DiagnosticData[] Diagnostics { get; }
	public DiagnosticData[]? ExternalDiagnostics { get; set; }
	public IncludedType[] IncludedTypes { get; }
	public string[] Modules => _def.Modules;
	public string PackageName => _def.PackageName;
	public string PackageType => _def.PackageType;
	public string Version => _def.Version;

	public Configuration(in PackageDefinition definition, DiagnosticData[] diagnostics, IncludedType[] includedTypes)
	{
		_def = definition;
		Diagnostics = diagnostics;
		IncludedTypes = includedTypes;
	}
}