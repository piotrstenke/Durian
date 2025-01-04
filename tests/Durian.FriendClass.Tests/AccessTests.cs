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
		public async Task Error_When_ChildCallsNotFriendConstructor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
class Test
{{
	internal Test(string name)
	{{
	}}
}}

class Other
{{
}}

class Child : Test
{{
	public Child(string name) : base(name)
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0307_MemberCannotBeAccessedByChildClass.Id);
		}

		[Fact]
		public async Task Error_When_ChildCallsNotFriendMethod()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
class Test
{{
	internal void M1()
	{{
	}}
}}

class Other
{{
}}

class Child : Test
{{
	internal void M2()
	{{
		M1();
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0307_MemberCannotBeAccessedByChildClass.Id);
		}

		[Fact]
		public async Task Error_When_ChildCallsNotFriendPropertyOrField()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
class Test
{{
	static internal string Name {{ get; set; }}
}}

class Other
{{
}}

class Child : Test
{{
	internal void M2()
	{{
		Name = """";
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0307_MemberCannotBeAccessedByChildClass.Id);
		}

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
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0310_MemberCannotBeAccessedByChildClassOfFriend.Id);
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
	internal string Name {{ get; }}
}}

class Other
{{
}}

class Child : Test
{{
	void Main()
	{{
		string name = Name;
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0307_MemberCannotBeAccessedByChildClass.Id);
		}

		[Fact]
		public async Task Error_When_ChildClassTriesToAccessMember_And_DoesNotAllowChildrenAccessOfInternals_And_UsesThisKeyword()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{FriendClassConfigurationAttributeProvider.TypeName}({FriendClassConfigurationAttributeProvider.AllowChildren} = false)]
[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
class Test
{{
	internal string Name {{ get; }}
}}

class Other
{{
}}

class Child : Test
{{
	void Main()
	{{
		string name = this.Name;
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0307_MemberCannotBeAccessedByChildClass.Id);
		}

		[Fact]
		public async Task Error_When_ChildTriesToAccessInnerClass()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
class Test
{{
	internal class Inner
	{{
	}}
}}

class Other
{{
}}

class Child : Test
{{
	void M()
	{{
		new Inner();
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0307_MemberCannotBeAccessedByChildClass.Id);
		}

		[Fact]
		public async Task Error_When_HasInternalConstructor_And_ChildHasDefaultConstructor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
class Test
{{
	internal Test()
	{{
	}}
}}

class Other
{{
}}

class Child : Test
{{
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0307_MemberCannotBeAccessedByChildClass.Id);
		}

		[Fact]
		public async Task Error_When_HasInternalConstructor_And_ChildImplicitlyTriesToAccessParameterlessConstructor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
class Test
{{
	internal Test()
	{{
	}}
}}

class Other
{{
}}

class Child : Test
{{
	public Child()
	{{
	}}

	protected Child(string name)
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0307_MemberCannotBeAccessedByChildClass.Id);
		}

		[Fact]
		public async Task Error_When_IncludeInherited_And_NotFriendChildTriesToAccessInheritedInternalMember()
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

class Child : Test
{{
	void A()
	{{
		Id = """";
	}}
}}
";

			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0307_MemberCannotBeAccessedByChildClass.Id);
		}

		[Fact]
		public async Task Error_When_IncludeInherited_And_NotFriendChildTriesToAccessInheritedInternalMemberFromObject()
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

class Child : Test
{{
	void A()
	{{
		Test test = new();
		test.Id = """";
	}}
}}
";

			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0307_MemberCannotBeAccessedByChildClass.Id);
		}

		[Fact]
		public async Task Error_When_IncludeInherited_And_NotFriendChildTriesToAccessInheritedInternalMemberWithBaseKeyword()
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

class Child : Test
{{
	void A()
	{{
		base.Id = """";
	}}
}}
";

			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0307_MemberCannotBeAccessedByChildClass.Id);
		}

		[Fact]
		public async Task Error_When_IncludeInherited_And_NotFriendChildTriesToAccessInheritedInternalMemberWithThisKeyword()
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

