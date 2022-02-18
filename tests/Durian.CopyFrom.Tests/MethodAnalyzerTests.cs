﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Threading.Tasks;
using Durian.TestServices;
using Xunit;

using static Durian.Analysis.CopyFrom.CopyFromDiagnostics;

namespace Durian.Analysis.CopyFrom.Tests
{
	public sealed partial class MethodAnalyzerTests : AnalyzerTest<CopyFromAnalyzer>
	{
		[Fact]
		public async Task Error_When_AlreadyHasImplementation()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	partial void Method();

	void Target()
	{{
	}}
}}

partial class Test
{{
	partial void Method()
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0211_MethodAlreadyHasImplementation.Id);
		}

		[Fact]
		public async Task Error_When_ContainingTypeIsNotPartial()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	partial void Method();

	void Target()
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0201_ContainingTypeMustBePartial.Id);
		}

		[Fact]
		public async Task Error_When_CopiesFromAbstractMethod()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial abstract class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	partial void Method();

	abstract void Target();
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0209_CannotCopyFromMethodWithoutImplementation.Id);
		}

		[Fact]
		public async Task Error_When_CopiesFromDelegate()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	partial void Method();

	delegate void Target();
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0204_WrongTargetMemberKind.Id);
		}

		[Fact]
		public async Task Error_When_CopiesFromExternMethod()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial static class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	partial static void Method();

	static extern void Target();
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0209_CannotCopyFromMethodWithoutImplementation.Id);
		}

		[Fact]
		public async Task Error_When_CopiesFromIndexerWithoutParamList()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""this"")]
	partial void Method();

	int this[int index] => 2;
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0203_MemberCannotBeResolved.Id);
		}

		[Fact]
		public async Task Error_When_CopiesFromInterfaceMethod()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial interface ITest
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	partial void Method();

	void Target();
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0209_CannotCopyFromMethodWithoutImplementation.Id);
		}

		[Fact]
		public async Task Error_When_CopiesFromItself()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Method"")]
	partial void Method();
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0207_MemberCannotCopyFromItselfOrItsParent.Id);
		}

		[Fact]
		public async Task Error_When_CopiesFromItselfOverridenByChildType()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Child.Method"")]
	public virtual partial void Method();
}}

class Child : Test
{{
	public override void Method()
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0207_MemberCannotCopyFromItselfOrItsParent.Id);
		}

		[Fact]
		public async Task Error_When_CopiesFromItselfOverridenByChildTypeWithTypeArguments()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Child.Method<int>"")]
	public virtual partial void Method<T>();
}}

class Child : Test
{{
	public override void Method<T>()
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0207_MemberCannotCopyFromItselfOrItsParent.Id);
		}

		[Fact]
		public async Task Error_When_CopiesFromItselfWithTypeArguments()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Method<int>"")]
	partial void Method<T>();
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0207_MemberCannotCopyFromItselfOrItsParent.Id);
		}

		[Fact]
		public async Task Error_When_CopiesFromMethodWithoutImplementationWithErrorDiagnostic()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	partial void Method();

	virtual void Target();
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0209_CannotCopyFromMethodWithoutImplementation.Id);
		}

		[Fact]
		public async Task Error_When_CopiesFromNonMethodMember()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	partial void Method();

	string Target;
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0204_WrongTargetMemberKind.Id);
		}

		[Fact]
		public async Task Error_When_CopiesFromOverriddenMethod()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""base.Method"")]
	public override partial void Method();
}}

class Parent
{{
	public virtual void Method()
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0207_MemberCannotCopyFromItselfOrItsParent.Id);
		}

		[Fact]
		public async Task Error_When_CopiesFromOverriddenMethodWihtTypeParameters()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""base.Method<int>"")]
	public override partial void Method<T>();
}}

class Parent
{{
	public virtual void Method<T>()
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0207_MemberCannotCopyFromItselfOrItsParent.Id);
		}

		[Fact]
		public async Task Error_When_CopiesFromPartialMethodWithoutImplementation()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	partial void Method();

	partial void Target();
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0209_CannotCopyFromMethodWithoutImplementation.Id);
		}

		[Fact]
		public async Task Error_When_IsAbstractMethod()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

