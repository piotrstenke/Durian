// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Durian.Analysis
{
	/// <summary>
	/// <see cref="DurianGeneratorBase"/> that uses <see cref="IGeneratorSyntaxFilter"/>s to generate sources.
	/// </summary>
	/// <typeparam name="TFilter">Type of <see cref="IGeneratorSyntaxFilter"/> used to generate sources.</typeparam>
	public abstract class GeneratorWithFilters<TFilter> : DurianGeneratorBase where TFilter : IGeneratorSyntaxFilter
	{
		/// <summary>
		/// Delegate that accepts a <see cref="GeneratorExecutionContext"/>.
		/// </summary>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		public delegate void ContextAction(in GeneratorExecutionContext context);

		/// <summary>
		/// Delegate that accepts a <typeparamref name="TFilter"/> and a <see cref="GeneratorExecutionContext"/>.
		/// </summary>
		/// <param name="filter">Current <typeparamref name="TFilter"/>.</param>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		public delegate void FilterAction(TFilter filter, in GeneratorExecutionContext context);

		/// <summary>
		/// Delegate that accepts a <see cref="FilterGroup{TFilter}"/> and a <see cref="GeneratorExecutionContext"/>.
		/// </summary>
		/// <param name="filterGroup">Current <see cref="FilterGroup{TFilter}"/>.</param>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		public delegate void FilterGroupAction(FilterGroup<TFilter> filterGroup, in GeneratorExecutionContext context);

		/// <summary>
		/// Determines whether the current <typeparamref name="TFilter"/> is run with generated symbols.
		/// </summary>
		protected bool IsFilterWithGeneratedSymbols { get; private set; }

		/// <summary>
		/// Determines whether the last execution of the <see cref="Execute(in GeneratorExecutionContext)"/> method was a success.
		/// </summary>
		public bool IsSuccess { get; private protected set; }

		/// <summary>
		/// Invoked after all code is generated.
		/// </summary>
		public event ContextAction? AfterExecution;

		/// <summary>
		/// Invoked after a <see cref="FilterGroup{TFilter}"/> is done executing.
		/// </summary>
		public event FilterGroupAction? AfterExecutionOfGroup;

		/// <summary>
		/// Invoked before any generation takes places.
		/// </summary>
		public event ContextAction? BeforeExecution;

		/// <summary>
		/// Invoked after <typeparamref name="TFilter"/>s with <see cref="IGeneratorSyntaxFilter.IncludeGeneratedSymbols"/> set to <see langword="false"/> are filtrated, but before actual generation.
		/// </summary>
		public event FilterGroupAction? BeforeExecutionOfGroup;

		/// <summary>
		/// Invoked after <typeparamref name="TFilter"/>s with <see cref="IGeneratorSyntaxFilter.IncludeGeneratedSymbols"/> set to <see langword="false"/> generated their code,
		/// but before <typeparamref name="TFilter"/>s with <see cref="IGeneratorSyntaxFilter.IncludeGeneratedSymbols"/> set to <see langword="true"/> are filtrated and executed.
		/// </summary>
		public event FilterGroupAction? BeforeFiltrationOfGeneratedSymbols;

		/// <summary>
		/// Invoked before the filtration a <see cref="FilterGroup{TFilter}"/> is started.
		/// </summary>
		public event FilterGroupAction? BeforeFiltrationOfGroup;

		internal event FilterAction? IterateThroughFilterEvent;

		/// <inheritdoc cref="GeneratorWithFilters(in ConstructionContext, IHintNameProvider?)"/>
		protected GeneratorWithFilters()
		{
		}

		/// <inheritdoc cref="GeneratorWithFilters(in ConstructionContext, IHintNameProvider?)"/>
		protected GeneratorWithFilters(in ConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneratorWithFilters{TFilter}"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected GeneratorWithFilters(in ConstructionContext context, IHintNameProvider? fileNameProvider) : base(in context, fileNameProvider)
		{
		}

		/// <inheritdoc cref="GeneratorWithFilters(LoggingConfiguration?, IHintNameProvider?)"/>
		protected GeneratorWithFilters(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneratorWithFilters{TFilter}"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected GeneratorWithFilters(LoggingConfiguration? loggingConfiguration, IHintNameProvider? fileNameProvider) : base(loggingConfiguration, fileNameProvider)
		{
		}

		/// <inheritdoc/>
		public override void Execute(in GeneratorExecutionContext context)
		{
			if (!InitializeCompilation(in context, out _))
			{
				return;
			}

			try
			{
				Filtrate(in context);
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
		/// Returns a <see cref="FilterContainer{TFilter}"/> to be used during the current generation pass.
		/// </summary>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		public virtual FilterContainer<TFilter>? GetFilters(in GeneratorExecutionContext context)
		{
			return GetFilters(FileNameProvider);
		}

		/// <summary>
		/// Returns a <see cref="FilterContainer{TFilter}"/> to be used during the current generation pass.
		/// </summary>
		/// <param name="fileNameProvider">Creates name for the generated files.</param>
		public abstract FilterContainer<TFilter> GetFilters(IHintNameProvider fileNameProvider);

		/// <summary>
		/// Called before filters with generated symbols are executed.
		/// </summary>
		protected virtual void BeforeFiltersWithGeneratedSymbols()
		{
			// Do nothing.
		}

		/// <summary>
		/// Performs node filtration.
		/// </summary>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		protected void Filtrate(in GeneratorExecutionContext context)
		{
			FilterContainer<TFilter>? filters = GetFilters(in context);

			if (filters is null || filters.NumGroups == 0)
			{
				return;
			}

			OnBeforeExecution(in context);

			foreach (FilterGroup<TFilter> filterGroup in filters)
			{
				HandleFilterGroup(filterGroup, in context);
			}

			OnAfterExecution(in context);
		}

		/// <inheritdoc/>
		protected abstract bool Generate(IMemberData member, string hintName, in GeneratorExecutionContext context);

		/// <summary>
		/// Performs the generation for the specified <paramref name="data"/>.
		/// </summary>
		/// <param name="data"><see cref="IMemberData"/> to perform the generation for.</param>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		protected internal void GenerateFromData(IMemberData data, in GeneratorExecutionContext context)
		{
			string name = FileNameProvider.GetHintName(data.Symbol);

			if (Generate(data, name, in context))
			{
				FileNameProvider.Success();
			}
		}

		/// <summary>
		/// Manually iterates through a <typeparamref name="TFilter"/> that has the <see cref="IGeneratorSyntaxFilter.IncludeGeneratedSymbols"/> property set to <see langword="true"/>.
		/// </summary>
		/// <param name="filter"><typeparamref name="TFilter"/> to iterate through.</param>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		protected virtual void IterateThroughFilter(TFilter filter, in GeneratorExecutionContext context)
		{
			IEnumerator<IMemberData> iter = filter.GetEnumerator();

			while (iter.MoveNext())
			{
				GenerateFromData(iter.Current, in context);
			}
		}

		/// <summary>
		/// Called after a <see cref="FilterGroup{TFilter}"/> is done executing.
		/// </summary>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		protected virtual void OnAfterExecution(in GeneratorExecutionContext context)
		{
			AfterExecution?.Invoke(context);
		}

		/// <summary>
		/// Called after a <see cref="FilterGroup{TFilter}"/> is done executing.
		/// </summary>
		/// <param name="filterGroup">Current <see cref="FilterGroup{TFilter}"/>.</param>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		protected virtual void OnAfterExecutionOfGroup(FilterGroup<TFilter> filterGroup, in GeneratorExecutionContext context)
		{
			AfterExecutionOfGroup?.Invoke(filterGroup, context);
		}

		/// <summary>
		/// Called before any generation takes places.
		/// </summary>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		protected virtual void OnBeforeExecution(in GeneratorExecutionContext context)
		{
			BeforeExecution?.Invoke(in context);
		}

		/// <summary>
		/// Called after <typeparamref name="TFilter"/>s with <see cref="IGeneratorSyntaxFilter.IncludeGeneratedSymbols"/> set to <see langword="false"/> are filtrated, but before actual generation.
		/// </summary>
		/// <param name="filterGroup">Current <see cref="FilterGroup{TFilter}"/>.</param>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		protected virtual void OnBeforeExecutionOfGroup(FilterGroup<TFilter> filterGroup, in GeneratorExecutionContext context)
		{
			BeforeExecutionOfGroup?.Invoke(filterGroup, in context);
		}

		/// <summary>
		/// Called after <typeparamref name="TFilter"/>s with <see cref="IGeneratorSyntaxFilter.IncludeGeneratedSymbols"/> set to <see langword="false"/> generated their code,
		/// but before <typeparamref name="TFilter"/>s with <see cref="IGeneratorSyntaxFilter.IncludeGeneratedSymbols"/> set to <see langword="true"/> are filtrated and executed.
		/// </summary>
		/// <param name="filterGroup">Current <see cref="FilterGroup{TFilter}"/>.</param>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		protected virtual void OnBeforeFiltrationOfGeneratedSymbols(FilterGroup<TFilter> filterGroup, in GeneratorExecutionContext context)
		{
			BeforeFiltrationOfGeneratedSymbols?.Invoke(filterGroup, in context);
		}

		/// <summary>
		/// Called before the filtration a <see cref="FilterGroup{TFilter}"/> is started.
		/// </summary>
		/// <param name="filterGroup">Current <see cref="FilterGroup{TFilter}"/>.</param>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		protected virtual void OnBeforeFiltrationOfGroup(FilterGroup<TFilter> filterGroup, in GeneratorExecutionContext context)
		{
			BeforeFiltrationOfGroup?.Invoke(filterGroup, in context);
		}

		private protected void BeginIterationOfFiltersWithGeneratedSymbols()
		{
			IsFilterWithGeneratedSymbols = true;
		}

		private protected void EndIterationOfFiltersWithGeneratedSymbols()
		{
			IsFilterWithGeneratedSymbols = false;
		}

		private void GenerateFromFiltersWithGeneratedSymbols(FilterGroup<TFilter> filterGroup, List<TFilter> filtersWithGeneratedSymbols, in GeneratorExecutionContext context)
		{
			BeforeFiltersWithGeneratedSymbols();
			OnBeforeFiltrationOfGeneratedSymbols(filterGroup, in context);
			BeginIterationOfFiltersWithGeneratedSymbols();

			if(IterateThroughFilterEvent is null)
			{
				foreach (TFilter filter in filtersWithGeneratedSymbols)
				{
					IterateThroughFilter(filter, in context);
				}
			}
			else
			{
				foreach (TFilter filter in filtersWithGeneratedSymbols)
				{
					IterateThroughFilterEvent(filter, in context);
				}
			}

			EndIterationOfFiltersWithGeneratedSymbols();
		}

		private void HandleFilterGroup(FilterGroup<TFilter> filterGroup, in GeneratorExecutionContext context)
		{
			int numFilters = filterGroup.Count;
			List<TFilter> filtersWithGeneratedSymbols = new(numFilters);

			filterGroup.Unseal();
			OnBeforeFiltrationOfGroup(filterGroup, in context);
			filterGroup.Seal();

			foreach (TFilter filter in filterGroup)
			{
				if (filter.IncludeGeneratedSymbols)
				{
					filtersWithGeneratedSymbols.Add(filter);
				}
				else
				{
					foreach (IMemberData data in filter.Filtrate(in context))
					{
						GenerateFromData(data, in context);
					}
				}
			}

			OnBeforeExecutionOfGroup(filterGroup, in context);

			GenerateFromFiltersWithGeneratedSymbols(filterGroup, filtersWithGeneratedSymbols, in context);

			filterGroup.Unseal();
			OnAfterExecutionOfGroup(filterGroup, in context);
		}
	}
}