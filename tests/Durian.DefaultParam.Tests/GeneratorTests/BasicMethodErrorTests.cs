using Xunit;

namespace Durian.Tests.DefaultParam.Generator
{
	public sealed class BasicMethodErrorTests : DefaultParamGeneratorTest
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

		[Fact]
		public void MethodIsInvalid_When_DefaultParamAttributeIsNotLast()
		{
			const string input =
@"using Durian;

partial class Test
{
	void Method<[DefaultParam(typeof(string)]T, U>()
	{

	}
}
";

			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs("DUR0018"));
		}

		[Fact]
		public void MethodIsInvalid_When_OneOfMultipleDefaultParamAttributeIsNotLast()
		{
			const string input =
@"using Durian;

partial class Test
{
	void Method<[DefaultParam(typeof(string))]T, U, [DefaultParam(typeof(int))]V>()
	{

	}
}
";

			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs("DUR0018"));
		}

		[Fact]
		public void MethodIsInvalid_When_HasGeneratedCodeAttribute()
		{
			const string input =
@"using Durian;

partial class Test
{
	[System.CodeDom.Compiler.GeneratedCode("", "")]
	void Method<[DefaultParam(typeof(int))]T>()
	{

	}
}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs("DUR0017"));
		}

		[Fact]
		public void MethodIsInvalid_When_ContainingTypeIsNotPartial()
		{
			const string input =
@"using Durian;

class Test
{
	void Method<[DefaultParam(typeof(int))]T>()
	{

	}
}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs("DUR0014"));
		}

		[Fact]
		public void MethodIsInvalid_When_OneOfContainingTypesIsNotPartial()
		{
			const string input =
@"using Durian;

class Parent
{
	partial class Test
	{
		void Method<[DefaultParam(typeof(int))]T>()
		{

		}
	}
}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs("DUR0014"));
		}

		[Fact]
		public void MethodIsInvalid_When_DefaultParamArgumentIsInvalidForConstraint()
		{
			const string input =
@"using Durian;

partial class Test
{
	void Method<[DefaultParam(typeof(int))]T>() where T : class
	{

	}
}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs("DUR0019"));
		}

		[Fact]
		public void MethodIsInvalid_When_OneOfDefaultParamArgumentsIsInvalidForConstraint()
		{
			const string input =
@"using Durian;

partial class Test
{
	void Method<[DefaultParam(typeof(int))]T, [DefaultParam(typeof(string))]>() where T : class where U : class
	{

	}
}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs("DUR0019"));
		}
	}
}