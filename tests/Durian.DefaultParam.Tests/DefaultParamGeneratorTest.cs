﻿using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Durian.Data;
using Durian.DefaultParam;
using Durian.Logging;
using Microsoft.CodeAnalysis;

namespace Durian.Tests.DefaultParam
{
	public abstract class DefaultParamGeneratorTest : LoggableGeneratorTest<DefaultParamGenerator>
	{
		public DefaultParamGeneratorTest() : base(true)
		{
		}

		public sealed override SingletonGeneratorTestResult RunGenerator(string? input, int index, [CallerMemberName] string testName = "")
		{
			return base.RunGenerator(input, DefaultParamGenerator.NumStaticTrees + index, testName);
		}

		public sealed override SingletonGeneratorTestResult RunGenerator(string? input, [CallerMemberName] string testName = "")
		{
			return base.RunGenerator(input, DefaultParamGenerator.NumStaticTrees, testName);
		}

		protected sealed override DefaultParamGenerator CreateGenerator(GeneratorLoggingConfiguration configuration, string testName)
		{
			return new TestableGenerator(configuration, testName);
		}

		public static string GetCodeGenerationAttributes(string? source, int indent = 1)
		{
			return AutoGenerated.GetCodeGenerationAttributes(DefaultParamGenerator.GeneratorName, DefaultParamGenerator.Version, source, indent);
		}

		private sealed class TestableGenerator : DefaultParamGenerator
		{
			private int _analyzerCounter;
			private int _generatorCounter;

			public TestableGenerator(GeneratorLoggingConfiguration configuration, string testName) : base(configuration, new TestNameToFile(testName))
			{
			}

			protected override FilterContainer<IDefaultParamFilter> GetFilters(in GeneratorExecutionContext context)
			{
				return GetFilters(FileNameProvider);
			}

			protected override void BeforeFiltrationOfGroup(FilterGroup<IDefaultParamFilter> filterGroup, in GeneratorExecutionContext context)
			{
				SetAnalyzerMode();
			}

			protected override void BeforeExecutionOfGroup(FilterGroup<IDefaultParamFilter> filterGroup, in GeneratorExecutionContext context)
			{
				SetGeneratorMode();
			}

			protected override void AfterExecutionOfGroup(FilterGroup<IDefaultParamFilter> filterGroup, in GeneratorExecutionContext context)
			{
				SetAnalyzerMode();
			}

			protected override void IterateThroughFilter(IDefaultParamFilter filter, in GeneratorExecutionContext context)
			{
				IEnumerator<IMemberData> iter = filter.GetEnumerator();

				SetAnalyzerMode();

				while (iter.MoveNext())
				{
					SetGeneratorMode();
					GenerateFromData(iter.Current, in context);
					SetAnalyzerMode();
				}
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
}