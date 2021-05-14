﻿using Durian.Configuration;
using Durian.Generator;
using Durian.Generator.DefaultParam;
using Xunit;

namespace Durian.Tests.DefaultParam
{
	public sealed class MethodNewModifierConfigurationTests : DefaultParamGeneratorTest
	{
		[Fact]
		public void AppliesNewModifier_When_SignatureExistsInBaseClass()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace}

[assembly: {nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]

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

[assembly: {nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]

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

[assembly: {nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]

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

[assembly: {nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]

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

[assembly: {nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]

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

[assembly: {nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]

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
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0118_MethodWithSignatureAlreadyExists.Id));
		}
	}
}