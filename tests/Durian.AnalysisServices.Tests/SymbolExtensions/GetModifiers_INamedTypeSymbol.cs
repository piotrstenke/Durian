// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;
using Durian.Analysis.Extensions;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Durian.Analysis.Tests.SymbolExtensions
{
	public sealed class GetModifiers_INamedTypeSymbol : CompilationTest
	{
		[Fact]
		public void CanReturnMultipleModifiers()
		{
			INamedTypeSymbol symbol = GetSymbol<INamedTypeSymbol, StructDeclarationSyntax>("class Parent { protected internal unsafe readonly ref partial struct Test { }}");
			SyntaxToken[] tokens = symbol.GetModifiers().ToArray();

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
			INamedTypeSymbol symbol = GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>("internal class Test { }");
			SyntaxToken[] tokens = symbol.GetModifiers().ToArray();
			Assert.True(tokens.Length == 1 && tokens[0].IsKind(SyntaxKind.InternalKeyword));
		}

		[Fact]
		public void DoesNotReturnIdenticalModifiers()
		{
			INamedTypeSymbol symbol = GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>("public sealed partial class Test { } public sealed partial class Test { }");
			SyntaxToken[] tokens = symbol.GetModifiers().ToArray();

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
			INamedTypeSymbol symbol = GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>("public partial class Test { } partial sealed class Test { } ", 1);
			SyntaxToken[] tokens = symbol.GetModifiers().ToArray();

			Assert.True(
				tokens.Any(t => t.IsKind(SyntaxKind.PublicKeyword)) &&
				tokens.Any(t => t.IsKind(SyntaxKind.SealedKeyword)) &&
				tokens.Any(t => t.IsKind(SyntaxKind.PartialKeyword))
			);
		}

		[Fact]
		public void ReturnsEmpty_When_TypeHasNoModifiers()
		{
			INamedTypeSymbol symbol = GetSymbol<INamedTypeSymbol, ClassDeclarationSyntax>("class Test { }");
			SyntaxToken[] tokens = symbol.GetModifiers().ToArray();
			Assert.True(tokens.Length == 0);
		}

		[Fact]
		public void ThrowsArgumentNullException_When_TypeIsNull()
		{
			INamedTypeSymbol symbol = null!;
			Assert.Throws<ArgumentNullException>(() => symbol.GetModifiers());
		}
	}
}
