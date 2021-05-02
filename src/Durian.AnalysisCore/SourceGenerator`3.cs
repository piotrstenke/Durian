using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Durian.Data;
using Durian.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian
{
	/// <summary>
	/// Abstract implementation of the <see cref="IDurianSourceGenerator"/> interface that performs early validation of the input <see cref="GeneratorExecutionContext"/>.
	/// </summary>
	/// <typeparam name="TCompilationData">User-defined type of <see cref="ICompilationData"/> this <see cref="IDurianSourceGenerator"/> operates on.</typeparam>
	/// <typeparam name="TSyntaxReceiver">User-defined type of <see cref="IDurianSyntaxReceiver"/> that provides the <see cref="CSharpSyntaxNode"/>s to perform the generation on.</typeparam>
	/// <typeparam name="TFilter">User-defined type of <see cref="ISyntaxFilter"/> that decides what <see cref="CSharpSyntaxNode"/>s collected by the <see cref="SyntaxReceiver"/> are valid for generation.</typeparam>
	[DebuggerDisplay("Name = {GetGeneratorName()}, Version = {GetVersion()}")]
	public abstract class SourceGenerator<TCompilationData, TSyntaxReceiver, TFilter> : LoggableSourceGenerator, IDurianSourceGenerator
		where TCompilationData : class, ICompilationData
		where TSyntaxReceiver : class, IDurianSyntaxReceiver
		where TFilter : notnull, IGeneratorSyntaxFilterWithDiagnostics
	{
		private ReadonlyContextualDiagnosticReceiver<GeneratorExecutionContext>? _diagnosticReceiver;
		private IFileNameProvider _fileNameProvider;

		/// <summary>
		/// A <see cref="IDiagnosticReceiver"/> that is used to report diagnostics.
		/// </summary>
		/// <remarks>Can be set only if <see cref="SupportsDiagnostics"/> is <see langword="true"/>.</remarks>
		/// <exception cref="InvalidOperationException">
		/// <see cref="DiagnosticReceiver"/> cannot be set if <see cref="SupportsDiagnostics"/> is <see langword="false"/>. -or-
		/// <see cref="DiagnosticReceiver"/> cannot be set to <see langword="null"/> if <see cref="SupportsDiagnostics"/> is <see langword="true"/>.
		/// </exception>
		[DisallowNull]
		public ReadonlyContextualDiagnosticReceiver<GeneratorExecutionContext>? DiagnosticReceiver
		{
			get => _diagnosticReceiver;
			set
			{
				if (!SupportsDiagnostics)
				{
					throw new InvalidOperationException($"{nameof(DiagnosticReceiver)} cannot be set if {nameof(SupportsDiagnostics)} is false!");
				}

				if (value is null)
				{
					if (SupportsDiagnostics)
					{
						throw new InvalidOperationException($"{nameof(DiagnosticReceiver)} cannot be set to null if {nameof(SupportsDiagnostics)} is true!");
					}
				}
				else
				{
					_diagnosticReceiver = value;
				}
			}
		}

		/// <summary>
		/// A <see cref="IDiagnosticReceiver"/> that is used to create log files outside of this <see cref="ISourceGenerator"/>.
		/// </summary>
		public LoggableGeneratorDiagnosticReceiver LogReceiver { get; }

		/// <summary>
		/// Creates names for generated files.
		/// </summary>
		/// <exception cref="ArgumentNullException"><see cref="FileNameProvider"/> cannot be <see langword="null"/>.</exception>
		public IFileNameProvider FileNameProvider
		{
			get => _fileNameProvider;
			set
			{
				if (value is null)
				{
					throw new ArgumentNullException(nameof(FileNameProvider));
				}

				_fileNameProvider = value;
			}
		}

		/// <inheritdoc cref="IDurianSourceGenerator.ParseOptions"/>
		[MaybeNull]
		public CSharpParseOptions ParseOptions { get; private set; }

		/// <inheritdoc cref="IDurianSourceGenerator.TargetCompilation"/>
		[MaybeNull]
		public TCompilationData TargetCompilation { get; private set; }

		/// <inheritdoc cref="IDurianSourceGenerator.SyntaxReceiver"/>
		[MaybeNull]
		public TSyntaxReceiver SyntaxReceiver { get; private set; }

		/// <inheritdoc/>
		public CancellationToken CancellationToken { get; private set; }

		/// <summary>
		/// Determines whether the last execution of the <see cref="Execute(in GeneratorExecutionContext)"/> method was a success.
		/// </summary>
		[MemberNotNullWhen(true, nameof(TargetCompilation), nameof(SyntaxReceiver), nameof(ParseOptions))]
		public bool IsSuccess { get; private set; }

		/// <summary>
		/// Determines whether data of this <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/> was successfully initialized by the last call to the <see cref="Execute(in GeneratorExecutionContext)"/> method.
		/// </summary>
		[MemberNotNullWhen(true, nameof(TargetCompilation), nameof(SyntaxReceiver), nameof(ParseOptions))]
		public bool HasValidData { get; private set; }

		/// <inheritdoc/>
		[MemberNotNullWhen(true, nameof(DiagnosticReceiver))]
		public bool SupportsDiagnostics => LoggingConfiguration.SupportsDiagnostics;

		/// <inheritdoc/>
		/// <exception cref="InvalidOperationException"><see cref="EnableDiagnostics"/> cannot be set to <see langword="true"/> if <see cref="SupportsDiagnostics"/> is <see langword="false"/>.</exception>
		[MemberNotNullWhen(true, nameof(DiagnosticReceiver))]
		public bool EnableDiagnostics
		{
			get => LoggingConfiguration.EnableDiagnostics;
			set => LoggingConfiguration.EnableDiagnostics = value;
		}

		/// <summary>
		/// Determines whether to enable logging.
		/// </summary>
		/// <exception cref="InvalidOperationException"><see cref="EnableLogging"/> cannot be set to <see langword="true"/> if generator logging is globally disabled.</exception>
		public bool EnableLogging
		{
			get => LoggingConfiguration.EnableLogging;
			set => LoggingConfiguration.EnableLogging = value;
		}

		CSharpParseOptions IDurianSourceGenerator.ParseOptions => ParseOptions!;
		ICompilationData IDurianSourceGenerator.TargetCompilation => TargetCompilation!;
		IDurianSyntaxReceiver IDurianSourceGenerator.SyntaxReceiver => SyntaxReceiver!;
		string IDurianSourceGenerator.GeneratorName => GetGeneratorName();
		string IDurianSourceGenerator.Version => GetVersion();

		/// <summary>
		/// Initializes a new instance of the <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/> class.
		/// </summary>
		/// <param name="checkForConfigurationAttribute">Determines whether to try to create a <see cref="GeneratorLoggingConfiguration"/> based on one of the logging attributes.
		/// <para>See: <see cref="GeneratorLoggingConfigurationAttribute"/>, <see cref="DefaultGeneratorLoggingConfigurationAttribute"/></para></param>
		/// <param name="enableLoggingIfSupported">Determines whether to enable logging for this <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/> instance if logging is supported.</param>
		/// <param name="enableDiagnosticsIfSupported">Determines whether to set <see cref="EnableDiagnostics"/> to <see langword="true"/> if <see cref="SupportsDiagnostics"/> is <see langword="true"/>.</param>
		protected SourceGenerator(bool checkForConfigurationAttribute, bool enableLoggingIfSupported = true, bool enableDiagnosticsIfSupported = true) : this(checkForConfigurationAttribute, enableLoggingIfSupported, enableDiagnosticsIfSupported, new SymbolNameToFile())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/> class.
		/// </summary>
		/// <param name="checkForConfigurationAttribute">Determines whether to try to create a <see cref="GeneratorLoggingConfiguration"/> based on one of the logging attributes.
		/// <para>See: <see cref="GeneratorLoggingConfigurationAttribute"/>, <see cref="DefaultGeneratorLoggingConfigurationAttribute"/></para></param>
		/// <param name="enableLoggingIfSupported">Determines whether to enable logging for this <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/> instance if logging is supported.</param>
		/// <param name="enableDiagnosticsIfSupported">Determines whether to set <see cref="EnableDiagnostics"/> to <see langword="true"/> if <see cref="SupportsDiagnostics"/> is <see langword="true"/>.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		/// <exception cref="ArgumentNullException"><paramref name="fileNameProvider"/> is <see langword="null"/>.</exception>
		protected SourceGenerator(bool checkForConfigurationAttribute, bool enableLoggingIfSupported, bool enableDiagnosticsIfSupported, IFileNameProvider fileNameProvider) : base(checkForConfigurationAttribute, enableLoggingIfSupported, enableDiagnosticsIfSupported)
		{
			if (fileNameProvider is null)
			{
				throw new ArgumentNullException(nameof(fileNameProvider));
			}

			if (SupportsDiagnostics)
			{
				_diagnosticReceiver = DiagnosticReceiverFactory.SourceGenerator();
			}

			_fileNameProvider = fileNameProvider;
			LogReceiver = new(this);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		protected SourceGenerator(GeneratorLoggingConfiguration? loggingConfiguration) : this(loggingConfiguration, new SymbolNameToFile())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		/// <exception cref="ArgumentNullException"><paramref name="fileNameProvider"/> is <see langword="null"/>.</exception>
		protected SourceGenerator(GeneratorLoggingConfiguration? loggingConfiguration, IFileNameProvider fileNameProvider) : base(loggingConfiguration)
		{
			if (fileNameProvider is null)
			{
				throw new ArgumentNullException(nameof(fileNameProvider));
			}

			if (SupportsDiagnostics)
			{
				_diagnosticReceiver = DiagnosticReceiverFactory.SourceGenerator();
			}

			_fileNameProvider = fileNameProvider;
			LogReceiver = new LoggableGeneratorDiagnosticReceiver(this);
		}

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
			try
			{
				ResetData();

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
				HasValidData = true;

				if (EnableDiagnostics)
				{
					DiagnosticReceiver.SetContext(in context);
				}

				Filtrate(in context);
				AfterExecution(in context);
				IsSuccess = true;
			}
			catch (Exception e)
			{
				LogException(e);
				IsSuccess = false;
				throw;
			}
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
		/// Method called before any node filtration takes places.
		/// </summary>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		protected virtual void BeforeFiltration(in GeneratorExecutionContext context)
		{
			// Do nothing by default.
		}

		/// <summary>
		/// Method called before the execution of <paramref name="filterGroup"/> is started.
		/// </summary>
		/// <param name="filterGroup">Current filter group. The group is sealed, so it is impossible to change its contents.</param>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		/// <exception cref="InvalidOperationException"><see cref="FilterGroup{TFilter}"/> is sealed. -or- Parent <see cref="FilterContainer{TFilter}"/> is sealed.</exception>
		protected virtual void BeforeFiltrationOfGroup(FilterGroup<TFilter> filterGroup, in GeneratorExecutionContext context)
		{
			// Do nothing by default.
		}

		/// <summary>
		/// Method called after the <paramref name="filterGroup"/> is done executing.
		/// </summary>
		/// <param name="filterGroup">Current filter group. The group is unsealed, so it is possible to change its contents.</param>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
		/// <exception cref="InvalidOperationException">Parent <see cref="FilterContainer{TFilter}"/> is sealed.</exception>
		protected virtual void AfterFiltrationOfGroup(FilterGroup<TFilter> filterGroup, in GeneratorExecutionContext context)
		{
			// Do nothing by default.
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
		/// Returns version of this <see cref="IDurianSourceGenerator"/>.
		/// </summary>
		protected virtual string GetVersion()
		{
			return "1.0.0";
		}

		/// <summary>
		/// Returns name of this <see cref="IDurianSourceGenerator"/>.
		/// </summary>
		protected virtual string GetGeneratorName()
		{
			return nameof(SourceGenerator);
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

		/// <summary>
		/// Actually begins the generator execution.
		/// </summary>
		/// <param name="member"><see cref="IMemberData"/> to generate the source for.</param>
		/// <param name="hintName">A <see cref="string"/> that was generated using the <see cref="FileNameProvider"/>.</param>
		/// <param name="context">The <see cref="GeneratorExecutionContext"/> to add source to.</param>
		/// <returns>A <see cref="bool"/> value indicating whether the generation process was successful.</returns>
		protected abstract bool Generate(IMemberData member, string hintName, in GeneratorExecutionContext context);

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

			if (LoggingConfiguration.EnableLogging && LoggingConfiguration.SupportedLogs.HasFlag(GeneratorLogs.Node))
			{
				LogNode_Internal(tree.GetRoot(context.CancellationToken), hintName);
			}
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
		/// <exception cref="InvalidOperationException"><see cref="HasValidData"/> must be <see langword="true"/> in order to add new source.</exception>
		protected void AddSource(CodeBuilder builder, string hintName, in GeneratorExecutionContext context)
		{
			ThrowIfHasNoValidData();

			CSharpSyntaxTree tree = builder.ParseSyntaxTree();
			builder.Clear();
			AddSource_Internal(tree, hintName, in context);
		}

		/// <summary>
		/// Adds the generated <paramref name="source"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="source">The generated text.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
		/// <exception cref="InvalidOperationException"><see cref="HasValidData"/> must be <see langword="true"/> in order to add new source.</exception>
		protected void AddSource(string source, string hintName, in GeneratorExecutionContext context)
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
		protected void AddSource(CSharpSyntaxTree tree, string hintName, in GeneratorExecutionContext context)
		{
			ThrowIfHasNoValidData();
			AddSource_Internal(tree, hintName, in context);
		}

		/// <summary>
		/// Adds the source created using the <paramref name="builder"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="original">The <see cref="CSharpSyntaxNode"/> the source was generated from.</param>
		/// <param name="builder"><see cref="CodeBuilder"/> that was used to build the generated code.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
		/// <exception cref="InvalidOperationException"><see cref="HasValidData"/> must be <see langword="true"/> in order to add new source.</exception>
		protected void AddSource(CSharpSyntaxNode original, CodeBuilder builder, string hintName, in GeneratorExecutionContext context)
		{
			ThrowIfHasNoValidData();
			CSharpSyntaxTree tree = builder.ParseSyntaxTree();
			builder.Clear();
			AddSource_Internal(original, tree, hintName, in context);
		}

		/// <summary>
		/// Adds the generated <paramref name="text"/> to the <paramref name="context"/>.
		/// </summary>
		/// <param name="original">The <see cref="CSharpSyntaxNode"/> the source was generated from.</param>
		/// <param name="text">The generated text.</param>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> to add the source to.</param>
		/// <exception cref="InvalidOperationException"><see cref="HasValidData"/> must be <see langword="true"/> in order to add new source.</exception>
		protected void AddSource(CSharpSyntaxNode original, string text, string hintName, in GeneratorExecutionContext context)
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
		protected void AddSource(CSharpSyntaxNode original, CSharpSyntaxTree tree, string hintName, in GeneratorExecutionContext context)
		{
			ThrowIfHasNoValidData();
			AddSource_Internal(original, tree, hintName, in context);
		}

		private void Filtrate(in GeneratorExecutionContext context)
		{
			FilterContainer<TFilter>? filters = GetFilters(in context);

			if (filters is null || filters.NumGroups == 0)
			{
				return;
			}

			BeforeFiltration(in context);

			foreach (FilterGroup<TFilter> filterGroup in filters)
			{
				filterGroup.Seal();
				BeforeFiltrationOfGroup(filterGroup, in context);

				HandleFilterGroup(filterGroup, in context);

				filterGroup.Unseal();
				AfterFiltrationOfGroup(filterGroup, in context);
			}
		}

		private void HandleFilterGroup(FilterGroup<TFilter> filterGroup, in GeneratorExecutionContext context)
		{
			int numFilters = filterGroup.Count;
			List<TFilter> filtersWithGeneratedSymbols = new(numFilters);
			List<IMemberData[]> filtrated = new(numFilters);

			foreach (TFilter filter in filterGroup)
			{
				if (filter.IncludeGeneratedSymbols)
				{
					filtersWithGeneratedSymbols.Add(filter);
				}
				else
				{
					filtrated.Add(filter.Filtrate());
				}
			}

			foreach (IMemberData[] data in filtrated)
			{
				GenerateFromFilterResult(data, in context);
			}

			foreach (TFilter filter in filtersWithGeneratedSymbols)
			{
				IterateThroughFilter(filter, in context);
			}
		}

		private void GenerateFromFilterResult(IMemberData[] filtrated, in GeneratorExecutionContext context)
		{
			foreach (IMemberData d in filtrated)
			{
				GenerateFromData(d, in context);
			}
		}

		private void ResetData()
		{
			SyntaxReceiver = null!;
			TargetCompilation = null!;
			ParseOptions = null!;
			CancellationToken = default;
			IsSuccess = false;
			HasValidData = false;
			FileNameProvider.Reset();
		}

		[MemberNotNull(nameof(TargetCompilation), nameof(SyntaxReceiver), nameof(ParseOptions))]
		private void ThrowIfHasNoValidData()
		{
			if (!HasValidData)
			{
				throw new InvalidOperationException($"{nameof(HasValidData)} must be true in order to add a new source!");
			}
		}

		private void AddSource_Internal(CSharpSyntaxTree tree, string hintName, in GeneratorExecutionContext context)
		{
			context.AddSource(hintName, tree.GetText(context.CancellationToken));
			TargetCompilation!.UpdateCompilation(tree);

			if (LoggingConfiguration.EnableLogging && LoggingConfiguration.SupportedLogs.HasFlag(GeneratorLogs.Node))
			{
				LogNode_Internal(tree.GetRoot(context.CancellationToken), hintName);
			}
		}

		private void AddSource_Internal(CSharpSyntaxNode original, CSharpSyntaxTree tree, string hintName, in GeneratorExecutionContext context)
		{
			context.AddSource(hintName, tree.GetText(context.CancellationToken));
			TargetCompilation!.UpdateCompilation(tree);

			if (LoggingConfiguration.EnableLogging && LoggingConfiguration.SupportedLogs.HasFlag(GeneratorLogs.InputOutput))
			{
				LogInputOutput_Internal(original, tree.GetRoot(context.CancellationToken), hintName);
			}
		}
	}
}
