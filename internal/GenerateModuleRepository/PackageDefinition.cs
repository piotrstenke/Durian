// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

internal readonly struct PackageDefinition
{
	public readonly string[] DiagnosticFiles { get; }

	public readonly string[] ExternalDiagnostics { get; }

	public readonly string[] IncludedTypes { get; }

	public readonly string[] Modules { get; }

	public readonly string PackageName { get; }

	public readonly string PackageType { get; }

	public readonly string Version { get; }

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