// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Cache;
using Durian.Analysis.Data;
using Durian.Analysis.Logging;
using Durian.Analysis.Extensions;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

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
	public sealed class CopyFromGenerator : CachedGenerator<ICopyFromMember, CopyFromCompilationData, CopyFromSyntaxReceiver, ICopyFromFilter>
	{
		private const int _numStaticTrees = 4;

		private FilterContainer<ICopyFromFilter>? _filters;

		/// <summary>
		/// Name of this source generator.
		/// </summary>
		public static string GeneratorName => "CopyFrom";

		/// <summary>
		/// Version of this source generator.
		/// </summary>
		public static string Version => "1.0.0";

		/// <inheritdoc/>
		public override int NumStaticTrees => _numStaticTrees;

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
			return new ISourceTextProvider[_numStaticTrees - 1]
			{
				new CopyFromTypeAttributeProvider(),
				new CopyFromMethodAttributeProvider(),
				new PatternAttributeProvider(),
			};
		}

		/// <inheritdoc/>
		public override CopyFromCompilationData? CreateCompilationData(CSharpCompilation compilation)
		{
			return new CopyFromCompilationData(compilation);
		}

		/// <inheritdoc/>
		public override CopyFromSyntaxReceiver CreateSyntaxReceiver()
		{
			return new CopyFromSyntaxReceiver();
		}

		/// <summary>
		/// Returns a <see cref="FilterContainer{TFilter}"/> to be used during the current generation pass.
		/// </summary>
		/// <param name="fileNameProvider">Creates name for the generated files.</param>
		public override FilterContainer<ICopyFromFilter> GetFilters(IHintNameProvider fileNameProvider)
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
		public override string? GetGeneratorName()
		{
			return GeneratorName;
		}

		/// <inheritdoc/>
		public override string? GetGeneratorVersion()
		{
			return Version;
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
		protected override bool Generate(IMemberData member, string hintName, in GeneratorExecutionContext context)
		{
			if (member is not ICopyFromMember target)
			{
				return false;
			}

			TargetData[] targets = target.Targets;
			SortByOrder(targets);

			Generate(0);
			AddSourceWithOriginal(target.Declaration, hintName, in context);

			for (int i = 1; i < targets.Length; i++)
			{
				Generate(i);
				AddSourceWithOriginal(target.Declaration, hintName + $"_{i}", in context);
			}

			return true;

			void Generate(int index)
			{
				ref readonly TargetData t = ref targets[index];

				CodeBuilder.WriteDeclarationLead(target, GetUsings(in t), GeneratorName, Version);
				CodeBuilder.EndAllBlocks();
			}
		}

		private static void SortByOrder(TargetData[] targets)
		{
			if(targets.Length == 1)
			{
				return;
			}

			Array.Sort(targets, (left, right) =>
			{
				if (left.Order > right.Order)
				{
					return 1;
				}
				else if (left.Order < right.Order)
				{
					return -1;
				}
				else
				{
					return 0;
				}
			});
		}

		private static void GenerateType(CopyFromTypeData type, in TargetData data)
		{

		}

		private static IEnumerable<string>? GetUsings(in TargetData target, out TypeDeclarationSyntax[]? partialDeclarations)
		{
			List<string> list = new(24);

			if(target.CopyUsings)
			{
				if(target.PartialPart is null)
				{
					partialDeclarations = target.Symbol.GetPartialDeclarations<TypeDeclarationSyntax>().ToArray();

					foreach (TypeDeclarationSyntax partial in partialDeclarations)
					{
						
					}
				}
			}

			return target.Usings;
		}
	}
}