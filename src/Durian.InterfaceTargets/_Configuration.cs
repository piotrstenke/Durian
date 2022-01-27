// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian;
using Durian.Analysis.InterfaceTargets;
using Durian.Generator;
using Durian.Info;

[assembly: PackageDefinition(DurianPackage.InterfaceTargets, PackageType.Analyzer, "1.0.0", DurianModule.InterfaceTargets)]
[assembly: DiagnosticFiles(nameof(IntfTargDiagnostics))]
[assembly: IncludeTypes(
	"InterfaceTargetsAttribute",
	"InterfaceTargets"
)]