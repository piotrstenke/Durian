// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

#if DEBUG
using Durian;
using Durian.Analysis.FriendClass;
using Durian.Generator;
using Durian.Info;

[assembly: PackageDefinition(DurianPackage.FriendClass, PackageType.Analyzer | PackageType.CodeFixLibrary, "1.0.0", DurianModule.FriendClass)]
[assembly: DiagnosticFiles(nameof(FriendClassDiagnostics))]
[assembly: IncludeTypes(
	"FriendClassAttribute",
	"FriendClassConfigurationAttribute"
)]
#endif