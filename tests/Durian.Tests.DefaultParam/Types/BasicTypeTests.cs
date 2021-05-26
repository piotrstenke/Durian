using Durian.Generator;
using Xunit;

namespace Durian.Tests.DefaultParam.Types
{
	public sealed class BasicTypeTests : DefaultParamGeneratorTest
	{
		[Fact]
		public void SkipsType_When_HasNoDefaultParamAttribute()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent<T>
{{
}}";

			Assert.False(RunGenerator(input).IsGenerated);
		}

		[Fact]
		public void SkipsContainingTypeAttributes()
		{
			string input =
@$"using System;
using {DurianStrings.MainNamespace};

[Serializable]
partial class Parent
{{
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T>
	{{
	}}
}}
";

			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	class Test
	{{
	}}
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

partial class Parent
{{
	[CLSCompliant(true)]
	[Obsolete]
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T>
	{{
	}}
}}
";

			string expected =
@$"using System;

partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	[CLSCompliant(true)]
	[Obsolete]
	class Test
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void HandlesTypeWithOneTypeParameter()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T>
	{{
	}}
}}
";

			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	class Test
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void HandlesTypeWithMultipleTypeParameters_When_AllParametersAreDefaultParam()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T, [{nameof(DefaultParamAttribute)}(typeof(string))]U>
	{{
	}}
}}
";

			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T, U>")}
	class Test<T>
	{{
	}}

	{GetCodeGenerationAttributes("Parent.Test<T, U>")}
	class Test
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void HandlesTypeWithMultipleTypeParameters_When_OnlySomeParametersAreDefaultParam()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{
	class Test<T, [{nameof(DefaultParamAttribute)}(typeof(string))]U>
	{{
	}}
}}
";

			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T, U>")}
	class Test<T>
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void ReplacesAllReferencesToParameter()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using System.Collections;
using System;
using System.Collections.Generic;

partial class Parent
{{
	class Test<T, [{nameof(DefaultParamAttribute)}(typeof(System.Collections.IEnumerable))]U> where T : IEnumerable<U>
	{{
		private static U _U;

		public void Method(U value)
		{{
			List<U> list = new List<U>();
			Type type = typeof(U);
		}}
	}}
}}
";

			string expected =
@$"using System;
using System.Collections;
using System.Collections.Generic;

partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T, U>")}
	class Test<T> where T : IEnumerable<IEnumerable>
	{{
		private static IEnumerable _U;

		public void Method(IEnumerable value)
		{{
			List<IEnumerable> list = new List<IEnumerable>();
			Type type = typeof(IEnumerable);
		}}
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void RemovesConstraintsOfSingleDefaultParam()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T> where T : unmanaged
	{{
	}}
}}
";

			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	class Test
	{{
	}}
}}
";

			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void RemovesConstraintsOfMultipleDefaultParams()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T, [{nameof(DefaultParamAttribute)}(typeof(string))]U, [{nameof(DefaultParamAttribute)}(typeof(float))]V> where T : unmanaged where U : class where V : notnull
	{{
	}}
}}
";

			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T, U, V>")}
	class Test<T, U> where T : unmanaged where U : class
	{{
	}}

	{GetCodeGenerationAttributes("Parent.Test<T, U, V>")}
	class Test<T> where T : unmanaged
	{{
	}}

	{GetCodeGenerationAttributes("Parent.Test<T, U, V>")}
	class Test
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void DoesNotRemoveConstraintOfNonDefaultParam()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{
	class Test<T, [{nameof(DefaultParamAttribute)}(typeof(string))]U, [{nameof(DefaultParamAttribute)}(typeof(float))]V> where T : unmanaged where U : class where V : notnull
	{{
	}}
}}
";
			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T, U, V>")}
	class Test<T, U> where T : unmanaged where U : class
	{{
	}}

	{GetCodeGenerationAttributes("Parent.Test<T, U, V>")}
	class Test<T> where T : unmanaged
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void PreservesModifiers()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{
	public class Test<[{nameof(DefaultParamAttribute)}(typeof(string)]T>
	{{
	}}
}}
";

			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	public class Test
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}


		[Fact]
		public void WritesSortedUsings()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using System.Collections.Generic;
using System.Numerics;
using System;

partial class Parent
{{
	class Test<[{nameof(DefaultParamAttribute)}(typeof(BigInteger))]T>
	{{
		List<DateTime> list = new();
		System.Collections.ICollection = list;
		T value;
	}}
}}
";

			string expected =
@$"using System;
using System.Collections.Generic;
using System.Numerics;

partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	class Test
	{{
		List<DateTime> list = new();
		System.Collections.ICollection = list;
		BigInteger value;
	}}
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
				public partial class Parent
				{{
					public class Test<[Durian.DefaultParam((typeof(int)))]T>
					{{
					}}
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
			public partial class Parent
			{{
				{GetCodeGenerationAttributes("N1.N2.Parent.Child.Parent.Test<T>", 4)}
				public class Test
				{{
				}}
			}}
		}}
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void WritesType_When_IsGlobal()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T>
{{
}}
";

			string expected =
@$"
{GetCodeGenerationAttributes("Test<T>", 0)}
class Test
{{
}}
";

			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void WritesType_When_IsIsNamespace()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

namespace Parent
{{
	public class Test<[{nameof(DefaultParamAttribute)}(typeof(string)]T>
	{{
	}}
}}
";

