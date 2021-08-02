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

		[Theory]
		[InlineData("class")]
		[InlineData("struct")]
		[InlineData("interface")]
		public async Task Error_When_IsValidOnRecord_And_TargetIsNotRecord(string memberType)
		{
			string input =
@$"using {DurianStrings.MainNamespace};

[{nameof(InterfaceTargetsAttribute)}({nameof(Durian.InterfaceTargets)}.{nameof(Durian.InterfaceTargets.Record)})]
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
		public async Task Success_When_IsValidOnRecord_And_TargetIsRecord()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

[{nameof(InterfaceTargetsAttribute)}({nameof(Durian.InterfaceTargets)}.{nameof(Durian.InterfaceTargets.Record)})]
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
