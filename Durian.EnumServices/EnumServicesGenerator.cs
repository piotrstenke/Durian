// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Durian.Analysis.Cache;
using Durian.Analysis.Logging;

namespace Durian.Analysis.EnumServices
{
#if !MAIN_PACKAGE

	[Generator(LanguageNames.CSharp)]
#endif

	[GeneratorLoggingConfiguration(SupportedLogs = GeneratorLogs.All, LogDirectory = "EnumServices", SupportsDiagnostics = true, RelativeToGlobal = true, EnableExceptions = true)]
	public class EnumServicesGenerator : CachedDurianGenerator<EnumServicesData, EnumServicesCompilationData>
	{
	}
}
