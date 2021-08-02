// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis;
using Durian.Info;
using Microsoft.CodeAnalysis;

namespace Durian.Manager
{
	internal static class DurianManagerUtilities
	{
		public static bool ShouldAnalyze(Compilation compilation, DurianModule module)
		{
			return
				IsCSharpCompilationAnalyzer.Analyze(compilation) &&
				DisabledModuleAnalyzer.IsEnabled(module, compilation) &&
				DependencyAnalyzer.Analyze(compilation);
		}
	}
}
