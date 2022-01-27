// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Linq;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Durian.Analysis.Tests.TypeData
{
	public sealed class Modifiers : CompilationTest
	{
		[Fact]
		public void CanReturnMultipleModifiers()
		{
			Data.TypeData data = GetType("class Parent { protected internal unsafe readonly ref partial struct Test { }}", 1);
			SyntaxToken[] tokens = data.Modifiers;

			Assert.True(
				tokens.Any(t => t.IsKind(SyntaxKind.ProtectedKeyword)) &&
				tokens.Any(t => t.IsKind(SyntaxKind.InternalKeyword)) &&
				tokens.Any(t => t.IsKind(SyntaxKind.UnsafeKeyword)) &&
				tokens.Any(t => t.IsKind(SyntaxKind.ReadOnlyKeyword)) &&
				tokens.Any(t => t.IsKind(SyntaxKind.RefKeyword)) &&
				tokens.Any(t => t.IsKind(SyntaxKind.PartialKeyword))
			);
		}

		[Fact]
		public void CanReturnSingleModifier()
		{
			Data.TypeData data = GetType("internal class Test { }");
			SyntaxToken[] tokens = data.Modifiers;

			Assert.True(tokens.Length == 1 && tokens[0].IsKind(SyntaxKind.InternalKeyword));
		}

		[Fact]
		public void DoesNotReturnIdenticalModifiers()
		{
			Compilation.UpdateCompilation("public sealed partial class Test { }");
			Data.TypeData data = GetType("public sealed partial class Test { }");
			SyntaxToken[] tokens = data.Modifiers;

			Assert.True(
				tokens.Length == 3 &&
				tokens.Any(t => t.IsKind(SyntaxKind.PublicKeyword)) &&
				tokens.Any(t => t.IsKind(SyntaxKind.SealedKeyword)) &&
				tokens.Any(t => t.IsKind(SyntaxKind.PartialKeyword))
			);
		}

		[Fact]
		public void ReturnsAllModifiers_When_IsPartial()
		{
			Compilation.UpdateCompilation("public partial class Test { }");
			Data.TypeData data = GetType("partial sealed class Test { }");
			SyntaxToken[] tokens = data.Modifiers;

			Assert.True(
				tokens.Any(t => t.IsKind(SyntaxKind.PublicKeyword)) &&
				tokens.Any(t => t.IsKind(SyntaxKind.SealedKeyword)) &&
				tokens.Any(t => t.IsKind(SyntaxKind.PartialKeyword))
			);
		}

		[Fact]
		public void ReturnsEmpty_When_DeclHasNoModifiers()
		{
			Data.TypeData data = GetType("class Test { }");
			SyntaxToken[] tokens = data.Modifiers;
			Assert.True(tokens.Length == 0);
		}
	}
}