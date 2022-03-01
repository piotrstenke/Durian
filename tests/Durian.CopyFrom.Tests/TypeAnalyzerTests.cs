// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Threading.Tasks;
using Durian.TestServices;
using Xunit;

using static Durian.Analysis.CopyFrom.CopyFromDiagnostics;

namespace Durian.Analysis.CopyFrom.Tests
{
	public sealed class TypeAnalyzerTests : AnalyzerTest<CopyFromAnalyzer>
	{
		[Fact]
		public async Task Error_When_ContainingTypeIsNotPartial()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

class Parent
{{
	[{CopyFromTypeAttributeProvider.TypeName}(typeof(Target))]
	partial class Test
	{{
	}}
}}

class Target
{{
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0201_ContainingTypeMustBePartial.Id);
		}

		[Fact]
		public async Task Error_When_CopiedFromDynamic()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(dynamic))]
partial class Test
{{
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0204_WrongTargetMemberKind.Id);
		}

		[Fact]
		public async Task Error_When_CopiedFromEnum()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(Target))]
partial class Test
{{
}}

enum Target
{{
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0204_WrongTargetMemberKind.Id);
		}

		[Fact]
		public async Task Error_When_CopiedFromNonType()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target.Method"")]
partial class Test
{{
}}

class Target
{{
	void Method()
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0203_MemberCannotBeResolved.Id);
		}

		[Fact]
		public async Task Error_When_CopiesFromChildType()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(Child))]
partial class Test
{{
}}

class Child : Test
{{
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0207_MemberCannotCopyFromItselfOrItsParent.Id);
		}

		[Fact]
		public async Task Error_When_CopiesFromContainingType()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Outer
{{
	[{CopyFromTypeAttributeProvider.TypeName}(typeof(Outer))]
	partial class Test
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0207_MemberCannotCopyFromItselfOrItsParent.Id);
		}

		[Fact]
		public async Task Error_When_CopiesFromDelegate()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(Delegate))]
partial class Test
{{
}}

delegate void Delegate();
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0204_WrongTargetMemberKind.Id);
		}

		[Fact]
		public async Task Error_When_CopiesFromItself()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(Test))]
partial class Test
{{
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0207_MemberCannotCopyFromItselfOrItsParent.Id);
		}

		[Fact]
		public async Task Error_When_CopiesFromParentType()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(Parent))]
partial class Test : Parent
{{
}}

class Parent
{{
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0207_MemberCannotCopyFromItselfOrItsParent.Id);
		}

		[Fact]
		public async Task Error_When_HasConflictingMemberNamesFromDifferentNamespaces()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

using N1;
using N2;

[{CopyFromTypeAttributeProvider.TypeName}(""Target"")]
partial class Test
{{
}}

namespace N1
{{
	class Target
	{{
	}}
}}

namespace N2
{{
	class Target
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0208_MemberConflict.Id);
		}

		[Fact]
		public async Task Error_When_IsNotPartial()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(Target))]
class Test
{{
}}

class Target
{{
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0202_MemberMustBePartial.Id);
		}

		[Fact]
		public async Task Error_When_TargetIsArray()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(string[]))]
partial class Test
{{
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0204_WrongTargetMemberKind.Id);
		}

		[Fact]
		public async Task Error_When_TargetIsFunctionPointer()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(delegate*<int>))]
partial class Test
{{
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0204_WrongTargetMemberKind.Id);
		}

		[Fact]
		public async Task Error_When_TargetIsInDifferentAssembly()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(Target))]
partial class Test
{{
}}
";
			string external =
$@"class Target
{{
}}
";
			Assert.Contains(await RunAnalyzerWithDependency(input, external), d => d.Id == DUR0205_ImplementationNotAccessible.Id);
		}

		[Fact]
		public async Task Error_When_TargetIsInDifferentAssembly_And_GetsByString()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"")]
partial class Test
{{
}}
";
			string external =
$@"class Target
{{
}}
";
			Assert.Contains(await RunAnalyzerWithDependency(input, external), d => d.Id == DUR0205_ImplementationNotAccessible.Id);
		}

		[Fact]
		public async Task Error_When_TargetIsMultidimensionalArray()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(string[,]))]
partial class Test
{{
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0204_WrongTargetMemberKind.Id);
		}

		[Fact]
		public async Task Error_When_TargetIsNestedArray()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(string[][]))]
partial class Test
{{
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0204_WrongTargetMemberKind.Id);
		}

		[Fact]
		public async Task Error_When_TargetIsNullAsString()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}((string)null)]
partial class Test
{{
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0203_MemberCannotBeResolved.Id);
		}

		[Fact]
		public async Task Error_When_TargetIsNullAsType()
		{
			string input =
$@"using System;
using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}((Type)null)]
partial class Test
{{
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0203_MemberCannotBeResolved.Id);
		}

		[Fact]
		public async Task Error_When_TargetIsPointer()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(int*))]
partial class Test
{{
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0204_WrongTargetMemberKind.Id);
		}

		[Fact]
		public async Task Error_When_TargetIsTypeParameter()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Outer<T>
{{
	[{CopyFromTypeAttributeProvider.TypeName}(""T"")]
	partial class Test
	{{
	}}
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0204_WrongTargetMemberKind.Id);
		}

		[Fact]
		public async Task Error_When_TypeDoesNotExist()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"")]
partial class Test
{{
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0203_MemberCannotBeResolved.Id);
		}

		[Fact]
		public async Task Success_When_CopiesFromInnerType()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(Inner))]
