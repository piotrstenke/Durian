using System.Collections.Generic;
using System.Threading.Tasks;
using Durian.TestServices;
using Xunit;
using static Durian.Analysis.InterfaceTargets.InterfaceTargetsDiagnostics;

namespace Durian.Analysis.InterfaceTargets.Tests;

public sealed class AnalyzerTests : AnalyzerTest<InterfaceTargetsAnalyzer>
{
	[Theory]
	[InlineData("class")]
	[InlineData("interface")]
	[InlineData("struct")]
	[InlineData("record")]
	[InlineData("record struct")]
	public async Task Error_When_IsReflectionOnly(string memberType)
	{
		string input =
@$"using {DurianStrings.MainNamespace};

[{InterfaceTargetsAttributeProvider.TypeName}({InterfaceTargetsProvider.TypeName}.{InterfaceTargetsProvider.ReflectionOnly})]
public interface ITest
{{
}}

public {memberType} Test : ITest
{{
}}
";

		Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0403_InterfaceIsNotDirectlyAccessible.Id);
	}

	[Theory]
	[InlineData("interface")]
	[InlineData("struct")]
	[InlineData("record")]
	[InlineData("record struct")]
	public async Task Error_When_IsValidOnClass_And_TargetIsNotClass(string memberType)
	{
		string input =
@$"using {DurianStrings.MainNamespace};

[{InterfaceTargetsAttributeProvider.TypeName}({InterfaceTargetsProvider.TypeName}.{InterfaceTargetsProvider.Class})]
public interface ITest
{{
}}

public {memberType} Test : ITest
{{
}}
";
		string diag = GetDiagnosticId(memberType);

		Assert.Contains(await RunAnalyzer(input), d => d.Id == diag);
	}

	[Theory]
	[InlineData("class")]
	[InlineData("struct")]
	[InlineData("record")]
	[InlineData("record struct")]
	public async Task Error_When_IsValidOnInterface_And_TargetIsNotInterface(string memberType)
	{
		string input =
@$"using {DurianStrings.MainNamespace};

[{InterfaceTargetsAttributeProvider.TypeName}({InterfaceTargetsProvider.TypeName}.{InterfaceTargetsProvider.Interface})]
public interface ITest
{{
}}

public {memberType} Test : ITest
{{
}}
";
		string diag = GetDiagnosticId(memberType);

		Assert.Contains(await RunAnalyzer(input), d => d.Id == diag);
	}

	[Fact]
	public async Task Error_When_IsValidOnMultipleMemberTypes_And_IsNotOneOfTypes()
	{
		string input =
@$"using {DurianStrings.MainNamespace};

[{InterfaceTargetsAttributeProvider.TypeName}(
	{InterfaceTargetsProvider.TypeName}.{InterfaceTargetsProvider.Class} |
	{InterfaceTargetsProvider.TypeName}.{InterfaceTargetsProvider.Struct})]
public interface ITest
{{
}}

public record Test : ITest
{{
}}
";
		Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0401_InterfaceCannotBeImplementedByMembersOfThisKind.Id);
	}

	[Theory]
	[InlineData("class")]
	[InlineData("struct")]
	[InlineData("interface")]
	[InlineData("record struct")]
	public async Task Error_When_IsValidOnRecordClass_And_TargetIsNotRecordClass(string memberType)
	{
		string input =
@$"using {DurianStrings.MainNamespace};

[{InterfaceTargetsAttributeProvider.TypeName}({InterfaceTargetsProvider.TypeName}.{InterfaceTargetsProvider.RecordClass})]
public interface ITest
{{
}}

public {memberType} Test : ITest
{{
}}
";
		string diag = GetDiagnosticId(memberType);

		Assert.Contains(await RunAnalyzer(input), d => d.Id == diag);
	}

	[Theory]
	[InlineData("struct")]
	[InlineData("class")]
	[InlineData("interface")]
	[InlineData("record")]
	public async Task Error_When_IsValidOnRecordStruct_And_TargetIsNotRecordStruct(string memberType)
	{
		string input =
@$"using {DurianStrings.MainNamespace};

[{InterfaceTargetsAttributeProvider.TypeName}({InterfaceTargetsProvider.TypeName}.{InterfaceTargetsProvider.RecordStruct})]
public interface ITest
{{
}}

public {memberType} Test : ITest
{{
}}
";
		string diag = GetDiagnosticId(memberType);

		Assert.Contains(await RunAnalyzer(input), d => d.Id == diag);
	}

	[Theory]
	[InlineData("class")]
	[InlineData("interface")]
	[InlineData("record")]
	[InlineData("record struct")]
	public async Task Error_When_IsValidOnStruct_And_TargetIsNotStruct(string memberType)
	{
		string input =
@$"using {DurianStrings.MainNamespace};

[{InterfaceTargetsAttributeProvider.TypeName}({InterfaceTargetsProvider.TypeName}.{InterfaceTargetsProvider.Struct})]
public interface ITest
{{
}}

public {memberType} Test : ITest
{{
}}
";
		string diag = GetDiagnosticId(memberType);

		Assert.Contains(await RunAnalyzer(input), d => d.Id == diag);
	}

	[Theory]
	[InlineData("class")]
	[InlineData("interface")]
	[InlineData("struct")]
	[InlineData("record")]
	[InlineData("record struct")]
	public async Task NoneHasSameEffectAsReflectionOnly(string memberType)
	{
		string input =
@$"using {DurianStrings.MainNamespace};

[{InterfaceTargetsAttributeProvider.TypeName}({InterfaceTargetsProvider.TypeName}.{InterfaceTargetsProvider.None})]
public interface ITest
{{
}}

public {memberType} Test : ITest
{{
}}
";

		Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0403_InterfaceIsNotDirectlyAccessible.Id);
	}

	[Theory]
	[InlineData(InterfaceTargetsProvider.Class, "class")]
	[InlineData(InterfaceTargetsProvider.Class, "notnull")]
	[InlineData(InterfaceTargetsProvider.Class, "new()")]
	[InlineData(InterfaceTargetsProvider.Class, "Other")]
	[InlineData(InterfaceTargetsProvider.RecordClass, "class")]
	[InlineData(InterfaceTargetsProvider.RecordClass, "notnull")]
	[InlineData(InterfaceTargetsProvider.RecordClass, "new()")]
	[InlineData(InterfaceTargetsProvider.RecordClass, "OtherRecord")]
	[InlineData(InterfaceTargetsProvider.Interface, "class")]
	[InlineData(InterfaceTargetsProvider.Interface, "notnull")]
	[InlineData(InterfaceTargetsProvider.Interface, "new()")]
	[InlineData(InterfaceTargetsProvider.Interface, "Other")]
	[InlineData(InterfaceTargetsProvider.Interface, "OtherRecord")]
	[InlineData(InterfaceTargetsProvider.ReflectionOnly, "class")]
	[InlineData(InterfaceTargetsProvider.ReflectionOnly, "notnull")]
	[InlineData(InterfaceTargetsProvider.ReflectionOnly, "new()")]
	public async Task Success_When_ChainConstraintsToValidTypeParameter(string target, string constraint)
	{
		string input =
@$"using {DurianStrings.MainNamespace};

[{InterfaceTargetsAttributeProvider.TypeName}({InterfaceTargetsProvider.TypeName}.{target})]
public interface ITest
{{
}}

public class Other<T, U, V> where T : {constraint} where U : T where V : U, ITest
{{
}}

public class Other
{{
}}

public record OtherRecord
{{
}}
";
		Assert.Empty(await RunAnalyzer(input));
	}

	[Theory]
	[InlineData(InterfaceTargetsProvider.Class, InterfaceTargetsProvider.Class)]
	[InlineData(InterfaceTargetsProvider.Class, InterfaceTargetsProvider.Interface)]
	[InlineData(InterfaceTargetsProvider.Class, InterfaceTargetsProvider.ReflectionOnly)]
	[InlineData(InterfaceTargetsProvider.RecordClass, InterfaceTargetsProvider.RecordClass)]
	[InlineData(InterfaceTargetsProvider.RecordClass, InterfaceTargetsProvider.Interface)]
	[InlineData(InterfaceTargetsProvider.RecordClass, InterfaceTargetsProvider.ReflectionOnly)]
	[InlineData(InterfaceTargetsProvider.Struct, InterfaceTargetsProvider.Struct)]
	[InlineData(InterfaceTargetsProvider.Struct, InterfaceTargetsProvider.Interface)]
	[InlineData(InterfaceTargetsProvider.Struct, InterfaceTargetsProvider.ReflectionOnly)]
	[InlineData(InterfaceTargetsProvider.RecordStruct, InterfaceTargetsProvider.RecordStruct)]
	[InlineData(InterfaceTargetsProvider.RecordStruct, InterfaceTargetsProvider.Interface)]
	[InlineData(InterfaceTargetsProvider.RecordStruct, InterfaceTargetsProvider.ReflectionOnly)]
	[InlineData(InterfaceTargetsProvider.Interface, InterfaceTargetsProvider.Class)]
	[InlineData(InterfaceTargetsProvider.Interface, InterfaceTargetsProvider.RecordClass)]
	[InlineData(InterfaceTargetsProvider.Interface, InterfaceTargetsProvider.Struct)]
	[InlineData(InterfaceTargetsProvider.Interface, InterfaceTargetsProvider.RecordStruct)]
	[InlineData(InterfaceTargetsProvider.Interface, InterfaceTargetsProvider.Interface)]
	[InlineData(InterfaceTargetsProvider.Interface, InterfaceTargetsProvider.ReflectionOnly)]
	[InlineData(InterfaceTargetsProvider.ReflectionOnly, InterfaceTargetsProvider.Class)]
	[InlineData(InterfaceTargetsProvider.ReflectionOnly, InterfaceTargetsProvider.RecordClass)]
	[InlineData(InterfaceTargetsProvider.ReflectionOnly, InterfaceTargetsProvider.Struct)]
	[InlineData(InterfaceTargetsProvider.ReflectionOnly, InterfaceTargetsProvider.RecordStruct)]
	[InlineData(InterfaceTargetsProvider.ReflectionOnly, InterfaceTargetsProvider.Interface)]
	[InlineData(InterfaceTargetsProvider.ReflectionOnly, InterfaceTargetsProvider.ReflectionOnly)]
	public async Task Success_When_ConstraintCanMatchOtherInterface(string firstTarget, string secondTarget)
	{
		string input =
@$"using {DurianStrings.MainNamespace};

[{InterfaceTargetsAttributeProvider.TypeName}({InterfaceTargetsProvider.TypeName}.{firstTarget})]
public interface ITest
{{
}}

public class Other<T> where T : ITest, IOther
{{
}}

[{InterfaceTargetsAttributeProvider.TypeName}({InterfaceTargetsProvider.TypeName}.{secondTarget})]
public interface IOther
{{
}}
";
		Assert.Empty(await RunAnalyzer(input));
	}

	[Theory]
	[InlineData(InterfaceTargetsProvider.Class, "class")]
	[InlineData(InterfaceTargetsProvider.Class, "notnull")]
	[InlineData(InterfaceTargetsProvider.Class, "new()")]
	[InlineData(InterfaceTargetsProvider.Class, "Other")]
	[InlineData(InterfaceTargetsProvider.RecordClass, "class")]
	[InlineData(InterfaceTargetsProvider.RecordClass, "notnull")]
	[InlineData(InterfaceTargetsProvider.RecordClass, "new()")]
	[InlineData(InterfaceTargetsProvider.RecordClass, "OtherRecord")]
	[InlineData(InterfaceTargetsProvider.Struct, "notnull")]
	[InlineData(InterfaceTargetsProvider.Struct, "new()")]
	[InlineData(InterfaceTargetsProvider.RecordStruct, "notnull")]
	[InlineData(InterfaceTargetsProvider.RecordStruct, "new()")]
	[InlineData(InterfaceTargetsProvider.Interface, "class")]
	[InlineData(InterfaceTargetsProvider.Interface, "notnull")]
	[InlineData(InterfaceTargetsProvider.Interface, "new()")]
	[InlineData(InterfaceTargetsProvider.Interface, "Other")]
	[InlineData(InterfaceTargetsProvider.Interface, "OtherRecord")]
	[InlineData(InterfaceTargetsProvider.ReflectionOnly, "class")]
	[InlineData(InterfaceTargetsProvider.ReflectionOnly, "notnull")]
	[InlineData(InterfaceTargetsProvider.ReflectionOnly, "new()")]
	public async Task Success_When_ConstraintsToValidTypeParameter(string target, string constraint)
	{
		string input =
@$"using {DurianStrings.MainNamespace};

[{InterfaceTargetsAttributeProvider.TypeName}({InterfaceTargetsProvider.TypeName}.{target})]
public interface ITest
{{
}}

public class Other<T, U> where T : {constraint} where U : T, ITest
{{
}}

public class Other
{{
}}

public record OtherRecord
{{
}}
";
		Assert.Empty(await RunAnalyzer(input));
	}

	[Theory]
	[InlineData(InterfaceTargetsProvider.Class, "class")]
	[InlineData(InterfaceTargetsProvider.Class, "notnull")]
	[InlineData(InterfaceTargetsProvider.Class, "new()")]
	[InlineData(InterfaceTargetsProvider.Class, "Other")]
	[InlineData(InterfaceTargetsProvider.RecordClass, "class")]
	[InlineData(InterfaceTargetsProvider.RecordClass, "notnull")]
	[InlineData(InterfaceTargetsProvider.RecordClass, "new()")]
	[InlineData(InterfaceTargetsProvider.RecordClass, "OtherRecord")]
	[InlineData(InterfaceTargetsProvider.Struct, "struct")]
	[InlineData(InterfaceTargetsProvider.Struct, "unmanaged")]
	[InlineData(InterfaceTargetsProvider.Struct, "notnull")]
	[InlineData(InterfaceTargetsProvider.Struct, "new()")]
	[InlineData(InterfaceTargetsProvider.RecordStruct, "struct")]
	[InlineData(InterfaceTargetsProvider.RecordStruct, "notnull")]
	[InlineData(InterfaceTargetsProvider.RecordStruct, "new()")]
	[InlineData(InterfaceTargetsProvider.Interface, "class")]
	[InlineData(InterfaceTargetsProvider.Interface, "struct")]
	[InlineData(InterfaceTargetsProvider.Interface, "unmanaged")]
	[InlineData(InterfaceTargetsProvider.Interface, "notnull")]
	[InlineData(InterfaceTargetsProvider.Interface, "new()")]
	[InlineData(InterfaceTargetsProvider.Interface, "Other")]
	[InlineData(InterfaceTargetsProvider.Interface, "OtherRecord")]
	[InlineData(InterfaceTargetsProvider.ReflectionOnly, "class")]
	[InlineData(InterfaceTargetsProvider.ReflectionOnly, "struct")]
	[InlineData(InterfaceTargetsProvider.ReflectionOnly, "unmanaged")]
	[InlineData(InterfaceTargetsProvider.ReflectionOnly, "notnull")]
	[InlineData(InterfaceTargetsProvider.ReflectionOnly, "new()")]
	public async Task Success_When_ConstraintWillMatch(string target, string constraint)
	{
		string input =
@$"using {DurianStrings.MainNamespace};

[{InterfaceTargetsAttributeProvider.TypeName}({InterfaceTargetsProvider.TypeName}.{target})]
public interface ITest
{{
}}

public class Other<T, U> where T : {constraint}, ITest
{{
}}

public class Other
{{
}}

public record OtherRecord
{{
}}
";
		Assert.Empty(await RunAnalyzer(input));
	}

	[Fact]
	public async Task Success_When_HasSingleInterfaceConstraint()
	{
		string input =
@$"using {DurianStrings.MainNamespace};

[{InterfaceTargetsAttributeProvider.TypeName}({InterfaceTargetsProvider.TypeName}.{InterfaceTargetsProvider.Class})]
public interface ITest
{{
}}

public class Other<T> where T : ITest
{{
}}
";
		Assert.Empty(await RunAnalyzer(input));
	}

	[Fact]
	public async Task Success_When_IsValidOnClass_And_TargetIsClass()
	{
		string input =
@$"using {DurianStrings.MainNamespace};

[{InterfaceTargetsAttributeProvider.TypeName}({InterfaceTargetsProvider.TypeName}.{InterfaceTargetsProvider.Class})]
public interface ITest
{{
}}

public class Test : ITest
{{
}}
";
		Assert.Empty(await RunAnalyzer(input));
	}

	[Fact]
	public async Task Success_When_IsValidOnInterface_And_TargetIsInterface()
	{
		string input =
@$"using {DurianStrings.MainNamespace};

[{InterfaceTargetsAttributeProvider.TypeName}({InterfaceTargetsProvider.TypeName}.{InterfaceTargetsProvider.Interface})]
public interface ITest
{{
}}

public interface Test : ITest
{{
}}
";
		Assert.Empty(await RunAnalyzer(input));
	}

	[Fact]
	public async Task Success_When_IsValidOnMultipleMemberTypes_And_IsOneOfTypes()
	{
		string input =
@$"using {DurianStrings.MainNamespace};

[{InterfaceTargetsAttributeProvider.TypeName}(
	{InterfaceTargetsProvider.TypeName}.{InterfaceTargetsProvider.Class} |
	{InterfaceTargetsProvider.TypeName}.{InterfaceTargetsProvider.Struct})]
public interface ITest
{{
}}

public struct Test : ITest
{{
}}
";
		Assert.Empty(await RunAnalyzer(input));
	}

	[Fact]
	public async Task Success_When_IsValidOnRecordClass_And_TargetIsRecordClass()
	{
		string input =
@$"using {DurianStrings.MainNamespace};

[{InterfaceTargetsAttributeProvider.TypeName}({InterfaceTargetsProvider.TypeName}.{InterfaceTargetsProvider.RecordClass})]
public interface ITest
{{
}}

public record Test : ITest
{{
}}
";
		Assert.Empty(await RunAnalyzer(input));
	}

	[Fact]
	public async Task Success_When_IsValidOnRecordStruct_And_TargetIsRecordStruct()
	{
		string input =
@$"using {DurianStrings.MainNamespace};

[{InterfaceTargetsAttributeProvider.TypeName}({InterfaceTargetsProvider.TypeName}.{InterfaceTargetsProvider.RecordStruct})]
public interface ITest
{{
}}

public record struct Test : ITest
{{
}}
";
		Assert.Empty(await RunAnalyzer(input));
	}

	[Fact]
	public async Task Success_When_IsValidOnStruct_And_TargetIsStruct()
	{
		string input =
@$"using {DurianStrings.MainNamespace};

[{InterfaceTargetsAttributeProvider.TypeName}({InterfaceTargetsProvider.TypeName}.{InterfaceTargetsProvider.Struct})]
public interface ITest
{{
}}

public struct Test : ITest
{{
}}
";
		Assert.Empty(await RunAnalyzer(input));
	}

	[Theory]
	[InlineData(InterfaceTargetsProvider.Class, "OtherRecord")]
	[InlineData(InterfaceTargetsProvider.RecordClass, "Other")]
	[InlineData(InterfaceTargetsProvider.Struct, "class")]
	[InlineData(InterfaceTargetsProvider.Struct, "Other")]
	[InlineData(InterfaceTargetsProvider.Struct, "OtherRecord")]
	[InlineData(InterfaceTargetsProvider.RecordStruct, "class")]
	[InlineData(InterfaceTargetsProvider.RecordStruct, "Other")]
	[InlineData(InterfaceTargetsProvider.RecordStruct, "OtherRecord")]
	[InlineData(InterfaceTargetsProvider.ReflectionOnly, "Other")]
	[InlineData(InterfaceTargetsProvider.ReflectionOnly, "OtherRecord")]
	public async Task Warning_When_ChainConstraintsToInvalidTypeParameter(string target, string constraint)
	{
		string input =
@$"using {DurianStrings.MainNamespace};

[{InterfaceTargetsAttributeProvider.TypeName}({InterfaceTargetsProvider.TypeName}.{target})]
public interface ITest
{{
}}

public class Other<T, U, V> where T : {constraint} where U : T where V : U, ITest
{{
}}

public class Other
{{
}}

public record OtherRecord
{{
}}
";
		Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0404_InvalidConstraint.Id);
	}

	[Theory]
	[InlineData(InterfaceTargetsProvider.Class, "OtherRecord")]
	[InlineData(InterfaceTargetsProvider.RecordClass, "Other")]
	[InlineData(InterfaceTargetsProvider.Struct, "class")]
	[InlineData(InterfaceTargetsProvider.Struct, "Other")]
	[InlineData(InterfaceTargetsProvider.Struct, "OtherRecord")]
	[InlineData(InterfaceTargetsProvider.RecordStruct, "class")]
	[InlineData(InterfaceTargetsProvider.RecordStruct, "Other")]
	[InlineData(InterfaceTargetsProvider.RecordStruct, "OtherRecord")]
	[InlineData(InterfaceTargetsProvider.ReflectionOnly, "Other")]
	[InlineData(InterfaceTargetsProvider.ReflectionOnly, "OtherRecord")]
	public async Task Warning_When_ConstraintsToInvalidTypeParameter(string target, string constraint)
	{
		string input =
@$"using {DurianStrings.MainNamespace};

[{InterfaceTargetsAttributeProvider.TypeName}({InterfaceTargetsProvider.TypeName}.{target})]
public interface ITest
{{
}}

public class Other<T, U> where T : {constraint} where U : T, ITest
{{
}}

public class Other
{{
}}

public record OtherRecord
{{
}}
";
		Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0404_InvalidConstraint.Id);
	}

	[Theory]
	[InlineData(InterfaceTargetsProvider.Class, "struct")]
	[InlineData(InterfaceTargetsProvider.Class, "unmanaged")]
	[InlineData(InterfaceTargetsProvider.RecordClass, "struct")]
	[InlineData(InterfaceTargetsProvider.RecordClass, "unmanaged")]
	[InlineData(InterfaceTargetsProvider.RecordClass, "Other")]
	[InlineData(InterfaceTargetsProvider.Struct, "class")]
	[InlineData(InterfaceTargetsProvider.Struct, "Other")]
	[InlineData(InterfaceTargetsProvider.Struct, "OtherRecord")]
	[InlineData(InterfaceTargetsProvider.RecordStruct, "class")]
	[InlineData(InterfaceTargetsProvider.RecordStruct, "unmanaged")]
	[InlineData(InterfaceTargetsProvider.RecordStruct, "Other")]
	[InlineData(InterfaceTargetsProvider.RecordStruct, "OtherRecord")]
	[InlineData(InterfaceTargetsProvider.ReflectionOnly, "Other")]
	[InlineData(InterfaceTargetsProvider.ReflectionOnly, "OtherRecord")]
	public async Task Warning_When_ConstraintWillNeverMatch(string target, string constraint)
	{
		string input =
@$"using {DurianStrings.MainNamespace};

[{InterfaceTargetsAttributeProvider.TypeName}({InterfaceTargetsProvider.TypeName}.{target})]
public interface ITest
{{
}}

public class Other<T> where T : {constraint}, ITest
{{
}}

public class Other
{{
}}

public record OtherRecord
{{
}}
";
		Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0404_InvalidConstraint.Id);
	}

	[Theory]
	[InlineData(InterfaceTargetsProvider.Class, InterfaceTargetsProvider.RecordClass)]
	[InlineData(InterfaceTargetsProvider.Class, InterfaceTargetsProvider.Struct)]
	[InlineData(InterfaceTargetsProvider.Class, InterfaceTargetsProvider.RecordStruct)]
	[InlineData(InterfaceTargetsProvider.RecordClass, InterfaceTargetsProvider.Class)]
	[InlineData(InterfaceTargetsProvider.RecordClass, InterfaceTargetsProvider.Struct)]
	[InlineData(InterfaceTargetsProvider.RecordClass, InterfaceTargetsProvider.RecordStruct)]
	[InlineData(InterfaceTargetsProvider.Struct, InterfaceTargetsProvider.Class)]
	[InlineData(InterfaceTargetsProvider.Struct, InterfaceTargetsProvider.RecordClass)]
	[InlineData(InterfaceTargetsProvider.Struct, InterfaceTargetsProvider.RecordStruct)]
	[InlineData(InterfaceTargetsProvider.RecordStruct, InterfaceTargetsProvider.Class)]
	[InlineData(InterfaceTargetsProvider.RecordStruct, InterfaceTargetsProvider.RecordClass)]
	[InlineData(InterfaceTargetsProvider.RecordStruct, InterfaceTargetsProvider.Struct)]
	public async Task Warning_When_ConstraintWillNeverMatchOtherInterface(string firstTarget, string secondTarget)
	{
		string input =
@$"using {DurianStrings.MainNamespace};

[{InterfaceTargetsAttributeProvider.TypeName}({InterfaceTargetsProvider.TypeName}.{firstTarget})]
public interface ITest
{{
}}

public class Other<T> where T : ITest, IOther
{{
}}

[{InterfaceTargetsAttributeProvider.TypeName}({InterfaceTargetsProvider.TypeName}.{secondTarget})]
public interface IOther
{{
}}
";
		Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0404_InvalidConstraint.Id);
	}

	protected override IEnumerable<ISourceTextProvider>? GetInitialSources()
	{
		return InterfaceTargetsGenerator.GetSourceProviders();
	}

	private static string GetDiagnosticId(string memberType)
	{
		if (memberType == "interface")
		{
			return DUR0402_InterfaceCannotBeBaseOfAnotherInterface.Id;
		}

		return DUR0401_InterfaceCannotBeImplementedByMembersOfThisKind.Id;
	}
}
