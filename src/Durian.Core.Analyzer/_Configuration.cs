using Durian.Generator;
using Durian.Info;

[assembly: ModuleDefinition(DurianModule.CoreAnalyzer, ModuleType.Analyzer, "1.0.0")]
[assembly: IncludeDiagnostics("DUR0001", "DUR0002")]