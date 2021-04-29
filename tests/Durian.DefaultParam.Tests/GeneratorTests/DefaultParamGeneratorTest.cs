using System.Runtime.CompilerServices;
using Durian.Data;
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
			private readonly TestNameToFile _fileNameProvider;
			private int _analyzerCounter;
			private int _generatorCounter;

			public TestableGenerator(GeneratorLoggingConfiguration configuration, string testName) : base(configuration)
			{
				_fileNameProvider = new(testName);
			}

			protected override FilterContainer<IDefaultParamFilter> GetFilters(in GeneratorExecutionContext context)
			{
				FilterContainer<IDefaultParamFilter> list = new();

				list.RegisterFilterGroup(new IDefaultParamFilter[] { new DefaultParamDelegateFilter(this, _fileNameProvider), new DefaultParamMethodFilter(this, _fileNameProvider) });
				list.RegisterFilterGroup(new IDefaultParamFilter[] { new DefaultParamTypeFilter(this, _fileNameProvider) });

				if (EnableDiagnostics)
				{
					list.RegisterFilterGroup(new IDefaultParamFilter[] { new DefaultParamLocalFunctionFilter(this, _fileNameProvider) });
				}

				return list;
			}

			protected override void BeforeFiltrationOfGroup(FilterGroup<IDefaultParamFilter> filterGroup, in GeneratorExecutionContext context)
			{
				_analyzerCounter = _fileNameProvider.Counter;
				_fileNameProvider.Counter = _generatorCounter;
			}

			protected override void AfterFiltrationOfGroup(FilterGroup<IDefaultParamFilter> filterGroup, in GeneratorExecutionContext context)
			{
				_generatorCounter = _fileNameProvider.Counter;
				_fileNameProvider.Counter = _analyzerCounter;
			}

			protected override void Generate(IMemberData data, IDefaultParamFilter filter, in GeneratorExecutionContext context)
			{
				if (data is not IDefaultParamTarget target)
				{
					return;
				}

				Generate(target, filter, _fileNameProvider.GetFileName(), in context);
				_fileNameProvider.Success();
			}
		}
	}
}