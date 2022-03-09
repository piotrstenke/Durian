// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Newtonsoft.Json;

[JsonObject]
internal sealed class ModuleData
{
	[JsonProperty("diagnosticFiles")]
	public string[]? DiagnosticFiles { get; set; }

	[JsonProperty("diagnosticIdPrefix")]
	public string? DiagnosticIdPrefix { get; set; }

	[JsonProperty("documentation")]
	public string? Documentation { get; set; }

	[JsonProperty("externalDiagnostics")]
	public string[]? ExternalDiagnostics { get; set; }

	[JsonProperty("includedTypes")]
	public string[]? IncludedTypes { get; set; }

	[JsonProperty("name", Required = Required.Always)]
	public string? Name { get; set; }

	[JsonProperty("packages", Required = Required.Always)]
	public PackageData[]? Packages { get; set; }
}