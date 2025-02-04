using System.Collections.Generic;
using Durian.Analysis.Cache;
using Durian.Analysis.CodeGeneration;
using Durian.Analysis.Data;
using Durian.Analysis.Filtering;
using Durian.Analysis.Logging;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.DefaultParam;

/// <summary>
/// Main class of the <c>DefaultParam</c> module. Generates source code of members marked with the <c>Durian.DefaultParamAttribute</c>.
/// </summary>
[Generator(LanguageNames.CSharp)]
[LoggingConfiguration(
	SupportedLogs = GeneratorLogs.All,
	LogDirectory = "DefaultParam",
	SupportsDiagnostics = true,
	RelativeToGlobal = true,
	EnableExceptions = true,
	DefaultNodeOutput = NodeOutput.Containing)]
public sealed class DefaultParamGenerator : CachedGenerator<IDefaultParamTarget, DefaultParamPassContext>
{
	/// <summary>
	/// Name of this source generator.
	/// </summary>
	public static string Name => "DefaultParam";

	/// <summary>
	/// Version of this source generator.
	/// </summary>
	public static string Version => "3.0.0";

	/// <inheritdoc/>
	public override string GeneratorName => Name;

	/// <inheritdoc/>
	public override string GeneratorVersion => Version;

	/// <inheritdoc/>
	public override int NumStaticTrees => 6;

	/// <summary>
	/// Initializes a new instance of the <see cref="DefaultParamGenerator"/> class.
	/// </summary>
	public DefaultParamGenerator()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DefaultParamGenerator"/> class.
	/// </summary>
	/// <param name="context">Configures how this <see cref="DefaultParamGenerator"/> is initialized.</param>
	public DefaultParamGenerator(in GeneratorLogCreationContext context) : base(in context)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DefaultParamGenerator"/> class.
	/// </summary>
	/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
	public DefaultParamGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
	{
	}

	/// <summary>
	/// Returns a collection of <see cref="ISourceTextProvider"/> used by this generator to create initial sources.
	/// </summary>
	public static IEnumerable<ISourceTextProvider> GetSourceProviders()
	{
		return new ISourceTextProvider[]
		{
			new DPMethodConventionProvider(),
			new DPTypeConventionProvider(),
			new DefaultParamAttributeProvider(),
			new DefaultParamConfigurationAttributeProvider(),
			new DefaultParamScopedConfigurationAttributeProvider()
		};
	}

	/// <inheritdoc/>
	public override IReadOnlyFilterContainer<IGeneratorSyntaxFilter>? GetFilters(DefaultParamPassContext context)
	{
		FilterContainer<IGeneratorSyntaxFilter> list = new();

		list.RegisterGroup("Methods", new Methods.DefaultParamMethodFilter());
		list.RegisterGroup("Delegates", new Delegates.DefaultParamDelegateFilter());
		list.RegisterGroup("Types", new Types.DefaultParamTypeFilter());

		if (LoggingConfiguration.EnableDiagnostics)
		{
			list.RegisterGroup("Local Functions", new Methods.DefaultParamLocalFunctionFilter());
		}

		return list;
	}

	/// <inheritdoc/>
	protected internal override ICompilationData CreateCompilationData(CSharpCompilation compilation)
	{
		return new DefaultParamCompilationData(compilation);
	}

	/// <summary>
	/// Creates a new <see cref="DefaultParamSyntaxReceiver"/> to be used during the current generation pass.
	/// </summary>
	protected internal override IDurianSyntaxReceiver CreateSyntaxReceiver()
	{
		return new DefaultParamSyntaxReceiver(LoggingConfiguration.SupportsDiagnostics);
	}

	/// <inheritdoc/>
	protected internal override IEnumerable<ISourceTextProvider>? GetInitialSources()
	{
		return GetSourceProviders();
	}

	/// <inheritdoc/>
	protected internal override DurianModule[] GetRequiredModules()
	{
		return new DurianModule[] { DurianModule.DefaultParam };
	}

