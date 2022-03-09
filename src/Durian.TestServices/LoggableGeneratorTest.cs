// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis;
using Durian.Analysis.Logging;
using System;
using System.Runtime.CompilerServices;

namespace Durian.TestServices
{
	/// <summary>
	/// An abstract class that provides methods to test <see cref="ILoggableGenerator"/>s and log information about the generator test.
	/// </summary>
	/// <typeparam name="T">Type of target <see cref="ILoggableGenerator"/>.</typeparam>
	public abstract class LoggableGeneratorTest<T> where T : ILoggableGenerator
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
		/// <param name="enableDiagnostics">Determines whether to enable diagnostics for the created <see cref="ILoggableGenerator"/> if it supports any.</param>
		protected LoggableGeneratorTest(bool enableDiagnostics) : this(enableDiagnostics, typeof(T))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggableGeneratorTest{T}"/> class.
		/// </summary>
		/// <param name="enableDiagnostics">Determines whether to enable diagnostics for the created <see cref="ILoggableGenerator"/> if it supports any.</param>
		/// <param name="generatorType"><see cref="Type"/> to get the <see cref="LoggingConfiguration"/> from.</param>
		protected LoggableGeneratorTest(bool enableDiagnostics, Type generatorType)
		{
			_configuration = LoggingConfiguration.CreateConfigurationForGenerator(generatorType);
			_configuration.LogDirectory += $"/{GetType().Name}";
			_enableDiagnostics = enableDiagnostics;
		}

		/// <summary>
		/// Returns a <see cref="SingletonGeneratorTestResult"/> created by performing a test on the target <see cref="ILoggableGenerator"/>.
		/// </summary>
		/// <param name="input">Input for the generator.</param>
		/// <param name="testName">Name of the test that is currently performed.</param>
		/// <exception cref="InvalidOperationException"><see cref="CreateGenerator(LoggingConfiguration, string)"/> returned <see langword="null"/>.</exception>
		public virtual SingletonGeneratorTestResult RunGenerator(string? input, [CallerMemberName] string testName = "")
		{
			return RunGenerator(input, 0, testName);
		}

		/// <summary>
		/// Returns a <see cref="SingletonGeneratorTestResult"/> created by performing a test on the target <see cref="ILoggableGenerator"/>.
		/// </summary>
		/// <param name="input">Input for the generator.</param>
		/// <param name="index">Index of the source in the generator's output.</param>
		/// <param name="testName">Name of the test that is currently performed.</param>
		/// <exception cref="InvalidOperationException"><see cref="CreateGenerator(LoggingConfiguration, string)"/> returned <see langword="null"/>.</exception>
		public virtual SingletonGeneratorTestResult RunGenerator(string? input, int index, [CallerMemberName] string testName = "")
		{
			return GeneratorTest.RunGenerator(GetGeneratorAndTryEnableDiagnostics(testName), input, index);
		}

		/// <summary>
		/// Returns a <see cref="MultiOutputGeneratorTestResult"/> created by performing a test on the target <see cref="ILoggableGenerator"/>.
		/// </summary>
		/// <param name="input">Input for the generator.</param>
		/// <param name="testName">Name of the test that is currently performed.</param>
		public virtual MultiOutputGeneratorTestResult RunGeneratorWithMultipleOutputs(string? input, [CallerMemberName] string testName = "")
		{
			return RunGeneratorWithMultipleOutputs(input, 0, testName);
		}

		/// <summary>
		/// Returns a <see cref="MultiOutputGeneratorTestResult"/> created by performing a test on the target <see cref="ILoggableGenerator"/>.
		/// </summary>
		/// <param name="input">Input for the generator.</param>
		/// <param name="startIndex">Number of generated sources to skip.</param>
		/// <param name="testName">Name of the test that is currently performed.</param>
		public virtual MultiOutputGeneratorTestResult RunGeneratorWithMultipleOutputs(string? input, int startIndex, [CallerMemberName] string testName = "")
		{
			return GeneratorTest.RunGeneratorWithMultipleOutputs(GetGeneratorAndTryEnableDiagnostics(testName), input, startIndex);
		}

		/// <summary>
		/// Returns a <see cref="SingletonGeneratorTestResult"/> created by performing a test on the target <see cref="ILoggableGenerator"/>.
		/// </summary>
		/// <param name="input">Input for the generator.</param>
		/// <param name="external">Code in external assembly that is referenced by assembly containing the <paramref name="input"/> text.</param>
		/// <param name="testName">Name of the test that is currently performed.</param>
		/// <exception cref="InvalidOperationException"><see cref="CreateGenerator(LoggingConfiguration, string)"/> returned <see langword="null"/>.</exception>
		public virtual SingletonGeneratorTestResult RunGeneratorWithDependency(string? input, string external, [CallerMemberName]string testName = "")
		{
			return RunGeneratorWithDependency(input, external, 0, testName);
		}

		/// <summary>
		/// Returns a <see cref="SingletonGeneratorTestResult"/> created by performing a test on the target <see cref="ILoggableGenerator"/>.
		/// </summary>
		/// <param name="input">Input for the generator.</param>
		/// <param name="external">Code in external assembly that is referenced by assembly containing the <paramref name="input"/> text.</param>
		/// <param name="index">Index of the source in the generator's output.</param>
		/// <param name="testName">Name of the test that is currently performed.</param>
		/// <exception cref="InvalidOperationException"><see cref="CreateGenerator(LoggingConfiguration, string)"/> returned <see langword="null"/>.</exception>
		public virtual SingletonGeneratorTestResult RunGeneratorWithDependency(string? input, string external, int index, [CallerMemberName] string testName = "")
		{
			return GeneratorTest.RunGeneratorWithDependency(GetGeneratorAndTryEnableDiagnostics(testName), input, external, index);
		}

		/// <summary>
		/// Creates a new <see cref="ILoggableGenerator"/> based on the specified <paramref name="configuration"/> and <paramref name="testName"/>.
		/// </summary>
		/// <param name="configuration">Configuration for the <see cref="ILoggableGenerator"/>.</param>
		/// <param name="testName">Name of the current test.</param>
		protected abstract T CreateGenerator(LoggingConfiguration configuration, string testName);

		private T GetGeneratorAndTryEnableDiagnostics(string testName)
		{
			T generator = CreateGenerator(_configuration, testName);

			if (generator is null)
			{
				throw new InvalidOperationException($"{nameof(CreateGenerator)} returned null");
			}

			if (_enableDiagnostics && generator is IDurianGenerator g && g.SupportsDiagnostics)
			{
				g.EnableDiagnostics = true;
			}

			return generator;
		}
	}
}