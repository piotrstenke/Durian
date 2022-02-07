// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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

[{FriendClassAttributeProvider.TypeName}(typeof(External))]
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
			Assert.Contains(await GetDiagnosticsFromDifferentAssembly(input, external), d => d.Id == DUR0301_TargetTypeIsOutsideOfAssembly.Id);
		}

		[Fact]
		public async Task Error_When_FriendIsNotNamedType()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(string[]))]
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

[{FriendClassAttributeProvider.TypeName}(null)]
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

[{FriendClassAttributeProvider.TypeName}(typeof(Test))]
class Test
{{
	internal static string Name {{ get; }}
}}
";
			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0309_TypeCannotBeFriendOfItself.Id);
		}

		[Fact]
		public async Task Error_When_IncludeInherited_And_DoesNotInherit()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
[{FriendClassConfigurationAttributeProvider.TypeName}({FriendClassConfigurationAttributeProvider.IncludeInherited} = true)]
class Test
{{
	internal static string Name {{ get; }}
}}

class Other
{{
}}
";
			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0315_DoNotAllowInheritedOnTypeWithoutBaseType.Id);
		}

		[Fact]
		public async Task Error_When_IncludeInherited_And_OnlyImplementsInterfaces()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
[{FriendClassConfigurationAttributeProvider.TypeName}({FriendClassConfigurationAttributeProvider.IncludeInherited} = true)]
class Test : IInterface
{{
	internal static string Name {{ get; }}
}}

class Other
{{
}}

interface IInterface
{{
}}
";
			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0315_DoNotAllowInheritedOnTypeWithoutBaseType.Id);
		}

		[Fact]
		public async Task Error_When_IncludeInheritedOnStaticClass()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
[{FriendClassConfigurationAttributeProvider.TypeName}({FriendClassConfigurationAttributeProvider.IncludeInherited} = true)]
static class Test
{{
	internal static string Name {{ get; }}
}}

class Other
{{
}}
";
			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0315_DoNotAllowInheritedOnTypeWithoutBaseType.Id);
		}

		[Fact]
		public async Task Error_When_IncludeInheritedOnStruct()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
[{FriendClassConfigurationAttributeProvider.TypeName}({FriendClassConfigurationAttributeProvider.IncludeInherited} = true)]
struct Test
{{
	internal static string Name {{ get; }}
}}

class Other
{{
}}
";
			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0315_DoNotAllowInheritedOnTypeWithoutBaseType.Id);
		}

		[Fact]
		public async Task Success_When_ChildClassIsFriend()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Child))]
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
		public async Task Success_When_ChildTriesToAccessProtectedInternalMember()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
class Test
{{
	protected internal static string Name {{ get; }}
}}

class Other
{{
}}

class Child : Test
{{
	void M()
	{{
		Test.Name = "";
	}}
}}
";
			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task Success_When_ConfigurationValuesAreDefault()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
[{FriendClassConfigurationAttributeProvider.TypeName}({FriendClassConfigurationAttributeProvider.AllowChildren} = false)]
class Test
{{
	internal static string Name {{ get; }}
}}

class Other
{{
}}
";
			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task Success_When_FriendTriesToAccessProtectedInternalMember()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
class Test
{{
	protected internal static string Name {{ get; }}
}}

class Other
{{
	void M()
	{{
		Test.Name = "";
	}}
}}
";
			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task Success_When_HasMultipleFriendTypes()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
[{FriendClassAttributeProvider.TypeName}(typeof(Another))]
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
		public async Task Success_When_IncludeInherited_And_HasNoInternalMembers_And_ParentHasInternalInstanceMembers()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
[{FriendClassConfigurationAttributeProvider.TypeName}({FriendClassConfigurationAttributeProvider.IncludeInherited} = true)]
class Test : Parent
{{
}}

class Other
{{
}}

class Parent
{{
	internal string Name {{ get; }}
}}
";
			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task Success_When_IncludeInherited_And_ParetIsInDifferentAssembly_And_HasInternalsAttribute()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
[{FriendClassConfigurationAttributeProvider.TypeName}({FriendClassConfigurationAttributeProvider.IncludeInherited} = true)]
class Test : Parent
{{
	internal string Id {{ get; }}
}}

class Other
{{
}}
";
			string external =
$@"using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(""{RoslynUtilities.DefaultCompilationName}"")]
public class Parent
{{
	internal string Name {{ get; }}
}}
";
			Assert.Empty(await GetDiagnosticsFromDifferentAssembly(input, external));
		}

		[Fact]
		public async Task Success_When_IncludeInheriteed_And_HasNoInternalMembers_And_ParentOfParentHasInternalInstanceMembers()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
[{FriendClassConfigurationAttributeProvider.TypeName}({FriendClassConfigurationAttributeProvider.IncludeInherited} = true)]
class Test : Parent
{{
}}

class Other
{{
}}

class Parent : ParentParent
{{
}}

class ParentParent
{{
	internal string Name {{ get; }}
}}
";
			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task Success_When_IsClass_And_TargetsInterface()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(IOther))]
