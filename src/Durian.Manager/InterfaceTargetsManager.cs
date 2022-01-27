// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis;
using Durian.Analysis.InterfaceTargets;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Manager
{
	/// <summary>
	/// Manages the analyzers from the <c>Durian.InterfaceTargets</c> package.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class InterfaceTargetsManager : AnalysisManager, IDurianModuleManager
	{
		/// <inheritdoc/>
		public DurianModule Module => DurianModule.InterfaceTargets;

		/// <summary>
		/// Initializes a new instance of the <see cref="InterfaceTargetsManager"/> class.
		/// </summary>
		public InterfaceTargetsManager()
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
				new IntfTargAnalyzer()
			};
		}
	}
}