using Durian.Configuration;
using Durian.Generator;
using Durian.Generator.DefaultParam;
using Xunit;

namespace Durian.Tests.DefaultParam.Types
{
	public sealed class TypeConventionTests : DefaultParamGeneratorTest
	{
		[Fact]
		public void Warning_And_GeneratesAsCopy_When_IsScopedInherit_And_IsSealedType()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.TypeConvention)} = {nameof(DPTypeConvention)}.{nameof(DPTypeConvention.Inherit)})]

sealed class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
{{
	int value;
}}
";
			string expected =
$@"{GetCodeGenerationAttributes("Test<T>", 0)}
sealed class Test
{{
	int value;
}}
";
			SingletonGeneratorTestResult result = RunGenerator(input);
			Assert.True(result.HasSucceededAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0118_ApplyCopyTypeConventionOnStructOrSealedTypeOrTypeWithNoPublicCtor.Id));
			Assert.True(result.Compare(expected));
		}

		[Fact]
		public void Warning_And_GeneratesAsCopy_When_IsScopedInherit_And_IsStruct()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.TypeConvention)} = {nameof(DPTypeConvention)}.{nameof(DPTypeConvention.Inherit)})]

struct Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
{{
	int value;
}}
";
			string expected =
$@"{GetCodeGenerationAttributes("Test<T>", 0)}
struct Test
{{
	int value;
}}
";
			SingletonGeneratorTestResult result = RunGenerator(input);
			Assert.True(result.HasSucceededAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0118_ApplyCopyTypeConventionOnStructOrSealedTypeOrTypeWithNoPublicCtor.Id));
			Assert.True(result.Compare(expected));
		}

		[Fact]
		public void Warning_And_GeneratesAsCopy_When_IsScopedInherit_And_IsStatic()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.TypeConvention)} = {nameof(DPTypeConvention)}.{nameof(DPTypeConvention.Inherit)})]

static class<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
{{
	int value;
}}
";
			string expected =
$@"{GetCodeGenerationAttributes("Test<T>", 0)}
static class Test
{{
	int value;
}}
";
			SingletonGeneratorTestResult result = RunGenerator(input);
			Assert.True(result.HasSucceededAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0118_ApplyCopyTypeConventionOnStructOrSealedTypeOrTypeWithNoPublicCtor.Id));
			Assert.True(result.Compare(expected));
		}

		[Fact]
		public void Warning_And_GeneratesAsCopy_When_IsScopedInherit_And_HasNoAccessibleConstructors()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.TypeConvention)} = {nameof(DPTypeConvention)}.{nameof(DPTypeConvention.Inherit)})]

class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
{{
	T value;

	private Test()
	{{
	}}
}}
";
			string expected =
$@"{GetCodeGenerationAttributes("Test<T>", 0)}
class Test
{{
	string value;

	private Test()
	{{
	}}
}}
";
			SingletonGeneratorTestResult result = RunGenerator(input);
			Assert.True(result.HasSucceededAndContainsDiagnosticIDs(DefaultParamDiagnostics.DUR0118_ApplyCopyTypeConventionOnStructOrSealedTypeOrTypeWithNoPublicCtor.Id));
			Assert.True(result.Compare(expected));
		}

		[Fact]
		public void ProperlyWritesAllTypeArguments()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.TypeConvention)} = {nameof(DPTypeConvention)}.{nameof(DPTypeConvention.Inherit)}]

partial class Parent
{{
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T, [{nameof(DefaultParamAttribute)}(typeof(int))]U, [{nameof(DefaultParamAttribute)}(typeof(string))]V>
	{{
		T t = default;
	}}
}}
";

			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T, U, V>")}
	class Test<T, U> : Test<T, U, string>
	{{
	}}

	{GetCodeGenerationAttributes("Parent.Test<T, U, V>")}
	class Test<T> : Test<T, int, string>
	{{
	}}

	{GetCodeGenerationAttributes("Parent.Test<T, U, V>")}
	class Test : Test<int, int, string>
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Inherits_When_AppliedGlobally()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.TypeConvention)} = {nameof(DPTypeConvention)}.{nameof(DPTypeConvention.Inherit)})]

partial class Parent
{{
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T>
	{{
		T value = default;
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
		public void Inherits_When_AppliedLocally()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial class Parent
{{
	[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.TypeConvention)} = {nameof(DPTypeConvention)}.{nameof(DPTypeConvention.Inherit)})]
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T>
	{{
		T value = default;
	}}
}}
";
			string expected =
@$"using {DurianStrings.ConfigurationNamespace};

partial class Parent
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
		public void Inherits_When_GlobalyFalse_And_LocallyTrue()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.TypeConvention)} = {nameof(DPTypeConvention)}.{nameof(DPTypeConvention.Copy)})]

partial class Parent
{{
	[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.TypeConvention)} = {nameof(DPTypeConvention)}.{nameof(DPTypeConvention.Inherit)})]
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T>
	{{
		T value = default;
	}}
}}
";
			string expected =
@$"using {DurianStrings.ConfigurationNamespace};

partial class Parent
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
		public void Copies_When_GloballyTrue_And_LocallyFalse()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.TypeConvention)} = {nameof(DPTypeConvention)}.{nameof(DPTypeConvention.Inherit)})]

