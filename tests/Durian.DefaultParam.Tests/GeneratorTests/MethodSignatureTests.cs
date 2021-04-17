using Xunit;

namespace Durian.Tests.DefaultParam.Generator
{
	public sealed class MethodSignatureTests : DefaultParamGeneratorTest
	{
		[Fact]
		public void MethodIsInvalid_When_AllTypeParametersAreDefaultParam_And_IsParametereless_And_MethodWithGeneratedSignatureAlreadyExistsInTheSameClass()
		{
			const string input =
@"using Durian;

partial class Test
{
	void Method<[DefaultParam(typeof(int))]T>()
	{
	}

	void Method()
	{
	}
}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs("DUR0024"));
		}

		[Fact]
		public void MethodIsInvalid_When_MethodWithGeneratedSignatureButOtherReturnTypeAlreadyExistsInTheSameClass()
		{
			const string input =
@"using Durian;

partial class Test
{
	void Method<[DefaultParam(typeof(int))]T>()
	{
	}

	string Method()
	{
		return null;
	}
}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs("DUR0024"));
		}

		[Fact]
		public void MethodIsInvalid_When_AllTypeParametersAreDefaultParam_And_HasParametersThatAreNotTypeArguments_And_MethodWithGeneratedSignatureAlreadyExistsInTheSameClass()
		{
			const string input =
@"using Durian;

partial class Test
{
	void Method<[DefaultParam(typeof(int))]T>(string value)
	{
	}

	void Method(string value)
	{
	}
}
";
			//System.IO.File.WriteAllText("../../tests/generated.cs", RunGenerator(input).SourceText.ToString());
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs("DUR0024"));
		}

		[Fact]
		public void MethodIsInvalid_When_AllTypeParametersAreDefaultParam_And_AllParametersAreTypeArguments_And_MethodWithGeneratedSignatureAlreadyExistsInTheSameClass()
		{
			const string input =
@"using Durian;

partial class Test
{
	void Method<[DefaultParam(typeof(int))]T>(T value)
	{
	}

	void Method(int value)
	{
	}
}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs("DUR0024"));
		}

		[Fact]
		public void MethodIsInvalid_When_NotAllTypeParametersAreDefaultParam_And_MethodWithGeneratedSignatureAlreadyExistsInTheSameClass()
		{
			const string input =
@"using Durian;

partial class Test
{
	void Method<T, [DefaultParam(typeof(int))]U>(U value)
	{
	}

	void Method<T>(int value)
	{
	}
}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs("DUR0024"));
		}
	}
}