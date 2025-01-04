using Durian.Analysis;
using Microsoft.CodeAnalysis;

namespace Durian.TestServices
{
	/// <summary>
	/// A wrapper for <see cref="ISourceGenerator"/> that offers better logging experience.
	/// </summary>
	public interface ITestableGenerator : IDurianGenerator
	{
		/// <summary>
		/// <see cref="ISourceGenerator"/> that is used to actually generate sources.
		/// </summary>
		IDurianGenerator UnderlayingGenerator { get; }
	}
}