class Child : Test
{{
	void A()
	{{
		this.Id = """";
	}}
}}
";

			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0307_MemberCannotBeAccessedByChildClass.Id);
		}

		[Fact]
		public async Task Error_When_IncludeInherited_And_NotFriendTriesToAccessInstanceMemberReturnedFromMethod()
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

	public Test AsTest()
	{{
		return this as Test;
	}}
}}

class Other
{{
}}

class NotFriend
{{
	void A()
	{{
		Test test = new();
		test.AsTest().Id = """";
	}}
}}
";

			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0302_MemberCannotBeAccessedOutsideOfFriendClass.Id);
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
		test.Id = """";
	}}
}}
";

			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0302_MemberCannotBeAccessedOutsideOfFriendClass.Id);
		}

		[Fact]
		public async Task Error_When_IncludesInherited_And_NotFriendTriesToAccessInheritedInternalMember_And_UsesObjectInitializer()
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
		Test test = new()
		{{
			Id = """";
		}}
	}}
}}
";

			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0302_MemberCannotBeAccessedOutsideOfFriendClass.Id);
		}

		[Fact]
		public async Task Error_When_IncludesInherited_And_NotFriendTriesToAccessInheritedInternalMember_And_UsesPatternMatching()
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

		if(test is {{ Id: """" }})
		{{
		}}
	}}
}}
";

			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0302_MemberCannotBeAccessedOutsideOfFriendClass.Id);
		}

		[Fact]
		public async Task Error_When_IncludesInherited_And_NotFriendTriesToAccessInheritedInternalMember_And_UsesRecursivePatternMatching()
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

		if(test is {{ Id: {{ Length: > 5 }} }})
		{{
		}}
	}}
}}
";

			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0302_MemberCannotBeAccessedOutsideOfFriendClass.Id);
		}

		[Fact]
		public async Task Error_When_IncludesInherited_And_NotFriendTriesToAccessInheritedInternalMember_And_UsesWithExpression()
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
		test = test with {{ Id = """" }};
	}}
}}
";

			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0302_MemberCannotBeAccessedOutsideOfFriendClass.Id);
		}

		[Fact]
		public async Task Error_When_IsRecord_And_HasInternalConstructor_And_ChildCallsConstructorInRecordDeclaration()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
record Test()
{{
	internal Test(string name)
	{{
	}}
}}

class Other
{{
}}

record Child(string Name) : Test(Name);
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0307_MemberCannotBeAccessedByChildClass.Id);
		}

		[Fact]
		public async Task Error_When_IsRecord_And_HasInternalConstructor_And_ChildImplicitlyTriesToAccessParameterlessConstructor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
record Test
{{
	internal Test()
	{{
	}}
}}

class Other
{{
}}

record Child(string Name) : Test;
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0307_MemberCannotBeAccessedByChildClass.Id);
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
		test.Id = """";
		Test.Name = """";
	}}
}}

class Other
{{
}}
";

			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0302_MemberCannotBeAccessedOutsideOfFriendClass.Id);
		}

		[Fact]
		public async Task Error_When_NotFriendTriesToAccessInternalConstructor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
class Test
{{
	internal Test()
	{{
	}}

	internal Test(string name)
	{{
	}}
}}

class Other
{{
}}

class NotFriend
{{
	void A()
	{{
		Test test1 = new();
		Test test2 = new Test();
		var test3 = new Test("""");
		Test 4 = new("""");
	}}
}}
";

			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0302_MemberCannotBeAccessedOutsideOfFriendClass.Id);
		}

		[Fact]
		public async Task Error_When_NotFriendTriesToAccessInternalMember_And_UsesObjectInitializer()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
class Test
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
		Test test = new()
		{{
			Id = """";
		}}
	}}
}}
";

			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0302_MemberCannotBeAccessedOutsideOfFriendClass.Id);
		}

		[Fact]
		public async Task Error_When_NotFriendTriesToAccessInternalMember_And_UsesWithExpression()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
record Test
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
		test = test with {{ Id = """" }};
	}}
}}
";

			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0302_MemberCannotBeAccessedOutsideOfFriendClass.Id);
		}

		[Fact]
		public async Task Error_When_NotFriendTriesToAccessProtectedInternalMember()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
class Test
{{
	protected internal string Name {{ get; set; }}
}}

class Other
{{
}}

