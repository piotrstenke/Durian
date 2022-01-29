// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.TestServices;
using Xunit;
using static Durian.Analysis.DefaultParam.DefaultParamDiagnostics;

namespace Durian.Analysis.DefaultParam.Tests.Types
{
	public sealed class TypeNewModifierConfigurationTests : DefaultParamGeneratorTest
	{
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
	class Test<T, [{DefaultParamAttributeProvider.TypeName}(typeof(int))]U>
	{{
	}}
}}
";

			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T, U>")}
	new class Test<T> : Test<T, int>
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
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
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>
	{{
	}}
}}
";

			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	new class Test : Test<int>
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void AppliesNewModifier_When_GloballyFalse_And_InTypeTrue()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace}

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.ApplyNewModifierWhenPossible} = false)]
class Inner
{{
	public class Test
	{{
	}}
}}

[{DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.ApplyNewModifierWhenPossible} = true)]
partial class Parent : Inner
{{
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>
	{{
	}}
}}
";
			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	new class Test : Test<int>
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void AppliesNewModifier_When_GloballyFalse_And_LocallyFalse()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace}

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.ApplyNewModifierWhenPossible} = false)]
class Inner
{{
	public class Test
	{{
	}}
}}

partial class Parent : Inner
{{
	[{nameof(DefaultParamConfiguration)}({DefaultParamConfigurationAttributeProvider.ApplyNewModifierWhenPossible} = true)]
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>
	{{
	}}
}}
";
			string expected =
@$"using {DurianStrings.ConfigurationNamespace};

partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	new class Test : Test<int>
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

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.ApplyNewModifierWhenPossible} = true)]
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
	class Test<T, U, [{DefaultParamAttributeProvider.TypeName}(typeof(int))]V>
	{{
	}}
}}
";

			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T, U, V>")}
	new class Test<T, U> : Test<T, U, int>
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

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.ApplyNewModifierWhenPossible} = true)]
class Inner
{{
	public class Test<T>
	{{
	}}
}}

partial class Parent : Inner
{{
	class Test<T, [{DefaultParamAttributeProvider.TypeName}(typeof(int))]U>
	{{
	}}
}}
";

			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T, U>")}
	new class Test<T> : Test<T, int>
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
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

[{DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.ApplyNewModifierWhenPossible} = true)]
partial class Parent : Inner
{{
	[{nameof(DefaultParamConfiguration)}({DefaultParamConfigurationAttributeProvider.ApplyNewModifierWhenPossible} = true)]
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>
	{{
	}}
}}
";
			string expected =
@$"using {DurianStrings.ConfigurationNamespace};

partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	new class Test : Test<int>
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void AppliesNewModifier_When_IsInInterface_And_GeneratedNameAlreadyExistsInBaseInterface()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.ApplyNewModifierWhenPossible} = true)]
interface IParent
{{
	class Test
	{{
	}}
}}

partial interface IChild : IParent
{{
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
	{{
	}}
}}
";
			string expected =
$@"partial interface IChild
{{
	{GetCodeGenerationAttributes("IChild.Test<T>")}
	new class Test : Test<string>
	{{
	}}
}}";

			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void AppliesNewModifierBeforeRefKeyword()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace}

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.ApplyNewModifierWhenPossible} = true)]
class Inner
{{
	public class Test
	{{
	}}
}}

partial class Parent : Inner
{{
	ref struct Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>
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

		[Fact]
		public void DoesNotApplyNewModifer_When_GloballyTrue_And_InTypeFalse()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace}

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.ApplyNewModifierWhenPossible} = true)]
class Inner
{{
	public class Test
	{{
	}}
}}

[{DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.ApplyNewModifierWhenPossible} = false)]
partial class Parent : Inner
{{
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T> : Test<int>
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0116_MemberWithNameAlreadyExists.Id));
		}

		[Fact]
		public void DoesNotApplyNewModifer_When_GloballyTrue_And_LocallyFalse()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace}

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.ApplyNewModifierWhenPossible} = true)]
class Inner
{{
	public class Test
	{{
	}}
}}

partial class Parent : Inner
{{
	[{nameof(DefaultParamConfiguration)}({DefaultParamConfigurationAttributeProvider.ApplyNewModifierWhenPossible} = false)]
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0116_MemberWithNameAlreadyExists.Id));
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

[{nameof(DefaultParamConfiguration)}({DefaultParamConfigurationAttributeProvider.ApplyNewModifierWhenPossible} = true)]
partial class Parent : Inner
{{
	[{nameof(DefaultParamConfiguration)}({DefaultParamConfigurationAttributeProvider.ApplyNewModifierWhenPossible} = false)]
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0116_MemberWithNameAlreadyExists.Id));
		}

		[Fact]
		public void Error_When_GeneratedGenericNameAlreadyExistsInBaseClass_And_ConfigurationIsFalse()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.ApplyNewModifierWhenPossible} = false)]
class Inner
{{
	public void Test<T>()
	{{
	}}
}}

partial class Parent : Inner
{{
	class Test<T, [{DefaultParamAttributeProvider.TypeName}(typeof(int))]U>
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

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.ApplyNewModifierWhenPossible} = true)]
partial class Parent
{{
	public void Test<T>()
	{{
	}}

	class Test<T, [{DefaultParamAttributeProvider.TypeName}(typeof(int))]U>
	{{
	}}
}}
";

			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0116_MemberWithNameAlreadyExists.Id));
		}

		[Fact]
		public void Error_When_GeneratedMethodExistsInBaseInterface_And_ApplyNewModifierIsFalse()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.ApplyNewModifierWhenPossible} = false)]
interface IParent
{{
	class Test
	{{
	}}
}}

partial interface IChild : IParent
{{
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
	{{
	}}
}}
";

			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0116_MemberWithNameAlreadyExists.Id));
		}

		[Fact]
		public void Error_When_GeneratedNonGenericNameAlreadyExistsInBaseClass_And_ConfigurationIsFalse()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.ApplyNewModifierWhenPossible} = false)]
class Inner
{{
	public string Test {{ get; }}
}}

partial class Parent : Inner
{{
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>
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

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.ApplyNewModifierWhenPossible} = true)]
partial class Parent
{{
	public string Test {{ get; }}

	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>
	{{
	}}
}}
";

			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0116_MemberWithNameAlreadyExists.Id));
		}

		[Fact]
		public void Error_When_IsGlobal_AndGeneratedNonGenericNameAlreadyExists()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

class Test
{{
}}

class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string)]T>
{{
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0129_TargetNamespaceAlreadyContainsMemberWithName.Id));
		}

		[Fact]
		public void IgnoresPrivateMembersInBaseType()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace}

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.ApplyNewModifierWhenPossible} = true)]
class Inner
{{
	private class Test
	{{
	}}
}}

partial class Parent : Inner
{{
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>
	{{
	}}
}}
";

			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	class Test : Test<int>
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}
	}
}