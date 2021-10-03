// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian;
using Durian.Analysis.EnumServices;
using Durian.Generator;
using Durian.Info;

[assembly: PackageDefinition(DurianPackage.EnumServices, PackageType.Analyzer | PackageType.SyntaxBasedGenerator, "1.0.0", DurianModule.EnumServices)]
[assembly: DiagnosticFiles(nameof(EnumServicesDiagnostics))]
[assembly: IncludeTypes(
	typeof(EnumServicesAttribute),
	typeof(EnumServices),
	typeof(GeneratedTypeAccess)
)]
