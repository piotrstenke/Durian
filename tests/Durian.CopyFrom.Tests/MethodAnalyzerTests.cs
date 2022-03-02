// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.TestServices;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using static Durian.Analysis.CopyFrom.CopyFromDiagnostics;

namespace Durian.Analysis.CopyFrom.Tests
{
    public sealed class MethodAnalyzerTests : AnalyzerTest<CopyFromAnalyzer>
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
            Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0207_MemberCannotCopyFromItselfOrItsParent.Id);
        }

        [Fact]
        public async Task Error_When_CopiesFromOverriddenMethodWihtTypeParameters()
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
        public async Task Error_When_HasReturnType_And_TargetIsEventProperty_AndUsesAddAccessor()
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
            Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0212_TargetDoesNotHaveReturnType.Id);
        }

        [Fact]
        public async Task Error_When_HasReturnType_And_TargetIsEventProperty_AndUsesRemoveAccessor()
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
            Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0212_TargetDoesNotHaveReturnType.Id);
        }

        [Fact]
        public async Task Error_When_HasReturnType_And_TargetIsImplementedPropertyWithoutGetAccessor()
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
            Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0212_TargetDoesNotHaveReturnType.Id);
        }

        [Fact]
        public async Task Error_When_HasReturnType_And_TargetIsIndexerInitAccessor()
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
            Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0212_TargetDoesNotHaveReturnType.Id);
        }

        [Fact]
        public async Task Error_When_HasReturnType_And_TargetIsIndexerSetAccessor()
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
            Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0212_TargetDoesNotHaveReturnType.Id);
        }

        [Fact]
        public async Task Error_When_HasReturnType_And_TargetIsIndexerWithoutGetAccessor()
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
            Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0212_TargetDoesNotHaveReturnType.Id);
        }

        [Fact]
        public async Task Error_When_HasReturnType_And_TargetIsInitAccessor()
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
            Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0212_TargetDoesNotHaveReturnType.Id);
        }

        [Fact]
        public async Task Error_When_HasReturnType_And_TargetIsSetAccessor()
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
            Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0212_TargetDoesNotHaveReturnType.Id);
        }

        [Fact]
        public async Task Error_When_HasReturnType_And_TargetIsVoid()
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

            Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0212_TargetDoesNotHaveReturnType.Id);
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
        public async Task Error_When_IsVoid_And_TargetHasReturnType()
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

            Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0213_TargetCannotHaveReturnType.Id);
        }

        [Fact]
        public async Task Error_When_IsVoid_And_TargetIsGetAccessor()
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
            Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0213_TargetCannotHaveReturnType.Id);
        }

        [Fact]
        public async Task Error_When_IsVoid_And_TargetIsImplementedPropertyWithoutSetOrInitAccessor()
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
            Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0213_TargetCannotHaveReturnType.Id);
        }

        [Fact]
        public async Task Error_When_IsVoid_And_TargetIsIndexerGetAccessor()
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
            Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0213_TargetCannotHaveReturnType.Id);
        }

        [Fact]
        public async Task Error_When_IsVoid_And_TargetIsIndexerWithoutSetOrInitAccessor()
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
            Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0213_TargetCannotHaveReturnType.Id);
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
            Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0209_CannotCopyFromMethodWithoutImplementation.Id);
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
            Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0209_CannotCopyFromMethodWithoutImplementation.Id);
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

