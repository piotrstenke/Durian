// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.TestServices;
using Xunit;

namespace Durian.Analysis.DefaultParam.Tests.Types
{
	public sealed class TypeConventionTests : DefaultParamGeneratorTest
	{
		[Fact]
		public void Copies_When_GloballyTrue_And_LocallyFalse()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Inherit})]
partial class Parent
{{
	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Copy})]
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>
	{{
		T value = default;
	}}
}}
";
			string expected =
@$"partial class Parent
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
		public void Copies_When_InTypeTrue_And_LocallyFalse()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Inherit})]
partial class Parent
{{
	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Copy})]
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>
	{{
		T value = default;
	}}
}}
";
			string expected =
@$"partial class Parent
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
		public void GeneratesConstructorsOfBaseType()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Inherit})]
partial class Parent
{{
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>
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
		public void Inherits_When_AppliedGlobally()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Inherit})]
partial class Parent
{{
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>
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
	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Inherit})]
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>
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
		public void Inherits_When_GloballyFalse_And_InTypeTrue()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Copy})]
[{DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Inherit})]
partial class Parent
{{
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>
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
		public void Inherits_When_GlobalyFalse_And_LocallyTrue()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Copy})]
partial class Parent
{{
	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Inherit})]
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>
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

[{DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Copy})]
partial class Parent
{{
	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Inherit})]
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>
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
		public void PreservesAttributes_When_IsInherit()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};
using System;

partial class Parent
{{
	[Serializable]
	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Inherit})]
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>
	{{
		T value = default;
	}}
}}
";
			string expected =
@$"using System;

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
		public void ProperlyHandlesConstraints_When_IsInherit()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial class Parent
{{
	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Inherit})]
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T, [{DefaultParamAttributeProvider.TypeName}(typeof(string))]U> where T : struct where U : class
	{{
		T value = default;
	}}
}}
";
			string expected =
@$"partial class Parent
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
		public void ProperlyWritesAllTypeArguments()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Inherit}]
partial class Parent
{{
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T, [{DefaultParamAttributeProvider.TypeName}(typeof(int))]U, [{DefaultParamAttributeProvider.TypeName}(typeof(string))]V>
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
		public void RemovesBaseList_When_IsInherit()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};
using System;

partial class Parent
{{
	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Inherit})]
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T> : Attribute, System.Collections.ICollection
	{{
		T value = default;
	}}
}}
";
			string expected =
@$"using System;

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
	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Inherit})]
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T, [{DefaultParamAttributeProvider.TypeName}(typeof(string))]U> : Attribute, System.Collections.ICollection where T : struct where U : class
	{{
		T value = default;
	}}
}}
";
			string expected =
@$"using System;

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
		public void SkipsConstructorsWithSameParameters()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Inherit})]
partial class Parent
{{
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>
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

		[Fact]
		public void SkipsPrivateConstructors()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Inherit})]
partial class Parent
{{
	class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(int))]T>
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
		public void Warning_And_GeneratesAsCopy_When_IsScopedInherit_And_HasNoAccessibleConstructors()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Inherit})]
class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
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
			Assert.True(result.SucceededAndContainsDiagnostics(DefaultParamDiagnostics.DUR0118_ApplyCopyTypeConventionOnStructOrSealedTypeOrTypeWithNoPublicCtor.Id));
			Assert.True(result.Compare(expected));
		}

		[Fact]
		public void Warning_And_GeneratesAsCopy_When_IsScopedInherit_And_IsSealedType()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Inherit})]
sealed class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
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
			Assert.True(result.SucceededAndContainsDiagnostics(DefaultParamDiagnostics.DUR0118_ApplyCopyTypeConventionOnStructOrSealedTypeOrTypeWithNoPublicCtor.Id));
			Assert.True(result.Compare(expected));
		}

		[Fact]
		public void Warning_And_GeneratesAsCopy_When_IsScopedInherit_And_IsStatic()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Inherit})]
static class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
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
			Assert.True(result.SucceededAndContainsDiagnostics(DefaultParamDiagnostics.DUR0118_ApplyCopyTypeConventionOnStructOrSealedTypeOrTypeWithNoPublicCtor.Id));
			Assert.True(result.Compare(expected));
		}

		[Fact]
		public void Warning_And_GeneratesAsCopy_When_IsScopedInherit_And_IsStruct()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Inherit})]
struct Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
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
			Assert.True(result.SucceededAndContainsDiagnostics(DefaultParamDiagnostics.DUR0118_ApplyCopyTypeConventionOnStructOrSealedTypeOrTypeWithNoPublicCtor.Id));
			Assert.True(result.Compare(expected));
		}
	}
}
