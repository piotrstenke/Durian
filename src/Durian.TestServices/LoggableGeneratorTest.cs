using System;
using System.Runtime.CompilerServices;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;

namespace Durian.TestServices;

/// <summary>
/// An abstract class that provides methods to test source generators and log information about the generator test.
/// </summary>
/// <typeparam name="T">Type of target source generator.</typeparam>
public abstract class LoggableGeneratorTest<T>
{
	internal readonly LoggingConfiguration _configuration;

	private readonly bool _enableDiagnostics;

	/// <summary>
	/// Initializes a new instance of the <see cref="LoggableGeneratorTest{T}"/> class.
	/// </summary>
	protected LoggableGeneratorTest() : this(false)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="LoggableGeneratorTest{T}"/> class.
	/// </summary>
	/// <param name="enableDiagnostics">Determines whether to enable diagnostics for the created source generator if it supports any.</param>
	protected LoggableGeneratorTest(bool enableDiagnostics) : this(enableDiagnostics, typeof(T))
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="LoggableGeneratorTest{T}"/> class.
	/// </summary>
	/// <param name="enableDiagnostics">Determines whether to enable diagnostics for the created source generator if it supports any.</param>
	/// <param name="generatorType"><see cref="Type"/> to get the <see cref="LoggingConfiguration"/> from.</param>
	protected LoggableGeneratorTest(bool enableDiagnostics, Type generatorType)
	{
		_configuration = LoggingConfiguration.ForGenerator(generatorType);
		_configuration.LogDirectory += $"/{GetType().Name}";
		_enableDiagnostics = enableDiagnostics;
	}

	/// <summary>
	/// Returns a <see cref="SingleGeneratorTestResult"/> created by performing a test on the target source generator.
	/// </summary>
	/// <param name="input">Input for the generator.</param>
	/// <param name="testName">Name of the test that is currently performed.</param>
	/// <exception cref="InvalidOperationException"><see cref="CreateGenerator(LoggingConfiguration, string)"/> returned <see langword="null"/>.</exception>
	public virtual SingleGeneratorTestResult RunGenerator(string? input, [CallerMemberName] string testName = "")
	{
		return RunGenerator(input, 0, testName);
	}

	/// <summary>
	/// Returns a <see cref="SingleGeneratorTestResult"/> created by performing a test on the target source generator.
	/// </summary>
	/// <param name="input">Input for the generator.</param>
	/// <param name="index">Index of the source in the generator's output.</param>
	/// <param name="testName">Name of the test that is currently performed.</param>
	/// <exception cref="InvalidOperationException"><see cref="CreateGenerator(LoggingConfiguration, string)"/> returned <see langword="null"/>.</exception>
	public virtual SingleGeneratorTestResult RunGenerator(string? input, int index, [CallerMemberName] string testName = "")
	{
		T generator = GetGeneratorAndTryEnableDiagnostics(testName);

		if (generator is IIncrementalGenerator incremental)
		{
			return IncrementalGeneratorTest.RunGenerator(incremental, input, index);
		}

		return SourceGeneratorTest.RunGenerator((generator as ISourceGenerator)!, input, index);
	}

	/// <summary>
	/// Returns a <see cref="SingleGeneratorTestResult"/> created by performing a test on the target source generator.
	/// </summary>
	/// <param name="input">Input for the generator.</param>
	/// <param name="external">Code in external assembly that is referenced by assembly containing the <paramref name="input"/> text.</param>
	/// <param name="testName">Name of the test that is currently performed.</param>
	/// <exception cref="InvalidOperationException"><see cref="CreateGenerator(LoggingConfiguration, string)"/> returned <see langword="null"/>.</exception>
	public virtual SingleGeneratorTestResult RunGeneratorWithDependency(string? input, string external, [CallerMemberName] string testName = "")
	{
		return RunGeneratorWithDependency(input, external, 0, testName);
	}

	/// <summary>
	/// Returns a <see cref="SingleGeneratorTestResult"/> created by performing a test on the target source generator.
	/// </summary>
	/// <param name="input">Input for the generator.</param>
	/// <param name="external">Code in external assembly that is referenced by assembly containing the <paramref name="input"/> text.</param>
	/// <param name="index">Index of the source in the generator's output.</param>
	/// <param name="testName">Name of the test that is currently performed.</param>
	/// <exception cref="InvalidOperationException"><see cref="CreateGenerator(LoggingConfiguration, string)"/> returned <see langword="null"/>.</exception>
	public virtual SingleGeneratorTestResult RunGeneratorWithDependency(string? input, string external, int index, [CallerMemberName] string testName = "")
	{
		T generator = GetGeneratorAndTryEnableDiagnostics(testName);

		if (generator is IIncrementalGenerator incremental)
		{
			return IncrementalGeneratorTest.RunGeneratorWithDependency(incremental, input, external, index);
		}

		return SourceGeneratorTest.RunGeneratorWithDependency((generator as ISourceGenerator)!, input, external, index);
	}

	/// <summary>
	/// Returns a <see cref="MultipleGeneratorTestResult"/> created by performing a test on the target source generator.
	/// </summary>
	/// <param name="input">Input for the generator.</param>
	/// <param name="testName">Name of the test that is currently performed.</param>
	public virtual MultipleGeneratorTestResult RunGeneratorWithMultipleOutputs(string? input, [CallerMemberName] string testName = "")
	{
		return RunGeneratorWithMultipleOutputs(input, 0, testName);
	}

	/// <summary>
	/// Returns a <see cref="MultipleGeneratorTestResult"/> created by performing a test on the target source generator.
	/// </summary>
	/// <param name="input">Input for the generator.</param>
	/// <param name="startIndex">Number of generated sources to skip.</param>
	/// <param name="testName">Name of the test that is currently performed.</param>
	public virtual MultipleGeneratorTestResult RunGeneratorWithMultipleOutputs(string? input, int startIndex, [CallerMemberName] string testName = "")
	{
		T generator = GetGeneratorAndTryEnableDiagnostics(testName);

		if(generator is IIncrementalGenerator incremental)
		{
			return IncrementalGeneratorTest.RunGeneratorWithMultipleOutputs(incremental, input, startIndex);
		}

		return SourceGeneratorTest.RunGeneratorWithMultipleOutputs((generator as ISourceGenerator)!, input, startIndex);
	}

	/// <summary>
	/// Creates a new source generator based on the specified <paramref name="configuration"/> and <paramref name="testName"/>.
	/// </summary>
	/// <param name="configuration">Configuration for the source generator.</param>
	/// <param name="testName">Name of the current test.</param>
	protected abstract T CreateGenerator(LoggingConfiguration configuration, string testName);

	private T GetGeneratorAndTryEnableDiagnostics(string testName)
	{
		T generator = CreateGenerator(_configuration, testName);

		if (generator is null)
		{
			throw new InvalidOperationException($"{nameof(CreateGenerator)} returned null");
		}

		if (generator is not ISourceGenerator and not IIncrementalGenerator)
		{
			throw new InvalidOperationException($"{nameof(CreateGenerator)} returned a generator that does not implement the {nameof(ISourceGenerator)} or {nameof(IIncrementalGenerator)} interfaces");
		}

		if (_enableDiagnostics && generator is ILoggableSourceGenerator g && g.LogHandler is not null)
		{
			g.LogHandler.EnableDiagnosticsIfSupported();
		}

		return generator;
	}
}
