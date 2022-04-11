// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Xunit;
using static Durian.Analysis.DefaultParam.DefaultParamDiagnostics;

namespace Durian.Analysis.DefaultParam.Tests
{
	public sealed class ConfigurationAnalyzerTests : AnalyzerTest<DefaultParamConfigurationAnalyzer>
	{
		[Fact]
		public async Task Error_When_ConfigurationIsOnLocalFunction()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial class Test
{{
	public static void Method()
	{{
		[{DefaultParamConfigurationAttributeProvider.TypeName}()]
		static void Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>()
		{{
		}}
	}}
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzer(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0115_DefaultParamConfigurationIsNotValidOnThisTypeOfMethod.Id));
		}

		[Fact]
		public async Task Error_When_IsInheritOnSealedType()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Inherit})]
public sealed class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>()
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzer(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0117_InheritTypeConventionCannotBeUsedOnStructOrSealedType.Id));
		}

		[Fact]
		public async Task Error_When_IsInheritOnStaticType()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Inherit})]
public static class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzer(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0117_InheritTypeConventionCannotBeUsedOnStructOrSealedType.Id));
		}

		[Fact]
		public async Task Error_When_IsInheritOnStruct()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Inherit})]
public struct Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>()
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzer(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0117_InheritTypeConventionCannotBeUsedOnStructOrSealedType.Id));
		}

		[Fact]
		public async Task Error_When_IsInheritOnTypeWithNoAccessibleConstructors()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Inherit})]
public class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
{{
	private Test()
	{{
	}}
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzer(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0123_InheritTypeConventionCannotBeUsedOnTypeWithNoAccessibleConstructor.Id));
		}

		[Fact]
		public async Task Error_When_MethodIsExplicitImplementation()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

interface ITest
{{
	void Method<[DefaultParam(typeof(string))]T>();
}}

class Test : ITest
{{
	[{DefaultParamConfigurationAttributeProvider.TypeName}]
	void ITest.Method<[DefaultParam(typeof(string))]T>()
	{{
	}}
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzer(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0115_DefaultParamConfigurationIsNotValidOnThisTypeOfMethod.Id));
		}

		[Fact]
		public async Task Error_When_MethodIsInInterface()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial interface ITest
{{
	[{DefaultParamConfigurationAttributeProvider.TypeName}()]
	void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>();
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzer(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0115_DefaultParamConfigurationIsNotValidOnThisTypeOfMethod.Id));
		}

		[Fact]
		public async Task NoDiagnostics_When_IsDefaultParamDelegate()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial class Test
{{
	[{DefaultParamConfigurationAttributeProvider.TypeName}()]
	public delegate void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>();
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task NoDiagnostics_When_IsDefaultParamMethod()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial class Test
{{
	[{DefaultParamConfigurationAttributeProvider.TypeName}()]
	public static void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>()
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task NoDiagnostics_When_IsDefaultParamType()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamConfigurationAttributeProvider.TypeName}()]
partial class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
{{
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task NoDiagnostics_When_MethodConventionOnMethod()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial class Test
{{
	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.MethodConvention} = {DPMethodConventionProvider.TypeName}.{DPMethodConventionProvider.Call})]
	public static void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>()
	{{
	}}
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task NoDiagnostics_When_TypeConventionOnType()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Inherit})]
partial class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
{{
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task NoWarning_When_TargetNamespaceIsNamedLikeKeyword_And_HasAtSign()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TargetNamespace} = ""@int"")]
public class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
{{
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task NoWarning_When_TargetNamespaceIsNull()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TargetNamespace} = null)]
public class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
{{
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task NoWarning_When_TargetNamespaceIsValid()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TargetNamespace} = ""Durian"")]
public class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
{{
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task NoWarning_When_TargetNamespaceIsValid_And_HasDot()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TargetNamespace} = ""Durian.Core"")]
public class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
{{
}}
";
			Assert.Empty(await RunAnalyzer(input));
		}

		[Fact]
		public async Task Warning_When_ConfigurationIsOnNonDefaultParamDelegate()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial class Test
{{
	[{DefaultParamConfigurationAttributeProvider.TypeName}()]
	public delegate void Method<T>();
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzer(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0111_DefaultParamConfigurationAttributeCannotBeAppliedToMembersWithoutDefaultParamAttribute.Id));
		}

		[Fact]
		public async Task Warning_When_ConfigurationIsOnNonDefaultParamMethod()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial class Test
{{
	[{DefaultParamConfigurationAttributeProvider.TypeName}()]
	public static void Method<T>()
	{{
	}}
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzer(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0111_DefaultParamConfigurationAttributeCannotBeAppliedToMembersWithoutDefaultParamAttribute.Id));
		}

		[Fact]
		public async Task Warning_When_ConfigurationIsOnNonDefaultParamType()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamConfigurationAttributeProvider.TypeName}()]