abstract partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	abstract void Method();

	void Target()
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0210_InvalidMethodKind.Id);
		}

		[Fact]
		public async Task Error_When_IsAddAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	event System.Action Event
	{{
		[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
		add {{}}
		remove {{ }}
	}}

	void Target()
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0210_InvalidMethodKind.Id);
		}

		[Fact]
		public async Task Error_When_IsConversionOperator()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	public static implicit operator int(Test a) => 2;

	void Target()
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0210_InvalidMethodKind.Id);
		}

		[Fact]
		public async Task Error_When_IsDestructor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	~Test()
	{{
	}}

	void Target()
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0210_InvalidMethodKind.Id);
		}

		[Fact]
		public async Task Error_When_IsExplicitInterfaceImplementation()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test : IA
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	void IA.Method()
	{{
	}}

	void Target()
	{{
	}}
}}

interface IA
{{
	void Method();
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0210_InvalidMethodKind.Id);
		}

		[Fact]
		public async Task Error_When_IsExternMethod()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	extern void Method();

	void Target()
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0210_InvalidMethodKind.Id);
		}

		[Fact]
		public async Task Error_When_IsGetAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	string Property
	{{
		[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
		get => """";
	}}

	void Target()
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0210_InvalidMethodKind.Id);
		}

		[Fact]
		public async Task Error_When_IsInitAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	string Property
	{{
		[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
		init {{}}
	}}

	void Target()
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0210_InvalidMethodKind.Id);
		}

		[Fact]
		public async Task Error_When_IsLambda()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	void Method()
	{{
		System.Func<int, int> func = [{CopyFromMethodAttributeProvider.TypeName}(""Target"")](int a) => 3;
	}}

	void Target()
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0210_InvalidMethodKind.Id);
		}

		[Fact]
		public async Task Error_When_IsLocalFunction()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	void Method()
	{{
		[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
		void Local()
		{{
		}}
	}}

	void Target()
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0210_InvalidMethodKind.Id);
		}

		[Fact]
		public async Task Error_When_IsNotPartial()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Method"")]
	void Method();
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0202_MemberMustBePartial.Id);
		}

		[Fact]
		public async Task Error_When_IsOperator()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	public static int operator +(Test a, Test b) => 2;

	void Target()
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0210_InvalidMethodKind.Id);
		}

		[Fact]
		public async Task Error_When_IsPartialImplementation()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	partial void Method()
	{{
	}}

	void Target()
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0211_MethodAlreadyHasImplementation.Id);
		}

		[Fact]
		public async Task Error_When_IsPartialImplementation_And_HasPartialDefinition()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	partial void Method()
	{{
	}}

	void Target()
	{{
	}}
}}

partial class Test
{{
	partial void Method();
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0211_MethodAlreadyHasImplementation.Id);
		}

		[Fact]
		public async Task Error_When_IsRemoveAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	event System.Action Event
	{{
		add {{}}
		[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
		remove {{ }}
	}}

	void Target()
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0210_InvalidMethodKind.Id);
		}

		[Fact]
		public async Task Error_When_IsSetAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	string Property
	{{
		[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
		set {{}}
	}}

	void Target()
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0210_InvalidMethodKind.Id);
		}

		[Fact]
		public async Task Error_When_MemberDoesNotExist()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	partial void Method();
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0203_MemberCannotBeResolved.Id);
		}

		[Fact]
		public async Task Error_When_NoArguments_And_TargetIsAmbigious()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	partial void Method();

	void Target()
	{{
	}}

	void Target(int a)
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0208_MemberConflict.Id);
		}

		[Fact]
		public async Task Error_When_TargetIsAutoProperty()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Property"")]
	partial void Method();

	string Property {{ get; set; }}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0205_ImplementationNotAccessible.Id);
		}

		[Fact]
		public async Task Error_When_TargetIsEventField()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Event"")]
	partial void Method();

	event System.Action Event;
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0204_WrongTargetMemberKind.Id);
		}

		[Fact]
		public async Task Error_When_TargetIsEventProperty_And_DoesNotSpecifyAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Event"")]
	partial void Method();

	event System.Action Event
	{{
		add {{ }}
		remove {{ }}
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0204_WrongTargetMemberKind.Id);
		}

		[Fact]
		public async Task Success_When_TargetIsEventProperty_And_UsesAddAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Event_add"")]
	partial void Method();

	event System.Action Event
	{{
		add {{ }}
		remove {{ }}
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TargetIsEventProperty_And_UsesRemoveAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Event_remove"")]
	partial void Method();

	event System.Action Event
	{{
		add {{ }}
		remove {{ }}
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Error_When_TargetIsInDifferentAssembly()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Other.Method"")]
	partial void Method();
}}
";
			string external =
$@"class Other
{{
	public void Method()
	{{
	}}
}}";
			Assert.Contains(await RunAnalyzerWithDependency(input, external), d => d.Id == DUR0205_ImplementationNotAccessible.Id);
		}

		[Fact]
		public async Task Success_When_TargetIsInDifferentNamespace()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

namespace N1
{{
	partial class Test
	{{
		[{CopyFromMethodAttributeProvider.TypeName}(""N2.Other.Method"")]
		partial void Method();
	}}
}}

namespace N2
{{
	class Other
	{{
		void Method()
		{{
		}}
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TargetIsInAliasedType()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using A = N2.Other;

namespace N1
{{
	partial class Test
	{{
		[{CopyFromMethodAttributeProvider.TypeName}(""A.Method"")]
		partial void Method();
	}}
}}

namespace N2
{{
	class Other
	{{
		void Method()
		{{
		}}
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Error_When_TargetIsAutoPropertyAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Property_set"")]
	partial void Method();

	string Property {{ get; set; }}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0205_ImplementationNotAccessible.Id);
		}

		[Fact]
		public async Task Error_When_TargetIsDestructor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""~Test()"")]
	partial void Method();

	~Test()
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0203_MemberCannotBeResolved.Id);
		}

		[Fact]
		public async Task Error_When_TargetIsExplicitConversionOperatorWithoutParamList()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""explicit operator int"")]
	partial void Method();

	public static explicit operator int(Test t) => 2;
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0203_MemberCannotBeResolved.Id);
		}

		[Fact]
		public async Task Error_When_TargetIsFunctionPointerSignature()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""delegate*<int>"")]
	partial void Method();
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0203_MemberCannotBeResolved.Id);
		}

		[Fact]
		public async Task Error_When_TargetIsImplementedPropertyAccessor_And_AccessorDoesNotExist()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Property_set"")]
	partial void Method();

	private string _property;
	string Property => _property;
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0203_MemberCannotBeResolved.Id);
		}

		[Fact]
		public async Task Error_When_TargetIsImplicit()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial record Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Equals(Test)"")]
	partial void Method();
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0209_CannotCopyFromMethodWithoutImplementation.Id);
		}

		[Fact]
		public async Task Error_When_TargetIsImplicitConstructor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Test()"")]
	partial void Method();
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0209_CannotCopyFromMethodWithoutImplementation.Id);
		}

		[Fact]
		public async Task Error_When_TargetIsImplicitConversionOperatorWithoutParamList()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""implicit operator int"")]
	partial void Method();

	public static implicit operator int(Test t) => 2;
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0203_MemberCannotBeResolved.Id);
		}

		[Fact]
		public async Task Error_When_TargetIsLambda()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""() => {{ return; }}"")]
	partial void Method();

	event System.Action Target
	{{
		add {{}}
		remove {{}}
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0203_MemberCannotBeResolved.Id);
		}

		[Fact]
		public async Task Error_When_TargetIsNull()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(null)]
	partial void Method();
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0203_MemberCannotBeResolved.Id);
		}

		[Fact]
		public async Task Success_When_TargetIsOperator_And_HasBracketParamList()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""operator +[Test, Test]"")]
	partial void Method();

	public static int operator +(Test a, Test b) => 2;
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Error_When_TargetIsOperator_And_HasNoParamList()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""operator +"")]
	partial void Method();

	public static int operator +(Test a, Test b) => 2;
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0203_MemberCannotBeResolved.Id);
		}

		[Fact]
		public async Task Error_When_TargetIsOperator_And_HasNoParamList_And_HasSameOperatorWithDifferentParameters()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""operator +"")]
	partial void Method();

	public static int operator +(Test a, Test b) => 2;

	public static int operator +(Test a, int b) => 2;
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0208_MemberConflict.Id);
		}

		[Fact]
		public async Task Error_When_TargetIsStaticConstructor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""static Test()"")]
	partial void Method();

	static Test()
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0203_MemberCannotBeResolved.Id);
		}

		public async Task Success_When_CopiesFromPartialMethodWithImplementation()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	partial void Method();

	partial void Target();
}}

partial class Test
{{
	partial void Target()
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_IsGeneric()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	partial void Method<T>();

	void Target()
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TargetIsAccessible()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	partial void Method();

	void Target()
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TargetIsAccessible_And_CurrentHasInheritDoc()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	///	<inheritdoc cref=""Test""/>
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	partial void Method();

	void Target()
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TargetIsConstructor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Test()"")]
	partial void Method();

	Test()
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TargetIsConstructorWithParameters()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Test(string)"")]
	partial void Method();

	Test(string a)
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TargetIsDefaultInterfaceImplementation()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial interface ITest
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target()"")]
	partial void Method();

	virtual void Target()
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TargetIsExplicitConversionOperatorWithParamList()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""explicit operator int(Test)"")]
	partial void Method();

	public static explicit operator int(Test t) => 2;
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TargetIsGeneric()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target<int>"")]
	partial void Method();

	void Target<T>()
	{{
	}}
}}
";

			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TargetIsGenericWithParamList()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target<int>(int)"")]
	partial void Method();

	void Target<T>(int a)
	{{
	}}
}}
";

			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TargetIsGenericWithParamListWithBoundArgument()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target<int>(int)"")]
	partial void Method();

	void Target<T>(int a)
	{{
	}}
}}
";

			Assert.Empty(await RunAnalyzer(input));
		}


		[Fact]
		public async Task Success_When_TargetIsImplementedProperty()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Property"")]
	partial void Method();

	private string _property;
	string Property
	{{
		get => _property;
		set => _property = value;
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TargetIsImplementedPropertyAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Property_set"")]
	partial void Method();

	private string _property;
	string Property
	{{
		get => _property;
		set => _property = value;
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TargetIsImplicitConversionOperatorWithParamList()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""implicit operator int(Test)"")]
	partial void Method();

	public static implicit operator int(Test t) => 2;
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TargetIsInherited()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test : Parent
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Parent.Target()"")]
	partial void Method();
}}

class Parent
{{
	public void Target()
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Error_When_TargetIsOperator_And_HasParenthesisParamList()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""operator +(Test, Test)"")]
	partial void Method();

	public static int operator +(Test a, Test b) => 2;
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0203_MemberCannotBeResolved.Id);
		}

		[Fact]
		public async Task Success_When_TargetIsUnboundGeneric()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target<,>"")]
	partial void Method();

	void Target<T, U>()
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_UsesArgumentList()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target(int)"")]
	partial void Method();

	void Target()
	{{
	}}

	void Target(int a)
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_UsesArgumentListWithMultipleParameters()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target(int, string)"")]
	partial void Method();

	void Target()
	{{
	}}

	void Target(int a, string b)
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_UsesEmptyArgumentList()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target()"")]
	partial void Method();

	void Target()
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_UsesOverrideMethod()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test : Parent
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target()"")]
	partial void Method();

	public override void Target()
	{{
	}}
}}

class Parent
{{
	public abstract void Target();
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Sucess_When_CopiesFromIndexer()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""this[int]"")]
	partial void Method();

	int this[int index] => 2;
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Sucess_When_CopiesFromIndexerAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""this[int]_set"")]
	partial void Method();

	int this[int index] 
	{{
		get => 2;
		set
		{{
		}}
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Error_When_CopiesFromIndexerAccessor_And_AccessorDoesNotExist()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""this[int]_set"")]
	partial void Method();

	int this[int index] 
	{{
		get => 2;
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0203_MemberCannotBeResolved.Id);
		}

		[Fact]
		public async Task Sucess_When_IsExtensionMethod()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

static partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	static partial void Method(this int a);

	static void Target()
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Warning_When_IdenticalAttributesSpecified()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	partial void Method();

	void Target()
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0206_EquivalentAttributes.Id);
		}

		[Fact]
		public async Task Warning_When_ResolvedToSameMemberThroughAlias()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using Test = A;

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	[{CopyFromMethodAttributeProvider.TypeName}(""A.Target"")]
	partial void Method();

	public void Target()
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0206_EquivalentAttributes.Id);
		}

		[Fact]
		public async Task Warning_When_ResolvedToSameMemberThroughBaseKeyword()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test : Parent
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	[{CopyFromMethodAttributeProvider.TypeName}(""base.Target"")]
	partial void Method();
}}

class Parent
{{
	public void Target()
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0206_EquivalentAttributes.Id);
		}

		[Fact]
		public async Task Warning_When_ResolvedToSameMemberThroughThisKeyword()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	[{CopyFromMethodAttributeProvider.TypeName}(""this.Target"")]
	partial void Method();

	public void Target()
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0206_EquivalentAttributes.Id);
		}

		protected override IEnumerable<ISourceTextProvider>? GetInitialSources()
		{
			return CopyFromGenerator.GetSourceProviders();
		}
	}
}