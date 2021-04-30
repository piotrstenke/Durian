using System.Runtime.CompilerServices;
using Durian.DefaultParam;
using Durian.Logging;
using Microsoft.CodeAnalysis;

namespace Durian.Tests.DefaultParam.Generator
{
	public abstract class DefaultParamGeneratorTest : LoggableGeneratorTest<DefaultParamGenerator>
	{
		public DefaultParamGeneratorTest() : base(true)
		{
		}

		public sealed override SingletonGeneratorTestResult RunGenerator(string? input, int index, [CallerMemberName] string testName = "")
		{
			return base.RunGenerator(input, DefaultParamGenerator.NumDefaultParamAttributes + index, testName);
		}

		public sealed override SingletonGeneratorTestResult RunGenerator(string? input, [CallerMemberName] string testName = "")
		{
			return base.RunGenerator(input, DefaultParamGenerator.NumDefaultParamAttributes, testName);
		}

		protected sealed override DefaultParamGenerator CreateGenerator(GeneratorLoggingConfiguration configuration, string testName)
		{
			return new TestableGenerator(configuration, testName);
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
				FilterContainer<IDefaultParamFilter> list = new();

				list.RegisterFilterGroup(new IDefaultParamFilter[] { new DefaultParamDelegateFilter(this, FileNameProvider), new DefaultParamMethodFilter(this, FileNameProvider) });
				list.RegisterFilterGroup(new IDefaultParamFilter[] { new DefaultParamTypeFilter(this, FileNameProvider) });

				if (EnableDiagnostics)
				{
					list.RegisterFilterGroup(new IDefaultParamFilter[] { new DefaultParamLocalFunctionFilter(this, FileNameProvider) });
				}

				return list;
			}

			protected override void BeforeFiltrationOfGroup(FilterGroup<IDefaultParamFilter> filterGroup, in GeneratorExecutionContext context)
			{
				if (FileNameProvider is not TestNameToFile t)
				{
					return;
				}

				_analyzerCounter = t.Counter;
				t.Counter = _generatorCounter;
			}

			protected override void AfterFiltrationOfGroup(FilterGroup<IDefaultParamFilter> filterGroup, in GeneratorExecutionContext context)
			{
				if (FileNameProvider is not TestNameToFile t)
				{
					return;
				}

				_generatorCounter = t.Counter;
				t.Counter = _analyzerCounter;
			}
		}
	}
}