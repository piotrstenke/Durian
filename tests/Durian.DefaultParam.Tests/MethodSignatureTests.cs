using Durian.Generator;
using Durian.Generator.DefaultParam;
using Xunit;

namespace Durian.Tests.DefaultParam
{
	public sealed class MethodSignatureTests : DefaultParamGeneratorTest
	{
		[Fact]
		public void Error_When_IsParameterless_And_OtherParameterlessExists()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>()
	{{
	}}

	void Method()
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0114_MethodWithSignatureAlreadyExists.Id));
		}

		[Fact]
		public void Error_When_IsParameterless_And_OtherParameterlessExistsWithOtherReturnType()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>()
	{{
	}}

	string Method()
	{{
		return null;
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0114_MethodWithSignatureAlreadyExists.Id));
		}

		[Fact]
		public void Error_When_HasOnlyNonTypeArgumentParameters_And_SignatureAlreadyExists()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>(string value)
	{{
	}}

	void Method(string value)
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0114_MethodWithSignatureAlreadyExists.Id));
		}

		[Fact]
		public void Error_When_HasOnlyTypeArgumentParameters_And_GeneratedSignatureExists()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
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
		public void Error_When_NotAllTypeParametersAreDefaultParam_And_HasDefaultParamParameters()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	void Method<T, [{nameof(DefaultParamAttribute)}(typeof(int))]U>(U value)
	{{
	}}

	void Method<T>(int value)
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0114_MethodWithSignatureAlreadyExists.Id));
		}

		[Fact]
		public void Error_When_NotAllTypeParametersAreDefaultParam_And_HasNonDefaultParamParameters()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	void Method<T, [{nameof(DefaultParamAttribute)}(typeof(int))]U>(T value)
	{{
	}}

	void Method<T>(T value)
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0114_MethodWithSignatureAlreadyExists.Id));
		}

		[Fact]
		public void IgnoresBaseMethod()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

class Parent
{{
	public virtual void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>(string value)
	{{
	}}
}}

partial class Test : Parent
{{
	public override void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>(string value)
	{{
	}}
}}
";
			Assert.True(!RunGenerator(input).ContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0114_MethodWithSignatureAlreadyExists.Id));
		}

		[Fact]
		public void IgnoresMethodWhenHasNewModifier()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

class Parent
{{
	public void Method<T>(string value)
	{{
	}}
}}

partial class Test : Parent
{{
	public new void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>(string value)
	{{
	}}
}}
";
			Assert.True(!RunGenerator(input).ContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0114_MethodWithSignatureAlreadyExists.Id));
		}

		[Fact]
		public void IgnoresMethodsGeneratedFromThisMethod()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>(string value)
	{{
	}}
}}
";
			Assert.True(!RunGenerator(input).ContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0114_MethodWithSignatureAlreadyExists.Id));
		}
	}
}