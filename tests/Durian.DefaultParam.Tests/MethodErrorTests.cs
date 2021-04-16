using Xunit;

namespace Durian.Tests.DefaultParam
{
	public sealed class MethodErrorTests : DefaultParamTestBase
	{
		[Fact]
		public void MethodIsInvalid_When_IsExtern()
		{
			const string input =
@"using Durian;

partial class Test 
{
	public static extern void Method<[DefaultParam(typeof(string))]T>();
}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs("DUR0011"));
		}

		[Fact]
		public void MethodIsInvalid_When_IsPartial_And_HasNoImplementation()
		{
			const string input =
@"using Durian;

partial class Test 
{
	partial void Method<[DefaultParam(typeof(string))]T>();
}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs("DUR0011"));
		}

		[Fact]
		public void MethodIsInvalid_When_IsPartial_And_HasImplementation_And_AttributeIsDeclaradOnDefinitionPart()
		{
			const string input =
@"using Durian;

partial class Test
{
	partial void Method<[DefaultParam(typeof(string))]T>();
}

partial class Test
{
	partial void Method<T>()
	{
	}
}
";

			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs("DUR0011"));
		}

		[Fact]
		public void MethodIsInvalid_When_IsPartial_And_HasImplementation_And_AttributeIsDeclaradOnImplementationPart()
		{
			const string input =
@"using Durian;

partial class Test
{
	partial void Method<T>();
}

partial class Test
{
	partial void Method<[DefaultParam(typeof(string))]T>()
	{
	}
}
";

			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs("DUR0011"));
		}

		[Fact]
		public void MethodIsInvalid_When_IsLocalFunction()
		{
			const string input =
@"using Durian;

partial class Test
{
	void Method()
	{
		void Local<[DefaultParam(typeof(string))]T>(T value)
		{
		}
	}
}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs("DUR0016"));
		}
	}
}