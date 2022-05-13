// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Durian.TestServices.Fixtures;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Moq;
using Xunit;

namespace Durian.Analysis.Tests.SymbolExtensions
{
	public sealed class IsPredefined : IClassFixture<CompilationFixture>
	{
		private readonly CSharpCompilation _compilation;

		public IsPredefined(CompilationFixture fixture)
		{
			_compilation = fixture.Compilation;
		}

		[Theory]
		[ClassData(typeof(Utilities.NonPredefinedTypeCollection))]
		public void False(SpecialType type)
		{
			Assert.False(_compilation.GetSpecialType(type).IsKeyword());
		}

		[Fact]
		public void False_When_IsDynamic()
		{
			ITypeSymbol symbol = _compilation.DynamicType;
			Assert.False(symbol.IsKeyword());
		}

		[Fact]
		public void False_When_IsNotSpecialType()
		{
			INamedTypeSymbol symbol = Mock.Of<INamedTypeSymbol>();
			Assert.False(symbol.IsKeyword());
		}

		[Theory]
		[ClassData(typeof(Utilities.PredefinedTypeCollection))]
		public void True(SpecialType type)
		{
			Assert.True(_compilation.GetSpecialType(type).IsKeyword());
		}
	}
}
