using System.Collections.Generic;

internal class ModuleData
{
	public string Id { get; }
	public string Name { get; }
	public string Documentation { get; }
	public List<string> Packages { get; }
	public DiagnosticData[] Diagnostics { get; }
	public DiagnosticData[] ExternalDiagnostics { get; }
	public IncludedType[] IncludedTypes { get; }

	public ModuleData(string moduleName, DiagnosticData[] diagnostics, DiagnosticData[] externalDiagnostics, IncludedType[] includedTypes)
	{
		Name = moduleName;
		Diagnostics = diagnostics;
		ExternalDiagnostics = externalDiagnostics;
		IncludedTypes = includedTypes;
		Packages = new(4);
		Documentation = $@"docs\{moduleName}";

		if (diagnostics.Length == 0)
		{
			Id = "default";
		}
		else
		{
			ref readonly DiagnosticData data = ref diagnostics[0];

			if (data.Id.Length < 5)
			{
				Id = "default";
			}
			else
			{
				Id = data.Id.Substring(3, 2);
			}
		}
	}
}
