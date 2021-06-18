// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis;
using Durian.Analysis.Cache;
using Durian.Analysis.DefaultParam;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Manager
{
	/// <summary>
	/// Manages the analyzers and source generators from the <c>Durian.DefaultParam</c> package.
	/// </summary>
#pragma warning disable RS1001 // Missing diagnostic analyzer attribute.

	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	[Generator(LanguageNames.CSharp)]
	public sealed class DefaultParamManager : AnalysisManagerWithGenerators<IDefaultParamTarget>, IDurianModuleManager
#pragma warning restore RS1001 // Missing diagnostic analyzer attribute.
	{
		/// <inheritdoc/>
		public DurianModule Module => DurianModule.DefaultParam;

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamManager"/> class.
		/// </summary>
		public DefaultParamManager()
		{
		}

		/// <inheritdoc/>
		public override IDurianSyntaxReceiver CreateSyntaxReceiver()
		{
			return new DefaultParamSyntaxReceiver();
		}

		/// <inheritdoc/>
		public override bool ShouldAnalyze(Compilation compilation)
		{
			return
				IsCSharpCompilationAnalyzer.Analyze(compilation) &&
				DisabledModuleAnalyzer.IsEnabled(Module) &&
				DependenciesAnalyzer.Analyze(compilation);
		}

		/// <inheritdoc/>
		protected override IAnalyzerInfo[] GetAnalyzersCore()
		{
			return new IAnalyzerInfo[]
			{
				new DefaultParamConfigurationAnalyzer(),
				new DefaultParamScopedConfigurationAnalyzer(),
				new DefaultParamLocalFunctionAnalyzer()
			};
		}

		/// <inheritdoc/>
		protected override ICachedAnalyzerInfo<IDefaultParamTarget>[] GetCachedAnalyzersCore()
		{
			return new ICachedAnalyzerInfo<IDefaultParamTarget>[]
			{
				new DefaultParamMethodFilter.AsAnalyzer(),
				new DefaultParamDelegateFilter.AsAnalyzer(),
				new DefaultParamTypeFilter.AsAnalyzer(),
			};
		}

		/// <inheritdoc/>
		protected override ICachedDurianSourceGenerator<IDefaultParamTarget>[] GetSourceGeneratorsCore()
		{
			return new ICachedDurianSourceGenerator<IDefaultParamTarget>[]
			{
				new DefaultParamGenerator()
			};
		}
	}
}
