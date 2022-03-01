// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using Durian.Analysis.Cache;
using Durian.Analysis.Data;
using Durian.Analysis.Logging;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.CopyFrom
{
    /// <summary>
    /// Main class of the <c>CopyFrom</c> module. Generates source code of members marked with the <c>Durian.CopyFromAttribute</c>.
    /// </summary>
    [Generator(LanguageNames.CSharp)]
	[LoggingConfiguration(SupportedLogs = GeneratorLogs.All, LogDirectory = "CopyFrom", SupportsDiagnostics = true, RelativeToGlobal = true, EnableExceptions = true)]
	public sealed class CopyFromGenerator : CachedGenerator<ICopyFromMember, CopyFromCompilationData, CopyFromSyntaxReceiver, ICopyFromFilter>
	{
		private FilterContainer<ICopyFromFilter>? _filters;

		/// <summary>
		/// Number of trees generated statically by this generator.
		/// </summary>
		public const int NumStaticTrees = 3;

		/// <summary>
		/// Name of this source generator.
		/// </summary>
		public static string GeneratorName => "CopyFrom";

		/// <summary>
		/// Version of this source generator.
		/// </summary>
		public static string Version => "1.0.0";

		/// <inheritdoc cref="CopyFromGenerator(in ConstructionContext, IHintNameProvider?)"/>
		public CopyFromGenerator()
		{
		}

		/// <inheritdoc cref="CopyFromGenerator(in ConstructionContext, IHintNameProvider?)"/>
		public CopyFromGenerator(in ConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CopyFromGenerator"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		public CopyFromGenerator(in ConstructionContext context, IHintNameProvider? fileNameProvider) : base(in context, fileNameProvider)
		{
		}

		/// <inheritdoc cref="CopyFromGenerator(LoggingConfiguration?, IHintNameProvider?)"/>
		public CopyFromGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CopyFromGenerator"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		public CopyFromGenerator(LoggingConfiguration? loggingConfiguration, IHintNameProvider? fileNameProvider) : base(loggingConfiguration, fileNameProvider)
		{
		}

		/// <summary>
		/// Returns a collection of <see cref="ISourceTextProvider"/> used by this generator to create initial sources.
		/// </summary>
		public static IEnumerable<ISourceTextProvider> GetSourceProviders()
		{
			return new ISourceTextProvider[]
			{
				new CopyFromTypeAttributeProvider(),
				new CopyFromMethodAttributeProvider(),
				new PatternAttributeProvider(),
			};
		}

		/// <inheritdoc/>
		public override CopyFromSyntaxReceiver CreateSyntaxReceiver()
		{
			return new CopyFromSyntaxReceiver();
		}

		/// <inheritdoc/>
		public override FilterContainer<ICopyFromFilter> GetFilters(in GeneratorExecutionContext context)
		{
			return GetFilters(new SymbolNameToFile());
		}

        /// <summary>
        /// Returns a <see cref="FilterContainer{TFilter}"/> to be used during the current generation pass.
        /// </summary>
        /// <param name="fileNameProvider">Creates name for the generated files.</param>
        public FilterContainer<ICopyFromFilter> GetFilters(IHintNameProvider fileNameProvider)
		{
			if (_filters is null)
			{
				FilterContainer<ICopyFromFilter> list = new();

				list.RegisterFilterGroup("Methods", new CopyFromMethodFilter(this, fileNameProvider));
				list.RegisterFilterGroup("Types", new CopyFromTypeFilter(this, fileNameProvider));

				_filters = list;
			}

			return _filters;
		}

        /// <inheritdoc/>
        protected override CopyFromCompilationData? CreateCompilationData(CSharpCompilation compilation)
		{
			return new CopyFromCompilationData(compilation);
		}

		/// <inheritdoc/>
		protected override bool Generate(IMemberData member, string hintName, in GeneratorExecutionContext context)
		{
			if(member is not ICopyFromMember target)
            {
				return false;
            }

			CodeBuilder.WriteDeclarationLead(target, member.Declaration.SyntaxTree.GetCompilationUnitRoot().Usings.Select(u => u.Name.ToString()), GeneratorName, Version);
			CodeBuilder.EndAllBlocks();
			AddSourceWithOriginal(target.Declaration, hintName, in context);

			return true;
		}

		/// <inheritdoc/>
		protected override string? GetGeneratorName()
		{
			return GeneratorName;
		}

		/// <inheritdoc/>
		protected override string? GetGeneratorVersion()
		{
			return Version;
		}

		/// <inheritdoc/>
		protected override IEnumerable<ISourceTextProvider>? GetInitialSources()
		{
			return GetSourceProviders();
		}

		/// <inheritdoc/>
		protected override DurianModule[] GetRequiredModules()
		{
			return new DurianModule[]
			{
				DurianModule.CopyFrom
			};
		}
	}
}