			string expected =
@$"namespace Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	public class Test
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void WritesType_When_IsInGenericType()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent<TNumber>
{{
	class Test<[{nameof(DefaultParamAttribute)}(typeof(string)]T>
	{{
	}}
}}
";

			string expected =
@$"partial class Parent<TNumber>
{{
	{GetCodeGenerationAttributes("Parent<TNumber>.Test<T>")}
	class Test
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void WritesType_When_IsInGenericTypeWithConstraints()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent<TNumber> where TNumber : class
{{
	class Test<[{nameof(DefaultParamAttribute)}(typeof(string)]T>
	{{
	}}
}}
";

			string expected =
@$"partial class Parent<TNumber>
{{
	{GetCodeGenerationAttributes("Parent<TNumber>.Test<T>")}
	class Test
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void PreservesVariance_When_IsInterface()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

interface ITest<[{nameof(DefaultParamAttribute)}(typeof(float))]out T, [{nameof(DefaultParamAttribute)}(typeof(string))]in U, [{nameof(DefaultParamAttribute)}(typeof(int))]V>
{{
}}
";
			string expected =
$@"{GetCodeGenerationAttributes("ITest<T, U, V>")}
interface ITest<out T, in U>
{{
}}

{GetCodeGenerationAttributes("ITest<T, U, V>")}
interface ITest<out T>
{{
}}

{GetCodeGenerationAttributes("ITest<T, U, V>")}
interface ITest
{{
}}
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
	class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
	{{
	}}
}}
";
			string expected =
$@"partial interface ITest<in TType, out TName>
{{
	{GetCodeGenerationAttributes("ITest<TType, TName>.Test<T>")}
	class Test
	{{
	}}
}}";

			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Generates_When_TypeWithSameNameButOtherParametersExistsInSameType()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{
	class Test<T, [{nameof(DefaultParamAttribute)}(typeof(string)]U>
	{{
	}}

	class Test
	{{
	}}
}}
";

			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T, U>")}
	class Test<T>
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Generates_When_TypeWithSameNameButOtherParametersExistsInBaseType()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

class Parent
{{
	class Test
}}

partial class Parent : Parent
{{
	class Test<T, [{nameof(DefaultParamAttribute)}(typeof(string)]U>
	{{
	}}
}}
";

			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T, U>")}
	class Test<T>
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Generates_When_TypeWithNameSameButOtherParametersExistsInGlobal()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

class Test
{{
}}

class Test<T, [{nameof(DefaultParamAttribute)}(typeof(string)]U>
{{
}}
";

			string expected =
@$"{GetCodeGenerationAttributes("Test<T, U>")}
class Test<T>
{{
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void ProperlyHandlesTypeParameterOfParentType()
		{
			string input =
$@"using {DurianStrings.MainNamespace};

partial class Parent<TNumber> where TNumber : class
{{
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T>
	{{
		TNumber number;
		T Value {{ get; }}
	}}
}}
";

			string expected =
@$"partial class Parent<TNumber>
{{
	{GetCodeGenerationAttributes("Parent<TNumber>.Test<T>")}
	class Test
	{{
		TNumber number;
		int Value {{ get; }}
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_ArgumentIsGenericType()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{
	public class Test<[{nameof(DefaultParamAttribute)}(typeof(System.Collections.Generic.List<int>)]T>
	{{
		T value = default;
	}}
}}
";

			string expected =
@$"using System.Collections.Generic;

partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	public class Test
	{{
		List<int> value = default;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_IsArray_And_IsNotConstraint()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{
	public class Test<[{nameof(DefaultParamAttribute)}(typeof(string[])]T>
	{{
		T t = default;
	}}
}}
";

			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	public class Test
	{{
		string[] t = default;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_IsSystemArray_And_IsNotConstraint()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{
	public class Test<[{nameof(DefaultParamAttribute)}(typeof(System.Array))]T>
	{{
		T t = default;
	}}
}}
";

			string expected =
@$"using System;

partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	public class Test
	{{
		Array t = default;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_IsSystemValueType_And_IsNotConstraint()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{
	public class Test<[{nameof(DefaultParamAttribute)}(typeof(System.ValueType))]T>
	{{
		T t = default;
	}}
}}
";

			string expected =
@$"using System;

partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	public class Test
	{{
		ValueType t = default;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_IsObject_And_IsNotConstraint()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{
	public class Test<[{nameof(DefaultParamAttribute)}(typeof(object))]T>
	{{
		T t = default;
	}}
}}
";

			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	public class Test
	{{
		object t = default;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_IsValueType_And_IsNotConstraint()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{
	public class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T>
	{{
		T t = default;
	}}
}}
";

			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	public class Test
	{{
		int t = default;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_IsLessAccessible_And_IsNotPartOfSignature()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{
	private class C
	{{
	}}

	public class Test<[{nameof(DefaultParamAttribute)}(typeof(C))]T>
	{{
		T t = default;
	}}
}}
";

			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	public class Test
	{{
		C t = default;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_HasTwoDefaultParam_And_FirstIsConstraintOfSecond()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using System.Collections;

partial class Parent
{{
	class Test<[{nameof(DefaultParamAttribute)}(typeof(ICollection))]T, [{nameof(DefaultParamAttribute)}(typeof(IEnumerable))]U> where T : U
	{{
		T t = default;
	}}
}}
";

			string expected =
@$"using System.Collections;

partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T, U>")}
	class Test<T> where T : IEnumerable
	{{
		T t = default;
	}}

	{GetCodeGenerationAttributes("Parent.Test<T, U>")}
	class Test
	{{
		ICollection t = default;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}
	}
}
