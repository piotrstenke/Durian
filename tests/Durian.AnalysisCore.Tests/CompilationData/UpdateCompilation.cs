using System;
using System.Linq;
using System.Numerics;
using Durian.Tests.Fixtures;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Durian.Tests.AnalysisCore.CompilationData
{
	public sealed class UpdateCompilation : IClassFixture<CompilationFixture>
	{
		private readonly CSharpCompilation _compilation;

		public UpdateCompilation(CompilationFixture fixture)
		{
			_compilation = fixture.Compilation;
		}

		[Fact]
		public void UpdateCompilationReturns_When_SyntaxTreeIsNull()
		{
			Data.CompilationData data = new(_compilation);
			data.UpdateCompilation(tree: null);
			Assert.True(data.Compilation is not null && data.Compilation == _compilation);
		}

		[Fact]
		public void UpdateCompilationReturns_When_OriginalTreeIsNull()
		{
			Data.CompilationData data = new(_compilation);
			data.UpdateCompilation(original: null, GetExampleSyntaxTree1());
			Assert.True(data.Compilation is not null && data.Compilation == _compilation);
		}

		[Fact]
		public void UpdateCompilationReturns_When_UpdatedTreeIsNull()
		{
			CSharpSyntaxTree tree = GetExampleSyntaxTree1();
			CSharpCompilation compilation = _compilation.AddSyntaxTrees(tree);
			Data.CompilationData data = new(compilation);
			data.UpdateCompilation(tree, null);
			Assert.True(data.Compilation is not null && data.Compilation == compilation);
		}

		[Fact]
		public void UpdateCompilationReturns_When_TreeCollectionIsNull()
		{
			Data.CompilationData data = new(_compilation);
			data.UpdateCompilation(trees: null);
			Assert.True(data.Compilation is not null && data.Compilation == _compilation);
		}

		[Fact]
		public void UpdateCompilationReturns_When_ReferenceIsNull()
		{
			Data.CompilationData data = new(_compilation);
			data.UpdateCompilation(reference: null);
			Assert.True(data.Compilation is not null && data.Compilation == _compilation);
		}

		[Fact]
		public void UpdateCompilationReturns_When_OriginalReferenceIsNull()
		{
			Data.CompilationData data = new(_compilation);
			data.UpdateCompilation(original: null, GetExampleMetadataReference1());
			Assert.True(data.Compilation is not null && data.Compilation == _compilation);
		}

		[Fact]
		public void UpdateCompilationReturns_When_UpdatedReferenceIsNull()
		{
			MetadataReference reference = GetExampleMetadataReference1();
			CSharpCompilation compilation = _compilation.AddReferences(reference);
			Data.CompilationData data = new(compilation);
			data.UpdateCompilation(reference, null);
			Assert.True(data.Compilation is not null && data.Compilation == compilation);
		}

		[Fact]
		public void UpdateCompilationReturns_When_ReferenceCollectionIsNull()
		{
			Data.CompilationData data = new(_compilation);
			data.UpdateCompilation(references: null);
			Assert.True(data.Compilation is not null && data.Compilation == _compilation);
		}

		[Fact]
		public void UpdateCompilationAddsSyntaxTree()
		{
			CSharpSyntaxTree tree = GetExampleSyntaxTree1();
			Data.CompilationData data = new(_compilation);
			data.UpdateCompilation(tree);
			Assert.True(data.Compilation is not null && data.Compilation != _compilation && data.Compilation.ContainsSyntaxTree(tree));
		}

		[Fact]
		public void UpdateCompilationReplacesOriginalSyntaxTree()
		{
			CSharpSyntaxTree original = GetExampleSyntaxTree1();
			CSharpSyntaxTree replaced = GetExampleSyntaxTree2();
			CSharpCompilation compilation = _compilation.AddSyntaxTrees(original);
			Data.CompilationData data = new(compilation);
			data.UpdateCompilation(original, replaced);
			Assert.True(data.Compilation is not null && data.Compilation != compilation && data.Compilation.ContainsSyntaxTree(replaced) && !data.Compilation.ContainsSyntaxTree(original));
		}

		[Fact]
		public void UpdateCompilationAddsMultipleSyntaxTreesAtOnce()
		{
			CSharpSyntaxTree[] trees = { GetExampleSyntaxTree1(), GetExampleSyntaxTree2() };
			Data.CompilationData data = new(_compilation);
			data.UpdateCompilation(trees);
			Assert.True(data.Compilation is not null && data.Compilation != _compilation && data.Compilation.ContainsSyntaxTree(trees[0]) && data.Compilation.ContainsSyntaxTree(trees[1]));
		}

		[Fact]
		public void UpdateCompilationAddsMetadataReference()
		{
			MetadataReference reference = GetExampleMetadataReference1();
			Data.CompilationData data = new(_compilation);
			data.UpdateCompilation(reference);
			Assert.True(data.Compilation is not null && data.Compilation != _compilation && data.Compilation.References.Contains(reference));
		}

		[Fact]
		public void UpdateCompilationReplacesOriginalMetadataReference()
		{
			MetadataReference original = GetExampleMetadataReference1();
			MetadataReference replaced = GetExampleMetadataReference2();
			CSharpCompilation compilation = _compilation.AddReferences(original);
			Data.CompilationData data = new(compilation);
			data.UpdateCompilation(original, replaced);
			Assert.True(data.Compilation is not null && data.Compilation != compilation && data.Compilation.References.Contains(replaced) && !data.Compilation.References.Contains(original));
		}

		[Fact]
		public void UpdateCompilationAddsMultipleMetadataReferencesAtOnce()
		{
			MetadataReference[] references = { GetExampleMetadataReference1(), GetExampleMetadataReference2() };
			Data.CompilationData data = new(_compilation);
			data.UpdateCompilation(references);
			Assert.True(data.Compilation is not null && data.Compilation != _compilation && data.Compilation.References.Contains(references[0]) && data.Compilation.References.Contains(references[1]));
		}

		private static CSharpSyntaxTree GetExampleSyntaxTree1()
		{
			return (CSharpSyntaxTree)CSharpSyntaxTree.ParseText("class Test { }");
		}

		private static CSharpSyntaxTree GetExampleSyntaxTree2()
		{
			return (CSharpSyntaxTree)CSharpSyntaxTree.ParseText("class Parent { }");
		}

		private static MetadataReference GetExampleMetadataReference1()
		{
			return MetadataReference.CreateFromFile(typeof(BigInteger).Assembly.Location);
		}

		private static MetadataReference GetExampleMetadataReference2()
		{
			return MetadataReference.CreateFromFile(typeof(MetadataReference).Assembly.Location);
		}
	}
}
