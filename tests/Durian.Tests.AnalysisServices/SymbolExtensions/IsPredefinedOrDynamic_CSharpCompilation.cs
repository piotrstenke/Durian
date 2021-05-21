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
	public sealed class IsPredefinedOrDynamic_CSharpCompilation : IClassFixture<CompilationFixture>
	{
		private readonly CSharpCompilation _compilation;

		public IsPredefinedOrDynamic_CSharpCompilation(CompilationFixture fixture)
		{
			_compilation = fixture.Compilation;
		}

		[Fact]
		public void ThrowsArgumentNullException_When_SymbolIsNull()
		{
			INamedTypeSymbol symbol = null!;
			Assert.Throws<ArgumentNullException>(() => symbol.IsPredefinedOrDynamic(_compilation));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_CompilationIsNull()
		{
			INamedTypeSymbol symbol = Mock.Of<INamedTypeSymbol>();
			Assert.Throws<ArgumentNullException>(() => symbol.IsPredefinedOrDynamic((CSharpCompilation)null!));
		}

		[Fact]
		public void False_When_IsNotSpecialType()
		{
			INamedTypeSymbol symbol = Mock.Of<INamedTypeSymbol>();
			Assert.False(symbol.IsPredefinedOrDynamic(_compilation));
		}

		[Fact]
		public void True_When_IsDynamic()
		{
			ITypeSymbol symbol = _compilation.DynamicType;
			Assert.True(symbol.IsPredefinedOrDynamic(_compilation));
		}

		[Theory]
		[ClassData(typeof(Utilities.PredefinedTypeCollection))]
		public void True(SpecialType type)
		{
			Assert.True(_compilation.GetSpecialType(type).IsPredefinedOrDynamic(_compilation));
		}

		[Theory]
		[ClassData(typeof(Utilities.NonPredefinedTypeCollection))]
		public void False(SpecialType type)
		{
			Assert.False(_compilation.GetSpecialType(type).IsPredefinedOrDynamic(_compilation));
		}
	}
}
