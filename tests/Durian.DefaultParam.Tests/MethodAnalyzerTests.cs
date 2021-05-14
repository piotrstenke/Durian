using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Durian.Configuration;
using Durian.Generator;
using Durian.Generator.DefaultParam;
using Microsoft.CodeAnalysis;
using Xunit;
using Desc = Durian.Generator.DefaultParam.DefaultParamDiagnostics;

namespace Durian.Tests.DefaultParam
{
	public sealed class MethodAnalyzerTests : DefaultParamAnalyzerTest<DefaultParamMethodAnalyzer>
	{
		[Fact]
		public async Task Error_When_IsExtern()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test 
{{
	public static extern void Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T>();
}}
";

			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DUR0102_MethodCannotBePartialOrExtern.Id));
		}

		[Fact]
		public async Task Error_When_IsPartial_And_HasNoImplementation()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test 
{{
	partial void Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T>();
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DUR0102_MethodCannotBePartialOrExtern.Id));
		}


		[Fact]
		public async Task Error_When_IsPartial_And_HasImplementation_And_AttributeIsDeclaradOnDefinitionPart()
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

			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DUR0102_MethodCannotBePartialOrExtern.Id));
		}

		[Fact]
		public async Task Error_When_IsPartial_And_HasImplementation_And_AttributeIsDeclaradOnImplementationPart()
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

			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DUR0102_MethodCannotBePartialOrExtern.Id));
		}

		[Fact]
		public async Task Error_When_IsLocalFunction()
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

			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DUR0103_DefaultParamIsNotValidOnLocalFunctionsOrLambdas.Id));
		}

		[Fact]
		public async Task Error_When_DefaultParamAttributeIsNotLast()
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
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DUR0105_DefaultParamMustBeLast.Id));
		}

		[Fact]
		public async Task Error_When_OneOfMultipleDefaultParamAttributeIsNotLast()
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

			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DUR0105_DefaultParamMustBeLast.Id));
		}

		// If a method has the System.CodeDom.Compiler.GeneratedCodeAttribute, it is not analyzed by design.

		//		[Fact]
		//		public async Task Error_When_HasGeneratedCodeAttribute()
		//		{
		//			string input =
		//@$"using {DurianStrings.MainNamespace};

		//		partial class Test
		//		{{
		//			[System.CodeDom.Compiler.GeneratedCode("", "")]
		//			void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>()
		//			{{
		//			}}
		//		}}
		//		";
		//			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
		//			Assert.True(diagnostics.Any(d => d.Id == Desc.DUR0104_DefaultParamCannotBeAppliedWhenGenerationAttributesArePresent.Id));
		//		}

		[Fact]
		public async Task Error_When_HasDurianGeneratedAttribute()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{DurianStrings.GeneratorNamespace}.DurianGenerated]
	void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>()
	{{
	}}
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DUR0104_DefaultParamCannotBeAppliedWhenGenerationAttributesArePresent.Id));
		}

		[Fact]
		public async Task Error_When_ContainingTypeIsNotPartial()
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
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DUR0101_ContainingTypeMustBePartial.Id));
		}

		[Fact]
		public async Task Error_When_OneOfContainingTypesIsNotPartial()
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
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DUR0101_ContainingTypeMustBePartial.Id));
		}

		[Fact]
		public async Task Error_When_DefaultParamArgumentIsInvalidForConstraint()
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
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DUR0106_TargetTypeDoesNotSatisfyConstraint.Id));
		}

		[Fact]
		public async Task Error_When_OneOfDefaultParamArgumentsIsInvalidForConstraint()
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
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DUR0106_TargetTypeDoesNotSatisfyConstraint.Id));
		}

		[Fact]
		public async Task Error_When_InheritsGeneratedGenericMethod()
		{
			string input =
@$"partial class Parent
{{	
	{DefaultParamGeneratorTest.GetCodeGenerationAttributes("Parent.Method<T, U>(T)")}
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

			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DUR0107_DoNotOverrideGeneratedMethods.Id));
		}

		[Fact]
		public async Task Error_When_InheritsGeneratedNonGenericMethod()
		{
			string input =
@$"partial class Parent
{{	
	{DefaultParamGeneratorTest.GetCodeGenerationAttributes("Parent.Method<T, U>(T)")}
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

			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DUR0107_DoNotOverrideGeneratedMethods.Id));
		}

		[Fact]
		public async Task Warning_When_HasNoAttributeOfBaseMethod()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{	
	public virtual void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>(T value)
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

			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DUR0110_OverriddenDefaultParamAttribuetShouldBeAddedForClarity.Id));
		}

		[Fact]
		public async Task Warning_When_HasNoAttributeOfBaseMethod_And_HasMultipleDefaultParams_And_LastIsMissing()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{	
	public virtual void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T, [{nameof(DefaultParamAttribute)}(typeof(string))]U>(T value)
	{{
	}}
}}

