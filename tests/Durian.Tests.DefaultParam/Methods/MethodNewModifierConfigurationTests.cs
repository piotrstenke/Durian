﻿using Durian.Configuration;
using Durian.Generator;
using Durian.Generator.DefaultParam;
using Xunit;

namespace Durian.Tests.DefaultParam.Methods
{
	public sealed class MethodNewModifierConfigurationTests : DefaultParamGeneratorTest
	{
		[Fact]
		public void AppliesNewModifier_When_GloballyFalse_And_InTypeTrue()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace}

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = false)]

class Parent
{{
	public void Method(int value)
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
		Method<int>(value);
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
	public void Method(int value)
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
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0114_MethodWithSignatureAlreadyExists.Id));
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
	public void Method(int value)
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
		Method<int>(value);
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
	public void Method(int value)
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
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0114_MethodWithSignatureAlreadyExists.Id));
		}

		[Fact]
		public void AppliesNewModifier_When_InTypeFalse_LocallyTrue()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace}

class Parent
{{
	public void Method(int value)
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
		Method<int>(value);
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
	public void Method(int value)
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
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0114_MethodWithSignatureAlreadyExists.Id));
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
	public void Method(int value)
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
		Method<int>(value);
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
	public void Method<T>(int value)
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
		Method<T, int>(value);
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
	public void Method<T>(int value)
	{{
	}}

	public void Method<T, U>(int value)
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
		Method<T, U, int>(value);
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
	public void Method<T>(int value)
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
		Method<T, U, string>(value);
	}}

	{GetCodeGenerationAttributes("Test.Method<T, U, V>(U)")}
	new void Method<T>(int value)
	{{
		Method<T, int, string>(value);
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
	public void Method<T>(int value)
	{{
	}}

	public void Method<T, U>(int value)
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
		Method<T, U, int>(value);
	}}

	{GetCodeGenerationAttributes("Test.Method<T, U, V>(V)")}
	new void Method<T>(int value)
	{{
		Method<T, string, int>(value);
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

		[Fact]
		public void Error_When_SignatureExistsInBaseClass_And_ApplyNewModifierIsFalse()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = false)]

class Parent
{{
	public void Method(string value)
	{{
	}}
}}

partial class Test : Parent
{{
	void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>(string value)
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0114_MethodWithSignatureAlreadyExists.Id));
		}

		[Fact]
		public void AppliesNewModifier_When_NonGenericCompatibleMemberIsPresentInBaseClass()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace}

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]

class Parent
{{
	public string Method {{ get; }}
}}

partial class Test : Parent
{{
	void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>()
	{{
	}}
}}
";

			string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T>()")}
	new void Method()
	{{
		Method<int>();
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void AppliesNewModifier_When_GenericMemberOtherThanMethodIsPresentInBaseClass()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace}

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]

class Parent
{{
	public delegate void Method();
}}

partial class Test : Parent
{{
	void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>()
	{{
	}}
}}
";

			string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T>()")}
	new void Method()
	{{
		Method<int>();
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Error_When_NonGenericCompatibleMemberIsPresentInBaseClass_And_ConfigurationIsFalse()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace}

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = false)]

class Parent
{{
	public string Method {{ get; }}
}}

partial class Test : Parent
{{
	void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>()
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0116_MemberWithNameAlreadyExists.Id));
		}

		[Fact]
		public void Error_When_GenericMemberOtherThanMethodIsPresentInBaseClass_And_ConfigurationIsFalse()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace}

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = false)]

class Parent
{{
	public delegate void Method();
}}

partial class Test : Parent
{{
	void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>()
	{{
	}}
}}
";

			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0116_MemberWithNameAlreadyExists.Id));
		}

		[Fact]
		public void IgnoresPrivateMembersInBaseType()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace}

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]

class Parent
{{
	private void Method()
	{{
	}}
}}

partial class Test : Parent
{{
	void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>()
	{{
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
	}
}