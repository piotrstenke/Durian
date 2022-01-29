// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Collections.Immutable;
using Durian.Analysis;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Manager.Modules
{
	/// <summary>
	/// Manages the analyzers from the <c>Durian.Core</c> module.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class CoreManager : AnalyzerManager
	{
		/// <inheritdoc/>
		public override ImmutableArray<DurianModule> RequiredModules => ImmutableArray.Create(DurianModule.Core);

		/// <summary>
		/// Initializes a new instance of the <see cref="CoreManager"/> class.
		/// </summary>
		public CoreManager()
		{
		}

		/// <inheritdoc/>
		protected override void OnCompilationStart(CompilationStartAnalysisContext context)
		{
			if (context.Compilation is not CSharpCompilation compilation)
			{
				context.RegisterCompilationEndAction(context => context.ReportDiagnostic(IsCSharpCompilationAnalyzer.GetNotCSharpCompilationDiagnostic()));

				return;
			}

			if (ShouldAnalyze(compilation))
			{
				IDurianAnalysisContext durianContext = new DurianCompilationStartAnalysisContext(context);
				Register(durianContext, compilation);
			}
		}

		/// <inheritdoc/>
		public override IEnumerable<IDurianAnalyzer> GetAnalyzers()
		{
			return new IDurianAnalyzer[]
			{
				new IsCSharpCompilationAnalyzer(),
				new DependencyAnalyzer(),
				new CustomTypesInGeneratorNamespaceAnalyzer(),
				new TypeImportAnalyzer()
			};
		}
	}
}