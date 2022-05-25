// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.TestServices;
using Xunit;
using static Durian.Analysis.CopyFrom.CopyFromDiagnostics;

namespace Durian.Analysis.CopyFrom.Tests
{
	public sealed class CopyFromMethodGeneratatorTests : CopyFromGeneratorTest
	{
		[Fact]
		public void Error_When_AlreadyHasImplementation()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0211_MethodAlreadyHasImplementation));
		}

		[Fact]
		public void Error_When_ContainingTypeIsNotPartial()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0201_ContainingTypeMustBePartial));
		}

		[Fact]
		public void Error_When_CopiesFromAbstractMethod()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0209_CannotCopyFromMethodWithoutImplementation));
		}

		[Fact]
		public void Error_When_CopiesFromDelegate()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0204_WrongTargetMemberKind));
		}

		[Fact]
		public void Error_When_CopiesFromExternMethod()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0209_CannotCopyFromMethodWithoutImplementation));
		}

		[Fact]
		public void Error_When_CopiesFromIndexerAccessor_And_AccessorDoesNotExist()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0203_MemberCannotBeResolved));
		}

		[Fact]
		public void Error_When_CopiesFromIndexerWithoutParamList()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0203_MemberCannotBeResolved));
		}

		[Fact]
		public void Error_When_CopiesFromInterfaceMethod()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0209_CannotCopyFromMethodWithoutImplementation));
		}

		[Fact]
		public void Error_When_CopiesFromItself()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Method"")]
	partial void Method();
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0207_MemberCannotCopyFromItselfOrItsParent));
		}

		[Fact]
		public void Error_When_CopiesFromItselfOverridenByChildType()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0207_MemberCannotCopyFromItselfOrItsParent));
		}

		[Fact]
		public void Error_When_CopiesFromItselfOverridenByChildTypeWithTypeArguments()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0207_MemberCannotCopyFromItselfOrItsParent));
		}

		[Fact]
		public void Error_When_CopiesFromItselfWithTypeArguments()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Method<int>"")]
	partial void Method<T>();
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0207_MemberCannotCopyFromItselfOrItsParent));
		}

		[Fact]
		public void Error_When_CopiesFromNonMethodMember()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0204_WrongTargetMemberKind));
		}

		[Fact]
		public void Error_When_CopiesFromOverriddenMethod()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test : Parent
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Parent.Method"")]
	public override partial void Method();
}}

class Parent
{{
	public virtual void Method()
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0207_MemberCannotCopyFromItselfOrItsParent));
		}

		[Fact]
		public void Error_When_CopiesFromOverriddenMethodWihtTypeParameters()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test : Parent
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Parent.Method<int>"")]
	public override partial void Method<T>();
}}

