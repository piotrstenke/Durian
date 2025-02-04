﻿using Durian.TestServices;
using Xunit;

namespace Durian.Analysis.DefaultParam.Tests.Methods;

public sealed class MethodInheritanceTests : DefaultParamGeneratorTest
{
	[Fact]
	public void Error_When_AddedAttributeOnNonDefaultParamParameter()
	{
		string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{
	public virtual void Method<T, [{DefaultParamAttributeProvider.TypeName}(typeof(int))]U>(U value)
	{{
	}}
}}

partial class Child : Parent
{{
	public override void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T, [{DefaultParamAttributeProvider.TypeName}(typeof(int))]U>(U value)
	{{
	}}
}}
";

		Assert.True(RunGenerator(input, 1).FailedAndContainsDiagnostics(DefaultParamDiagnostics.DUR0109_DoNotAddDefaultParamAttributeOnOverriddenParameters.Id));
	}

	[Fact]
	public void Error_When_HasAttributeWithDifferentValueThanBase()
	{
		string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{
	public virtual void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>(T value)
	{{
	}}
}}

partial class Child : Parent
{{
	public override void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>(T value)
	{{
		Method<int>(value);
	}}
}}
";

		Assert.True(RunGenerator(input, 1).FailedAndContainsDiagnostics(DefaultParamDiagnostics.DUR0108_ValueOfOverriddenMethodMustBeTheSameAsBase.Id));
	}

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

		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DefaultParamDiagnostics.DUR0107_DoNotOverrideGeneratedMethods.Id));
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

		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DefaultParamDiagnostics.DUR0107_DoNotOverrideGeneratedMethods.Id));
	}

	[Fact]
	public void Generates_When_IsAbstractOverride()
	{
		string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial class Parent
{{
	public virtual void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>()
	{{
	}}
}}

partial abstract class Test : Parent
{{
	public abstract override void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>();
}}
";

		string expected =
$@"internal abstract partial class Test : Parent
{{
	{GetCodeGenerationAttributes("Test.Method<T>()")}
	public abstract override void Method();
}}";

		Assert.True(RunGenerator(input, 1).Compare(expected));
	}

	[Fact]
	public void GeneratesOverridesForVirtualDefaultParamMethod_When_HasBaseAttribute()
	{
		string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{
	public virtual void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T, [{DefaultParamAttributeProvider.TypeName}(typeof(string))]U>(T value)
	{{
	}}
}}

partial class Child : Parent
{{
	public override void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T, [{DefaultParamAttributeProvider.TypeName}(typeof(string))]U>(T value)
	{{
	}}
}}
";

		string expected =
@$"internal partial class Child : Parent
{{
	{GetCodeGenerationAttributes("Child.Method<T, U>(T)")}
	public override void Method<T>(T value)
	{{
		Method<T, string>(value);
	}}

	{GetCodeGenerationAttributes("Child.Method<T, U>(T)")}
	public override void Method(int value)
	{{
		Method<int, string>(value);
	}}
}}
";
		Assert.True(RunGenerator(input, 1).Compare(expected));
	}

	[Fact]
	public void GeneratesOverridesForVirtualDefaultParamMethod_When_HasNoBaseAttribute()
	{
		string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{
	public virtual void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T, [{DefaultParamAttributeProvider.TypeName}(typeof(string))]U>(T value)
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
@$"internal partial class Child : Parent
{{
	{GetCodeGenerationAttributes("Child.Method<T, U>(T)")}
	public override void Method<T>(T value)
	{{
		Method<T, string>(value);
	}}

	{GetCodeGenerationAttributes("Child.Method<T, U>(T)")}
	public override void Method(int value)
	{{
		Method<int, string>(value);
	}}
}}
";
		Assert.True(RunGenerator(input, 1).Compare(expected));
	}

	[Fact]
	public void GeneratesWithNew_When_ParentClassHasTheSameMethod_AndThisMethodHasNewModifier()
	{
		string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{
	public void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>(T value)
	{{
	}}
}}

partial class Child : Parent
{{
	public new void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>(T value)
	{{
	}}
}}
";
		string expected =
@$"internal partial class Child : Parent
{{
	{GetCodeGenerationAttributes("Child.Method<T>(T)")}
	public new void Method(int value)
	{{
		Method<int>(value);
	}}
}}
";

		Assert.True(RunGenerator(input, 1).Compare(expected));
	}

	[Fact]
	public void RemovesNewModifier_WhenIsNotNecessary()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

class Parent
{{
	public void Method<T>()
	{{
	}}
}}

partial class Test : Parent
{{
	public new void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>()
	{{
	}}
}}";

		string expected =
$@"internal partial class Test : Parent
{{
	{GetCodeGenerationAttributes("Test.Method<T>()")}
	public void Method()
	{{
		Method<string>();
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Warning_When_HasNoAttributeOfBaseMethod()
	{
		string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{
	public virtual void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>(T value)
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
		string expected =
$@"internal partial class Child : Parent
{{
	{GetCodeGenerationAttributes("Child.Method<T>(T)")}
	public override void Method(int value)
	{{
		Method<int>(value);
	}}
}}
";
		SingleGeneratorTestResult result = RunGenerator(input, 1);

		Assert.True(result.SucceededAndContainsDiagnostics(DefaultParamDiagnostics.DUR0110_OverriddenDefaultParamAttributeShouldBeAddedForClarity.Id));
		Assert.True(result.Compare(expected));
	}

	[Fact]
	public void Warning_When_HasNoAttributeOfBaseMethod_And_BaseMethodAlsoDoesNot()
	{
		string input =
@$"using {DurianStrings.MainNamespace};

partial class Inner
{{
	public virtual void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>(T value)
	{{
	}}
}}

partial class Parent : Inner
{{
	public override void Method<T>(T value)
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

		string expected =
$@"internal partial class Child : Parent
{{
	{GetCodeGenerationAttributes("Child.Method<T>(T)")}
	public override void Method(int value)
	{{
		Method<int>(value);
	}}
}}
";

		SingleGeneratorTestResult result = RunGenerator(input, 2);

		Assert.True(result.SucceededAndContainsDiagnostics(DefaultParamDiagnostics.DUR0110_OverriddenDefaultParamAttributeShouldBeAddedForClarity.Id));
		Assert.True(result.Compare(expected));
	}

	[Fact]
	public void Warning_When_HasNoAttributeOfBaseMethod_And_HasMultipleDefaultParams_And_LastIsMissing()
	{
		string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{
	public virtual void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T, [{DefaultParamAttributeProvider.TypeName}(typeof(string))]U>(T value)
	{{
	}}
}}

partial class Child : Parent
{{
	public override void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T, U>(T value)
	{{
	}}
}}
";

		string expected =
$@"internal partial class Child : Parent
{{
	{GetCodeGenerationAttributes("Child.Method<T, U>(T)")}
	public override void Method<T>(T value)
	{{
		Method<T, string>(value);
	}}

	{GetCodeGenerationAttributes("Child.Method<T, U>(T)")}
	public override void Method(int value)
	{{
		Method<int, string>(value);
	}}
}}
";

		SingleGeneratorTestResult result = RunGenerator(input, 1);

		Assert.True(result.SucceededAndContainsDiagnostics(DefaultParamDiagnostics.DUR0110_OverriddenDefaultParamAttributeShouldBeAddedForClarity.Id));
		Assert.True(result.Compare(expected));
	}
}
