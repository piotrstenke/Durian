using Xunit;
using Durian.DefaultParam;

namespace Durian.Tests.DefaultParam.Generator
{
	public sealed class MethodSignatureErrorTests : DefaultParamGeneratorTest
	{
		[Fact]
		public void Error_When_IsParameterless_And_OtherParameterlessExists()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T>()
	{{
	}}

	void Method()
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs("DUR0024"));
		}

		[Fact]
		public void Error_When_IsParameterless_And_OtherParameterlessExistsWithOtherReturnType()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T>()
	{{
	}}

	string Method()
	{{
		return null;
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs("DUR0024"));
		}

		[Fact]
		public void Error_When_HasOnlyNonTypeArgumentParameters_And_SignatureAlreadyExists()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T>(string value)
	{{
	}}

	void Method(string value)
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs("DUR0024"));
		}

		[Fact]
		public void Error_When_HasOnlyTypeArgumentParameters_And_GeneratedSignatureExists()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T>(T value)
	{{
	}}

	void Method(int value)
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs("DUR0024"));
		}

		[Fact]
		public void Error_When_NotAllTypeParametersAreDefaultParam_And_HasDefaultParamParameters()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	void Method<T, [{DefaultParamAttribute.AttributeName}(typeof(int))]U>(U value)
	{{
	}}

	void Method<T>(int value)
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs("DUR0024"));
		}

		[Fact]
		public void Error_When_NotAllTypeParametersAreDefaultParam_And_HasNonDefaultParamParameters()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	void Method<T, [{DefaultParamAttribute.AttributeName}(typeof(int))]U>(T value)
	{{
	}}

	void Method<T>(T value)
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs("DUR0024"));
		}

		[Fact]
		public void Error_When_SignatureExistsInBaseClass()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

class Parent
{{
	void Method(string value)
	{{
	}}
}}

partial class Test : Parent
{{
	void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T>(string value)
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs("DUR0024"));
		}
	}
}