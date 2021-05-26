using System;
using System.Threading;
using Durian.Generator;
using Durian.Generator.Data;
using Durian.Generator.Logging;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Tests
{
	/// <summary>
	/// A simple proxy class that inherits the <see cref="DurianGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/> class and leaves the actual implementation details to be defined by the user through appropriate C# events, as well as provides references to the passed <see cref="GeneratorInitializationContext"/> and <see cref="GeneratorExecutionContext"/>.
	/// </summary>
	[GeneratorLoggingConfiguration(SupportsDiagnostics = true, LogDirectory = "test", RelativeToDefault = true, SupportedLogs = GeneratorLogs.All)]
	public class DurianGeneratorProxy : DurianGenerator
	{
		private GeneratorExecutionContext _exeContext;
		private GeneratorInitializationContext _initContext;

		/// <summary>
		/// Returns a readonly reference to the last <see cref="GeneratorExecutionContext"/>.
		/// </summary>
		public ref readonly GeneratorExecutionContext ExecutionContext => ref _exeContext;

		/// <summary>
		/// Returns a readonly reference to the last <see cref="GeneratorInitializationContext"/>.
		/// </summary>
		public ref readonly GeneratorInitializationContext InitializationContext => ref _initContext;

		/// <summary>
		/// Event invoked when the <see cref="CreateSyntaxReceiver"/> method is called.
		/// </summary>
		public event Func<IDurianSyntaxReceiver?>? OnCreateSyntaxReceiver;

		/// <summary>
		/// Event invoked when the <see cref="CreateCompilationData"/> method is called.
		/// </summary>
		public event Func<CSharpCompilation, ICompilationDataWithSymbols?>? OnCreateCompilationData;

		/// <summary>
		/// Event invoked when the <see cref="Generate"/> method is called.
		/// </summary>
		public event Func<IMemberData, string, GeneratorExecutionContext, bool>? OnGenerate;

		/// <summary>
		/// Event invoked when the <see cref="Initialize"/> method is called.
		/// </summary>
		public event GeneratorInitialize? OnInitialize;

		/// <summary>
		/// Event invoked when the <see cref="BeforeExecution(in GeneratorExecutionContext)"/> method is called.
		/// </summary>
		public event GeneratorExecute? OnBeforeExecution;

		/// <summary>
		/// Event invoked when the <see cref="AfterExecution(in GeneratorExecutionContext)"/> method is called.
		/// </summary>
		public event GeneratorExecute? OnAfterExecution;

		/// <summary>
		/// Event invoked when the <see cref="BeforeFiltrationOfGroup(FilterGroup{IGeneratorSyntaxFilterWithDiagnostics}, in GeneratorExecutionContext)"/> method is called.
		/// </summary>
		public event GeneratorFiltrate<IGeneratorSyntaxFilterWithDiagnostics>? OnBeforeFiltrationOfGroup;

		/// <summary>
		/// Event invoked when the <see cref="BeforeExecutionOfGroup(FilterGroup{IGeneratorSyntaxFilterWithDiagnostics}, in GeneratorExecutionContext)"/> method is called.
		/// </summary>
		public event GeneratorFiltrate<IGeneratorSyntaxFilterWithDiagnostics>? OnBeforeExecutionOfGroup;

		/// <summary>
		/// Event invoked when the <see cref="AfterExecutionOfGroup(FilterGroup{IGeneratorSyntaxFilterWithDiagnostics}, in GeneratorExecutionContext)"/> method is called.
		/// </summary>
		public event GeneratorFiltrate<IGeneratorSyntaxFilterWithDiagnostics>? OnAfterExecutinoOfGroup;

		/// <summary>
		/// Event invoked when the <see cref="BeforeFiltrationAndExecutionOfFiltersWithGeneratedSymbols(FilterGroup{IGeneratorSyntaxFilterWithDiagnostics}, in GeneratorExecutionContext)"/> method is called.
		/// </summary>
		public event GeneratorFiltrate<IGeneratorSyntaxFilterWithDiagnostics>? OnBeforeFiltrationAndExecutionOfFiltersWithGeneratedSymbols;

		/// <summary>
		/// Event invoked when the <see cref="GetVersion"/> method is called.
		/// </summary>
		public event Func<string>? OnGetVersion;

		/// <summary>
		/// Event invoked when the <see cref="GetGeneratorName"/> method is called.
		/// </summary>
		public event Func<string>? OnGetGeneratorName;

		/// <summary>
		/// Event invoked when the <see cref="ValidateSyntaxReceiver"/> method is called.
		/// </summary>
		public event Func<IDurianSyntaxReceiver?, bool>? OnValidateSyntaxReceiver;

		/// <summary>
		/// Event invoked when the <see cref="GetStaticSyntaxTrees(CancellationToken)"/> method is called.
		/// </summary>
		public event Func<CancellationToken, (CSharpSyntaxTree tree, string hintName)[]?>? OnGetStaticTrees;

		/// <summary>
		/// Event invoked when the <see cref="GetFilters(in GeneratorExecutionContext)"/> method is called.
		/// </summary>
		public event Func<GeneratorExecutionContext, FilterContainer<IGeneratorSyntaxFilterWithDiagnostics>>? OnGetFilters;

		/// <summary>
		/// Event invoked when the <see cref="IterateThroughFilter(IGeneratorSyntaxFilterWithDiagnostics, in GeneratorExecutionContext)"/> method is called.
		/// </summary>
		public event Action<IGeneratorSyntaxFilterWithDiagnostics, GeneratorExecutionContext>? OnIterateThroughFilter;

		/// <summary>
		/// Event invoked when the <see cref="GetEnabledModules()"/> method is called.
		/// </summary>
		public event Func<DurianModule[]>? OnGetEnabledModules;

		/// <inheritdoc cref="DurianGeneratorProxy(in LoggableGeneratorConstructionContext, IFileNameProvider?)"/>
		protected DurianGeneratorProxy()
		{
		}

		/// <inheritdoc cref="DurianGeneratorProxy(in LoggableGeneratorConstructionContext, IFileNameProvider?)"/>
		protected DurianGeneratorProxy(in LoggableGeneratorConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGeneratorProxy"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableSourceGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected DurianGeneratorProxy(in LoggableGeneratorConstructionContext context, IFileNameProvider? fileNameProvider) : base(in context, fileNameProvider)
		{
		}

		/// <inheritdoc cref="DurianGeneratorProxy(GeneratorLoggingConfiguration?, IFileNameProvider?)"/>
		protected DurianGeneratorProxy(GeneratorLoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGeneratorProxy"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected DurianGeneratorProxy(GeneratorLoggingConfiguration? loggingConfiguration, IFileNameProvider? fileNameProvider) : base(loggingConfiguration, fileNameProvider)
		{
		}

		/// <summary>
		/// Disposes the proxy by setting the <see cref="ExecutionContext"/> and <see cref="InitializationContext"/> to its default values.
		/// </summary>
		public void Release()
		{
			_initContext = default;
			_exeContext = default;
		}

		/// <summary>
		/// Initializes the <see cref="DurianGeneratorProxy"/>.
		/// </summary>
		/// <param name="context"><see cref="GeneratorInitializationContext"/> to be used when initializing the <see cref="DurianGeneratorProxy"/>.</param>
		public override void Initialize(GeneratorInitializationContext context)
		{
			base.Initialize(context);

			OnInitialize?.Invoke(context);
			_initContext = context;
		}

		/// <inheritdoc/>
		public override IDurianSyntaxReceiver CreateSyntaxReceiver()
		{
			return OnCreateSyntaxReceiver?.Invoke() ?? new SyntaxReceiverProxy();
		}

		/// <inheritdoc/>
		protected override (CSharpSyntaxTree tree, string hintName)[]? GetStaticSyntaxTrees(CancellationToken cancellationToken)
		{
			return OnGetStaticTrees?.Invoke(cancellationToken);
		}

		/// <inheritdoc/>
		protected override FilterContainer<IGeneratorSyntaxFilterWithDiagnostics>? GetFilters(in GeneratorExecutionContext context)
		{
			return OnGetFilters?.Invoke(context);
		}

		/// <inheritdoc/>
		protected override ICompilationDataWithSymbols? CreateCompilationData(CSharpCompilation compilation)
		{
			return OnCreateCompilationData?.Invoke(compilation);
		}

		/// <inheritdoc/>
		protected override bool Generate(IMemberData member, string hintName, in GeneratorExecutionContext context)
		{
			bool value = OnGenerate?.Invoke(member, hintName, context) ?? false;
			_exeContext = context;

			return value;
		}

		/// <inheritdoc/>
		protected override DurianModule[] GetEnabledModules()
		{
			return OnGetEnabledModules?.Invoke() ?? Array.Empty<DurianModule>();
		}

		/// <inheritdoc/>
		protected override bool ValidateSyntaxReceiver(IDurianSyntaxReceiver syntaxReceiver)
		{
			return OnValidateSyntaxReceiver?.Invoke(syntaxReceiver) ?? base.ValidateSyntaxReceiver(syntaxReceiver);
		}

		/// <inheritdoc/>
		protected override string GetGeneratorName()
		{
			return OnGetGeneratorName?.Invoke() ?? base.GetGeneratorName();
		}

		/// <inheritdoc/>
		protected override void BeforeExecution(in GeneratorExecutionContext context)
		{
			OnBeforeExecution?.Invoke(in context);
		}

		/// <inheritdoc/>
		protected override void AfterExecution(in GeneratorExecutionContext context)
		{
			OnAfterExecution?.Invoke(in context);
		}

		/// <inheritdoc/>
		protected override void BeforeExecutionOfGroup(FilterGroup<IGeneratorSyntaxFilterWithDiagnostics> filterGroup, in GeneratorExecutionContext context)
		{
			OnBeforeExecutionOfGroup?.Invoke(filterGroup, in context);
		}

		/// <inheritdoc/>
		protected override void AfterExecutionOfGroup(FilterGroup<IGeneratorSyntaxFilterWithDiagnostics> filterGroup, in GeneratorExecutionContext context)
		{
			OnAfterExecutinoOfGroup?.Invoke(filterGroup, in context);
		}

		/// <inheritdoc/>
		protected override void BeforeFiltrationOfGroup(FilterGroup<IGeneratorSyntaxFilterWithDiagnostics> filterGroup, in GeneratorExecutionContext context)
		{
			OnBeforeFiltrationOfGroup?.Invoke(filterGroup, context);
		}

		/// <inheritdoc/>
		protected override void BeforeFiltrationAndExecutionOfFiltersWithGeneratedSymbols(FilterGroup<IGeneratorSyntaxFilterWithDiagnostics> filterGroup, in GeneratorExecutionContext context)
		{
			OnBeforeFiltrationAndExecutionOfFiltersWithGeneratedSymbols?.Invoke(filterGroup, context);
		}

		/// <inheritdoc/>
		protected override void IterateThroughFilter(IGeneratorSyntaxFilterWithDiagnostics filter, in GeneratorExecutionContext context)
		{
			if (OnIterateThroughFilter is null)
			{
				base.IterateThroughFilter(filter, context);
			}
			else
			{
				OnIterateThroughFilter.Invoke(filter, context);
			}
		}

		/// <inheritdoc/>
		protected override string GetVersion()
		{
			return OnGetVersion?.Invoke() ?? base.GetVersion();
		}
	}
}
