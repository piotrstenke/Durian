// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Durian.Analysis.Cache
{
	/// <summary>
	/// <see cref="DurianGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/> that retrieves cached data from a <see cref="ConcurrentDictionary{TKey, TValue}"/>
	/// </summary>
	/// <typeparam name="TData">Type of cached values this generator can retrieve.</typeparam>
	/// <typeparam name="TCompilationData">User-defined type of <see cref="ICompilationData"/> this <see cref="IDurianGenerator"/> operates on.</typeparam>
	/// <typeparam name="TSyntaxReceiver">User-defined type of <see cref="IDurianSyntaxReceiver"/> that provides the <see cref="CSharpSyntaxNode"/>s to perform the generation on.</typeparam>
	/// <typeparam name="TFilter">User-defined type of <see cref="ISyntaxFilter"/> that decides what <see cref="CSharpSyntaxNode"/>s collected by the <see cref="DurianGenerator{TCompilationData, TSyntaxReceiver, TFilter}.SyntaxReceiver"/> are valid for generation.</typeparam>
	public abstract class CachedGenerator<TData, TCompilationData, TSyntaxReceiver, TFilter> : DurianGeneratorWithBuilder<TCompilationData, TSyntaxReceiver, TFilter>, ICachedGenerator<TData>
		where TData : IMemberData
		where TCompilationData : ICompilationData
		where TSyntaxReceiver : IDurianSyntaxReceiver
		where TFilter : ICachedGeneratorSyntaxFilterWithDiagnostics<TData>
	{
		/// <inheritdoc cref="CachedGenerator(LoggingConfiguration?, IHintNameProvider?)"/>
		protected CachedGenerator()
		{
		}

		/// <inheritdoc cref="CachedGenerator(LoggingConfiguration?, IHintNameProvider?)"/>
		protected CachedGenerator(in ConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedGenerator{TCompilationData, TSyntaxReceiver, TFilter, TData}"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected CachedGenerator(in ConstructionContext context, IHintNameProvider? fileNameProvider) : base(in context, fileNameProvider)
		{
		}

		/// <inheritdoc cref="CachedGenerator(LoggingConfiguration?, IHintNameProvider?)"/>
		protected CachedGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedGenerator{TData, TCompilationData, TSyntaxReceiver, TFilter}"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected CachedGenerator(LoggingConfiguration? loggingConfiguration, IHintNameProvider? fileNameProvider) : base(loggingConfiguration, fileNameProvider)
		{
		}

		/// <inheritdoc/>
		public void Execute(in CachedGeneratorExecutionContext<TData> context)
		{
			ResetData();

			ref readonly GeneratorExecutionContext c = ref context.GetContext();

			if (!InitializeCompilation(in c, out CSharpCompilation? compilation) ||
				c.SyntaxReceiver is not TSyntaxReceiver receiver ||
				!ValidateSyntaxReceiver(receiver)
			)
			{
				return;
			}

			try
			{
				InitializeExecutionData(compilation, receiver, in c);

				if (!HasValidData)
				{
					return;
				}

				Filtrate(in context, in c);
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
		/// Returns a <see cref="FilterContainer{TFilter}"/> to be used during the current generation pass.
		/// </summary>
		/// <param name="context">Current <see cref="CachedGeneratorExecutionContext{TData}"/>.</param>
		public virtual FilterContainer<TFilter>? GetFilters(in CachedGeneratorExecutionContext<TData> context)
		{
			return GetFilters(in context.GetContext());
		}

		/// <summary>
		/// Manually iterates through a <typeparamref name="TFilter"/> that has the <see cref="IGeneratorSyntaxFilter.IncludeGeneratedSymbols"/> property set to <see langword="true"/>.
		/// </summary>
		/// <param name="filter"><typeparamref name="TFilter"/> to iterate through.</param>
		/// <param name="context">Current <see cref="CachedGeneratorExecutionContext{TData}"/>.</param>
		protected virtual void IterateThroughFilter(TFilter filter, in CachedGeneratorExecutionContext<TData> context)
		{
			IEnumerator<IMemberData> iter = filter.GetEnumerator(in context);
			ref readonly GeneratorExecutionContext original = ref context.GetContext();

			while (iter.MoveNext())
			{
				GenerateFromData(iter.Current, in original);
			}
		}

		private void Filtrate(in CachedGeneratorExecutionContext<TData> cachedContext, in GeneratorExecutionContext originalContext)
		{
			FilterContainer<TFilter>? filters = GetFilters(in cachedContext);

			if (filters is null || filters.NumGroups == 0)
			{
				return;
			}

			OnBeforeExecution(in originalContext);

			foreach (FilterGroup<TFilter> filterGroup in filters)
			{
				HandleFilterGroup(filterGroup, in cachedContext, in originalContext);
			}

			OnAfterExecution(in originalContext);
		}

		private void GenerateFromFiltersWithGeneratedSymbols(FilterGroup<TFilter> filterGroup, List<TFilter> filtersWithGeneratedSymbols, in CachedGeneratorExecutionContext<TData> cachedContext, in GeneratorExecutionContext originalContext)
		{
			BeforeFiltersWithGeneratedSymbols();
			OnBeforeFiltrationOfGeneratedSymbols(filterGroup, in originalContext);
			BeginIterationOfFiltersWithGeneratedSymbols();

			foreach (TFilter filter in filtersWithGeneratedSymbols)
			{
				IterateThroughFilter(filter, in cachedContext);
			}

			EndIterationOfFiltersWithGeneratedSymbols();
		}

		private void HandleFilterGroup(FilterGroup<TFilter> filterGroup, in CachedGeneratorExecutionContext<TData> cachedContext, in GeneratorExecutionContext originalContext)
		{
			int numFilters = filterGroup.Count;
			List<TFilter> filtersWithGeneratedSymbols = new(numFilters);

			filterGroup.Unseal();
			OnBeforeFiltrationOfGroup(filterGroup, in originalContext);
			filterGroup.Seal();

			foreach (TFilter filter in filterGroup)
			{
				if (filter.IncludeGeneratedSymbols)
				{
					filtersWithGeneratedSymbols.Add(filter);
				}
				else
				{
					foreach (IMemberData data in filter.Filtrate(in cachedContext))
					{
						GenerateFromData(data, in originalContext);
					}
				}
			}

			OnBeforeExecutionOfGroup(filterGroup, in originalContext);

			GenerateFromFiltersWithGeneratedSymbols(filterGroup, filtersWithGeneratedSymbols, in cachedContext, in originalContext);

			filterGroup.Unseal();
			OnAfterExecutionOfGroup(filterGroup, in originalContext);
		}
	}

	/// <inheritdoc cref="CachedGenerator{TData, TCompilationData, TSyntaxReceiver, TFilter}"/>
	public abstract class CachedGenerator<TData, TCompilationData, TSyntaxReceiver> : CachedGenerator<TData, TCompilationData, TSyntaxReceiver, ICachedGeneratorSyntaxFilterWithDiagnostics<TData>>
		where TData : IMemberData
		where TCompilationData : ICompilationData
		where TSyntaxReceiver : IDurianSyntaxReceiver
	{
		/// <inheritdoc cref="CachedGenerator(in ConstructionContext, IHintNameProvider?)"/>
		protected CachedGenerator()
		{
		}

		/// <inheritdoc cref="CachedGenerator(in ConstructionContext, IHintNameProvider?)"/>
		protected CachedGenerator(in ConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedGenerator{TData, TCompilationData, TSyntaxReceiver}"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected CachedGenerator(in ConstructionContext context, IHintNameProvider? fileNameProvider) : base(in context, fileNameProvider)
		{
		}

		/// <inheritdoc cref="CachedGenerator(LoggingConfiguration?, IHintNameProvider?)"/>
		protected CachedGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedGenerator{TData, TCompilationData, TSyntaxReceiver}"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected CachedGenerator(LoggingConfiguration? loggingConfiguration, IHintNameProvider? fileNameProvider) : base(loggingConfiguration, fileNameProvider)
		{
		}
	}

	/// <inheritdoc cref="CachedGenerator{TData, TCompilationData, TSyntaxReceiver, TFilter}"/>
	public abstract class CachedGenerator<TData, TCompilationData> : CachedGenerator<TData, TCompilationData, IDurianSyntaxReceiver, ICachedGeneratorSyntaxFilterWithDiagnostics<TData>>
		where TData : IMemberData
		where TCompilationData : ICompilationData
	{
		/// <inheritdoc cref="CachedGenerator(in ConstructionContext, IHintNameProvider?)"/>
		protected CachedGenerator()
		{
		}

		/// <inheritdoc cref="CachedGenerator(in ConstructionContext, IHintNameProvider?)"/>
		protected CachedGenerator(in ConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedGenerator{TData, TCompilationData}"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected CachedGenerator(in ConstructionContext context, IHintNameProvider? fileNameProvider) : base(in context, fileNameProvider)
		{
		}

		/// <inheritdoc cref="CachedGenerator(LoggingConfiguration?, IHintNameProvider?)"/>
		protected CachedGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedGenerator{TData, TCompilationData}"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected CachedGenerator(LoggingConfiguration? loggingConfiguration, IHintNameProvider? fileNameProvider) : base(loggingConfiguration, fileNameProvider)
		{
		}
	}

	/// <inheritdoc cref="CachedGenerator{TData, TCompilationData, TSyntaxReceiver, TFilter}"/>
	public abstract class CachedGenerator<TData> : CachedGenerator<TData, ICompilationData, IDurianSyntaxReceiver, ICachedGeneratorSyntaxFilterWithDiagnostics<TData>>
		where TData : IMemberData
	{
		/// <inheritdoc cref="CachedGenerator(in ConstructionContext, IHintNameProvider?)"/>
		protected CachedGenerator()
		{
		}

		/// <inheritdoc cref="CachedGenerator(in ConstructionContext, IHintNameProvider?)"/>
		protected CachedGenerator(in ConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedGenerator{TData}"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected CachedGenerator(in ConstructionContext context, IHintNameProvider? fileNameProvider) : base(in context, fileNameProvider)
		{
		}

		/// <inheritdoc cref="CachedGenerator(LoggingConfiguration?, IHintNameProvider?)"/>
		protected CachedGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedGenerator{TData}"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected CachedGenerator(LoggingConfiguration? loggingConfiguration, IHintNameProvider? fileNameProvider) : base(loggingConfiguration, fileNameProvider)
		{
		}
	}
}