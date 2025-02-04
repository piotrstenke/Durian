﻿using Durian.TestServices;
using Xunit;
using static Durian.Analysis.CopyFrom.CopyFromDiagnostics;

namespace Durian.Analysis.CopyFrom.Tests;

public sealed class CopyFromTypeGeneratorTests : CopyFromGeneratorTest
{
	[Fact]
	public void Error_When_ContainingTypeIsNotPartial()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

class Parent
{{
	[{CopyFromTypeAttributeProvider.TypeName}(typeof(Target))]
	partial class Test
	{{
	}}
}}

class Target
{{
}}
";
		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0201_ContainingTypeMustBePartial.Id));
	}

	[Fact]
	public void Error_When_CopiedFromDynamic()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(dynamic))]
partial class Test
{{
}}
";
		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0204_WrongTargetMemberKind.Id));
	}

	[Fact]
	public void Error_When_CopiedFromEnum()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(Target))]
partial class Test
{{
}}

enum Target
{{
}}
";
		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0204_WrongTargetMemberKind.Id));
	}

	[Fact]
	public void Error_When_CopiedFromNonType()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target.Method"")]
partial class Test
{{
}}

class Target
{{
	void Method()
	{{
	}}
}}
";
		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0203_MemberCannotBeResolved.Id));
	}

	[Fact]
	public void Error_When_CopiesFromChildType()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(Child))]
partial class Test
{{
}}

class Child : Test
{{
}}
";
		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0207_MemberCannotCopyFromItselfOrItsParent.Id));
	}

	[Fact]
	public void Error_When_CopiesFromContainingType()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

partial class Outer
{{
	[{CopyFromTypeAttributeProvider.TypeName}(typeof(Outer))]
	partial class Test
	{{
	}}
}}
";
		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0207_MemberCannotCopyFromItselfOrItsParent.Id));
	}

	[Fact]
	public void Error_When_CopiesFromDelegate()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(Delegate))]
partial class Test
{{
}}

delegate void Delegate();
";
		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0204_WrongTargetMemberKind.Id));
	}

	[Fact]
	public void Error_When_CopiesFromItself()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(Test))]
partial class Test
{{
}}
";
		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0207_MemberCannotCopyFromItselfOrItsParent.Id));
	}

	[Fact]
	public void Error_When_CopiesFromParentType()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(Parent))]
partial class Test : Parent
{{
}}

class Parent
{{
}}
";
		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0207_MemberCannotCopyFromItselfOrItsParent.Id));
	}

	[Fact]
	public void Error_When_HasConflictingMemberNamesFromDifferentNamespaces()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

using N1;
using N2;

[{CopyFromTypeAttributeProvider.TypeName}(""Target"")]
partial class Test
{{
}}

namespace N1
{{
	class Target
	{{
	}}
}}

namespace N2
{{
	class Target
	{{
	}}
}}
";
		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0208_MemberConflict.Id));
	}

	[Fact]
	public void Error_When_HasDirectCircularDependency()
	{
		string input =
@$"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Other"")]
partial class Test
{{
	void A()
	{{
	}}
}}

[{CopyFromTypeAttributeProvider.TypeName}(""Test"")]
partial class Other
{{
	void B()
	{{
	}}
}}
";
		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0221_CircularDependency));
	}

	[Fact]
	public void Error_When_HasNestedCircularDependency()
	{
		string input =
@$"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"")]
partial class Test
{{
	void A()
	{{
	}}
}}

[{CopyFromTypeAttributeProvider.TypeName}(""Other"")]
partial class Target
{{
	void B()
	{{
	}}
}}

[{CopyFromTypeAttributeProvider.TypeName}(""Test"")]
partial class Other
{{
	void C()
	{{
	}}
}}
";
		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0221_CircularDependency));
	}

	[Fact]
	public void Error_When_IsNotPartial()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(Target))]
class Test
{{
}}

class Target
{{
}}
";
		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0202_MemberMustBePartial.Id));
	}

	[Fact]
	public void Error_When_TargetIsArray()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(string[]))]
partial class Test
{{
}}
";
		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0204_WrongTargetMemberKind.Id));
	}

	[Fact]
	public void Error_When_TargetIsFunctionPointer()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(delegate*<int>))]
partial class Test
{{
}}
";
		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0204_WrongTargetMemberKind.Id));
	}

	[Fact]
	public void Error_When_TargetIsGeneric_And_TypeArgumentIsNotValid()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Other<int>"")]
partial class Test
{{
}}

class Other<T> where T : class
{{
}}
";
		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0217_TypeParameterIsNotValid.Id));
	}

	[Fact]
	public void Error_When_TargetIsGenericWithMultipleArguments_And_AtLeastOneArgumentIsNotValid()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Other<string, int, char>"")]
partial class Test
{{
}}

class Other<T, U, V> where T : class where U : struct where V : T
{{
}}
";
		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0217_TypeParameterIsNotValid.Id));
	}

	[Fact]
	public void Error_When_TargetIsGenericWithMultipleParameters_And_ParameterHasWrongName()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target<TType, UType>"")]
partial class Test
{{
}}

class Target<T, U>
{{
}}
";
		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0217_TypeParameterIsNotValid.Id));
	}

	[Fact]
	public void Error_When_TargetIsInDifferentAssembly()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(Target))]
partial class Test
{{
}}
";
		string external =
$@"class Target
{{
}}
";
		Assert.True(RunGeneratorWithDependency(input, external).FailedAndContainsDiagnostics(DUR0205_ImplementationNotAccessible.Id));
	}

	[Fact]
	public void Error_When_TargetIsInDifferentAssembly_And_GetsByString()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"")]
partial class Test
{{
}}
";
		string external =
$@"class Target
{{
}}
";
		Assert.True(RunGeneratorWithDependency(input, external).FailedAndContainsDiagnostics(DUR0205_ImplementationNotAccessible.Id));
	}

	[Fact]
	public void Error_When_TargetIsMultidimensionalArray()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(string[,]))]
partial class Test
{{
}}
";
		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0204_WrongTargetMemberKind.Id));
	}

	[Fact]
	public void Error_When_TargetIsNestedArray()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(string[][]))]
partial class Test
{{
}}
";
		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0204_WrongTargetMemberKind.Id));
	}

	[Fact]
	public void Error_When_TargetIsNullAsString()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}((string)null)]
