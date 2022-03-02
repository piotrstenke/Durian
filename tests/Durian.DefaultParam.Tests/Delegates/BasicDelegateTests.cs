// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Xunit;

namespace Durian.Analysis.DefaultParam.Tests.Delegates
{
    public sealed class BasicDelegateTests : DefaultParamGeneratorTestBase
    {
        [Fact]
        public void DoesNotRemoveConstraintOfNonDefaultParam()
        {
            string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	delegate void Del<T, [{DefaultParamAttributeProvider.TypeName}(typeof(string))]U, [{DefaultParamAttributeProvider.TypeName}(typeof(float))]V>() where T : unmanaged where U : class where V : notnull;
}}
";
            string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Del<T, U, V>")}
	delegate void Del<T, U>() where T : unmanaged where U : class;

	{GetCodeGenerationAttributes("Test.Del<T, U, V>")}
	delegate void Del<T>() where T : unmanaged;
}}
";
            Assert.True(RunGenerator(input).Compare(expected));
        }

        [Fact]
        public void Generates_When_DelegateWithNameSameButOtherParametersExistsInGlobal()
        {
            string input =
@$"using {DurianStrings.MainNamespace};

delegate void Del(string value);

delegate void Del<T, [{DefaultParamAttributeProvider.TypeName}(typeof(string)]U>(U value);
";

            string expected =
@$"{GetCodeGenerationAttributes("Del<T, U>")}
delegate void Del<T>(string value);
";
            Assert.True(RunGenerator(input).Compare(expected));
        }

        [Fact]
        public void Generates_When_DelegateWithSameNameButOtherParametersExistsInBaseType()
        {
            string input =
@$"using {DurianStrings.MainNamespace};

class Parent
{{
	delegate void Del(string value);
}}

partial class Test : Parent
{{
	delegate void Del<T, [{DefaultParamAttributeProvider.TypeName}(typeof(string)]U>(U value);
}}
";

            string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Del<T, U>")}
	delegate void Del<T>(string value);
}}
";
            Assert.True(RunGenerator(input).Compare(expected));
        }

        [Fact]
        public void Generates_When_DelegateWithSameNameButOtherParametersExistsInSameType()
        {
            string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	delegate void Del<T, [{DefaultParamAttributeProvider.TypeName}(typeof(string)]U>(U value);

	delegate void Del(string value);
}}
";

            string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Del<T, U>")}
	delegate void Del<T>(string value);
}}
";
            Assert.True(RunGenerator(input).Compare(expected));
        }

        [Fact]
        public void HandlesDelegateWithMultipleTypeParameters_When_AllParametersAreDefaultParam()
        {
            string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	delegate void Del<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T, [{DefaultParamAttributeProvider.TypeName}(typeof(string))]U>(T value);
}}
";

            string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Del<T, U>")}
	delegate void Del<T>(T value);

	{GetCodeGenerationAttributes("Test.Del<T, U>")}
	delegate void Del(int value);
}}
";
            Assert.True(RunGenerator(input).Compare(expected));
        }

        [Fact]
        public void HandlesDelegateWithMultipleTypeParameters_When_OnlySomeParametersAreDefaultParam()
        {
            string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	delegate void Del<T, [{DefaultParamAttributeProvider.TypeName}(typeof(string))]U>(T value);
}}
";

            string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Del<T, U>")}
	delegate void Del<T>(T value);
}}
";
            Assert.True(RunGenerator(input).Compare(expected));
        }

        [Fact]
        public void HandlesDelegateWithOneTypeParameter()
        {
            string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	delegate void Del<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>(T value);
}}
";

            string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Del<T>")}
	delegate void Del(int value);
}}
";
            Assert.True(RunGenerator(input).Compare(expected));
        }

        [Fact]
        public void PreservesModifiers()
        {
            string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	public delegate void Del<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>(T value);
}}
";

            string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Del<T>")}
	public delegate void Del(string value);
}}
";
            Assert.True(RunGenerator(input).Compare(expected));
        }

        [Fact]
        public void PreservesTargetAttributes()
        {
            string input =
@$"using System;
using {DurianStrings.MainNamespace};

partial class Test
{{
	[CLSCompliant(true)]
	[Obsolete]
	delegate void Del<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>(T value);
}}
";

            string expected =
@$"using System;

partial class Test
{{
	{GetCodeGenerationAttributes("Test.Del<T>")}
	[CLSCompliant(true)]
	[Obsolete]
	delegate void Del(int value);
}}
";
            Assert.True(RunGenerator(input).Compare(expected));
        }

        [Fact]
        public void PreservesVariance()
        {
            string input =
$@"using {DurianStrings.MainNamespace};

delegate void Del<[{DefaultParamAttributeProvider.TypeName}(typeof(float))]out T, [{DefaultParamAttributeProvider.TypeName}(typeof(string))]in U, [{DefaultParamAttributeProvider.TypeName}(typeof(int))]V>();
";
            string expected =
$@"{GetCodeGenerationAttributes("Del<T, U, V>")}
delegate void Del<out T, in U>();

{GetCodeGenerationAttributes("Del<T, U, V>")}
delegate void Del<out T>();

{GetCodeGenerationAttributes("Del<T, U, V>")}
delegate void Del();
";

            Assert.True(RunGenerator(input).Compare(expected));
        }

        [Fact]
        public void PreservesVarianceOfParentInterface()
        {
            string input =
$@"using {DurianStrings.MainNamespace};

partial interface ITest<in TType, out TName>
{{
	delegate void Del<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>();
}}
";
            string expected =
$@"partial interface ITest<in TType, out TName>
{{
	{GetCodeGenerationAttributes("ITest<TType, TName>.Del<T>")}
	delegate void Del();
}}";

            Assert.True(RunGenerator(input).Compare(expected));
        }

        [Fact]
        public void ProperlyHandlesTypeParameterOfParentType()
        {
            string input =
$@"using {DurianStrings.MainNamespace};

partial class Test<TNumber> where TNumber : class
{{
	delegate TNumber Del<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>(T value, TNumber number);
}}
";

            string expected =
@$"partial class Test<TNumber>
{{
	{GetCodeGenerationAttributes("Test<TNumber>.Del<T>")}
	delegate TNumber Del(int value, TNumber number);
}}
";
            Assert.True(RunGenerator(input).Compare(expected));
        }

        [Fact]
        public void RemovesConstraintsOfMultipleDefaultParams()
        {
            string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	delegate void Del<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T, [{DefaultParamAttributeProvider.TypeName}(typeof(string))]U, [{DefaultParamAttributeProvider.TypeName}(typeof(float))]V>() where T : unmanaged where U : class where V : notnull;
}}
";

            string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Del<T, U, V>")}
	delegate void Del<T, U>() where T : unmanaged where U : class;

	{GetCodeGenerationAttributes("Test.Del<T, U, V>")}
	delegate void Del<T>() where T : unmanaged;

	{GetCodeGenerationAttributes("Test.Del<T, U, V>")}
	delegate void Del();
}}
";
            Assert.True(RunGenerator(input).Compare(expected));
        }

        [Fact]
        public void RemovesConstraintsOfSingleDefaultParam()
        {
            string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	delegate void Del<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>() where T : unmanaged;
}}
";

            string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Del<T>")}
	delegate void Del();
}}
";

            Assert.True(RunGenerator(input).Compare(expected));
        }

        [Fact]
        public void ReplacesReferencesIsConstraintsAndParameters()
        {
            string input =
@$"using {DurianStrings.MainNamespace};
using System.Collections;
using System.Collections.Generic;

partial class Test
{{
	delegate void Del<T, [{DefaultParamAttributeProvider.TypeName}(typeof(System.Collections.IEnumerable))]U>(U value) where T : IEnumerable<U>;
}}
";

            string expected =
@$"using System.Collections;
using System.Collections.Generic;

partial class Test
{{
	{GetCodeGenerationAttributes("Test.Del<T, U>")}
	delegate void Del<T>(IEnumerable value) where T : IEnumerable<IEnumerable>;
}}
";
            Assert.True(RunGenerator(input).Compare(expected));
        }

        [Fact]
        public void SkipsContainingTypeAttributes()
        {
            string input =
@$"using System;
using {DurianStrings.MainNamespace};

[Serializable]
partial class Test
{{
	delegate void Del<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>(T value);
}}
";

            string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Del<T>")}
	delegate void Del(int value);
}}
";
            Assert.True(RunGenerator(input).Compare(expected));
        }

        [Fact]
        public void SkipsDelegate_When_HasNoDefaultParamAttribute()
        {
            string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	delegate void Del<T>(T value);
}}";

            Assert.False(RunGenerator(input).IsGenerated);
        }

        [Fact]
        public void Success_When_HasTwoDefaultParam_And_FirstIsConstraintOfSecond()
        {
            string input =
@$"using {DurianStrings.MainNamespace};
using System.Collections;

partial class Test
{{
	delegate U Del<[{DefaultParamAttributeProvider.TypeName}(typeof(ICollection))]T, [{DefaultParamAttributeProvider.TypeName}(typeof(IEnumerable))]U>(T value) where T : U;
}}
";

            string expected =
@$"using System.Collections;

partial class Test
{{
	{GetCodeGenerationAttributes("Test.Del<T, U>")}
	delegate IEnumerable Del<T>(T value) where T : IEnumerable;

	{GetCodeGenerationAttributes("Test.Del<T, U>")}
	delegate IEnumerable Del(ICollection value);
}}
";
            Assert.True(RunGenerator(input).Compare(expected));
        }

        [Fact]
        public void Success_When_IsArray_And_IsNotConstraint()
        {
            string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	public delegate void Del<[{DefaultParamAttributeProvider.TypeName}(typeof(string[])]T>(T value);
}}
";

            string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Del<T>")}
	public delegate void Del(string[] value);
}}
";
            Assert.True(RunGenerator(input).Compare(expected));
        }

        [Fact]
        public void Success_When_IsGenericType()
        {
            string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	public delegate void Del<[{DefaultParamAttributeProvider.TypeName}(typeof(System.Collections.Generic.List<int>)]T>(T value);
}}
";

            string expected =
