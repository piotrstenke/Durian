using Durian.Configuration;
using Durian.Generator;
using Durian.Generator.DefaultParam;
using Xunit;

namespace Durian.Tests.DefaultParam
{
	public sealed class MethodNewModifierConfigurationTests : DefaultParamGeneratorTest
	{
		[Fact]
		public void AppliesNewModifier_When_IsInterfaceMethod_And_GeneratedMethodExists()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]

interface IParent
{{
	void Method();
}}

partial interface IChild : IParent
{{
	void Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T>();
}}
";
			string expected =
$@"partial interface IChild
{{
	{GetCodeGenerationAttributes("IChild.Method<T>()")}
	new void Method();
}}";

			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void AppliesNewModifier_When_GloballyFalse_And_InTypeTrue()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace}

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = false)]

class Parent
{{
	void Method(int value)
	{{
	}}
}}

[{nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]
partial class Test : Parent
{{
	void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>(T value)
	{{
	}}
}}
";
			string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T>(T)")}
	new void Method(int value)
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void DoesNotApplyNewModifer_When_GloballyTrue_And_InTypeFalse()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace}

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]

class Parent
{{
	void Method(int value)
	{{
	}}
}}

[{nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = false)]
partial class Test : Parent
{{
	void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>(T value)
	{{
	}}
}}
";
			Assert.False(RunGenerator(input).IsGenerated);
		}

		[Fact]
		public void AppliesNewModifier_When_GloballyFalse_And_LocallyFalse()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace}

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = false)]

class Parent
{{
	void Method(int value)
	{{
	}}
}}

partial class Test : Parent
{{
	[{nameof(DefaultParamConfiguration)}({nameof(DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]
	void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>(T value)
	{{
	}}
}}
";
			string expected =
@$"using {DurianStrings.ConfigurationNamespace};

partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T>(T)")}
	new void Method(int value)
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void DoesNotApplyNewModifer_When_GloballyTrue_And_LocallyFalse()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace}

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]

class Parent
{{
	void Method(int value)
	{{
	}}
}}

partial class Test : Parent
{{
	[{nameof(DefaultParamConfiguration)}({nameof(DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossible)} = false)]
	void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>(T value)
	{{
	}}
}}
";
			Assert.False(RunGenerator(input).IsGenerated);
		}

		[Fact]
		public void AppliesNewModifier_When_InTypeFalse_LocallyTrue()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace}

class Parent
{{
	void Method(int value)
	{{
	}}
}}

[{nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]
partial class Test : Parent
{{
	[{nameof(DefaultParamConfiguration)}({nameof(DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]
	void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>(T value)
	{{
	}}
}}
";
			string expected =
@$"using {DurianStrings.ConfigurationNamespace};

partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T>(T)")}
	new void Method(int value)
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void DoesNotApplyNewModifer_When_InTypeTrue_LocallyFalse()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace}

class Parent
{{
	void Method(int value)
	{{
	}}
}}

[{nameof(DefaultParamConfiguration)}({nameof(DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]
partial class Test : Parent
{{
	[{nameof(DefaultParamConfiguration)}({nameof(DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossible)} = false)]
	void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>(T value)
	{{
	}}
}}
";
			Assert.False(RunGenerator(input).IsGenerated);
		}

		[Fact]
		public void AppliesNewModifier_When_SignatureExistsInBaseClass()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace}

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]

class Parent
{{
	void Method(int value)
	{{
	}}
}}

partial class Test : Parent
{{
	void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>(T value)
	{{
	}}
}}
";

			string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T>(T)")}
	new void Method(int value)
	{{

	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void AppliesNewModifier_When_HasNonDefaultParamParameters()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace}

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]

class Parent
{{
	void Method<T>(int value)
	{{
	}}
}}

partial class Test : Parent
{{
	void Method<T, [{nameof(DefaultParamAttribute)}(typeof(int))]U>(U value)
	{{
	}}
}}
";

			string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T, U>(U)")}
	new void Method<T>(int value)
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void AppliesNewModifier_When_HasMultipleNonDefaultParamParameters()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace}

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]

class Parent
{{
	void Method<T>(int value)
	{{
	}}

	void Method<T, U>(int value)
	{{
	}}
}}

partial class Test : Parent
{{
	void Method<T, U, [{nameof(DefaultParamAttribute)}(typeof(int))]V>(V value)
	{{
	}}
}}
";

			string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T, U, V>(V)")}
	new void Method<T, U>(int value)
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void AppliesNewModifierToSingleDefaultParam_When_HasMultipleDefaultParams_And_OnlyOneHasExistingSignature()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace}

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]

class Parent
{{
	void Method<T>(int value)
	{{
	}}
}}

partial class Test : Parent
{{
	void Method<T, [{nameof(DefaultParamAttribute)}(typeof(int))]U, [{nameof(DefaultParamAttribute)}(typeof(string))]V>(U value)
	{{
	}}
}}
";

			string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T, U, V>(U)")}
	void Method<T, U>(U value)
	{{
	}}

	{GetCodeGenerationAttributes("Test.Method<T, U, V>(U)")}
	new void Method<T>(int value)
	{{

	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void AppliesNewModifierToAllDefaultParam_When_HasMultipleDefaultParams_And_AllHaveExistingSignature()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace}

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]

class Parent
{{
	void Method<T>(int value)
	{{
	}}

	void Method<T, U>(int value)
	{{
	}}
}}

partial class Test : Parent
{{
	void Method<T, [{nameof(DefaultParamAttribute)}(typeof(string))]U, [{nameof(DefaultParamAttribute)}(typeof(int))]V>(V value)
	{{
	}}
}}
";

			string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T, U, V>(V)")}
	new void Method<T, U>(int value)
	{{
	}}

	{GetCodeGenerationAttributes("Test.Method<T, U, V>(V)")}
	new void Method<T>(int value)
	{{

	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Error_When_SignatureExistsInSameClass()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace}

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]

partial class Test : Parent
{{
	void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>(T value)
	{{
	}}

	void Method(int value)
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0114_MethodWithSignatureAlreadyExists.Id));
		}
	}
}