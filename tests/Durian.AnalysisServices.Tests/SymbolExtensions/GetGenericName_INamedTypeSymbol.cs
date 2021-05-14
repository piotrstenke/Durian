using System;
using Durian.Generator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Durian.Tests.AnalysisCore.SymbolExtensions
{
	public sealed class GetGenericName_INamedTypeSymbol : CompilationTest
	{
		[Fact]
		public void ThrowsArgumentNullException_When_MethodIsNull()
		{
			INamedTypeSymbol symbol = null!;
			Assert.Throws<ArgumentNullException>(() => symbol.GetGenericName(true));
		}

		[Fact]
		public void ReturnsOnlyName_When_HasNoTypeParameters()
		{
			INamedTypeSymbol symbol = GetSymbolForParameter("class Test { }");
			Assert.True(symbol.GetGenericName(true) == "Test");
		}

		[Fact]
		public void CanReturnNameWithSingleTypeParameter()
		{
			INamedTypeSymbol symbol = GetSymbolForParameter("class Test<T> { }");
			Assert.True(symbol.GetGenericName(true) == "Test<T>");
		}

		[Fact]
		public void CanReturnNameWithSingleTypeArgument()
		{
			INamedTypeSymbol symbol = GetSymbolForArgument("class Test<T> { } class New : Test<int> { }");
			Assert.True(symbol.GetGenericName(false) == "Test<int>");
		}

		[Fact]
		public void CanReturnNameWithMultipleTypeParameters()
		{
			INamedTypeSymbol symbol = GetSymbolForParameter("class Test<T, U> { }");
			Assert.True(symbol.GetGenericName(true) == "Test<T, U>");
		}

		[Fact]
		public void CanReturnNameWithMultipleTypeArguments()
		{
			INamedTypeSymbol symbol = GetSymbolForArgument("class Test<T, U> { } class New : Test<int, string> { }");
			Assert.True(symbol.GetGenericName(false) == "Test<int, string>");
		}

		private INamedTypeSymbol GetSymbolForParameter(string source)
		{
			return GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>(source);
		}

		private INamedTypeSymbol GetSymbolForArgument(string source)
		{
			BaseTypeSyntax b = GetNode<BaseTypeSyntax>(source);
			SemanticModel semanticModel = Compilation.CurrentCompilation.GetSemanticModel(b.SyntaxTree);
			TypeInfo info = semanticModel.GetTypeInfo(b.Type);
			return (info.Type as INamedTypeSymbol)!;
		}
	}
}