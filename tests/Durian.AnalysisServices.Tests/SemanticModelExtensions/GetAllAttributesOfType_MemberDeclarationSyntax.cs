using System;
using System.Linq;
using Durian.Generator.Data;
using Durian.Generator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Durian.Tests.AnalysisCore.SemanticModelExtensions
{
	public sealed class GetAllAttributesOfType_MemberDeclarationSyntax : CompilationTest
	{
		private INamedTypeSymbol AttributeSymbol => Compilation.CurrentCompilation.GetTypeByMetadataName("TestAttribute")!;

		public GetAllAttributesOfType_MemberDeclarationSyntax() : base(Utilities.TestAttribute, Utilities.OtherAttribute)
		{

		}

		[Fact]
		public void ThrowsArgumentNullException_When_SemanticModelIsNull()
		{
			IMemberData member = GetClass("class Test { }");
			SemanticModel? semanticModel = null;
			Assert.Throws<ArgumentNullException>(() => semanticModel!.GetAllAttributesOfType(member.Declaration, AttributeSymbol));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_SyntaxNodelIsNull()
		{
			IMemberData member = GetClass("class Test { }");
			Assert.Throws<ArgumentNullException>(() => member.SemanticModel.GetAllAttributesOfType((MemberDeclarationSyntax)null!, AttributeSymbol));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_AttributeSymbolIsNull()
		{
			IMemberData member = GetClass("class Test { }");
			Assert.Throws<ArgumentNullException>(() => member.SemanticModel.GetAllAttributesOfType(member.Declaration, null!));
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
		public void ReturnsOneAttribute_WhenHasOneAttribute()
		{
			AttributeSyntax[] attrs = Execute("[Test]class Test { }");
			Assert.True(attrs.Length == 1 && attrs[0].ToString() == "Test");
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
		public void ReturnsAttributesFromLeftToRight()
		{
			AttributeSyntax[] attrs = Execute("[Test(\"name\")][Test]class Test { }");
			Assert.True(
				attrs.Length == 2 &&
				attrs[0].ToString() == "Test(\"name\")" &&
				attrs[1].ToString() == "Test"
			);
		}

		private AttributeSyntax[] Execute(string src)
		{
			IMemberData member = GetClass(src);
			return member.SemanticModel.GetAllAttributesOfType(member.Declaration, AttributeSymbol).ToArray();
		}
	}
}