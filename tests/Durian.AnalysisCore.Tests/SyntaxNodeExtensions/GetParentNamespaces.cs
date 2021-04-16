using System;
using System.Linq;
using Durian.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Durian.Tests.CorePackage.SyntaxNodeExtensions
{
	public sealed class GetParentNamespaces : CompilationTest
	{
		[Fact]
		public void ThrowsArgumentNullException_When_NodeIsNull()
		{
			MemberDeclarationSyntax member = null!;
			Assert.Throws<ArgumentNullException>(() => member.GetParentNamespaces());
		}

		[Fact]
		public void ReturnsEmpty_When_IsInGlobalNamespace()
		{
			MemberDeclarationSyntax member = GetNode<ClassDeclarationSyntax>("class Test { }");
			Assert.True(!member.GetParentNamespaces().Any());
		}

		[Fact]
		public void ReturnsParentNamespace_When_IsInSingleNamespace()
		{
			MemberDeclarationSyntax member = GetNode<ClassDeclarationSyntax>("namespace N { class Test { } }");
			string[] namespaces = member.GetParentNamespaces().ToArray();
			Assert.True(namespaces.Length == 1 && namespaces[0] == "N");
		}

		[Fact]
		public void ReturnsMultipleNamespaces_When_IsInNestedNamespace()
		{
			MemberDeclarationSyntax member = GetNode<ClassDeclarationSyntax>("namespace N1.N2 { class Test { } }");
			string[] namespaces = member.GetParentNamespaces().ToArray();
			Assert.True(namespaces.Length == 2 && namespaces.Contains("N1") && namespaces.Contains("N2"));
		}

		[Fact]
		public void ReturnsMultipleNamespaces_When_IsInNestedNamespace_And_NamespacesAreWrittenSeparately()
		{
			MemberDeclarationSyntax member = GetNode<ClassDeclarationSyntax>("namespace N1 { namespace N2 { class Test { } } }");
			string[] namespaces = member.GetParentNamespaces().ToArray();
			Assert.True(namespaces.Length == 2 && namespaces.Contains("N1") && namespaces.Contains("N2"));
		}

		[Fact]
		public void IsSuccess_When_IsNotDirectlyInNamespaceScope()
		{
			MemberDeclarationSyntax member = GetNode<ClassDeclarationSyntax>("namespace N { class Parent { class Child { } } }", 1);
			string[] namespaces = member.GetParentNamespaces().ToArray();
			Assert.True(namespaces.Length == 1 && namespaces[0] == "N");
		}

		[Fact]
		public void ReturnsNamespacesFromLeftToRight()
		{
			MemberDeclarationSyntax member = GetNode<ClassDeclarationSyntax>("namespace N1 { namespace N2 { class Test { } } }");
			string[] namespaces = member.GetParentNamespaces().ToArray();
			Assert.True(namespaces.Length == 2 && namespaces[0] == "N1" && namespaces[1] == "N2");
		}
	}
}