class NotFriend
{{
	void M()
	{{
		Test test = new();
		test.Name = """";
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0302_MemberCannotBeAccessedOutsideOfFriendClass.Id);
		}

		[Fact]
		public async Task Error_When_OverridesNotFriendEvent()
		{
			string input =
$@"using System;
using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
class Test
{{
	internal virtual event Action Event;
}}

class Other
{{
}}

class Child : Test
{{
	internal override event Action Event
	{{
		add {{ }}
		remove {{ }}
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0307_MemberCannotBeAccessedByChildClass.Id);
		}

		[Fact]
		public async Task Error_When_OverridesNotFriendIndexer()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
class Test
{{
	internal virtual string this[int index] => null;
}}

class Other
{{
}}

class Child : Test
{{
	internal override string this[int index] => null;
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0307_MemberCannotBeAccessedByChildClass.Id);
		}

		[Fact]
		public async Task Error_When_OverridesNotFriendMethod()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
class Test
{{
	internal virtual void M()
	{{
	}}
}}

class Other
{{
}}

class Child : Test
{{
	internal override void M()
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0307_MemberCannotBeAccessedByChildClass.Id);
		}

		[Fact]
		public async Task Error_When_OverridesNotFriendProperty()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
class Test
{{
	internal virtual string Name {{ get; }}
}}

class Other
{{
}}

class Child : Test
{{
	internal override string Name {{ get; }}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0307_MemberCannotBeAccessedByChildClass.Id);
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
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0302_MemberCannotBeAccessedOutsideOfFriendClass.Id);
		}

		[Fact]
		public async Task Error_When_TriesToImplementInnerInterface()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
class Test
{{
	internal interface Inner
	{{
	}}
}}

class Other
{{
}}

class NotFriend : Parent, Test.Inner
{{
}}

class Parent
{{
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0302_MemberCannotBeAccessedOutsideOfFriendClass.Id);
		}

		[Fact]
		public async Task Error_When_TriesToInheritInnerClass()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
class Test
{{
	internal class Inner
	{{
	}}
}}

class Other
{{
}}

class NotFriend : Test.Inner
{{
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0302_MemberCannotBeAccessedOutsideOfFriendClass.Id);
		}

		[Fact]
		public async Task Success_When_TriesToAccessInheritedInternalMember()
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
		Test.Name = """";
		Test test = new();
		test.Id = """";
	}}
}}
";

			Assert.Empty(await RunAnalyzer(input));
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
			Assert.Empty(await RunAnalyzer(input));
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
			Assert.Empty(await RunAnalyzer(input));
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
		test.Id = """";
		Test.Name = """";
	}}
}}
";

			Assert.Empty(await RunAnalyzer(input));
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
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_IncludeInherited_And_AllowChildren_And_ChildTriesToAccessInheritedInternalMember()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
[{FriendClassConfigurationAttributeProvider.TypeName}({FriendClassConfigurationAttributeProvider.IncludeInherited} = true, {FriendClassConfigurationAttributeProvider.AllowChildren} = true)]
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

class Child : Test
{{
	void A()
	{{
		Id = """";
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
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
		test.Id = """";
		Test.Name = """";
	}}
}}
";

			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_IncludeInherited_And_NotFriendChildTriesToAccessInheritedProtectedInternalMember()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{FriendClassConfigurationAttributeProvider.TypeName}({FriendClassConfigurationAttributeProvider.AllowChildren} = true)]
[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
class Test : Parent
{{
}}

class Other
{{
}}

class Parent
{{
	internal protected string Name {{ get; set; }}
}}

class Child : Test
{{
	void Main()
	{{
		Test test = new();
		test.Name = """";
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_IncludeInherited_And_NotFriendChildTriesToAccessStaticMemberProperly()
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

class Child : Test
{{
	void A()
	{{
		Parent.Name == """";
	}}
}}
";

			Assert.Empty(await RunAnalyzer(input));
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
		test.Id = """";
		Test.Name = """";
	}}
}}

class Other
{{
}}
";

			Assert.Empty(await RunAnalyzer(input));
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
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_NotFriendChildTriesToAccessInheritedProtectedInternalMember()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
class Test : Parent
{{
}}

class Parent
{{
	internal protected string Name {{ get; set; }}
}}

class Other
{{
}}

class Child : Test
{{
	void Main()
	{{
		Test test = new();
		test.Name = """";
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_NotFriendChildTriesToAccessProtectedInternalMember()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
class Test
{{
	internal protected string Name {{ get; set; }}
}}

class Other
{{
}}

class Child : Test
{{
	void Main()
	{{
		Test test = new();
		test.Name = """";
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
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
		test.Name = """";
	}}
}}
";

			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_NotFriendTriesToAccessInheritedInternalMember_And_UsesObjectInitializer()
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
}}

class Other
{{
}}

class NotFriend
{{
	void A()
	{{
		Test test = new()
		{{
			Id = """";
		}}
	}}
}}
";

			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TriesToAccessInnerClassGlobally()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

using static Test.Inner;

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
class Test
{{
	internal class Inner
	{{
	}}
}}

class Other
{{
}}
";
			Assert.Empty(await RunAnalyzer(input));
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
			Assert.Empty(await RunAnalyzer(input));
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
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TriesToAccessNotFriendMemberInDocComment()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

using static Test.Inner;

[{FriendClassAttributeProvider.TypeName}(typeof(Other))]
class Test
{{
	internal static string Name {{ get; }}
}}

class Other
{{
}}

class NotFriend
{{
	/// <inheritdoc cref=""Test.Name""/>
	void Method()
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TriesToAccessInsideInterfaceDefaultImplementation()
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
		Test.Name == """";
	}}
}}
";

			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Warning_When_IncludeInherited_And_NotFriendChildTriesToAccessInheritedProtectedInternalStaticMember()
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

class Child : Test
{{
	void A()
	{{
		Test.Name = """";
	}}
}}
";

			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0314_DoNotAccessInheritedStaticMembers.Id);
		}

		[Fact]
		public async Task Warning_When_IncludeInherited_And_NotFriendTriesToAccessStaticMember()
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
		Test.Name == """";
	}}
}}
";

			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0314_DoNotAccessInheritedStaticMembers.Id);
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
		Test.Name = """";
	}}
}}
";

			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0314_DoNotAccessInheritedStaticMembers.Id);
		}

		protected override IEnumerable<ISourceTextProvider>? GetInitialSources()
		{
			return FriendClassGenerator.GetSourceProviders();
		}
	}
}