class Parent
{{
	public virtual void Method<T>()
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0207_MemberCannotCopyFromItselfOrItsParent));
		}

		[Fact]
		public void Error_When_CopiesFromPartialMethodWithoutImplementation()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0209_CannotCopyFromMethodWithoutImplementation));
		}

		[Fact]
		public void Error_When_HasDirectCircularDependency()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""B"")]
	partial void A();
	[{CopyFromMethodAttributeProvider.TypeName}(""A"")]
	partial void B();
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0221_CircularDependency));
		}

		[Fact]
		public void Error_When_HasNestedCircularDepenency()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""B"")]
	partial void A();
	[{CopyFromMethodAttributeProvider.TypeName}(""C"")]
	partial void B();
	[{CopyFromMethodAttributeProvider.TypeName}(""A"")]
	partial void C();
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0221_CircularDependency));
		}

		[Fact]
		public void Error_When_HasReturnType_And_TargetIsEventProperty_AndUsesAddAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Event_add"")]
	private partial string Method();

	event System.Action Event
	{{
		add {{ }}
		remove {{ }}
	}}
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0212_TargetDoesNotHaveReturnType));
		}

		[Fact]
		public void Error_When_HasReturnType_And_TargetIsEventProperty_AndUsesRemoveAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Event_remove"")]
	private partial string Method();

	event System.Action Event
	{{
		add {{ }}
		remove {{ }}
	}}
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0212_TargetDoesNotHaveReturnType));
		}

		[Fact]
		public void Error_When_HasReturnType_And_TargetIsImplementedPropertyWithoutGetAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Property"")]
	private partial string Method();

	private string _property;
	string Property
	{{
		set => _property = value;
	}}
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0212_TargetDoesNotHaveReturnType));
		}

		[Fact]
		public void Error_When_HasReturnType_And_TargetIsIndexerInitAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""this[int]_set"")]
	private partial string Method();

	private string _property;
	string this[int i]
	{{
		init => _property = value;
	}}
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0212_TargetDoesNotHaveReturnType));
		}

		[Fact]
		public void Error_When_HasReturnType_And_TargetIsIndexerSetAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""this[int]_set"")]
	private partial string Method();

	private string _property;
	string this[int i]
	{{
		set => _property = value;
	}}
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0212_TargetDoesNotHaveReturnType));
		}

		[Fact]
		public void Error_When_HasReturnType_And_TargetIsIndexerWithoutGetAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""this[int]"")]
	private partial string Method();

	private string _property;
	string this[int i]
	{{
		set => _property = value;
	}}
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0212_TargetDoesNotHaveReturnType));
		}

		[Fact]
		public void Error_When_HasReturnType_And_TargetIsInitAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Property_set"")]
	private partial string Method();

	private string _property;
	string Property
	{{
		init => _property = value;
	}}
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0212_TargetDoesNotHaveReturnType));
		}

		[Fact]
		public void Error_When_HasReturnType_And_TargetIsSetAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Property_set"")]
	private partial string Method();

	private string _property;
	string Property
	{{
		set => _property = value;
	}}
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0212_TargetDoesNotHaveReturnType));
		}

		[Fact]
		public void Error_When_HasReturnType_And_TargetIsVoid()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	private partial string Method();

	void Target()
	{{
	}}
}}
";

			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0212_TargetDoesNotHaveReturnType));
		}

		[Fact]
		public void Error_When_IsAbstractMethod()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0210_InvalidMethodKind));
		}

		[Fact]
		public void Error_When_IsAddAccessor()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0210_InvalidMethodKind));
		}

		[Fact]
		public void Error_When_IsConversionOperator()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0210_InvalidMethodKind));
		}

		[Fact]
		public void Error_When_IsDestructor()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0210_InvalidMethodKind));
		}

		[Fact]
		public void Error_When_IsExplicitInterfaceImplementation()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0210_InvalidMethodKind));
		}

		[Fact]
		public void Error_When_IsExternMethod()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0210_InvalidMethodKind));
		}

		[Fact]
		public void Error_When_IsGetAccessor()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0210_InvalidMethodKind));
		}

		[Fact]
		public void Error_When_IsInitAccessor()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0210_InvalidMethodKind));
		}

		[Fact]
		public void Error_When_IsLambda()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0210_InvalidMethodKind));
		}

		[Fact]
		public void Error_When_IsLocalFunction()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0210_InvalidMethodKind));
		}

		[Fact]
		public void Error_When_IsNotPartial()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Method"")]
	void Method();
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0202_MemberMustBePartial));
		}

		[Fact]
		public void Error_When_IsOperator()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0210_InvalidMethodKind));
		}

		[Fact]
		public void Error_When_IsPartialImplementation()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0211_MethodAlreadyHasImplementation));
		}

		[Fact]
		public void Error_When_IsPartialImplementation_And_HasPartialDefinition()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0211_MethodAlreadyHasImplementation));
		}

		[Fact]
		public void Error_When_IsRemoveAccessor()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0210_InvalidMethodKind));
		}

		[Fact]
		public void Error_When_IsSetAccessor()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0210_InvalidMethodKind));
		}

		[Fact]
		public void Error_When_IsVoid_And_TargetHasReturnType()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	partial void Method();

	string Target()
	{{
		return string.Empty;
	}}
}}
";

			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0213_TargetCannotHaveReturnType));
		}

		[Fact]
		public void Error_When_IsVoid_And_TargetIsGetAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Property_get"")]
	partial void Method();

	private string _property;
	string Property
	{{
		get => _property;
	}}
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0213_TargetCannotHaveReturnType));
		}

		[Fact]
		public void Error_When_IsVoid_And_TargetIsImplementedPropertyWithoutSetOrInitAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Property"")]
	partial void Method();

	private string _property;
	string Property => _property;
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0213_TargetCannotHaveReturnType));
		}

		[Fact]
		public void Error_When_IsVoid_And_TargetIsIndexerGetAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""this[int]_get"")]
	partial void Method();

	private string _property;
	string this[int i]
	{{
		get => _property;
	}}
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0213_TargetCannotHaveReturnType));
		}

		[Fact]
		public void Error_When_IsVoid_And_TargetIsIndexerWithoutSetOrInitAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""this[int]"")]
	partial void Method();

	private string _property;
	string this[int i]
	{{
		get => _property;
	}}
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0213_TargetCannotHaveReturnType));
		}

		[Fact]
		public void Error_When_MemberDoesNotExist()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	partial void Method();
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0203_MemberCannotBeResolved));
		}

		[Fact]
		public void Error_When_NoArguments_And_TargetIsAmbigious()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0208_MemberConflict));
		}

		[Fact]
		public void Error_When_TargetIsAutoProperty()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0209_CannotCopyFromMethodWithoutImplementation));
		}

		[Fact]
		public void Error_When_TargetIsAutoPropertyAccessor()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0209_CannotCopyFromMethodWithoutImplementation));
		}

		[Fact]
		public void Error_When_TargetIsDestructor()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0203_MemberCannotBeResolved));
		}

		[Fact]
		public void Error_When_TargetIsEventField()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0204_WrongTargetMemberKind));
		}

		[Fact]
		public void Error_When_TargetIsEventProperty_And_DoesNotSpecifyAccessor()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0204_WrongTargetMemberKind));
		}

		[Fact]
		public void Error_When_TargetIsExplicitConversionOperatorWithoutParamList()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0203_MemberCannotBeResolved));
		}

		[Fact]
		public void Error_When_TargetIsFunctionPointerSignature()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""delegate*<int>"")]
	partial void Method();
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0203_MemberCannotBeResolved));
		}

		[Fact]
		public void Error_When_TargetIsGeneric_And_TypeArgumentIsNotValid()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target<int>"")]
	partial void Method();

	void Target<T>() where T : class
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0217_TypeParameterIsNotValid));
		}

		[Fact]
		public void Error_When_TargetIsGenericWithMultipleParameters_And_AtLeastOneArgumentIsNotValid()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target<string, int, char>"")]
	partial void Method();

	void Target<T, U, V>() where T : class where U : struct where V : T
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0217_TypeParameterIsNotValid));
		}

		[Fact]
		public void Error_When_TargetIsGenericWithMutlipleParameters_And_ParameterHasWrongName()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target<TType, UType>"")]
	partial void Method();

	void Target<T, U>()
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0217_TypeParameterIsNotValid));
		}

		[Fact]
		public void Error_When_TargetIsImplementedPropertyAccessor_And_AccessorDoesNotExist()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0203_MemberCannotBeResolved));
		}

		[Fact]
		public void Error_When_TargetIsImplicit()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial record Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Equals(Test)"")]
	public partial bool Method();
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0209_CannotCopyFromMethodWithoutImplementation));
		}

		[Fact]
		public void Error_When_TargetIsImplicitConstructor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Test()"")]
	partial void Method();
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0209_CannotCopyFromMethodWithoutImplementation));
		}

		[Fact]
		public void Error_When_TargetIsImplicitConversionOperatorWithoutParamList()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0203_MemberCannotBeResolved));
		}

		[Fact]
		public void Error_When_TargetIsInDifferentAssembly()
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
$@"public class Other
{{
	public void Method()
	{{
	}}
}}";
			Assert.True(RunGeneratorWithDependency(input, external).FailedAndContainsDiagnostics(DUR0205_ImplementationNotAccessible));
		}

		[Fact]
		public void Error_When_TargetIsLambda()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0203_MemberCannotBeResolved));
		}

		[Fact]
		public void Error_When_TargetIsNull()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(null)]
	partial void Method();
}}
";
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0203_MemberCannotBeResolved));
		}

		[Fact]
		public void Error_When_TargetIsOperator_And_HasNoParamList()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0203_MemberCannotBeResolved));
		}

		[Fact]
		public void Error_When_TargetIsStaticConstructor()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0203_MemberCannotBeResolved));
		}

		[Fact]
		public void Error_When_TargetIsUnboundGeneric()
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
			Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0217_TypeParameterIsNotValid));
		}

		[Fact]
		public void Success_When_AddsStaticUsingsAndAliases()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"", {CopyFromMethodAttributeProvider.AddUsings} = new string[] {{ ""static System.Int32"", ""Ta = Target"" }})]
	partial void Method();

	void Target()
	{{
		string a = string.Empty;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};
using static System.Int32;
using Ta = Target;

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target()")}
	private partial void Method()
	{{
		string a = string.Empty;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_AddsUsings()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"", {CopyFromMethodAttributeProvider.AddUsings} = new string[] {{ ""System"" }})]
	partial void Method();

	void Target()
	{{
		string a = string.Empty;
	}}
}}

