using Microsoft.CodeAnalysis;

namespace Durian.Tests
{
	/// <summary>
	/// A delegate that mirrors the signature of the <see cref="SourceGenerator{TCompilationData, TSyntaxReceiver, TFilter}.Initialize(GeneratorInitializationContext)"/> method.
	/// </summary>
	/// <param name="context">The <see cref="GeneratorInitializationContext"/> to be used when performing the action.</param>
	public delegate void GeneratorInitialize(GeneratorInitializationContext context);
}
