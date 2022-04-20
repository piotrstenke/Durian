// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using Durian.Analysis.Cache;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Durian.Analysis.Filters;
using Durian.Analysis.Logging;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.DefaultParam
{
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
		public override ICompilationData CreateCompilationData(CSharpCompilation compilation)
		{
			return new DefaultParamCompilationData(compilation);
		}

		/// <summary>
		/// Creates a new <see cref="DefaultParamSyntaxReceiver"/> to be used during the current generation pass.
		/// </summary>
		public override IDurianSyntaxReceiver CreateSyntaxReceiver()
		{
			return new DefaultParamSyntaxReceiver(LoggingConfiguration.SupportsDiagnostics);
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
		public override IEnumerable<ISourceTextProvider>? GetInitialSources()
		{
			return GetSourceProviders();
		}

		/// <inheritdoc/>
		public override DurianModule[] GetRequiredModules()
		{
			return new DurianModule[] { DurianModule.DefaultParam };
		}

		/// <inheritdoc/>
		protected internal override void BeforeExecution(DefaultParamPassContext context)
		{
			context.Rewriter.ParentCompilation = (DefaultParamCompilationData)context.TargetCompilation;
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
			AddSourceWithOriginal(target.Declaration, hintName, context);

			return true;
		}

		/// <inheritdoc/>
		protected override DefaultParamPassContext CreateCurrentPassContext(ICompilationData currentCompilation, in GeneratorExecutionContext context)
		{
			return new DefaultParamPassContext();
		}

		private static CSharpSyntaxNode[] CreateDefaultParamDeclarations(in TypeParameterContainer parameters, DefaultParamPassContext context)
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
				context.Rewriter.ReplaceType(data.Symbol, data.TargetType!, name);

				members[memberIndex] = context.Rewriter.CurrentNode;

				dataIndex--;
				memberIndex++;
			}

			length = originalLength;
			memberIndex = originalMemberIndex;
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
				return t.Arity > 0 ? t.GetGenericName(GenericSubstitution.TypeArguments) : AnalysisUtilities.TypeToKeyword(targetType.Name);
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
			context.CodeBuilder.WriteHeader(Name, Version);
			context.CodeBuilder.WriteLine();
			string[] namespaces = AnalysisUtilities.SortUsings(target.GetUsedNamespaces()).ToArray();

			if (namespaces.Length > 0)
			{
				context.CodeBuilder.WriteUsings(namespaces);
				context.CodeBuilder.WriteLine();
			}

			if (target.TargetNamespace != "global")
			{
				context.CodeBuilder.BeginNamespaceDeclaration(target.TargetNamespace);
			}

			context.CodeBuilder.WriteParentDeclarations(target.GetContainingTypes());
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
				context.CodeBuilder.EndAllScopes();
			}
		}
	}
}
