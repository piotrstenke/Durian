using Durian.Generator;
using Durian.Generator.DefaultParam;
using Xunit;

namespace Durian.Tests.DefaultParam.Methods
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
	public static extern void Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T>();
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
	partial void Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T>();
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
	partial void Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T>();
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
	partial void Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T>()
	{{
	}}
}}
";

			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0102_MethodCannotBePartialOrExtern.Id));
		}

		[Fact]
		public void Error_When_IsInterfaceMethod()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial interface ITest
{{
	void Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T>();
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0103_DefaultParamIsNotOnThisTypeOfMethod.Id));
		}

		[Fact]
		public void Error_When_IsExplicitlyDeclaratedInterfaceMethod()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

interface ITest
{{
	void Method<T>();
}}

partial class Test : ITest
{{
	void ITest.Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T>()
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0103_DefaultParamIsNotOnThisTypeOfMethod.Id));
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
		void Local<[{nameof(DefaultParamAttribute)}(typeof(string))]T>(T value)
		{{
		}}
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0103_DefaultParamIsNotOnThisTypeOfMethod.Id));
		}

		[Fact]
		public void Error_When_DefaultParamAttributeIsNotLast()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	void Method<[{nameof(DefaultParamAttribute)}(typeof(string)]T, U>()
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
	void Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T, U, [{nameof(DefaultParamAttribute)}(typeof(int))]V>()
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
	void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>()
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
	[{DurianStrings.GeneratorNamespace}.{nameof(DurianGeneratedAttribute)}]
	void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>()
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
	void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>()
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
		void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>()
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
	void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>() where T : class
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
	void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T, [{nameof(DefaultParamAttribute)}(typeof(string))]>() where T : class where U : class
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0106_TargetTypeDoesNotSatisfyConstraint.Id));
		}

		[Fact]
		public void Error_When_HasTwoDefaultParam_And_FirstIsConstraintOfSecond_And_SecondDoesNotInheritOrImplementFirst()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using System.Collections;

partial class Test
{{
	void Method<[{nameof(DefaultParamAttribute)}(typeof(IEnumerable))]T, [{nameof(DefaultParamAttribute)}(typeof(ICollection))]U>(T value) where T : U
	{{
		T t = default;
	}}
}}
";

			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0106_TargetTypeDoesNotSatisfyConstraint.Id));
		}

		[Fact]
		public void Error_When_ContainingTypeIsDefaultParam()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
{{
	void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>()
	{{
	}}
}}
";

			Assert.True(RunGenerator(input, 1).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0126_DefaultParamMembersCannotBeNested.Id));
		}
	}
}