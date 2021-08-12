// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Xunit;
using Durian.TestServices;
using Durian.Configuration;
using System.Threading.Tasks;
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

[{nameof(FriendClassAttribute)}(typeof(Other), {nameof(FriendClassAttribute.AllowsFriendChildren)} = false)]
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
			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0311_MemberCannotBeAccessedByChildClassOfFriend.Id);
		}

		[Fact]
		public async Task Error_When_ChildClassTriesToAccessMember_And_DoesNotAllowChildrenAccessOfInternals()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{nameof(FriendClassConfigurationAttribute)}({nameof(FriendClassConfigurationAttribute.AllowsChildren)} = false)]
[{nameof(FriendClassAttribute)}(typeof(Other))]
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

[{nameof(FriendClassAttribute)}(typeof(Child))]
class Test
{{
	internal static string Name {{ get; }}
}}

class Child
{{
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

[{nameof(FriendClassAttribute)}(typeof(Other), {nameof(FriendClassAttribute.AllowsFriendChildren)} = true)]
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
			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task Success_When_ChildClassTriesToAccessMember_And_AllowsChildrenAccessOfInternals()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{nameof(FriendClassConfigurationAttribute)}({nameof(FriendClassConfigurationAttribute.AllowsChildren)} = true)]
[{nameof(FriendClassAttribute)}(typeof(Other))]
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

[{nameof(FriendClassAttribute)}(typeof(Child))]
class Test
{{
	internal static string Name {{ get; }}

	void Main()
	{{
		string name = Child.Name;
	}}
}}

[{nameof(FriendClassAttribute)}(typeof(Test))]
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

[{nameof(FriendClassAttribute)}(typeof(Other))]
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

[{nameof(FriendClassAttribute)}(typeof(Other))]
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
	}
}
