// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Xunit;

namespace Durian.Analysis.DefaultParam.Tests
{
	public sealed class TargetNamespaceTests : DefaultParamGeneratorTest
	{
		[Fact]
		public void CanUseGlobalNamespace()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

namespace Parent
{{
	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TargetNamespace} = ""global"")]
	public class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
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
		public void IgnoresTargetNamespace_When_IsNestedMember()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

namespace Parent
{{
	public partial class A
	{{
		[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TargetNamespace} = ""Durian"")]
		public class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
		{{
		}}
	}}
}}
";
			string expected =
$@"using {DurianStrings.ConfigurationNamespace};

namespace Parent
{{
	public partial class A
	{{
		{GetCodeGenerationAttributes("Parent.A.Test<T>")}
		public class Test : Test<string>
		{{
		}}
	}}
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
	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TargetNamespace} = ""@int"")]
	public class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
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
	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TargetNamespace} = ""Durian"")]
	public class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
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
	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TargetNamespace} = ""Durian.Configuration"")]
	public class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
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

[assembly: {DefaultParamScopedConfigurationAttributeProvider.TypeName}({DefaultParamScopedConfigurationAttributeProvider.TargetNamespace} = ""Durian"")]
namespace Parent
{{
	public class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
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
		public void UsesParent_When_IsDurianGenerator()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

namespace Parent
{{
	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TargetNamespace} = ""Durian.Generator"")]
	public class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
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
	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TargetNamespace} = ""2352"")]
	public class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
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
	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TargetNamespace} = ""int"")]
	public class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
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
		public void UsesParent_When_IsNull()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

namespace Parent
{{
	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TargetNamespace} = null)]
	public class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
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
		public void UsesParent_When_IsWhitespaceOrEmpty()
		{
			string input =
$@"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

namespace Parent
{{
	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TargetNamespace} = ""  "")]
	public class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
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
	public class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
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
	}
}
