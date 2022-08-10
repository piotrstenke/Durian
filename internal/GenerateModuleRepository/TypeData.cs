// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Newtonsoft.Json;

[JsonObject]
internal sealed class TypeData
{
	[JsonProperty("modules", Required = Required.Always)]
	public List<string>? Modules { get; set; }

	[JsonProperty("name", Required = Required.Always)]
	public string? Name { get; set; }

	[JsonProperty("namespace", Required = Required.Always)]
	public string? Namespace { get; set; }
}
