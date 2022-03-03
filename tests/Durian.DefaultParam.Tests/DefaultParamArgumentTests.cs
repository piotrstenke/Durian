// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.TestServices;
using Xunit;
using static Durian.Analysis.DefaultParam.DefaultParamDiagnostics;

namespace Durian.Analysis.DefaultParam.Tests
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
	public static void Method<T, [{DefaultParamAttributeProvider.TypeName}(typeof(int[]))]U>() where T : U
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
	public static void Method<T, [{DefaultParamAttributeProvider.TypeName}(typeof(Action))]U>() where T : U
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
	public static unsafe void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(delegate*<int>))]T>()
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

	public static void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(C))]T, [{DefaultParamAttributeProvider.TypeName}(typeof(string))]U>(T[] value)
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

	public static void Method<T, [{DefaultParamAttributeProvider.TypeName}(typeof(C))]U>() where T : U
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

	public static void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(C))]T, [{DefaultParamAttributeProvider.TypeName}(typeof(string))]U>(T value)
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

	public static T Method<[{DefaultParamAttributeProvider.TypeName}(typeof(C))]T, [{DefaultParamAttributeProvider.TypeName}(typeof(string))]U>()
	{{
	}}
}}
";
            Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0119_DefaultParamValueCannotBeLessAccessibleThanTargetMember.Id));
        }

        [Fact]
        public void Error_When_IsNull()
        {
            string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	public static void Method<[{DefaultParamAttributeProvider.TypeName}(null)]T>()
	{{
	}}
}}
";

            Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0121_TypeIsNotValidDefaultParamValue.Id));
        }

        [Fact]
        public void Error_When_IsObject_And_IsConstraint()
        {
            string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	public static void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(object))]T, [{DefaultParamAttributeProvider.TypeName}(typeof(string))]U>() where U : T
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
	public static unsafe void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int*))]T>()
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
	public static void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(System.Span<int>))]T>()
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
	public static void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(System.Linq.Enumerable))]T>()
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
	public static void Method<T, [{DefaultParamAttributeProvider.TypeName}(typeof(System.Array))]U>() where T : U
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
	public static void Method<T, [{DefaultParamAttributeProvider.TypeName}(typeof(System.ValueType))]U>() where T : U
	{{
	}}
}}
";
            Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0120_TypeCannotBeUsedWithConstraint.Id));
        }

        [Fact]
        public void Error_When_IsSystemVoid()
        {
            string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	public static void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(System.Void))]T>()
	{{
	}}
}}
";

            Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0121_TypeIsNotValidDefaultParamValue.Id));
        }

        [Fact]
        public void Error_When_IsUnboundGenericType()
        {
            string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	public static void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(System.Collections.Generic.List<>))]T>()
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
	public static void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(void))]T>()
	{{
	}}
}}
";

            Assert.True(RunGenerator(input).HasFailedAndContainsDiagnosticIDs(DUR0121_TypeIsNotValidDefaultParamValue.Id));
        }
    }
}