partial class Test
{{
}}
";
		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0203_MemberCannotBeResolved.Id));
	}

	[Fact]
	public void Error_When_TargetIsNullAsType()
	{
		string input =
$@"using System;
using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}((Type)null)]
partial class Test
{{
}}
";
		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0203_MemberCannotBeResolved.Id));
	}

	[Fact]
	public void Error_When_TargetIsPointer()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(int*))]
partial class Test
{{
}}
";
		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0204_WrongTargetMemberKind.Id));
	}

	[Fact]
	public void Error_When_TargetIsTypeParameter()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

partial class Outer<T>
{{
	[{CopyFromTypeAttributeProvider.TypeName}(""T"")]
	partial class Test
	{{
	}}
}}
";
		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0204_WrongTargetMemberKind.Id));
	}

	[Fact]
	public void Error_When_TargetPartialPartDoesNotExist()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.PartialPart} = ""Part2"")]
partial class Test
{{
}}

[{nameof(PartialNameAttribute)}(""Part1"")]
partial class Target
{{
	void Method()
	{{
		string b = string.Empty;
	}}
}}

partial class Target
{{
	void Other()
	{{
		int a = 2;
	}}
}}
";

		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0218_UnknownPartialPartName.Id));
	}

	[Fact]
	public void Error_When_TypeDoesNotExist()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"")]
partial class Test
{{
}}
";
		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0203_MemberCannotBeResolved.Id));
	}

	[Fact]
	public void Success_When_AddsStaticUsingsAndAliases()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.AddUsings} = new string[] {{ ""static System.Int32"", ""Ta = Target"" }})]
partial class Test
{{
}}

class Target
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};
using static System.Int32;
using Ta = Target;

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_AddsUsings()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.AddUsings} = new string[] {{ ""System"" }})]
partial class Test
{{
}}

class Target
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};
using System;

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_AddsUsings_And_UsingAlreadyExistsBecauseOfCopyUsings()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using System;

[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.AddUsings} = new string[] {{ ""System"" }})]
partial class Test
{{
}}

class Target
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};
using System;

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_AppliedOrderToPartialPart()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.PartialPart} = ""Part1"", {CopyFromTypeAttributeProvider.Order} = 1)]
[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.PartialPart} = ""Part2"", {CopyFromTypeAttributeProvider.Order} = 0)]
partial class Test
{{
}}

[{nameof(PartialNameAttribute)}(""Part1"")]
partial class Target
{{
	void A()
	{{
		string a = string.Empty;
	}}
}}

[{nameof(PartialNameAttribute)}(""Part2"")]
partial class Target
{{
	void B()
	{{
		int b = int.MaxValue;
	}}
}}
";
		string expected1 =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.B()")}
	void B()
	{{
		int b = int.MaxValue;
	}}
}}
";
		string expected2 =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.A()")}
	void A()
	{{
		string a = string.Empty;
	}}
}}
";

		Assert.True(RunGeneratorWithMultipleOutputs(input).Compare(expected1, expected2));
	}

	[Fact]
	public void Success_When_AppliesOrder()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.Order} = 1)]
[{CopyFromTypeAttributeProvider.TypeName}(""Other"", {CopyFromTypeAttributeProvider.Order} = 0)]
partial class Test
{{
}}

class Target
{{
	void A()
	{{
		string a = string.Empty;
	}}
}}

class Other
{{
	void B()
	{{
		int b = int.MaxValue;
	}}
}}
";
		string expected1 =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Other.B()")}
	void B()
	{{
		int b = int.MaxValue;
	}}
}}
";
		string expected2 =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.A()")}
	void A()
	{{
		string a = string.Empty;
	}}
}}
";

		Assert.True(RunGeneratorWithMultipleOutputs(input).Compare(expected1, expected2));
	}

	[Fact]
	public void Success_When_CopiesBaseType()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.BaseType})]
partial class Test
{{
}}

class Target : System.Exception, System.IDisposable
{{
	void Method()
	{{
		string a = string.Empty;
	}}

	public void Dispose()
	{{
	}}
}}
";
		string expected =
$@"internal partial class Test : System.Exception
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}

	{GetCodeGenerationAttributes("Target.Dispose()")}
	public void Dispose()
	{{
	}}
}}
";
		SingleGeneratorTestResult runResult = RunGenerator(input);

		Assert.True(runResult.SucceededAndDoesNotContainDiagnostics(DUR0226_CannotApplyBaseType));
		Assert.True(runResult.Compare(expected));
	}

	[Fact]
	public void Success_When_CopiesBaseType_And_HasPattern()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.BaseType})]
[{PatternAttributeProvider.TypeName}(""System\.Exception"", ""System.Attribute"")]
partial class Test
{{
}}

class Target : System.Exception, System.IDisposable
{{
	void Method()
	{{
		string a = string.Empty;
	}}

	public void Dispose()
	{{
	}}
}}
";
		string expected =
$@"internal partial class Test : System.Attribute
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}

	{GetCodeGenerationAttributes("Target.Dispose()")}
	public void Dispose()
	{{
	}}
}}
";
		SingleGeneratorTestResult runResult = RunGenerator(input);

		Assert.True(runResult.SucceededAndDoesNotContainDiagnostics(DUR0226_CannotApplyBaseType));
		Assert.True(runResult.Compare(expected));
	}

	[Fact]
	public void Success_When_CopiesBaseType_And_TargetHasNoBaseType()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.BaseType})]
partial class Test
{{
}}

class Target
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
$@"internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		SingleGeneratorTestResult runResult = RunGenerator(input);

		Assert.True(runResult.SucceededAndDoesNotContainDiagnostics(DUR0226_CannotApplyBaseType));
		Assert.True(runResult.Compare(expected));
	}

	[Fact]
	public void Success_When_CopiesConstraints_And_HasPattern()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target<TType>"", {CopyFromTypeAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.Constraints})]
[{PatternAttributeProvider.TypeName}(""TType"", ""T"")]
partial class Test<T>
{{
}}

