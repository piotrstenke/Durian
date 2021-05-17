using System.Diagnostics.CodeAnalysis;
using Durian.Generator;
using Durian.Info;

#region Durian Configuration

[assembly: PackageDefinition(DurianModule.Core, DurianPackage.CoreAnalyzer, PackageType.Analyzer, "1.0.0")]
[assembly: DiagnosticFiles(nameof(DurianDiagnostics))]
[assembly: IncludeTypes(nameof(IncludeTypesAttribute), nameof(IncludeDiagnosticsAttribute), nameof(EnableModuleAttribute), nameof(DiagnosticFilesAttribute), nameof(PackageDefinitionAttribute))]

#endregion

#region SupressMessages

[assembly: SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1025:Configure generated code analysis", Justification = "Abstract class Durian.Analyzers.DurianAnayzer configured generated code analysis in its Initialize(context) method.")]
[assembly: SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1026:Enable concurrent execution", Justification = "Abstract class Durian.Analyzers.DurianAnalyzer enables concurrent execution in its Initialize(context) method.")]

#endregion
