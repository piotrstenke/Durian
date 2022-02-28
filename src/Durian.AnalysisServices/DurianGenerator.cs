// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Durian.Analysis.Data;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis
{
	/// <summary>
	/// Abstract implementation of the <see cref="IDurianGenerator"/> interface that performs early validation of the input <see cref="GeneratorExecutionContext"/>.
	/// </summary>
	/// <typeparam name="TCompilationData">User-defined type of <see cref="ICompilationData"/> this <see cref="IDurianGenerator"/> operates on.</typeparam>
	/// <typeparam name="TSyntaxReceiver">User-defined type of <see cref="IDurianSyntaxReceiver"/> that provides the <see cref="CSharpSyntaxNode"/>s to perform the generation on.</typeparam>
	/// <typeparam name="TFilter">User-defined type of <see cref="ISyntaxFilter"/> that decides what <see cref="CSharpSyntaxNode"/>s collected by the <see cref="SyntaxReceiver"/> are valid for generation.</typeparam>
	public abstract class DurianGenerator<TCompilationData, TSyntaxReceiver, TFilter> : DurianGeneratorBase, IDurianGenerator
		where TCompilationData : ICompilationDataWithSymbols
		where TSyntaxReceiver : IDurianSyntaxReceiver
		where TFilter : IGeneratorSyntaxFilterWithDiagnostics
	{
		private readonly List<CSharpSyntaxTree> _generatedDuringCurrentPass = new(16);

		private bool _isFilterWithGeneratedSymbols;

		/// <inheritdoc/>
		public CancellationToken CancellationToken { get; private set; }

		/// <summary>
		/// Determines whether data of this <see cref="DurianGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/> was successfully initialized by the last call to the <see cref="Execute(in GeneratorExecutionContext)"/> method.
		/// </summary>
		[MemberNotNullWhen(true, nameof(TargetCompilation), nameof(SyntaxReceiver), nameof(ParseOptions))]
		public bool HasValidData { get; private set; }

		/// <summary>
		/// Determines whether the last execution of the <see cref="Execute(in GeneratorExecutionContext)"/> method was a success.
		/// </summary>
		[MemberNotNullWhen(true, nameof(TargetCompilation), nameof(SyntaxReceiver), nameof(ParseOptions))]
		public bool IsSuccess { get; private protected set; }

		/// <inheritdoc cref="IDurianGenerator.ParseOptions"/>
		public CSharpParseOptions? ParseOptions { get; private set; }

		/// <inheritdoc cref="IDurianGenerator.SyntaxReceiver"/>
		public TSyntaxReceiver? SyntaxReceiver { get; private set; }

		/// <inheritdoc cref="IDurianGenerator.TargetCompilation"/>
		public TCompilationData? TargetCompilation { get; private set; }

		string? IDurianGenerator.GeneratorName => GetGeneratorName();
		CSharpParseOptions IDurianGenerator.ParseOptions => ParseOptions!;
		IDurianSyntaxReceiver IDurianGenerator.SyntaxReceiver => SyntaxReceiver!;
		ICompilationData IDurianGenerator.TargetCompilation => TargetCompilation!;
		string? IDurianGenerator.GeneratorVersion => GetGeneratorVersion();

		/// <inheritdoc cref="DurianGenerator{TCompilationData, TSyntaxReceiver, TFilter}.DurianGenerator(in ConstructionContext, IHintNameProvider?)"/>
		protected DurianGenerator()
		{
		}

		/// <inheritdoc cref="DurianGenerator{TCompilationData, TSyntaxReceiver, TFilter}.DurianGenerator(in ConstructionContext, IHintNameProvider?)"/>
		protected DurianGenerator(in ConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected DurianGenerator(in ConstructionContext context, IHintNameProvider? fileNameProvider) : base(in context, fileNameProvider)
		{
		}

		/// <inheritdoc cref="DurianGenerator(LoggingConfiguration?, IHintNameProvider?)"/>
		protected DurianGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected DurianGenerator(LoggingConfiguration? loggingConfiguration, IHintNameProvider? fileNameProvider) : base(loggingConfiguration, fileNameProvider)
		{
		}

		/// <summary>
		/// Creates a new <typeparamref name="TSyntaxReceiver"/> to be used during the current generation pass.
		/// </summary>
		public abstract TSyntaxReceiver CreateSyntaxReceiver();

		/// <summary>
		/// Begins the generation.
		/// </summary>
		/// <param name="context">The <see cref="GeneratorInitializationContext"/> to work on.</param>
		public sealed override void Execute(in GeneratorExecutionContext context)
		{
			ResetData();

			if (!InitializeCompilation(in context, out CSharpCompilation? compilation) ||
				context.SyntaxReceiver is not TSyntaxReceiver receiver ||
				!ValidateSyntaxReceiver(receiver)
			)
			{
				return;
			}

			try
			{
				InitializeExecutionData(compilation, receiver, in context);

				if (!HasValidData)
				{
					return;
				}

				Filtrate(in context);
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
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		public abstract FilterContainer<TFilter>? GetFilters(in GeneratorExecutionContext context);

		/// <summary>
		/// Initializes the source generator.
		/// </summary>
		/// <param name="context">The <see cref="GeneratorInitializationContext"/> to work on.</param>
		public override void Initialize(GeneratorInitializationContext context)
		{
			base.Initialize(context);
			context.RegisterForSyntaxNotifications(() => CreateSyntaxReceiver());
		}

		IDurianSyntaxReceiver IDurianGenerator.CreateSyntaxReceiver()
		{
			return CreateSyntaxReceiver();
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
		/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
		/// <exception cref="InvalidOperationException"><see cref="HasValidData"/> must be <see langword="true"/> in order to add new source.</exception>
		protected sealed override void AddSource(string source, string hintName, in GeneratorExecutionContext context)
		{
			ThrowIfHasNoValidData();

			CSharpSyntaxTree tree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(source, ParseOptions, encoding: System.Text.Encoding.UTF8, cancellationToken: context.CancellationToken);
			AddSource_Internal(tree, hintName, in context);
		}

		/// <summary>
		/// Adds the generated <paramref name="tree"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="tree">The generated <see cref="CSharpSyntaxTree"/>.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
		/// <exception cref="InvalidOperationException"><see cref="HasValidData"/> must be <see langword="true"/> in order to add new source.</exception>
		protected sealed override void AddSource(CSharpSyntaxTree tree, string hintName, in GeneratorExecutionContext context)
		{
			ThrowIfHasNoValidData();
			AddSource_Internal(tree, hintName, in context);
		}

		private protected void AddSource_Internal(CSharpSyntaxTree tree, string hintName, in GeneratorExecutionContext context)
		{
			context.AddSource(hintName, tree.GetText(context.CancellationToken));

			if (_isFilterWithGeneratedSymbols)
			{
				TargetCompilation!.UpdateCompilation(tree);
			}
			else
			{
				_generatedDuringCurrentPass.Add(tree);
			}

			if (LoggingConfiguration.EnableLogging && LoggingConfiguration.SupportedLogs.HasFlag(GeneratorLogs.Node))
			{
				LogNode_Internal(tree.GetRoot(context.CancellationToken), hintName);
			}
		}

		private protected void AddSource_Internal(CSharpSyntaxNode original, CSharpSyntaxTree tree, string hintName, in GeneratorExecutionContext context)
		{
			context.AddSource(hintName, tree.GetText(context.CancellationToken));

			if (_isFilterWithGeneratedSymbols)
			{
				TargetCompilation!.UpdateCompilation(tree);
			}
			else
			{
				_generatedDuringCurrentPass.Add(tree);
			}

			if (LoggingConfiguration.EnableLogging && LoggingConfiguration.SupportedLogs.HasFlag(GeneratorLogs.InputOutput))
			{
				LogInputOutput_Internal(original, tree.GetRoot(context.CancellationToken), hintName);
			}
		}

		/// <summary>
		/// Adds the generated <paramref name="text"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="original">The <see cref="CSharpSyntaxNode"/> the source was generated from.</param>
		/// <param name="text">The generated text.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
		/// <exception cref="InvalidOperationException"><see cref="HasValidData"/> must be <see langword="true"/> in order to add new source.</exception>
		protected void AddSourceWithOriginal(CSharpSyntaxNode original, string text, string hintName, in GeneratorExecutionContext context)
		{
			ThrowIfHasNoValidData();
			CSharpSyntaxTree tree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(text, ParseOptions, encoding: System.Text.Encoding.UTF8, cancellationToken: context.CancellationToken);
			AddSource_Internal(original, tree, hintName, in context);
		}

		/// <summary>
		/// Adds the generated <paramref name="tree"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="original">The <see cref="CSharpSyntaxNode"/> the source was generated from.</param>
		/// <param name="tree">The generated <see cref="CSharpSyntaxTree"/>.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
		/// <exception cref="InvalidOperationException"><see cref="HasValidData"/> must be <see langword="true"/> in order to add new source.</exception>
		protected void AddSourceWithOriginal(CSharpSyntaxNode original, CSharpSyntaxTree tree, string hintName, in GeneratorExecutionContext context)
		{
			ThrowIfHasNoValidData();
			AddSource_Internal(original, tree, hintName, in context);
		}

		/// <summary>
		/// Method called after all code is generated.
		/// </summary>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		protected virtual void AfterExecution(in GeneratorExecutionContext context)
		{
			// Do nothing by default.
		}

		/// <summary>
		/// Method called after the <paramref name="filterGroup"/> is done executing.
		/// </summary>
		/// <remarks>The <paramref name="filterGroup"/> is unsealed, so it is possible to change its state.</remarks>
		/// <param name="filterGroup">Current filter group. The group is unsealed, so it is possible to change its contents.</param>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		/// <exception cref="InvalidOperationException">Parent <see cref="FilterContainer{TFilter}"/> is sealed.</exception>
		protected virtual void AfterExecutionOfGroup(FilterGroup<TFilter> filterGroup, in GeneratorExecutionContext context)
		{
			// Do nothing by default.
		}

		/// <summary>
		/// Method called before any generation takes places.
		/// </summary>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		protected virtual void BeforeExecution(in GeneratorExecutionContext context)
		{
			// Do nothing by default.
		}

		/// <summary>
		/// Method called after <typeparamref name="TFilter"/>s with <see cref="IGeneratorSyntaxFilter.IncludeGeneratedSymbols"/> set to <see langword="false"/> are filtrated, but before actual generation.
		/// </summary>
		/// <remarks>The <paramref name="filterGroup"/> is sealed, so it is not possible to change its state.</remarks>
		/// <param name="filterGroup">Current <see cref="FilterGroup{TFilter}"/>.</param>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		/// <exception cref="InvalidOperationException"><see cref="FilterGroup{TFilter}"/> is sealed. -or- Parent <see cref="FilterContainer{TFilter}"/> is sealed.</exception>
		protected virtual void BeforeExecutionOfGroup(FilterGroup<TFilter> filterGroup, in GeneratorExecutionContext context)
		{
			// Do nothing by default.
		}

		/// <summary>
		/// Method called after <typeparamref name="TFilter"/>s with <see cref="IGeneratorSyntaxFilter.IncludeGeneratedSymbols"/> set to <see langword="false"/> generated their code,
		/// but before <typeparamref name="TFilter"/>s with <see cref="IGeneratorSyntaxFilter.IncludeGeneratedSymbols"/> set to <see langword="true"/> are filtrated and executed.
		/// </summary>
		/// <remarks>The <paramref name="filterGroup"/> is sealed, so it is not possible to change its state.</remarks>
		/// <param name="filterGroup">Current <see cref="FilterGroup{TFilter}"/>.</param>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		/// <exception cref="InvalidOperationException"><see cref="FilterGroup{TFilter}"/> is sealed. -or- Parent <see cref="FilterContainer{TFilter}"/> is sealed.</exception>
		protected virtual void BeforeFiltrationAndExecutionOfFiltersWithGeneratedSymbols(FilterGroup<TFilter> filterGroup, in GeneratorExecutionContext context)
		{
			// Do nothing by default.
		}

		/// <summary>
		/// Method called before the filtration of <paramref name="filterGroup"/> is started.
		/// </summary>
		/// <remarks>The <paramref name="filterGroup"/> is unsealed, so it is possible to change its state.</remarks>
		/// <param name="filterGroup">Current <see cref="FilterGroup{TFilter}"/>.</param>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		/// <exception cref="InvalidOperationException">Parent <see cref="FilterContainer{TFilter}"/> is sealed.</exception>
		protected virtual void BeforeFiltrationOfGroup(FilterGroup<TFilter> filterGroup, in GeneratorExecutionContext context)
		{
			// Do nothing by default.
		}

		private protected void BeginIterationOfFiltersWithGeneratedSymbols()
		{
			_isFilterWithGeneratedSymbols = true;
		}

		/// <summary>
		/// Creates new instance of <see cref="ICompilationData"/>.
		/// </summary>
		/// <param name="compilation">Current <see cref="CSharpCompilation"/>.</param>
		protected abstract TCompilationData? CreateCompilationData(CSharpCompilation compilation);

		private protected void EndIterationOfFiltersWithGeneratedSymbols()
		{
			_isFilterWithGeneratedSymbols = false;
		}

		/// <inheritdoc/>
		protected abstract bool Generate(IMemberData member, string hintName, in GeneratorExecutionContext context);

		/// <summary>
		/// Performs the generation for the specified <paramref name="data"/>.
		/// </summary>
		/// <param name="data"><see cref="IMemberData"/> to perform the generation for.</param>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		protected void GenerateFromData(IMemberData data, in GeneratorExecutionContext context)
		{
			string name = FileNameProvider.GetFileName(data.Symbol);

			if (Generate(data, name, in context))
			{
				FileNameProvider.Success();
			}
		}

		private protected void InitializeExecutionData(CSharpCompilation currentCompilation, TSyntaxReceiver syntaxReceiver, in GeneratorExecutionContext context)
		{
			TCompilationData? data = CreateCompilationData(currentCompilation);

			if (data is null || data.HasErrors)
			{
				return;
			}

			TargetCompilation = data;
			ParseOptions = context.ParseOptions as CSharpParseOptions ?? CSharpParseOptions.Default;
			SyntaxReceiver = syntaxReceiver;
			CancellationToken = context.CancellationToken;
			HasValidData = true;

			if (EnableDiagnostics)
			{
				DiagnosticReceiver.SetContext(in context);
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

		private protected void ResetData()
		{
			SyntaxReceiver = default;
			TargetCompilation = default;
			ParseOptions = null!;
			CancellationToken = default;
			IsSuccess = false;
			HasValidData = false;
			FileNameProvider.Reset();
		}

		private protected void ThrowIfHasNoValidData()
		{
			if (!HasValidData && EnableExceptions)
			{
				throw new InvalidOperationException($"{nameof(HasValidData)} must be true in order to add a new source!");
			}
		}

		private protected void UpdateCompilationBeforeFiltersWithGeneratedSymbols()
		{
			if (_generatedDuringCurrentPass.Count > 0)
			{
				// Generated sources should be added AFTER all filters that don't include generated symbols were executed to avoid conflicts with SemanticModels.
				foreach (CSharpSyntaxTree generatedTree in _generatedDuringCurrentPass)
				{
					TargetCompilation!.UpdateCompilation(generatedTree);
				}

				_generatedDuringCurrentPass.Clear();
			}
		}

		/// <summary>
		/// Validates the <paramref name="syntaxReceiver"/>.
		/// </summary>
		/// <param name="syntaxReceiver"><typeparamref name="TSyntaxReceiver"/> to validate.</param>
		protected virtual bool ValidateSyntaxReceiver(TSyntaxReceiver syntaxReceiver)
		{
			return !syntaxReceiver.IsEmpty();
		}

		private void Filtrate(in GeneratorExecutionContext context)
		{
			FilterContainer<TFilter>? filters = GetFilters(in context);

			if (filters is null || filters.NumGroups == 0)
			{
				return;
			}

			BeforeExecution(in context);

			foreach (FilterGroup<TFilter> filterGroup in filters)
			{
				HandleFilterGroup(filterGroup, in context);
			}

			AfterExecution(in context);
		}

		private void GenerateFromFiltersWithGeneratedSymbols(FilterGroup<TFilter> filterGroup, List<TFilter> filtersWithGeneratedSymbols, in GeneratorExecutionContext context)
		{
			UpdateCompilationBeforeFiltersWithGeneratedSymbols();
			BeforeFiltrationAndExecutionOfFiltersWithGeneratedSymbols(filterGroup, in context);
			BeginIterationOfFiltersWithGeneratedSymbols();

			foreach (TFilter filter in filtersWithGeneratedSymbols)
			{
				IterateThroughFilter(filter, in context);
			}

			EndIterationOfFiltersWithGeneratedSymbols();
		}

		private void HandleFilterGroup(FilterGroup<TFilter> filterGroup, in GeneratorExecutionContext context)
		{
			int numFilters = filterGroup.Count;
			List<TFilter> filtersWithGeneratedSymbols = new(numFilters);

			filterGroup.Unseal();
			BeforeFiltrationOfGroup(filterGroup, in context);
			filterGroup.Seal();

			foreach (TFilter filter in filterGroup)
			{
				if (filter.IncludeGeneratedSymbols)
				{
					filtersWithGeneratedSymbols.Add(filter);
				}
				else
				{
					foreach (IMemberData data in Filtrate(filter, in context))
					{
						GenerateFromData(data, in context);
					}
				}
			}

			BeforeExecutionOfGroup(filterGroup, in context);

			GenerateFromFiltersWithGeneratedSymbols(filterGroup, filtersWithGeneratedSymbols, in context);

			filterGroup.Unseal();
			AfterExecutionOfGroup(filterGroup, in context);
		}

		/// <summary>
		/// Performs syntax filtration on the specified <paramref name="context"/> using the given <paramref name="filter"/>.
		/// </summary>
		/// <param name="filter"><see cref="ISyntaxFilter"/> to perform the filtration with.</param>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		protected virtual IEnumerable<IMemberData> Filtrate(TFilter filter, in GeneratorExecutionContext context)
        {
			return filter.Filtrate(in context);
        }
	}

	/// <inheritdoc cref="DurianGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/>
	public abstract class DurianGenerator<TCompilationData, TSyntaxReceiver> : DurianGenerator<TCompilationData, TSyntaxReceiver, IGeneratorSyntaxFilterWithDiagnostics>
		where TCompilationData : ICompilationDataWithSymbols
		where TSyntaxReceiver : IDurianSyntaxReceiver
	{
		/// <inheritdoc cref="DurianGenerator{TCompilationData, TSyntaxReceiver}.DurianGenerator(in ConstructionContext, IHintNameProvider?)"/>
		protected DurianGenerator()
		{
		}

		/// <inheritdoc cref="DurianGenerator{TCompilationData, TSyntaxReceiver}.DurianGenerator(in ConstructionContext, IHintNameProvider?)"/>
		protected DurianGenerator(in ConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGenerator{TCompilationData, TSyntaxReceiver}"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected DurianGenerator(in ConstructionContext context, IHintNameProvider? fileNameProvider) : base(in context, fileNameProvider)
		{
		}

		/// <inheritdoc cref="DurianGenerator(LoggingConfiguration?, IHintNameProvider?)"/>
		protected DurianGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGenerator{TCompilationData, TSyntaxReceiver}"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected DurianGenerator(LoggingConfiguration? loggingConfiguration, IHintNameProvider? fileNameProvider) : base(loggingConfiguration, fileNameProvider)
		{
		}
	}

	/// <inheritdoc cref="DurianGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/>
	public abstract class DurianGenerator<TCompilationData> : DurianGenerator<TCompilationData, IDurianSyntaxReceiver, IGeneratorSyntaxFilterWithDiagnostics>
		where TCompilationData : ICompilationDataWithSymbols
	{
		/// <inheritdoc cref="DurianGenerator{TCompilationData}.DurianGenerator(in ConstructionContext, IHintNameProvider?)"/>
		protected DurianGenerator()
		{
		}

		/// <inheritdoc cref="DurianGenerator{TCompilationData}.DurianGenerator(in ConstructionContext, IHintNameProvider?)"/>
		protected DurianGenerator(in ConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGenerator{TCompilationData}"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected DurianGenerator(in ConstructionContext context, IHintNameProvider? fileNameProvider) : base(in context, fileNameProvider)
		{
		}

		/// <inheritdoc cref="DurianGenerator(LoggingConfiguration?, IHintNameProvider?)"/>
		protected DurianGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGenerator{TCompilationData}"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected DurianGenerator(LoggingConfiguration? loggingConfiguration, IHintNameProvider? fileNameProvider) : base(loggingConfiguration, fileNameProvider)
		{
		}
	}

	/// <inheritdoc cref="DurianGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/>
	public abstract class DurianGenerator : DurianGenerator<ICompilationDataWithSymbols, IDurianSyntaxReceiver, IGeneratorSyntaxFilterWithDiagnostics>
	{
		/// <inheritdoc cref="DurianGenerator(in ConstructionContext, IHintNameProvider?)"/>
		protected DurianGenerator()
		{
		}

		/// <inheritdoc cref="DurianGenerator(in ConstructionContext, IHintNameProvider?)"/>
		protected DurianGenerator(in ConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGenerator"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected DurianGenerator(in ConstructionContext context, IHintNameProvider? fileNameProvider) : base(in context, fileNameProvider)
		{
		}

		/// <inheritdoc cref="DurianGenerator(LoggingConfiguration?, IHintNameProvider?)"/>
		protected DurianGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGenerator"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected DurianGenerator(LoggingConfiguration? loggingConfiguration, IHintNameProvider? fileNameProvider) : base(loggingConfiguration, fileNameProvider)
		{
		}
	}
}