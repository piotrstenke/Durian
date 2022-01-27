// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Durian.Configuration;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Emit;
using Xunit;
using static Durian.Analysis.FriendClass.FriendClassDiagnostics;

namespace Durian.Analysis.FriendClass.Tests
{
	public class DeclarationTests : AnalyzerTest<FriendClassDeclarationAnalyzer>
	{
		[Fact]
		public async Task Error_When_FriendIsInDifferentAssembly()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

[{nameof(FriendClassAttribute)}(typeof(External))]
class Test
{{
	internal static string Name {{ get; }}
}}
";

			string external =
@"public class External
{
}
";
			CSharpCompilation dependency = RoslynUtilities
				.CreateBaseCompilation()
				.AddSyntaxTrees(CSharpSyntaxTree.ParseText(external, encoding: Encoding.UTF8));

			using MemoryStream stream = new();

			EmitResult emit = dependency.Emit(stream);

			if (!emit.Success)
			{
				throw new InvalidOperationException("Emit failed!");
			}

			MetadataReference reference = MetadataReference.CreateFromImage(stream.ToArray());
			CSharpCompilation current = RoslynUtilities
				.CreateBaseCompilation()
				.AddSyntaxTrees(CSharpSyntaxTree.ParseText(input, encoding: Encoding.UTF8))
				.AddReferences(reference);

			FriendClassDeclarationAnalyzer analyzer = new();

			AnalysisResult result = await current
				.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer))
				.GetAnalysisResultAsync(default);

			Assert.Contains(result.GetAllDiagnostics(analyzer), d => d.Id == DUR0301_TargetTypeIsOutsideOfAssembly.Id);
		}

		[Fact]
		public async Task Error_When_FriendIsNotNamedType()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

[{nameof(FriendClassAttribute)}(typeof(string[]))]
class Test
{{
	internal static string Name {{ get; }}
}}
";
			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0308_TypeIsNotValid.Id);
		}

		[Fact]
		public async Task Error_When_FriendIsNull()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

[{nameof(FriendClassAttribute)}(null)]
class Test
{{
	internal static string Name {{ get; }}
}}
";
			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0308_TypeIsNotValid.Id);
		}

		[Fact]
		public async Task Error_When_FriendIsTheSameType()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{nameof(FriendClassAttribute)}(typeof(Test))]
class Test
{{
	internal static string Name {{ get; }}
}}
";
			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0309_TypeCannotBeFriendOfItself.Id);
		}

		[Fact]
		public async Task Success_When_ChildClassIsFriend()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{nameof(FriendClassAttribute)}(typeof(Child))]
class Test
{{
	internal static string Name {{ get; }}
}}

class Child : Test
{{
}}
";
			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task Success_When_HasMultipleFriendTypes()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{nameof(FriendClassAttribute)}(typeof(Other))]
[{nameof(FriendClassAttribute)}(typeof(Another))]
class Test
{{
	internal static string Name {{ get; }}
}}

class Other
{{
}}

class Another
{{
}}
";
			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task Success_When_IsFriendOfFriend()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{nameof(FriendClassAttribute)}(typeof(Child))]
class Test
{{
	internal static string Name {{ get; }}
}}

[{nameof(FriendClassAttribute)}(typeof(Test))]
class Child
{{
	internal static string Name {{ get; }}
}}
";
			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task Success_When_UsesConfigurationWithNoExplicitFriends_And_TargetChildrenAreFriends()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{nameof(FriendClassConfigurationAttribute)}({nameof(FriendClassConfigurationAttribute.AllowsChildren)} = true)]
class Test
{{
	internal static string Name {{ get; }}
}}
";
			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task Warning_When_AllowChildrenOnSealedClass()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{nameof(FriendClassAttribute)}(typeof(Other))]
[{nameof(FriendClassConfigurationAttribute)}({nameof(FriendClassConfigurationAttribute.AllowsChildren)} = true)]
sealed class Test
{{
	internal static string Name {{ get; }}
}}

class Other
{{
}}
";
			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0311_DoNotAllowChildrenOnSealedType.Id);
		}

		[Fact]
		public async Task Warning_When_AllowChildrenOnStaticClass()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{nameof(FriendClassAttribute)}(typeof(Other))]
[{nameof(FriendClassConfigurationAttribute)}({nameof(FriendClassConfigurationAttribute.AllowsChildren)} = true)]
static class Test
{{
	internal static string Name {{ get; }}
}}

class Other
{{
}}
";
			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0311_DoNotAllowChildrenOnSealedType.Id);
		}

		[Fact]
		public async Task Warning_When_AllowChildrenOnStruct()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{nameof(FriendClassAttribute)}(typeof(Other))]
[{nameof(FriendClassConfigurationAttribute)}({nameof(FriendClassConfigurationAttribute.AllowsChildren)} = true)]
struct Test
{{
	internal static string Name {{ get; }}
}}

class Other
{{
}}
";
			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0311_DoNotAllowChildrenOnSealedType.Id);
		}

		[Fact]
		public async Task Warning_When_ConfigurationValuesAreDefault()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{nameof(FriendClassAttribute)}(typeof(Other))]
[{nameof(FriendClassConfigurationAttribute)}({nameof(FriendClassConfigurationAttribute.AllowsChildren)} = false)]
class Test
{{
	internal static string Name {{ get; }}
}}

class Other
{{
}}
";
			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0313_ConfigurationIsRedundant.Id);
		}

		[Fact]
		public async Task Warning_When_FriendTypeSpecifiedMoreThanOnce()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{nameof(FriendClassAttribute)}(typeof(Other))]
[{nameof(FriendClassAttribute)}(typeof(Other))]
class Test
{{
	internal static string Name {{ get; }}
}}

class Other
{{
}}
";
			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0306_FriendTypeSpecifiedByMultipleAttributes.Id);
		}

		[Fact]
		public async Task Warning_When_HasNoInternalMembers()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

[{nameof(FriendClassAttribute)}(typeof(Other))]
class Test
{{
	private static string _name;
	public static string Name {{ get {{ return _name; }} }}
	protected void Method() {{ }}
	private protected class Sub {{ }}
	protected internal class Child : Test {{ }}
}}

class Other
{{
}}
";
			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0305_TypeDoesNotDeclareInternalMembers.Id);
		}

		[Fact]
		public async Task Warning_When_InnerTypeIsFriend()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

[{nameof(FriendClassAttribute)}(typeof(Inner))]
class Test
{{
	internal static string Name {{ get; }}

	class Inner
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0312_InnerTypeIsImplicitFriend.Id);
		}

		[Fact]
		public async Task Warning_When_IsNotVisibleToFriend()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

class Parent
{{
	[{nameof(FriendClassAttribute)}(typeof(Other))]
	private class Test
	{{
		internal static string Name {{ get; }}
	}}
}}

class Other
{{
}}
";
			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0304_ValueOfFriendClassCannotAccessTargetType.Id);
		}

		[Fact]
		public async Task Warning_When_UsesConfigurationWithNoFriends()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{nameof(FriendClassConfigurationAttribute)}({nameof(FriendClassConfigurationAttribute.AllowsChildren)} = false)]
class Test
{{
	internal static string Name {{ get; }}
}}
";
			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0303_DoNotUseFriendClassConfigurationAttributeOnTypesWithNoFriends.Id);
		}
	}
}