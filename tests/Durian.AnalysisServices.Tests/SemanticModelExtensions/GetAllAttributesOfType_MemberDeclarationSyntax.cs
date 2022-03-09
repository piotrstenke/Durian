// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using Xunit;

namespace Durian.Analysis.Tests.SemanticModelExtensions
{
	public sealed class GetAllAttributesOfType_MemberDeclarationSyntax : CompilationTest
	{
		private INamedTypeSymbol AttributeSymbol => Compilation.CurrentCompilation.GetTypeByMetadataName("TestAttribute")!;

		public GetAllAttributesOfType_MemberDeclarationSyntax() : base(Utilities.TestAttribute, Utilities.OtherAttribute)
		{
		}

		[Fact]
		public void ReturnsAttributesFromLeftToRight()
		{
			AttributeSyntax[] attrs = Execute("[Test(\"name\")][Test]class Test { }");
			Assert.True(
				attrs.Length == 2 &&
				attrs[0].ToString() == "Test(\"name\")" &&
				attrs[1].ToString() == "Test"
			);
		}

		[Fact]
		public void ReturnsEmpty_When_HasNoAttributes()
		{
			Assert.True(Execute("class Test { }").Length == 0);
		}

		[Fact]
		public void ReturnsEmpty_When_HasOtherAttribute()
		{
			Assert.True(Execute("[Other]class Test { }").Length == 0);
		}

		[Fact]
		public void ReturnsMultipleAttributes_WhenHasMultipleAttributesOfTheSameType()
		{
			AttributeSyntax[] attrs = Execute("[Test(\"name\")][Test]class Test { }");
			Assert.True(
				attrs.Length == 2 &&
				attrs.Any(n => n.ToString() == "Test") &&
				attrs.Any(n => n.ToString() == "Test(\"name\")")
			);
		}

		[Fact]
		public void ReturnsOneAttribute_WhenHasOneAttribute()
		{
			AttributeSyntax[] attrs = Execute("[Test]class Test { }");
			Assert.True(attrs.Length == 1 && attrs[0].ToString() == "Test");
		}

		[Fact]
		public void SkipAttributesOfOtherTypes()
		{
			AttributeSyntax[] attrs = Execute("[Test(\"name\")][Other][Test]class Test { }");
			Assert.True(
				attrs.Length == 2 &&
				attrs.Any(n => n.ToString() == "Test") &&
				attrs.Any(n => n.ToString() == "Test(\"name\")")
			);
		}

		[Fact]
		public void ThrowsArgumentNullException_When_AttributeSymbolIsNull()
		{
			IMemberData member = GetClass("class Test { }")!;
			Assert.Throws<ArgumentNullException>(() => member.SemanticModel.GetAttributes(member.Declaration, null!));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_SemanticModelIsNull()
		{
			IMemberData member = GetClass("class Test { }")!;
			SemanticModel? semanticModel = null;
			Assert.Throws<ArgumentNullException>(() => semanticModel!.GetAttributes(member.Declaration, AttributeSymbol));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_SyntaxNodelIsNull()
		{
			IMemberData member = GetClass("class Test { }")!;
			Assert.Throws<ArgumentNullException>(() => member.SemanticModel.GetAttributes((MemberDeclarationSyntax)null!, AttributeSymbol));
		}

		private AttributeSyntax[] Execute(string src)
		{
			IMemberData member = GetClass(src)!;
			return member.SemanticModel.GetAttributes(member.Declaration, AttributeSymbol).ToArray();
		}
	}
}