@$"using System.Collections.Generic;

partial class Test
{{
	{GetCodeGenerationAttributes("Test.Del<T>")}
	public delegate void Del(List<int> value);
}}
";
            Assert.True(RunGenerator(input).Compare(expected));
        }

        [Fact]
        public void Success_When_IsObject_And_IsNotConstraint()
        {
            string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	public delegate void Del<[{DefaultParamAttributeProvider.TypeName}(typeof(object))]T>(T value);
}}
";

            string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Del<T>")}
	public delegate void Del(object value);
}}
";
            Assert.True(RunGenerator(input).Compare(expected));
        }

        [Fact]
        public void Success_When_IsSystemArray_And_IsNotConstraint()
        {
            string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	public delegate void Del<[{DefaultParamAttributeProvider.TypeName}(typeof(System.Array))]T>(T value);
}}
";

            string expected =
@$"using System;

partial class Test
{{
	{GetCodeGenerationAttributes("Test.Del<T>")}
	public delegate void Del(Array value);
}}
";
            Assert.True(RunGenerator(input).Compare(expected));
        }

        [Fact]
        public void Success_When_IsSystemValueType_And_IsNotConstraint()
        {
            string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	public delegate void Del<[{DefaultParamAttributeProvider.TypeName}(typeof(System.ValueType))]T>(T value);
}}
";

            string expected =
