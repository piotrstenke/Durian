// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Durian.Analysis;
using Durian.Generator;
using Microsoft.CodeAnalysis;
using Durian.Info;
using Durian.Analysis.InterfaceTargets;
using Durian;

[assembly: PackageDefinition(DurianPackage.InterfaceTargets, PackageType.Analyzer, "1.0.0", DurianModule.InterfaceTargets)]
[assembly: DiagnosticFiles(nameof(IntfTargDiagnostics))]
[assembly: IncludeTypes(
	typeof(InterfaceTargetsAttribute),
	typeof(InterfaceTargets)
)]
