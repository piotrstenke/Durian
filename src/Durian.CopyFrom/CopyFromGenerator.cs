// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Cache;
using Durian.Analysis.Data;
using Durian.Analysis.Filters;
using Durian.Analysis.Logging;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;

namespace Durian.Analysis.CopyFrom
{
	/// <summary>
	/// Main class of the <c>CopyFrom</c> module. Generates source code of members marked with the <c>Durian.CopyFromAttribute</c>.
	/// </summary>
	[Generator(LanguageNames.CSharp)]
	[LoggingConfiguration(
		SupportedLogs = GeneratorLogs.All,
		LogDirectory = "CopyFrom",
		SupportsDiagnostics = true,
		RelativeToGlobal = true,
		EnableExceptions = true,
		DefaultNodeOutput = NodeOutput.SyntaxTree)]
	public sealed partial class CopyFromGenerator : CachedGenerator<ICopyFromMember>
	{
		private const int _numStaticTrees = 4;

		/// <inheritdoc/>
		public override string GeneratorName => "CopyFrom";

		/// <inheritdoc/>
		public override string GeneratorVersion => "1.0.0";

		/// <inheritdoc/>
		public override int NumStaticTrees => _numStaticTrees;

		/// <summary>
		/// Initializes a new instance of the <see cref="CopyFromGenerator"/> class.
		/// </summary>
		public CopyFromGenerator()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CopyFromGenerator"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="CopyFromGenerator"/> is initialized.</param>
		public CopyFromGenerator(in GeneratorLogCreationContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CopyFromGenerator"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		public CopyFromGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Returns a collection of <see cref="ISourceTextProvider"/> used by this generator to create initial sources.
		/// </summary>
		public static IEnumerable<ISourceTextProvider> GetSourceProviders()
		{
			return new ISourceTextProvider[_numStaticTrees - 1]
			{
				new CopyFromTypeAttributeProvider(),
				new CopyFromMethodAttributeProvider(),
				new PatternAttributeProvider(),
			};
		}

		/// <inheritdoc/>
		public override ICompilationData? CreateCompilationData(CSharpCompilation compilation)
		{
			return new CopyFromCompilationData(compilation);
		}

		/// <inheritdoc/>
		public override IDurianSyntaxReceiver CreateSyntaxReceiver()
		{
			return new CopyFromSyntaxReceiver();
		}

		/// <inheritdoc/>
		public override IReadOnlyFilterContainer<IGeneratorSyntaxFilter>? GetFilters(GeneratorPassBuilderContext context)
		{
			FilterContainer<IGeneratorSyntaxFilter> list = new();

			list.RegisterGroup("Methods", new CopyFromMethodFilter());
			list.RegisterGroup("Types", new CopyFromTypeFilter());

			return list;
		}

		/// <inheritdoc/>
		public override IEnumerable<ISourceTextProvider>? GetInitialSources()
		{
			return GetSourceProviders();
		}

		/// <inheritdoc/>
		public override DurianModule[] GetRequiredModules()
		{
			return new DurianModule[]
			{
				DurianModule.CopyFrom
			};
		}

		/// <inheritdoc/>
		protected internal override bool Generate(IMemberData data, string hintName, GeneratorPassBuilderContext context)
		{
			if (data is not ICopyFromMember target)
			{
				return false;
			}

			if (target is CopyFromTypeData type)
			{
				return GenerateType(type, hintName, in context);
			}

			return false;
		}
	}
}
