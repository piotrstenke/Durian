using System;
using System.Collections.Generic;
using System.ComponentModel;
using Durian.Analysis.Data;
using Durian.Analysis.Filtering;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Durian.Analysis;

/// <summary>
/// Abstract implementation of the <see cref="IDurianGenerator"/> interface that performs early validation of the input <see cref="GeneratorExecutionContext"/>.
/// </summary>
/// <typeparam name="TContext">Type of <see cref="IGeneratorPassContext"/> this generator uses.</typeparam>
public abstract class DurianGeneratorWithContext<TContext> : DurianBasicGenerator where TContext : class, IGeneratorPassContext
{
	private readonly int _sourceThreadId = Environment.CurrentManagedThreadId;

	/// <summary>
	/// Initializes a new instance of the <see cref="DurianGeneratorWithContext{TContext}"/> class.
	/// </summary>
	protected DurianGeneratorWithContext()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DurianGeneratorWithContext{TContext}"/> class.
	/// </summary>
	/// <param name="context">Configures how this <see cref="DurianGeneratorWithContext{TContext}"/> is initialized.</param>
	protected DurianGeneratorWithContext(in GeneratorLogCreationContext context) : base(in context)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DurianGeneratorWithContext{TContext}"/> class.
	/// </summary>
	/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
	protected DurianGeneratorWithContext(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
	{
	}

	/// <inheritdoc/>
	public sealed override bool Execute(GeneratorExecutionContext context)
	{
		TContext? pass;

		try
		{
			Reset(Environment.CurrentManagedThreadId);

			if (!PrepareForExecution(context, out CSharpCompilation? compilation))
			{
				return false;
			}

			pass = CreateCurrentPassContext(compilation, context);

			if (pass is null)
			{
				return false;
			}
		}
		catch when (!LoggingConfiguration.EnableExceptions)
		{
			return false;
		}

		try
		{
			return Execute_Internal(pass);
		}
		catch (Exception e) when (HandleException(e, pass))
		{
			return false;
		}
	}

	/// <summary>
	/// Called to perform source generation. A generator can use the context to add source files via
	/// the Microsoft.CodeAnalysis.GeneratorExecutionContext.AddSource(System.String,Microsoft.CodeAnalysis.Text.SourceText) method.
	/// </summary>
	/// <param name="context"><typeparamref name="TContext"/> to add the sources to.</param>
	/// <exception cref="ArgumentNullException"><paramref name="context"/> is <see langword="null"/>.</exception>
	public bool Execute(TContext context)
	{
		if (context is null)
		{
			throw new ArgumentNullException(nameof(context));
		}

		try
		{
			return Execute_Internal(context);
		}
		catch (Exception e) when (HandleException(e, context))
		{
			return false;
		}
	}

	/// <summary>
	/// Performs node filtration.
	/// </summary>
	/// <param name="context">Current <typeparamref name="TContext"/>.</param>
	public void Filter(TContext context)
	{
		IReadOnlyFilterContainer<IGeneratorSyntaxFilter>? filters = GetFilters(context);

		if (filters is null || filters.NumGroups == 0)
		{
			return;
		}

		Filter(context, filters);
	}

	/// <summary>
	/// Performs node filtration.
	/// </summary>
	/// <param name="context">Current <typeparamref name="TContext"/>.</param>
	/// <param name="filters">Collection of <see cref="IGeneratorSyntaxFilter"/>s to filter the nodes with.</param>
	public void Filter(TContext context, IReadOnlyFilterContainer<IGeneratorSyntaxFilter> filters)
	{
		BeforeExecution(context);
		HandleFilterContainer(filters, context);
		AfterExecution(context);
	}

	/// <summary>
	/// Returns a <typeparamref name="TContext"/> used during generator pass on the main thread.
	/// </summary>
	/// <exception cref="ArgumentException">Context not found for the specified <see cref="GeneratorThreadHandle"/>.</exception>
	/// <exception cref="InvalidOperationException">Registered context is not of type <typeparamref name="TContext"/>.</exception>
	public new TContext GetCurrentPassContext()
	{
		GeneratorThreadHandle handle = GetThreadHandle();
		return GeneratorContextRegistry.GetContext<TContext>(handle);
	}

	/// <summary>
	/// Returns a <typeparamref name="TContext"/>  used during generator pass on the thread with the specified <paramref name="threadId"/>.
	/// </summary>
	/// <exception cref="ArgumentException">Context not found for the specified <see cref="GeneratorThreadHandle"/>.</exception>
	/// <exception cref="InvalidOperationException">Registered context is not of type <typeparamref name="TContext"/>.</exception>
	public TContext GetCurrentPassContext(int threadId)
	{
		GeneratorThreadHandle handle = GetThreadHandle(threadId);
		return GeneratorContextRegistry.GetContext<TContext>(handle);
	}

	/// <summary>
	/// Returns a <see cref="IReadOnlyFilterContainer{TFilter}"/> to be used during the current generation pass.
	/// </summary>
	/// <param name="context">Current <typeparamref name="TContext"/>.</param>
	public abstract IReadOnlyFilterContainer<IGeneratorSyntaxFilter>? GetFilters(TContext context);

	/// <summary>
	/// Returns a new <see cref="GeneratorThreadHandle"/> targeting the current thread.
	/// </summary>
	public GeneratorThreadHandle GetThreadHandle()
	{
		return GetThreadHandle(Environment.CurrentManagedThreadId);
	}

	/// <summary>
	/// Returns a new <see cref="GeneratorThreadHandle"/> targeting a thread with the specified <paramref name="threadId"/>.
	/// </summary>
	/// <param name="threadId">Id of the target thread.</param>
	public GeneratorThreadHandle GetThreadHandle(int threadId)
	{
		return new GeneratorThreadHandle(InstanceId, threadId, _sourceThreadId);
	}

	/// <summary>
	/// Initializes the source generator.
	/// </summary>
	/// <param name="context">The <see cref="GeneratorInitializationContext"/> to work on.</param>
	public override void Initialize(GeneratorInitializationContext context)
	{
		base.Initialize(context);
		context.RegisterForSyntaxNotifications(CreateSyntaxReceiver);
	}

	/// <summary>
	/// Removes all the registered <typeparamref name="TContext"/>s associated with this generator.
	/// </summary>
	public void Reset()
	{
		GeneratorContextRegistry.RemoveAllContexts(InstanceId);
	}

	/// <summary>
	/// Removes the <typeparamref name="TContext"/> associated with this generator and a thread with the specified <paramref name="threadId"/>.
	/// </summary>
	/// <param name="threadId">Id of thread to remove the <typeparamref name="TContext"/> associated with.</param>
	public void Reset(int threadId)
	{
		GeneratorContextRegistry.RemoveContext(InstanceId, threadId);
	}

	/// <summary>
	/// Creates a new instance of <see cref="IDurianSyntaxReceiver"/> to be used during the current generation pass.
	/// </summary>
	protected internal abstract IDurianSyntaxReceiver CreateSyntaxReceiver();

	/// <summary>
	/// Directly adds the generated <paramref name="tree"/> to the <paramref name="context"/> without any validation.
	/// </summary>
	/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
	/// <param name="tree">The generated <see cref="SyntaxTree"/>.</param>
	/// <param name="context"><typeparamref name="TContext"/> to add the source to.</param>
	protected internal virtual void AddSourceCore(string hintName, SyntaxTree tree, TContext context)
	{
		context.OriginalContext.AddSource(hintName, tree.GetText(context.CancellationToken));
	}

	/// <summary>
	/// Directly adds the generated <paramref name="tree"/> to the <paramref name="context"/> without any validation.
	/// </summary>
	/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
	/// <param name="tree">The generated <see cref="SyntaxTree"/>.</param>
	/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
	protected internal virtual void AddSourceCore(string hintName, SyntaxTree tree, GeneratorExecutionContext context)
	{
		context.AddSource(hintName, tree.GetText(context.CancellationToken));
	}

	/// <summary>
	/// Called after all code is generated.
	/// </summary>
	/// <param name="context">Current <typeparamref name="TContext"/>.</param>
	protected internal virtual void AfterExecution(TContext context)
	{
		// Do nothing by default.
	}

	/// <summary>
	/// Called after a <see cref="IReadOnlyFilterGroup{TFilter}"/> is done executing.
	/// </summary>
	/// <param name="filterGroup">Current <see cref="IReadOnlyFilterGroup{TFilter}"/>.</param>
	/// <param name="context">Current <typeparamref name="TContext"/>.</param>
	protected internal virtual void AfterExecutionOfGroup(IReadOnlyFilterGroup<IGeneratorSyntaxFilter> filterGroup, TContext context)
	{
		// Do nothing by default.
	}

	/// <summary>
	/// Called before any generation takes places.
	/// </summary>
	/// <param name="context">Current <typeparamref name="TContext"/>.</param>
	protected internal virtual void BeforeExecution(TContext context)
	{
		// Do nothing by default.
	}

	/// <summary>
	/// Called after <see cref="IGeneratorSyntaxFilter"/>s with <see cref="IGeneratorSyntaxFilter.IncludeGeneratedSymbols"/> set to <see langword="false"/> are filtered, but before actual generation.
	/// </summary>
	/// <param name="filterGroup">Current <see cref="IReadOnlyFilterGroup{TFilter}"/>.</param>
	/// <param name="context">Current <typeparamref name="TContext"/>.</param>
	protected internal virtual void BeforeExecutionOfGroup(IReadOnlyFilterGroup<IGeneratorSyntaxFilter> filterGroup, TContext context)
	{
		// Do nothing by default.;
	}

	/// <summary>
	/// Called before filters with generated symbols are executed.
	/// </summary>
	/// <param name="context">Current <typeparamref name="TContext"/>.</param>
	protected internal virtual void BeforeFiltersWithGeneratedSymbols(TContext context)
	{
		// Do nothing by default.
	}

	/// <summary>
	/// Called after <see cref="IGeneratorSyntaxFilter"/>s with <see cref="IGeneratorSyntaxFilter.IncludeGeneratedSymbols"/> set to <see langword="false"/> generated their code,
	/// but before <see cref="IGeneratorSyntaxFilter"/>s with <see cref="IGeneratorSyntaxFilter.IncludeGeneratedSymbols"/> set to <see langword="true"/> are filtered and executed.
	/// </summary>
	/// <param name="filterGroup">Current <see cref="IReadOnlyFilterGroup{TFilter}"/>.</param>
	/// <param name="context">Current <typeparamref name="TContext"/>.</param>
	protected internal virtual void BeforeFiltrationOfGeneratedSymbols(IReadOnlyFilterGroup<IGeneratorSyntaxFilter> filterGroup, TContext context)
	{
		// Do nothing by default.
	}

	/// <summary>
	/// Called before the filtration a <see cref="IReadOnlyFilterGroup{TFilter}"/> is started.
	/// </summary>
	/// <param name="filterGroup">Current <see cref="IReadOnlyFilterGroup{TFilter}"/>.</param>
	/// <param name="context">Current <typeparamref name="TContext"/>.</param>
	protected internal virtual void BeforeFilteringOfGroup(IReadOnlyFilterGroup<IGeneratorSyntaxFilter> filterGroup, TContext context)
	{
		// Do nothing by default.
	}

	/// <summary>
	/// Creates a new <typeparamref name="TContext"/> for the current generator pass.
	/// </summary>
	/// <param name="currentCompilation">Current <see cref="CSharpCompilation"/>.</param>
	/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
	protected internal abstract TContext? CreateCurrentPassContext(CSharpCompilation currentCompilation, GeneratorExecutionContext context);

	/// <summary>
	/// Performs the generation for the specified <paramref name="data"/>.
	/// </summary>
	/// <param name="data"><see cref="IMemberData"/> to perform the generation for.</param>
	/// <param name="hintName">Name of the generated file.</param>
	/// <param name="context">Current <typeparamref name="TContext"/>.</param>
	protected internal abstract bool Generate(IMemberData data, string hintName, TContext context);

	/// <summary>
	/// Performs the generation for the specified <paramref name="data"/>.
	/// </summary>
	/// <param name="data"><see cref="IMemberData"/> to perform the generation for.</param>
	/// <param name="context">Current <typeparamref name="TContext"/>.</param>
	protected internal void GenerateFromData(IMemberData data, TContext context)
	{
		string name = context.FileNameProvider.GetHintName(data.Symbol);

		if (Generate(data, name, context))
		{
			context.FileNameProvider.Success();
		}
	}

	/// <summary>
	/// Manually iterates through a <see cref="ISyntaxFilter"/> that has the <see cref="IGeneratorSyntaxFilter.IncludeGeneratedSymbols"/> property set to <see langword="true"/>.
	/// </summary>
	/// <param name="filter"><see cref="IGeneratorSyntaxFilter"/> to iterate through.</param>
	/// <param name="context">Current <typeparamref name="TContext"/>.</param>
	protected internal virtual void IterateThroughFilter(IGeneratorSyntaxFilter filter, TContext context)
	{
		IEnumerator<IMemberData> iter = filter.GetEnumerator(context);

		while (iter.MoveNext())
		{
			GenerateFromData(iter.Current, context);
		}
	}

	/// <summary>
	/// Executed upon detecting an <see cref="Exception"/>.
	/// </summary>
	/// <param name="e"><see cref="Exception"/> that was detected.</param>
	/// <param name="context">Current <typeparamref name="TContext"/>.</param>
	/// <param name="allowLog">Determines whether to log the <see cref="Exception"/>.</param>
	protected internal virtual void OnException(Exception e, TContext context, bool allowLog)
	{
		if (allowLog)
		{
			LogHandler.LogException(e);
		}
	}

	/// <inheritdoc/>
	protected sealed override void AddSource(string hintName, SyntaxTree syntaxTree, GeneratorPostInitializationContext context)
	{
		base.AddSource(hintName, syntaxTree, context);
	}

	/// <inheritdoc/>
	protected sealed override void AddSource(string hintName, SourceText source, GeneratorPostInitializationContext context)
	{
		base.AddSource(hintName, source, context);
	}

	/// <inheritdoc/>
	protected sealed override void AddSource(string hintName, string source, GeneratorPostInitializationContext context)
	{
		base.AddSource(hintName, source, context);
	}

	/// <summary>
	/// Adds the generated <paramref name="source"/> to the <paramref name="context"/>.
	/// </summary>
	/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
	/// <param name="source">The generated text.</param>
	/// <param name="context"><typeparamref name="TContext"/> to add the source to.</param>
	protected void AddSource(string hintName, string source, TContext context)
	{
		SyntaxTree tree = CSharpSyntaxTree.ParseText(source, context.ParseOptions as CSharpParseOptions, encoding: System.Text.Encoding.UTF8, cancellationToken: context.CancellationToken);
		AddSource_Internal(hintName, tree, context);
	}

	/// <summary>
	/// Adds the generated <paramref name="tree"/> to the <paramref name="context"/>.
	/// </summary>
	/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
	/// <param name="tree">The generated <see cref="SyntaxTree"/>.</param>
	/// <param name="context"><typeparamref name="TContext"/> to add the source to.</param>
	protected void AddSource(string hintName, SyntaxTree tree, TContext context)
	{
		AddSource_Internal(hintName, tree, context);
	}

	/// <summary>
	/// Adds the generated <paramref name="source"/> to the <paramref name="context"/>.
	/// </summary>
	/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
	/// <param name="source">The generated text.</param>
	/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
	protected sealed override void AddSource(string hintName, string source, GeneratorExecutionContext context)
	{
		SyntaxTree tree = CSharpSyntaxTree.ParseText(source, context.ParseOptions as CSharpParseOptions, encoding: System.Text.Encoding.UTF8, cancellationToken: context.CancellationToken);
		AddSource_Internal(hintName, tree, context);
	}

	/// <summary>
	/// Adds the generated <paramref name="tree"/> to the <paramref name="context"/>.
	/// </summary>
	/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
	/// <param name="tree">The generated <see cref="SyntaxTree"/>.</param>
	/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
	protected sealed override void AddSource(string hintName, SyntaxTree tree, GeneratorExecutionContext context)
	{
		AddSource_Internal(hintName, tree, context);
	}

	/// <summary>
	/// Adds the generated <paramref name="text"/> to the <paramref name="context"/>.
	/// </summary>
	/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
	/// <param name="original">The <see cref="SyntaxNode"/> the source was generated from.</param>
	/// <param name="text">The generated text.</param>
	/// <param name="context"><typeparamref name="TContext"/> to add the source to.</param>
	protected void AddSourceWithOriginal(string hintName, SyntaxNode original, string text, TContext context)
	{
		SyntaxTree tree = CSharpSyntaxTree.ParseText(text, context.ParseOptions as CSharpParseOptions, encoding: System.Text.Encoding.UTF8, cancellationToken: context.CancellationToken);
		AddSource_Internal(hintName, original, tree, context);
	}

	/// <summary>
	/// Adds the generated <paramref name="tree"/> to the <paramref name="context"/>.
	/// </summary>
	/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
	/// <param name="original">The <see cref="SyntaxNode"/> the source was generated from.</param>
	/// <param name="tree">The generated <see cref="SyntaxTree"/>.</param>
	/// <param name="context"><typeparamref name="TContext"/> to add the source to.</param>
	protected void AddSourceWithOriginal(string hintName, SyntaxNode original, SyntaxTree tree, TContext context)
	{
		AddSource_Internal(hintName, original, tree, context);
	}

	/// <summary>
	/// Adds the generated <paramref name="text"/> to the <paramref name="context"/>.
	/// </summary>
	/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
	/// <param name="original">The <see cref="SyntaxNode"/> the source was generated from.</param>
	/// <param name="text">The generated text.</param>
	/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
	protected void AddSourceWithOriginal(string hintName, SyntaxNode original, string text, GeneratorExecutionContext context)
	{
		SyntaxTree tree = CSharpSyntaxTree.ParseText(text, context.ParseOptions as CSharpParseOptions, encoding: System.Text.Encoding.UTF8, cancellationToken: context.CancellationToken);
		AddSource_Internal(hintName, original, tree, context);
	}

	/// <summary>
	/// Adds the generated <paramref name="tree"/> to the <paramref name="context"/>.
	/// </summary>
	/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
	/// <param name="original">The <see cref="SyntaxNode"/> the source was generated from.</param>
	/// <param name="tree">The generated <see cref="SyntaxTree"/>.</param>
	/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
	protected void AddSourceWithOriginal(string hintName, SyntaxNode original, SyntaxTree tree, GeneratorExecutionContext context)
	{
		AddSource_Internal(hintName, original, tree, context);
	}

	/// <inheritdoc/>
	protected override void Dispose(bool disposing)
	{
		GeneratorContextRegistry.RemoveAllContexts(InstanceId);
	}

	/// <inheritdoc/>
	[Obsolete("Use GetCurrentPassContext() instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
	protected sealed override IGeneratorPassContext? GetCurrentPassContextCore()
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
	{
		return GetCurrentPassContext(Environment.CurrentManagedThreadId);
	}

	/// <summary>
	/// Performs node filtration without the <see cref="BeforeExecution"/> and <see cref="AfterExecution"/> callbacks.
	/// </summary>
	/// <param name="filters">Collection of <see cref="ISyntaxFilter"/>s to filter the nodes with.</param>
	/// <param name="context">Current <typeparamref name="TContext"/>.</param>
	protected void HandleFilterContainer(IReadOnlyFilterContainer<IGeneratorSyntaxFilter> filters, TContext context)
	{
		foreach (IReadOnlyFilterGroup<IGeneratorSyntaxFilter> filterGroup in filters)
		{
			HandleFilterGroup(filterGroup, context);
		}
	}

	internal bool HandleException(Exception e, TContext context)
	{
		OnException(e, context, true);
		return !LoggingConfiguration.EnableExceptions;
	}

	internal void HandleFilterGroup(IReadOnlyFilterGroup<IGeneratorSyntaxFilter> filterGroup, TContext context)
	{
		int numFilters = filterGroup.Count;
		List<IGeneratorSyntaxFilter> filtersWithGeneratedSymbols = new(numFilters);

		//filterGroup.Unseal();
		BeforeFilteringOfGroup(filterGroup, context);
		//filterGroup.Seal();

		foreach (IGeneratorSyntaxFilter filter in filterGroup)
		{
			if (filter.IncludeGeneratedSymbols)
			{
				filtersWithGeneratedSymbols.Add(filter);
			}
			else
			{
				foreach (IMemberData data in filter.Filter(context))
				{
					GenerateFromData(data, context);
				}
			}
		}

		BeforeExecutionOfGroup(filterGroup, context);

		GenerateFromFiltersWithGeneratedSymbols(filterGroup, filtersWithGeneratedSymbols, context);

		//filterGroup.Unseal();
		AfterExecutionOfGroup(filterGroup, context);
	}

	private protected void AddSource_Internal(string hintName, SyntaxTree tree, TContext context)
	{
		AddSourceCore(hintName, tree, context);

		LogHandler.LogNode(tree.GetRoot(context.CancellationToken), hintName, NodeOutput.Node);
	}

	private protected void AddSource_Internal(string hintName, SyntaxNode original, SyntaxTree tree, TContext context)
	{
		AddSourceCore(hintName, tree, context);

		LogHandler.LogInputOutput(original, tree.GetRoot(context.CancellationToken), hintName, default);
	}

	private protected void AddSource_Internal(string hintName, SyntaxTree tree, GeneratorExecutionContext context)
	{
		AddSourceCore(hintName, tree, context);

		LogHandler.LogNode(tree.GetRoot(context.CancellationToken), hintName, NodeOutput.Node);
	}

	private protected void AddSource_Internal(string hintName, SyntaxNode original, SyntaxTree tree, GeneratorExecutionContext context)
	{
		AddSourceCore(hintName, tree, context);

		LogHandler.LogInputOutput(original, tree.GetRoot(context.CancellationToken), hintName, default);
	}

	private bool Execute_Internal(TContext context)
	{
		GeneratorContextRegistry.AddContext(InstanceId, Environment.CurrentManagedThreadId, context);
		Filter(context);
		return true;
	}

	private void GenerateFromFiltersWithGeneratedSymbols(IReadOnlyFilterGroup<IGeneratorSyntaxFilter> filterGroup, List<IGeneratorSyntaxFilter> filtersWithGeneratedSymbols, TContext context)
	{
		BeforeFiltersWithGeneratedSymbols(context);
		BeforeFiltrationOfGeneratedSymbols(filterGroup, context);

		foreach (IGeneratorSyntaxFilter filter in filtersWithGeneratedSymbols)
		{
			IterateThroughFilter(filter, context);
		}
	}
}