class Target<TType> where TType : struct
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
$@"internal partial class Test<T> where T : struct
{{
	{GetCodeGenerationAttributes("Target<TType>.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_CopiesConstraints_And_TargetIsGenericWithConstraints()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target<T>"", {CopyFromTypeAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.Constraints})]
partial class Test<T>
{{
}}

class Target<T> where T : struct
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
$@"internal partial class Test<T> where T : struct
{{
	{GetCodeGenerationAttributes("Target<T>.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_CopiesConstraints_And_TargetIsGenericWithoutConstraints()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target<T>"", {CopyFromTypeAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.Constraints})]
partial class Test<T>
{{
}}

class Target<T>
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
$@"internal partial class Test<T>
{{
	{GetCodeGenerationAttributes("Target<T>.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_CopiesConstraints_And_TargetIsNotGeneric()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.Constraints})]
partial class Test<T>
{{
}}

class Target
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
$@"internal partial class Test<T>
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_CopiesDocumentation()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.Documentation})]
partial class Test
{{
}}

/// <summary>
/// Hello there
/// </summary>
class Target
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
$@"/// <summary>
/// Hello there
/// </summary>
internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected, true));
	}

	[Fact]
	public void Success_When_CopiesDocumentation_And_HasPattern()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.Documentation})]
[{PatternAttributeProvider.TypeName}(""Hello"", ""No"")]
partial class Test
{{
}}

/// <summary>
/// Hello there
/// </summary>
class Target
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
$@"/// <summary>
/// No there
/// </summary>
internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected, true));
	}

	[Fact]
	public void Success_When_CopiesFromTypeWithAttributes_And_AllowsCopyFromAttributes_And_HasPattern()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.Attributes})]
[{PatternAttributeProvider.TypeName}(""DEBUG"", ""RELEASE"")]
partial class Test
{{
}}

[System.Diagnostics.Conditional(""DEBUG"")]
class Target
{{
	void Method()
	{{
		string b = string.Empty;
	}}
}}
";
		string expected =
$@"[System.Diagnostics.Conditional(""RELEASE"")]
internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string b = string.Empty;
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_CopiesFromTypeWithAttributes()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"")]
partial class Test
{{
}}

[System.Diagnostics.Conditional(""DEBUG"")]
class Target
{{
	void Method()
	{{
		string b = string.Empty;
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string b = string.Empty;
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_CopiesFromTypeWithAttributes_And_AllowsCopyFromAttributes()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.Attributes})]
partial class Test
{{
}}

[System.Diagnostics.Conditional(""DEBUG"")]
class Target
{{
	void Method()
	{{
		string b = string.Empty;
	}}
}}
";
		string expected =
$@"[System.Diagnostics.Conditional(""DEBUG"")]
internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string b = string.Empty;
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_CopiesFromTypeWithCopyFrom()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"")]
partial class Test
{{
}}

[{CopyFromTypeAttributeProvider.TypeName}(""Other"")]
partial class Target
{{
}}

class Other
{{
	void A()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected1 =
$@"using {DurianStrings.MainNamespace};

internal partial class Target
{{
	{GetCodeGenerationAttributes("Other.A()")}
	void A()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected2 =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.A()")}
	void A()
	{{
		string a = string.Empty;
	}}
}}
";
		Assert.True(RunGeneratorWithMultipleOutputs(input).Compare(expected1, expected2));
	}

	[Fact]
	public void Success_When_CopiesFromTypeWithCopyFromWithPattern()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"")]
partial class Test
{{
}}

[{CopyFromTypeAttributeProvider.TypeName}(""Other""), {PatternAttributeProvider.TypeName}(""string "", ""var "")]
partial class Target
{{
}}

class Other
{{
	void A()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected1 =
$@"using {DurianStrings.MainNamespace};

internal partial class Target
{{
	{GetCodeGenerationAttributes("Other.A()")}
	void A()
	{{
		var a = string.Empty;
	}}
}}
";
		string expected2 =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.A()")}
	void A()
	{{
		var a = string.Empty;
	}}
}}
";
		Assert.True(RunGeneratorWithMultipleOutputs(input).Compare(expected1, expected2));
	}

	[Fact]
	public void Success_When_CopiesInterfaces()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.BaseInterfaces})]
partial class Test
{{
}}

class Target : System.Exception, System.IDisposable
{{
	void Method()
	{{
		string a = string.Empty;
	}}

	public void Dispose()
	{{
	}}
}}
";
		string expected =
$@"internal partial class Test : System.IDisposable
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}

	{GetCodeGenerationAttributes("Target.Dispose()")}
	public void Dispose()
	{{
	}}
}}
";
		SingleGeneratorTestResult runResult = RunGenerator(input);

		Assert.True(runResult.SucceededAndDoesNotContainDiagnostics(DUR0226_CannotApplyBaseType));
		Assert.True(runResult.Compare(expected));
	}

	[Fact]
	public void Success_When_CopiesInterfaces_And_HasPattern()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.BaseInterfaces})]
[{PatternAttributeProvider.TypeName}(""System.IDisposable"", ""System.ICloneable"")]
partial class Test
{{
	public object Clone()
	{{
		return this;
	}}
}}

class Target : System.Exception, System.IDisposable
{{
	void Method()
	{{
		string a = string.Empty;
	}}

	public void Dispose()
	{{
	}}
}}
";
		string expected =
$@"internal partial class Test : System.ICloneable
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}

	{GetCodeGenerationAttributes("Target.Dispose()")}
	public void Dispose()
	{{
	}}
}}
";
		SingleGeneratorTestResult runResult = RunGenerator(input);

		Assert.True(runResult.SucceededAndDoesNotContainDiagnostics(DUR0226_CannotApplyBaseType));
		Assert.True(runResult.Compare(expected));
	}

	[Fact]
	public void Success_When_CopiesInterfaces_And_TargetHasNoInterfaces()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.BaseInterfaces})]
partial class Test
{{
}}

class Target
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
$@"internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		SingleGeneratorTestResult runResult = RunGenerator(input);

		Assert.True(runResult.SucceededAndDoesNotContainDiagnostics(DUR0226_CannotApplyBaseType));
		Assert.True(runResult.Compare(expected));
	}

	[Fact]
	public void Success_When_CopyAttributes_And_TargetHasMultiplePartialParts()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.Attributes})]
partial class Test
{{
}}

[System.Diagnostics.Conditional(""DEBUG"")]
partial class Target
{{
	void Method1()
	{{
		string a = string.Empty;
	}}
}}

[System.Serializable]
partial class Target
{{
	void Method2()
	{{
		string b = string.Empty;
	}}
}}
";
		string expected1 =
$@"[System.Diagnostics.Conditional(""DEBUG"")]
internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method1()")}
	void Method1()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected2 =