class Test
{{
	internal static string Name {{ get; }}
}}

interface IOther
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

[{FriendClassAttributeProvider.TypeName}(typeof(Child))]
class Test
{{
	internal static string Name {{ get; }}
}}

[{FriendClassAttributeProvider.TypeName}(typeof(Test))]
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

[{FriendClassConfigurationAttributeProvider.TypeName}({FriendClassConfigurationAttributeProvider.AllowChildren} = true)]
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

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
[{FriendClassConfigurationAttributeProvider.TypeName}({FriendClassConfigurationAttributeProvider.AllowChildren} = true)]
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

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
[{FriendClassConfigurationAttributeProvider.TypeName}({FriendClassConfigurationAttributeProvider.AllowChildren} = true)]
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

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
[{FriendClassConfigurationAttributeProvider.TypeName}({FriendClassConfigurationAttributeProvider.AllowChildren} = true)]
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
		public async Task Warning_When_ConfigurationHasNoValues()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
[{FriendClassConfigurationAttributeProvider.TypeName}]
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

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
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

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
class Test
{{
	private static string _name;
	public static string Name {{ get {{ return _name; }} }}
	protected void Method() {{ }}
	private protected class Sub {{ }}
}}

class Other
{{
}}
";
			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0305_TypeDoesNotDeclareInternalMembers.Id);
		}

		[Fact]
		public async Task Warning_When_HasNoInternalMembers_And_InheritsInternalMembers()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
class Test : Parent
{{
}}

class Parent
{{
	internal string Name {{ get; }}
}}

class Other
{{
}}
";
			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0305_TypeDoesNotDeclareInternalMembers.Id);
		}

		[Fact]
		public async Task Warning_When_IncludeInherited_And_InheritedDoesNotHaveInternalMembers()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
[{FriendClassConfigurationAttributeProvider.TypeName}({FriendClassConfigurationAttributeProvider.IncludeInherited} = true)]
class Test : Parent
{{
	internal string Id {{ get; }}
}}

class Other
{{
}}

class Parent
{{
}}
";
			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0316_BaseTypeHasNoInternalInstanceMembers.Id);
		}

		[Fact]
		public async Task Warning_When_IncludeInherited_And_InheritedHasOnlyInternalConstructor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
[{FriendClassConfigurationAttributeProvider.TypeName}({FriendClassConfigurationAttributeProvider.IncludeInherited} = true)]
class Test : Parent
{{
	internal string Id {{ get; }}
}}

class Other
{{
}}

class Parent
{{
	internal Parent()
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0316_BaseTypeHasNoInternalInstanceMembers.Id);
		}

		[Fact]
		public async Task Warning_When_IncludeInherited_And_ParentHasOnlyStaticInternalMembers()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
[{FriendClassConfigurationAttributeProvider.TypeName}({FriendClassConfigurationAttributeProvider.IncludeInherited} = true)]
class Test : Parent
{{
	internal static string A {{ get; }}
}}

class Other
{{
}}

class Parent
{{
	internal static string Name {{ get; }}
}}
";
			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0316_BaseTypeHasNoInternalInstanceMembers.Id);
		}

		[Fact]
		public async Task Warning_When_IncludeInherited_And_ParetIsInDifferentAssembly_And_HasNoInternalsAttribute()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
[{FriendClassConfigurationAttributeProvider.TypeName}({FriendClassConfigurationAttributeProvider.IncludeInherited} = true)]
class Test : Parent
{{
}}

class Other
{{
}}
";
			string external =
$@"public class Parent
{{
	internal string Name {{ get; }}
}}
";
			Assert.Contains(await GetDiagnosticsFromDifferentAssembly(input, external), d => d.Id == DUR0316_BaseTypeHasNoInternalInstanceMembers.Id);
		}

		[Fact]
		public async Task Warning_When_InnerTypeIsFriend()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Inner))]
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
	[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
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

[{FriendClassConfigurationAttributeProvider.TypeName}({FriendClassConfigurationAttributeProvider.AllowChildren} = false)]
class Test
{{
	internal static string Name {{ get; }}
}}
";
			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0303_DoNotUseFriendClassConfigurationAttributeOnTypesWithNoFriends.Id);
		}

		protected override IEnumerable<ISourceTextProvider>? GetInitialSources()
		{
			return FriendClassGenerator.GetSourceProviders();
		}

		private async Task<ImmutableArray<Diagnostic>> GetDiagnosticsFromDifferentAssembly(string input, string external)
		{
			CSharpCompilation dependency = RoslynUtilities
				.CreateBaseCompilation()
				.AddSyntaxTrees(CSharpSyntaxTree.ParseText(external, encoding: Encoding.UTF8));

			AddInitialSources(ref dependency);

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

			AddInitialSources(ref current);

			FriendClassDeclarationAnalyzer analyzer = new();

			AnalysisResult result = await current
				.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer))
				.GetAnalysisResultAsync(default);

			return result.GetAllDiagnostics(analyzer);
		}
	}
}
