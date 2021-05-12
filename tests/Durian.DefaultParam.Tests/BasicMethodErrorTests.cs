using Durian.DefaultParam;
using Xunit;

namespace Durian.Tests.DefaultParam
{
	public sealed class BasicMethodErrorTests : DefaultParamGeneratorTest
	{
		[Fact]
		public void Error_When_IsExtern()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test 
{{
	public static extern void Method<[{DefaultParamAttribute.AttributeName}(typeof(string))]T>();
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0102_MethodCannotBePartialOrExtern.Id));
		}

		[Fact]
		public void Error_When_IsPartial_And_HasNoImplementation()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test 
{{
	partial void Method<[{DefaultParamAttribute.AttributeName}(typeof(string))]T>();
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0102_MethodCannotBePartialOrExtern.Id));
		}

		[Fact]
		public void Error_When_IsPartial_And_HasImplementation_And_AttributeIsDeclaradOnDefinitionPart()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	partial void Method<[{DefaultParamAttribute.AttributeName}(typeof(string))]T>();
}}

partial class Test
{{
	partial void Method<T>()
	{{
	}}
}}
";

			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0102_MethodCannotBePartialOrExtern.Id));
		}

		[Fact]
		public void Error_When_IsPartial_And_HasImplementation_And_AttributeIsDeclaradOnImplementationPart()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	partial void Method<T>();
}}

partial class Test
{{
	partial void Method<[{DefaultParamAttribute.AttributeName}(typeof(string))]T>()
	{{
	}}
}}
";

			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0102_MethodCannotBePartialOrExtern.Id));
		}

		[Fact]
		public void Error_When_IsLocalFunction()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	void Method()
	{{
		void Local<[{DefaultParamAttribute.AttributeName}(typeof(string))]T>(T value)
		{{
		}}
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0103_DefaultParamIsNotValidOnLocalFunctionsOrLambdas.Id));
		}

		[Fact]
		public void Error_When_DefaultParamAttributeIsNotLast()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	void Method<[{DefaultParamAttribute.AttributeName}(typeof(string)]T, U>()
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0105_DefaultParamMustBeLast.Id));
		}

		[Fact]
		public void Error_When_OneOfMultipleDefaultParamAttributeIsNotLast()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	void Method<[{DefaultParamAttribute.AttributeName}(typeof(string))]T, U, [{DefaultParamAttribute.AttributeName}(typeof(int))]V>()
	{{
	}}
}}
";

			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0105_DefaultParamMustBeLast.Id));
		}

		[Fact]
		public void Error_When_HasGeneratedCodeAttribute()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	[System.CodeDom.Compiler.GeneratedCode("", "")]
	void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T>()
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0104_DefaultParamCannotBeAppliedWhenGenerationAttributesArePresent.Id));
		}

		[Fact]
		public void Error_When_HasDurianGeneratedAttribute()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{DurianStrings.GeneratorNamespace}.DurianGenerated]
	void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T>()
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0104_DefaultParamCannotBeAppliedWhenGenerationAttributesArePresent.Id));
		}

		[Fact]
		public void Error_When_ContainingTypeIsNotPartial()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

class Test
{{
	void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T>()
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0101_ContainingTypeMustBePartial.Id));
		}

		[Fact]
		public void Error_When_OneOfContainingTypesIsNotPartial()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

class Parent
{{
	partial class Test
	{{
		void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T>()
		{{
		}}
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0101_ContainingTypeMustBePartial.Id));
		}

		[Fact]
		public void Error_When_DefaultParamArgumentIsInvalidForConstraint()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T>() where T : class
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0106_TargetTypeDoesNotSatisfyConstraint.Id));
		}

		[Fact]
		public void Error_When_OneOfDefaultParamArgumentsIsInvalidForConstraint()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T, [{DefaultParamAttribute.AttributeName}(typeof(string))]>() where T : class where U : class
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0106_TargetTypeDoesNotSatisfyConstraint.Id));
		}
	}
}