class Target
{{
	void Method()
	{{
		string a = string.Empty;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};
using System;

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target()")}
	private partial void Method()
	{{
		string a = string.Empty;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_AddsUsings_And_UsingAlreadyExistsBecauseOfCopyUsings()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using System;

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"", {CopyFromMethodAttributeProvider.AddUsings} = new string[] {{ ""System"" }})]
	partial void Method();

	void Target()
	{{
		string a = string.Empty;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};
using System;

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target()")}
	private partial void Method()
	{{
		string a = string.Empty;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_CopiesDocumentation()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"", {CopyFromMethodAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.Documentation})]
	partial void Method();

	/// <summary>
	/// Hello there
	/// </summary>
	void Target()
	{{
		string a = string.Empty;
	}}
}}
";
			string expected =
$@"internal partial class Test
{{
	/// <inheritdoc cref=""Target""/>
	{GetCodeGenerationAttributes("Test.Target()")}
	private partial void Method()
	{{
		string a = string.Empty;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected, true));
		}

		[Fact]
		public void Success_When_CopiesDocumentation_And_HasPattern()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"", {CopyFromMethodAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.Documentation})]
	[{PatternAttributeProvider.TypeName}(""Hello"", ""No"")]
	partial void Method();

	/// <summary>
	/// Hello there
	/// </summary>
	void Target()
	{{
		string a = string.Empty;
	}}
}}
";
			string expected =
