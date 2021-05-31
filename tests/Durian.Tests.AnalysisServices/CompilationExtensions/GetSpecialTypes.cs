using System;
using System.Collections;
using System.Collections.Generic;
using Durian.Generator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Durian.Tests.AnalysisServices.CompilationExtensions
{
	public sealed class GetSpecialTypes : IClassFixture<GetSpecialTypes.Fixture>
	{
		private const int _numSpecialTypes = 45;
		private readonly Fixture _fixture;

		public GetSpecialTypes(Fixture fixture)
		{
			_fixture = fixture;
		}

		[Fact]
		public void ThrowsNull_When_CompilationIsNull()
		{
			CSharpCompilation compilation = null!;
			Assert.Throws<ArgumentNullException>(() => compilation.GetSpecialTypes());
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

		public sealed class Fixture
		{
			public HashSet<INamedTypeSymbol> Set { get; }
			public CSharpCompilation Compilation { get; }

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
	}
}