partial class Test
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
        public async Task Success_When_CopiesFromPrivateMethod()
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
	}}
}}
";
            Assert.Empty(await RunAnalyzer(input));
        }

        [Fact]
        public async Task Success_When_HasRefOutInParameters()
        {
            string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target(in string, ref string, out string)"")]
	partial void Method();

	void Target(in string a, ref string b, out string c)
	{{
		c = string.Empty;
	}}
}}
";
            Assert.Empty(await RunAnalyzer(input));
        }

        [Fact]
        public async Task Success_When_HasReturnType_And_TargetHasReturnType()
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

            Assert.Empty(await RunAnalyzer(input));
        }

        [Fact]
        public async Task Success_When_HasReturnType_And_TargetIsGetAccessor()
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
            Assert.Empty(await RunAnalyzer(input));
        }

        [Fact]
        public async Task Success_When_HasReturnType_And_TargetIsImplementedPropertyWithInitAccessor()
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
            Assert.Empty(await RunAnalyzer(input));
        }

        [Fact]
        public async Task Success_When_HasReturnType_And_TargetIsImplementedPropertyWithSetAccessor()
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
            Assert.Empty(await RunAnalyzer(input));
        }

        [Fact]
        public async Task Success_When_HasReturnType_And_TargetIsIndexerGetAccessor()
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
            Assert.Empty(await RunAnalyzer(input));
        }

        [Fact]
        public async Task Success_When_HasReturnType_And_TargetIsIndexerWithInitAccessor()
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
            Assert.Empty(await RunAnalyzer(input));
        }

        [Fact]
        public async Task Success_When_HasReturnType_And_TargetIsIndexerWithSetAccessor()
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
        public async Task Success_When_IsVoid_And_TargetIsImplementedPropertyWithGetAccessor()
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
        public async Task Success_When_IsVoid_And_TargetIsIndexerInitAccessor()
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
            Assert.Empty(await RunAnalyzer(input));
        }

        [Fact]
        public async Task Success_When_IsVoid_And_TargetIsIndexerSetAccessor()
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
            Assert.Empty(await RunAnalyzer(input));
        }

        [Fact]
        public async Task Success_When_IsVoid_And_TargetIsIndexerWithGetAccessor()
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
            Assert.Empty(await RunAnalyzer(input));
        }

        [Fact]
        public async Task Success_When_IsVoid_And_TargetIsInitAccessor()
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
            Assert.Empty(await RunAnalyzer(input));
        }

        [Fact]
        public async Task Success_When_IsVoid_And_TargetIsSetAccessor()
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
            Assert.Empty(await RunAnalyzer(input));
        }

        [Fact]
        public async Task Success_When_IsVoid_And_TargetIsVoid()
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
        public async Task Success_When_TargetIsExplicitConversionOperatorWithParamList()
        {
            string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""explicit operator int(Test)"")]
	private partial int Method();

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
        public async Task Success_When_TargetIsImplicitConversionOperatorWithParamList()
        {
            string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""implicit operator int(Test)"")]
	private partial int Method();

	public static implicit operator int(Test t) => 2;
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
        public async Task Success_When_TargetIsOperator()
        {
            string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""operator +(Test, Test)"")]
	private partial int Method();

	public static int operator +(Test a, Test b) => 2;
}}
";
            Assert.Empty(await RunAnalyzer(input));
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
        public async Task Sucess_When_HasArglist()
        {
            string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target(__arglist)"")]
	partial void Method();

	static void Target(__arglist)
	{{
	}}
}}
";
            Assert.Empty(await RunAnalyzer(input));
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
        public async Task Warning_When_HasPattern_And_PatternIsNull()
        {
            string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target""), {PatternAttributeProvider.TypeName}(null, """")]
	partial void Method();

	void Target()
	{{
	}}
}}
";
            Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0214_InvalidPatternAttributeSpecified.Id);
        }

        [Fact]
        public async Task Warning_When_HasPattern_And_ReplacementIsNull()
        {
            string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target""), {PatternAttributeProvider.TypeName}(""\w+"", null)]
	partial void Method();

	void Target()
	{{
	}}
}}
";
            Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0214_InvalidPatternAttributeSpecified.Id);
        }

        [Fact]
        public async Task Warning_When_PatternIsRedundant()
        {
            string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{PatternAttributeProvider.TypeName}(""\w+"", """")]
	partial void Method();
}}
";
            Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0215_RedundantPatternAttribute.Id);
        }

        [Fact]
        public async Task Warning_When_SamePatternAlreadySpecified()
        {
            string input =
$@"using {DurianStrings.MainNamespace};

partial class Test
{{
	[{CopyFromMethodAttributeProvider.TypeName}(""Target"")]
	[{PatternAttributeProvider.TypeName}(""\w+"", """")]
	[{PatternAttributeProvider.TypeName}(""\w+"", ""xyz"")]
	partial void Method();
}}
";
            Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0216_EquivalentPatternAttribute.Id);
        }

        protected override IEnumerable<ISourceTextProvider>? GetInitialSources()
        {
            return CopyFromGenerator.GetSourceProviders();
        }
    }
}