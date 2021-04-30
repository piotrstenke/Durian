using System;
using Durian.Data;
using Durian.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Tests
{
	/// <summary>
	/// A simple proxy class that inherits the <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/> class and leaves the actual implementation details to be defined by the user through appropriate C# events, as well as provides references to the passed <see cref="GeneratorInitializationContext"/> and <see cref="GeneratorExecutionContext"/>.
	/// </summary>
	[GeneratorLoggingConfiguration(SupportsDiagnostics = true, LogDirectory = "test", RelativeToDefault = true, SupportedLogs = GeneratorLogs.All)]
	public class DurianSourceGeneratorProxy : SourceGenerator<ICompilationData, IDurianSyntaxReceiver, IGeneratorSyntaxFilterWithDiagnostics>
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
		public event Func<CSharpCompilation, ICompilationData?>? OnCreateCompilationData;

		/// <summary>
		/// Event invoked when the <see cref="Generate"/> method is called.
		/// </summary>
		public event Func<IMemberData, string, IGeneratorSyntaxFilterWithDiagnostics, GeneratorExecutionContext, bool>? OnGenerate;

		/// <summary>
		/// Event invoked when the <see cref="Initialize"/> method is called.
		/// </summary>
		public event GeneratorInitialize? OnInitialize;

		/// <summary>
		/// Event invoked when the <see cref="BeforeFiltration(in GeneratorExecutionContext)"/> method is called.
		/// </summary>
		public event GeneratorExecute? OnBeforeFiltration;

		/// <summary>
		/// Event invoked when the <see cref="AfterFiltration(in GeneratorExecutionContext)"/> method is called.
		/// </summary>
		public event GeneratorExecute? OnAfterFiltration;

		/// <summary>
		/// Event invoked when the <see cref="BeforeFiltrationOfGroup(FilterGroup{IGeneratorSyntaxFilterWithDiagnostics}, in GeneratorExecutionContext)"/> method is called.
		/// </summary>
		public event GeneratorFiltrate<IGeneratorSyntaxFilterWithDiagnostics>? OnBeforeFiltrationOfGroup;

		/// <summary>
		/// Event invoked when the <see cref="AfterFiltrationOfGroup(FilterGroup{IGeneratorSyntaxFilterWithDiagnostics}, in GeneratorExecutionContext)"/> method is called.
		/// </summary>
		public event GeneratorFiltrate<IGeneratorSyntaxFilterWithDiagnostics>? OnAfterFiltrationOfGroup;

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
		/// Event invoked when the <see cref="GetFilters(in GeneratorExecutionContext)"/> method is called.
		/// </summary>
		public event Func<GeneratorExecutionContext, FilterContainer<IGeneratorSyntaxFilterWithDiagnostics>>? OnGetFilters;

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianSourceGeneratorProxy"/> class.
		/// </summary>
		/// <param name="checkForConfigurationAttribute">Determines whether to try to create a <see cref="GeneratorLoggingConfiguration"/> based on one of the logging attributes.
		/// <para>See: <see cref="GeneratorLoggingConfigurationAttribute"/>, <see cref="DefaultGeneratorLoggingConfigurationAttribute"/></para></param>
		/// <param name="enableLoggingIfSupported">Determines whether to enable logging for this <see cref="DurianSourceGeneratorProxy"/> instance if logging is supported.</param>
		/// <param name="enableDiagnosticsIfSupported">Determines whether to set <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}.EnableDiagnostics"/> to <see langword="true"/> if <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}.SupportsDiagnostics"/> is <see langword="true"/>.</param>
		public DurianSourceGeneratorProxy(bool checkForConfigurationAttribute, bool enableLoggingIfSupported = true, bool enableDiagnosticsIfSupported = true) : base(checkForConfigurationAttribute, enableLoggingIfSupported, enableDiagnosticsIfSupported)
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
		/// Initializes the <see cref="DurianSourceGeneratorProxy"/>.
		/// </summary>
		/// <param name="context"><see cref="GeneratorInitializationContext"/> to be used when initializing the <see cref="DurianSourceGeneratorProxy"/>.</param>
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
		protected override FilterContainer<IGeneratorSyntaxFilterWithDiagnostics>? GetFilters(in GeneratorExecutionContext context)
		{
			return OnGetFilters?.Invoke(context);
		}

		/// <inheritdoc/>
		protected override ICompilationData? CreateCompilationData(CSharpCompilation compilation)
		{
			return OnCreateCompilationData?.Invoke(compilation);
		}

		/// <inheritdoc/>
		protected override bool Generate(IMemberData member, string hintName, IGeneratorSyntaxFilterWithDiagnostics filter, in GeneratorExecutionContext context)
		{
			bool value = OnGenerate?.Invoke(member, hintName, filter, context) ?? false;
			_exeContext = context;

			return value;
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
		protected override void BeforeFiltration(in GeneratorExecutionContext context)
		{
			OnBeforeFiltration?.Invoke(in context);
		}

		/// <inheritdoc/>
		protected override void AfterFiltration(in GeneratorExecutionContext context)
		{
			OnAfterFiltration?.Invoke(in context);
		}

		/// <inheritdoc/>
		protected override void BeforeFiltrationOfGroup(FilterGroup<IGeneratorSyntaxFilterWithDiagnostics> filterGroup, in GeneratorExecutionContext context)
		{
			OnBeforeFiltrationOfGroup?.Invoke(filterGroup, in context);
		}

		/// <inheritdoc/>
		protected override void AfterFiltrationOfGroup(FilterGroup<IGeneratorSyntaxFilterWithDiagnostics> filterGroup, in GeneratorExecutionContext context)
		{
			OnAfterFiltrationOfGroup?.Invoke(filterGroup, in context);
		}

		/// <inheritdoc/>
		protected override string GetVersion()
		{
			return OnGetVersion?.Invoke() ?? base.GetVersion();
		}
	}
}
