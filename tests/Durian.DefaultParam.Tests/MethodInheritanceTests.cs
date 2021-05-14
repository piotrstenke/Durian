using Durian.Generator;
using Durian.Generator.DefaultParam;
using Xunit;

namespace Durian.Tests.DefaultParam
{
	public sealed class MethodInheritanceTests : DefaultParamGeneratorTest
	{
		[Fact]
		public void Error_When_InheritsGeneratedGenericMethod()
		{
			string input =
@$"partial class Parent
{{	
	{GetCodeGenerationAttributes("Parent.Method<T, U>(T)")}
	public virtual void Method<T>(T value)
	{{
	}}
}}

partial class Child : Parent
{{
	public override void Method<T>(T value)
	{{
	}}
}}
";

			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0107_DoNotOverrideGeneratedMethods.Id));
		}

		[Fact]
		public void Error_When_InheritsGeneratedNonGenericMethod()
		{
			string input =
@$"partial class Parent
{{	
	{GetCodeGenerationAttributes("Parent.Method<T, U>(T)")}
	public virtual void Method(string value)
	{{
	}}
}}

partial class Child : Parent
{{
	public override void Method(string value)
	{{
	}}
}}
";

			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0107_DoNotOverrideGeneratedMethods.Id));
		}

		[Fact]
		public void GeneratesOverridesForVirtualDefaultParamMethod_When_HasNoBaseAttribute()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{	
	public virtual void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T, [{nameof(DefaultParamAttribute)}(typeof(string))]U>(T value)
	{{
	}}
}}

partial class Child : Parent
{{
	public override void Method<T, U>(T value)
	{{
	}}
}}
";

			string expected =
@$"partial class Child
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

		[Fact]
		public void GeneratesOverridesForVirtualDefaultParamMethod_When_HasBaseAttribute()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{	
	public virtual void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T, [{nameof(DefaultParamAttribute)}(typeof(string))]U>(T value)
	{{
	}}
}}

partial class Child : Parent
{{
	public override void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T, [{nameof(DefaultParamAttribute)}(typeof(string))]U>(T value)
	{{
	}}
}}
";

			string expected =
@$"partial class Child
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

		[Fact]
		public void Warning_When_HasNoAttributeOfBaseMethod()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{	
	public virtual void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>(T value)
	{{
	}}
}}

partial class Child : Parent
{{
	public override void Method<T>(T value)
	{{
	}}
}}
";

			Assert.True(RunGenerator(input, 1).HasSucceededAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0110_OverriddenDefaultParamAttribuetShouldBeAddedForClarity.Id));
		}

		[Fact]
		public void Warning_When_HasNoAttributeOfBaseMethod_And_HasMultipleDefaultParams_And_LastIsMissing()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{	
	public virtual void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T, [{nameof(DefaultParamAttribute)}(typeof(string))]U>(T value)
	{{
	}}
}}

partial class Child : Parent
{{
	public override void Method<[[{nameof(DefaultParamAttribute)}(typeof(int))]T, U>(T value)
	{{
	}}
}}
";

			Assert.True(RunGenerator(input, 1).HasSucceededAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0110_OverriddenDefaultParamAttribuetShouldBeAddedForClarity.Id));
		}

		[Fact]
		public void Error_When_HasAttributeWithDifferentValueThanBase()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{	
	public virtual void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>(T value)
	{{
	}}
}}

partial class Child : Parent
{{
	public override void Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T>(T value)
	{{
	}}
}}
";

			Assert.True(RunGenerator(input, 1).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0108_ValueOfOverriddenMethodMustBeTheSameAsBase.Id));
		}

		[Fact]
		public void Error_When_AddedAttributeOnNonDefautParamParameter()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{	
	public virtual void Method<T, [{nameof(DefaultParamAttribute)}(typeof(int))]U>(U value)
	{{
	}}
}}

partial class Child : Parent
{{
	public override void Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T, [{nameof(DefaultParamAttribute)}(typeof(int))]U>(U value)
	{{
	}}
}}
";

			Assert.True(RunGenerator(input, 1).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0109_DoNotAddDefaultParamAttributeOnOverridenParameters.Id));
		}

		[Fact]
		public void GeneratesWithNew_When_ParentClassHasTheSameMethod_AndThisMethodHasNewModifier()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{	
	public void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>(T value)
	{{
	}}
}}

partial class Child : Parent
{{
	public new void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>(T value)
	{{
	}}
}}
";
			string expected =
@$"partial class Child
{{
	{GetCodeGenerationAttributes("Child.Method<T>(T)")}
	public new void Method(int value)
	{{
	}}
}}
";

			Assert.True(RunGenerator(input, 1).Compare(expected));
		}
	}
}