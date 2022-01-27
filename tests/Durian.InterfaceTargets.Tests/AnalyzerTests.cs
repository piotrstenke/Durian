// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Threading.Tasks;
using Durian.TestServices;
using Xunit;
using static Durian.Analysis.InterfaceTargets.IntfTargDiagnostics;

namespace Durian.Analysis.InterfaceTargets.Tests
{
	public sealed class AnalyzerTests : AnalyzerTest<IntfTargAnalyzer>
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

[{nameof(InterfaceTargetsAttribute)}({nameof(Durian.InterfaceTargets)}.{nameof(Durian.InterfaceTargets.ReflectionOnly)})]
public interface ITest
{{
}}

public {memberType} Test : ITest
{{
}}
";

			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0403_InterfaceIsNotDirectlyAccessible.Id);
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

[{nameof(InterfaceTargetsAttribute)}({nameof(Durian.InterfaceTargets)}.{nameof(Durian.InterfaceTargets.Class)})]
public interface ITest
{{
}}

public {memberType} Test : ITest
{{
}}
";
			string diag = GetDiagnosticId(memberType);

			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == diag);
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

[{nameof(InterfaceTargetsAttribute)}({nameof(Durian.InterfaceTargets)}.{nameof(Durian.InterfaceTargets.Interface)})]
public interface ITest
{{
}}

public {memberType} Test : ITest
{{
}}
";
			string diag = GetDiagnosticId(memberType);

			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == diag);
		}

		[Fact]
		public async Task Error_When_IsValidOnMultipleMemberTypes_And_IsNotOneOfTypes()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

[{nameof(InterfaceTargetsAttribute)}(
	{nameof(Durian.InterfaceTargets)}.{nameof(Durian.InterfaceTargets.Class)} |
	{nameof(Durian.InterfaceTargets)}.{nameof(Durian.InterfaceTargets.Struct)})]
public interface ITest
{{
}}

public record Test : ITest
{{
}}
";
			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0401_InterfaceCannotBeImplementedByMembersOfThisKind.Id);
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

[{nameof(InterfaceTargetsAttribute)}({nameof(Durian.InterfaceTargets)}.{nameof(Durian.InterfaceTargets.RecordClass)})]
public interface ITest
{{
}}

public {memberType} Test : ITest
{{
}}
";
			string diag = GetDiagnosticId(memberType);

			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == diag);
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

[{nameof(InterfaceTargetsAttribute)}({nameof(Durian.InterfaceTargets)}.{nameof(Durian.InterfaceTargets.RecordStruct)})]
public interface ITest
{{
}}

public {memberType} Test : ITest
{{
}}
";
			string diag = GetDiagnosticId(memberType);

			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == diag);
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

[{nameof(InterfaceTargetsAttribute)}({nameof(Durian.InterfaceTargets)}.{nameof(Durian.InterfaceTargets.Struct)})]
public interface ITest
{{
}}

public {memberType} Test : ITest
{{
}}
";
			string diag = GetDiagnosticId(memberType);

			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == diag);
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

[{nameof(InterfaceTargetsAttribute)}({nameof(Durian.InterfaceTargets)}.{nameof(Durian.InterfaceTargets.None)})]
public interface ITest
{{
}}

public {memberType} Test : ITest
{{
}}
";

			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0403_InterfaceIsNotDirectlyAccessible.Id);
		}

		[Fact]
		public async Task Success_When_IsValidOnClass_And_TargetIsClass()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

[{nameof(InterfaceTargetsAttribute)}({nameof(Durian.InterfaceTargets)}.{nameof(Durian.InterfaceTargets.Class)})]
public interface ITest
{{
}}

public class Test : ITest
{{
}}
";
			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task Success_When_IsValidOnInterface_And_TargetIsInterface()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

[{nameof(InterfaceTargetsAttribute)}({nameof(Durian.InterfaceTargets)}.{nameof(Durian.InterfaceTargets.Interface)})]
public interface ITest
{{
}}

public interface Test : ITest
{{
}}
";
			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task Success_When_IsValidOnMultipleMemberTypes_And_IsOneOfTypes()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

[{nameof(InterfaceTargetsAttribute)}(
	{nameof(Durian.InterfaceTargets)}.{nameof(Durian.InterfaceTargets.Class)} |
	{nameof(Durian.InterfaceTargets)}.{nameof(Durian.InterfaceTargets.Struct)})]
public interface ITest
{{
}}

public struct Test : ITest
{{
}}
";
			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task Success_When_IsValidOnRecordClass_And_TargetIsRecorClass()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

[{nameof(InterfaceTargetsAttribute)}({nameof(Durian.InterfaceTargets)}.{nameof(Durian.InterfaceTargets.RecordClass)})]
public interface ITest
{{
}}

public record Test : ITest
{{
}}
";
			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task Success_When_IsValidOnRecordStruct_And_TargetIsRecordStruct()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

[{nameof(InterfaceTargetsAttribute)}({nameof(Durian.InterfaceTargets)}.{nameof(Durian.InterfaceTargets.RecordStruct)})]
public interface ITest
{{
}}

public record struct Test : ITest
{{
}}
";
			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task Success_When_IsValidOnStruct_And_TargetIsStruct()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

[{nameof(InterfaceTargetsAttribute)}({nameof(Durian.InterfaceTargets)}.{nameof(Durian.InterfaceTargets.Struct)})]
public interface ITest
{{
}}

public struct Test : ITest
{{
}}
";
			Assert.Empty(await RunAnalyzerAsync(input));
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
}