using Durian.Logging;
using Microsoft.CodeAnalysis;

namespace Durian.Tests
{
	/// <summary>
	/// A <see cref="IFileNameProvider"/> that returns name of the current test.
	/// </summary>
	public sealed class TestNameToFile : IFileNameProvider
	{
		/// <summary>
		/// Name of the current test.
		/// </summary>
		public string TestName { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="TestNameToFile"/> class.
		/// </summary>
		/// <param name="testName">Name of the current test.</param>
		public TestNameToFile(string testName)
		{
			TestName = testName;
		}

		/// <inheritdoc/>
		public string GetFileName(ISymbol symbol)
		{
			return TestName;
		}
	}
}
