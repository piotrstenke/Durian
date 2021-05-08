using System;
using System.Collections.Generic;
using System.Threading;
using Durian.Data;
using Durian.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.DefaultParam
{
	/// <summary>
	/// Main class of the <c>DefaultParam</c> module. Generates the source code of the members marked with the <see cref="DefaultParamAttribute"/>.
	/// </summary>
	[Generator]
	[GeneratorLoggingConfiguration(SupportedLogs = GeneratorLogs.All, LogDirectory = "DefaultParam", SupportsDiagnostics = true, RelativeToGlobal = true, EnableExceptions = true)]
	public class DefaultParamGenerator : SourceGenerator<DefaultParamCompilationData, DefaultParamSyntaxReceiver, IDefaultParamFilter>.WithBuilder
	{
		/// <summary>
		/// Version of this source generator.
		/// </summary>
		public static string Version => DefaultParamUtilities.Package.Version;

		/// <summary>
		/// Name of this source generator, i.e. 'DefaultParam'.
		/// </summary>
		public static string GeneratorName => DefaultParamUtilities.Package.Name;

		/// <summary>
		/// Number of statically-generated DefaultParam attributes (<see cref="DefaultParamAttribute"/>, <see cref="DefaultParamConfigurationAttribute"/> and <see cref="DefaultParamMethodConfigurationAttribute"/>).
		/// </summary>
		public const int NumDefaultParamAttributes = 3;

		private readonly DefaultParamRewriter _rewriter = new();

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamGenerator"/> class.
		/// </summary>
		public DefaultParamGenerator() : base(true, false, false, true)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamGenerator"/> class.
		/// </summary>
		/// <param name="checkForConfigurationAttribute">Determines whether to try to create a <see cref="GeneratorLoggingConfiguration"/> based on one of the logging attributes.
		/// <para>See: <see cref="GeneratorLoggingConfigurationAttribute"/>, <see cref="DefaultGeneratorLoggingConfigurationAttribute"/></para></param>
		/// <param name="enableLoggingIfSupported">Determines whether to enable logging for this <see cref="DefaultParamGenerator"/> instance if logging is supported.</param>
		/// <param name="enableDiagnosticsIfSupported">Determines whether to set <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}.EnableDiagnostics"/> to <see langword="true"/> if <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}.SupportsDiagnostics"/> is <see langword="true"/>.</param>
		/// <param name="enableExceptionsIfDebug">Determines whether to set <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}.EnableExceptions"/> to <see langword="true"/> if the DEBUG symbol is present and the initial value of <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}.EnableExceptions"/> is <see langword="false"/>.</param>
		public DefaultParamGenerator(bool checkForConfigurationAttribute, bool enableLoggingIfSupported = true, bool enableDiagnosticsIfSupported = true, bool enableExceptionsIfDebug = true) : base(checkForConfigurationAttribute, enableLoggingIfSupported, enableDiagnosticsIfSupported, enableExceptionsIfDebug)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamGenerator"/> class.
		/// </summary>
		/// <param name="checkForConfigurationAttribute">Determines whether to try to create a <see cref="GeneratorLoggingConfiguration"/> based on one of the logging attributes.
		/// <para>See: <see cref="GeneratorLoggingConfigurationAttribute"/>, <see cref="DefaultGeneratorLoggingConfigurationAttribute"/></para></param>
		/// <param name="enableLoggingIfSupported">Determines whether to enable logging for this <see cref="DefaultParamGenerator"/> instance if logging is supported.</param>
		/// <param name="enableDiagnosticsIfSupported">Determines whether to set <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}.EnableDiagnostics"/> to <see langword="true"/> if <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}.SupportsDiagnostics"/> is <see langword="true"/>.</param>
		/// <param name="enableExceptionsIfDebug">Determines whether to set <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}.EnableExceptions"/> to <see langword="true"/> if the DEBUG symbol is present and the initial value of <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}.EnableExceptions"/> is <see langword="false"/>.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		/// <exception cref="ArgumentNullException"><paramref name="fileNameProvider"/> is <see langword="null"/>.</exception>
		public DefaultParamGenerator(bool checkForConfigurationAttribute, bool enableLoggingIfSupported, bool enableDiagnosticsIfSupported, bool enableExceptionsIfDebug, IFileNameProvider fileNameProvider) : base(checkForConfigurationAttribute, enableLoggingIfSupported, enableDiagnosticsIfSupported, enableExceptionsIfDebug, fileNameProvider)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamGenerator"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		public DefaultParamGenerator(GeneratorLoggingConfiguration loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamGenerator"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		/// <exception cref="ArgumentNullException"><paramref name="fileNameProvider"/> is <see langword="null"/>.</exception>
		public DefaultParamGenerator(GeneratorLoggingConfiguration loggingConfiguration, IFileNameProvider fileNameProvider) : base(loggingConfiguration, fileNameProvider)
		{
		}

		/// <summary>
		/// Creates a new <see cref="DefaultParamSyntaxReceiver"/> to be used during the current generation pass.
		/// </summary>
		public override DefaultParamSyntaxReceiver CreateSyntaxReceiver()
		{
			return new DefaultParamSyntaxReceiver(SupportsDiagnostics && EnableDiagnostics);
		}

		/// <inheritdoc/>
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
		protected override (CSharpSyntaxTree tree, string hintName)[]? GetStaticSyntaxTrees(CancellationToken cancellationToken)
		{
			return new[]
			{
				(DefaultParamAttribute.CreateSyntaxTree(ParseOptions, cancellationToken), DefaultParamAttribute.FullyQualifiedName),
				(DefaultParamConfigurationAttribute.CreateSyntaxTree(ParseOptions, cancellationToken), DefaultParamConfigurationAttribute.FullyQualifiedName),
				(DefaultParamMethodConfigurationAttribute.CreateSyntaxTree(ParseOptions, cancellationToken), DefaultParamMethodConfigurationAttribute.FullyQualifiedName)
			};
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

			WriteTargetLeadDeclaration(target);
			GenerateAllVersionsOfTarget(target, in context);
			CodeBuilder.EndAllBlocks();
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
				_rewriter.RemoveLastTypeParameter();
				_rewriter.RemoveConstraintsOf(data.Symbol);

				members[memberIndex] = _rewriter.CurrentNode;

				dataIndex--;
				memberIndex++;
			}

			return members;
		}
	}
}
