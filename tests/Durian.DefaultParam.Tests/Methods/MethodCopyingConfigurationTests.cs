// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Xunit;

namespace Durian.Analysis.DefaultParam.Tests.Methods
{
	public sealed class MethodCopyingConfigurationTests : DefaultParamGeneratorTestBase
	{
		[Fact]
		public void AppliesReturn_When_MethodIsNotVoid()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.MethodConvention} = {DPMethodConventionProvider.TypeName}.{DPMethodConventionProvider.Call})]
partial class Test
{{
	int Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>()
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
		public void AppliesReturn_When_MethodIsNotVoid_And_HasArguments()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.MethodConvention} = {DPMethodConventionProvider.TypeName}.{DPMethodConventionProvider.Call})]
partial class Test
{{
	int Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>(int a, float b, T value)
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
		public void Calls_When_AppliedGlobally()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.MethodConvention} = {DPMethodConventionProvider.TypeName}.{DPMethodConventionProvider.Call})]
partial class Test
{{
	void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>()
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
	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.MethodConvention} = {DPMethodConventionProvider.TypeName}.{DPMethodConventionProvider.Call})]
	void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>()
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
		public void Calls_When_GloballyFalse_And_InTypeTrue()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.MethodConvention} = {DPMethodConventionProvider.TypeName}.{DPMethodConventionProvider.Copy})]
[{DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.MethodConvention} = {DPMethodConventionProvider.TypeName}.{DPMethodConventionProvider.Call})]
partial class Test
{{
	void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>()
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

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.MethodConvention} = {DPMethodConventionProvider.TypeName}.{DPMethodConventionProvider.Copy})]
partial class Test
{{
	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.MethodConvention} = {DPMethodConventionProvider.TypeName}.{DPMethodConventionProvider.Call})]
	void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>()
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
		public void Calls_When_InTypeFalse_And_LocallyTrue()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.MethodConvention} = {DPMethodConventionProvider.TypeName}.{DPMethodConventionProvider.Copy})]
partial class Test
{{
	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.MethodConvention} = {DPMethodConventionProvider.TypeName}.{DPMethodConventionProvider.Call})]
	void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>()
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
		public void CallsWithFullParameterList_When_IsExtensionMethod()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial static class Test
{{
	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.MethodConvention} = {DPMethodConventionProvider.TypeName}.{DPMethodConventionProvider.Call})]
	public static void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>(this T value)
	{{
		T t = default;
	}}
}}
";

			string expected =
@$"partial static class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T>(T)")}
	public static void Method(this int value)
	{{
		Method<int>(value);
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

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.MethodConvention} = {DPMethodConventionProvider.TypeName}.{DPMethodConventionProvider.Call})]
partial class Test
{{
	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.MethodConvention} = {DPMethodConventionProvider.TypeName}.{DPMethodConventionProvider.Copy})]
	void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>()
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
		public void Copies_When_InTypeTrue_And_LocallyFalse()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.MethodConvention} = {DPMethodConventionProvider.TypeName}.{DPMethodConventionProvider.Call})]
partial class Test
{{
	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.MethodConvention} = {DPMethodConventionProvider.TypeName}.{DPMethodConventionProvider.Copy})]
	void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>()
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
		public void IgnoresConventionConfiguration_When_IsAbstract()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.MethodConvention} = {DPMethodConventionProvider.TypeName}.{DPMethodConventionProvider.Call}]
abstract partial class Test
{{
	public abstract void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>();
}}
";
			string expected =
$@"abstract partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T>()")}
	public abstract void Method();
}}";

			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void ProperlyWritesAllArguments()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.MethodConvention} = {DPMethodConventionProvider.TypeName}.{DPMethodConventionProvider.Call}]
partial class Test
{{
	void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>(int a, float b, T value)
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
		public void WritesProperSignature_When_HasMultipleDefaultParams()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.MethodConvention} = {DPMethodConventionProvider.TypeName}.{DPMethodConventionProvider.Call})]
partial class Test
{{
	void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T, [{DefaultParamAttributeProvider.TypeName}(typeof(string))]U>(T obj1, U obj2)
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
	}
}