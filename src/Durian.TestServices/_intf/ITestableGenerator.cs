using Durian.Analysis;

namespace Durian.TestServices;

/// <summary>
/// Provides members required for testing source generators.
/// </summary>
public interface ITestableGenerator : IDurianGenerator
{
	/// <summary>
	/// Source generator that is used to actually generate sources.
	/// </summary>
	IDurianGenerator UnderlayingGenerator { get; }

	/// <summary>
	/// Name of the test that is currently running.
	/// </summary>
	string TestName { get; }
}