partial class Test<T>
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzer(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0111_DefaultParamConfigurationAttributeCannotBeAppliedToMembersWithoutDefaultParamAttribute.Id));
		}

		[Fact]
		public async Task Warning_When_IsNotChildType_And_UsesApplyNewModifierProperty()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.ApplyNewModifierWhenPossible} = true)]
public class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzer(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0124_ApplyNewModifierShouldNotBeUsedWhenIsNotChildOfType.Id));
		}

		[Fact]
		public async Task Warning_When_MethodConventionOnDelegate()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial class Test
{{
	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.MethodConvention} = {DPMethodConventionProvider.TypeName}.{DPMethodConventionProvider.Call})]
	public delegate void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>();
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzer(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0113_MethodConventionShouldNotBeUsedOnMembersOtherThanMethods.Id));
		}

		[Fact]
		public async Task Warning_When_MethodConventionOnType()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.MethodConvention} = {DPMethodConventionProvider.TypeName}.{DPMethodConventionProvider.Call})]
partial class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzer(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0113_MethodConventionShouldNotBeUsedOnMembersOtherThanMethods.Id));
		}

		[Fact]
		public async Task Warning_When_TargetNamespaceIsDefinedOnNestedMember()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

public class Parent
{{
	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TargetNamespace} = ""Durian"")]
	public class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
	{{
	}}
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzer(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0128_DoNotSpecifyTargetNamespaceForNestedMembers.Id));
		}

		[Fact]
		public async Task Warning_When_TargetNamespaceIsDurianGenerator()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TargetNamespace} = ""Durian.Generator"")]
public class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzer(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0127_InvalidTargetNamespace.Id));
		}

		[Fact]
		public async Task Warning_When_TargetNamespaceIsNamedLikeKeyword()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TargetNamespace} = ""int"")]
public class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzer(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0127_InvalidTargetNamespace.Id));
		}

		[Fact]
		public async Task Warning_When_TargetNamespaceIsNotValidIdentifier()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TargetNamespace} = ""12fwa"")]
public class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzer(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0127_InvalidTargetNamespace.Id));
		}

		[Fact]
		public async Task Warning_When_TargetNamespaceIsWhitespaceOrEmpty()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TargetNamespace} = "" "")]
public class Test<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>
{{
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzer(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0127_InvalidTargetNamespace.Id));
		}

		[Fact]
		public async Task Warning_When_TypeConventionOnDelegate()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial class Test
{{
	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Inherit})]
	public delegate void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>();
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzer(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0112_TypeConvetionShouldNotBeUsedOnMembersOtherThanTypes.Id));
		}

		[Fact]
		public async Task Warning_When_TypeConventionOnMethod()
		{
			string input =
@$"using {DurianStrings.MainNamespace};
using {DurianStrings.ConfigurationNamespace};

partial class Test
{{
	[{DefaultParamConfigurationAttributeProvider.TypeName}({DefaultParamConfigurationAttributeProvider.TypeConvention} = {DPTypeConventionProvider.TypeName}.{DPTypeConventionProvider.Inherit})]
	public static void Method<[{DefaultParamAttributeProvider.TypeName}(typeof(string))]T>()
	{{
	}}
}}
";
			ImmutableArray<Diagnostic> diagnostics = await RunAnalyzer(input);
			Assert.True(diagnostics.Any(d => d.Id == DUR0112_TypeConvetionShouldNotBeUsedOnMembersOtherThanTypes.Id));
		}

		protected override IEnumerable<ISourceTextProvider>? GetInitialSources()
		{
			return DefaultParamGenerator.GetSourceProviders();
		}
	}
}
