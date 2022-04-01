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
	public abstract class CachedGenerator<TData> : DurianGeneratorWithBuilder, ICachedGenerator<TData>
		where TData : IMemberData
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
		/// <param name="context">Configures how this <see cref="LoggableGenerator"/> is initialized.</param>
		protected CachedGenerator(in ConstructionContext context) : base(in context)
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
		public void Execute(in CachedGeneratorExecutionContext<TData> context)
		{
			ResetData();

			ref readonly GeneratorExecutionContext c = ref context.GetContext();

			if (!InitializeCompilation(in c, out CSharpCompilation? compilation))
			{
				IsSuccess = false;
				return;
			}

			try
			{
				CachedGeneratorPassContext<TData>? pass = CreateCurrentPassContext(compilation, in context);

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
		/// <param name="context">Current <see cref="CachedGeneratorPassContext{TData}"/>.</param>
		public void Filtrate(CachedGeneratorPassContext<TData> context)
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
		/// <param name="context">Current <see cref="CachedGeneratorPassContext{TData}"/>.</param>
		/// <param name="filters">Collection of <see cref="IGeneratorSyntaxFilter"/>s to filtrate the nodes with.</param>
		public void Filtrate(CachedGeneratorPassContext<TData> context, IReadOnlyFilterContainer<ICachedGeneratorSyntaxFilter<TData>> filters)
		{
			BeforeExecution(context);
			HandleFilterContainer(filters, context);
			AfterExecution(context);
		}

		/// <summary>
		/// Returns a <see cref="IFilterContainer{TFilter}"/> to be used during the current generation pass.
		/// </summary>
		/// <param name="context">Current <see cref="CachedGeneratorPassContext{TData}"/>.</param>
		public virtual IReadOnlyFilterContainer<ICachedGeneratorSyntaxFilter<TData>>? GetFilters(CachedGeneratorPassContext<TData> context)
		{
			IReadOnlyFilterContainer<IGeneratorSyntaxFilter> filters = GetFilters(context.FileNameProvider);

			if (filters is IReadOnlyFilterContainer<ICachedGeneratorSyntaxFilter<TData>> r)
			{
				return r;
			}

			return new FilterContainer<ICachedGeneratorSyntaxFilter<TData>>(filters.Cast<FilterGroup<ICachedGeneratorSyntaxFilter<TData>>>());
		}

		/// <summary>
		/// Creates a new <see cref="CachedGeneratorPassContext{TData}"/> for the current generator pass.
		/// </summary>
		/// <param name="currentCompilation">Current <see cref="CSharpCompilation"/>.</param>
		/// <param name="context">Current <see cref="CachedGeneratorExecutionContext{T}"/>.</param>
		protected CachedGeneratorPassContext<TData>? CreateCurrentPassContext(CSharpCompilation currentCompilation, in CachedGeneratorExecutionContext<TData> context)
		{
			ref readonly GeneratorExecutionContext original = ref context.GetContext();

			if (original.SyntaxReceiver is not IDurianSyntaxReceiver syntaxReceiver || !ValidateSyntaxReceiver(syntaxReceiver))
			{
				return default;
			}

			ICompilationData? data = CreateCompilationData(currentCompilation);

			if (data is null || data.HasErrors)
			{
				return default;
			}

			IGeneratorServiceContainer services = new GeneratorServiceContainer();

			ConfigureServices(services);

			CachedGeneratorPassContext<TData> pass = CreateCurrentPassContext(data, in context);

			pass.CancellationToken = original.CancellationToken;
			pass.Services = services;
			pass.DiagnosticReceiver = EnableDiagnostics ? DiagnosticReceiver.Factory.SourceGenerator(in original) : default;
			pass.LogReceiver = EnableLogging ? new LoggableDiagnosticReceiver(this) : default;
			pass.TargetCompilation = data;
			pass.Generator = this;
			pass.SyntaxReceiver = syntaxReceiver;
			pass._originalContext = original;
			pass._cachedContext = context;

			return pass;
		}

		/// <summary>
		/// Creates a new <see cref="CachedGeneratorPassContext{TData}"/> for the current generator pass.
		/// </summary>
		/// <param name="currentCompilation">Current <see cref="ICompilationData"/>.</param>
		/// <param name="context">Current <see cref="CachedGeneratorExecutionContext{T}"/>.</param>
		protected virtual CachedGeneratorPassContext<TData> CreateCurrentPassContext(ICompilationData currentCompilation, in CachedGeneratorExecutionContext<TData> context)
		{
			return new CachedGeneratorPassContext<TData>();
		}

		/// <summary>
		/// Performs node filtration without the <see cref="DurianGeneratorWithContext{TContext}.BeforeExecution"/> and <see cref="DurianGeneratorWithContext{TContext}.AfterExecution"/> callbacks.
		/// </summary>
		/// <param name="filters">Collection of <see cref="ISyntaxFilter"/>s to filtrate the nodes with.</param>
		/// <param name="context">Current <see cref="CachedGeneratorPassContext{TData}"/>.</param>
		protected virtual void HandleFilterContainer(IReadOnlyFilterContainer<ICachedGeneratorSyntaxFilter<TData>> filters, CachedGeneratorPassContext<TData> context)
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
		/// <param name="context">Current <see cref="CachedGeneratorPassContext{TData}"/>.</param>
		protected virtual void IterateThroughFilter(ICachedGeneratorSyntaxFilter<TData> filter, CachedGeneratorPassContext<TData> context)
		{
			IEnumerator<IMemberData> iter = filter.GetEnumerator(context);

			while (iter.MoveNext())
			{
				GenerateFromData(iter.Current, context);
			}
		}

		private void GenerateFromFiltersWithGeneratedSymbols(
			IReadOnlyFilterGroup<ICachedGeneratorSyntaxFilter<TData>> filterGroup,
			List<ICachedGeneratorSyntaxFilter<TData>> filtersWithGeneratedSymbols,
			CachedGeneratorPassContext<TData> context
		)
		{
			BeforeFiltersWithGeneratedSymbols(context);
			BeforeFiltrationOfGeneratedSymbols(filterGroup, context);

			foreach (ICachedGeneratorSyntaxFilter<TData> filter in filtersWithGeneratedSymbols)
			{
				IterateThroughFilter(filter, context);
			}
		}

		private void HandleFilterGroup(IReadOnlyFilterGroup<ICachedGeneratorSyntaxFilter<TData>> filterGroup, CachedGeneratorPassContext<TData> context)
		{
			int numFilters = filterGroup.Count;
			List<ICachedGeneratorSyntaxFilter<TData>> filtersWithGeneratedSymbols = new(numFilters);

			//filterGroup.Unseal();
			BeforeFiltrationOfGroup(filterGroup, context);
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
