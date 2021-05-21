using System;
using Durian.Generator.Data;
using Durian.Generator.Extensions;
using Durian.Tests.Fixtures;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Moq;
using Xunit;

namespace Durian.Tests.AnalysisServices.SymbolExtensions
{
	public sealed class IsPredefined : IClassFixture<CompilationFixture>
	{
		private readonly CSharpCompilation _compilation;

		public IsPredefined(CompilationFixture fixture)
		{
			_compilation = fixture.Compilation;
		}

		[Fact]
		public void ThrowsArgumentNullException_When_SymbolIsNull()
		{
			INamedTypeSymbol symbol = null!;
			Assert.Throws<ArgumentNullException>(() => symbol.IsPredefined());
		}

		[Fact]
		public void False_When_IsNotSpecialType()
		{
			INamedTypeSymbol symbol = Mock.Of<INamedTypeSymbol>();
			Assert.False(symbol.IsPredefined());
		}

		[Fact]
		public void False_When_IsDynamic()
		{
			ITypeSymbol symbol = _compilation.DynamicType;
			Assert.False(symbol.IsPredefined());
		}

		[Theory]
		[ClassData(typeof(Utilities.PredefinedTypeCollection))]
		public void True(SpecialType type)
		{
			Assert.True(_compilation.GetSpecialType(type).IsPredefined());
		}

		[Theory]
		[ClassData(typeof(Utilities.NonPredefinedTypeCollection))]
		public void False(SpecialType type)
		{
			Assert.False(_compilation.GetSpecialType(type).IsPredefined());
		}
	}
}
