// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

[JsonObject]
internal sealed class ModuleCollection : IEnumerable<ModuleData>
{
	[JsonProperty("modules", Required = Required.Always)]
	public ModuleData[]? Modules { get; set; }

	public IEnumerator<ModuleData> GetEnumerator()
	{
		return ((IEnumerable<ModuleData>)Modules!).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