$@"internal partial class Test
{{
	/// <summary>
	/// Hello there
	/// </summary>
	{GetCodeGenerationAttributes("Test.Target()")}
	private partial void Method()
	{{
		string a = string.Empty;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected, true));
		}

		[Fact]
		public void Success_When_CopiesFromMethodWithAttribtes_And_AllowsCopyFromAttributes_And_HasPattern()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"", {CopyFromMethodAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.Attributes})]
	[{PatternAttributeProvider.TypeName}(""DEBUG"", ""RELEASE"")]
	partial void Method();

	[System.Diagnostics.Conditional(""DEBUG"")]
	void Target()
	{{
		string a = string.Empty;
	}}
}}
";
			string expected =
$@"internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target()")}
	[System.Diagnostics.Conditional(""RELEASE"")]
	private partial void Method()
	{{
		string a = string.Empty;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_CopiesFromMethodWithAttributes_And_AllowsCopyFromAttributes()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"", {CopyFromMethodAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.Attributes})]
	partial void Method();

	[System.Diagnostics.Conditional(""DEBUG"")]
	void Target()
	{{
		string a = string.Empty;
	}}
}}
";
			string expected =
$@"internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target()")}
	[System.Diagnostics.Conditional(""DEBUG"")]
	private partial void Method()
	{{
		string a = string.Empty;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_CopiesFromPartialMethodWithImplementation()
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
		string a = string.Empty;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Target.Target()")}
	private partial void Method()
	{{
		string a = string.Empty;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_CopiesFromPrivateMethod()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Other.Method"")]
	partial void Method();
}}

class Other
{{
	private void Method()
	{{
		string a = string.Empty;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Other.Method()")}
	private partial void Method()
	{{
		string a = string.Empty;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_CopyUsingsIsFalse()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"", {CopyFromMethodAttributeProvider.AdditionalNodes} = default)]
	partial void Method();

	void Target()
	{{
		string a = string.Empty;
	}}
}}
";
			string expected =
$@"internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target()")}
	private partial void Method()
	{{
		string a = string.Empty;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_CopyUsingsIsFalse_And_AddsUsings()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"", {CopyFromMethodAttributeProvider.AdditionalNodes} = default, {CopyFromMethodAttributeProvider.AddUsings} = new string[] {{ ""System"" }})]
	partial void Method();

	void Target()
	{{
		string a = string.Empty;
	}}
}}
";
			string expected =
$@"using System;

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target()")}
	private partial void Method()
	{{
		string a = string.Empty;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_HasCollidingMethodAndTypeName()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	private partial void Method();

	void Target()
	{{
		string a = string.Empty;
	}}
}}

