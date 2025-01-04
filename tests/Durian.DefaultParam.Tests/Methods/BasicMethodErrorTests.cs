// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Generator;
using Durian.TestServices;
using Xunit;

namespace Durian.Analysis.DefaultParam.Tests.Methods
{
	public sealed class BasicMethodErrorTests : DefaultParamGeneratorTest
	{
		[Fact]
		public void Error_When_ContainingTypeIsDefaultParam()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
{{
	void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>()
	{{
	}}
}}
";

			Assert.True(RunGenerator(input, 1).FailedAndContainsDiagnostics(DefaultParamDiagnostics.DUR0126_DefaultParamMembersCannotBeNested.Id));
		}

		[Fact]
		public void Error_When_ContainingTypeIsNotPartial()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

class Test
{{
	void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>()
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DefaultParamDiagnostics.DUR0101_ContainingTypeMustBePartial.Id));
		}

		[Fact]
		public void Error_When_DefaultParamArgumentIsInvalidForConstraint()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>() where T : class
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DefaultParamDiagnostics.DUR0106_TargetTypeDoesNotSatisfyConstraint.Id));
		}

		[Fact]
		public void Error_When_DefaultParamAttributeIsNotLast()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(string)]T, U>()
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DefaultParamDiagnostics.DUR0105_DefaultParamMustBeLast.Id));
		}

		[Fact]
		public void Error_When_HasDurianGeneratedAttribute()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{DurianStrings.GeneratorNamespace}.{nameof(DurianGeneratedAttribute)}]
	void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>()
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DefaultParamDiagnostics.DUR0104_DefaultParamCannotBeAppliedWhenGenerationAttributesArePresent.Id));
		}

		[Fact]
		public void Error_When_HasGeneratedCodeAttribute()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	[System.CodeDom.Compiler.GeneratedCode("", "")]
	void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>()
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DefaultParamDiagnostics.DUR0104_DefaultParamCannotBeAppliedWhenGenerationAttributesArePresent.Id));
		}

		[Fact]
		public void Error_When_HasTwoDefaultParam_And_FirstIsConstraintOfSecond_And_SecondDoesNotInheritOrImplementFirst()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using System.Collections;

partial class Test
{{
	void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(IEnumerable))]T, [{DefaultParamAttributeProvider.TypeName}(typeof(ICollection))]U>(T value) where T : U
	{{
		T t = default;
	}}
}}
";

			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DefaultParamDiagnostics.DUR0106_TargetTypeDoesNotSatisfyConstraint.Id));
		}

		[Fact]
		public void Error_When_IsExplicitlyDeclaredInterfaceMethod()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

interface ITest
{{
	void Method<T>();
}}

partial class Test : ITest
{{
	void ITest.Method<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>()
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DefaultParamDiagnostics.DUR0103_DefaultParamIsNotValidOnThisTypeOfMethod.Id));
		}

		[Fact]
		public void Error_When_IsExtern()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	public static extern void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>();
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DefaultParamDiagnostics.DUR0102_MethodCannotBePartialOrExtern.Id));
		}

		[Fact]
		public void Error_When_IsInterfaceMethod()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial interface ITest
{{
	void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>();
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DefaultParamDiagnostics.DUR0103_DefaultParamIsNotValidOnThisTypeOfMethod.Id));
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
		void Local<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>(T value)
		{{
		}}
	}}
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DefaultParamDiagnostics.DUR0103_DefaultParamIsNotValidOnThisTypeOfMethod.Id));
		}

		[Fact]
		public void Error_When_IsPartial_And_HasImplementation_And_AttributeIsDeclaredOnDefinitionPart()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	partial void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>();
}}

partial class Test
{{
	partial void Method<T>()
	{{
	}}
}}
";

			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DefaultParamDiagnostics.DUR0102_MethodCannotBePartialOrExtern.Id));
		}

		[Fact]
		public void Error_When_IsPartial_And_HasImplementation_And_AttributeIsDeclaredOnImplementationPart()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	partial void Method<T>();
}}

partial class Test
{{
	partial void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>()
	{{
	}}
}}
";

			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DefaultParamDiagnostics.DUR0102_MethodCannotBePartialOrExtern.Id));
		}

		[Fact]
		public void Error_When_IsPartial_And_HasNoImplementation()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	partial void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>();
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DefaultParamDiagnostics.DUR0102_MethodCannotBePartialOrExtern.Id));
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
		void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>()
		{{
		}}
	}}
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DefaultParamDiagnostics.DUR0101_ContainingTypeMustBePartial.Id));
		}

		[Fact]
		public void Error_When_OneOfDefaultParamArgumentsIsInvalidForConstraint()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T, [{DefaultParamAttributeProvider.TypeName}(typeof(string))]>() where T : class where U : class
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DefaultParamDiagnostics.DUR0106_TargetTypeDoesNotSatisfyConstraint.Id));
		}

		[Fact]
		public void Error_When_OneOfMultipleDefaultParamAttributeIsNotLast()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T, U, [{DefaultParamAttributeProvider.TypeName}(typeof(int))]V>()
	{{
	}}
}}
";

			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DefaultParamDiagnostics.DUR0105_DefaultParamMustBeLast.Id));
		}
	}
}