partial class Test
{{
	class Inner
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

[{CopyFromTypeAttributeProvider.TypeName}(typeof(Target))]
partial class Test
{{
}}

class Target
{{
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TargetIsAliased()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

using T = N2.Target;

namespace N1
{{
	[{CopyFromTypeAttributeProvider.TypeName}(""T"")]
	partial class Test
	{{
	}}
}}

namespace N2
{{
	class Target
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TargetIsGeneric()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target<int>"")]
partial class Test
{{
}}

class Target<T>
{{
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TargetIsGeneric_And_IsNestedTypeOfUnboundGeneric()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Outer<,>.Target<int>"")]
partial class Test
{{
}}

class Outer<T, U>
{{
	public class Target<X>
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TargetIsGlobalNamespace_And_CurrentInIsNormalNamespace()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

namespace N1
{{
	[{CopyFromTypeAttributeProvider.TypeName}(""Target"")]
	partial class Test
	{{
	}}
}}

class Target
{{
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TargetIsGlobalNamespace_And_CurrentTypeIsInGlobalNamespace()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target"")]
partial class Test
{{
}}

class Target
{{
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TargetIsGlobalNamespace_And_UsedGlobaKeyword()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

namespace N1
{{
	[{CopyFromTypeAttributeProvider.TypeName}(""global::Target"")]
	partial class Test
	{{
	}}
}}

class Target
{{
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TargetIsInDifferentNamespace_And_HasUsings()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using N2;

namespace N1
{{
	[{CopyFromTypeAttributeProvider.TypeName}(""Target"")]
	partial class Test
	{{
	}}
}}

namespace N2
{{
	class Target
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TargetIsInInnerNamespace()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

namespace N1
{{
	[{CopyFromTypeAttributeProvider.TypeName}(""N2.Target"")]
	partial class Test
	{{
	}}
}}

namespace N1.N2
{{
	class Target
	{{
	}}
}}

";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TargetIsInnerClass()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Outer.Target"")]
partial class Test
{{
}}

class Outer
{{
	public class Target
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TargetIsInnerClass_And_HasStaticUsing()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

using static Outer;

namespace N1
{{
	[{CopyFromTypeAttributeProvider.TypeName}(""Target"")]
	partial class Test
	{{
	}}
}}

class Outer
{{
	public class Target
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TargetIsInnerClassOfAliasedType()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

using O = Outer;

[{CopyFromTypeAttributeProvider.TypeName}(""O.Target"")]
partial class Test
{{
}}

class Outer
{{
	public class Target
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TargetIsInSameNamespace()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

namespace N1
{{
	[{CopyFromTypeAttributeProvider.TypeName}(""Target"")]
	partial class Test
	{{
	}}
}}

namespace N1
{{
	class Target
	{{
	}}
}}

";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TargetIsNestedTypeOfGeneric()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Outer<int, string>.Target"")]
partial class Test
{{
}}

class Outer<T, U>
{{
	public class Target
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TargetIsNestedTypeOfUnboundGeneric()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Outer<,>.Target"")]
partial class Test
{{
}}

class Outer<T, U>
{{
	public class Target
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TargetIsNullableReferenceType()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target?"")]
partial class Test
{{
}}

class Target
{{
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TargetIsPrivate()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(Outer.Target))]
partial class Test
{{
}}

class Outer
{{
	private class Target
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TargetIsSpecifiedUsingFullyQualifiedName()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""N1.N2.Target"")]
partial class Test
{{
}}

namespace N1.N2
{{
	class Target
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TargetIsSpecifiedUsingFullyQualifiedNameWithGlobalKeyword()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""global::N1.N2.Target"")]
partial class Test
{{
}}

namespace N1.N2
{{
	class Target
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Success_When_TargetIsUnboundGeneric()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target<,>"")]
partial class Test
{{
}}

class Target<T, U>
{{
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Warning_When_HasPattern_And_PatternIsNull()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target""), {PatternAttributeProvider.TypeName}(null, """")]
partial class Test
{{
}}

class Target
{{
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0214_InvalidPatternAttributeSpecified.Id);
		}

		[Fact]
		public async Task Warning_When_HasPattern_And_ReplacementIsNull()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(""Target""), {PatternAttributeProvider.TypeName}(""\w+"", null)]
partial class Test
{{
}}

class Target
{{
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0214_InvalidPatternAttributeSpecified.Id);
		}

		[Fact]
		public async Task Warning_When_IdenticalAttributesSpecified()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(Target))]
[{CopyFromTypeAttributeProvider.TypeName}(typeof(Target))]
partial class Test
{{
}}

class Target
{{
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0206_EquivalentAttributes.Id);
		}

		[Fact]
		public async Task Warning_When_PatternIsRedundant()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{PatternAttributeProvider.TypeName}(""\w+"", """")]
partial class Test
{{
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0215_RedundantPatternAttribute.Id);
		}

		[Fact]
		public async Task Warning_When_SourceAndSourceTypeOfTwoAttributesPointToTheSameMember()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(Target))]
[{CopyFromTypeAttributeProvider.TypeName}(""Target"")]
partial class Test
{{
}}

class Target
{{
}}
";
			Assert.Contains(await RunAnalyzer(input), d => d.Id == DUR0206_EquivalentAttributes.Id);
		}

		[Fact]
		public async Task Warning_When_SamePatternAlreadySpecified()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

[{CopyFromTypeAttributeProvider.TypeName}(typeof(Target))]
[{PatternAttributeProvider.TypeName}(""\w+"", """")]
[{PatternAttributeProvider.TypeName}(""\w+"", ""xyz"")]
partial class Test
{{
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