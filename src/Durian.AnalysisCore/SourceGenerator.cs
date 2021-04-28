#if ENABLE_GENERATOR_LOGS
using System;
#endif
using System.Linq;
using System.Threading;
using Durian.Data;
using Durian.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian
{
	/// <inheritdoc cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/>
	public abstract class SourceGenerator
#if ENABLE_GENERATOR_DIAGNOSTICS
		: SourceGenerator<ICompilationData, IDurianSyntaxReceiver, IGeneratorSyntaxFilterWithDiagnostics>
#else
		: SourceGenerator<ICompilationData, IDurianSyntaxReceiver, IGeneratorSyntaxFilter>
#endif
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SourceGenerator"/> class.
		/// </summary>
		protected SourceGenerator()
		{
		}
	}

	/// <inheritdoc cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/>
	public abstract class SourceGenerator<TCompilationData>
#if ENABLE_GENERATOR_DIAGNOSTICS
		: SourceGenerator<TCompilationData, IDurianSyntaxReceiver, IGeneratorSyntaxFilterWithDiagnostics>
#else
		: SourceGenerator<TCompilationData, IDurianSyntaxReceiver, IGeneratorSyntaxFilter>
#endif
		where TCompilationData : class, ICompilationData
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SourceGenerator{TCompilationData}"/> class.
		/// </summary>
		protected SourceGenerator()
		{
		}
	}

	/// <inheritdoc cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/>
	public abstract class SourceGenerator<TCompilationData, TSyntaxReceiver>
#if ENABLE_GENERATOR_DIAGNOSTICS
		: SourceGenerator<TCompilationData, TSyntaxReceiver, IGeneratorSyntaxFilterWithDiagnostics>
#else
		: SourceGenerator<TCompilationData, TSyntaxReceiver, IGeneratorSyntaxFilter>
#endif
		where TCompilationData : class, ICompilationData
		where TSyntaxReceiver : class, IDurianSyntaxReceiver
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver}"/> class.
		/// </summary>
		protected SourceGenerator()
		{
		}
	}

	/// <summary>
	/// Abstract implementation of the <see cref="IDurianSourceGenerator"/> interface that performs early validation of the input <see cref="GeneratorExecutionContext"/>.
	/// </summary>
	/// <typeparam name="TCompilationData">User-defined type of <see cref="ICompilationData"/> this <see cref="IDurianSourceGenerator"/> operates on.</typeparam>
	/// <typeparam name="TSyntaxReceiver">User-defined type of <see cref="IDurianSyntaxReceiver"/> that provides the <see cref="CSharpSyntaxNode"/>s to perform the generation on.</typeparam>
	/// <typeparam name="TFilter">User-defined type of <see cref="ISyntaxFilter"/> that decides what <see cref="CSharpSyntaxNode"/>s collected by the <see cref="SyntaxReceiver"/> are valid for generation.</typeparam>
	public abstract class SourceGenerator<TCompilationData, TSyntaxReceiver, TFilter> : LoggableSourceGenerator, IDurianSourceGenerator
		where TCompilationData : class, ICompilationData
		where TSyntaxReceiver : class, IDurianSyntaxReceiver
#if ENABLE_GENERATOR_DIAGNOSTICS
		where TFilter : notnull, IGeneratorSyntaxFilterWithDiagnostics
#else
		where TFilter : notnull, IGeneratorSyntaxFilter
