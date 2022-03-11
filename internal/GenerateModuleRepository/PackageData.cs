// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Newtonsoft.Json;

[JsonObject]
internal sealed class PackageData
{
	[JsonProperty("name", Required = Required.Always)]
	public string? Name { get; set; }

	[JsonProperty("type", Required = Required.Always)]
	public string[]? Type { get; set; }

	[JsonProperty("version", Required = Required.Always)]
	public string? Version { get; set; }
}
