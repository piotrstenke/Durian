using Durian.DefaultParam;
using Xunit;

namespace Durian.Tests.DefaultParam
{
	public sealed class MethodCopyingConfigurationTests : DefaultParamGeneratorTest
	{
		[Fact]
		public void ProperlyWritesAllArguments()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamConfigurationAttribute.AttributeName}({DefaultParamConfigurationAttribute.MethodConvetionProperty} = {DPMethodGenConvention.Name}.{nameof(DPMethodGenConvention.Call)}]

partial class Test
{{
	void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T>(int a, float b, T value)
	{{
		T t = default;
	}}
}}
";

			string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T>(int, float, T)")}
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

[assembly: {DefaultParamConfigurationAttribute.AttributeName}({DefaultParamConfigurationAttribute.MethodConvetionProperty} = {DPMethodGenConvention.Name}.{nameof(DPMethodGenConvention.Call)})]

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
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T>(int, float, T)")}
	int Method(int a, float b, int value)
	{{
		return Method<int>(a, b, value);
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void WritesProperSignature_When_HasMultipleDefaultParams()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamConfigurationAttribute.AttributeName}({DefaultParamConfigurationAttribute.MethodConvetionProperty} = {DPMethodGenConvention.Name}.{nameof(DPMethodGenConvention.Call)})]

partial class Test
{{
	void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T, [{DefaultParamAttribute.AttributeName}(typeof(string))]U>(T obj1, U obj2)
	{{
	}}
}}
";

			string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T, U>(T, U)")}
	void Method<T>(T obj1, string obj2)
	{{
		Method<T, string>(obj1, obj2);
	}}
	
	{GetCodeGenerationAttributes("Test.Method<T, U>(T, U)")}
	void Method(int obj1, string obj2)
	{{
		Method<int, string>(obj1, obj2);
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

[assembly: {DefaultParamConfigurationAttribute.AttributeName}({DefaultParamConfigurationAttribute.MethodConvetionProperty} = {DPMethodGenConvention.Name}.{nameof(DPMethodGenConvention.Call)})]

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
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T>()")}
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

[assembly: {DefaultParamConfigurationAttribute.AttributeName}({DefaultParamConfigurationAttribute.MethodConvetionProperty} = {DPMethodGenConvention.Name}.{nameof(DPMethodGenConvention.Call)})]

partial class Test
{{
	void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T>()
	{{
		T value = default;
	}}
}}
";
			string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T>()")}
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
	[{DefaultParamConfigurationAttribute.AttributeName}({DefaultParamConfigurationAttribute.MethodConvetionProperty} = {DPMethodGenConvention.Name}.{nameof(DPMethodGenConvention.Call)})]
	void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T>()
	{{
		T value = default;
	}}
}}
";
			string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T>()")}
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

[assembly: {DefaultParamConfigurationAttribute.AttributeName}({DefaultParamConfigurationAttribute.MethodConvetionProperty} = {DPMethodGenConvention.Name}.{nameof(DPMethodGenConvention.Copy)})]

partial class Test
{{
	[{DefaultParamConfigurationAttribute.AttributeName}({DefaultParamConfigurationAttribute.MethodConvetionProperty} = {DPMethodGenConvention.Name}.{nameof(DPMethodGenConvention.Call)})]
	void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T>()
	{{
		T value = default;
	}}
}}
";
			string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T>()")}
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

[assembly: {DefaultParamConfigurationAttribute.AttributeName}({DefaultParamConfigurationAttribute.MethodConvetionProperty} = {DPMethodGenConvention.Name}.{nameof(DPMethodGenConvention.Call)})]

partial class Test
{{
	[{DefaultParamConfigurationAttribute.AttributeName}({DefaultParamConfigurationAttribute.MethodConvetionProperty} = {DPMethodGenConvention.Name}.{nameof(DPMethodGenConvention.Copy)})]
	void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T>()
	{{
		T value = default;
	}}
}}
";
			string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T>()")}
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

[assembly: {DefaultParamConfigurationAttribute.AttributeName}({DefaultParamConfigurationAttribute.MethodConvetionProperty} = {DPMethodGenConvention.Name}.{nameof(DPMethodGenConvention.Copy)})]

partial class Test
{{
	[{DefaultParamConfigurationAttribute.AttributeName}({DefaultParamConfigurationAttribute.MethodConvetionProperty} = {DPMethodGenConvention.Name}.{nameof(DPMethodGenConvention.Copy)})]
	void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T>()
	{{
		T value = default;
	}}
}}
";
			string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T>()")}
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