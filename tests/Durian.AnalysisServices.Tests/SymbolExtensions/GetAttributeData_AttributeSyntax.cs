// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Durian.Analysis.Extensions;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Durian.Analysis.Tests.SymbolExtensions
{
	public sealed class GetAttributeData_AttributeSyntax : CompilationTest
	{
		public GetAttributeData_AttributeSyntax() : base(Utilities.TestAttribute)
		{
		}

		[Fact]
		public void ReturnsAtributeData_When_SyntaxIsDefinedOnTheSymbol()
		{
			AttributeSyntax attr = GetNode<AttributeSyntax>("[Test]class Test { }");
			SemanticModel semanticModel = Compilation.CurrentCompilation.GetSemanticModel(attr.SyntaxTree);
			INamedTypeSymbol type = (semanticModel.GetDeclaredSymbol(attr.Parent!.Parent!) as INamedTypeSymbol)!;
			Assert.True(type.GetAttribute(attr)?.ApplicationSyntaxReference?.GetSyntax().IsEquivalentTo(attr) ?? false);
		}

		[Fact]
		public void ReturnsNull_When_SyntaxIsNotDefinedOnTheSymbol()
		{
			AttributeSyntax attr = GetNode<AttributeSyntax>("[Test]class Test { ]");
			INamedTypeSymbol type = GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>("class Parent { }");
			Assert.True(type.GetAttribute(attr) is null);
		}

		[Fact]
		public void ThrowsArgumentNullException_When_SymbolIsNull()
		{
			AttributeSyntax attr = GetNode<AttributeSyntax>("[Test]class Test { }");
			Assert.Throws<ArgumentNullException>(() => ((INamedTypeSymbol)null!).GetAttribute(attr));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_SyntaxIsNull()
		{
			INamedTypeSymbol type = GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>("[Test]class Test { }");
			Assert.Throws<ArgumentNullException>(() => type.GetAttribute(syntax: null!));
		}
	}
}
