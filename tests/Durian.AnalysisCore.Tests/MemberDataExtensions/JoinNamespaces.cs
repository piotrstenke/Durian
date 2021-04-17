using System;
using Durian.Extensions;
using Xunit;

namespace Durian.Tests.AnalysisCore.MemberDataExtensions
{
	public sealed class JoinNamespaces : CompilationTest
	{
		[Fact]
		public void ThrowsArgumentNullException_When_MemberIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => GetClass(null).JoinNamespaces());
		}

		[Fact]
		public void ReturnsEmptyString_When_IsInGlobalNamespace()
		{
			Assert.True(GetClass("class Type { }").JoinNamespaces() == string.Empty);
		}

		[Fact]
		public void ReturnsParentNamespace_When_HasOneParentNamespace()
		{
			Assert.True(GetClass("namespace Test { class Type { } }").JoinNamespaces() == "Test");
		}

		[Fact]
		public void ReturnsParentNamespaces_When_IsInNestedNamespace()
		{
			Assert.True(GetClass("namespace N1.N2 { class Type { } }").JoinNamespaces() == "N1.N2");
		}

		[Fact]
		public void ReturnsParentNamespaces_When_IsInNestedNamespaces_AndNamespacesAreWrittenSeparately()
		{
			Assert.True(GetClass("namespace N1 { namespace N2 { class Type { } } }").JoinNamespaces() == "N1.N2");
		}
	}
}