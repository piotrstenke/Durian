using Durian.DefaultParam;
using Xunit;

namespace Durian.Tests.DefaultParam.Generator
{
	public sealed class MethodInheritanceTests : DefaultParamGeneratorTest
	{
		[Fact]
		public void Error_When_InheritsGeneratedMethod()
		{
			string input =
@$"using System.CodeDom.Compiler;
using {DurianStrings.MainNamespace};

partial abstract class Parent
{{	
	[global::System.CodeDom.Compiler.GeneratedCode(""{DefaultParamGenerator.GeneratorName}"", ""{DefaultParamGenerator.Version}"")]
	public abstract void Method<T>(T value);
}}

partial class Child : Parent
{{
	public override void Method<T>(T value);
}}
";

			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs("DUR0021"));
		}

		[Fact]
		public void GeneratesOverridesForVirtualDefaultParamMethod_When_HasNoBaseAttribute()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{	
	public virtual void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T, [{DefaultParamAttribute.AttributeName}(typeof(string))]U>(T value)
	{{
	}}
}}

partial class Child : Parent
{{
	public override void Method<T, U>(T value)
	{{
	}}
}}
";

			string expected =
@$"partial class Child
{{
	{GetCodeGenerationAttributes("Child.Method<T, U>(T)")}
	public override void Method<T>(T value)
	{{
	}}

	{GetCodeGenerationAttributes("Child.Method<T, U>(T)")}
	public override void Method(int value)
	{{
	}}
}}
";
			Assert.True(RunGenerator(input, 1).Compare(expected));
		}

		[Fact]
		public void GeneratesOverridesForVirtualDefaultParamMethod_When_HasBaseAttribute()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{	
	public virtual void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T, [{DefaultParamAttribute.AttributeName}(typeof(string))]U>(T value)
	{{
	}}
}}

partial class Child : Parent
{{
	public override void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T, [{DefaultParamAttribute.AttributeName}(typeof(string))]U>(T value)
	{{
	}}
}}
";

			string expected =
@$"partial class Child
{{
	{GetCodeGenerationAttributes("Child.Method<T, U>(T)")}
	public override void Method<T>(T value)
	{{
	}}

	{GetCodeGenerationAttributes("Child.Method<T, U>(T)")}
	public override void Method(int value)
	{{
	}}
}}
";
			Assert.True(RunGenerator(input, 1).Compare(expected));
		}

		[Fact]
		public void Warning_When_HasNoAttributeOfBaseMethod()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{	
	public virtual void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T>(T value)
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

			Assert.True(RunGenerator(input, 1).HasSucceededAndContainsDiagnosticIDs("DUR0020"));
		}

		[Fact]
		public void Error_When_HasAttributeWithDifferentValueThanBase()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{	
	public virtual void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T>(T value)
	{{
	}}
}}

partial class Child : Parent
{{
	public override void Method<[{DefaultParamAttribute.AttributeName}(typeof(string))]T>(T value)
	{{
	}}
}}
";

			Assert.True(RunGenerator(input, 1).HasFailedAndContainsDiagnosticIDs("DUR0022"));
		}

		[Fact]
		public void Error_When_AddedAttributeOnNonDefautParamParameter()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{	
	public virtual void Method<T, [{DefaultParamAttribute.AttributeName}(typeof(int))]U>(U value)
	{{
	}}
}}

partial class Child : Parent
{{
	public override void Method<[{DefaultParamAttribute.AttributeName}(typeof(string))]T, [{DefaultParamAttribute.AttributeName}(typeof(int))]U>(U value)
	{{
	}}
}}
";

			Assert.True(RunGenerator(input, 1).HasFailedAndContainsDiagnosticIDs("DUR0025"));
		}

		[Fact]
		public void GeneratesWithNew_When_ParentClassHasTheSameMethod_AndThisMethodHasNewModifier()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{	
	public void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T>(T value)
	{{
	}}
}}

partial class Child : Parent
{{
	public new void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T>(T value)
	{{
	}}
}}
";
			string expected =
@$"partial class Child
{{
	{GetCodeGenerationAttributes("Child.Method<T>(T)")}
	public new void Method(int value)
	{{
	}}
}}
";

			Assert.True(RunGenerator(input, 1).Compare(expected));
		}
	}
}