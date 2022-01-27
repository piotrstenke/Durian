// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

#if DEBUG
using Durian;
using Durian.Generator;
using Durian.Info;

[assembly: PackageDefinition(DurianPackage.Manager, PackageType.Analyzer, "2.0.0", DurianModule.Core)]
[assembly: IncludeTypes(
	"DisableModuleAttribute",
	"DurianGeneratedAttribute",
	"EnableModuleAttribute"
)]
#endif