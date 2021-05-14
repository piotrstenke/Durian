using System;
using System.Collections.Generic;
using System.Text;
using Durian.Info;
using Durian.Generator;

[assembly: ModuleDefinition(DurianModule.CoreAnalyzer, ModuleType.Analyzer, "1.0.0")]
[assembly: IncludeDiagnostics("DUR0001", "DUR0002")]