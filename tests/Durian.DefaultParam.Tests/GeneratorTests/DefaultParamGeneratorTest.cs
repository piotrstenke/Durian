using System;
using System.Runtime.CompilerServices;
using Durian.Data;
using Durian.DefaultParam;
using Durian.Logging;
using Microsoft.CodeAnalysis;

namespace Durian.Tests.DefaultParam.Generator
{
	public abstract class DefaultParamGeneratorTest : GeneratorWrapperTest<DefaultParamGenerator>
	{
		public DefaultParamGeneratorTest() : base(true)
		{

		}

		public sealed override SingletonGeneratorTestResult RunGenerator(string? input, int index, [CallerMemberName]string testName = "")
		{
			return base.RunGenerator(input, index, testName);
		}

		public sealed override SingletonGeneratorTestResult RunGenerator(string? input, [CallerMemberName]string testName = "")
		{
			return RunGenerator(input, 3, testName);
		}

		protected sealed override DefaultParamGenerator CreateGenerator(GeneratorLoggingConfiguration loggingConfiguration)
		{
			return new DefaultParamGenerator(loggingConfiguration);
		}
	}
}