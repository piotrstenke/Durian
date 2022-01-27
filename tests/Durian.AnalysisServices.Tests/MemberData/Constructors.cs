// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Immutable;
using System.Linq;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Durian.TestServices;
using Durian.TestServices.Fixtures;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Moq;
using Xunit;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Durian.Analysis.Tests.MemberData
{
	public sealed class Constructors : IClassFixture<CompilationFixture>
	{
		private readonly CSharpCompilation _compilation;

		public Constructors(CompilationFixture fixture)
		{
			_compilation = fixture.Compilation;
		}

		[Fact]
		public void ConstructorThrowsArgumentException_When_DeclarationDoesNotRepresentAnySymbol()
		{
			CompilationUnitSyntax unit = CompilationUnit();
			ClassDeclarationSyntax decl = ClassDeclaration("Test");
			FieldDeclarationSyntax field = FieldDeclaration(VariableDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword)), SingletonSeparatedList(VariableDeclarator("field"))));

			unit = unit.AddMembers(decl.AddMembers(field));

			Assert.Throws<ArgumentException>(() => new Data.MemberData(decl, CreateValidCompilationData(unit.SyntaxTree)));
		}

		[Fact]
		public void ConstructorThrowsArgumentNullException_When_CompilationIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => new Data.MemberData(declaration: ClassDeclaration("Test"), null!));
		}

		[Fact]
		public void ConstructorThrowsArgumentNullException_When_DeclarationIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => new Data.MemberData(declaration: null!, Mock.Of<ICompilationData>()));
		}

		[Fact]
		public void DeclarationReturnsDeclarationPassedAsArgument()
		{
			MemberDeclarationSyntax member = CreateValidDeclaration();
			Data.MemberData data = new(member, CreateValidCompilationData(member.SyntaxTree));

			Assert.True(data.Declaration is not null && data.Declaration.IsEquivalentTo(member));
		}

		[Fact]
		public void InternalFourParameterConstructorSetsAllData()
		{
			MemberDeclarationSyntax decl = CreateValidDeclaration();
			ICompilationData compilation = CreateValidCompilationData(decl.SyntaxTree);
			SemanticModel semanticModel = compilation.Compilation.GetSemanticModel(decl.SyntaxTree, true);
			ISymbol symbol = semanticModel.GetDeclaredSymbol(decl)!;
			Location location = decl.GetLocation();
			ITypeData[] containingTypes = symbol.GetContainingTypes(compilation).ToArray();
			INamespaceSymbol[] containingNamespaces = symbol.GetContainingNamespaces().ToArray();
			ImmutableArray<AttributeData> attributes = symbol.GetAttributes();
			Data.MemberData data = new(decl, compilation, symbol, semanticModel, containingTypes, containingNamespaces, attributes);
			Assert.True(
				data.SemanticModel is not null &&
				data.SemanticModel.SyntaxTree.IsEquivalentTo(semanticModel.SyntaxTree) &&
				data.Symbol is not null &&
				SymbolEqualityComparer.Default.Equals(data.Symbol, symbol) &&
				data.Location is not null &&
				data.Location == location &&
				data.Declaration is not null &&
				data.Declaration.IsEquivalentTo(decl) &&
				data.ParentCompilation is not null &&
				data.ParentCompilation == compilation &&
				data._containingTypes is not null &
				data._containingTypes == containingTypes &&
				!data._attributes.IsDefault &&
				data._attributes == attributes &&
				data._containingNamespaces is not null &&
				data._containingNamespaces == containingNamespaces
			);
		}

		[Fact]
		public void InternalTwoParameterConstructorSetsAllData()
		{
			MemberDeclarationSyntax decl = CreateValidDeclaration();
			ICompilationData compilation = CreateValidCompilationData(decl.SyntaxTree);
			SemanticModel semanticModel = compilation.Compilation.GetSemanticModel(decl.SyntaxTree, true);
			ISymbol symbol = semanticModel.GetDeclaredSymbol(decl)!;
			Location location = decl.GetLocation();
			Data.MemberData data = new(symbol, compilation);
			Assert.True(
				data.SemanticModel is not null &&
				data.SemanticModel.SyntaxTree.IsEquivalentTo(semanticModel.SyntaxTree) &&
				data.Symbol is not null &&
				SymbolEqualityComparer.Default.Equals(data.Symbol, symbol) &&
				data.Location is not null &&
				data.Location == location &&
				data.Declaration is not null &&
				data.Declaration.IsEquivalentTo(decl) &&
				data.ParentCompilation is not null &&
				data.ParentCompilation == compilation
			);
		}

		[Fact]
		public void LocationReturnsActualNodeLocation()
		{
			MemberDeclarationSyntax member = CreateValidDeclaration();
			Location location = member.GetLocation();
			Data.MemberData data = new(member, CreateValidCompilationData(member.SyntaxTree));
			Location dataLocation = data.Location;

			Assert.True(dataLocation is not null && dataLocation == location);
		}

		[Fact]
		public void NameReturnsSymbolName()
		{
			MemberDeclarationSyntax member = CreateValidDeclaration();
			ICompilationData compilation = CreateValidCompilationData(member.SyntaxTree);
			Data.MemberData data = new(member, compilation);

			Assert.True(data.Symbol is not null && data.Name == data.Symbol.Name);
		}

		[Fact]
		public void ParentCompilationIsProperlySetAfterConstructor()
		{
			MemberDeclarationSyntax member = CreateValidDeclaration();
			ICompilationData compilation = CreateValidCompilationData(member.SyntaxTree);
			Data.MemberData data = new(member, compilation);

			Assert.True(data.ParentCompilation is not null && data.ParentCompilation == compilation);
		}

		[Fact]
		public void SemanticModelReturnsValidSemanticModel()
		{
			MemberDeclarationSyntax member = CreateValidDeclaration();
			ICompilationData compilation = CreateValidCompilationData(member.SyntaxTree);
			SemanticModel semanticModel = compilation.Compilation.GetSemanticModel(member.SyntaxTree, true);
			Data.MemberData data = new(member, compilation);

			Assert.True(data.SemanticModel is not null && data.SemanticModel.SyntaxTree.IsEquivalentTo(semanticModel.SyntaxTree));
		}

		[Fact]
		public void SymbolReturnsValidSymbol()
		{
			MemberDeclarationSyntax member = CreateValidDeclaration();
			ICompilationData compilation = CreateValidCompilationData(member.SyntaxTree);
			SemanticModel semanticModel = compilation.Compilation.GetSemanticModel(member.SyntaxTree, true);
			ISymbol symbol = semanticModel.GetDeclaredSymbol(member)!;
			Data.MemberData data = new(member, compilation);

			Assert.True(data.Symbol is not null && SymbolEqualityComparer.Default.Equals(symbol, data.Symbol));
		}

		private static MemberDeclarationSyntax CreateValidDeclaration()
		{
			CSharpSyntaxTree tree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText("class Test { }");
			MemberDeclarationSyntax decl = RoslynUtilities.ParseNode<ClassDeclarationSyntax>(tree)!;

			return decl;
		}

		private ICompilationData CreateValidCompilationData(SyntaxTree tree)
		{
			CSharpCompilation compilation = _compilation.AddSyntaxTrees(tree);
			Mock<ICompilationData> mock = new();
			mock.Setup(c => c.Compilation).Returns(compilation);

			return mock.Object;
		}
	}
}