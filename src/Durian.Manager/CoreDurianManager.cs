// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Manager
{
	/// <summary>
	/// Manages the analyzers from the <c>Durian.Core.Analyzer</c> package.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class CoreDurianManager : AnalysisManager, IDurianModuleManager
	{
		/// <inheritdoc/>
		public DurianModule Module => DurianModule.Core;

		/// <summary>
		/// Initializes a new instance of the <see cref="CoreDurianManager"/> class.
		/// </summary>
		public CoreDurianManager()
		{
		}

		/// <inheritdoc/>
		public override bool ShouldAnalyze(Compilation compilation)
		{
			return DurianManagerUtilities.ShouldAnalyze(compilation, Module);
		}

		/// <inheritdoc/>
		protected override IAnalyzerInfo[] GetAnalyzersCore()
		{
			return new IAnalyzerInfo[]
			{
				new IsCSharpCompilationAnalyzer(),
				new DependenciesAnalyzer(),
				new CustomTypesInGeneratorNamespaceAnalyzer(),
				new TypeImportAnalyzer()
			};
		}
	}
}
