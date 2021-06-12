// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Durian.Generator;
using Durian.Generator.Core;
using Durian.Info;

[assembly: PackageDefinition(DurianPackage.CoreAnalyzer, PackageType.Analyzer, "1.1.0", DurianModule.Core)]
[assembly: DiagnosticFiles(nameof(DurianDiagnostics))]
[assembly: IncludeTypes(
	nameof(DurianGeneratedAttribute),
	nameof(EnableModuleAttribute),
	nameof(IncludeTypesAttribute),
	nameof(IncludeDiagnosticsAttribute),
	nameof(DiagnosticFilesAttribute),
	nameof(PackageDefinitionAttribute)
)]
[assembly: SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1025:Configure generated code analysis", Justification = "Abstract class Durian.Analyzers.DurianAnayzer configured generated code analysis in its Initialize(context) method.")]
[assembly: SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1026:Enable concurrent execution", Justification = "Abstract class Durian.Analyzers.DurianAnalyzer enables concurrent execution in its Initialize(context) method.")]
