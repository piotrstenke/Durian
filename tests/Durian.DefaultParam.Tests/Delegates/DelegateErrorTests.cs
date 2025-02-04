﻿using Durian.Generator;
using Durian.TestServices;
using Xunit;

namespace Durian.Analysis.DefaultParam.Tests.Delegates;

public sealed class DelegateErrorTests : DefaultParamGeneratorTest
{
	[Fact]
	public void Error_When_ContainingTypeIsDefaultParam()
	{
		string input =
@$"using {DurianStrings.MainNamespace};

partial class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
{{
	delegate void Del<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>();
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
	delegate void Del<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>();
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
	delegate void Del<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>() where T : class;
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
	delegate void Del<[{DefaultParamAttributeProvider.TypeName}(typeof(string)]T, U>();
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
	delegate void Del<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>();
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
	delegate void Del<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>();
}}
";
		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DefaultParamDiagnostics.DUR0104_DefaultParamCannotBeAppliedWhenGenerationAttributesArePresent.Id));
	}

	[Fact]
	public void Error_When_MemberExistsInTargetNamespace()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

namespace N2
{{
	delegate void Del();
}}

namespace N1
{{
	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TargetNamespace} = ""N2"")]
	delegate void Del<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>();
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

struct Del
{{
}}

namespace N1
{{
	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TargetNamespace} = ""global"")]
	delegate void Del<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>();
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
	class Del
	{{
	}}

	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TargetNamespace} = ""N1"")]
	delegate void Del<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>();
}}
";

		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DefaultParamDiagnostics.DUR0129_TargetNamespaceAlreadyContainsMemberWithName.Id));
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
		delegate void Del<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>();
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
	delegate void Del<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T, [{DefaultParamAttributeProvider.TypeName}(typeof(string))]>() where T : class where U : class;
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
	delegate void Del<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T, U, [{DefaultParamAttributeProvider.TypeName}(typeof(int))]V>();
}}
";

		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DefaultParamDiagnostics.DUR0105_DefaultParamMustBeLast.Id));
	}
}
