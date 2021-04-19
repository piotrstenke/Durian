using Xunit;

namespace Durian.Tests.DefaultParam.Generator
{
	public sealed class MethodSignatureErrorTests : DefaultParamGeneratorTest
	{
		[Fact]
		public void Error_When_IsParameterless_And_OtherParameterlessExists()
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
		public void Error_When_IsParameterless_And_OtherParameterlessExistsWithOtherReturnType()
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
		public void Error_When_HasOnlyNonTypeArgumentParameters_And_SignatureAlreadyExists()
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
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs("DUR0024"));
		}

		[Fact]
		public void Error_When_HasOnlyTypeArgumentParameters_And_GeneratedSignatureExists()
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
		public void Error_When_NotAllTypeParametersAreDefaultParam_And_HasDefaultParamParameters()
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

		[Fact]
		public void Error_When_NotAllTypeParametersAreDefaultParam_And_HasNonDefaultParamParameters()
		{
			const string input =
@"using Durian;

partial class Test
{
	void Method<T, [DefaultParam(typeof(int))]U>(T value)
	{
	}

	void Method<T>(T value)
	{
	}
}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs("DUR0024"));
		}

		[Fact]
		public void Error_When_SignatureExistsInBaseClass()
		{
			const string input =
@"using Durian;

class Parent
{
	void Method(string value)
	{
	}
}

partial class Test : Parent
{
	void Method<[DefaultParam(typeof(int))]T>(string value)
	{
	}
}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs("DUR0024"));
		}
	}
}