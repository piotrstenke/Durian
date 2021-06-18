// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Durian;
using Durian.Analysis.DefaultParam;
using Durian.Configuration;
using Durian.Generator;
using Durian.Info;

[assembly: PackageDefinition(DurianPackage.DefaultParam, PackageType.SyntaxBasedGenerator | PackageType.Analyzer | PackageType.CodeFixLibrary, "1.2.0", DurianModule.DefaultParam)]
[assembly: DiagnosticFiles(nameof(DefaultParamDiagnostics))]
[assembly: IncludeTypes(
	nameof(DefaultParamAttribute),
	nameof(DefaultParamConfigurationAttribute),
	nameof(DefaultParamScopedConfigurationAttribute),
	nameof(DPMethodConvention),
	nameof(DPTypeConvention))]
[assembly: SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1025:Configure generated code analysis", Justification = "Abstract class Durian.Analyzers.DurianAnayzer configured generated code analysis in its Initialize(context) method.")]
[assembly: SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1026:Enable concurrent execution", Justification = "Abstract class Durian.Analyzers.DurianAnalyzer enables concurrent execution in its Initialize(context) method.")]
[assembly: SuppressMessage("MicrosoftCodeAnalysisPerformance", "RS1012:Start action has no registered actions.", Justification = "The RegisterCompilationStartAction() of the Durian.Analyzers.DurianAnalyzer abstract class calls an abstract method instead of directly registering actions.")]