$@"[System.Serializable]
internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method2()")}
	void Method2()
	{{
		string b = string.Empty;
	}}
}}
";
		Assert.True(RunGeneratorWithMultipleOutputs(input).Compare(expected1, expected2));
	}

	[Fact]
	public void Success_When_CopyUsingsIsFalse()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.AdditionalNodes} = default)]
partial class Test
{{
}}

class Target
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
$@"internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_CopyUsingsIsFalse_And_AddsUsings()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.AdditionalNodes} = default, {CopyFromTypeAttributeProvider.AddUsings} = new string[] {{ ""System"" }})]
partial class Test
{{
}}

class Target
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
$@"using System;

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_HandleSpecialMembersIsFalse()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.HandleSpecialMembers} = false)]
partial class Test
{{
}}

class Target
{{
	Target()
	{{
	}}

	~Target()
	{{
	}}

	public static implicit operator string(Target a, Target b) => """";

	public static bool operator +(Target a, Target b) => true;
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Target()")}
	Target()
	{{
	}}

	{GetCodeGenerationAttributes("Target.~Target()")}
	~Target()
	{{
	}}

	{GetCodeGenerationAttributes("Target.implicit operator string(Target, Target)")}
	public static implicit operator string(Target a, Target b) => """";

	{GetCodeGenerationAttributes("Target.operator +(Target, Target)")}
	public static bool operator +(Target a, Target b) => true;
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_HasMultiplePatterns()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"")]
[{PatternAttributeProvider.TypeName}(""A"", ""B""), {PatternAttributeProvider.TypeName}(""B"", ""C"")]
partial class Test
{{
}}

class Target
{{
	void A()
	{{
		A();
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.A()")}
	void C()
	{{
		C();
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_HasMultiplePatternsWithOrder()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"")]
[{PatternAttributeProvider.TypeName}(""B"", ""C"", {PatternAttributeProvider.Order} = 1), {PatternAttributeProvider.TypeName}(""A"", ""B"", {PatternAttributeProvider.Order} = 0)]
partial class Test
{{
}}

class Target
{{
	void A()
	{{
		A();
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.A()")}
	void C()
	{{
		C();
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_HasMultipleTargets()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"")]
[{CopyFromTypeAttributeProvider.TypeName}(""Other"")]
partial class Test
{{
}}

class Target
{{
	void A1()
	{{
		A1();
	}}
}}

class Other
{{
	void A2()
	{{
		A2();
	}}
}}
";
		string expected1 =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.A1()")}
	void A1()
	{{
		A1();
	}}
}}
";
		string expected2 =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Other.A2()")}
	void A2()
	{{
		B2();
	}}
}}
";
		Assert.True(RunGeneratorWithMultipleOutputs(input).Compare(expected1, expected2));
	}

	[Fact]
	public void Success_When_HasPattern()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"")]
[{PatternAttributeProvider.TypeName}(""A"", ""B"")]
partial class Test
{{
}}

class Target
{{
	void A()
	{{
		A();
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.A()")}
	void B()
	{{
		B();
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_HasPattern_And_MemberOfTargetHasXmlComment()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(Target))]
[{PatternAttributeProvider.TypeName}(""string"", ""int"")]
partial class Test
{{
}}

class Target
{{
	/// <summary>
	/// <see cref=""string""/>.
	/// </summary>
	void Method()
	{{
		string b = default;
	}}

	/// <summary>
	/// <see cref=""string""/>.
	/// </summary>
	/// <param name=""a""></param>
	void Method(string a)
	{{
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	/// <summary>
	/// <see cref=""int""/>.
	/// </summary>
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		int b = default;
	}}

	/// <summary>
	/// <see cref=""int""/>.
	/// </summary>
	/// <param name=""a""></param>
	{GetCodeGenerationAttributes("Target.Method(string)")}
	void Method(int a)
	{{
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected, true));
	}

	[Fact]
	public void Success_When_HasPattern_And_MultipleTargets()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"")]
[{CopyFromTypeAttributeProvider.TypeName}(""Other"")]
[{PatternAttributeProvider.TypeName}(""A"", ""B"")]
partial class Test
{{
}}

class Target
{{
	void A1()
	{{
		A1();
	}}
}}

class Other
{{
	void A2()
	{{
		A2();
	}}
}}
";
		string expected1 =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.A1()")}
	void B1()
	{{
		B1();
	}}
}}
";
		string expected2 =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Other.A2()")}
	void B2()
	{{
		B2();
	}}
}}
";
		Assert.True(RunGeneratorWithMultipleOutputs(input).Compare(expected1, expected2));
	}

	[Fact]
	public void Success_When_HasPattern_And_TargetHasMultiplePartialParts()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"")]
[{PatternAttributeProvider.TypeName}(""string\.Empty"", ""default"")]
partial class Test
{{
}}

partial class Target
{{
	void Method1()
	{{
		string a = string.Empty;
	}}
}}

partial class Target
{{
	void Method2()
	{{
		string b = string.Empty;
	}}
}}

";
		string expected1 =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method1()")}
	void Method1()
	{{
		string a = default;
	}}
}}
";
		string expected2 =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method2()")}
	void Method2()
	{{
		string b = default;
	}}
}}
";
		Assert.True(RunGeneratorWithMultipleOutputs(input).Compare(expected1, expected2));
	}

	[Fact]
	public void Success_When_IncludesAllNonStandardNodes_And_HasDocumentation()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

/// <summary>
/// ABC
/// </summary>
[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.All})]
partial class Test
{{
}}

/// <summary>
/// Hello there
/// </summary>
class Target
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected, true));
	}

	[Fact]
	public void Success_When_IncludesAllNonStandardNodes_And_IsGenericWithConstraints()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target<T>"", {CopyFromTypeAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.All})]
partial class Test<T> where T : class
{{
}}

class Target<T> where T : struct
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

internal partial class Test<T>
{{
	{GetCodeGenerationAttributes("Target<T>.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_IncludesAllNonStandardNodes_And_IsNotGeneric()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target<T>"", {CopyFromTypeAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.All})]
partial class Test
{{
}}

class Target<T> where T : struct
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target<T>.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_IncludesAllNonStandardNodes_And_TargetHasBaseType_And_AlreadyHasBaseType()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.All})]
partial class Test : System.Attribute
{{
}}

class Target : System.Exception
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		SingleGeneratorTestResult runResult = RunGenerator(input);

