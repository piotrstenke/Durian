﻿using System.Diagnostics.CodeAnalysis;
using Durian.Logging;

[assembly: GlobalGeneratorLoggingConfiguration(LogDirectory = "/DefaultParam", RelativeToDefault = true)]

#region SuppressMessage
[assembly: SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1025:Configure generated code analysis", Justification = "Abstract class Durian.Analyzers.DurianAnayzer configured generated code analysis in its Initialize(context) method.")]
[assembly: SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1026:Enable concurrent execution", Justification = "Abstract class Durian.Analyzers.DurianAnalyzer enables concurrent execution in its Initialize(context) method.")]
#endregion
