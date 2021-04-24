using System.Runtime.CompilerServices;
using Durian.Logging;

namespace Durian.Tests
{
	/// <summary>
	/// An abstract class that provides methods to test <see cref="ILoggableSourceGenerator"/>s using a <see cref="GeneratorWrapper{T}"/>.
	/// </summary>
	public abstract class GeneratorWrapperTest<T> where T : ILoggableSourceGenerator
	{
		private static string? _typeName;

		/// <summary>
		/// Provides <see cref="ILoggableSourceGenerator"/>s to test.
		/// </summary>
		public GeneratorWrapper<T> Wrapper { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneratorWrapperTest{T}"/> class.
		/// </summary>
		protected GeneratorWrapperTest()
		{
			if(_typeName is null)
			{
				_typeName = GetType().Name;
			}

			Wrapper = new(config => CreateGenerator(config));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneratorWrapperTest{T}"/> class.
		/// </summary>
		/// <param name="enableDiagnostics">Determines whether to enable diagnostics for the created <see cref="ILoggableSourceGenerator"/> if it supports any.</param>
		protected GeneratorWrapperTest(bool enableDiagnostics)
		{
			if (_typeName is null)
			{
				_typeName = GetType().Name;
			}

			if (enableDiagnostics)
			{
				Wrapper = new(config =>
				{
					T generator = CreateGenerator(config);

					if (generator is IDurianSourceGenerator g && g.SupportsDiagnostics)
					{
						g.EnableDiagnostics = true;
					}

					return generator;
				});
			}
			else
			{
				Wrapper = new(config => CreateGenerator(config));
			}
		}

		/// <summary>
		/// Creates a new <see cref="ILoggableSourceGenerator"/> based on the specified <see cref="GeneratorLoggingConfiguration"/>.
		/// </summary>
		/// <param name="loggingConfiguration"><see cref="GeneratorLoggingConfiguration"/> for the newly-created <see cref="ILoggableSourceGenerator"/>.</param>
		protected abstract T CreateGenerator(GeneratorLoggingConfiguration loggingConfiguration);

		/// <summary>
		/// Returns a <see cref="SingletonGeneratorTestResult"/> created by performing a test on the target <see cref="ILoggableSourceGenerator"/>.
		/// </summary>
		/// <param name="input">Input for the generator.</param>
		/// <param name="testName">Name of the test that is currently performed.</param>
		public virtual SingletonGeneratorTestResult RunGenerator(string? input, [CallerMemberName]string testName = "")
		{
			Wrapper.TestName = $"{_typeName}/testName";
			return GeneratorTest.RunGenerator(input, Wrapper.CreateGenerator());
		}

		/// <summary>
		/// Returns a <see cref="SingletonGeneratorTestResult"/> created by performing a test on the target <see cref="ILoggableSourceGenerator"/>.
		/// </summary>
		/// <param name="input">Input for the generator.</param>
		/// <param name="index">Index of the source in the generator's output.</param>
		/// <param name="testName">Name of the test that is currently performed.</param>
		public virtual SingletonGeneratorTestResult RunGenerator(string? input, int index, [CallerMemberName]string testName = "")
		{
			Wrapper.TestName = $"{_typeName}/testName";
			return GeneratorTest.RunGenerator(input, Wrapper.CreateGenerator(), index);
		}
	}
}
