// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Durian.TestServices;
using Durian.TestServices.Fixtures;
using Microsoft.CodeAnalysis;
using Moq;
using Xunit;

namespace Durian.Analysis.Tests.SymbolExtensions
{
	public sealed class IsPredefinedOrDynamic_ICompilationData : IClassFixture<CompilationDataFixture>
	{
		private readonly TestableCompilationData _compilation;

		public IsPredefinedOrDynamic_ICompilationData(CompilationDataFixture fixture)
		{
			_compilation = fixture.Compilation;
		}

		[Theory]
		[ClassData(typeof(Utilities.NonPredefinedTypeCollection))]
		public void False(SpecialType type)
		{
			Assert.False(_compilation.CurrentCompilation.GetSpecialType(type).IsPredefinedOrDynamic());
		}

		[Fact]
		public void False_When_IsNotSpecialType()
		{
			INamedTypeSymbol symbol = Mock.Of<INamedTypeSymbol>();
			Assert.False(symbol.IsPredefinedOrDynamic());
		}

		[Theory]
		[ClassData(typeof(Utilities.PredefinedTypeCollection))]
		public void True(SpecialType type)
		{
			Assert.True(_compilation.CurrentCompilation.GetSpecialType(type).IsPredefinedOrDynamic());
		}

		[Fact]
		public void True_When_IsDynamic()
		{
			ITypeSymbol symbol = _compilation.CurrentCompilation.DynamicType;
			Assert.True(symbol.IsPredefinedOrDynamic());
		}
	}
}
