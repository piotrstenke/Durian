// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis;
using Durian.Analysis.Data;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Durian.TestServices
{
	/// <summary>
	/// A wrapper for <see cref="GeneratorWithFilters{TFilter}"/> that offers better logging experience.
	/// </summary>
	/// <typeparam name="TFilter">Type of <see cref="IGeneratorSyntaxFilter"/> used to generate sources.</typeparam>
	public class TestableGenerator<TFilter> : ITestableGenerator where TFilter : IGeneratorSyntaxFilter
	{
		private int _analyzerCounter;
		private int _generatorCounter;

		/// <inheritdoc/>
		public IHintNameProvider FileNameProvider
		{
			get => UnderlayingGenerator.FileNameProvider;
			set => UnderlayingGenerator.FileNameProvider = value;
		}

		/// <inheritdoc/>
		public LoggingConfiguration LoggingConfiguration => UnderlayingGenerator.LoggingConfiguration;

		/// <summary>
		/// Name of a test that is currently running.
		/// </summary>
		public string TestName { get; }

		/// <inheritdoc cref="ITestableGenerator.UnderlayingGenerator"/>
		public GeneratorWithFilters<TFilter> UnderlayingGenerator { get; }

		ILoggableGenerator ITestableGenerator.UnderlayingGenerator => UnderlayingGenerator;

		/// <summary>
		/// Initializes a new instance of the <see cref="TestableGenerator{TFilter}"/> class.
		/// </summary>
		/// <param name="generator"><see cref="GeneratorWithFilters{TFilter}"/> that is used to actually generate sources.</param>
		/// <param name="testName">Name of a test that is currently running.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="testName"/> cannot be <see langword="null"/> or empty.</exception>
		public TestableGenerator(GeneratorWithFilters<TFilter> generator, string testName)
		{
			if (generator is null)
			{
				throw new ArgumentNullException(nameof(generator));
			}

			UnderlayingGenerator = generator;
			FileNameProvider = new TestNameToFile(testName);
			TestName = testName;

			generator.AfterExecutionOfGroup += SetAnalyzerMode;
			generator.BeforeExecutionOfGroup += SetGeneratorMode;
			generator.BeforeFiltrationOfGroup += SetAnalyzerMode;
			generator.IterateThroughFilterEvent += IterateThroughFilter;
		}

		/// <inheritdoc/>
		public void Execute(in GeneratorExecutionContext context)
		{
			UnderlayingGenerator.LoggingConfiguration.SupportedLogs &= ~GeneratorLogs.Exception;

			try
			{
				UnderlayingGenerator.Execute(in context);
			}
			catch(Exception e)
			{
				UnderlayingGenerator.LoggingConfiguration.SupportedLogs |= GeneratorLogs.Exception;
				LogException(e, TestName);
				throw;
			}
		}

		/// <inheritdoc/>
		public void Initialize(GeneratorInitializationContext context)
		{
			UnderlayingGenerator.Initialize(context);
		}

		/// <inheritdoc/>
		public void LogDiagnostics(SyntaxNode node, string hintName, IEnumerable<Diagnostic> diagnostics, NodeOutput nodeOutput = default)
		{
			UnderlayingGenerator.LogDiagnostics(node, hintName, diagnostics, nodeOutput);
		}

		/// <inheritdoc/>
		public void LogException(Exception exception)
		{
			UnderlayingGenerator.LogException(exception);
		}

		/// <inheritdoc/>
		public void LogException(Exception exception, string source)
		{
			UnderlayingGenerator.LogException(exception, source);
		}

		/// <inheritdoc/>
		public void LogInputOutput(SyntaxNode input, SyntaxNode output, string hintName, NodeOutput nodeOutput = default)
		{
			UnderlayingGenerator.LogInputOutput(input, output, hintName, nodeOutput);
		}

		/// <inheritdoc/>
		public void LogNode(SyntaxNode node, string hintName, NodeOutput nodeOutput = default)
		{
			UnderlayingGenerator.LogNode(node, hintName, nodeOutput);
		}

		void ISourceGenerator.Execute(GeneratorExecutionContext context)
		{
			Execute(in context);
		}

		private void IterateThroughFilter(TFilter filter, in GeneratorExecutionContext context)
		{
			IEnumerator<IMemberData> iter = filter.GetEnumerator();

			SetAnalyzerMode();

			while (iter.MoveNext())
			{
				SetGeneratorMode();
				UnderlayingGenerator.GenerateFromData(iter.Current, in context);
				SetAnalyzerMode();
			}
		}

		private void SetAnalyzerMode(FilterGroup<TFilter>? filterGroup, in GeneratorExecutionContext context)
		{
			SetAnalyzerMode();
		}

		private void SetAnalyzerMode()
		{
			if (FileNameProvider is not TestNameToFile t)
			{
				return;
			}

			_generatorCounter = t.Counter;
			t.Counter = _analyzerCounter;
		}

		private void SetGeneratorMode(FilterGroup<TFilter>? filterGroup, in GeneratorExecutionContext context)
		{
			SetGeneratorMode();
		}

		private void SetGeneratorMode()
		{
			if (FileNameProvider is not TestNameToFile t)
			{
				return;
			}

			_analyzerCounter = t.Counter;
			t.Counter = _generatorCounter;
		}
	}
}