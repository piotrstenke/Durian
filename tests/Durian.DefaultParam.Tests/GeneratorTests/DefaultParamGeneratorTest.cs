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
			return base.RunGenerator(input, index, testName);
		}

		public sealed override SingletonGeneratorTestResult RunGenerator(string? input, [CallerMemberName] string testName = "")
		{
			return base.RunGenerator(input, 3, testName);
		}

		protected sealed override DefaultParamGenerator CreateGenerator(GeneratorLoggingConfiguration configuration, string testName)
		{
			return new LocalGenerator(configuration, testName);
		}

		private sealed class LocalGenerator : DefaultParamGenerator
		{
			private readonly string _testName;

			public LocalGenerator(GeneratorLoggingConfiguration configuration, string testName) : base(configuration)
			{
				_testName = testName;
			}

			protected override FilterList<IDefaultParamFilter> GetFilters(in GeneratorExecutionContext context)
			{
				TestNameToFile fileNameProvider = new(_testName);
				FilterList<IDefaultParamFilter> list = new();

				list.RegisterFilterGroup(new IDefaultParamFilter[] { new DefaultParamDelegateFilter(this, fileNameProvider), new DefaultParamMethodFilter(this, fileNameProvider) });
				list.RegisterFilterGroup(new IDefaultParamFilter[] { new DefaultParamTypeFilter(this, fileNameProvider) });

				if (EnableDiagnostics)
				{
					list.RegisterFilterGroup(new IDefaultParamFilter[] { new DefaultParamLocalFunctionFilter(this, fileNameProvider) });
				}

				return list;
			}

			protected override void Generate(IMemberData data, IDefaultParamFilter filter, in GeneratorExecutionContext context)
			{
				if (data is not IDefaultParamTarget target)
				{
					return;
				}

				Generate(target, filter, _testName, in context);
			}
		}
	}
}