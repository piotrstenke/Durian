using Durian.Configuration;
using Durian.Generator;
using Durian.Generator.DefaultParam;
using Xunit;
using static Durian.Generator.DefaultParam.DefaultParamDiagnostics;

namespace Durian.Tests.DefaultParam.Delegates
{
	public sealed class DelegateNewModifierConfigurationTests : DefaultParamGeneratorTest
	{
		[Fact]
		public void Error_When_IsGlobal_AndGeneratedNonGenericNameAlreadyExists()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

class Del
{{
}}

delegate void Del<[{nameof(DefaultParamAttribute)}(typeof(string)]T>(T value);
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0120_MemberWithNameAlreadyExists.Id));
		}

		[Fact]
		public void AppliesNewModifier_When_GeneratedNonGenericNameAlreadyExistsInBaseClass()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

class Parent
{{
	public string Del {{ get; }}
}}

partial class Test : Parent
{{
	delegate void Del<[{nameof(DefaultParamAttribute)}(typeof(int))]T>(T value);
}}
";

			string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Del<T>")}
	new delegate void Del(int value);
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Error_When_GeneratedNonGenericNameAlreadyExistsInBaseClass_And_ConfigurationIsFalse()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = false)]

class Parent
{{
	public string Del {{ get; }}
}}

partial class Test : Parent
{{
	delegate void Del<[{nameof(DefaultParamAttribute)}(typeof(int))]T>(T value);
}}
";

			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0120_MemberWithNameAlreadyExists.Id));
		}

		[Fact]
		public void Error_When_GeneratedNonGenericNameAlreadyExistsInSameClass()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]

partial class Test
{{
	public string Del {{ get; }}

	delegate void Del<[{nameof(DefaultParamAttribute)}(typeof(int))]T>(T value);
}}
";

			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0120_MemberWithNameAlreadyExists.Id));
		}

		[Fact]
		public void AppliesNewModifier_When_GeneratedGenericNameAlreadyExistsInBaseClass()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

class Parent
{{
	public delegate void Del<T>()
	{{
	}}
}}

partial class Test : Parent
{{
	delegate void Del<T, [{nameof(DefaultParamAttribute)}(typeof(int))]U>(U value);
}}
";

			string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Del<T, U>")}
	new delegate void Del<T>(int value);
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Error_When_GeneratedGenericNameAlreadyExistsInBaseClass_And_ConfigurationIsFalse()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = false)]

class Parent
{{
	public void Del<T>()
	{{
	}}
}}

partial class Test : Parent
{{
	delegate void Del<T, [{nameof(DefaultParamAttribute)}(typeof(int))]U>(U value);
}}
";

			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0120_MemberWithNameAlreadyExists.Id));
		}

		[Fact]
		public void Error_When_GeneratedGenericNameAlreadyExistsInSameClass_And_ConfigurationIsFalse()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]

partial class Test
{{
	public void Del<T>()
	{{
	}}

	delegate void Del<T, [{nameof(DefaultParamAttribute)}(typeof(int))]U>(U value);
}}
";

			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0120_MemberWithNameAlreadyExists.Id));
		}

		[Fact]
		public void AppliesNewModifier_When_IsInInterface_And_GeneratedNameAlreadyExistsInBaseInterface()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]

interface IParent
{{
	delegate void Del();
}}

partial interface IChild : IParent
{{
	delegate void Del<[{nameof(DefaultParamAttribute)}(typeof(string))]T>();
}}
";
			string expected =
$@"partial interface IChild
{{
	{GetCodeGenerationAttributes("IChild.Del<T>")}
	new delegate void Del();
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
	public delegate void Del(int value);
}}

[{nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]
partial class Test : Parent
{{
	delegate void Del<[{nameof(DefaultParamAttribute)}(typeof(int))]T>(T value);
}}
";
			string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Del<T>")}
	new delegate void Del(int value);
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
	public delegate void Del(int value);
}}

[{nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = false)]
partial class Test : Parent
{{
	delegate void Del<[{nameof(DefaultParamAttribute)}(typeof(int))]T>(T value);
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0120_MemberWithNameAlreadyExists.Id));
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
	public delegate void Del(int value);
}}

partial class Test : Parent
{{
	[{nameof(DefaultParamConfiguration)}({nameof(DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]
	delegate void Del<[{nameof(DefaultParamAttribute)}(typeof(int))]T>(T value);
}}
";
			string expected =
@$"using {DurianStrings.ConfigurationNamespace};

partial class Test
{{
	{GetCodeGenerationAttributes("Test.Del<T>")}
	new delegate void Del(int value);
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
	public delegate void Del(int value);
}}

partial class Test : Parent
{{
	[{nameof(DefaultParamConfiguration)}({nameof(DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossible)} = false)]
	delegate void Del<[{nameof(DefaultParamAttribute)}(typeof(int))]T>(T value);
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0120_MemberWithNameAlreadyExists.Id));
		}

		[Fact]
		public void AppliesNewModifier_When_InTypeFalse_LocallyTrue()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace}

class Parent
{{
	public delegate void Del(int value);
}}

[{nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]
partial class Test : Parent
{{
	[{nameof(DefaultParamConfiguration)}({nameof(DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]
	delegate void Del<[{nameof(DefaultParamAttribute)}(typeof(int))]T>(T value);
}}
";
			string expected =
@$"using {DurianStrings.ConfigurationNamespace};

partial class Test
{{
	{GetCodeGenerationAttributes("Test.Del<T>")}
	new delegate void Del(int value);
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
	public delegate void Del(int value);
}}

[{nameof(DefaultParamConfiguration)}({nameof(DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]
partial class Test : Parent
{{
	[{nameof(DefaultParamConfiguration)}({nameof(DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossible)} = false)]
	delegate void Del<[{nameof(DefaultParamAttribute)}(typeof(int))]T>(T value);
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0120_MemberWithNameAlreadyExists.Id));
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
	public delegate void Del<T>(int value);
}}

partial class Test : Parent
{{
	delegate void Del<T, [{nameof(DefaultParamAttribute)}(typeof(int))]U>(U value);
}}
";

			string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Del<T, U>")}
	new delegate void Del<T>(int value);
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
	public delegate void Del<T>(int value);

	public delegate void Del<T, U>(int value);
}}

partial class Test : Parent
{{
	delegate void Del<T, U, [{nameof(DefaultParamAttribute)}(typeof(int))]V>(V value);
}}
";

			string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Del<T, U, V>")}
	new delegate void Del<T, U>(int value);
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Error_When_GeneratedMethodExistsInBaseInterface_And_ApplyNewModifierIsFalse()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = false)]

interface IParent
{{
	delegate void Del();
}}

partial interface IChild : IParent
{{
	delegate void Del<[{nameof(DefaultParamAttribute)}(typeof(string))]T>();
}}
";

			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0120_MemberWithNameAlreadyExists.Id));
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
	private delegate void Del();
}}

partial class Test : Parent
{{
	delegate void Del<[{nameof(DefaultParamAttribute)}(typeof(int))]T>();
}}
";

			string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Del<T>")}
	delegate void Del();
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}
	}
}
