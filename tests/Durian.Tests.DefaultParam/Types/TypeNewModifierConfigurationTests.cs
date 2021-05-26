using Durian.Configuration;
using Durian.Generator;
using Durian.Generator.DefaultParam;
using Xunit;
using static Durian.Generator.DefaultParam.DefaultParamDiagnostics;

namespace Durian.Tests.DefaultParam.Types
{
	public sealed class TypeNewModifierConfigurationTests : DefaultParamGeneratorTest
	{
		[Fact]
		public void Error_When_IsGlobal_AndGeneratedNonGenericNameAlreadyExists()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

class Test
{{
}}

class Test<[{nameof(DefaultParamAttribute)}(typeof(string)]T>
{{
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0116_MemberWithNameAlreadyExists.Id));
		}

		[Fact]
		public void AppliesNewModifier_When_GeneratedNonGenericNameAlreadyExistsInBaseClass()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

class Inner
{{
	public string Test {{ get; }}
}}

partial class Parent : Inner
{{
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T>
	{{
	}}
}}
";

			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	new class Test
	{{
	}}
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

class Inner
{{
	public string Test {{ get; }}
}}

partial class Parent : Inner
{{
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T>
	{{
	}}
}}
";

			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0116_MemberWithNameAlreadyExists.Id));
		}

		[Fact]
		public void Error_When_GeneratedNonGenericNameAlreadyExistsInSameClass()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]

partial class Parent
{{
	public string Test {{ get; }}

	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T>
	{{
	}}
}}
";

			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0116_MemberWithNameAlreadyExists.Id));
		}

		[Fact]
		public void AppliesNewModifier_When_GeneratedGenericNameAlreadyExistsInBaseClass()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

class Inner
{{
	public class Test<T>()
	{{
	}}
}}

partial class Parent : Inner
{{
	class Test<T, [{nameof(DefaultParamAttribute)}(typeof(int))]U>
	{{
	}}
}}
";

			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T, U>")}
	new class Test<T>
	{{
	}}
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

class Inner
{{
	public void Test<T>()
	{{
	}}
}}

partial class Parent : Inner
{{
	class Test<T, [{nameof(DefaultParamAttribute)}(typeof(int))]U>
	{{
	}}
}}
";

			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0116_MemberWithNameAlreadyExists.Id));
		}

		[Fact]
		public void Error_When_GeneratedGenericNameAlreadyExistsInSameClass_And_ConfigurationIsFalse()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]

partial class Parent
{{
	public void Test<T>()
	{{
	}}

	class Test<T, [{nameof(DefaultParamAttribute)}(typeof(int))]U>
	{{
	}}
}}
";

			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0116_MemberWithNameAlreadyExists.Id));
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
	class Test
	{{
	}}
}}

partial interface IChild : IParent
{{
	class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
	{{
	}}
}}
";
			string expected =
$@"partial interface IChild
{{
	{GetCodeGenerationAttributes("IChild.Test<T>")}
	new class Test
	{{
	}}
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

class Inner
{{
	public class Test
	{{
	}}
}}

[{nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]
partial class Parent : Inner
{{
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T>
	{{
	}}
}}
";
			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	new class Test
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

class Inner
{{
	public class Test
	{{
	}}
}}

[{nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = false)]
partial class Parent : Inner
{{
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T>
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0116_MemberWithNameAlreadyExists.Id));
		}

		[Fact]
		public void AppliesNewModifier_When_GloballyFalse_And_LocallyFalse()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace}

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = false)]

class Inner
{{
	public class Test
	{{
	}}
}}

partial class Parent : Inner
{{
	[{nameof(DefaultParamConfiguration)}({nameof(DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T>
	{{
	}}
}}
";
			string expected =
@$"using {DurianStrings.ConfigurationNamespace};

partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	new class Test
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

class Inner
{{
	public class Test
	{{
	}}
}}

partial class Parent : Inner
{{
	[{nameof(DefaultParamConfiguration)}({nameof(DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossible)} = false)]
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T>
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0116_MemberWithNameAlreadyExists.Id));
		}

		[Fact]
		public void AppliesNewModifier_When_InTypeFalse_LocallyTrue()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace}

class Inner
{{
	public class Test
	{{
	}}
}}

[{nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]
partial class Parent : Inner
{{
	[{nameof(DefaultParamConfiguration)}({nameof(DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T>
	{{
	}}
}}
";
			string expected =
@$"using {DurianStrings.ConfigurationNamespace};

partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	new class Test
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

class Inner
{{
	public class Test
	{{
	}}
}}

[{nameof(DefaultParamConfiguration)}({nameof(DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]
partial class Parent : Inner
{{
	[{nameof(DefaultParamConfiguration)}({nameof(DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossible)} = false)]
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T>
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0116_MemberWithNameAlreadyExists.Id));
		}

		[Fact]
		public void AppliesNewModifier_When_HasNonDefaultParamParameters()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace}

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]

class Inner
{{
	public class Test<T>
	{{
	}}
}}

partial class Parent : Inner
{{
	class Test<T, [{nameof(DefaultParamAttribute)}(typeof(int))]U>
	{{
	}}
}}
";

			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T, U>")}
	new class Test<T>
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

class Inner
{{
	public class Test<T>
	{{
	}}

	public class Test<T, U>
	{{
	}}
}}

partial class Parent : Inner
{{
	class Test<T, U, [{nameof(DefaultParamAttribute)}(typeof(int))]V>
	{{
	}}
}}
";

			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T, U, V>")}
	new class Test<T, U>
	{{
	}}
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
	class Test
	{{
	}}
}}

partial interface IChild : IParent
{{
	class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
	{{
	}}
}}
";

			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0116_MemberWithNameAlreadyExists.Id));
		}

		[Fact]
		public void IgnoresPrivateMembersInBaseType()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace}

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]

class Inner
{{
	private class Test
	{{
	}}
}}

partial class Parent : Inner
{{
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T>
	{{
	}}
}}
";

			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	class Test
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void AppliesNewModifierBeforeRefKeyword()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace}

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]

class Inner
{{
	public class Test
	{{
	}}
}}

partial class Parent : Inner
{{
	ref struct Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T>
	{{
	}}
}}
";

			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	new ref struct Test
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}
	}
}
