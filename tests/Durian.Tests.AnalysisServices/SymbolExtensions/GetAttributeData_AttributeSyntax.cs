using System;
using Durian.Generator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Durian.Tests.AnalysisServices.SymbolExtensions
{
	public sealed class GetAttributeData_AttributeSyntax : CompilationTest
	{
		public GetAttributeData_AttributeSyntax() : base(Utilities.TestAttribute)
		{

		}

		[Fact]
		public void ThrowsArgumentNullException_When_SymbolIsNull()
		{
			AttributeSyntax attr = GetNode<AttributeSyntax>("[Test]class Test { }");
			Assert.Throws<ArgumentNullException>(() => ((INamedTypeSymbol)null!).GetAttributeData(attr));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_SyntaxIsNull()
		{
			INamedTypeSymbol type = GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>("[Test]class Test { }");
			Assert.Throws<ArgumentNullException>(() => type.GetAttributeData(syntax: null!));
		}

		[Fact]
		public void ReturnsAtributeData_When_SyntaxIsDefinedOnTheSymbol()
		{
			AttributeSyntax attr = GetNode<AttributeSyntax>("[Test]class Test { }");
			SemanticModel semanticModel = Compilation.CurrentCompilation.GetSemanticModel(attr.SyntaxTree);
			INamedTypeSymbol type = (semanticModel.GetDeclaredSymbol(attr.Parent!.Parent!) as INamedTypeSymbol)!;
			Assert.True(type.GetAttributeData(attr)?.ApplicationSyntaxReference?.GetSyntax().IsEquivalentTo(attr) ?? false);
		}

		[Fact]
		public void ReturnsNull_When_SyntaxIsNotDefinedOnTheSymbol()
		{
			AttributeSyntax attr = GetNode<AttributeSyntax>("[Test]class Test { ]");
			INamedTypeSymbol type = GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>("class Parent { }");
			Assert.True(type.GetAttributeData(attr) is null);
		}
	}
}
