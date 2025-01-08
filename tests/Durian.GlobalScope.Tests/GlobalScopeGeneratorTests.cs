using Durian.TestServices;
using Xunit;
using static Durian.Analysis.GlobalScope.GlobalScopeDiagnostics;

namespace Durian.Analysis.GlobalScope.Tests;

public class GlobalScopeGeneratorTests : DurianGeneratorTest<GlobalScopeGenerator>
{
	[Fact]
	public void Error_When_TypeIsNotStatic()
	{
		string input =
$$"""
using {{DurianStrings.MainNamespace}};

[{{GlobalScopeAttributeProvider.TypeName}})]
public class Test
{
}

""";
		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0501_TypeIsNotStaticClass));
	}

	[Fact]
	public void Error_When_TypeIsNestedWithoutStatic()
	{
		string input =
$$"""
using {{DurianStrings.MainNamespace}};

public class Test
{
	[{{GlobalScopeAttributeProvider.TypeName}})]
	public class Nested
	{
	}
}

""";
		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0501_TypeIsNotStaticClass, DUR0502_TypeIsNotTopLevel));
	}

	[Fact]
	public void Error_When_TypeIsNestedAndStatic()
	{
		string input =
$$"""
using {{DurianStrings.MainNamespace}};

public class Test
{
	[{{GlobalScopeAttributeProvider.TypeName}})]
	public static class Nested
	{
	}
}

""";
		Assert.True(RunGenerator(input).FailedAndContainsDiagnostics(DUR0502_TypeIsNotTopLevel));
	}

	[Fact]
	public void Success_When_TypeIsInGlobalNamespace()
	{
		string input =
$$"""
using {{DurianStrings.MainNamespace}};

[{{GlobalScopeAttributeProvider.TypeName}})]
public static class Test
{
	public static void Method()
	{
	}
}

""";

		string expected =
"""
global using static Test;
""";



		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_TypeIsInTopLevelNamespace()
	{
		string input =
$$"""
using {{DurianStrings.MainNamespace}};

namespace Util;

[{{GlobalScopeAttributeProvider.TypeName}})]
public static class Test
{
	public static void Method()
	{
	}
}

""";

		string expected =
"""
global using static Util.Test;
""";



		Assert.True(RunGenerator(input).Compare(expected));
	}

	[Fact]
	public void Success_When_TypeIsInNestedNamespace()
	{
		string input =
$$"""
using {{DurianStrings.MainNamespace}};

namespace Util.Common;

[{{GlobalScopeAttributeProvider.TypeName}})]
public static class Test
{
	public static void Method()
	{
	}
}

""";

		string expected =
"""
global using static Util.Common.Test;
""";



		Assert.True(RunGenerator(input).Compare(expected));
	}
}
