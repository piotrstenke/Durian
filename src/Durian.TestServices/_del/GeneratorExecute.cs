using Microsoft.CodeAnalysis;
using Durian.Generator;

namespace Durian.Tests
{
	/// <summary>
	/// A delegate that mirrors the signature of the <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}.Execute(in GeneratorExecutionContext)"/> method.
	/// </summary>
	/// <param name="context">The <see cref="GeneratorExecutionContext"/> to be used when performing the action.</param>
	public delegate void GeneratorExecute(in GeneratorExecutionContext context);
}