class Target
{{
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target()")}
	private partial void Method()
	{{
		string a = string.Empty;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_HasMultipleCopyFromMethods()
		{
			string input =
$@"using {DurianStrings.MainNamespace}

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target1"")]
	partial void Method1();

	[{CopyFromMethodAttributeProvider.TypeName}(""Target2"")]
	partial void Method2();

	void Target1()
	{{
		string a = string.Empty;
	}}

	void Target2()
	{{
		string b = string.Empty;
	}}
}}
";
			string expected1 =
$@"using {DurianStrings.MainNamespace}

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target1()")}
	private partial void Method1()
	{{
		string a = string.Empty;
	}}
}}
";
			string expected2 =
$@"using {DurianStrings.MainNamespace}

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target2()")}
	private partial void Method2()
	{{
		string b = string.Empty;
	}}
}}
";

			Assert.True(RunGeneratorWithMultipleOutputs(input).Compare(expected1, expected2));
		}

		[Fact]
		public void Success_When_HasMultipleCopyFromMethods_And_SameTarget()
		{
			string input =
$@"using {DurianStrings.MainNamespace}

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	partial void Method1();

	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	partial void Method2();

	void Target()
	{{
		string a = string.Empty;
	}}
}}
";
			string expected1 =
$@"using {DurianStrings.MainNamespace}

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target()")}
	private partial void Method1()
	{{
		string a = string.Empty;
	}}
}}
";
			string expected2 =
$@"using {DurianStrings.MainNamespace}

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target()")}
	private partial void Method2()
	{{
		string a = string.Empty;
	}}
}}
";

			Assert.True(RunGeneratorWithMultipleOutputs(input).Compare(expected1, expected2));
		}

		[Fact]
		public void Success_When_HasRefOutInParameters()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target(in string, ref string, out string)"")]
	private partial void Method(out string c);

	void Target(in string a, ref string b, out string c)
	{{
		c = string.Empty;
	}}
}}
";

			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target(in string, ref string, out string)")}
	private partial void Method(out string c)
	{{
		c = string.Empty;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_HasReturnType_And_TargetHasReturnType()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	private partial string Method();

	string Target()
	{{
		return string.Empty;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target()")}
	private partial string Method()
	{{
		return string.Empty;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_HasReturnType_And_TargetIsGetAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Property_get"")]
	private partial string Method();

	private string _property;
	string Property => _property;
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Property")}
	private partial string Method() => _property;
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_HasReturnType_And_TargetIsImplementedPropertyWithInitAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Property"")]
	private partial string Method();

	private string _property;
	string Property
	{{
		get => _property;
		init => _property = value;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Property")}
	private partial string Method() => _property;
}}
";

			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_HasReturnType_And_TargetIsImplementedPropertyWithSetAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Property"")]
	private partial string Method();

	private string _property;
	string Property
	{{
		get => _property;
		set => _property = value;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Property")}
	private partial string Method() => _property;
}}
";

			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_HasReturnType_And_TargetIsIndexerGetAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""this[int]_get"")]
	private partial string Method();

	private string _property;
	string this[int i]
	{{
		get => _property;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.this[int]")}
	private partial string Method() => _property;
}}
";

			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_HasReturnType_And_TargetIsIndexerWithInitAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""this[int]"")]
	private partial string Method();

	private string _property;
	string this[int i]
	{{
		get => _property;
		init => _property = value;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.this[int]")}
	private partial string Method() => _property;
}}
";

			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_HasReturnType_And_TargetIsIndexerWithSetAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""this[int]"")]
	private partial string Method();

	private string _property;
	string this[int i]
	{{
		get => _property;
		set => _property = value;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.this[int]")}
	private partial string Method() => _property;
}}
";

			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_IncludesAllNonStandardNodes()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target<T>"", {CopyFromMethodAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.All})]
	partial void Method<T>();

	void Target<T>() where T : struct
	{{
		string a = string.Empty;
	}}
}}
";
			string expected =
$@"internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target<T>()")}
	private partial void Method<T>()
	{{
		string a = string.Empty;
	}}
}}
";
			SingleGeneratorTestResult result = RunGenerator(input);

			Assert.True(result.Compare(expected));
			Assert.True(result.SucceededAndDoesNotContainDiagnostics(DUR0225_BaseTypeAlreadySpecified, DUR0226_CannotApplyBaseType));
		}

		[Fact]
		public void Success_When_IncludesAllNonStandardNodes_And_HasDocumentation()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	/// <summary>
	/// ABC
	/// </summary>
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"", {CopyFromMethodAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.All})]
	partial void Method();

	/// <summary>
	/// Hello there
	/// </summary>
	void Target()
	{{
		string a = string.Empty;
	}}
}}
";
			string expected =
