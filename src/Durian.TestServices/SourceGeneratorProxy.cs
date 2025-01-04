using Microsoft.CodeAnalysis;

namespace Durian.TestServices
{
	/// <summary>
	/// A simple proxy class for the <see cref="ISourceGenerator"/> interface that leaves the actual implementation details to be defined by the user through appropriate C# events, as well as provides references to the passed <see cref="GeneratorInitializationContext"/> and <see cref="GeneratorExecutionContext"/>.
	/// </summary>
	public class SourceGeneratorProxy : ISourceGenerator
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
		/// Event invoked when the <see cref="Execute"/> method is called.
		/// </summary>
		public event GeneratorExecute? OnExecute;

		/// <summary>
		/// Event invoked when the <see cref="Initialize"/> method is called.
		/// </summary>
		public event GeneratorInitialize? OnInitialize;

		/// <summary>
		/// Initializes a new instance of the <see cref="SourceGeneratorProxy"/> class.
		/// </summary>
		public SourceGeneratorProxy()
		{
		}

		/// <summary>
		/// Actually runs the generation.
		/// </summary>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> to be used during the current generation pass.</param>
		public void Execute(GeneratorExecutionContext context)
		{
			_exeContext = context;

			OnExecute?.Invoke(in _exeContext);
		}

		/// <summary>
		/// Initializes the <see cref="SourceGeneratorProxy"/>.
		/// </summary>
		/// <param name="context"><see cref="GeneratorInitializationContext"/> to be used when initializing the <see cref="SourceGeneratorProxy"/>.</param>
		public void Initialize(GeneratorInitializationContext context)
		{
			_initContext = context;

			OnInitialize?.Invoke(_initContext);
		}

		/// <summary>
		/// Disposes the proxy by setting the <see cref="ExecutionContext"/> and <see cref="InitializationContext"/> to its default values.
		/// </summary>
		public void Release()
		{
			_exeContext = default;
			_initContext = default;
		}
	}
}
