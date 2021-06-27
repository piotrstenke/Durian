﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian;
using Durian.Analysis.GenericSpecialization;
using Durian.Configuration;
using Durian.Generator;
using Durian.Info;

[assembly: PackageDefinition(DurianPackage.GenericSpecialization, PackageType.SyntaxBasedGenerator | PackageType.Analyzer | PackageType.CodeFixLibrary, "1.0.0", DurianModule.GenericSpecialization)]
[assembly: DiagnosticFiles(nameof(GenSpecDiagnostics))]
[assembly: IncludeTypes(
	nameof(AllowSpecializationAttribute),
	nameof(GenericSpecializationAttribute),
	nameof(GenericSpecializationConfigurationAttribute),
	nameof(GenSpecImportOptions)
)]
