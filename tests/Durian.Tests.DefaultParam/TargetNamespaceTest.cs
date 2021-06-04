using Durian.Configuration;
using Durian.Generator;
using Xunit;

namespace Durian.Tests.DefaultParam
{
	public sealed class TargetNamespaceTest : DefaultParamGeneratorTest
	{
		[Fact]
		public void UsesParent_When_IsNull()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

namespace Parent
{{
	[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.TargetNamespace)} = null)]
	public class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
	{{
	}}
}}
";
			string expected =
$@"using {DurianStrings.ConfigurationNamespace};

namespace Parent
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
		public void UsesParentByDefault()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

namespace Parent
{{
	public class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
	{{
	}}
}}
";
			string expected =
$@"namespace Parent
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
		public void UsesParent_When_IsWhitespaceOrEmpty()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

namespace Parent
{{
	[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.TargetNamespace)} = ""  "")]
	public class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
	{{
	}}
}}
";
			string expected =
$@"using {DurianStrings.ConfigurationNamespace};

namespace Parent
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
		public void UsesParent_When_IsInvalidNamespaceIdentifier()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

namespace Parent
{{
	[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.TargetNamespace)} = ""2352"")]
	public class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
	{{
	}}
}}
";
			string expected =
$@"using {DurianStrings.ConfigurationNamespace};

namespace Parent
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
		public void UsesParent_When_IsDurianGenerator()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

namespace Parent
{{
	[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.TargetNamespace)} = ""Durian.Generator"")]
	public class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
	{{
	}}
}}
";
			string expected =
$@"using {DurianStrings.ConfigurationNamespace};

namespace Parent
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
		public void UsesParent_When_IsNamedLikeKeyword()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

namespace Parent
{{
	[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.TargetNamespace)} = ""int"")]
	public class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
	{{
	}}
}}
";
			string expected =
$@"using {DurianStrings.ConfigurationNamespace};

namespace Parent
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
		public void CanUseGlobalNamespace()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

namespace Parent
{{
	[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.TargetNamespace)} = ""global"")]
	public class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
	{{
	}}
}}
";
			string expected =
$@"using {DurianStrings.ConfigurationNamespace};
using Parent;

{GetCodeGenerationAttributes("Parent.Test<T>")}
public class Test : Test<string>
{{
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void Success_When_IsNamedLikeKeyword_And_HasAtSign()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

namespace Parent
{{
	[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.TargetNamespace)} = ""@int"")]
	public class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
	{{
	}}
}}
";
			string expected =
$@"using {DurianStrings.ConfigurationNamespace};
using Parent;

namespace @int
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
		public void Success_When_ValueIsValid()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

namespace Parent
{{
	[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.TargetNamespace)} = ""Durian"")]
	public class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
	{{
	}}
}}
";
			string expected =
$@"using {DurianStrings.ConfigurationNamespace};
using Parent;

namespace Durian
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
		public void Success_When_ValueIsValid_AndHasDot()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

namespace Parent
{{
	[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.TargetNamespace)} = ""Durian.Configuration"")]
	public class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
	{{
	}}
}}
";
			string expected =
$@"using {DurianStrings.ConfigurationNamespace};
using Parent;

namespace Durian.Configuration
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
		public void UsesGlobalValue()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.TargetNamespace)} = ""Durian"")]

namespace Parent
{{
	public class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
	{{
	}}
}}
";
			string expected =
$@"using Parent;

namespace Durian
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
		public void UsesScopedValue()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

namespace Parent
{{
	[{nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.TargetNamespace)} = ""Durian"")]
	public partial class P
	{{	
		public class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
		{{
		}}
	}}
}}
";
			string expected =
$@"using Parent;

namespace Durian
{{
	public partial class P
	{{
		{GetCodeGenerationAttributes("Parent.P.Test<T>")}
		public class Test : Test<string>
		{{
		}}
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void ScopedValueHasHigherPriority()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.TargetNamespace)} = ""Durian"")]

namespace Parent
{{
	[{nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.TargetNamespace)} = ""Other"")]
	public partial class P
	{{	
		public class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
		{{
		}}
	}}
}}
";
			string expected =
$@"using Parent;

namespace Other
{{
	public partial class P
	{{
		{GetCodeGenerationAttributes("Parent.P.Test<T>")}
		public class Test : Test<string>
		{{
		}}
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void NestedScopedValueHasHigherPriority()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.TargetNamespace)} = ""Durian"")]

namespace Parent
{{
	[{nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.TargetNamespace)} = ""Other"")]
	public partial class P
	{{	
		[{nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.TargetNamespace)} = ""Testable"")]
		public partial class A
		{{
			public class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
			{{
			}}
		}}
	}}
}}
";
			string expected =
$@"using Parent;

namespace Testable
{{
	public partial class P
	{{
		public partial class A
		{{
			{GetCodeGenerationAttributes("Parent.P.A.Test<T>")}
			public class Test : Test<string>
			{{
			}}
		}}
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}

		[Fact]
		public void LocalValueHasHighestPriority()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[assembly: {nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.TargetNamespace)} = ""Durian"")]

namespace Parent
{{
	[{nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.TargetNamespace)} = ""Other"")]
	public partial class P
	{{	
		[{nameof(DefaultParamScopedConfigurationAttribute)}({nameof(DefaultParamScopedConfigurationAttribute.TargetNamespace)} = ""Testable"")]
		public partial class A
		{{
			[{nameof(DefaultParamConfigurationAttribute)}({nameof(DefaultParamConfigurationAttribute.TargetNamespace)} = ""Local"")]
			public class Test<[{nameof(DefaultParamAttribute)}(typeof(string))]T>
			{{
			}}
		}}
	}}
}}
";
			string expected =
$@"using {DurianStrings.ConfigurationNamespace};
using Parent;

namespace Local
{{
	public partial class P
	{{
		public partial class A
		{{
			{GetCodeGenerationAttributes("Parent.P.A.Test<T>")}
			public class Test : Test<string>
			{{
			}}
		}}
	}}
}}
";
			Assert.True(RunGenerator(input).Compare(expected));
		}
	}
}
