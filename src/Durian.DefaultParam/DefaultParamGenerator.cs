using System.Collections.Generic;
using System.Linq;
using Durian.Generator.Data;
using Durian.Generator.Extensions;
using Durian.Generator.Logging;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Generator.DefaultParam
{
	/// <summary>
	/// Main class of the <c>DefaultParam</c> module. Generates the source code of the members marked with the <see cref="DefaultParamAttribute"/>.
	/// </summary>
	[Generator]
	[GeneratorLoggingConfiguration(SupportedLogs = GeneratorLogs.All, LogDirectory = "DefaultParam", SupportsDiagnostics = true, RelativeToGlobal = true, EnableExceptions = true)]
	public class DefaultParamGenerator : DurianGenerator<DefaultParamCompilationData, DefaultParamSyntaxReceiver, IDefaultParamFilter>.WithBuilder
	{
		/// <summary>
		/// Version of this source generator.
		/// </summary>
		public static string Version => "1.1.0";

		/// <summary>
		/// Name of this source generator, i.e. 'DefaultParam'.
		/// </summary>
		public static string GeneratorName => "DefaultParam";

		/// <summary>
		/// Number of trees generated statically by this generator.
		/// </summary>
		public const int NumStaticTrees = 1;

		private readonly DefaultParamRewriter _rewriter = new();

		/// <inheritdoc cref="DefaultParamGenerator(in LoggableGeneratorConstructionContext, IFileNameProvider?)"/>
		public DefaultParamGenerator()
		{
		}

		/// <inheritdoc cref="DefaultParamGenerator(in LoggableGeneratorConstructionContext, IFileNameProvider?)"/>
		public DefaultParamGenerator(in LoggableGeneratorConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamGenerator"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableSourceGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		public DefaultParamGenerator(in LoggableGeneratorConstructionContext context, IFileNameProvider? fileNameProvider) : base(in context, fileNameProvider)
		{
		}

		/// <inheritdoc cref="DefaultParamGenerator(GeneratorLoggingConfiguration?, IFileNameProvider?)"/>
		public DefaultParamGenerator(GeneratorLoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamGenerator"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		public DefaultParamGenerator(GeneratorLoggingConfiguration? loggingConfiguration, IFileNameProvider? fileNameProvider) : base(loggingConfiguration, fileNameProvider)
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
		protected override FilterContainer<IDefaultParamFilter> GetFilters(in GeneratorExecutionContext context)
		{
			return GetFilters(new SymbolNameToFile());
		}

		/// <summary>
		/// Returns a <see cref="FilterContainer{TFilter}"/> to be used during the current generation pass.
		/// </summary>
		/// <param name="fileNameProvider">Creates name for the generated files.</param>
		protected FilterContainer<IDefaultParamFilter> GetFilters(IFileNameProvider fileNameProvider)
		{
			FilterContainer<IDefaultParamFilter> list = new();

			list.RegisterFilterGroup("Methods", new DefaultParamMethodFilter(this, fileNameProvider));
			list.RegisterFilterGroup("Delegates", new DefaultParamDelegateFilter(this, fileNameProvider));
			list.RegisterFilterGroup("Types", new DefaultParamTypeFilter(this, fileNameProvider));

			if (EnableDiagnostics)
			{
				list.RegisterFilterGroup("Local Functions", new DefaultParamLocalFunctionFilter(this, fileNameProvider));
			}

			return list;
		}

		/// <inheritdoc/>
		protected override DefaultParamCompilationData CreateCompilationData(CSharpCompilation compilation)
		{
			return new DefaultParamCompilationData(compilation);
		}

		/// <inheritdoc/>
		protected sealed override string GetVersion()
		{
			return Version;
		}

		/// <inheritdoc/>
		protected sealed override string GetGeneratorName()
		{
			return GeneratorName;
		}

		/// <inheritdoc/>
		protected sealed override DurianModule[] GetEnabledModules()
		{
			return new DurianModule[] { DurianModule.DefaultParam };
		}

		/// <inheritdoc/>
		protected override void BeforeExecution(in GeneratorExecutionContext context)
		{
			_rewriter.ParentCompilation = TargetCompilation;
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
		protected override void IterateThroughFilter(IDefaultParamFilter filter, in GeneratorExecutionContext context)
		{
			switch (filter.Mode)
			{
				case FilterMode.None:
					IterateThroughFilter(new FilterEnumerator(filter), in context);
					break;

				case FilterMode.Diagnostics:
					IterateThroughFilter(new DiagnosticEnumerator(filter), in context);
					break;

				case FilterMode.Logs:
					IterateThroughFilter(new LoggableEnumerator(filter), in context);
					break;

				case FilterMode.Both:
					IterateThroughFilter(new LoggableDiagnosticEnumerator(filter), in context);
					break;

				default:
					goto case FilterMode.None;
			}
		}

		private void IterateThroughFilter<T>(T iter, in GeneratorExecutionContext context) where T : IEnumerator<IDefaultParamTarget>
		{
			while (iter.MoveNext())
			{
				GenerateFromData(iter.Current, in context);
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
	}
}
