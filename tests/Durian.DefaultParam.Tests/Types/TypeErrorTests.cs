// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Generator;
using Durian.TestServices;
using Xunit;

namespace Durian.Analysis.DefaultParam.Tests.Types
{
	public sealed class TypeErrorTests : DefaultParamGeneratorTest
	{
		[Fact]
		public void Error_When_ContainingTypeIsDefaultParam()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
{{
	class Child<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>
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

class Parent
{{
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>
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

partial class Parent
{{
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T> where T : class
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

partial class Parent
{{
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string)]T, U>
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

partial class Parent
{{
	[{DurianStrings.GeneratorNamespace}.{nameof(DurianGeneratedAttribute)}]
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>
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

partial class Parent
{{
	[System.CodeDom.Compiler.GeneratedCode("", "")]
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DefaultParamDiagnostics.DUR0104_DefaultParamCannotBeAppliedWhenGenerationAttributesArePresent.Id));
		}

		[Fact]
		public void Error_When_IsPartial()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string)]T>
{{
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DefaultParamDiagnostics.DUR0122_DoNotUseDefaultParamOnPartialType.Id));
		}

		[Fact]
		public void Error_When_MemberExistsInTargetNamespace()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

namespace N2
{{
	class Test
	{{
	}}
}}

namespace N1
{{
	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TargetNamespace} = ""N2"")]
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
	{{
	}}
}}
";

			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DefaultParamDiagnostics.DUR0129_TargetNamespaceAlreadyContainsMemberWithName.Id));
		}

		[Fact]
		public void Error_When_MemberExistsInTargetNamespace_And_TargetsGlobalNamespace()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

class Test
{{
}}

namespace N1
{{
	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TargetNamespace} = ""global"")]
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
	{{
	}}
}}
";

			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DefaultParamDiagnostics.DUR0129_TargetNamespaceAlreadyContainsMemberWithName.Id));
		}

		[Fact]
		public void Error_When_MemberExistsInTargetNamespace_And_TargetsParentNamespace()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

namespace N1
{{
	class Test
	{{
	}}

	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TargetNamespace} = ""N1"")]
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
	{{
	}}
}}
";

			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DefaultParamDiagnostics.DUR0129_TargetNamespaceAlreadyContainsMemberWithName.Id));
		}

		[Fact]
		public void Error_When_OneOfContainingTypesIsNotPartial()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

class Parent1
{{
	partial class Parent2
	{{
		class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>
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

partial class Parent
{{
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T, [{DefaultParamAttributeProvider.TypeName}(typeof(string))]> where T : class where U : class
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

partial class Parent
{{
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T, U, [{DefaultParamAttributeProvider.TypeName}(typeof(int))]V>
	{{
	}}
}}
";

			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DefaultParamDiagnostics.DUR0105_DefaultParamMustBeLast.Id));
		}
	}
}
