using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Durian.Tests.AnalysisServices.TypeData
{
	public sealed class GetPartialDeclarations : CompilationTest
	{
		[Fact]
		public void ReturnsOneDeclaration_When_IsNotPartial()
		{
			Generator.Data.TypeData data = GetType("class Test { }");
			TypeDeclarationSyntax[] decl = data.GetPartialDeclarations().ToArray();

			Assert.True(decl.Length == 1 && decl[0].IsEquivalentTo(data.Declaration));
		}

		[Fact]
		public void ReturnsOneDeclaration_When_PartialIsRedundant()
		{
			Generator.Data.TypeData data = GetType("partial class Test { }");
			TypeDeclarationSyntax[] decl = data.GetPartialDeclarations().ToArray();

			Assert.True(decl.Length == 1 && decl[0].IsEquivalentTo(data.Declaration));
		}

		[Fact]
		public void CanReturnsMultipleDeclarations()
		{
			TypeDeclarationSyntax firstDecl = GetNode<TypeDeclarationSyntax>("public partial sealed class Test { }");
			Generator.Data.TypeData data = GetType("partial class Test { }");
			TypeDeclarationSyntax[] decl = data.GetPartialDeclarations().ToArray();

			Assert.True(decl.Length == 2 && decl.Any(d => d.IsEquivalentTo(firstDecl)) && decl.Any(d => d.IsEquivalentTo(data.Declaration)));
		}
	}
}