using Xunit;

namespace Durian.Analysis.DefaultParam.Tests.Methods;

public sealed class BasicMethodTests : DefaultParamGeneratorTest
{
	[Fact]
	public void DoesNotRemoveConstraintOfNonDefaultParam()
	{
		string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.MethodConvention} = {DPMethodConventionProvider.TypeName}.{DPMethodConventionProvider.Copy}]
partial class Test
{{
	void Method<T, [{DefaultParamAttributeProvider.TypeName}(typeof(string))]U, [{DefaultParamAttributeProvider.TypeName}(typeof(float))]V>() where T : unmanaged where U : class where V : notnull
	{{
	}}
}}
";
		string expected =
@$"internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T, U, V>()")}
	void Method<T, U>() where T : unmanaged where U : class
	{{
	}}

	{GetCodeGenerationAttributes("Test.Method<T, U, V>()")}
	void Method<T>() where T : unmanaged
	{{
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void GeneratesForAbstractMethod()
	{
		string input =
@$"using {DurianStrings.MainNamespace};
using System.Collections;

partial abstract class Test
{{
	abstract void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>();
}}
";
		string expected =
@$"internal abstract partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T>()")}
	abstract void Method();
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void HandlesMethodWithMultipleTypeParameters_When_AllParametersAreDefaultParam()
	{
		string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.MethodConvention} = {DPMethodConventionProvider.TypeName}.{DPMethodConventionProvider.Copy}]
partial class Test
{{
	void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T, [{DefaultParamAttributeProvider.TypeName}(typeof(string))]U>(T value)
	{{
		U name = default;
	}}
}}
";

		string expected =
@$"internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T, U>(T)")}
	void Method<T>(T value)
	{{
		string name = default;
	}}

	{GetCodeGenerationAttributes("Test.Method<T, U>(T)")}
	void Method(int value)
	{{
		string name = default;
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void HandlesMethodWithMultipleTypeParameters_When_OnlySomeParametersAreDefaultParam()
	{
		string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.MethodConvention} = {DPMethodConventionProvider.TypeName}.{DPMethodConventionProvider.Copy}]
partial class Test
{{
	void Method<T, [{DefaultParamAttributeProvider.TypeName}(typeof(string))]U>(T value)
	{{
		U name = default;
	}}
}}
";

		string expected =
@$"internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T, U>(T)")}
	void Method<T>(T value)
	{{
		string name = default;
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void HandlesMethodWithOneTypeParameter()
	{
		string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>(T value)
	{{
	}}
}}
";

		string expected =
@$"internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T>(T)")}
	void Method(int value)
	{{
		Method<int>(value);
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

partial class Test
{{
	public static void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(string)]T>(T value)
	{{
	}}
}}
";

		string expected =
@$"internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T>(T)")}
	public static void Method(string value)
	{{
		Method<string>(value);
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

partial class Test
{{
	[CLSCompliant(true)]
	[Obsolete]
	void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>(T value)
	{{
	}}
}}
";

		string expected =
@$"using System;

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T>(T)")}
	[CLSCompliant(true)]
	[Obsolete]
	void Method(int value)
	{{
		Method<int>(value);
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void ProperlyHandlesTypeParameterOfParentType()
	{
		string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.MethodConvention} = {DPMethodConventionProvider.TypeName}.{DPMethodConventionProvider.Copy}]
partial class Test<TNumber> where TNumber : class
{{
	TNumber Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>(T value, TNumber number)
	{{
		return default;
	}}
}}
";

		string expected =
@$"internal partial class Test<TNumber> where TNumber : class
{{
	{GetCodeGenerationAttributes("Test<TNumber>.Method<T>(T, TNumber)")}
	TNumber Method(int value, TNumber number)
	{{
		return default;
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

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.MethodConvention} = {DPMethodConventionProvider.TypeName}.{DPMethodConventionProvider.Copy}]
partial class Test
{{
	void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T, [{DefaultParamAttributeProvider.TypeName}(typeof(string))]U, [{DefaultParamAttributeProvider.TypeName}(typeof(float))]V>() where T : unmanaged where U : class where V : notnull
	{{
	}}
}}
";

		string expected =
@$"internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T, U, V>()")}
	void Method<T, U>() where T : unmanaged where U : class
	{{
	}}

	{GetCodeGenerationAttributes("Test.Method<T, U, V>()")}
	void Method<T>() where T : unmanaged
	{{
	}}

	{GetCodeGenerationAttributes("Test.Method<T, U, V>()")}
	void Method()
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

partial class Test
{{
	void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>() where T : unmanaged
	{{
	}}
}}
";

		string expected =
@$"internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T>()")}
	void Method()
	{{
		Method<int>();
	}}
}}
";

		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void ReplacesAllReferencesToTypeParameter()
	{
		string input =
@$"using System;
using System.Collections;
using System.Collections.Generic;
using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.MethodConvention} = {DPMethodConventionProvider.TypeName}.{DPMethodConventionProvider.Copy}]
partial class Test
{{
	void Method<T, [{DefaultParamAttributeProvider.TypeName}(typeof(System.Collections.IEnumerable))]U>(U value) where T : IEnumerable<U>
	{{
		U v = default(U);
		object obj = (U)2;
		Type t = typeof(U);
		List<U> list = new List<U>();
	}}
}}
";

		string expected =
