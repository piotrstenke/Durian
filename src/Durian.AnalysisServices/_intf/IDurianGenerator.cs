using System;

namespace Durian.Analysis;

/// <summary>
/// Provides additional information about a source generator.
/// </summary>
public interface IDurianGenerator : IDisposable
{
	/// <summary>
	/// Name of this source generator.
	/// </summary>
	string? GeneratorName { get; }

	/// <summary>
	/// Version of this source generator.
	/// </summary>
	string? GeneratorVersion { get; }

	/// <summary>
	/// Number of trees generated statically by this generator.
	/// </summary>
	int NumStaticTrees { get; }
}
