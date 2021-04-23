using Durian.DefaultParam;
using Xunit;

namespace Durian.Tests.DefaultParam.Generator
{
	public sealed class MethodCopyingConfigurationTests : DefaultParamGeneratorTest
	{
		[Fact]
		public void ProperlyWritesAllArguments()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamConfigurationAttribute.AttributeName}({DefaultParamConfigurationAttribute.CallInsteadOfCopyingProperty} = true)]

partial class Test
{{
	void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T>(int a, float b, T value)
	{{
		T t = default;
	}}
}}
";

			string expected =
@$"using System.CodeDom.Compiler;

partial class Test
{{
	[GeneratedCode(""{DefaultParamGenerator.GeneratorName}"", ""{DefaultParamGenerator.Version}"")]
	void Method(int a, float b, int value)
	{{
		Method<int>(a, b, value);
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void AppliesReturn_When_MethodIsNotVoid_And_HasArguments()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamConfigurationAttribute.AttributeName}({DefaultParamConfigurationAttribute.CallInsteadOfCopyingProperty} = true)]

partial class Test
{{
	int Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T>(int a, float b, T value)
	{{
		T t = default;
		return 2;
	}}
}}
";

			string expected =
@$"using System.CodeDom.Compiler;

partial class Test
{{
	[GeneratedCode(""{DefaultParamGenerator.GeneratorName}"", ""{DefaultParamGenerator.Version}"")]
	int Method(int a, float b, int value)
	{{
		return Method<int>(a, b, value);
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void AppliesReturn_When_MethodIsNotVoid()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamConfigurationAttribute.AttributeName}({DefaultParamConfigurationAttribute.CallInsteadOfCopyingProperty} = true)]

partial class Test
{{
	int Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T>()
	{{
		T t = default;
		return 2;
	}}
}}
";
			string expected =
@$"using System.CodeDom.Compiler;

partial class Test
{{
	[GeneratedCode(""{DefaultParamGenerator.GeneratorName}"", ""{DefaultParamGenerator.Version}"")]
	int Method()
	{{
		return Method<int>();
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Calls_When_AppliedGlobally()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamConfigurationAttribute.AttributeName}({DefaultParamConfigurationAttribute.CallInsteadOfCopyingProperty} = true)]

partial class Test
{{
	void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T>()
	{{
		T value = default;
	}}
}}
";
			string expected =
@$"using System.CodeDom.Compiler;

partial class Test
{{
	[GeneratedCode(""{DefaultParamGenerator.GeneratorName}"", ""{DefaultParamGenerator.Version}"")]
	void Method()
	{{
		Method<int>();
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Calls_When_AppliedLocally()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial class Test
{{
	[{DefaultParamMethodConfigurationAttribute.AttributeName}({DefaultParamMethodConfigurationAttribute.CallInsteadOfCopyingProperty} = true)]
	void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T>()
	{{
		T value = default;
	}}
}}
";
			string expected =
@$"using System.CodeDom.Compiler;

partial class Test
{{
	[GeneratedCode(""{DefaultParamGenerator.GeneratorName}"", ""{DefaultParamGenerator.Version}"")]
	void Method()
	{{
		Method<int>();
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Calls_When_GlobalyFalse_And_LocallyTrue()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamConfigurationAttribute.AttributeName}({DefaultParamConfigurationAttribute.CallInsteadOfCopyingProperty} = false)]

partial class Test
{{
	[{DefaultParamMethodConfigurationAttribute.AttributeName}({DefaultParamMethodConfigurationAttribute.CallInsteadOfCopyingProperty} = true)]
	void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T>()
	{{
		T value = default;
	}}
}}
";
			string expected =
@$"using System.CodeDom.Compiler;

partial class Test
{{
	[GeneratedCode(""{DefaultParamGenerator.GeneratorName}"", ""{DefaultParamGenerator.Version}"")]
	void Method()
	{{
		Method<int>();
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Copies_When_GloballyTrue_And_LocallyFalse()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamConfigurationAttribute.AttributeName}({DefaultParamConfigurationAttribute.CallInsteadOfCopyingProperty} = true)]

partial class Test
{{
	[{DefaultParamMethodConfigurationAttribute.AttributeName}({DefaultParamMethodConfigurationAttribute.CallInsteadOfCopyingProperty} = false)]
	void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T>()
	{{
		T value = default;
	}}
}}
";
			string expected =
@$"using System.CodeDom.Compiler;

partial class Test
{{
	[GeneratedCode(""{DefaultParamGenerator.GeneratorName}"", ""{DefaultParamGenerator.Version}"")]
	void Method()
	{{
		int value = default;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Copies_When_GloballyFalse_And_LocallyFalse()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamConfigurationAttribute.AttributeName}({DefaultParamConfigurationAttribute.CallInsteadOfCopyingProperty} = false)]

partial class Test
{{
	[{DefaultParamMethodConfigurationAttribute.AttributeName}({DefaultParamMethodConfigurationAttribute.CallInsteadOfCopyingProperty} = false)]
	void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T>()
	{{
		T value = default;
	}}
}}
";
			string expected =
@$"using System.CodeDom.Compiler;

partial class Test
{{
	[GeneratedCode(""{DefaultParamGenerator.GeneratorName}"", ""{DefaultParamGenerator.Version}"")]
	void Method()
	{{
		int value = default;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}
	}
}