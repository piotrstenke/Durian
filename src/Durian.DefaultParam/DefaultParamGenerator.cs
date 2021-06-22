// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Linq;
using Durian.Analysis.Cache;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Durian.Analysis.Logging;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.DefaultParam
{
	/// <summary>
	/// Main class of the <c>DefaultParam</c> module. Generates source code of members marked with the <see cref="DefaultParamAttribute"/>.
	/// </summary>
#if !MAIN_PACKAGE

	[Generator(LanguageNames.CSharp)]
#endif

	[GeneratorLoggingConfiguration(SupportedLogs = GeneratorLogs.All, LogDirectory = "DefaultParam", SupportsDiagnostics = true, RelativeToGlobal = true, EnableExceptions = true)]
	public class DefaultParamGenerator : CachedDurianGenerator<DefaultParamCompilationData, DefaultParamSyntaxReceiver, IDefaultParamFilter, IDefaultParamTarget>
	{
		/// <summary>
		/// Number of trees generated statically by this generator.
		/// </summary>
#if MAIN_PACKAGE
		public const int NumStaticTrees = 0;
#else
		public const int NumStaticTrees = 1;
#endif

		private readonly DefaultParamRewriter _rewriter = new();

		private FilterContainer<IDefaultParamFilter>? _filters;

		/// <summary>
		/// Name of this source generator, i.e. 'DefaultParam'.
		/// </summary>
		public static string GeneratorName => "DefaultParam";

		/// <summary>
		/// Version of this source generator.
		/// </summary>
		public static string Version => "1.3.0";

		/// <inheritdoc/>
		public override bool EnableDiagnostics
		{
			get => base.EnableDiagnostics;
			set
			{
				bool old = base.EnableDiagnostics;

				if (old != value)
				{
					base.EnableDiagnostics = value;
					_filters = null;
				}
			}
		}

		/// <inheritdoc cref="DefaultParamGenerator(in LoggableGeneratorConstructionContext, IHintNameProvider?)"/>
		public DefaultParamGenerator()
		{
		}

		/// <inheritdoc cref="DefaultParamGenerator(in LoggableGeneratorConstructionContext, IHintNameProvider?)"/>
		public DefaultParamGenerator(in LoggableGeneratorConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamGenerator"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableSourceGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		public DefaultParamGenerator(in LoggableGeneratorConstructionContext context, IHintNameProvider? fileNameProvider) : base(in context, fileNameProvider)
		{
		}

		/// <inheritdoc cref="DefaultParamGenerator(GeneratorLoggingConfiguration?, IHintNameProvider?)"/>
		public DefaultParamGenerator(GeneratorLoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamGenerator"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		public DefaultParamGenerator(GeneratorLoggingConfiguration? loggingConfiguration, IHintNameProvider? fileNameProvider) : base(loggingConfiguration, fileNameProvider)
		{
		}

		/// <summary>
		/// Creates a new <see cref="DefaultParamSyntaxReceiver"/> to be used during the current generation pass.
		/// </summary>
		public override DefaultParamSyntaxReceiver CreateSyntaxReceiver()
		{
			return new DefaultParamSyntaxReceiver(SupportsDiagnostics);
		}

		/// <inheritdoc/>
		public override FilterContainer<IDefaultParamFilter> GetFilters(in GeneratorExecutionContext context)
		{
			return GetFilters(new SymbolNameToFile());
		}

		/// <summary>
		/// Returns a <see cref="FilterContainer{TFilter}"/> to be used during the current generation pass.
		/// </summary>
		/// <param name="fileNameProvider">Creates name for the generated files.</param>
		public FilterContainer<IDefaultParamFilter> GetFilters(IHintNameProvider fileNameProvider)
		{
			if (_filters is null)
			{
				FilterContainer<IDefaultParamFilter> list = new();

				list.RegisterFilterGroup("Methods", new DefaultParamMethodFilter(this, fileNameProvider));
				list.RegisterFilterGroup("Delegates", new DefaultParamDelegateFilter(this, fileNameProvider));
				list.RegisterFilterGroup("Types", new DefaultParamTypeFilter(this, fileNameProvider));

				if (EnableDiagnostics)
				{
					list.RegisterFilterGroup("Local Functions", new DefaultParamLocalFunctionFilter(this, fileNameProvider));
				}

				_filters = list;
			}

			return _filters;
		}

		/// <inheritdoc/>
		public override FilterContainer<IDefaultParamFilter> GetFilters(in CachedGeneratorExecutionContext<IDefaultParamTarget> context)
		{
			return GetFilters(new SymbolNameToFile());
		}

		/// <inheritdoc/>
		protected override void BeforeExecution(in GeneratorExecutionContext context)
		{
			_rewriter.ParentCompilation = TargetCompilation;
		}

		/// <inheritdoc/>
		protected override DefaultParamCompilationData CreateCompilationData(CSharpCompilation compilation)
		{
			return new DefaultParamCompilationData(compilation);
		}

		/// <inheritdoc/>
		protected sealed override bool Generate(IMemberData data, string hintName, in GeneratorExecutionContext context)
		{
			if (data is not IDefaultParamTarget target)
			{
				return false;
			}

			GenerateAllVersionsOfTarget(target, in context);
			AddSourceWithOriginal(target.Declaration, hintName, in context);

			return true;
		}

		/// <inheritdoc/>
		protected sealed override DurianModule[] GetEnabledModules()
		{
			return new DurianModule[] { DurianModule.DefaultParam };
		}

		/// <inheritdoc/>
		protected sealed override string GetGeneratorName()
		{
			return GeneratorName;
		}

		/// <inheritdoc/>
		protected sealed override string GetVersion()
		{
			return Version;
		}

		/// <inheritdoc/>
		protected override void IterateThroughFilter(IDefaultParamFilter filter, in GeneratorExecutionContext context)
		{
			switch (filter.Mode)
			{
				case FilterMode.Diagnostics:
				{
					FilterEnumeratorWithDiagnostics<IDefaultParamTarget> enumerator = new(filter, TargetCompilation!, filter, DiagnosticReceiver!);

					while (enumerator.MoveNext())
					{
						GenerateFromData(enumerator.Current, in context);
					}

					break;
				}

				case FilterMode.Logs:
				{
					DefaultParamFilterEnumerator<IDefaultParamTarget> enumerator = new(filter);

					while (enumerator.MoveNext())
					{
						GenerateFromData(enumerator.Current, in context);
					}

					break;
				}

				case FilterMode.Both:
				{
					DefaultParamFilterEnumerator<IDefaultParamTarget> enumerator = new(filter, LoggableGeneratorDiagnosticReceiverFactory.SourceGenerator(this, DiagnosticReceiver!));

					while (enumerator.MoveNext())
					{
						GenerateFromData(enumerator.Current, in context);
					}

					break;
				}

				default:
				{
					FilterEnumerator<IDefaultParamTarget> enumerator = new(filter, TargetCompilation!, filter);

					while (enumerator.MoveNext())
					{
						GenerateFromData(enumerator.Current, in context);
					}

					break;
				}
			}
		}

		/// <inheritdoc/>
		protected override void IterateThroughFilter(IDefaultParamFilter filter, in CachedGeneratorExecutionContext<IDefaultParamTarget> context)
		{
			ref readonly CachedData<IDefaultParamTarget> cache = ref context.GetCachedData();
			ref readonly GeneratorExecutionContext c = ref context.GetContext();

			switch (filter.Mode)
			{
				case FilterMode.Diagnostics:
				{
					CachedFilterEnumeratorWithDiagnostics<IDefaultParamTarget> enumerator = new(filter, TargetCompilation!, filter, DiagnosticReceiver!, in cache);

					while (enumerator.MoveNext())
					{
						GenerateFromData(enumerator.Current, in c);
					}

					break;
				}

				case FilterMode.Logs:
				{
					CachedDefaultParamFilterEnumerator<IDefaultParamTarget> enumerator = new(filter, in cache);

					while (enumerator.MoveNext())
					{
						GenerateFromData(enumerator.Current, in c);
					}

					break;
				}

				case FilterMode.Both:
				{
					CachedDefaultParamFilterEnumerator<IDefaultParamTarget> enumerator = new(filter, LoggableGeneratorDiagnosticReceiverFactory.SourceGenerator(this, DiagnosticReceiver!), in cache);

					while (enumerator.MoveNext())
					{
						GenerateFromData(enumerator.Current, in c);
					}

					break;
				}

				default:
				{
					CachedFilterEnumerator<IDefaultParamTarget> enumerator = new(filter, TargetCompilation!, filter, in cache);

					while (enumerator.MoveNext())
					{
						GenerateFromData(enumerator.Current, in c);
					}

					break;
				}
			}
		}

		private static string GetTargetName(ITypeSymbol targetType)
		{
			if (targetType is INamedTypeSymbol t)
			{
				return t.Arity > 0 ? t.GetGenericName(false) : AnalysisUtilities.TypeToKeyword(targetType.Name);
			}
			else if (targetType is IArrayTypeSymbol a)
			{
				return a.ToString();
			}
			else
			{
				return AnalysisUtilities.TypeToKeyword(targetType.Name);
			}
		}

		private CSharpSyntaxNode[] CreateDefaultParamDeclarations(in TypeParameterContainer parameters)
		{
			const int originalMemberIndex = 0;
			int originalDataIndex = parameters.Length - 1;
			int originalLength = parameters.NumDefaultParam;

			int length = originalLength;
			int memberIndex = originalMemberIndex;

			// goes from last to first
			int dataIndex = originalDataIndex;

			CSharpSyntaxNode[] members = new CSharpSyntaxNode[length];

			// Types must be replaced separately from the rest.

			while (memberIndex < length)
			{
				ref readonly TypeParameterData data = ref parameters[dataIndex];

				string name = GetTargetName(data.TargetType!);
				_rewriter.ReplaceType(data.Symbol, data.TargetType!, name);

				members[memberIndex] = _rewriter.CurrentNode;

				dataIndex--;
				memberIndex++;
			}

			length = originalLength;
			memberIndex = originalMemberIndex;
			dataIndex = originalDataIndex;

			while (memberIndex < length)
			{
				ref readonly TypeParameterData data = ref parameters[dataIndex];

				_rewriter.Emplace(members[memberIndex]);
				_rewriter.RemoveLastTypeParameter();
				_rewriter.RemoveConstraintsOf(data.Symbol);

				members[memberIndex] = _rewriter.CurrentNode;

				dataIndex--;
				memberIndex++;
			}

			return members;
		}

		private void GenerateAllVersionsOfTarget(IDefaultParamTarget target, in GeneratorExecutionContext context)
		{
			IDefaultParamDeclarationBuilder declBuilder = target.GetDeclarationBuilder(context.CancellationToken);
			_rewriter.Acquire(declBuilder);
			CSharpSyntaxNode[] members = CreateDefaultParamDeclarations(in target.TypeParameters);

			if (members.Length > 0)
			{
				WriteTargetLeadDeclaration(target);
				WriteGeneratedMembers(members, target);
				CodeBuilder.EndAllBlocks();
			}
		}

		private void WriteTargetLeadDeclaration(IDefaultParamTarget target)
		{
			CodeBuilder.WriteHeader(GeneratorName, Version);
			CodeBuilder.AppendLine();
			string[] namespaces = AnalysisUtilities.SortUsings(target.GetUsedNamespaces()).ToArray();

			if (namespaces.Length > 0)
			{
				CodeBuilder.WriteUsings(namespaces);
				CodeBuilder.AppendLine();
			}

			if (target.TargetNamespace != "global")
			{
				CodeBuilder.BeginNamespaceDeclaration(target.TargetNamespace);
			}

			CodeBuilder.WriteParentDeclarations(target.GetContainingTypes());
		}
	}
}