@$"using System;
using System.Collections;
using System.Collections.Generic;

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T, U>(U)")}
	void Method<T>(IEnumerable value) where T : IEnumerable<IEnumerable>
	{{
		IEnumerable v = default(IEnumerable);
		object obj = (IEnumerable)2;
		Type t = typeof(IEnumerable);
		List<IEnumerable> list = new List<IEnumerable>();
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
partial class Test
{{
	void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>(T value)
	{{
	}}
}}
";

		string expected =
@$"internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T>(T)")}
	void Method(int value)
	{{
		Method<int>(value);
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void SkipsMethod_When_HasNoDefaultParamAttribute()
	{
		string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial class Test
{{
	void Method<T>(T value)
	{{
	}}
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
	void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(ICollection))]T, [{DefaultParamAttributeProvider.TypeName}(typeof(IEnumerable))]U>(T value) where T : U
	{{
		T t = default;
	}}
}}
";

		string expected =
@$"using System.Collections;

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T, U>(T)")}
	void Method<T>(T value) where T : IEnumerable
	{{
		Method<T, IEnumerable>(value);
	}}

	{GetCodeGenerationAttributes("Test.Method<T, U>(T)")}
	void Method(ICollection value)
	{{
		Method<ICollection, IEnumerable>(value);
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

partial class Test
{{
	public static void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(string[])]T>(T value)
	{{
		T t = default;
	}}
}}
";

		string expected =
@$"internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T>(T)")}
	public static void Method(string[] value)
	{{
		Method<string[]>(value);
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_IsExtensionMethod()
	{
		string input =
@$"using {DurianStrings.MainNamespace};

partial static class Test
{{
	public static void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>(this T value)
	{{
		T t = default;
	}}
}}
";

		string expected =
@$"internal static partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T>(T)")}
	public static void Method(this int value)
	{{
		Method<int>(value);
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_IsGenericType()
	{
		string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.MethodConvention} = {DPMethodConventionProvider.TypeName}.{DPMethodConventionProvider.Copy}]
partial class Test
{{
	public static void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(System.Collections.Generic.List<int>)]T>()
	{{
		T value = default;
	}}
}}
";

		string expected =
@$"using System.Collections.Generic;

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T>()")}
	public static void Method()
	{{
		List<int> value = default;
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

partial class Test
{{
	private class C
	{{
	}}

	public static void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(C))]T>()
	{{
		T t = default;
	}}
}}
";

		string expected =
@$"internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T>()")}
	public static void Method()
	{{
		Method<C>();
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

partial class Test
{{
	public static void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(object))]T>(T value)
	{{
		T t = default;
	}}
}}
";

		string expected =
@$"internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T>(T)")}
	public static void Method(object value)
	{{
		Method<object>(value);
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

partial class Test
{{
	public static void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(System.Array))]T>(T value)
	{{
		T t = default;
	}}
}}
";

		string expected =
@$"using System;

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T>(T)")}
	public static void Method(Array value)
	{{
		Method<Array>(value);
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

partial class Test
{{
	public static void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(System.ValueType))]T>(T value)
	{{
		T t = default;
	}}
}}
";

		string expected =
@$"using System;

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T>(T)")}
	public static void Method(ValueType value)
	{{
		Method<ValueType>(value);
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

partial class Test
{{
	public static void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>(T value)
	{{
		T t = default;
	}}
}}
";

		string expected =
@$"internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T>(T)")}
	public static void Method(int value)
	{{
		Method<int>(value);
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
				public partial class Test
				{{
					public void Method<[Durian.DefaultParam((typeof(int)))]T>()
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
		partial struct Child
		{{
			public partial class Test
			{{
				{GetCodeGenerationAttributes("N1.N2.Parent.Child.Test.Method<T>()", 4)}
				public void Method()
				{{
					Method<int>();
				}}
			}}
		}}
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void WritesMethod_When_IsInGenericType()
	{
		string input =
@$"using {DurianStrings.MainNamespace};

partial class Test<TNumber>
{{
	void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>(T value)
	{{
	}}
}}
";

		string expected =
@$"internal partial class Test<TNumber>
{{
	{GetCodeGenerationAttributes("Test<TNumber>.Method<T>(T)")}
	void Method(string value)
	{{
		Method<string>(value);
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void WritesMethod_When_IsInGenericTypeWithConstraints()
	{
		string input =
@$"using {DurianStrings.MainNamespace};

partial class Test<TNumber> where TNumber : class
{{
	void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>(T value)
	{{
	}}
}}
";

		string expected =
@$"internal partial class Test<TNumber> where TNumber : class
{{
	{GetCodeGenerationAttributes("Test<TNumber>.Method<T>(T)")}
	void Method(string value)
	{{
		Method<string>(value);
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
using System.Numerics;
using System.Collections;
using {DurianStrings.ConfigurationNamespace};
using System;

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.MethodConvention} = {DPMethodConventionProvider.TypeName}.{DPMethodConventionProvider.Copy}]
partial class Test
{{
	void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(System.Int32))]T>()
	{{
		Attribute atrr = null;
		System.Collections.Generic.List<BigInteger> list = new();
		IEnumerable e = default;
	}}
}}
";

		string expected =
@$"using System;
using System.Collections;
using System.Numerics;

internal partial class Test
{{
	{GetCodeGenerationAttributes("Test.Method<T>()")}
	void Method()
	{{
		Attribute atrr = null;
		System.Collections.Generic.List<BigInteger> list = new();
		IEnumerable e = default;
	}}
}}
";
		Assert.True(RunGenerator(input).Compare(expected));
	}
}
