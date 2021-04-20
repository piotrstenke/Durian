using Durian.DefaultParam;
using Xunit;

namespace Durian.Tests.DefaultParam.Generator
{
	public sealed class BasicMethodTests : DefaultParamGeneratorTest
	{
		[Fact]
		public void SkipsMethod_When_HasNoDefaultParamAttribute()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	void Method<T>(T value)
	{{
	}}
}}";

			Assert.False(RunGenerator(input).IsGenerated);
		}

		[Fact]
		public void HandlesMethodWithOneTypeParameter()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T>(T value)
	{{
	}}
}}
";

			string expected =
@$"using System.CodeDom.Compiler;

partial class Test
{{
	[GeneratedCode(""{DefaultParamGenerator.GeneratorName}"", ""{DefaultParamGenerator.Version}"")]
	void Method(int value)
	{{

	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void HandlesMethodWithMultipleTypeParameters_When_AllParametersAreDefaultParam()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T, [{DefaultParamAttribute.AttributeName}(typeof(string))]U>(T value)
	{{
		U name = default;
	}}
}}
";

			string expected =
@$"using System.CodeDom.Compiler;

partial class Test
{{
	[GeneratedCode(""{DefaultParamGenerator.GeneratorName}"", ""{DefaultParamGenerator.Version}"")]
	void Method<T>(T value)
	{{
		string name = default;
	}}

	[GeneratedCode(""{DefaultParamGenerator.GeneratorName}"", ""{DefaultParamGenerator.Version}"")]
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

partial class Test
{{
	void Method<T, [{DefaultParamAttribute.AttributeName}(typeof(string))]U>(T value)
	{{
		U name = default;
	}}
}}
";

			string expected =
@$"using System.CodeDom.Compiler;

partial class Test
{{
	[GeneratedCode(""{DefaultParamGenerator.GeneratorName}"", ""{DefaultParamGenerator.Version}"")]
	void Method<T>(T value)
	{{
		string name = default;
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
using {DurianStrings.MainNamespace};
using System.Collections;
using System.Collections.Generic;

partial class Test
{{
	void Method<T, [{DefaultParamAttribute.AttributeName}(typeof(System.Collections.IEnumerable))]U>(U value) where T : IEnumerable<U>
	{{
		U v = default(U);
		object obj = (U)2;
		Type type = typeof(U);
		List<U> list = new List<U>();
	}}
}}
";

			string expected =
@$"using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;

partial class Test
{{
	[GeneratedCode(""{DefaultParamGenerator.GeneratorName}"", ""{DefaultParamGenerator.Version}"")]
	void Method<T>(IEnumerable value) where T : IEnumerable<IEnumerable>
	{{
		IEnumerable v = default(IEnumerable);
		object obj = (IEnumerable)2;
		Type type = typeof(IEnumerable);
		List<IEnumerable> list = new List<IEnumerable>();
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
	void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T>() where T : unmanaged
	{{
	}}
}}
";

			string expected =
@$"using System.CodeDom.Compiler;

partial class Test
{{
	[GeneratedCode(""{DefaultParamGenerator.GeneratorName}"", ""{DefaultParamGenerator.Version}"")]
	void Method()
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

partial class Test
{{
	void Method<[{DefaultParamAttribute.AttributeName}(typeof(int))]T, [{DefaultParamAttribute.AttributeName}(typeof(string))]U, [{DefaultParamAttribute.AttributeName}(typeof(float))]V>() where T : unmanaged where U : class where V : notnull
	{{
	}}
}}
";

			string expected =
@$"using System.CodeDom.Compiler;

partial class Test
{{
	[GeneratedCode(""{DefaultParamGenerator.GeneratorName}"", ""{DefaultParamGenerator.Version}"")]
	void Method<T, U>() where T : unmanaged where U : class
	{{
	}}

	[GeneratedCode(""{DefaultParamGenerator.GeneratorName}"", ""{DefaultParamGenerator.Version}"")]
	void Method<T>() where T : unmanaged
	{{
	}}

	[GeneratedCode(""{DefaultParamGenerator.GeneratorName}"", ""{DefaultParamGenerator.Version}"")]
	void Method()
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

partial class Test
{{
	void Method<T, [{DefaultParamAttribute.AttributeName}(typeof(string))]U, [{DefaultParamAttribute.AttributeName}(typeof(float))]V>() where T : unmanaged where U : class where V : notnull
	{{
	}}
}}
";
			string expected =
@$"using System.CodeDom.Compiler;

partial class Test
{{
	[GeneratedCode(""{DefaultParamGenerator.GeneratorName}"", ""{DefaultParamGenerator.Version}"")]
	void Method<T, U>() where T : unmanaged where U : class
	{{
	}}

	[GeneratedCode(""{DefaultParamGenerator.GeneratorName}"", ""{DefaultParamGenerator.Version}"")]
	void Method<T>() where T : unmanaged
	{{
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void PreservesAccessibilityAndStaticModifiers()
		{
			string input =
@$"using {DurianStrings.MainNamespace};

partial class Test
{{
	public static void Method<[{DefaultParamAttribute.AttributeName}(typeof(string)]T>(T value)
	{{
	}}
}}
";

			string expected =
@$"using System.CodeDom.Compiler;

partial class Test
{{
	[GeneratedCode(""{DefaultParamGenerator.GeneratorName}"", ""{DefaultParamGenerator.Version}"")]
	public static void Method(string value)
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
using System.Collections;
using System.Numerics;

partial class Test
{{
	void Method<[{DefaultParamAttribute.AttributeName}(typeof(System.Int32))]T>()
	{{
		System.Collections.Generic.List<BigInteger> list = new();
	}}
}}
";

			string expected =
@$"using System.CodeDom.Compiler;
using System.Numerics;

partial class Test
{{
	[GeneratedCode(""{DefaultParamGenerator.GeneratorName}"", ""{DefaultParamGenerator.Version}"")]
	void Method()
	{{
		System.Collections.Generic.List<BigInteger> list = new();
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
				public partial class Child
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
@$"using System.CodeDom.Compiler;

namespace N1.N2
{{
	public partial interface Parent
	{{
		public partial struct Child
		{{
			public partial class Child
			{{
				[GeneratedCode(""{DefaultParamGenerator.GeneratorName}"", ""{DefaultParamGenerator.Version}"")]
				public void Method()
				{{
				}}
			}}
		}}
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}
	}
}