		Assert.True(runResult.SucceededAndDoesNotContainDiagnostics(DUR0225_BaseTypeAlreadySpecified));
		Assert.True(runResult.Compare(expected));
	}

	[Fact]
	public void Success_When_IncludesAllNonStandardNodes_And_TargetHasBaseType_And_IsInterface()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.All})]
partial interface ITest
{{
}}

class Target : System.Exception
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

internal partial interface ITest
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		SingleGeneratorTestResult runResult = RunGenerator(input);

		Assert.True(runResult.SucceededAndDoesNotContainDiagnostics(DUR0226_CannotApplyBaseType));
		Assert.True(runResult.Compare(expected));
	}

	[Fact]
	public void Success_When_IncludesAllNonStandardNodes_And_TargetHasBaseType_And_IsStatic()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.All})]
static partial class Test
{{
}}

class Target : System.Exception
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

internal static partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		SingleGeneratorTestResult runResult = RunGenerator(input);

		Assert.True(runResult.SucceededAndDoesNotContainDiagnostics(DUR0226_CannotApplyBaseType));
		Assert.True(runResult.Compare(expected));
	}

	[Fact]
	public void Success_When_IncludesAllNonStandardNodes_And_TargetHasBaseType_And_IsStruct()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.All})]
partial struct Test
{{
}}

class Target : System.Exception
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

internal partial struct Test
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		SingleGeneratorTestResult runResult = RunGenerator(input);

		Assert.True(runResult.SucceededAndDoesNotContainDiagnostics(DUR0226_CannotApplyBaseType));
		Assert.True(runResult.Compare(expected));
	}

	[Fact]
	public void Success_When_IsPartial_And_TargetIsPartialPart()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(Target), PartialPart = ""Part1"")]
partial class Test
{{
}}

[{CopyFromTypeAttributeProvider.TypeName}(typeof(Target), PartialPart = ""Part2"")]
partial class Test
{{
}}

[{nameof(PartialNameAttribute)}(""Part1"")]
partial class Target
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}

[{nameof(PartialNameAttribute)}(""Part2"")]
partial class Target
{{
	void Other()
	{{
		int a = default(int);
	}}
}}
";
		string expected1 =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected2 =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Other()")}
	void Other()
	{{
		int a = default(int);
	}}
}}
";
		Assert.True(RunGeneratorWithMultipleOutputs(input).Compare(expected1, expected2));
	}

	[Fact]
	public void Success_When_MemberOfTargetHasXmlComment()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(Target))]
partial class Test
{{
}}

class Target
{{
	/// <summary>
	/// Hello there
	/// </summary>
	void Method()
	{{
		string b = """";
	}}

	/// <inheritdoc cref=""Method()""/>
	void Method(string a)
	{{
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	/// <inheritdoc cref=""Target.Method()""/>
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string b = """";
	}}

	/// <inheritdoc cref=""Target.Method(string)""/>
	{GetCodeGenerationAttributes("Target.Method(string)")}
	void Method(string a)
	{{
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected, true));
	}

	[Fact]
	public void Success_When_SameTargetButDifferentPartialPart()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(Target), PartialPart = ""Part1"")]
[{CopyFromTypeAttributeProvider.TypeName}(typeof(Target), PartialPart = ""Part2"")]
partial class Test
{{
}}

[{nameof(PartialNameAttribute)}(""Part1"")]
partial class Target
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}

[{nameof(PartialNameAttribute)}(""Part2"")]
partial class Target
{{
	void Other()
	{{
		int a = default(int);
	}}
}}
";
		string expected1 =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected2 =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Other()")}
	void Other()
	{{
		int a = default(int);
	}}
}}
";
		Assert.True(RunGeneratorWithMultipleOutputs(input).Compare(expected1, expected2));
	}

	[Fact]
	public void Success_When_SpecifiesPartialPart()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.PartialPart} = ""Part2"")]
partial class Test
{{
}}

[{nameof(PartialNameAttribute)}(""Part1"")]
partial class Target
{{
	void Method()
	{{
		string b = string.Empty;
	}}
}}

