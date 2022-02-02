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

[{FriendClassAttributeProvider.TypeName}(typeof(Other), {FriendClassAttributeProvider.AllowFriendChildren} = false)]
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

[{FriendClassConfigurationAttributeProvider.TypeName}({FriendClassConfigurationAttributeProvider.AllowChildren} = false)]
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
		public async Task Error_When_IncludeInherited_And_NotFriendTriesToAccessInstanceMember()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
[{FriendClassConfigurationAttributeProvider.TypeName}({FriendClassConfigurationAttributeProvider.IncludeInherited} = true)]
class Test : Parent
{{
}}

class Parent
{{
	internal string Id {{ get; set; }}
}}

class Other
{{
}}

class NotFriend
{{
	void A()
	{{
		Test test = new();
		test.Id = "";
	}}
}}
";

			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0302_MemberCannotBeAccessedOutsideOfFriendClass.Id);
		}

		[Fact]
		public async Task Error_When_IncludesInherited_And_NotFriendTriesToAccessInheritedInternalMember()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
[{FriendClassConfigurationAttributeProvider.TypeName}({FriendClassConfigurationAttributeProvider.IncludeInherited} = true)]
class Test : Parent
{{
}}

class Parent
{{
	internal string Id {{ get; set; }}
}}

class Other
{{
}}

class NotFriend
{{
	void A()
	{{
		Test test = new();
		test.Id = "";
	}}
}}
";

			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0302_MemberCannotBeAccessedOutsideOfFriendClass.Id);
		}

		[Fact]
		public async Task Error_When_NotFriendParentTriesToAccessChildMembers()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
class Test : Parent
{{
	internal string Id {{ get; set; }}
	internal static string Name {{ get; set; }}
}}

class Parent
{{
	void A()
	{{
		Test test = new();
		test.Id = "";
		Test.Name = "";
	}}
}}

class Other
{{
}}
";

			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0302_MemberCannotBeAccessedOutsideOfFriendClass.Id);
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
		public async Task Succes_When_TriesToAccessInheritedInternalMember()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
class Test : Parent
{{
}}

class Parent
{{
	internal string Id {{ get; set; }}
	internal static string Name {{ get; set; }}
}}

class Other
{{
	void Method()
	{{
		Test.Name = "";
		Test test = new();
		test.Id = "";
	}}
}}
";

			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task Success_When_ChildClassOfFriendTriesToAccessMember_And_AllowChildrenOfFriends()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other), {FriendClassAttributeProvider.AllowFriendChildren} = true)]
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
		public async Task Success_When_ChildClassTriesToAccessMember_And_AllowChildrenAccessOfInternals()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{FriendClassConfigurationAttributeProvider.TypeName}({FriendClassConfigurationAttributeProvider.AllowChildren} = true)]
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
		public async Task Success_When_FriendParentTriesToAccessChildMembers()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Parent))]
class Test : Parent
{{
	internal string Id {{ get; set; }}
	internal static string Name {{ get; set; }}
}}

class Parent
{{
	void A()
	{{
		Test test = new();
		test.Id = "";
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
		public async Task Success_When_IncludeInherited_And_FriendTriesToAccess()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
[{FriendClassConfigurationAttributeProvider.TypeName}({FriendClassConfigurationAttributeProvider.IncludeInherited} = true)]
class Test : Parent
{{
}}

class Parent
{{
	internal string Id {{ get; set; }}
	internal static string Name {{ get; set; }}
}}

class Other
{{
	void A()
	{{
		Test test = new();
		test.Id = "";
		Test.Name = "";
	}}
}}
";

			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task Success_When_IncludeInherited_And_NotFriendParentTriesToAccessItsMembers()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
[{FriendClassConfigurationAttributeProvider.TypeName}({FriendClassConfigurationAttributeProvider.IncludeInherited} = true)]
class Test : Parent
{{
}}

class Parent
{{
	internal string Id {{ get; set; }}
	internal static string Name {{ get; set; }}

	void A()
	{{
		Test test = new();
		test.Id = "";
		Test.Name = "";
	}}
}}

class Other
{{
}}
";

			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task Success_When_IncludeInherited_And_NotFriendParentTriesToAccessMembersOfParent()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
[{FriendClassConfigurationAttributeProvider.TypeName}({FriendClassConfigurationAttributeProvider.IncludeInherited} = true)]
class Test : Parent
{{
}}

class Parent : ParentParent
{{
	void A()
	{{
		Test test = new();
		test.Id = "";
		Test.Name = "";
	}}
}}

class ParentParent
{{
	internal string Id {{ get; set; }}
	internal static string Name {{ get; set; }}
}}

class Other
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
		public async Task Success_When_NotFriendTriesToAccessInheritedInternalInstanceMember()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
class Test : Parent
{{
}}

class Parent
{{
	internal string Name {{ get; set; }}
}}

class Other
{{
}}

class NotFriend
{{
	void Method()
	{{
		Test test = new();
		test.Name = "";
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

		[Fact]
		public async Task Sucess_When_TriesToAccessInsideInterfaceDefaultImplementation()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(IOther))]]
class Test
{{
	protected internal static string Name {{ get; set; }}
}}

interface IOther
{{
	void M()
	{{
		Test.Name == "";
	}}
}}
";

			Assert.Empty(await RunAnalyzerAsync(input));
		}

		[Fact]
		public async Task Warning_When_IncludeInherited_And_NotFriendTriessToAccessStaticMember()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
[{FriendClassConfigurationAttributeProvider.TypeName}({FriendClassConfigurationAttributeProvider.IncludeInherited} = true)]
class Test : Parent
{{
}}

class Parent
{{
	internal static string Name {{ get; set; }}
}}

class Other
{{
}}

class NotFriend
{{
	void A()
	{{
		Test.Name == "";
	}}
}}
";

			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0314_DoNotAccessInheritedStaticMembers.Id);
		}

		[Fact]
		public async Task Warning_When_NotFriendTriesToAccessInheritedInternalStaticMember()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
class Test : Parent
{{
}}

class Parent
{{
	internal static string Name {{ get; set; }}
}}

class Other
{{
}}

class NotFriend
{{
	void Method()
	{{
		Test.Name = "";
	}}
}}
";

			Assert.Contains(await RunAnalyzerAsync(input), d => d.Id == DUR0314_DoNotAccessInheritedStaticMembers.Id);
		}

		protected override IEnumerable<ISourceTextProvider>? GetInitialSources()
		{
			return FriendClassGenerator.GetSourceProviders();
		}
	}
}