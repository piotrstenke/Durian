// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis;
using Durian.Analysis.FriendClass;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Manager
{
	/// <summary>
	/// Manages the analyzers from the <c>Durian.FriendClass</c> package.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class FriendClassManager : AnalysisManager, IDurianModuleManager
	{
		/// <inheritdoc/>
		public DurianModule Module => DurianModule.FriendClass;

		/// <summary>
		/// Initializes a new instance of the <see cref="FriendClassManager"/> class.
		/// </summary>
		public FriendClassManager()
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
				new FriendClassDeclarationAnalyzer(),
				new FriendClassAccessAnalyzer()
			};
		}
	}
}