[{nameof(PartialNameAttribute)}(""Part2"")]
partial class Target
{{
	void Other()
	{{
		int a = 2;
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Other()")}
	void Other()
	{{
		int a = 2;
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_TargetHasXmlComment()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(Target))]
partial class Test
{{
}}

/// <summary>
/// Hello there
/// </summary>
class Target
{{
	void Method()
	{{
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected, true));
	}

	[Fact]
	public void Success_When_TargetIsAccessible()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(Target))]
partial class Test
{{
}}

class Target
{{
	void Method()
	{{
		string b = """";
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string b = """";
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_TargetIsAliased()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

using T = N2.Target;

namespace N1
{{
	[{CopyFromTypeAttributeProvider.TypeName}(""T"")]
	partial class Test
	{{
	}}
}}

namespace N2
{{
	class Target
	{{
		void Method()
		{{
			string b = """";
		}}
	}}
}}
";

		string expected =
$@"using {DurianStrings.MainNamespace};
using T = N2.Target;

namespace N1
{{
	internal partial class Test
	{{
		{GetCodeGenerationAttributes("N2.Target.Method()", 2)}
		void Method()
		{{
			string b = """";
		}}
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_TargetIsGeneric()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target<int>"")]
partial class Test
{{
}}

class Target<T>
{{
	void Method(T value)
	{{
		T b = default(T);
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target<T>.Method(T)")}
	void Method(int value)
	{{
		int b = default(int);
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_TargetIsGeneric_And_IsNestedTypeOfUnboundGeneric()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Outer<,>.Target<int>"")]
partial class Test
{{
}}

class Outer<T, U>
{{
	public class Target<X>
	{{
		void Method(T a, U b, X c)
		{{
			T a1 = default(T);
			U b2 = b;
			X[] c3 = new X[] {{ c }};
		}}
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Outer<T, U>.Target<X>.Method(T, U, X)")}
	void Method(T a, U b, int c)
	{{
		T a1 = default(T);
		U b2 = b;
		int[] c3 = new int[] {{ c }};
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_TargetIsGenericWithMultipleParameters()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target<T, U>"")]
partial class Test
{{
}}

class Target<T, U>
{{
	void Method(T t)
	{{
	}}

	void Method(T t, U u)
	{{
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target<T, U>.Method(T)")}
	void Method(T t)
	{{
	}}

	{GetCodeGenerationAttributes("Target<T, U>.Method(T, U)")}
	void Method(T t, U u)
	{{
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_TargetIsGlobalNamespace_And_CurrentInIsNormalNamespace()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

namespace N1
{{
	[{CopyFromTypeAttributeProvider.TypeName}(""Target"")]
	partial class Test
	{{
	}}
}}

class Target
{{
	void Method()
	{{
		string b = string.Empty;
	}}
}}
";

		string expected =
$@"using {DurianStrings.MainNamespace};

namespace N1
{{
	internal partial class Test
	{{
		{GetCodeGenerationAttributes("Target.Method()", 2)}
		void Method()
		{{
			string b = string.Empty;
		}}
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_TargetIsGlobalNamespace_And_CurrentTypeIsInGlobalNamespace()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"")]
partial class Test
{{
}}

class Target
{{
	void Method()
	{{
		string b = string.Empty;
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method()", 2)}
	void Method()
	{{
		string b = string.Empty;
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_TargetIsGlobalNamespace_And_UsedGlobalKeyword()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

namespace N1
{{
	[{CopyFromTypeAttributeProvider.TypeName}(""global::Target"")]
	partial class Test
	{{
	}}
}}

class Target
{{
	void Method()
	{{
		string b = string.Empty;
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

namespace N1
{{
	internal partial class Test
	{{
		{GetCodeGenerationAttributes("Target.Method()", 2)}
		void Method()
		{{
			string b = string.Empty;
		}}
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_TargetIsInDifferentNamespace_And_HasUsings()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using N2;

namespace N1
{{
	[{CopyFromTypeAttributeProvider.TypeName}(""Target"")]
	partial class Test
	{{
	}}
}}

namespace N2
{{
	class Target
	{{
		void Method()
		{{
			string b = string.Empty;
		}}
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};
using N2;

namespace N1
{{
	internal partial class Test
	{{
		{GetCodeGenerationAttributes("N2.Target.Method()", 2)}
		void Method()
		{{
			string b = string.Empty;
		}}
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_TargetIsInInnerNamespace()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

namespace N1
{{
	[{CopyFromTypeAttributeProvider.TypeName}(""N2.Target"")]
	partial class Test
	{{
	}}
}}

namespace N1.N2
{{
	class Target
	{{
		void Method()
		{{
			string b = string.Empty;
		}}
	}}
}}

";
		string expected =
$@"using {DurianStrings.MainNamespace};

namespace N1
{{
	internal partial class Test
	{{
		{GetCodeGenerationAttributes("N1.N2.Target.Method()", 2)}
		void Method()
		{{
			string b = string.Empty;
		}}
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_TargetIsInnerClass()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Outer.Target"")]
partial class Test
{{
}}

class Outer
{{
	public class Target
	{{
		void Method()
		{{
			string b = string.Empty;
		}}
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Outer.Target.Method()")}
	void Method()
	{{
		string b = string.Empty;
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_TargetIsInnerClass_And_HasStaticUsing()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

using static Outer;

namespace N1
{{
	[{CopyFromTypeAttributeProvider.TypeName}(""Target"")]
	partial class Test
	{{
	}}
}}

class Outer
{{
	public class Target
	{{
		void Method()
		{{
			string b = string.Empty;
		}}
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

using static Outer;

namespace N1
{{
	internal partial class Test
	{{
		{GetCodeGenerationAttributes("Outer.Target.Method()", 2)}
		void Method()
		{{
			string b = string.Empty;
		}}
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_TargetIsInnerClassOfAliasedType()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

using O = Outer;

[{CopyFromTypeAttributeProvider.TypeName}(""O.Target"")]
partial class Test
{{
}}

class Outer
{{
	public class Target
	{{
		void Method()
		{{
			string b = string.Empty;
		}}
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

using O = Outer;

internal partial class Test
{{
	{GetCodeGenerationAttributes("Outer.Target.Method()")}
	void Method()
	{{
		string b = string.Empty;
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_TargetIsInnerType()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(Inner))]
partial class Test
{{
	class Inner
	{{
		void Method()
		{{
			string b = """";
		}}
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Inner.Method()")}
	void Method()
	{{
		string b = """";
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_TargetIsInSameNamespace()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

namespace N1
{{
	[{CopyFromTypeAttributeProvider.TypeName}(""Target"")]
	partial class Test
	{{
	}}
}}

namespace N1
{{
	class Target
	{{
		void Method()
		{{
			string b = string.Empty;
		}}
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

namespace N1
{{
	internal partial class Test
	{{
		{GetCodeGenerationAttributes("N1.Target.Method()", 2)}
		void Method()
		{{
			string b = string.Empty;
		}}
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_TargetIsNestedTypeOfGeneric()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Outer<int, string>.Target"")]
partial class Test
{{
}}

class Outer<T, U>
{{
	public class Target
	{{
		void Method(T t, U u)
		{{
			T b = default(T);
			new System.Collections.Generic.List<U>();
		}}
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Outer<T, U>.Target.Method(T, U)")}
	void Method(int t, string u)
	{{
		int b = default(int);
		new System.Collections.Generic.List<string>();
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_TargetIsNestedTypeOfUnboundGeneric()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Outer<,>.Target"")]
partial class Test
{{
}}

class Outer<T, U>
{{
	public class Target
	{{
		void Method(T t, U u)
		{{
			T b = default(T);
			new System.Collections.Generic.List<U>();
		}}
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Outer<T, U>.Target.Method(T, U)")}
	void Method(T t, U u)
	{{
		T b = default(T);
		new System.Collections.Generic.List<U>();
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_TargetIsNullableReferenceType()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target?"")]
partial class Test
{{
}}

class Target
{{
	void Method()
	{{
		string b = string.Empty;
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string b = string.Empty;
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_TargetIsPartial()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"")]
partial class Test
{{
}}

partial class Target
{{
	void Method()
	{{
		string b = string.Empty;
	}}
}}

partial class Target
{{
	void Method(string a)
	{{
		while(true)
		{{
			break;
		}}
	}}
}}
";

		string expected1 =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string b = string.Empty;
	}}
}}
";
		string expected2 =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method(string)")}
	void Method(string a)
	{{
		while(true)
		{{
			break;
		}}
	}}
}}
";
		Assert.True(RunGeneratorWithMultipleOutputs(input).Compare(expected1, expected2));
	}

	[Fact]
	public void Success_When_TargetIsPrivate()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(Outer.Target))]
partial class Test
{{
}}

class Outer
{{
	private class Target
	{{
		void Method()
		{{
			string b = string.Empty;
		}}
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Outer.Target.Method()")}
	void Method()
	{{
		string b = string.Empty;
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_TargetIsSpecifiedUsingFullyQualifiedName()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""N1.N2.Target"")]
partial class Test
{{
}}

namespace N1.N2
{{
	class Target
	{{
		void Method()
		{{
			string b = string.Empty;
		}}
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("N1.N2.Target.Method()")}
	void Method()
	{{
		string b = string.Empty;
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_TargetIsSpecifiedUsingFullyQualifiedNameWithGlobalKeyword()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""global::N1.N2.Target"")]
partial class Test
{{
}}

namespace N1.N2
{{
	class Target
	{{
		void Method()
		{{
			string b = string.Empty;
		}}
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("N1.N2.Target.Method()")}
	void Method()
	{{
		string b = string.Empty;
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_TargetIsUnboundGeneric()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target<,>"")]
partial class Test
{{
}}

class Target<T, U>
{{
	void Method(T t, U u)
	{{
		T b = default(T);
		new System.Collections.Generic.List<U>();
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target<T, U>.Method(T, U)")}
	void Method(T t, U u)
	{{
		T b = default(T);
		new System.Collections.Generic.List<U>();
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_TargetHasAllPossibleMemberTypes()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"")]
partial class Test
{{
}}

class Target : System.IDisposable
{{
	public Target()
	{{
	}}

	static Target()
	{{
	}}

	~Target()
	{{
	}}

	public delegate void Delegate();

	event Delegate Event;

	event System.Action Event2 {{ add {{ }} remove {{ }}}}

	[System.NonSerialized]
	private protected readonly int _field, _field2;

	const float cos = 2;

	public int Field => _field;

	public string Name {{ get; set; }}

	public int this[int index] => 2;

	string Method(dynamic a) => string.Empty;

	void Method<T>(ref T t1, in T t2, out T t2)
	{{
		string b = string.Empty;
	}}

	void System.IDisposable.Dispose()
	{{
	}}

	public static int operator +(Target a, Target b) => 2;
	public static implicit operator bool(Target a) => a is not null;

	class Inner
	{{
		string field;
	}}

	enum Values
	{{
		A = 1,
		B
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Target()")}
	public Test()
	{{
	}}

	{GetCodeGenerationAttributes("Target.static Target()")}
	static Test()
	{{
	}}

	{GetCodeGenerationAttributes("Target.~Target()")}
	~Test()
	{{
	}}

	{GetCodeGenerationAttributes("Target.Delegate")}
	public delegate void Delegate();

	{GetCodeGenerationAttributes("Target.Event")}
	event Delegate Event;

	{GetCodeGenerationAttributes("Target.Event2")}
	event System.Action Event2 {{ add {{ }} remove {{ }}}}

	{GetCodeGenerationAttributes("Target._field")}
	[System.NonSerialized]
	private protected readonly int _field;

	{GetCodeGenerationAttributes("Target._field2")}
	[System.NonSerialized]
	private protected readonly int _field2;

	{GetCodeGenerationAttributes("Target.cos")}
	const float cos = 2;

	{GetCodeGenerationAttributes("Target.Field")}
	public int Field => _field;

	{GetCodeGenerationAttributes("Target.Name")}
	public string Name {{ get; set; }}

	{GetCodeGenerationAttributes("Target.this[int]")}
	public int this[int index] => 2;

	{GetCodeGenerationAttributes("Target.Method(dynamic)")}
	string Method(dynamic a) => string.Empty;

	{GetCodeGenerationAttributes("Target.Method<T>(ref T, in T, out T)")}
	void Method<T>(ref T t1, in T t2, out T t2)
	{{
		string b = string.Empty;
	}}

	{GetCodeGenerationAttributes("(System.IDisposable)Target.Dispose()")}
	void System.IDisposable.Dispose()
	{{
	}}

	{GetCodeGenerationAttributes("Target.operator +(Target, Target)")}
	public static int operator +(Test a, Test b) => 2;

	{GetCodeGenerationAttributes("Target.implicit operator bool(Target)")}
	public static implicit operator bool(Test a) => a is not null;

	{GetCodeGenerationAttributes("Target.Inner")}
	class Inner
	{{
		string field;
	}}

	{GetCodeGenerationAttributes("Target.Values")}
	enum Values
	{{
		A = 1,
		B
	}}
}}
";

		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Warning_When_AddsEquivalentUsing()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.AddUsings} = new string[] {{ ""System"", ""System"" }})]
partial class Test
{{
}}

class Target
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};
using System;

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		SingleGeneratorTestResult runResult = RunGenerator(input);

		Assert.True(runResult.SucceededAndContainsDiagnostics(DUR0220_UsingAlreadySpecified.Id));
		Assert.True(runResult.Compare(expected));
	}

	[Fact]
	public void Warning_When_CopiesBaseType_And_AlreadyHasBaseType()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.BaseType})]
partial class Test : System.Attribute
{{
}}

class Target : System.Exception
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
$@"internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		SingleGeneratorTestResult runResult = RunGenerator(input);

		Assert.True(runResult.SucceededAndContainsDiagnostics(DUR0225_BaseTypeAlreadySpecified.Id));
		Assert.True(runResult.Compare(expected));
	}

	[Fact]
	public void Warning_When_CopiesBaseType_And_IsInterface()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.BaseType})]
partial interface ITest
{{
}}

class Target : System.Exception
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
$@"internal partial interface ITest
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		SingleGeneratorTestResult runResult = RunGenerator(input);

		Assert.True(runResult.SucceededAndContainsDiagnostics(DUR0226_CannotApplyBaseType.Id));
		Assert.True(runResult.Compare(expected));
	}

	[Fact]
	public void Warning_When_CopiesBaseType_And_IsStatic()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.BaseType})]
static partial class Test
{{
}}

class Target : System.Exception
{{
	static void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
$@"internal static partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	static void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		SingleGeneratorTestResult runResult = RunGenerator(input);

		Assert.True(runResult.SucceededAndContainsDiagnostics(DUR0226_CannotApplyBaseType.Id));
		Assert.True(runResult.Compare(expected));
	}

	[Fact]
	public void Warning_When_CopiesBaseType_And_IsStruct()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.BaseType})]
partial struct Test
{{
}}

class Target : System.Exception
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
$@"internal partial struct Test
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		SingleGeneratorTestResult runResult = RunGenerator(input);

		Assert.True(runResult.SucceededAndContainsDiagnostics(DUR0226_CannotApplyBaseType.Id));
		Assert.True(runResult.Compare(expected));
	}

	[Fact]
	public void Warning_When_CopiesConstraints_And_AlreadyHasConstraints()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target<T>"", {CopyFromTypeAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.Constraints})]
[{PatternAttributeProvider.TypeName}(""TType"", ""T"")]
partial class Test<T> where T : class
{{
}}

class Target<T> where T : struct
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
$@"internal partial class Test<T>
{{
	{GetCodeGenerationAttributes("Target<T>.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		SingleGeneratorTestResult runResult = RunGenerator(input);

		Assert.True(runResult.SucceededAndContainsDiagnostics(DUR0223_MemberAlreadyHasConstraints.Id));
		Assert.True(runResult.Compare(expected));
	}

	[Fact]
	public void Warning_When_CopiesConstraints_And_IsNotGeneric()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target<T>"", {CopyFromTypeAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.Constraints})]
[{PatternAttributeProvider.TypeName}(""TType"", ""T"")]
partial class Test
{{
}}

class Target<T> where T : struct
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
$@"internal partial class Test
{{
	{GetCodeGenerationAttributes("Target<T>.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		SingleGeneratorTestResult runResult = RunGenerator(input);

		Assert.True(runResult.SucceededAndContainsDiagnostics(DUR0224_CannotCopyConstraintsForMethodOrNonGenericMember.Id));
		Assert.True(runResult.Compare(expected));
	}

	[Fact]
	public void Warning_When_CopiesDocumentation_And_AlreadyHasDocumentation()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

/// <summary>
/// hello there
/// <summary>
[{CopyFromTypeAttributeProvider.TypeName}(""Target"", {CopyFromTypeAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.Documentation})]
partial class Test
{{
}}

/// <summary>
/// ABC
/// </summary>
class Target
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
$@"internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		SingleGeneratorTestResult runResult = RunGenerator(input);

		Assert.True(runResult.SucceededAndContainsDiagnostics(DUR0222_MemberAlreadyHasDocumentation.Id));
		Assert.True(runResult.Compare(expected, true));
	}

	[Fact]
	public void Warning_When_HasPattern_And_CopyFromIsOnDifferentPartialDeclaration()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"")]
partial class Test
{{
}}

[{PatternAttributeProvider.TypeName}(""A"", ""B"")]
partial class Test
{{
}}

class Target
{{
	void A()
	{{
		A();
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.A()")}
	void A()
	{{
		A();
	}}
}}
";
		SingleGeneratorTestResult runResult = RunGenerator(input);

		Assert.True(runResult.SucceededAndContainsDiagnostics(DUR0219_PatternOnDifferentDeclaration.Id));
		Assert.True(runResult.Compare(expected));
	}

	[Fact]
	public void Warning_When_HasPattern_And_PatternIsNull()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target""), {PatternAttributeProvider.TypeName}(null, """")]
partial class Test
{{
}}

class Target
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		SingleGeneratorTestResult runResult = RunGenerator(input);

		Assert.True(runResult.SucceededAndContainsDiagnostics(DUR0214_InvalidPatternAttributeSpecified.Id));
		Assert.True(runResult.Compare(expected));
	}

	[Fact]
	public void Warning_When_HasPattern_And_ReplacementIsNull()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target""), {PatternAttributeProvider.TypeName}(""\w+"", null)]
partial class Test
{{
}}

class Target
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		SingleGeneratorTestResult runResult = RunGenerator(input);

		Assert.True(runResult.SucceededAndContainsDiagnostics(DUR0214_InvalidPatternAttributeSpecified.Id));
		Assert.True(runResult.Compare(expected));
	}

	[Fact]
	public void Warning_When_IdenticalAttributesSpecified()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(Target))]
[{CopyFromTypeAttributeProvider.TypeName}(typeof(Target))]
partial class Test
{{
}}

class Target
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
 $@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		MultipleGeneratorTestResult runResult = RunGeneratorWithMultipleOutputs(input);

		Assert.True(runResult.SucceededAndContainsDiagnostics(DUR0206_EquivalentTarget.Id));
		Assert.True(runResult.Compare(expected));
	}

	[Fact]
	public void Warning_When_SourceAndSourceTypeOfTwoAttributesPointToTheSameMember()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(Target))]
[{CopyFromTypeAttributeProvider.TypeName}(""Target"")]
partial class Test
{{
}}

class Target
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		MultipleGeneratorTestResult runResult = RunGeneratorWithMultipleOutputs(input);

		Assert.True(runResult.SucceededAndContainsDiagnostics(DUR0206_EquivalentTarget.Id));
		Assert.True(runResult.Compare(expected));
	}

	[Fact]
	public void Warning_When_TwoAttributesHaveSameTarget_And_OneHasPartialPart()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"")]
[{CopyFromTypeAttributeProvider.TypeName}(""Target"", PartialPart = ""A"")]
partial class Test
{{
}}

[{nameof(PartialNameAttribute)}(""A"")]
partial class Target
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		MultipleGeneratorTestResult runResult = RunGeneratorWithMultipleOutputs(input);

		Assert.True(runResult.SucceededAndContainsDiagnostics(DUR0206_EquivalentTarget.Id));
		Assert.True(runResult.Compare(expected));
	}

	[Fact]
	public void Warning_When_TwoAttributesHaveSameTarget_And_SamePartialPart()
	{
		string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"", PartialPart = ""A"")]
[{CopyFromTypeAttributeProvider.TypeName}(""Target"", PartialPart = ""A"")]
partial class Test
{{
}}

[{nameof(PartialNameAttribute)}(""A"")]
partial class Target
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Method()")}
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
		MultipleGeneratorTestResult runResult = RunGeneratorWithMultipleOutputs(input);

		Assert.True(runResult.SucceededAndContainsDiagnostics(DUR0206_EquivalentTarget.Id));
		Assert.True(runResult.Compare(expected));
	}
}
