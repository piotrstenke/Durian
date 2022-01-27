// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

internal class IncludedType
{
	public List<string> ModuleNames { get; }

	public string Name { get; }

	public string Namespace { get; }

	public IncludedType(string name, string @namespace)
	{
		Name = name;
		Namespace = @namespace;
		ModuleNames = new(8);
	}

	public void TryAddModules(string[] modules)
	{
		foreach (string module in modules)
		{
			if (!ModuleNames.Contains(module))
			{
				ModuleNames.Add(module);
			}
		}
	}
}