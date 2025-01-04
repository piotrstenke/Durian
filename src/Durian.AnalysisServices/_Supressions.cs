using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1025:Configure generated code analysis", Justification = "Abstract class Durian.Analyzers.DurianAnayzer configured generated code analysis in its Initialize(context) method.")]
[assembly: SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1026:Enable concurrent execution", Justification = "Abstract class Durian.Analyzers.DurianAnalyzer enables concurrent execution in its Initialize(context) method.")]
[assembly: SuppressMessage("MicrosoftCodeAnalysisPerformance", "RS1012:Start action has no registered actions.", Justification = "The RegisterCompilationStartAction() of the Durian.Analyzers.DurianAnalyzer abstract class calls an abstract method instead of directly registering actions.")]