$@"internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target()")}
	private partial void Method()
	{{
		string a = string.Empty;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected, true));
		}

		[Fact]
		public void Success_When_IsGeneric()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	private partial void Method<T>();

	void Target()
	{{
		T t = default;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Property")}
	private partial void Method<T>()
	{{
		return _property;
	}}
}}
";

			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_IsGenericWithConstraints()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	private partial void Method<T>() where T : struct, System.IDisposable;

	public void Target()
	{{
		T t = default;
	}}
}}

class T
{{
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target()")}
	private partial void Method<T>() where T : struct, System.IDisposable
	{{
		T t = default;
	}}
}}
";

			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_IsVoid_And_TargetIsImplementedPropertyWithGetAccessor()
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
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Property")}
	private partial void Method() => _property = value;
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_IsVoid_And_TargetIsIndexerInitAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""this[int]_set"")]
	partial void Method();

	private string _property;
	string this[int i]
	{{
		init => _property = value;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.this[int]")}
	private partial void Method() => _property = value;
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_IsVoid_And_TargetIsIndexerSetAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""this[int]_set"")]
	partial void Method();

	private string _property;
	string this[int i]
	{{
		set => _property = value;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.this[int]")}
	private partial void Method() => _property = value;
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_IsVoid_And_TargetIsIndexerWithGetAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""this[int]"")]
	partial void Method();

	private string _property;
	string this[int i]
	{{
		get => _property;
		set => _property = value;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.this[int]")}
	private partial void Method() => _property = value;
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_IsVoid_And_TargetIsInitAccessor()
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
		init => _property = value;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Property")}
	private partial void Method() => _property = value;
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_IsVoid_And_TargetIsSetAccessor()
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
		set => _property = value;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Property")}
	private partial void Method() => _property = value;
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_IsVoid_And_TargetIsVoid()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	partial void Method();

	void Target()
	{{
		string a = string.Empty;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target")}
	private partial void Method()
	{{
		string a = string.Empty;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_TargetIsAccessible_And_CurrentHasInheritDoc()
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
		string a = string.Empty;
	}}
}}
";

			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target")}
	private partial void Method()
	{{
		string a = string.Empty;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_TargetIsArrowExpression()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	private partial string Method();

	string Target() => string.Empty;
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target()")}
	private private partial string Method()
	{{
		return string.Empty;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_TargetIsConstructor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Test()"")]
	partial void Method();

	Test()
	{{
		string a = string.Empty;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Test()")}
	private partial void Method()
	{{
		string a = string.Empty;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_TargetIsConstructorWithParameters()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Test(string)"")]
	partial void Method();

	Test(string a)
	{{
		string b = string.Empty;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Test(string)")}
	private partial void Method()
	{{
		string b = string.Empty;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_TargetIsDefaultInterfaceImplementation()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial interface ITest
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target()"")]
	partial void Method();

	virtual void Target()
	{{
		string b = string.Empty;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("ITest.Target()")}
	private partial void Method()
	{{
		string b = string.Empty;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_TargetIsEventProperty_And_UsesAddAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Event_add"")]
	partial void Method();

	event System.Action Event
	{{
		add {{ string b = string.Empty; }}
		remove {{ int a = default; }}
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Event")}
	private partial void Method()
	{{
		string b = string.Empty;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_TargetIsEventProperty_And_UsesRemoveAccessor()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Event_remove"")]
	partial void Method();

	event System.Action Event
	{{
		add {{ string b = string.Empty; }}
		remove {{ int a = default; }}
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Event")}
	private partial void Method()
	{{
		int a = default;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_TargetIsExplicitConversionOperatorWithParamList()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""explicit operator int(Test)"")]
	private partial int Method();

	public static explicit operator int(Test t)
	{{
		return 2;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.explicit operator int(Test t)")}
	private partial int Method()
	{{
		return 2;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_TargetIsGeneric()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target<int>"")]
	partial void Method();

	void Target<T>()
	{{
		T t = default;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target<T>()")}
	private partial void Method()
	{{
		int t = default;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_TargetIsGenericWithMutlipleParameters()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target<T, U>()"")]
	partial void Method();

	void Target<T, U>()
	{{
		T t = default;
		U u = default;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target<T, U>()")}
	private partial void Method()
	{{
		T t = default;
		U u = default;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_TargetIsGenericWithMutlipleParameters_And_OneBoundedParameter()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target<int, U>()"")]
	partial void Method();

	void Target<T, U>()
	{{
		T t = default;
		U u = default;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target<T, U>()")}
	private partial void Method()
	{{
		int t = default;
		U u = default;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_TargetIsGenericWithMutlipleParameters_And_WithParamList()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target<T, U>(T, U)"")]
	partial void Method();

	void Target<T, U>(T a, U b)
	{{
		T t = default;
		U u = default;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target<T, U>(T, U)")}
	private partial void Method()
	{{
		int t = default;
		U u = default;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_TargetIsGenericWithMutlipleParameters_And_WithParamList_And_OneBoundedParameter()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target<int, U>(int, U)"")]
	partial void Method();

	void Target<T, U>(T a, U b)
	{{
		T t = default;
		U u = default;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target<T, U>(T, U)")}
	private partial void Method()
	{{
		int t = default;
		U u = default;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_TargetIsGenericWithParamList()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	private int a;

	[{CopyFromMethodAttributeProvider.TypeName}(""Target<T>(out T)"")]
	partial void Method();

	void Target<T>(out T a)
	{{
		a = default;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target<T>(out T)")}
	private partial void Method()
	{{
		a = default;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_TargetIsGenericWithParamList_And_OneBoundArgument()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target<int>(out int)"")]
	private partial void Method(out int a);

	void Target<T>(out T a)
	{{
		a = default;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target<T>(out T)")}
	private partial void Method(out int a)
	{{
		a = default;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_TargetIsImplicitConversionOperatorWithParamList()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""implicit operator int(Test)"")]
	private partial int Method();

	public static implicit operator int(Test t)
	{{
		return 2;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.implicit operator int(Test)")}
	private partial int Method()
	{{
		return 2;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_TargetIsInAliasedType()
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
			string a = string.Empty;
		}}
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};
using A = N2.Other;

namespace N1
{{
	internal partial class Test
	{{
		{GetCodeGenerationAttributes("N2.Other.Method", 2)}
		private partial void Method()
		{{
			string a = string.Empty;
		}}
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_TargetIsInDifferentNamespace()
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
			string a = string.Empty;
		}}
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

namespace N1
{{
	internal partial class Test
	{{
		{GetCodeGenerationAttributes("N2.Other.Method", 2)}
		private partial void Method()
		{{
			string a = string.Empty;
		}}
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_TargetIsInherited()
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
		string a = string.Empty;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Parent.Target()")}
	private partial void Method()
	{{
		string a = string.Empty;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_TargetIsOperator()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""operator +(Test, Test)"")]
	private partial int Method();

	public static int operator +(Test a, Test b)
	{{
		return 2;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.int operator +(Test, Test)")}
	private partial int Method()
	{{
		return 2;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_UsesArgumentList()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target(int)"")]
	partial void Method();

	void Target()
	{{
		int a = default;
	}}

	void Target(int a)
	{{
		string a = string.Empty;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target(int)")}
	private partial void Method()
	{{
		string a = string.Empty;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_UsesArgumentListWithMultipleParameters()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target(int, string)"")]
	partial void Method();

	void Target()
	{{
		int a = default;
	}}

	void Target(int a, string b)
	{{
		string c = string.Empty;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target(int, string)")}
	private partial void Method()
	{{
		string c = string.Empty;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_UsesEmptyArgumentList()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target()"")]
	partial void Method();

	void Target()
	{{
		string a = string.Empty;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target()")}
	private partial void Method()
	{{
		string a = string.Empty;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_UsesOverrideMethod()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test : Parent
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target()"")]
	partial void Method();

	public override void Target()
	{{
		string a = string.Empty;
	}}
}}

class Parent
{{
	public abstract void Target();
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target()")}
	private partial void Method()
	{{
		string a = string.Empty;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_HasArglist()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target(__arglist)"")]
	private partial void Method(__arglist);

	static void Target(__arglist)
	{{
		string a = string.Empty;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target(__arglist)")}
	private partial void Method(__arglist)
	{{
		string a = string.Empty;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Sucess_When_IsExtensionMethod()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

static partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	static partial void Method(this int a);

	static void Target()
	{{
		string a = string.Empty;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target(__arglist)")}
	private partial void Method(__arglist)
	{{
		string a = string.Empty;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Warning_When_AddsEquivalentUsing()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"", {CopyFromMethodAttributeProvider.AddUsings} = new string[] {{ ""System"", ""System"" }})]
	partial void Method();

	void Target()
	{{
		string a = string.Empty;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};
using System;

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target()")}
	private void Method()
	{{
		string a = string.Empty;
	}}
}}
";
			SingleGeneratorTestResult runResult = RunGenerator(input);

			Assert.True(runResult.SucceededAndContainsDiagnostics(DUR0220_UsingAlreadySpecified));
			Assert.True(runResult.Compare(expected));
		}

		[Fact]
		public void Warning_When_CopiesBaseInterfaces()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"", {CopyFromMethodAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.BaseInterfaces})]
	partial void Method();

	void Target()
	{{
		string a = string.Empty;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target()")}
	private partial void Method()
	{{
		string a = string.Empty;
	}}
}}
";
			SingleGeneratorTestResult runResult = RunGenerator(input);

			Assert.True(runResult.SucceededAndContainsDiagnostics(DUR0226_CannotApplyBaseType));
			Assert.True(runResult.Compare(expected));
		}

		[Fact]
		public void Warning_When_CopiesBaseType()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"", {CopyFromMethodAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.BaseType})]
	partial void Method();

	void Target()
	{{
		string a = string.Empty;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target()")}
	private partial void Method()
	{{
		string a = string.Empty;
	}}
}}
";
			SingleGeneratorTestResult runResult = RunGenerator(input);

			Assert.True(runResult.SucceededAndContainsDiagnostics(DUR0226_CannotApplyBaseType));
			Assert.True(runResult.Compare(expected));
		}

		[Fact]
		public void Warning_When_CopiesConstraints()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target<T>"", {CopyFromMethodAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.Constraints})]
	partial void Method<T>();

	void Target<T>() where T : struct
	{{
		string a = string.Empty;
	}}
}}
";
			string expected =
$@"internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target<T>()")}
	private partial void Method<T>()
	{{
		string a = string.Empty;
	}}
}}
";
			SingleGeneratorTestResult runResult = RunGenerator(input);

			Assert.True(runResult.SucceededAndContainsDiagnostics(DUR0224_CannotCopyConstraintsForMethodOrNonGenericMember));
			Assert.True(runResult.Compare(expected));
		}

		[Fact]
		public void Warning_When_CopiesDocumentation_And_AlreadyHasDocumentation()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	/// <summary>
	/// hello there
	/// <summary>
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"", {CopyFromMethodAttributeProvider.AdditionalNodes} = {CopyFromAdditionalNodesProvider.TypeName}.{CopyFromAdditionalNodesProvider.Documentation})]
	partial void Method();

	/// <summary>
	/// ABC
	/// </summary>
	void Target()
	{{
		string a = string.Empty;
	}}
}}
";
			string expected =
$@"internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target()")}
	private partial void Method()
	{{
		string a = string.Empty;
	}}
}}
";
			SingleGeneratorTestResult runResult = RunGenerator(input);

			Assert.True(runResult.SucceededAndContainsDiagnostics(DUR0222_MemberAlreadyHasDocumentation.Id));
			Assert.True(runResult.Compare(expected, true));
		}

		[Fact]
		public void Warning_When_HasPattern_And_PatternIsNull()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target""), {PatternAttributeProvider.TypeName}(null, """")]
	partial void Method();

	void Target()
	{{
		string a = string.Empty;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target()")}
	private partial void Method()
	{{
		string a = string.Empty;
	}}
}}
";
			SingleGeneratorTestResult runResult = RunGenerator(input);

			Assert.True(runResult.SucceededAndContainsDiagnostics(DUR0214_InvalidPatternAttributeSpecified));
			Assert.True(runResult.Compare(expected));
		}

		[Fact]
		public void Warning_When_HasPattern_And_ReplacementIsNull()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target""), {PatternAttributeProvider.TypeName}(""\w+"", null)]
	partial void Method();

	void Target()
	{{
		string a = string.Empty;
	}}
}}
";
			string expected =
$@"using {DurianStrings.MainNamespace};

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Target()")}
	private partial void Method()
	{{
		string a = string.Empty;
	}}
}}
";
			SingleGeneratorTestResult runResult = RunGenerator(input);

			Assert.True(runResult.SucceededAndContainsDiagnostics(DUR0214_InvalidPatternAttributeSpecified));
			Assert.True(runResult.Compare(expected));
		}
	}
}
