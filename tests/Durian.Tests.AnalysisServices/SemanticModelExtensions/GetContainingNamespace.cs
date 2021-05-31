using System;
using Durian.Generator.Data;
using Durian.Generator.Extensions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Durian.Tests.AnalysisServices.SemanticModelExtensions
{
	public sealed class GetContainingNamespace : CompilationTest
	{
		[Fact]
		public void ThrowsArgumentNullException_When_SemanticModelIsNull()
		{
			IMemberData member = GetClass("class Test { }")!;
			SemanticModel? semanticModel = null;
			Assert.Throws<ArgumentNullException>(() => semanticModel!.GetContainingNamespace(member.Declaration, Compilation));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_SyntaxNodeIsNull()
		{
			IMemberData member = GetClass("class Test { }");
			Assert.Throws<ArgumentNullException>(() => member.SemanticModel.GetContainingNamespace(null!, Compilation));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_CompilationDataIsNull()
		{
			IMemberData member = GetClass("class Test { }");
			Assert.Throws<ArgumentNullException>(() => member.SemanticModel.GetContainingNamespace(member.Declaration, compilationData: null!));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_CompilationIsNull()
		{
			IMemberData member = GetClass("class Test { }");
			Assert.Throws<ArgumentNullException>(() => member.SemanticModel.GetContainingNamespace(member.Declaration, compilation: null!));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_AssemblySymbolIsNull()
		{
			IMemberData member = GetClass("class Test { }");
			Assert.Throws<ArgumentNullException>(() => member.SemanticModel.GetContainingNamespace(member.Declaration, assembly: null!));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_GlobalNamespaceIsNull()
		{
			IMemberData member = GetClass("class Test { }");
			Assert.Throws<ArgumentNullException>(() => member.SemanticModel.GetContainingNamespace(member.Declaration, globalNamespace: null!));
		}

		[Fact]
		public void ThrowsArgumentException_When_GlobalNamespaceIsNotActuallyGlobal()
		{
			IMemberData member = GetClass("namespace N { class Test { } }");
			Assert.Throws<ArgumentException>(() => member.SemanticModel.GetContainingNamespace(member.Declaration, member.Symbol.ContainingNamespace));
		}

		[Fact]
		public void ReturnsGlobalNamespace_When_IsInGlobalNamespace()
		{
			Assert.True(SymbolEqualityComparer.Default.Equals(
				Execute("class Test { }"),
				Compilation.CurrentCompilation.Assembly.GlobalNamespace
			));
		}

		[Fact]
		public void ReturnsParentNamespace_When_HasParentNamespace()
		{
			Assert.True(Execute("namespace N { class Test { } }").Name == "N");
		}

		[Fact]
		public void ReturnsParentNamespace_When_IsInNestedNamespace()
		{
			Assert.True(Execute("namespace N1.N2 { class Test { } }").Name == "N2");
		}

		[Fact]
		public void ReturnsParentNamespace_When_IsInsideType()
		{
			Assert.True(Execute("namespace N1.N2 { class Parent { class Child { } } }", 1).Name == "N2");
		}

		private INamespaceSymbol Execute(string src, int index = 0)
		{
			IMemberData member = GetClass(src, index);
			return member.SemanticModel.GetContainingNamespace(member.Declaration, Compilation);
		}
	}
}