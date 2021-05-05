using System.Collections.Generic;
using System.Threading;
using Durian.Data;
using Durian.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.DefaultParam
{
	[Generator]
	[GeneratorLoggingConfiguration(SupportsDiagnostics = true, SupportedLogs = GeneratorLogs.All, LogDirectory = "DefaultParam", RelativeToGlobal = true)]
	public class DefaultParamGenerator : SourceGenerator<DefaultParamCompilationData, DefaultParamSyntaxReceiver, IDefaultParamFilter>.WithBuilder
	{
		public static string Version => DefaultParamUtilities.Package.Version;
		public static string GeneratorName => DefaultParamUtilities.Package.Name;

		public const int NumDefaultParamAttributes = 3;

		private readonly DefaultParamRewriter _rewriter = new();

		public DefaultParamGenerator() : base(true, false, false)
		{
		}

		public DefaultParamGenerator(bool checkForConfigurationAttribute, bool enableLoggingIfSupported = true, bool enableDiagnosticsIfSupported = true) : base(checkForConfigurationAttribute, enableLoggingIfSupported, enableDiagnosticsIfSupported)
		{
		}

		public DefaultParamGenerator(bool checkForConfigurationAttribute, bool enableLoggingIfSupported, bool enableDiagnosticsIfSupported, IFileNameProvider fileNameProvider) : base(checkForConfigurationAttribute, enableLoggingIfSupported, enableDiagnosticsIfSupported, fileNameProvider)
		{
		}

		public DefaultParamGenerator(GeneratorLoggingConfiguration loggingConfiguration) : base(loggingConfiguration)
		{
		}

		public DefaultParamGenerator(GeneratorLoggingConfiguration loggingConfiguration, IFileNameProvider fileNameProvider) : base(loggingConfiguration, fileNameProvider)
		{
		}

		public override DefaultParamSyntaxReceiver CreateSyntaxReceiver()
		{
			return new DefaultParamSyntaxReceiver(SupportsDiagnostics && EnableDiagnostics);
		}

		protected override FilterContainer<IDefaultParamFilter> GetFilters(in GeneratorExecutionContext context)
		{
			FilterContainer<IDefaultParamFilter> list = new();

			list.RegisterFilterGroup(new IDefaultParamFilter[] { new DefaultParamDelegateFilter(this), new DefaultParamMethodFilter(this) });
			list.RegisterFilterGroup(new IDefaultParamFilter[] { new DefaultParamTypeFilter(this) });

			if (EnableDiagnostics)
			{
				list.RegisterFilterGroup(new IDefaultParamFilter[] { new DefaultParamLocalFunctionFilter(this) });
			}

			return list;
		}

		protected override DefaultParamCompilationData CreateCompilationData(CSharpCompilation compilation)
		{
			return new DefaultParamCompilationData(compilation);
		}

		protected sealed override string GetVersion()
		{
			return Version;
		}

		protected sealed override string GetGeneratorName()
		{
			return GeneratorName;
		}

		protected override (CSharpSyntaxTree tree, string hintName)[]? GetStaticSyntaxTrees(CancellationToken cancellationToken)
		{
			return new[]
			{
				(DefaultParamAttribute.CreateSyntaxTree(), DefaultParamAttribute.FullyQualifiedName),
				(DefaultParamConfigurationAttribute.CreateSyntaxTree(), DefaultParamConfigurationAttribute.FullyQualifiedName),
				(DefaultParamMethodConfigurationAttribute.CreateSyntaxTree(), DefaultParamMethodConfigurationAttribute.FullyQualifiedName)
			};
		}

		protected override void BeforeExecution(in GeneratorExecutionContext context)
		{
			_rewriter.ParentCompilation = TargetCompilation;
		}

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

		protected sealed override bool Generate(IMemberData data, string hintName, in GeneratorExecutionContext context)
		{
			if (data is not IDefaultParamTarget target)
			{
				return false;
			}

			WriteTargetLeadDeclaration(target);
			GenerateAllVersionsOfTarget(target, in context);
			CodeBuilder.EndAllBlocks();
			AddSourceWithOriginal(target.Declaration, hintName, in context);

			return true;
		}

		private void WriteTargetLeadDeclaration(IDefaultParamTarget target)
		{
			CodeBuilder.WriteDeclarationLead(target, AnalysisUtilities.SortUsings(target.GetUsedNamespaces()), GeneratorName, Version);
		}

		private void GenerateAllVersionsOfTarget(IDefaultParamTarget target, in GeneratorExecutionContext context)
		{
			IDefaultParamDeclarationBuilder declBuilder = target.GetDeclarationBuilder(context.CancellationToken);
			_rewriter.Acquire(declBuilder);
			CSharpSyntaxNode[] members = CreateDefaultParamDeclarations(in target.TypeParameters);
			WriteGeneratedMembers(members, target);
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

			// Types must be replaces separately from the rest.

			while (memberIndex < length)
			{
				ref readonly TypeParameterData data = ref parameters[dataIndex];

				_rewriter.ReplaceType(data.Symbol, AnalysisUtilities.TypeToKeyword(data.TargetType!.Name));

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
				_rewriter.RemoveLastParameter();
				_rewriter.RemoveConstraintsOf(data.Symbol);

				members[memberIndex] = _rewriter.CurrentNode;

				dataIndex--;
				memberIndex++;
			}

			return members;
		}
	}
}
