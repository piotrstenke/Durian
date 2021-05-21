using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Durian.Tests.AnalysisServices.MemberData
{
	public sealed class GetContainingNamespaces : CompilationTest
	{
		[Fact]
		public void ReturnsEmpty_When_IsNotInNamespace()
		{
			Generator.Data.MemberData data = GetMember("class Test { }");
			INamespaceSymbol[] containingNamespaces = data.GetContainingNamespaces().ToArray();
			Assert.True(containingNamespaces.Length == 0);
		}

		[Fact]
		public void CanReturnSingleNamespace()
		{
			Generator.Data.MemberData data = GetMember("namespace N1 { class Parent { } }", 1);
			INamespaceSymbol[] containingNamespaces = data.GetContainingNamespaces().ToArray();
			Assert.True(containingNamespaces.Length == 1 && containingNamespaces[0].Name == "N1");
		}

		[Fact]
		public void CanReturnMultipleNamespaces()
		{
			Generator.Data.MemberData data = GetMember("namespace N1.N2 { class Parent { } }", 1);
			INamespaceSymbol[] containingNamespaces = data.GetContainingNamespaces().ToArray();
			Assert.True(containingNamespaces.Length == 2 && containingNamespaces.Any(t => t.Name == "N1") && containingNamespaces.Any(t => t.Name == "N2"));
		}

		[Fact]
		public void ReturnsNamespacesInParentFirstOrder()
		{
			Generator.Data.MemberData data = GetMember("namespace N1.N2 { class Parent { } }", 1);
			INamespaceSymbol[] containingNamespaces = data.GetContainingNamespaces().ToArray();
			Assert.True(containingNamespaces.Length == 2 && containingNamespaces[0].Name == "N1" && containingNamespaces[1].Name == "N2");
		}
	}
}