	/// <inheritdoc/>
	protected internal override void BeforeExecution(DefaultParamPassContext context)
	{
		context.Rewriter.ParentCompilation = context.TargetCompilation;
		base.BeforeExecution(context);
	}

	/// <inheritdoc/>
	protected internal override bool Generate(IMemberData data, string hintName, DefaultParamPassContext context)
	{
		if (data is not IDefaultParamTarget target)
		{
			return false;
		}

		GenerateAllVersionsOfTarget(target, context);
		AddSourceWithOriginal(hintName, target.Declaration!, context);

		return true;
	}

	/// <inheritdoc/>
	protected override DefaultParamPassContext CreateCurrentPassContext(ICompilationData currentCompilation, GeneratorExecutionContext context)
	{
		return new DefaultParamPassContext();
	}

	private static CSharpSyntaxNode[] CreateDefaultParamDeclarations(in TypeParameterContainer parameters, DefaultParamPassContext context)
	{
		const int ORIGINAL_MEMBER_INDEX = 0;
		int originalDataIndex = parameters.Length - 1;
		int originalLength = parameters.NumDefaultParam;

		int length = originalLength;
		int memberIndex = ORIGINAL_MEMBER_INDEX;

		// goes from last to first
		int dataIndex = originalDataIndex;

		CSharpSyntaxNode[] members = new CSharpSyntaxNode[length];

		// Types must be replaced separately from the rest.

		while (memberIndex < length)
		{
			ref readonly TypeParameterData data = ref parameters[dataIndex];

			string name = GetTargetName(data.TargetType!);
			context.Rewriter.ReplaceType(data.Symbol, data.TargetType!, name);

			members[memberIndex] = context.Rewriter.CurrentNode;

			dataIndex--;
			memberIndex++;
		}

		length = originalLength;
		memberIndex = ORIGINAL_MEMBER_INDEX;
		dataIndex = originalDataIndex;

		while (memberIndex < length)
		{
			ref readonly TypeParameterData data = ref parameters[dataIndex];

			context.Rewriter.Emplace(members[memberIndex]);
			context.Rewriter.RemoveLastTypeParameter();
			context.Rewriter.RemoveConstraintsOf(data.Symbol);

			members[memberIndex] = context.Rewriter.CurrentNode;

			dataIndex--;
			memberIndex++;
		}

		return members;
	}

	private static string GetTargetName(ITypeSymbol targetType)
	{
		if (targetType is INamedTypeSymbol t)
		{
			return t.Arity > 0 ? t.GetGenericName(true) : AnalysisUtilities.TypeToKeyword(targetType.Name);
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

	private static void WriteTargetLeadDeclaration(IDefaultParamTarget target, DefaultParamPassContext context)
	{
		context.CodeBuilder.Write(AutoGenerated.GetHeader(Name, Version));
		context.CodeBuilder.NewLine();

		foreach (string @namespace in AnalysisUtilities.SortUsings(target.GetUsedNamespaces()))
		{
			context.CodeBuilder.Using(@namespace);
		}

		if (context.CodeBuilder.Changed)
		{
			context.CodeBuilder.NewLine();
		}

		if (target.TargetNamespace != "global")
		{
			context.CodeBuilder.Namespace(target.TargetNamespace);
		}

		foreach (IMemberData type in target.ContainingTypes.GetData())
		{
			context.CodeBuilder.Declaration(type.Symbol);
		}
	}

	private void GenerateAllVersionsOfTarget(IDefaultParamTarget target, DefaultParamPassContext context)
	{
		IDefaultParamDeclarationBuilder declBuilder = target.GetDeclarationBuilder(context.CancellationToken);
		context.Rewriter.Acquire(declBuilder);
		CSharpSyntaxNode[] members = CreateDefaultParamDeclarations(in target.TypeParameters, context);

		if (members.Length > 0)
		{
			WriteTargetLeadDeclaration(target, context);
			WriteGeneratedMembers(members, target, context);
			context.CodeBuilder.EndAllBlocks();
		}
	}
}
