using System.Collections.Generic;

internal class ModuleData
{
	public string Name { get; }
	public string Documentation { get; }
	public List<string> Packages { get; }
	public List<DiagnosticData> Diagnostics { get; }
	public List<DiagnosticData> ExternalDiagnostics { get; }
	public List<IncludedType> IncludedTypes { get; }

	public ModuleData(string moduleName)
	{
		Name = moduleName;
		Diagnostics = new(32);
		ExternalDiagnostics = new(8);
		IncludedTypes = new(8);
		Packages = new(4);
		Documentation = $@"docs\{moduleName}";
	}

	public string GetId()
	{
		if (Diagnostics.Count > 0)
		{
			ref readonly DiagnosticData data = ref Diagnostics.ToArray()[0];

			if (data.Id.Length >= 5)
			{
				return data.Id.Substring(3, 2);
			}
		}

		return "default";
	}
}
