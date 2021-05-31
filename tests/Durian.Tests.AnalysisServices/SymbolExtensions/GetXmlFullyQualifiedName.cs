using System;
using Durian.Generator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Durian.Tests.AnalysisServices.SymbolExtensions
{
	public sealed class GetXmlFullyQualifiedName : CompilationTest
	{
		[Fact]
		public void ThrowsArgumentNullException_When_MemberIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => ((ISymbol)null!).GetXmlFullyQualifiedName());
		}

		[Fact]
		public void ReturnsMemberName_When_HasNoParentTypes_And_IsInGlobalNamespace()
		{
			Assert.True(GetSymbol("class Test { }").GetXmlFullyQualifiedName() == "Test");
		}

		[Fact]
		public void ReturnsMemberNameAndParentTypes_When_IsInGlobalNamespace()
		{
			Assert.True(GetSymbol("class Parent { class Child { } }", 1).GetXmlFullyQualifiedName() == "Parent.Child");
		}

		[Fact]
		public void ReturnsMemberNameAndNamespace_When_MemberHasNoParentTypes()
		{
			Assert.True(GetSymbol("namespace Test { class Parent { } }").GetXmlFullyQualifiedName() == "Test.Parent");
		}

		[Fact]
		public void ReturnsMemberNameAndNamespaceAndParentTypes_When_IsGeneric()
		{
			Assert.True(GetSymbol("namespace Test { class Parent { class Child { } } }", 1).GetXmlFullyQualifiedName() == "Test.Parent.Child");
		}

		[Fact]
		public void ReturnsGenericName_When_IsGeneric_And_HasNoParentTypes_And_IsInGlobalNamespace()
		{
			Assert.True(GetSymbol("class Test<T> { }").GetXmlFullyQualifiedName() == "Test{T}");
		}

		[Fact]
		public void ReturnsGenericNameAndParentTypes_When_IsGeneric_And_IsInGlobalNamespace()
		{
			Assert.True(GetSymbol("class Parent { class Test<T> { } }", 1).GetXmlFullyQualifiedName() == "Parent.Test{T}");
		}

		[Fact]
		public void ReturnsGenericMemberNameAndNamespace_When_IsGeneric_And_HasNoParentTypes()
		{
			Assert.True(GetSymbol("namespace Test { class Parent<T> { } }").GetXmlFullyQualifiedName() == "Test.Parent{T}");
		}

		[Fact]
		public void ReturnsGenericNameAndNamespaceAndParentTypes_When_IsGeneric()
		{
			Assert.True(GetSymbol("namespace Test { class Parent { class Child<T> { } } }", 1).GetXmlFullyQualifiedName() == "Test.Parent.Child{T}");
		}

		[Fact]
		public void ReturnsMemberNameAndParentTypes_When_ParentIsGeneric_And_IsInGlobalNamespace()
		{
			Assert.True(GetSymbol("class Parent<T> { class Test { } }", 1).GetXmlFullyQualifiedName() == "Parent{T}.Test");
		}

		[Fact]
		public void ReturnsMemberNameAndNamespaceAndParentTypes_When_ParentIsGeneric()
		{
			Assert.True(GetSymbol("namespace Test { class Parent<T> { class Child { } } }", 1).GetXmlFullyQualifiedName() == "Test.Parent{T}.Child");
		}

		[Fact]
		public void ReturnsGenericNameAndParentTypes_When_IsGeneric_And_ParentIsGeneric_And_IsInGlobalNamespace()
		{
			Assert.True(GetSymbol("class Parent<T> { class Test<U> { } }", 1).GetXmlFullyQualifiedName() == "Parent{T}.Test{U}");
		}

		[Fact]
		public void ReturnsGenericNameAndNamespaceAndParentTypes_When_IsGeneric_And_ParentIsGeneric()
		{
			Assert.True(GetSymbol("namespace Test { class Parent<T> { class Child<U> { } } }", 1).GetXmlFullyQualifiedName() == "Test.Parent{T}.Child{U}");
		}

		[Fact]
		public void ReturnsGenericName_When_HasMultipleTypeParameters()
		{
			Assert.True(GetSymbol("class Test<T, U> { }").GetXmlFullyQualifiedName() == "Test{T, U}");
		}

		[Fact]
		public void ReturnsMemberNameAndParentTypes_When_IsInGlobalNamespace_And_HasMultipleParentTypes()
		{
			Assert.True(GetSymbol("class Parent { class Outer { class Inner { } } }", 2).GetXmlFullyQualifiedName() == "Parent.Outer.Inner");
		}

		[Fact]
		public void ReturnsMemberNameAndNamespaceAndParentTypes_When_HasMultipleParentTypes()
		{
			Assert.True(GetSymbol("namespace Test { class Parent { class Outer { class Inner { } } } }", 2).GetXmlFullyQualifiedName() == "Test.Parent.Outer.Inner");
		}

		private INamedTypeSymbol GetSymbol(string source, int index = 0)
		{
			return GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>(source, index);
		}
	}
}