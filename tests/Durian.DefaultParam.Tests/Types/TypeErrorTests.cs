// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Configuration;
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

partial class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
{{
	class Child<[{nameof(DefaultParamAttribute)}(typeof(int))]T>
	{{
	}}
}}
";

			Assert.True(RunGenerator(input, 1).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0126_DefaultParamMembersCannotBeNested.Id));
		}

		[Fact]
		public void Error_When_ContainingTypeIsNotPartial()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

class Parent
{{
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T>
	{{
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

partial class Parent
{{
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T> where T : class
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0106_TargetTypeDoesNotSatisfyConstraint.Id));
		}

		[Fact]
		public void Error_When_DefaultParamAttributeIsNotLast()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{
	class Test<[{nameof(DefaultParamAttribute)}(typeof(string)]T, U>
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0105_DefaultParamMustBeLast.Id));
		}

		[Fact]
		public void Error_When_HasDurianGeneratedAttribute()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{
	[{DurianStrings.GeneratorNamespace}.{nameof(Generator.DurianGeneratedAttribute)}]
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T>
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0104_DefaultParamCannotBeAppliedWhenGenerationAttributesArePresent.Id));
		}

		[Fact]
		public void Error_When_HasGeneratedCodeAttribute()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{
	[System.CodeDom.Compiler.GeneratedCode("", "")]
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T>
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0104_DefaultParamCannotBeAppliedWhenGenerationAttributesArePresent.Id));
		}

		[Fact]
		public void Error_When_IsPartial()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test<[{nameof(DefaultParamAttribute)}(typeof(string)]T>
{{
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0122_DoNotUseDefaultParamOnPartialType.Id));
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
	[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.TargetNamespace)} = ""N2"")]
	class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
	{{
	}}
}}
";

			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0129_TargetNamespaceAlreadyContainsMemberWithName.Id));
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
	[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.TargetNamespace)} = ""global"")]
	class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
	{{
	}}
}}
";

			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0129_TargetNamespaceAlreadyContainsMemberWithName.Id));
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

	[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.TargetNamespace)} = ""N1"")]
	class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
	{{
	}}
}}
";

			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0129_TargetNamespaceAlreadyContainsMemberWithName.Id));
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
		class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T>
		{{
		}}
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0101_ContainingTypeMustBePartial.Id));
		}

		[Fact]
		public void Error_When_OneOfDefaultParamArgumentsIsInvalidForConstraint()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T, [{nameof(DefaultParamAttribute)}(typeof(string))]> where T : class where U : class
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0106_TargetTypeDoesNotSatisfyConstraint.Id));
		}

		[Fact]
		public void Error_When_OneOfMultipleDefaultParamAttributeIsNotLast()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{
	class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T, U, [{nameof(DefaultParamAttribute)}(typeof(int))]V>
	{{
	}}
}}
";

			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0105_DefaultParamMustBeLast.Id));
		}
	}
}
