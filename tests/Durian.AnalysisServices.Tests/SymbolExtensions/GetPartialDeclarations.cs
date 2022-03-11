// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Extensions;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using Xunit;

namespace Durian.Analysis.Tests.SymbolExtensions
{
	public sealed class GetPartialDeclarations : CompilationTest
	{
		[Fact]
		public void CanReturnsMultipleDeclarations()
		{
			ClassDeclarationSyntax decl1 = GetNode<ClassDeclarationSyntax>("partial class Test { }")!;
			ClassDeclarationSyntax decl2 = GetNode<ClassDeclarationSyntax>("public partial sealed class Test { }")!;

			INamedTypeSymbol symbol = GetSymbol<INamedTypeSymbol>(decl1)!;
			ClassDeclarationSyntax[] partialDecls = symbol.GetPartialDeclarations<ClassDeclarationSyntax>().ToArray();

			Assert.True(partialDecls.Length == 2 && partialDecls.Any(d => d.IsEquivalentTo(decl1)) && partialDecls.Any(d => d.IsEquivalentTo(decl2)));
		}

		[Fact]
		public void ReturnsOneDeclaration_When_IsNotPartial()
		{
			ClassDeclarationSyntax decl = GetNode<ClassDeclarationSyntax>("class Test { }")!;
			INamedTypeSymbol symbol = GetSymbol<INamedTypeSymbol>(decl)!;
			ClassDeclarationSyntax[] partialDecls = symbol.GetPartialDeclarations<ClassDeclarationSyntax>().ToArray();

			Assert.True(partialDecls.Length == 1 && partialDecls[0].IsEquivalentTo(decl));
		}

		[Fact]
		public void ReturnsOneDeclaration_When_PartialIsRedundant()
		{
			ClassDeclarationSyntax decl = GetNode<ClassDeclarationSyntax>("partial class Test { }")!;
			INamedTypeSymbol symbol = GetSymbol<INamedTypeSymbol>(decl)!;
			ClassDeclarationSyntax[] partialDecls = symbol.GetPartialDeclarations<ClassDeclarationSyntax>().ToArray();

			Assert.True(partialDecls.Length == 1 && partialDecls[0].IsEquivalentTo(decl));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_TypeIsNull()
		{
			INamedTypeSymbol symbol = null!;
			Assert.Throws<ArgumentNullException>(() => symbol.GetPartialDeclarations<TypeDeclarationSyntax>());
		}
	}
}
