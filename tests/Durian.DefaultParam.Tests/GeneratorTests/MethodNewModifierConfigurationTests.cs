using Xunit;
using Durian.DefaultParam;

namespace Durian.Tests.DefaultParam.Generator
{
	public sealed class MethodInheritanceConfigurationTests : DefaultParamGeneratorTest
	{

	}

	public sealed class MethodNewModifierConfigurationTests : DefaultParamGeneratorTest
	{
		[Fact]
		public void AppliesNewModifier_When_SignatureExistsInBaseClass()
		{
			string input =
@$"using Durian;
using Durian.Configuration;

[assembly: DefaultParamConfiguration({DefaultParamConfigurationAttribute.ApplyNewToGeneratedMembersWithEquivalentSignatureProperty} = true)]

class Parent
{{
	void Method(int value)
	{{
	}}
}}

partial class Test : Parent
{{
	void Method<[DefaultParam(typeof(int))]T>(T value)
	{{
	}}
}}
";

			string expected =
@$"using System.CodeDom.Compiler;

partial class Test
{{
	[GeneratedCode(""{DefaultParamGenerator.GeneratorName}"", ""{DefaultParamGenerator.Version}"")]
	new void Method(int value)
	{{

	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void AppliesNewModifier_When_HasNonDefaultParamParameters()
		{
			string input =
@$"using Durian;
using Durian.Configuration;

[assembly: DefaultParamConfiguration({DefaultParamConfigurationAttribute.ApplyNewToGeneratedMembersWithEquivalentSignatureProperty} = true)]

class Parent
{{
	void Method<T>(int value)
	{{
	}}
}}

partial class Test : Parent
{{
	void Method<T, [DefaultParam(typeof(int))]U>(U value)
	{{
	}}
}}
";

			string expected =
@$"using System.CodeDom.Compiler;

partial class Test
{{
	[GeneratedCode(""{DefaultParamGenerator.GeneratorName}"", ""{DefaultParamGenerator.Version}"")]
	new void Method<T>(int value)
	{{

	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void AppliesNewModifier_When_HasMultipleNonDefaultParamParameters()
		{
			string input =
@$"using Durian;
using Durian.Configuration;

[assembly: DefaultParamConfiguration({DefaultParamConfigurationAttribute.ApplyNewToGeneratedMembersWithEquivalentSignatureProperty} = true)]

class Parent
{{
	void Method<T>(int value)
	{{
	}}

	void Method<T, U>(int value)
	{{
	}}
}}

partial class Test : Parent
{{
	void Method<T, U, [DefaultParam(typeof(int))]V>(V value)
	{{
	}}
}}
";

			string expected =
@$"using System.CodeDom.Compiler;

partial class Test
{{
	[GeneratedCode(""{DefaultParamGenerator.GeneratorName}"", ""{DefaultParamGenerator.Version}"")]
	new void Method<T, U>(int value)
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void AppliesNewModifierToSingleDefaultParam_When_HasMultipleDefaultParams_And_OnlyOneHasExistingSignature()
		{
			string input =
@$"using Durian;
using Durian.Configuration;

[assembly: DefaultParamConfiguration({DefaultParamConfigurationAttribute.ApplyNewToGeneratedMembersWithEquivalentSignatureProperty} = true)]

class Parent
{{
	void Method<T>(int value)
	{{
	}}
}}

partial class Test : Parent
{{
	void Method<T, [DefaultParam(typeof(int))]U, [DefaultParam(typeof(string))]V>(U value)
	{{
	}}
}}
";

			string expected =
@$"using System.CodeDom.Compiler;

partial class Test
{{
	[GeneratedCode(""{DefaultParamGenerator.GeneratorName}"", ""{DefaultParamGenerator.Version}"")]
	void Method<T, U>(U value)
	{{
	}}

	[GeneratedCode(""{DefaultParamGenerator.GeneratorName}"", ""{DefaultParamGenerator.Version}"")]
	new void Method<T>(int value)
	{{

	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void AppliesNewModifierToAllDefaultParam_When_HasMultipleDefaultParams_And_AllHaveExistingSignature()
		{
			string input =
@$"using Durian;
using Durian.Configuration;

[assembly: DefaultParamConfiguration({DefaultParamConfigurationAttribute.ApplyNewToGeneratedMembersWithEquivalentSignatureProperty} = true)]

class Parent
{{
	void Method<T>(int value)
	{{
	}}

	void Method<T, U>(int value)
	{{
	}}
}}

partial class Test : Parent
{{
	void Method<T, [DefaultParam(typeof(string))]U, [DefaultParam(typeof(int))]V>(V value)
	{{
	}}
}}
";

			string expected =
@$"using System.CodeDom.Compiler;

partial class Test
{{
	[GeneratedCode(""{DefaultParamGenerator.GeneratorName}"", ""{DefaultParamGenerator.Version}"")]
	new void Method<T, U>(int value)
	{{
	}}

	[GeneratedCode(""{DefaultParamGenerator.GeneratorName}"", ""{DefaultParamGenerator.Version}"")]
	new void Method<T>(int value)
	{{

	}}
}}
";
			//System.IO.File.WriteAllText("../../tests/generated.cs", RunGenerator(input).SourceText.ToString());
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Error_When_SignatureExistsInSameClass()
		{
			string input =
$@"using Durian;
using Durian.Configuration;

[assembly: DefaultParamConfiguration({DefaultParamConfigurationAttribute.ApplyNewToGeneratedMembersWithEquivalentSignatureProperty} = true)]

partial class Test : Parent
{{
	void Method<[DefaultParam(typeof(int))]T>(T value)
	{{
	}}

	void Method(int value)
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs("DUR0024"));
		}
	}
}