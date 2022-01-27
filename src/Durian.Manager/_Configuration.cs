// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian;
using Durian.Generator;
using Durian.Info;

[assembly: PackageDefinition(DurianPackage.Manager, PackageType.Analyzer, "1.1.0", DurianModule.Core)]
[assembly: IncludeTypes("DisableModuleAttribute")]