partial class Parent
{{
	[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.TypeConvention)} = {nameof(DPTypeConvention)}.{nameof(DPTypeConvention.Copy)})]
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T>
	{{
		T value = default;
	}}
}}
";
			string expected =
@$"using {DurianStrings.ConfigurationNamespace};

partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	class Test
	{{
		int value = default;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Inherits_When_GloballyFalse_And_InTypeTrue()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.TypeConvention)} = {nameof(DPTypeConvention)}.{nameof(DPTypeConvention.Copy)})]

[{nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.TypeConvention)} = {nameof(DPTypeConvention)}.{nameof(DPTypeConvention.Inherit)})]
partial class Parent
{{
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T>
	{{
		T value = default;
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
		public void Inherits_When_InTypeFalse_And_LocallyTrue()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.TypeConvention)} = {nameof(DPTypeConvention)}.{nameof(DPTypeConvention.Copy)})]
partial class Parent
{{
	[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.TypeConvention)} = {nameof(DPTypeConvention)}.{nameof(DPTypeConvention.Inherit)})]
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T>
	{{
		T value = default;
	}}
}}
";
			string expected =
@$"using {DurianStrings.ConfigurationNamespace};

partial class Parent
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
		public void Copies_When_InTypeTrue_And_LocallyFalse()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.TypeConvention)} = {nameof(DPTypeConvention)}.{nameof(DPTypeConvention.Inherit)})]
partial class Parent
{{
	[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.TypeConvention)} = {nameof(DPTypeConvention)}.{nameof(DPTypeConvention.Copy)})]
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T>
	{{
		T value = default;
	}}
}}
";
			string expected =
@$"using {DurianStrings.ConfigurationNamespace};

partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	class Test
	{{
		int value = default;
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void ProperlyHandlesConstraints_When_IsInherit()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial class Parent
{{
	[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.TypeConvention)} = {nameof(DPTypeConvention)}.{nameof(DPTypeConvention.Inherit)})]
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T, [{nameof(DefaultParamAttribute)}(typeof(string))]U> where T : struct where U : class
	{{
		T value = default;
	}}
}}
";
			string expected =
@$"using {DurianStrings.ConfigurationNamespace};

partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T, U>")}
	class Test<T> : Test<T, string> where T : struct
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
		public void PreservesAttributes_When_IsInherit()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};
using System;

partial class Parent
{{
	[Serializable]
	[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.TypeConvention)} = {nameof(DPTypeConvention)}.{nameof(DPTypeConvention.Inherit)})]
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T>
	{{
		T value = default;
	}}
}}
";
			string expected =
@$"using System;
using {DurianStrings.ConfigurationNamespace};

partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	[Serializable]
	class Test : Test<int>
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void RemovesBaseList_When_IsInherit()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};
using System;

partial class Parent
{{
	[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.TypeConvention)} = {nameof(DPTypeConvention)}.{nameof(DPTypeConvention.Inherit)})]
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T> : Attribute, System.Collections.ICollection
	{{
		T value = default;
	}}
}}
";
			string expected =
@$"using System;
using {DurianStrings.ConfigurationNamespace};

partial class Parent
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
		public void RemovesBaseListAndProperlyHandlesConstraints_When_IsInherit()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};
using System;

partial class Parent
{{
	[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.TypeConvention)} = {nameof(DPTypeConvention)}.{nameof(DPTypeConvention.Inherit)})]
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T, [{nameof(DefaultParamAttribute)}(typeof(string))]U> : Attribute, System.Collections.ICollection where T : struct where U : class
	{{
		T value = default;
	}}
}}
";
			string expected =
@$"using System;
using {DurianStrings.ConfigurationNamespace};

partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T, U>")}
	class Test<T> : Test<T, string> where T : struct
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
		public void GeneratesConstructorsOfBaseType()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.TypeConvention)} = {nameof(DPTypeConvention)}.{nameof(DPTypeConvention.Inherit)})]

partial class Parent
{{
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T>
	{{
		public Test()
		{{
		}}

		protected Test(T value)
		{{
		}}

		internal Test(ref string value)
		{{
		}}
	}}
}}
";
			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	class Test : Test<int>
	{{
		public Test() : base()
		{{
		}}

		protected Test(int value) : base(value)
		{{
		}}

		internal Test(ref string value) : base(ref value)
		{{
		}}
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void SkipsPrivateConstructors()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.TypeConvention)} = {nameof(DPTypeConvention)}.{nameof(DPTypeConvention.Inherit)})]

partial class Parent
{{
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T>
	{{
		private Test()
		{{
		}}

		protected Test(T value)
		{{
		}}
	}}
}}
";
			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	class Test : Test<int>
	{{
		protected Test(int value) : base(value)
		{{
		}}
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void SkipsConstructorsWithSameParameters()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.TypeConvention)} = {nameof(DPTypeConvention)}.{nameof(DPTypeConvention.Inherit)})]

partial class Parent
{{
	class Test<[{nameof(DefaultParamAttribute)}(typeof(int))]T>
	{{
		protected Test(int value)
		{{
		}}

		protected Test(T value)
		{{
		}}
	}}
}}
";
			string expected =
@$"partial class Parent
{{
	{GetCodeGenerationAttributes("Parent.Test<T>")}
	class Test : Test<int>
	{{
		protected Test(int value) : base(value)
		{{
		}}
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}
	}
}
