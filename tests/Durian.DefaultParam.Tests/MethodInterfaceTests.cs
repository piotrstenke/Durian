using Durian.Configuration;
using Durian.Generator;
using Xunit;
using static Durian.Generator.DefaultParam.DefaultParamDiagnostics;

namespace Durian.Tests.DefaultParam
{
	public sealed class MethodInterfaceTests : DefaultParamGeneratorTest
	{
		[Fact]
		public void Generates_When_IsMethodInInterface()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial interface ITest
{{
	void Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T>();
}}
";

			string expected =
$@"partial interface ITest
{{
	{GetCodeGenerationAttributes("ITest.Method<T>()")}
	void Method();
}}";

			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Generates_When_IsExplicitImplementation()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial interface ITest
{{
	void Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T>();
}}

partial class Test : ITest
{{
	void ITest.Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T>()
	{{
		const int a = 2;
	}}
}}
";

			string expected =
$@"partial class Test
{{
	{GetCodeGenerationAttributes("Test.ITest.Method<T>()")}
	void ITest.Method()
	{{
		const int a = 2;
	}}
}}";

			Assert.True(RunGenerator(input, 1).Compare(expected));
		}

		[Fact]
		public void IgnoresConventionConfiguration_When_IsExplicitImplementation()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.MethodConvention)} = {nameof(DPMethodConvention)}.{nameof(DPMethodConvention.Call)})]

partial interface ITest
{{
	void Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T>();
}}

partial class Test : ITest
{{
	void ITest.Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T>()
	{{
		const int a = 2;
	}}
}}
";

			string expected =
$@"partial class Test
{{
	{GetCodeGenerationAttributes("Test.ITest.Method<T>()")}
	void ITest.Method()
	{{
		const int a = 2;
	}}
}}";

			Assert.True(RunGenerator(input, 1).Compare(expected));
		}

		[Fact]
		public void IgnoresNewModifierConfiguration_When_IsExplicitImplementation()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.ApplyNewModifierWhenPossible)} = true)]

partial interface ITest
{{
	void Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T>();
}}

partial class Test : ITest
{{
	void ITest.Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T>()
	{{
		const int a = 2;
	}}
}}
";

			string expected =
$@"partial class Test
{{
	{GetCodeGenerationAttributes("Test.ITest.Method<T>()")}
	void ITest.Method()
	{{
		const int a = 2;
	}}
}}";

			Assert.True(RunGenerator(input, 1).Compare(expected));
		}

		[Fact]
		public void Warning_When_HasNoDefaultParamOnExplicitMethod()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial interface ITest
{{
	void Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T>();
}}

partial class Test : ITest
{{
	void ITest.Method<T>()
	{{
		const int a = 2;
	}}
}}
";
			Assert.True(RunGenerator(input, 1).HasSucceededAndContainsDiagnosticIDs(DUR0110_OverriddenDefaultParamAttribuetShouldBeAddedForClarity.Id));
		}

		[Fact]
		public void Warning_When_MissesSomeDefaultParamsOnExplicitMethod()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial interface ITest
{{
	void Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T, [{nameof(DefaultParamAttribute)}(typeof(int))]U>();
}}

partial class Test : ITest
{{
	void ITest.Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T, U>()
	{{
		const int a = 2;
	}}
}}
";
			Assert.True(RunGenerator(input, 1).HasSucceededAndContainsDiagnosticIDs(DUR0110_OverriddenDefaultParamAttribuetShouldBeAddedForClarity.Id));
		}

		[Fact]
		public void Error_ChangedDefaultParamValueInExplicitMethod()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial interface ITest
{{
	void Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T>();
}}

partial class Test : ITest
{{
	void ITest.Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>()
	{{
		const int a = 2;
	}}
}}
";
			Assert.True(RunGenerator(input, 1).HasFailedAndContainsDiagnosticIDs(DUR0116_DoNotChangeDefaultParamValueOfImplementedMethod.Id));
		}

		[Fact]
		public void Error_When_ExpliclyImplementsGeneratedInterfaceMethod()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial interface ITest
{{
	void Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T>();
}}

partial class Test : ITest
{{
	void ITest.Method()
	{{
	}}
}}
";
			Assert.True(RunGenerator(input, 1).HasFailedAndContainsDiagnosticIDs(DUR0117_DoNotImplementGeneratedInterfaceMethods.Id));
		}

		[Fact]
		public void Error_When_AddsDefaultParamOnNonDefaultParamTypeParametersOfExpliclyImplementedInterfaceMethod()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial interface ITest
{{
	void Method<T, [{nameof(DefaultParamAttribute)}(typeof(int))]U>();
}}

partial class Test : ITest
{{
	void ITest.Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T, [{nameof(DefaultParamAttribute)}(typeof(int))]U>()
	{{
		const int a = 2;
	}}
}}
";
			Assert.True(RunGenerator(input, 1).HasFailedAndContainsDiagnosticIDs(DUR0109_DoNotAddDefaultParamAttributeOnOverridenParameters.Id));
		}

		[Fact]
		public void Error_When_ImplictlyImplementedMethodHasDifferentValue()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial interface ITest
{{
	void Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T>();
}}

partial class Test : ITest
{{
	public void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>()
	{{
	}}
}}
";
			Assert.True(RunGenerator(input, 1).HasFailedAndContainsDiagnosticIDs(DUR0118_ConflictBetweenExistingMethodAndInterfaceMethod.Id));
		}

		[Fact]
		public void Error_When_ImplictlyImplementedMethodFromBaseInterfaceHasDifferentValue()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial interface ITest
{{
	void Method<[{nameof(DefaultParamAttribute)}(typeof(string))]T>();
}}

partial interface IBetterTest : ITest
{{
	string Name {{ get; }}
}}

partial class Test : IBetterTest
{{
	public string Name {{ get; }}

	public void Method<[{nameof(DefaultParamAttribute)}(typeof(int))]T>()
	{{
	}}
}}
";
			Assert.True(RunGenerator(input, 1).HasFailedAndContainsDiagnosticIDs(DUR0118_ConflictBetweenExistingMethodAndInterfaceMethod.Id));
		}
	}
}