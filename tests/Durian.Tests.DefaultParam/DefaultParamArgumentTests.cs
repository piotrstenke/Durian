// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Generator;
using Xunit;
using static Durian.Generator.DefaultParam.DefaultParamDiagnostics;

namespace Durian.Tests.DefaultParam
{
	public sealed class DefaultParamArgumentTests : DefaultParamGeneratorTest
	{
		[Fact]
		public void Error_When_IsArraySymbol_And_IsConstraint()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	public static void Method<T, [{nameof(DefaultParamAttribute)}(typeof(int[]))]U>() where T : U
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0120_TypeCannotBeUsedWithConstraint.Id));
		}

		[Fact]
		public void Error_When_IsDelegate_And_IsConstraint()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using System;

partial class Test
{{
	public static void Method<T, [{nameof(DefaultParamAttribute)}(typeof(Action))]U>() where T : U
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0120_TypeCannotBeUsedWithConstraint.Id));
		}

		[Fact]
		public void Error_When_IsFunctionPointer()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	public static unsafe void Method<[{nameof(DefaultParamAttribute)}(typeof(delegate*<int>))]T>()
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0121_TypeIsNotValidDefaultParamValue.Id));
		}

		[Fact]
		public void Error_When_IsLessAccessible_And_IsChildOfParameter()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

public partial class Test
{{
	internal class C
	{{
	}}

	public static void Method<[{nameof(DefaultParamAttribute)}(typeof(C))]T, [{nameof(DefaultParamAttribute)}(typeof(string))]U>(T[] value)
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0119_DefaultParamValueCannotBeLessAccessibleThanTargetMember.Id));
		}

		[Fact]
		public void Error_When_IsLessAccessible_And_IsConstraint()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

public partial class Test
{{
	internal class C
	{{
	}}

	public static void Method<T, [{nameof(DefaultParamAttribute)}(typeof(C))]U>() where T : U
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0119_DefaultParamValueCannotBeLessAccessibleThanTargetMember.Id));
		}

		[Fact]
		public void Error_When_IsLessAccessible_And_IsParameter()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

public partial class Test
{{
	internal class C
	{{
	}}

	public static void Method<[{nameof(DefaultParamAttribute)}(typeof(C))]T, [{nameof(DefaultParamAttribute)}(typeof(string))]U>(T value)
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0119_DefaultParamValueCannotBeLessAccessibleThanTargetMember.Id));
		}

		[Fact]
		public void Error_When_IsLessAccessible_And_IsReturnType()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

public partial class Test
{{
	internal class C
	{{
	}}

	public static T Method<[{nameof(DefaultParamAttribute)}(typeof(C))]T, [{nameof(DefaultParamAttribute)}(typeof(string))]U>()
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0119_DefaultParamValueCannotBeLessAccessibleThanTargetMember.Id));
		}

		[Fact]
		public void Error_When_IsObject_And_IsConstraint()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	public static void Method<[{nameof(DefaultParamAttribute)}(typeof(object))]T, [{nameof(DefaultParamAttribute)}(typeof(string))]U>() where U : T
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0120_TypeCannotBeUsedWithConstraint.Id));
		}

		[Fact]
		public void Error_When_IsPointer()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	public static unsafe void Method<[{nameof(DefaultParamAttribute)}(typeof(int*))]T>()
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0121_TypeIsNotValidDefaultParamValue.Id));
		}

		[Fact]
		public void Error_When_IsRefStruct()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	public static void Method<[{nameof(DefaultParamAttribute)}(typeof(System.Span<int>))]T>()
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0121_TypeIsNotValidDefaultParamValue.Id));
		}

		[Fact]
		public void Error_When_IsStaticClass()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	public static void Method<[{nameof(DefaultParamAttribute)}(typeof(System.Linq.Enumerable))]T>()
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0121_TypeIsNotValidDefaultParamValue.Id));
		}

		[Fact]
		public void Error_When_IsSystemArray_And_IsConstraint()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	public static void Method<T, [{nameof(DefaultParamAttribute)}(typeof(System.Array))]U>() where T : U
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0120_TypeCannotBeUsedWithConstraint.Id));
		}

		[Fact]
		public void Error_When_IsSystemValueType_And_IsConstraint()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	public static void Method<T, [{nameof(DefaultParamAttribute)}(typeof(System.ValueType))]U>() where T : U
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0120_TypeCannotBeUsedWithConstraint.Id));
		}

		[Fact]
		public void Error_When_IsUnboundGenericType()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	public static void Method<[{nameof(DefaultParamAttribute)}(typeof(System.Collections.Generic.List<>))]T>()
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0121_TypeIsNotValidDefaultParamValue.Id));
		}

		[Fact]
		public void Error_When_IsVoid()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	public static void Method<[{nameof(DefaultParamAttribute)}(typeof(void))]T>()
	{{
	}}
}}
";

			Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0121_TypeIsNotValidDefaultParamValue.Id));
		}
	}
}
