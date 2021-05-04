using Durian.DefaultParam;
using Xunit;

namespace Durian.Tests.DefaultParam
{
	public sealed class MethodValuesConfigurationTests : DefaultParamGeneratorTest
	{
		[Fact]
		public void AllowsDifferentValueThanBase_When_StatedInConfigurationAttribute()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamConfigurationAttribute.AttributeName}({DefaultParamConfigurationAttribute.AllowOverridingOfDefaultParamValuesProperty} = true)]

partial class Parent
{{	
	public virtual void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T>(T value)
	{{
	}}
}}

partial class Child : Parent
{{
	public override void Method<[{DefaultParamAttribute.AttributeName}(typeof(string))]T>(T value)
	{{
	}}
}}
";
			string expected =
$@"partial class Child
{{
	{GetCodeGenerationAttributes("Child.Method<T>(T)")}
	public override void Method(string value)
	{{
	}}
}}
";
			Assert.True(RunGenerator(input, 1).Compare(expected));
		}

		[Fact]
		public void AllowsAddingNewDefaultParamAttributesToOverrideMethod_When_StatedInConfigurationAttribute()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamConfigurationAttribute.AttributeName}({DefaultParamConfigurationAttribute.AllowAddingDefaultParamToNewParametersProperty} = true)]

partial class Parent
{{	
	public virtual void Method<T, [{DefaultParamAttribute.AttributeName}(typeof(string))]U>(T value)
	{{
	}}
}}

partial class Child : Parent
{{
	public override void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T, [{DefaultParamAttribute.AttributeName}(typeof(string))]U>(T value)
	{{
	}}
}}
";
			string expected =
$@"partial class Child
{{
	{GetCodeGenerationAttributes("Child.Method<T, U>(T)")}
	public override void Method<T>(T value)
	{{
	}}

	{GetCodeGenerationAttributes("Child.Method<T, U>(T)")}
	public override void Method(int value)
	{{
	}}
}}
";
			Assert.True(RunGenerator(input, 1).Compare(expected));
		}
	}
}