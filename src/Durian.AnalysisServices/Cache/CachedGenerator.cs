// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Durian.Analysis.Data;
using Durian.Analysis.Filters;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.Cache
{
	/// <summary>
	/// <see cref="DurianGenerator"/> that retrieves cached data from a <see cref="ConcurrentDictionary{TKey, TValue}"/>
	/// </summary>
	/// <typeparam name="TData">Type of cached values this generator can retrieve.</typeparam>
	/// <typeparam name="TContext">Type of <see cref="IGeneratorPassContext"/> this generator uses.</typeparam>
	public abstract class CachedGenerator<TData, TContext> : DurianGeneratorWithBuilder<TContext>, ICachedGenerator<TData>
		where TData : IMemberData
		where TContext : GeneratorPassBuilderContext
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CachedGenerator{TData, TContext}"/> class.
		/// </summary>
		protected CachedGenerator()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedGenerator{TData, TContext}"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="CachedGenerator{TData, TContext}"/> is initialized.</param>
		protected CachedGenerator(in GeneratorLogCreationContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedGenerator{TData, TContext}"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		protected CachedGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <inheritdoc/>
		public bool Execute(in CachedGeneratorExecutionContext<TData> context)
		{
			CachedGeneratorPassContext<TData, TContext>? pass;

			try
			{
				Reset(Environment.CurrentManagedThreadId);

				if (!InitializeCompilation(in context.GetContext(), out CSharpCompilation? compilation))
				{
					return false;
				}

				pass = CreateCurrentPassContext(compilation, in context);

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
			catch (Exception e) when (HandleException(e, pass.UnderlayingContext))
			{
				return false;
			}
		}

		/// <summary>
		/// Called to perform source generation. A generator can use the context to add source files via
		/// the Microsoft.CodeAnalysis.GeneratorExecutionContext.AddSource(System.String,Microsoft.CodeAnalysis.Text.SourceText) method.
		/// </summary>
		/// <param name="context"><see cref="CachedGeneratorPassContext{TData, TContext}"/> to add the sources to.</param>
		/// <exception cref="ArgumentNullException"><paramref name="context"/> is <see langword="null"/>.</exception>
		public bool Execute(CachedGeneratorPassContext<TData, TContext> context)
		{
			if (context is null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			try
			{
				return Execute_Internal(context);
			}
			catch (Exception e) when (HandleException(e, context.UnderlayingContext))
			{
				return false;
			}
		}

		/// <summary>
		/// Performs node filtration.
		/// </summary>
		/// <param name="context">Current <see cref="CachedGeneratorPassContext{TData, TContext}"/>.</param>
		public void Filtrate(CachedGeneratorPassContext<TData, TContext> context)
		{
			IReadOnlyFilterContainer<ICachedGeneratorSyntaxFilter<TData>>? filters = GetFilters(context);

			if (filters is null || filters.NumGroups == 0)
			{
				return;
			}

			Filtrate(context, filters);
		}

		/// <summary>
		/// Performs node filtration.
		/// </summary>
		/// <param name="context">Current <see cref="CachedGeneratorPassContext{TData, TContext}"/>.</param>
		/// <param name="filters">Collection of <see cref="IGeneratorSyntaxFilter"/>s to filtrate the nodes with.</param>
		public void Filtrate(CachedGeneratorPassContext<TData, TContext> context, IReadOnlyFilterContainer<ICachedGeneratorSyntaxFilter<TData>> filters)
		{
			BeforeExecution(context.UnderlayingContext);
			HandleFilterContainer(filters, context);
			AfterExecution(context.UnderlayingContext);
		}

		/// <summary>
		/// Returns a <see cref="IFilterContainer{TFilter}"/> to be used during the current generation pass.
		/// </summary>
		/// <param name="context">Current <see cref="CachedGeneratorPassContext{TData, TContext}"/>.</param>
		public virtual IReadOnlyFilterContainer<ICachedGeneratorSyntaxFilter<TData>>? GetFilters(CachedGeneratorPassContext<TData, TContext> context)
		{
			IReadOnlyFilterContainer<IGeneratorSyntaxFilter>? filters = GetFilters(context.UnderlayingContext);

			if (filters is IReadOnlyFilterContainer<ICachedGeneratorSyntaxFilter<TData>> r)
			{
				return r;
			}

			if (filters is null)
			{
				return null;
			}

			return new FilterContainer<ICachedGeneratorSyntaxFilter<TData>>(filters.Cast<FilterGroup<ICachedGeneratorSyntaxFilter<TData>>>());
		}

		/// <summary>
		/// Creates a new <see cref="CachedGeneratorPassContext{TData, TContext}"/> for the current generator pass.
		/// </summary>
		/// <param name="currentCompilation">Current <see cref="CSharpCompilation"/>.</param>
		/// <param name="context">Current <see cref="CachedGeneratorExecutionContext{T}"/>.</param>
		protected CachedGeneratorPassContext<TData, TContext>? CreateCurrentPassContext(CSharpCompilation currentCompilation, in CachedGeneratorExecutionContext<TData> context)
		{
			ref readonly GeneratorExecutionContext original = ref context.GetContext();

			TContext? pass = CreateCurrentPassContext(currentCompilation, in original);

			if (pass is null)
			{
				return null;
			}

			return new CachedGeneratorPassContext<TData, TContext>(pass, in context);
		}

		/// <summary>
		/// Performs node filtration without the <see cref="DurianGeneratorWithContext{TContext}.BeforeExecution"/> and <see cref="DurianGeneratorWithContext{TContext}.AfterExecution"/> callbacks.
		/// </summary>
		/// <param name="filters">Collection of <see cref="ISyntaxFilter"/>s to filtrate the nodes with.</param>
		/// <param name="context">Current <see cref="CachedGeneratorPassContext{TData, TContextData}"/>.</param>
		protected virtual void HandleFilterContainer(IReadOnlyFilterContainer<ICachedGeneratorSyntaxFilter<TData>> filters, CachedGeneratorPassContext<TData, TContext> context)
		{
			foreach (IReadOnlyFilterGroup<ICachedGeneratorSyntaxFilter<TData>> filterGroup in filters)
			{
				HandleFilterGroup(filterGroup, context);
			}
		}

		/// <summary>
		/// Manually iterates through a <see cref="ISyntaxFilter"/> that has the <see cref="IGeneratorSyntaxFilter.IncludeGeneratedSymbols"/> property set to <see langword="true"/>.
		/// </summary>
		/// <param name="filter"><see cref="ICachedGeneratorSyntaxFilter{T}"/> to iterate through.</param>
		/// <param name="context">Current <see cref="CachedGeneratorPassContext{TData, TContext}"/>.</param>
		protected virtual void IterateThroughFilter(ICachedGeneratorSyntaxFilter<TData> filter, CachedGeneratorPassContext<TData, TContext> context)
		{
			IEnumerator<IMemberData> iter = filter.GetEnumerator(context);

			while (iter.MoveNext())
			{
				GenerateFromData(iter.Current, context.UnderlayingContext);
			}
		}

		private bool Execute_Internal(CachedGeneratorPassContext<TData, TContext> context)
		{
			GeneratorContextRegistry.AddContext(InstanceId, Environment.CurrentManagedThreadId, context);
			Filtrate(context);
			return true;
		}

		private void GenerateFromFiltersWithGeneratedSymbols(
			IReadOnlyFilterGroup<ICachedGeneratorSyntaxFilter<TData>> filterGroup,
			List<ICachedGeneratorSyntaxFilter<TData>> filtersWithGeneratedSymbols,
			CachedGeneratorPassContext<TData, TContext> context
		)
		{
			BeforeFiltersWithGeneratedSymbols(context.UnderlayingContext);
			BeforeFiltrationOfGeneratedSymbols(filterGroup, context.UnderlayingContext);

			foreach (ICachedGeneratorSyntaxFilter<TData> filter in filtersWithGeneratedSymbols)
			{
				IterateThroughFilter(filter, context);
			}
		}

		private void HandleFilterGroup(IReadOnlyFilterGroup<ICachedGeneratorSyntaxFilter<TData>> filterGroup, CachedGeneratorPassContext<TData, TContext> context)
		{
			int numFilters = filterGroup.Count;
			List<ICachedGeneratorSyntaxFilter<TData>> filtersWithGeneratedSymbols = new(numFilters);

			//filterGroup.Unseal();
			BeforeFiltrationOfGroup(filterGroup, context.UnderlayingContext);
			//filterGroup.Seal();

			foreach (ICachedGeneratorSyntaxFilter<TData> filter in filterGroup)
			{
				if (filter.IncludeGeneratedSymbols)
				{
					filtersWithGeneratedSymbols.Add(filter);
				}
				else
				{
					foreach (IMemberData data in filter.Filtrate(context))
					{
						GenerateFromData(data, context.UnderlayingContext);
					}
				}
			}

			BeforeExecutionOfGroup(filterGroup, context.UnderlayingContext);

			GenerateFromFiltersWithGeneratedSymbols(filterGroup, filtersWithGeneratedSymbols, context);

			//filterGroup.Unseal();
			AfterExecutionOfGroup(filterGroup, context.UnderlayingContext);
		}
	}

	/// <inheritdoc cref="CachedGenerator{TData, TContext}"/>
	public abstract class CachedGenerator<TData> : CachedGenerator<TData, GeneratorPassBuilderContext> where TData : class, IMemberData
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CachedGenerator{TData}"/> class.
		/// </summary>
		protected CachedGenerator()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedGenerator{TData}"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="CachedGenerator{TData}"/> is initialized.</param>
		protected CachedGenerator(in GeneratorLogCreationContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedGenerator{TData}"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		protected CachedGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <inheritdoc/>
		protected override GeneratorPassBuilderContext CreateCurrentPassContext(ICompilationData currentCompilation, in GeneratorExecutionContext context)
		{
			return new GeneratorPassBuilderContext();
		}
	}
}
