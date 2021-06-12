// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Durian.Generator.Data;
using Durian.Generator.Extensions;
using Durian.Tests.Fixtures;
using Microsoft.CodeAnalysis;
using Moq;
using Xunit;

namespace Durian.Tests.AnalysisServices.SymbolExtensions
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
			Assert.False(_compilation.CurrentCompilation.GetSpecialType(type).IsPredefinedOrDynamic(_compilation));
		}

		[Fact]
		public void False_When_IsNotSpecialType()
		{
			INamedTypeSymbol symbol = Mock.Of<INamedTypeSymbol>();
			Assert.False(symbol.IsPredefinedOrDynamic(_compilation));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_CompilationDataIsNull()
		{
			INamedTypeSymbol symbol = Mock.Of<INamedTypeSymbol>();
			Assert.Throws<ArgumentNullException>(() => symbol.IsPredefinedOrDynamic((ICompilationData)null!));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_SymbolIsNull()
		{
			INamedTypeSymbol symbol = null!;
			Assert.Throws<ArgumentNullException>(() => symbol.IsPredefinedOrDynamic(_compilation));
		}

		[Theory]
		[ClassData(typeof(Utilities.PredefinedTypeCollection))]
		public void True(SpecialType type)
		{
			Assert.True(_compilation.CurrentCompilation.GetSpecialType(type).IsPredefinedOrDynamic(_compilation));
		}

		[Fact]
		public void True_When_IsDynamic()
		{
			ITypeSymbol symbol = _compilation.CurrentCompilation.DynamicType;
			Assert.True(symbol.IsPredefinedOrDynamic(_compilation));
		}
	}
}
