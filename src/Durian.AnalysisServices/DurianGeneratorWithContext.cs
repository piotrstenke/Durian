// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Durian.Analysis.Data;
using Durian.Analysis.Filters;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis
{
	/// <summary>
	/// Abstract implementation of the <see cref="IDurianGenerator"/> interface that performs early validation of the input <see cref="GeneratorExecutionContext"/>.
	/// </summary>
	/// <typeparam name="TContext">Type of <see cref="IGeneratorPassContext"/> this generator uses.</typeparam>
	public abstract class DurianGeneratorWithContext<TContext> : DurianGeneratorBase, IDurianGenerator where TContext : class, IGeneratorPassContext
	{
		private protected static class Registry
		{
			[ThreadStatic]
			private static readonly Dictionary<Guid, TContext> _data = new();

			public static TContext Get(Guid guid)
			{
				if (!_data.TryGetValue(guid, out TContext data))
				{
					throw new InvalidOperationException("Generator is not initialized");
				}

				return data;
			}

			public static void Remove(Guid guid)
			{
				_data.Remove(guid);
			}

			public static void Set(Guid guid, TContext data)
			{
				_data[guid] = data;
			}
		}

		string? IDurianGenerator.GeneratorName => GetGeneratorName();

		string? IDurianGenerator.GeneratorVersion => GetGeneratorVersion();

		/// <summary>
		/// Determines whether the last execution of the <see cref="Execute(in GeneratorExecutionContext)"/> method was a success.
		/// </summary>
		public bool IsSuccess { get; private protected set; }

		/// <summary>
		/// Determines whether data of this <see cref="DurianGeneratorWithContext{TContext}"/> was successfully initialized by the last call to the <see cref="Execute(in GeneratorExecutionContext)"/> method.
		/// </summary>
		public bool IsValid { get; set; }

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
		protected DurianGeneratorWithContext(in ConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGeneratorWithContext{TContext}"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		protected DurianGeneratorWithContext(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Creates a new instance of <see cref="IDurianSyntaxReceiver"/> to be used during the current generation pass.
		/// </summary>
		public abstract IDurianSyntaxReceiver CreateSyntaxReceiver();

		/// <summary>
		/// Begins the generation.
		/// </summary>
		/// <param name="context">The <see cref="GeneratorInitializationContext"/> to work on.</param>
		public sealed override void Execute(in GeneratorExecutionContext context)
		{
			ResetData();

			if (!InitializeCompilation(in context, out CSharpCompilation? compilation))
			{
				IsSuccess = false;
				return;
			}

			try
			{
				TContext? pass = CreateCurrentPassContext(compilation, in context);

				if (pass is null)
				{
					IsValid = false;
					IsSuccess = false;
					return;
				}

				IsValid = true;
				Registry.Set(InstanceId, pass);

				Filtrate(pass);
				IsSuccess = true;
			}
			catch (Exception e)
			{
				LogException(e);
				IsSuccess = false;

				if (EnableExceptions)
				{
					throw;
				}
			}
		}

		/// <summary>
		/// Performs node filtration.
		/// </summary>
		/// <param name="context">Current <typeparamref name="TContext"/>.</param>
		public void Filtrate(TContext context)
		{
			IReadOnlyFilterContainer<IGeneratorSyntaxFilter>? filters = GetFilters(context);

			if (filters is null || filters.NumGroups == 0)
			{
				return;
			}

			Filtrate(context, filters);
		}

		/// <summary>
		/// Performs node filtration.
		/// </summary>
		/// <param name="context">Current <typeparamref name="TContext"/>.</param>
		/// <param name="filters">Collection of <see cref="IGeneratorSyntaxFilter"/>s to filtrate the nodes with.</param>
		public void Filtrate(TContext context, IReadOnlyFilterContainer<IGeneratorSyntaxFilter> filters)
		{
			BeforeExecution(context);
			HandleFilterContainer(filters, context);
			AfterExecution(context);
		}

		IGeneratorPassContext IDurianGenerator.GetCurrentPassContext()
		{
			return GetCurrentPassContext();
		}

		/// <inheritdoc/>
		public TContext GetCurrentPassContext()
		{
			return Registry.Get(InstanceId);
		}

		/// <summary>
		/// Returns a <see cref="IReadOnlyFilterContainer{TFilter}"/> to be used during the current generation pass.
		/// </summary>
		/// <param name="context">Current <typeparamref name="TContext"/>.</param>
		public virtual IReadOnlyFilterContainer<IGeneratorSyntaxFilter>? GetFilters(TContext context)
		{
			return GetFilters(context.FileNameProvider);
		}

		/// <summary>
		/// Returns a <see cref="IReadOnlyFilterContainer{TFilter}"/> to be used during the current generation pass.
		/// </summary>
		/// <param name="fileNameProvider">Creates name for the generated files.</param>
		public abstract IReadOnlyFilterContainer<IGeneratorSyntaxFilter> GetFilters(IHintNameProvider fileNameProvider);

		/// <summary>
		/// Initializes the source generator.
		/// </summary>
		/// <param name="context">The <see cref="GeneratorInitializationContext"/> to work on.</param>
		public override void Initialize(GeneratorInitializationContext context)
		{
			base.Initialize(context);
			context.RegisterForSyntaxNotifications(CreateSyntaxReceiver);
		}

		/// <inheritdoc/>
		protected sealed override void AddSource(CSharpSyntaxTree syntaxTree, string hintName, in GeneratorPostInitializationContext context)
		{
			base.AddSource(syntaxTree, hintName, context);
		}

		/// <inheritdoc/>
		protected sealed override void AddSource(string source, string hintName, in GeneratorPostInitializationContext context)
		{
			base.AddSource(source, hintName, context);
		}

		/// <summary>
		/// Adds the generated <paramref name="source"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="source">The generated text.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><typeparamref name="TContext"/> to add the source to.</param>
		/// <exception cref="InvalidOperationException"><see cref="IsValid"/> must be <see langword="true"/> in order to add new source.</exception>
		protected void AddSource(string source, string hintName, TContext context)
		{
			ThrowIfHasNoValidData();

			CSharpSyntaxTree tree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(source, context.ParseOptions, encoding: System.Text.Encoding.UTF8, cancellationToken: context.CancellationToken);
			AddSource_Internal(tree, hintName, context);
		}

		/// <summary>
		/// Adds the generated <paramref name="tree"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="tree">The generated <see cref="CSharpSyntaxTree"/>.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><typeparamref name="TContext"/> to add the source to.</param>
		/// <exception cref="InvalidOperationException"><see cref="IsValid"/> must be <see langword="true"/> in order to add new source.</exception>
		protected void AddSource(CSharpSyntaxTree tree, string hintName, TContext context)
		{
			ThrowIfHasNoValidData();
			AddSource_Internal(tree, hintName, context);
		}

		/// <summary>
		/// Adds the generated <paramref name="source"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="source">The generated text.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
		/// <exception cref="InvalidOperationException"><see cref="IsValid"/> must be <see langword="true"/> in order to add new source.</exception>
		protected sealed override void AddSource(string source, string hintName, in GeneratorExecutionContext context)
		{
			ThrowIfHasNoValidData();

			CSharpSyntaxTree tree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(source, (CSharpParseOptions)context.ParseOptions, encoding: System.Text.Encoding.UTF8, cancellationToken: context.CancellationToken);
			AddSource_Internal(tree, hintName, in context);
		}

		/// <summary>
		/// Adds the generated <paramref name="tree"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="tree">The generated <see cref="CSharpSyntaxTree"/>.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
		/// <exception cref="InvalidOperationException"><see cref="IsValid"/> must be <see langword="true"/> in order to add new source.</exception>
		protected sealed override void AddSource(CSharpSyntaxTree tree, string hintName, in GeneratorExecutionContext context)
		{
			ThrowIfHasNoValidData();
			AddSource_Internal(tree, hintName, in context);
		}

		/// <summary>
		/// Directly adds the generated <paramref name="tree"/> to the <paramref name="context"/> without any validation.
		/// </summary>
		/// <param name="tree">The generated <see cref="CSharpSyntaxTree"/>.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><typeparamref name="TContext"/> to add the source to.</param>
		protected virtual void AddSourceCore(CSharpSyntaxTree tree, string hintName, TContext context)
		{
			context.OriginalContext.AddSource(hintName, tree.GetText(context.CancellationToken));
		}

		/// <summary>
		/// Directly adds the generated <paramref name="tree"/> to the <paramref name="context"/> without any validation.
		/// </summary>
		/// <param name="tree">The generated <see cref="CSharpSyntaxTree"/>.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
		protected virtual void AddSourceCore(CSharpSyntaxTree tree, string hintName, in GeneratorExecutionContext context)
		{
			context.AddSource(hintName, tree.GetText(context.CancellationToken));
		}

		/// <summary>
		/// Adds the generated <paramref name="text"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="original">The <see cref="CSharpSyntaxNode"/> the source was generated from.</param>
		/// <param name="text">The generated text.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><typeparamref name="TContext"/> to add the source to.</param>
		/// <exception cref="InvalidOperationException"><see cref="IsValid"/> must be <see langword="true"/> in order to add new source.</exception>
		protected void AddSourceWithOriginal(CSharpSyntaxNode original, string text, string hintName, TContext context)
		{
			ThrowIfHasNoValidData();
			CSharpSyntaxTree tree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(text, context.ParseOptions, encoding: System.Text.Encoding.UTF8, cancellationToken: context.CancellationToken);
			AddSource_Internal(original, tree, hintName, context);
		}

		/// <summary>
		/// Adds the generated <paramref name="tree"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="original">The <see cref="CSharpSyntaxNode"/> the source was generated from.</param>
		/// <param name="tree">The generated <see cref="CSharpSyntaxTree"/>.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><typeparamref name="TContext"/> to add the source to.</param>
		/// <exception cref="InvalidOperationException"><see cref="IsValid"/> must be <see langword="true"/> in order to add new source.</exception>
		protected void AddSourceWithOriginal(CSharpSyntaxNode original, CSharpSyntaxTree tree, string hintName, TContext context)
		{
			ThrowIfHasNoValidData();
			AddSource_Internal(original, tree, hintName, context);
		}

		/// <summary>
		/// Adds the generated <paramref name="text"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="original">The <see cref="CSharpSyntaxNode"/> the source was generated from.</param>
		/// <param name="text">The generated text.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
		/// <exception cref="InvalidOperationException"><see cref="IsValid"/> must be <see langword="true"/> in order to add new source.</exception>
		protected void AddSourceWithOriginal(CSharpSyntaxNode original, string text, string hintName, in GeneratorExecutionContext context)
		{
			ThrowIfHasNoValidData();
			CSharpSyntaxTree tree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(text, (CSharpParseOptions)context.ParseOptions, encoding: System.Text.Encoding.UTF8, cancellationToken: context.CancellationToken);
			AddSource_Internal(original, tree, hintName, in context);
		}

		/// <summary>
		/// Adds the generated <paramref name="tree"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="original">The <see cref="CSharpSyntaxNode"/> the source was generated from.</param>
		/// <param name="tree">The generated <see cref="CSharpSyntaxTree"/>.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
		/// <exception cref="InvalidOperationException"><see cref="IsValid"/> must be <see langword="true"/> in order to add new source.</exception>
		protected void AddSourceWithOriginal(CSharpSyntaxNode original, CSharpSyntaxTree tree, string hintName, in GeneratorExecutionContext context)
		{
			ThrowIfHasNoValidData();
			AddSource_Internal(original, tree, hintName, in context);
		}

		/// <summary>
		/// Called after all code is generated.
		/// </summary>
		/// <param name="context">Current <typeparamref name="TContext"/>.</param>
		protected virtual void AfterExecution(TContext context)
		{
			// Do nothing by default.
		}

		/// <summary>
		/// Called after a <see cref="IReadOnlyFilterGroup{TFilter}"/> is done executing.
		/// </summary>
		/// <param name="filterGroup">Current <see cref="IReadOnlyFilterGroup{TFilter}"/>.</param>
		/// <param name="context">Current <typeparamref name="TContext"/>.</param>
		protected virtual void AfterExecutionOfGroup(IReadOnlyFilterGroup<IGeneratorSyntaxFilter> filterGroup, TContext context)
		{
			// Do nothing by default.
		}

		/// <summary>
		/// Called before any generation takes places.
		/// </summary>
		/// <param name="context">Current <typeparamref name="TContext"/>.</param>
		protected virtual void BeforeExecution(TContext context)
		{
			// Do nothing by default.
		}

		/// <summary>
		/// Called after <see cref="IGeneratorSyntaxFilter"/>s with <see cref="IGeneratorSyntaxFilter.IncludeGeneratedSymbols"/> set to <see langword="false"/> are filtrated, but before actual generation.
		/// </summary>
		/// <param name="filterGroup">Current <see cref="IReadOnlyFilterGroup{TFilter}"/>.</param>
		/// <param name="context">Current <typeparamref name="TContext"/>.</param>
		protected virtual void BeforeExecutionOfGroup(IReadOnlyFilterGroup<IGeneratorSyntaxFilter> filterGroup, TContext context)
		{
			// Do nothing by default.;
		}

		/// <summary>
		/// Called before filters with generated symbols are executed.
		/// </summary>
		/// <param name="context">Current <typeparamref name="TContext"/>.</param>
		protected virtual void BeforeFiltersWithGeneratedSymbols(TContext context)
		{
			// Do nothing by default.
		}

		/// <summary>
		/// Called after <see cref="IGeneratorSyntaxFilter"/>s with <see cref="IGeneratorSyntaxFilter.IncludeGeneratedSymbols"/> set to <see langword="false"/> generated their code,
		/// but before <see cref="IGeneratorSyntaxFilter"/>s with <see cref="IGeneratorSyntaxFilter.IncludeGeneratedSymbols"/> set to <see langword="true"/> are filtrated and executed.
		/// </summary>
		/// <param name="filterGroup">Current <see cref="IReadOnlyFilterGroup{TFilter}"/>.</param>
		/// <param name="context">Current <typeparamref name="TContext"/>.</param>
		protected virtual void BeforeFiltrationOfGeneratedSymbols(IReadOnlyFilterGroup<IGeneratorSyntaxFilter> filterGroup, TContext context)
		{
			// Do nothing by default.
		}

		/// <summary>
		/// Called before the filtration a <see cref="IReadOnlyFilterGroup{TFilter}"/> is started.
		/// </summary>
		/// <param name="filterGroup">Current <see cref="IReadOnlyFilterGroup{TFilter}"/>.</param>
		/// <param name="context">Current <typeparamref name="TContext"/>.</param>
		protected virtual void BeforeFiltrationOfGroup(IReadOnlyFilterGroup<IGeneratorSyntaxFilter> filterGroup, TContext context)
		{
			// Do nothing by default.
		}

		/// <summary>
		/// Creates a new <typeparamref name="TContext"/> for the current generator pass.
		/// </summary>
		/// <param name="currentCompilation">Current <see cref="CSharpCompilation"/>.</param>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		protected abstract TContext? CreateCurrentPassContext(CSharpCompilation currentCompilation, in GeneratorExecutionContext context);

		/// <summary>
		/// Performs the generation for the specified <paramref name="data"/>.
		/// </summary>
		/// <param name="data"><see cref="IMemberData"/> to perform the generation for.</param>
		/// <param name="hintName">Name of the generated file.</param>
		/// <param name="context">Current <typeparamref name="TContext"/>.</param>
		protected abstract bool Generate(IMemberData data, string hintName, TContext context);

		/// <summary>
		/// Performs the generation for the specified <paramref name="data"/>.
		/// </summary>
		/// <param name="data"><see cref="IMemberData"/> to perform the generation for.</param>
		/// <param name="context">Current <typeparamref name="TContext"/>.</param>
		protected void GenerateFromData(IMemberData data, TContext context)
		{
			string name = context.FileNameProvider.GetHintName(data.Symbol);

			if (Generate(data, name, context))
			{
				context.FileNameProvider.Success();
			}
		}

		/// <summary>
		/// Performs node filtration without the <see cref="BeforeExecution"/> and <see cref="AfterExecution"/> callbacks.
		/// </summary>
		/// <param name="filters">Collection of <see cref="ISyntaxFilter"/>s to filtrate the nodes with.</param>
		/// <param name="context">Current <typeparamref name="TContext"/>.</param>
		protected virtual void HandleFilterContainer(IReadOnlyFilterContainer<IGeneratorSyntaxFilter> filters, TContext context)
		{
			foreach (IReadOnlyFilterGroup<IGeneratorSyntaxFilter> filterGroup in filters)
			{
				HandleFilterGroup(filterGroup, context);
			}
		}

		/// <summary>
		/// Manually iterates through a <see cref="ISyntaxFilter"/> that has the <see cref="IGeneratorSyntaxFilter.IncludeGeneratedSymbols"/> property set to <see langword="true"/>.
		/// </summary>
		/// <param name="filter"><see cref="IGeneratorSyntaxFilter"/> to iterate through.</param>
		/// <param name="context">Current <typeparamref name="TContext"/>.</param>
		protected virtual void IterateThroughFilter(IGeneratorSyntaxFilter filter, TContext context)
		{
			IEnumerator<IMemberData> iter = filter.GetEnumerator(context);

			while (iter.MoveNext())
			{
				GenerateFromData(iter.Current, context);
			}
		}

		/// <summary>
		/// Throws an <see cref="InvalidOperationException"/> if <see cref="IsValid"/> is <see langword="false"/> and <see cref="DurianGeneratorBase.EnableExceptions"/> is <see langword="true"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException"><see cref="IsValid"/> must be <see langword="true"/> in order to add a new source.</exception>
		protected void ThrowIfHasNoValidData()
		{
			if (!IsValid && EnableExceptions)
			{
				throw new InvalidOperationException($"{nameof(IsValid)} must be true in order to add a new source!");
			}
		}

		private protected void AddSource_Internal(CSharpSyntaxTree tree, string hintName, TContext context)
		{
			AddSourceCore(tree, hintName, context);

			if (LoggingConfiguration.EnableLogging && LoggingConfiguration.SupportedLogs.HasFlag(GeneratorLogs.Node))
			{
				LogNode_Internal(tree.GetRoot(context.CancellationToken), hintName, NodeOutput.Node);
			}
		}

		private protected void AddSource_Internal(CSharpSyntaxNode original, CSharpSyntaxTree tree, string hintName, TContext context)
		{
			AddSourceCore(tree, hintName, context);

			if (LoggingConfiguration.EnableLogging && LoggingConfiguration.SupportedLogs.HasFlag(GeneratorLogs.InputOutput))
			{
				LogInputOutput_Internal(original, tree.GetRoot(context.CancellationToken), hintName, default);
			}
		}

		private protected void AddSource_Internal(CSharpSyntaxTree tree, string hintName, in GeneratorExecutionContext context)
		{
			AddSourceCore(tree, hintName, in context);

			if (LoggingConfiguration.EnableLogging && LoggingConfiguration.SupportedLogs.HasFlag(GeneratorLogs.Node))
			{
				LogNode_Internal(tree.GetRoot(context.CancellationToken), hintName, NodeOutput.Node);
			}
		}

		private protected void AddSource_Internal(CSharpSyntaxNode original, CSharpSyntaxTree tree, string hintName, in GeneratorExecutionContext context)
		{
			AddSourceCore(tree, hintName, in context);

			if (LoggingConfiguration.EnableLogging && LoggingConfiguration.SupportedLogs.HasFlag(GeneratorLogs.InputOutput))
			{
				LogInputOutput_Internal(original, tree.GetRoot(context.CancellationToken), hintName, default);
			}
		}

		private protected void ResetData()
		{
			Registry.Remove(InstanceId);
			IsSuccess = false;
			IsValid = false;
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

		private void HandleFilterGroup(IReadOnlyFilterGroup<IGeneratorSyntaxFilter> filterGroup, TContext context)
		{
			int numFilters = filterGroup.Count;
			List<IGeneratorSyntaxFilter> filtersWithGeneratedSymbols = new(numFilters);

			//filterGroup.Unseal();
			BeforeFiltrationOfGroup(filterGroup, context);
			//filterGroup.Seal();

			foreach (IGeneratorSyntaxFilter filter in filterGroup)
			{
				if (filter.IncludeGeneratedSymbols)
				{
					filtersWithGeneratedSymbols.Add(filter);
				}
				else
				{
					foreach (IMemberData data in filter.Filtrate(context))
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
	}
}
