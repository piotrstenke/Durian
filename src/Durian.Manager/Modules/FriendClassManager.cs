// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Collections.Immutable;
using Durian.Analysis;
using Durian.Analysis.FriendClass;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Manager.Modules
{
	/// <summary>
	/// Manages analyzers and generators from the <c>Durian.FriendClass</c> module.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	[Generator(LanguageNames.CSharp)]
	public sealed class FriendClassManager : GeneratorManager
	{
		/// <inheritdoc/>
		public override ImmutableArray<DurianModule> RequiredModules => ImmutableArray.Create(DurianModule.FriendClass);

		/// <summary>
		/// Initializes a new instance of the <see cref="FriendClassManager"/> class.
		/// </summary>
		public FriendClassManager()
		{
		}

		/// <inheritdoc/>
		public override IEnumerable<IDurianAnalyzer> GetAnalyzers()
		{
			return new IDurianAnalyzer[]
			{
				new FriendClassAccessAnalyzer(),
				new FriendClassDeclarationAnalyzer()
			};
		}

		/// <inheritdoc/>
		public override IEnumerable<ISourceGenerator> GetGenerators()
		{
			return new ISourceGenerator[]
			{
				new FriendClassGenerator()
			};
		}
	}
}