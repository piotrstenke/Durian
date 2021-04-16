using Durian.DefaultParam;
using Xunit;

namespace Durian.Tests.DefaultParam
{
	public sealed class StructuralTests : DefaultParamTestBase
	{
		[Fact]
		public void WritesSortedUsings()
		{
			const string input =
@"using Durian;
using System.Collections;
using System.Numerics;

partial class Test
{
	void Method<[DefaultParam(typeof(System.Int32))]T>()
	{
		System.Collections.Generic.List<BigInteger> list = new();
	}
}
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
			const string input =
@"namespace N1
{
	namespace N2
	{
		public partial interface Parent
		{
			public partial struct Child
			{
				public partial class Child
				{
					public void Method<[Durian.DefaultParam((typeof(int)))]T>()
					{

					}
				}
			}
		}
	}
}
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