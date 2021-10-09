// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Durian.Analysis;
using Durian.Generator;
using Durian.Info;
using Durian;
using Durian.Analysis.FriendClass;

[assembly: PackageDefinition(DurianPackage.FriendClass, PackageType.Analyzer | PackageType.CodeFixLibrary, "1.0.0", DurianModule.FriendClass)]
[assembly: DiagnosticFiles(nameof(FriendClassDiagnostics))]
[assembly: IncludeTypes(
	"FriendClassAttribute",
	"FriendClassConfigurationAttribute"
)]
