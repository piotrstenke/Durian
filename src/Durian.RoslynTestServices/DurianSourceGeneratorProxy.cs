using System;
using Durian.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Tests
{
	/// <summary>
	/// A simple proxy class that inherits the <see cref="SourceGenerator"/> class and leaves the actual implementation details to be defined by the user through appropriate C# events, as well as provides references to the passed <see cref="GeneratorInitializationContext"/> and <see cref="GeneratorExecutionContext"/>.
	/// </summary>
	public sealed class DurianSourceGeneratorProxy : SourceGenerator
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
		public event GenerateAction? OnGenerate;

		/// <summary>
		/// Event invoked when the <see cref="Initialize"/> method is called.
		/// </summary>
		public event GeneratorInitialize? OnInitialize;

		/// <summary>
		/// Event invoked when the <see cref="BeforeFiltration(in GeneratorExecutionContext)"/> method is called.
		/// </summary>
		public event GeneratorExecute? OnBeforeFiltration;

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
		public event GetFiltersAction? OnGetFilters;

		/// <summary>
		/// A delegate with signature equivalent to the <see cref="SourceGenerator{TCompilationData, ISyntaxReceiver, IFilter}.Generate(IMemberData, IFilter, CodeBuilder, in GeneratorExecutionContext)"/> method.
		/// </summary>
		/// <param name="member"><see cref="IMemberData"/> to generate the source for.</param>
		/// <param name="filter"><see cref="ISyntaxFilter"/> that collected the target <paramref name="member"/>.</param>
		/// <param name="builder"><see cref="CodeBuilder"/> that should be used to generate the source code.</param>
		/// <param name="context">The <see cref="GeneratorExecutionContext"/> to add source to.</param>
		public delegate void GenerateAction(IMemberData member, ISyntaxFilter filter, CodeBuilder builder, in GeneratorExecutionContext context);

		/// <summary>
		/// A delegate with signature equivalent to the <see cref="SourceGenerator{TCompilationData, ISyntaxReceiver, IFilter}.GetFilters(in GeneratorExecutionContext)"/> method.
		/// </summary>
		/// <param name="context">Current <see cref="GeneratorExecutionContext"/>.</param>
#if ENABLE_GENERATOR_SYNTAX_DIAGNOSTICS
		public delegate FilterList<ISyntaxFilterWithDiagnostics> GetFiltersAction(in GeneratorExecutionContext context);
#else
		public delegate FilterList<ISyntaxFilter> GetFiltersAction(in GeneratorExecutionContext context);
#endif

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianSourceGeneratorProxy"/> class.
		/// </summary>
		public DurianSourceGeneratorProxy()
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
#if ENABLE_GENERATOR_SYNTAX_DIAGNOSTICS
		protected override FilterList<ISyntaxFilterWithDiagnostics>? GetFilters(in GeneratorExecutionContext context)
#else
		protected override FilterList<ISyntaxFilter>? GetFilters(in GeneratorExecutionContext context)
#endif
		{
			return OnGetFilters?.Invoke(in context);
		}

		/// <inheritdoc/>
		protected override ICompilationData? CreateCompilationData(CSharpCompilation compilation)
		{
			return OnCreateCompilationData?.Invoke(compilation);
		}

		/// <inheritdoc/>
#if ENABLE_GENERATOR_SYNTAX_DIAGNOSTICS
		protected override void Generate(IMemberData member, ISyntaxFilterWithDiagnostics filter, CodeBuilder builder, in GeneratorExecutionContext context)
#else
		protected override void Generate(IMemberData member, ISyntaxFilter filter, CodeBuilder builder, in GeneratorExecutionContext context)
#endif
		{
			OnGenerate?.Invoke(member, filter, builder, in context);
			_exeContext = context;
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
		protected override string GetVersion()
		{
			return OnGetVersion?.Invoke() ?? base.GetVersion();
		}
	}
}
