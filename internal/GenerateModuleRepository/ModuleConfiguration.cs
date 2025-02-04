﻿using System.Collections.Generic;

internal sealed class ModuleConfiguration
{
	public List<DiagnosticData>? Diagnostics { get; set; }
	public List<DiagnosticData>? ExternalDiagnostics { get; set; }
	public List<TypeData>? IncludedTypes { get; set; }
	public ModuleData? Module { get; set; }
}
