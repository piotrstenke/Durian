// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Threading.Tasks;
using Durian.TestServices;
using Xunit;
using static Durian.Analysis.FriendClass.FriendClassDiagnostics;

namespace Durian.Analysis.FriendClass.Tests
{
	public class AccessTests : AnalyzerTest<FriendClassAccessAnalyzer>
	{
		[Fact]
		public async Task Error_When_ChildClassOfFriendTriesToAccessMember_And_DoesNotAllowChildrenOfFriends()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other), {FriendClassAttributeProvider.AllowsFriendChildren} = false)]
class Test
{{
	internal static string Name {{ get; }}
}}

class Other
{{
}}

class Child : Other
{{
	void Main()
	{{
		string name = Test.Name;
	}}
}}
";
			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0310_MemberCannotBeAccessedByChildClassOfFriend.Id);
		}

		[Fact]
		public async Task Error_When_ChildClassTriesToAccessMember_And_DoesNotAllowChildrenAccessOfInternals()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{FriendClassConfigurationAttributeProvider.TypeName}({FriendClassConfigurationAttributeProvider.AllowsChildren} = false)]
[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
class Test
{{
	internal static string Name {{ get; }}
}}

class Other
{{
}}

class Child : Test
{{
	void Main()
	{{
		string name = Test.Name;
	}}
}}
";
			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0307_MemberCannotBeAccessedByChildClass.Id);
		}

		[Fact]
		public async Task Error_When_TriesToAccessInternalMember_And_IsNotFriend()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Child))]
class Test
{{
	internal static string Name {{ get; }}
}}

class Child
{{
	void Main()
	{{
		string name = Test.Name;
	}}
}}

class Other
{{
	void Main()
	{{
		string name = Test.Name;
	}}
}}
";
			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0302_MemberCannotBeAccessedOutsideOfFriendClass.Id);
		}

		[Fact]
		public async Task Success_When_ChildClassOfFriendTriesToAccessMember_And_AllowsChildrenOfFriends()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other), {FriendClassAttributeProvider.AllowsFriendChildren} = true)]
class Test
{{
	internal static string Name {{ get; }}
}}

class Other
{{
	void Main()
	{{
		string name = Test.Name;
	}}
}}

class Child : Other
{{
	void Main()
	{{
		string name = Test.Name;
	}}
}}
";
			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task Success_When_ChildClassTriesToAccessMember_And_AllowsChildrenAccessOfInternals()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{FriendClassConfigurationAttributeProvider.TypeName}({FriendClassConfigurationAttributeProvider.AllowsChildren} = true)]
[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
class Test
{{
	internal static string Name {{ get; }}
}}

class Other
{{
	void Main()
	{{
		string name = Test.Name;
	}}
}}

class Child : Test
{{
	void Main()
	{{
		string name = Test.Name;
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
	void Main()
	{{
		string name = Test.Name;
	}}
}}

class Another
{{
	void Main()
	{{
		string name = Test.Name;
	}}
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

	void Main()
	{{
		string name = Child.Name;
	}}
}}

[{FriendClassAttributeProvider.TypeName}(typeof(Test))]
class Child
{{
	internal static string Name {{ get; }}

	void Main()
	{{
		string name = Test.Name;
	}}
}}
";
			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task Success_When_TriesToAccessInternalMember_And_IsFriend()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
class Test
{{
	internal static string Name {{ get; }}
}}

class Other
{{
	void Main()
	{{
		string name = Test.Name;
	}}
}}
";
			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task Success_When_TriesToAccessInternalMember_And_OuterTypeIsFriend()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
class Test
{{
	internal static string Name {{ get; }}
}}

class Other
{{
	class Inner
	{{
		void Main()
		{{
			string name = Test.Name;
		}}
	}}
}}
";
			Assert.Empty(await RunAnalyzerAsync(input));
		}

		protected override IEnumerable<ISourceTextProvider>? GetInitialSources()
		{
			return FriendClassGenerator.GetSourceProviders();
		}
	}
}