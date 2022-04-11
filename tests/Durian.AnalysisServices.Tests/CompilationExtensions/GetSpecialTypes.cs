// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using Durian.Analysis.Extensions;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Durian.Analysis.Tests.CompilationExtensions
{
	public sealed class GetSpecialTypes : IClassFixture<GetSpecialTypes.Fixture>
	{
		public sealed class Fixture
		{
			public CSharpCompilation Compilation { get; }

			public HashSet<INamedTypeSymbol> Set { get; }

			public Fixture()
			{
				TestableCompilationData compilation = TestableCompilationData.Create();
				Compilation = compilation.CurrentCompilation;
#pragma warning disable RS1024 // Compare symbols correctly
				Set = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
#pragma warning restore RS1024 // Compare symbols correctly

				foreach (INamedTypeSymbol types in compilation.CurrentCompilation.GetSpecialTypes())
				{
					Set.Add(types);
				}
			}
		}

		private sealed class SpecialTypeCollection : IEnumerable<object[]>
		{
			public IEnumerator<object[]> GetEnumerator()
			{
				foreach (SpecialType type in Utilities.GetSpecialTypes())
				{
					yield return new object[] { type };
				}
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		private const int _numSpecialTypes = 45;

		private readonly Fixture _fixture;

		public GetSpecialTypes(Fixture fixture)
		{
			_fixture = fixture;
		}

		[Theory]
		[ClassData(typeof(SpecialTypeCollection))]
		public void ContainsSpecialType(SpecialType type)
		{
			Assert.Contains(_fixture.Compilation.GetSpecialType(type), _fixture.Set);
		}

		[Fact]
		public void ReturnsSameAmountOfTypesAsThereAreSpecialTypes()
		{
			Assert.True(_fixture.Set.Count == _numSpecialTypes);
		}

		[Fact]
		public void ThrowsNull_When_CompilationIsNull()
		{
			CSharpCompilation compilation = null!;
			Assert.Throws<ArgumentNullException>(() => compilation.GetSpecialTypes());
		}
	}
}
