// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Xunit;

namespace Durian.Analysis.DefaultParam.Tests.Types
{
	public sealed class BasicTypeTests : DefaultParamGeneratorTest
	{
		[Fact]
		public void DoesNotRemoveConstraintOfNonDefaultParam()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Copy}]
partial class Parent
{{
	class Test<T, [{DefaultParamAttributeProvider.TypeName}(typeof(string))]U, [{DefaultParamAttributeProvider.TypeName}(typeof(float))]V> where T : unmanaged where U : class where V : notnull
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
		public void Generates_When_TypeWithNameSameButOtherParametersExistsInGlobal()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

class Test
{{
}}

class Test<T, [{DefaultParamAttributeProvider.TypeName}(typeof(string)]U>
{{
}}
";

			string expected =
@$"{GetCodeGenerationAttributes("Test<T, U>")}
class Test<T> : Test<T, string>
{{
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
	class Test<T, [{DefaultParamAttributeProvider.TypeName}(typeof(string)]U>
	{{
	}}
}}
";

			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T, U>")}
	class Test<T> : Test<T, string>
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Generates_When_TypeWithSameNameButOtherParametersExistsInSameType()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{
	class Test<T, [{DefaultParamAttributeProvider.TypeName}(typeof(string)]U>
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
	class Test<T> : Test<T, string>
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
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T, [{DefaultParamAttributeProvider.TypeName}(typeof(string))]U>
	{{
	}}
}}
";

			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T, U>")}
	class Test<T> : Test<T, string>
	{{
	}}

	{GetCodeGenerationAttributes("Parent.Test<T, U>")}
	class Test : Test<int, string>
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
	class Test<T, [{DefaultParamAttributeProvider.TypeName}(typeof(string))]U>
	{{
	}}
}}
";

			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T, U>")}
	class Test<T> : Test<T, string>
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
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>
	{{
	}}
}}
";

			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	class Test : Test<int>
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
using {DurianStrings.ConfigurationNamespace};

partial class Parent
{{
	public class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string)]T>
	{{
	}}
}}
";

			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	public class Test : Test<string>
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
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>
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
	class Test : Test<int>
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

interface ITest<[{DefaultParamAttributeProvider.TypeName}(typeof(float))]out T, [{DefaultParamAttributeProvider.TypeName}(typeof(string))]in U, [{DefaultParamAttributeProvider.TypeName}(typeof(int))]V>
{{
}}
";
			string expected =
$@"{GetCodeGenerationAttributes("ITest<T, U, V>")}
interface ITest<out T, in U> : ITest<T, U, int>
{{
}}

{GetCodeGenerationAttributes("ITest<T, U, V>")}
interface ITest<out T> : ITest<T, string, int>
{{
}}

{GetCodeGenerationAttributes("ITest<T, U, V>")}
interface ITest : ITest<float, string, int>
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
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
	{{
	}}
}}
";
			string expected =
$@"partial interface ITest<in TType, out TName>
{{
	{GetCodeGenerationAttributes("ITest<TType, TName>.Test<T>")}
	class Test : Test<string>
	{{
	}}
}}";

			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void ProperlyHandlesTypeParameterOfParentType()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Copy}]
partial class Parent<TNumber> where TNumber : class
{{
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>
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
		public void RemovesConstraintsOfMultipleDefaultParams()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Copy}]
partial class Parent
{{
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T, [{DefaultParamAttributeProvider.TypeName}(typeof(string))]U, [{DefaultParamAttributeProvider.TypeName}(typeof(float))]V> where T : unmanaged where U : class where V : notnull
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
		public void RemovesConstraintsOfSingleDefaultParam()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Parent
{{
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T> where T : unmanaged
	{{
	}}
}}
";

			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	class Test : Test<int>
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
using {DurianStrings.ConfigurationNamespace};
using System;
using System.Collections;
using System.Collections.Generic;

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Copy}]
partial class Parent
{{
	class Test<T, [{DefaultParamAttributeProvider.TypeName}(typeof(System.Collections.IEnumerable))]U> where T : IEnumerable<U>
	{{
		private static U _U;

		public void Method(U value)
		{{
			List<U> list = new List<U>();
			Type t = typeof(U);
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
			Type t = typeof(IEnumerable);
		}}
	}}
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
partial class Parent
{{
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>
	{{
	}}
}}
";

			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	class Test : Test<int>
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

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
		public void Success_When_ArgumentIsGenericType()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Copy}]
partial class Parent
{{
	public class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(System.Collections.Generic.List<int>)]T>
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
		public void Success_When_HasTwoDefaultParam_And_FirstIsConstraintOfSecond()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using System.Collections;
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Copy}]
partial class Parent
{{
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(ICollection))]T, [{DefaultParamAttributeProvider.TypeName}(typeof(IEnumerable))]U> where T : U
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

		[Fact]
		public void Success_When_IsArray_And_IsNotConstraint()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Copy}]
partial class Parent
{{
	public class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string[])]T>
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
		public void Success_When_IsLessAccessible_And_IsNotPartOfSignature()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Copy}]
partial class Parent
{{
	private class C
	{{
	}}

	public class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(C))]T>
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
		public void Success_When_IsObject_And_IsNotConstraint()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Copy}]
partial class Parent
{{
	public class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(object))]T>
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
		public void Success_When_IsSystemArray_And_IsNotConstraint()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Copy}]
partial class Parent
{{
	public class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(System.Array))]T>
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
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Copy}]
partial class Parent
{{
	public class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(System.ValueType))]T>
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
		public void Success_When_IsValueType_And_IsNotConstraint()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Copy}]
partial class Parent
{{
	public class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>
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
				public class Test : Test<int>
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
		public void WritesSortedUsings()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using System.Collections.Generic;
using System;
using System.Numerics;
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Copy}]
partial class Parent
{{
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(BigInteger))]T>
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
		public void WritesType_When_IsGlobal()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>
{{
}}
";

			string expected =
@$"{GetCodeGenerationAttributes("Test<T>", 0)}
class Test : Test<int>
{{
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
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string)]T>
	{{
	}}
}}
";

			string expected =
@$"partial class Parent<TNumber>
{{
	{GetCodeGenerationAttributes("Parent<TNumber>.Test<T>")}
	class Test : Test<string>
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
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string)]T>
	{{
	}}
}}
";

			string expected =
@$"partial class Parent<TNumber>
{{
	{GetCodeGenerationAttributes("Parent<TNumber>.Test<T>")}
	class Test : Test<string>
	{{
	}}
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
	public class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string)]T>
	{{
	}}
}}
";

			string expected =
@$"namespace Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	public class Test : Test<string>
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}
	}
}