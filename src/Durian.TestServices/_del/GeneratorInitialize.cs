using Durian.Analysis;
using Microsoft.CodeAnalysis;

namespace Durian.TestServices
{
	/// <summary>
	/// A delegate that mirrors the signature of the <see cref="DurianGeneratorBase.Initialize(GeneratorInitializationContext)"/> method.
	/// </summary>
	/// <param name="context">The <see cref="GeneratorInitializationContext"/> to be used when performing the action.</param>
	public delegate void GeneratorInitialize(GeneratorInitializationContext context);
}