#endif
	{
#if ENABLE_GENERATOR_DIAGNOSTICS
#pragma warning disable IDE0032 // Use auto property
		private bool _enableDiagnostics;
#pragma warning restore IDE0032 // Use auto property
#endif

#if ENABLE_GENERATOR_LOGS
#endif

		/// <summary>
		/// A <see cref="IDiagnosticReceiver"/> that is used to report diagnostics.
		/// </summary>
#if ENABLE_GENERATOR_DIAGNOSTICS
		public new ReadonlyContextualDiagnosticReceiver<GeneratorExecutionContext> DiagnosticReceiver { get; } = DiagnosticReceiverFactory.SourceGenerator();
#else
		public new ReadonlyContextualDiagnosticReceiver<GeneratorExecutionContext>? DiagnosticReceiver { get; } = null;
#endif

		/// <inheritdoc cref="IDurianSourceGenerator.TargetCompilation"/>
		public TCompilationData TargetCompilation { get; private set; }

		/// <inheritdoc cref="IDurianSourceGenerator.SyntaxReceiver"/>
		public TSyntaxReceiver SyntaxReceiver { get; private set; }

		/// <inheritdoc/>
		public CSharpParseOptions ParseOptions { get; private set; }

		/// <inheritdoc/>
		public bool SupportsDiagnostics
		{
			get
			{
#if ENABLE_GENERATOR_DIAGNOSTICS
				return true;
#else
				return false;
#endif
			}
		}

#pragma warning disable IDE0027 // Use expression body for accessors
		/// <inheritdoc/>
		public bool EnableDiagnostics
		{
			get
			{
#if ENABLE_GENERATOR_DIAGNOSTICS
				return _enableDiagnostics;
#else
				return false;
#endif

			}
			set
			{
#if ENABLE_GENERATOR_DIAGNOSTICS

				_enableDiagnostics = value;
#endif
			}
		}
#pragma warning restore IDE0027 // Use expression body for accessors

		/// <summary>
		/// A <see cref="System.Threading.CancellationToken"/> that can be checked to see if the generation should be canceled.
		/// </summary>
		public CancellationToken CancellationToken { get; private set; }

		ICompilationData IDurianSourceGenerator.TargetCompilation => TargetCompilation;
		IDurianSyntaxReceiver IDurianSourceGenerator.SyntaxReceiver => SyntaxReceiver;
		string IDurianSourceGenerator.GeneratorName => GetGeneratorName();
		string IDurianSourceGenerator.Version => GetVersion();

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		/// <summary>
		/// Initializes a new instance of the <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/> class.
		/// </summary>
		protected SourceGenerator()
#if ENABLE_GENERATOR_LOGS
			: base(true)
#else
			: base(false)
#endif
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		protected SourceGenerator(GeneratorLoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		/// <summary>
		/// Initializes the source generator.
		/// </summary>
		/// <param name="context">The <see cref="GeneratorInitializationContext"/> to work on.</param>
		public override void Initialize(GeneratorInitializationContext context)
		{
			context.RegisterForSyntaxNotifications(CreateSyntaxReceiver);
		}

		/// <summary>
		/// Begins the generation.
		/// </summary>
		/// <param name="context">The <see cref="GeneratorInitializationContext"/> to work on.</param>
		public sealed override void Execute(in GeneratorExecutionContext context)
		{
			if (context.SyntaxReceiver is not TSyntaxReceiver receiver || !ValidateSyntaxReceiver(receiver))
			{
				return;
			}

			if (context.Compilation is not CSharpCompilation currentCompilation)
			{
				return;
			}

			TCompilationData? data = CreateCompilationData(currentCompilation);

			if (data is null || data.HasErrors)
			{
				return;
			}

			TargetCompilation = data;
			ParseOptions = context.ParseOptions as CSharpParseOptions ?? CSharpParseOptions.Default;
			SyntaxReceiver = receiver;
			CancellationToken = context.CancellationToken;

			BeforeFiltration(in context);
			Filtrate(in context);
		}

		/// <summary>
		/// Creates a new <see cref="IDurianSyntaxReceiver"/> to be used during the current generation pass.
		/// </summary>
		public abstract TSyntaxReceiver CreateSyntaxReceiver();

		IDurianSyntaxReceiver IDurianSourceGenerator.CreateSyntaxReceiver()
		{
			return CreateSyntaxReceiver();
		}

		/// <summary>
		/// Returns a list of <see cref="ISyntaxFilter"/>s to be used during the current generation pass.
		/// </summary>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		protected abstract FilterContainer<TFilter>? GetFilters(in GeneratorExecutionContext context);

		/// <summary>
		/// Validates the <paramref name="syntaxReceiver"/>.
		/// </summary>
		/// <param name="syntaxReceiver"><typeparamref name="TSyntaxReceiver"/> to validate.</param>
		protected virtual bool ValidateSyntaxReceiver(TSyntaxReceiver syntaxReceiver)
		{
			return !syntaxReceiver.IsEmpty();
		}

		/// <summary>
		/// Method called before node filtration is performed.
		/// </summary>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		protected virtual void BeforeFiltration(in GeneratorExecutionContext context)
		{
			// Do nothing by default.
		}

		/// <summary>
		/// Method called before the execution of <paramref name="filterGroup"/> is started.
		/// </summary>
		/// <param name="filterGroup">Current filter group. The group is sealed, so it is impossible to change its state.</param>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		protected virtual void BeforeFiltrationOfGroup(FilterGroup<TFilter> filterGroup, in GeneratorExecutionContext context)
		{
			// Do nothing by default.
		}

		/// <summary>
		/// Method called after the <paramref name="filterGroup"/> is done executing.
		/// </summary>
		/// <param name="filterGroup">Current filter group. The group is unsealed, so it is possible to change its state.</param>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		protected virtual void AfterFiltrationOfGroup(FilterGroup<TFilter> filterGroup, in GeneratorExecutionContext context)
		{
			// Do nothing by default.
		}

		/// <summary>
		/// Method called after node filtration is performed.
		/// </summary>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		protected virtual void AfterFiltration(in GeneratorExecutionContext context)
		{
			// Do nothing by default.
		}

		/// <summary>
		/// Actually begins the generator execution.
		/// </summary>
		/// <param name="member"><see cref="IMemberData"/> to generate the source for.</param>
		/// <param name="filter"><see cref="ISyntaxFilter"/> that collected the target <paramref name="member"/>.</param>
		/// <param name="context">The <see cref="GeneratorExecutionContext"/> to add source to.</param>
		protected abstract void Generate(IMemberData member, TFilter filter, in GeneratorExecutionContext context);

		/// <summary>
		/// Creates new instance of <see cref="ICompilationData"/>.
		/// </summary>
		/// <param name="compilation">Current <see cref="CSharpCompilation"/>.</param>
		protected abstract TCompilationData? CreateCompilationData(CSharpCompilation compilation);

		/// <summary>
		/// Adds the specified <paramref name="source"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="source">A <see cref="string"/> representation of the generated code.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorPostInitializationContext"/> to add the source to.</param>
		protected void InitializeSource(string source, string hintName, in GeneratorPostInitializationContext context)
		{
			CSharpSyntaxTree tree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(source);
			InitializeSource(tree, hintName, in context);
		}

		/// <summary>
		/// Adds the specified <paramref name="tree"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="tree"><see cref="CSharpSyntaxTree"/> to add to the <paramref name="context"/>.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorPostInitializationContext"/> to add the source to.</param>
		protected void InitializeSource(CSharpSyntaxTree tree, string hintName, in GeneratorPostInitializationContext context)
		{
			context.AddSource(hintName, tree.GetText(context.CancellationToken));

#if ENABLE_GENERATOR_LOGS
			LogNode(tree.GetRoot(context.CancellationToken), hintName);
#endif
		}

		/// <summary>
		/// Adds the text of the specified <paramref name="builder"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="builder"><see cref="CodeBuilder"/> that was used to build the generated code.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorPostInitializationContext"/> to add the source to.</param>
		protected void InitializeSource(CodeBuilder builder, string hintName, in GeneratorPostInitializationContext context)
		{
			CSharpSyntaxTree tree = builder.ParseSyntaxTree();
			builder.Clear();
			InitializeSource(tree, hintName, in context);
		}

		/// <summary>
		/// Adds the source created using the <paramref name="builder"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="builder"><see cref="CodeBuilder"/> that was used to build the generated code.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
		protected void AddSource(CodeBuilder builder, string hintName, in GeneratorExecutionContext context)
		{
			CSharpSyntaxTree tree = builder.ParseSyntaxTree();
			builder.Clear();
			AddSource(tree, hintName, in context);
		}

		/// <summary>
		/// Adds the generated <paramref name="source"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="source">The generated text.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
		protected void AddSource(string source, string hintName, in GeneratorExecutionContext context)
		{
			CSharpSyntaxTree tree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(source, ParseOptions, encoding: System.Text.Encoding.UTF8, cancellationToken: context.CancellationToken);
			AddSource(tree, hintName, in context);
		}

		/// <summary>
		/// Adds the generated <paramref name="tree"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="tree">The generated <see cref="CSharpSyntaxTree"/>.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
		protected void AddSource(CSharpSyntaxTree tree, string hintName, in GeneratorExecutionContext context)
		{
			context.AddSource(hintName, tree.GetText(context.CancellationToken));
			TargetCompilation.UpdateCompilation(tree);
		}

#if !ENABLE_GENERATOR_LOGS
#pragma warning disable RCS1163 // Unused parameter.
#endif

		/// <summary>
		/// Adds the source created using the <paramref name="builder"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="original">The <see cref="CSharpSyntaxNode"/> the source was generated from.</param>
		/// <param name="builder"><see cref="CodeBuilder"/> that was used to build the generated code.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
		protected void AddSource(CSharpSyntaxNode original, CodeBuilder builder, string hintName, in GeneratorExecutionContext context)
		{
			CSharpSyntaxTree tree = builder.ParseSyntaxTree();
			builder.Clear();
			AddSource(tree, hintName, in context);

#if ENABLE_GENERATOR_LOGS
			LogGeneratedTree(original, tree, hintName, context.CancellationToken);
#endif
		}

		/// <summary>
		/// Adds the generated <paramref name="text"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="original">The <see cref="CSharpSyntaxNode"/> the source was generated from.</param>
		/// <param name="text">The generated text.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
		protected void AddSource(CSharpSyntaxNode original, string text, string hintName, in GeneratorExecutionContext context)
		{
			CSharpSyntaxTree tree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(text, ParseOptions, encoding: System.Text.Encoding.UTF8, cancellationToken: context.CancellationToken);
			AddSource(tree, hintName, in context);

#if ENABLE_GENERATOR_LOGS
			LogGeneratedTree(original, tree, hintName, context.CancellationToken);
#endif
		}

		/// <summary>
		/// Adds the generated <paramref name="tree"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="original">The <see cref="CSharpSyntaxNode"/> the source was generated from.</param>
		/// <param name="tree">The generated <see cref="CSharpSyntaxTree"/>.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
		protected void AddSource(CSharpSyntaxNode original, CSharpSyntaxTree tree, string hintName, in GeneratorExecutionContext context)
		{
			AddSource(tree, hintName, in context);

#if ENABLE_GENERATOR_LOGS
			LogGeneratedTree(original, tree, hintName, context.CancellationToken);
#endif
		}

#if !ENABLE_GENERATOR_LOGS
#pragma warning restore RCS1163 // Unused parameter.
#endif

		private void Filtrate(in GeneratorExecutionContext context)
		{
			FilterContainer<TFilter>? filters = GetFilters(in context);

			if (filters is null || filters.NumGroups == 0)
			{
				return;
			}

			foreach (FilterGroup<TFilter> filterGroup in filters)
			{
				IMemberData[][] data = FiltrateUsingGroup(filterGroup);
				int length = filterGroup.Count;

				filterGroup.Seal();
				BeforeFiltrationOfGroup(filterGroup, in context);

				for (int i = 0; i < length; i++)
				{
					GenerateFromFilterResult(data[i], filterGroup[i], in context);
				}

				filterGroup.Unseal();
				AfterFiltrationOfGroup(filterGroup, in context);
			}

			AfterFiltration(in context);
		}

		private static IMemberData[][] FiltrateUsingGroup(FilterGroup<TFilter> filterGroup)
		{
			int length = filterGroup.Count;
			IMemberData[][] data = new IMemberData[length][];

			for (int i = 0; i < length; i++)
			{
				data[i] = filterGroup[i].Filtrate().ToArray();
			}

			return data;
		}

		private void GenerateFromFilterResult(IMemberData[] result, TFilter parentFilter, in GeneratorExecutionContext context)
		{
			foreach (IMemberData d in result)
			{
#if ENABLE_GENERATOR_LOGS
				try
				{
#endif
					Generate(d, parentFilter, in context);
#if ENABLE_GENERATOR_LOGS
				}
				catch (Exception e)
				{
					LogException(e);
					throw;
				}
#endif
			}
		}

#if ENABLE_GENERATOR_LOGS
		private void LogGeneratedTree(CSharpSyntaxNode original, CSharpSyntaxTree tree, string hintName, CancellationToken cancellationToken)
		{
			if (GeneratorLoggingConfiguration.IsEnabled && LoggingConfiguration.EnableLogging && LoggingConfiguration.SupportedLogs.HasFlag(GeneratorLogs.InputOutput))
			{
				LogInputOutput_Internal(original, tree.GetRoot(cancellationToken), hintName);
			}
		}
#endif
	}
}
