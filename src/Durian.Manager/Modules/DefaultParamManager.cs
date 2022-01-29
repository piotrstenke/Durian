// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Durian.Analysis;
using Durian.Analysis.DefaultParam;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Manager.Modules
{
	/// <summary>
	/// Manages analyzers and generators from the <c>Durian.DefaultParam</c> module.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	[Generator(LanguageNames.CSharp)]
	public sealed class DefaultParamManager : CachedGeneratorManager<IDefaultParamTarget>
	{
		/// <inheritdoc/>
		public override ImmutableArray<DurianModule> RequiredModules => ImmutableArray.Create(DurianModule.DefaultParam);

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamManager"/> class.
		/// </summary>
		public DefaultParamManager()
		{
		}

		/// <inheritdoc/>
		public override bool TryGetSyntaxReceiverCreator([NotNullWhen(true)] out SyntaxReceiverCreator? syntaxReceiverCreator)
		{
			syntaxReceiverCreator = () => new DefaultParamSyntaxReceiver();
			return true;
		}

		/// <inheritdoc/>
		public override IEnumerable<IDurianAnalyzer> GetAnalyzers()
		{
			return new IDurianAnalyzer[]
			{
				new DefaultParamMethodFilter.AsAnalyzer(),
				new DefaultParamDelegateFilter.AsAnalyzer(),
				new DefaultParamTypeFilter.AsAnalyzer()
			};
		}

		/// <inheritdoc/>
		public override IEnumerable<ISourceGenerator> GetGenerators()
		{
			return new ISourceGenerator[]
			{
				new DefaultParamGenerator()
			};
		}
	}
}