@$"using System;

partial class Test
{{
	{GetCodeGenerationAttributes("Test.Del<T>")}
	public delegate void Del(ValueType value);
}}
";
            Assert.True(RunGenerator(input).Compare(expected));
        }

        [Fact]
        public void Success_When_IsValueType_And_IsNotConstraint()
        {
            string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	public delegate void Del<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>(T value);
}}
";

            string expected =
@$"partial class Test
{{
	{GetCodeGenerationAttributes("Test.Del<T>")}
	public delegate void Del(int value);
}}
";
            Assert.True(RunGenerator(input).Compare(expected));
        }

        [Fact]
        public void WritesAllContainingNamespacesAndTypes()
        {
            string input =
@$"namespace N1
{{
	namespace N2
	{{
		public partial interface Parent
		{{
			public partial struct Child
			{{
				public partial class Test
				{{
					public delegate void Del<[Durian.DefaultParam((typeof(int)))]T>();
				}}
			}}
		}}
	}}
}}
";

            string expected =
@$"namespace N1.N2
{{
	public partial interface Parent
	{{
		public partial struct Child
		{{
			public partial class Test
			{{
				{GetCodeGenerationAttributes("N1.N2.Parent.Child.Test.Del<T>", 4)}
				public delegate void Del();
			}}
		}}
	}}
}}
";
            Assert.True(RunGenerator(input).Compare(expected));
        }

        [Fact]
        public void WritesDelegate_When_IsGlobal()
        {
            string input =
@$"using {DurianStrings.MainNamespace};

delegate void Del<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>();
";

            string expected =
@$"
{GetCodeGenerationAttributes("Del<T>", 0)}
delegate void Del();
";

            Assert.True(RunGenerator(input).Compare(expected));
        }

        [Fact]
        public void WritesDelegate_When_IsInGenericType()
        {
            string input =
@$"using {DurianStrings.MainNamespace};

partial class Test<TNumber>
{{
	delegate void Del<[{DefaultParamAttributeProvider.TypeName}(typeof(string)]T>(T value);
}}
";

            string expected =
@$"partial class Test<TNumber>
{{
	{GetCodeGenerationAttributes("Test<TNumber>.Del<T>")}
	delegate void Del(string value);
}}
";
            Assert.True(RunGenerator(input).Compare(expected));
        }

        [Fact]
        public void WritesDelegate_When_IsInGenericTypeWithConstraints()
        {
            string input =
@$"using {DurianStrings.MainNamespace};

partial class Test<TNumber> where TNumber : class
{{
	delegate void Del<[{DefaultParamAttributeProvider.TypeName}(typeof(string)]T>(T value);
}}
";

            string expected =
@$"partial class Test<TNumber>
{{
	{GetCodeGenerationAttributes("Test<TNumber>.Del<T>")}
	delegate void Del(string value);
}}
";
            Assert.True(RunGenerator(input).Compare(expected));
        }

        [Fact]
        public void WritesDelegate_When_IsIsNamespace()
        {
            string input =
@$"using {DurianStrings.MainNamespace};

namespace Test
{{
	public delegate void Del<[{DefaultParamAttributeProvider.TypeName}(typeof(string)]T>(T value);
}}
";

            string expected =
@$"namespace Test
{{
	{GetCodeGenerationAttributes("Test.Del<T>")}
	public delegate void Del(string value);
}}
";
            Assert.True(RunGenerator(input).Compare(expected));
        }

        [Fact]
        public void WritesSortedUsings()
        {
            string input =
@$"using {DurianStrings.MainNamespace};
using System.Numerics;

partial class Test
{{
	delegate void Del<[{DefaultParamAttributeProvider.TypeName}(typeof(System.Int32))]T>(BigInteger integer);
}}
";

            string expected =
@$"using System.Numerics;

partial class Test
{{
	{GetCodeGenerationAttributes("Test.Del<T>")}
	delegate void Del(BigInteger integer);
}}
";
            Assert.True(RunGenerator(input).Compare(expected));
        }
    }
}