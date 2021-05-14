using System.Diagnostics.CodeAnalysis;
using Durian.Generator;
using Durian.Generator.DefaultParam;
using Durian.Info;

#region Durian Configuration

[assembly: ModuleDefinition(DurianModule.DefaultParam, ModuleType.SyntaxBasedGenerator | ModuleType.Analyzer, "1.0.0", 01)]
[assembly: IncludeDiagnostics("DUR0001")]
[assembly: IncludeTypes("DefaultParamAttribute", "DefaultParamConfigurationAttribute", "DPMethodConvention", "DPTypeConvention")]
[assembly: DiagnosticFiles(nameof(DefaultParamDiagnostics))]

#endregion

#region SuppressMessage
[assembly: SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1025:Configure generated code analysis", Justification = "Abstract class Durian.Analyzers.DurianAnayzer configured generated code analysis in its Initialize(context) method.")]
[assembly: SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1026:Enable concurrent execution", Justification = "Abstract class Durian.Analyzers.DurianAnalyzer enables concurrent execution in its Initialize(context) method.")]
[assembly: SuppressMessage("MicrosoftCodeAnalysisPerformance", "RS1012:Start action has no registered actions.", Justification = "The RegisterCompilationStartAction() of the Durian.Analyzers.DurianAnalyzer abstract class calls an abstract method instead of directly registering actions.")]
#endregion
