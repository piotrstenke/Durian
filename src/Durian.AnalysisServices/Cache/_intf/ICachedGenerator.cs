using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Cache
{
	/// <summary>
	/// <see cref="IDurianGenerator"/> that can retrieve data defined in a <see cref="CachedGeneratorExecutionContext{T}"/>.
	/// </summary>
	/// <typeparam name="T">Type of values this generator can retrieve from the <see cref="CachedGeneratorExecutionContext{T}"/>.</typeparam>
	public interface ICachedGenerator<T> : IDurianGenerator
	{
		/// <summary>
		/// Executes the generator using a <see cref="CachedGeneratorExecutionContext{T}"/> instead of usual <see cref="GeneratorExecutionContext"/>.
		/// </summary>
		/// <param name="context"><see cref="CachedGeneratorExecutionContext{T}"/> that is used to execute the generator.</param>
		bool Execute(in CachedGeneratorExecutionContext<T> context);
	}
}
