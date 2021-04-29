using System;
using Durian.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Tests
{
	/// <summary>
	/// A simple proxy class that inherits the <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/> class and leaves the actual implementation details to be defined by the user through appropriate C# events, as well as provides references to the passed <see cref="GeneratorInitializationContext"/> and <see cref="GeneratorExecutionContext"/>.
	/// </summary>
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
		public event Action<IMemberData, IGeneratorSyntaxFilterWithDiagnostics, GeneratorExecutionContext>? OnGenerate;

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
		public event Func<GeneratorExecutionContext, FilterContainer<IGeneratorSyntaxFilterWithDiagnostics>>? OnGetFilters;

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
		protected override void Generate(IMemberData member, IGeneratorSyntaxFilterWithDiagnostics filter, in GeneratorExecutionContext context)
		{
			OnGenerate?.Invoke(member, filter, context);
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
