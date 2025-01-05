using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Durian.CopyFrom")]
[assembly: InternalsVisibleTo("Durian.DefaultParam")]
[assembly: InternalsVisibleTo("Durian.InterfaceTargets")]
[assembly: InternalsVisibleTo("Durian.FriendClass")]
[assembly: InternalsVisibleTo("Durian.Core.Analyzer")]
[assembly: InternalsVisibleTo("Durian.TestServices")]
[assembly: InternalsVisibleTo("Durian.GlobalScope")]

[assembly: SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1025:Configure generated code analysis", Justification = "Abstract class Durian.Analyzers.DurianAnayzer configured generated code analysis in its Initialize(context) method.")]
[assembly: SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1026:Enable concurrent execution", Justification = "Abstract class Durian.Analyzers.DurianAnalyzer enables concurrent execution in its Initialize(context) method.")]
[assembly: SuppressMessage("MicrosoftCodeAnalysisPerformance", "RS1012:Start action has no registered actions.", Justification = "The RegisterCompilationStartAction() of the Durian.Analyzers.DurianAnalyzer abstract class calls an abstract method instead of directly registering actions.")]
[assembly: SuppressMessage("Roslynator", "RCS1173:Use coalesce expression instead of 'if'", Justification = "<Pending>")]