partial class Child : Parent
{{
	public override void Method<[[{nameof(DefaultParamAttribute)}(typeof(int))]T, U>(T value)
	{{
	}}
}}
";

			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DUR0110_OverriddenDefaultParamAttribuetShouldBeAddedForClarity.Id));
		}

		[Fact]
		public async Task Error_When_HasAttributeWithDifferentValueThanBase()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{	
	public virtual void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>(T value)
	{{
	}}
}}

partial class Child : Parent
{{
	public override void Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T>(T value)
	{{
	}}
}}
";

			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DUR0108_ValueOfOverriddenMethodMustBeTheSameAsBase.Id));
		}

		[Fact]
		public async Task Error_When_AddedAttributeOnNonDefautParamParameter()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{	
	public virtual void Method<T, [{nameof(DefaultParamAttribute)}(typeof(int))]U>(U value)
	{{
	}}
}}

partial class Child : Parent
{{
	public override void Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T, [{nameof(DefaultParamAttribute)}(typeof(int))]U>(U value)
	{{
	}}
}}
";

			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Id == Desc.DUR0109_DoNotAddDefaultParamAttributeOnOverridenParameters.Id));
		}

		[Fact]
		public async Task Error_When_HasNewModifierConfiguration_And_SignatureExistsInSameClass()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace}

[assembly: {nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]

partial class Test : Parent
{{
	void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>(T value)
	{{
	}}

	void Method(int value)
	{{
	}}
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Descriptor.Equals(Desc.DUR0118_MethodWithSignatureAlreadyExists)));
		}

		[Fact]
		public async Task Error_When_IsParameterless_And_OtherParameterlessExists()
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
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Descriptor.Equals(Desc.DUR0118_MethodWithSignatureAlreadyExists)));
		}

		[Fact]
		public async Task Error_When_IsParameterless_And_OtherParameterlessExistsWithOtherReturnType()
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
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Descriptor.Equals(Desc.DUR0118_MethodWithSignatureAlreadyExists)));
		}

		[Fact]
		public async Task Error_When_HasOnlyNonTypeArgumentParameters_And_SignatureAlreadyExists()
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
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Descriptor.Equals(Desc.DUR0118_MethodWithSignatureAlreadyExists)));
		}

		[Fact]
		public async Task Error_When_HasOnlyTypeArgumentParameters_And_GeneratedSignatureExists()
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
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Descriptor.Equals(Desc.DUR0118_MethodWithSignatureAlreadyExists)));
		}

		[Fact]
		public async Task Error_When_NotAllTypeParametersAreDefaultParam_And_HasDefaultParamParameters()
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
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Descriptor.Equals(Desc.DUR0118_MethodWithSignatureAlreadyExists)));
		}

		[Fact]
		public async Task Error_When_NotAllTypeParametersAreDefaultParam_And_HasNonDefaultParamParameters()
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
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Descriptor.Equals(Desc.DUR0118_MethodWithSignatureAlreadyExists)));
		}

		[Fact]
		public async Task Error_When_SignatureExistsInBaseClass()
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
	void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>(string value)
	{{
	}}
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzerAsync(input);
			Assert.True(diagnostics.Any(d => d.Descriptor.Equals(Desc.DUR0118_MethodWithSignatureAlreadyExists)));
		}
	}
}