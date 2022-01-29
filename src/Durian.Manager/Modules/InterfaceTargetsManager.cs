// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Collections.Immutable;
using Durian.Analysis;
using Durian.Analysis.InterfaceTargets;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Manager.Modules
{
	/// <summary>
	/// Manages analyzers and generators from the <c>Durian.InterfaceTargets</c> module.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	[Generator(LanguageNames.CSharp)]
	public sealed class InterfaceTargetsManager : GeneratorManager
	{
		/// <inheritdoc/>
		public override ImmutableArray<DurianModule> RequiredModules => ImmutableArray.Create(DurianModule.InterfaceTargets);

		/// <summary>
		/// Initializes a new instance of the <see cref="InterfaceTargetsManager"/> class.
		/// </summary>
		public InterfaceTargetsManager()
		{
		}

		/// <inheritdoc/>
		public override IEnumerable<IDurianAnalyzer> GetAnalyzers()
		{
			return new IDurianAnalyzer[]
			{
				new InterfaceTargetsAnalyzer()
			};
		}

		/// <inheritdoc/>
		public override IEnumerable<ISourceGenerator> GetGenerators()
		{
			return new ISourceGenerator[]
			{
				new InterfaceTargetsGenerator()
			};
		}
	}
}