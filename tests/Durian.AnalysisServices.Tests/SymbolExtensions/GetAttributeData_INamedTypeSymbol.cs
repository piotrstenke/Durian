// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Extensions;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using Xunit;

namespace Durian.Analysis.Tests.SymbolExtensions
{
	public sealed class GetAttributeData_INamedTypeSymbol : CompilationTest
	{
		public GetAttributeData_INamedTypeSymbol() : base(Utilities.TestAttribute)
		{
		}

		[Fact]
		public void ReturnsAttributeData_When_HasTargetAttribute()
		{
			INamedTypeSymbol type = GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>("[Test]class Test { }")!;
			INamedTypeSymbol attr = GetAttr();
			AttributeData? data = type.GetAttribute(attr);

			Assert.True(data is not null && SymbolEqualityComparer.Default.Equals(data.AttributeClass, attr));
		}

		[Fact]
		public void ReturnsAttributeData_When_HasTargetAttributeAndOtherAttribute()
		{
			INamedTypeSymbol type = GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>("[Other][Test]class Test { }")!;
			INamedTypeSymbol attr = GetAttr();
			AttributeData? data = type.GetAttribute(attr);

			Assert.True(data is not null && SymbolEqualityComparer.Default.Equals(data.AttributeClass, attr));
		}

		[Fact]
		public void ReturnsNull_When_HasNoTargetAttribute()
		{
			INamedTypeSymbol type = GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>("[Other]class Test { }")!;
			INamedTypeSymbol attr = GetAttr();
			AttributeData? data = type.GetAttribute(attr);

			Assert.True(data is null);
		}

		[Fact]
		public void ThrowsArgumentNullException_When_SymbolIsNull()
		{
			INamedTypeSymbol attr = GetAttr();
			Assert.Throws<ArgumentNullException>(() => ((INamedTypeSymbol)null!).GetAttribute(attr));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_SyntaxIsNull()
		{
			INamedTypeSymbol type = GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>("[Test]class Test { }")!;
			Assert.Throws<ArgumentNullException>(() => type.GetAttribute(syntax: null!));
		}

		private INamedTypeSymbol GetAttr()
		{
			return Compilation.CurrentCompilation.GetTypeByMetadataName("TestAttribute")!;
		}
	}
}
