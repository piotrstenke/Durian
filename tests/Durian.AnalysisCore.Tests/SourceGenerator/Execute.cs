using System;
using Durian.Data;
using Durian.Tests.Fixtures;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using Moq;
using Xunit;

namespace Durian.Tests.AnalysisCore.SourceGenerator
{
	public sealed class Execute : IClassFixture<CompilationFixture>
	{
		private readonly CSharpCompilation _compilation;

		public Execute(CompilationFixture fixture)
		{
			_compilation = fixture.Compilation;
		}

		[Fact]
		public void Returns_When_SyntaxReceiverIsOfWrongType()
		{
			GeneratorExecutionContext context = RoslynUtilities.CreateExecutionContext(_compilation, (GeneratorInitializationContext init)
				=> init.RegisterForSyntaxNotifications(() => Mock.Of<ISyntaxReceiver>()));

			Assert.True(RunAndValidate(in context, g => g.SyntaxReceiver is null));
		}

		[Fact]
		public void Returns_When_SyntaxReceiverIsEmpty()
		{
			Mock<IDurianSyntaxReceiver> mock = new();
			mock.Setup(receiver => receiver.IsEmpty()).Returns(true);

			Assert.True(RunAndValidate(GetValidContext(mock.Object), g => g.SyntaxReceiver is null));
		}

		[Fact]
		public void SetsSyntaxReceiver_When_SyntaxReceiverIsValid()
		{
			IDurianSyntaxReceiver syntaxReceiver = GetValidSyntaxReceiver();
			GeneratorExecutionContext context = GetValidContext(syntaxReceiver);

			Assert.True(RunAndValidate(in context, g => g.SyntaxReceiver is not null && g.SyntaxReceiver == syntaxReceiver));
		}

		[Fact]
		public void Returns_When_CompilationIsNotCSharpCompilation()
		{
			CSharpCompilation baseCompilation = RoslynUtilities.CreateBaseCompilation();
			VisualBasicCompilation invalidCompilation = VisualBasicCompilation.Create(
				baseCompilation.AssemblyName,
				baseCompilation.SyntaxTrees,
				baseCompilation.References
			);

			GeneratorExecutionContext context = GetValidContext(GetValidSyntaxReceiver(), invalidCompilation);

			Assert.True(RunAndValidate(in context, g => g.TargetCompilation is null));
		}

		[Fact]
		public void Returns_When_CreateCompilationDataReturnsNull()
		{
			DurianSourceGeneratorProxy generator = new();
			generator.OnCreateCompilationData += c => null;
			GeneratorExecutionContext context = GetValidContext();

			generator.Execute(in context);

			Assert.True(generator.TargetCompilation is null);
		}

		[Fact]
		public void Returns_When_CompilationDataHasErrors()
		{
			Mock<ICompilationData> mock = new();
			mock.SetupGet(c => c.Compilation).Returns(_compilation);
			mock.SetupGet(c => c.HasErrors).Returns(true);
			ICompilationData compilation = mock.Object;

			DurianSourceGeneratorProxy generator = new();
			generator.OnCreateCompilationData += c => compilation;
			GeneratorExecutionContext context = GetValidContext();

			generator.Execute(in context);

			Assert.True(generator.TargetCompilation is null);
		}

		[Fact]
		public void SetsCompilation_When_CompilationIsValid()
		{
			ICompilationData compilation = GetValidCompilationData(_compilation);

			DurianSourceGeneratorProxy generator = new();
			generator.OnCreateCompilationData += c => compilation;
			GeneratorExecutionContext context = GetValidContext();

			generator.Execute(in context);

			Assert.True(generator.TargetCompilation is not null && generator.TargetCompilation == compilation);
		}

		[Fact]
		public void SetsParseOptions_WhenAllDataIsValid()
		{
			GeneratorExecutionContext context = GetValidContext();

			Assert.True(RunAndValidate(in context, g => g.ParseOptions is not null && g.ParseOptions == context.ParseOptions));
		}

		private static ICompilationData GetValidCompilationData(CSharpCompilation compilation)
		{
			Mock<ICompilationData> mock = new();
			mock.SetupGet(c => c.Compilation).Returns(compilation);
			mock.SetupGet(c => c.HasErrors).Returns(false);
			return mock.Object;
		}

		private GeneratorExecutionContext GetValidContext()
		{
			return GetValidContext(GetValidSyntaxReceiver(), _compilation);
		}

		private GeneratorExecutionContext GetValidContext(IDurianSyntaxReceiver syntaxReceiver)
		{
			return GetValidContext(syntaxReceiver, _compilation);
		}

		private static GeneratorExecutionContext GetValidContext(IDurianSyntaxReceiver syntaxReceiver, Compilation compilation)
		{
			GeneratorExecutionContext context = RoslynUtilities.CreateExecutionContext(compilation, (GeneratorInitializationContext init)
				=> init.RegisterForSyntaxNotifications(() => syntaxReceiver));

			return context;
		}

		private static IDurianSyntaxReceiver GetValidSyntaxReceiver()
		{
			Mock<IDurianSyntaxReceiver> mock = new();
			mock.Setup(receiver => receiver.IsEmpty()).Returns(false);

			return mock.Object;
		}

		private static bool RunAndValidate(in GeneratorExecutionContext context, Predicate<Durian.SourceGenerator> predicate)
		{
			DurianSourceGeneratorProxy generator = new();
			generator.OnCreateCompilationData += GetValidCompilationData;
			generator.Execute(in context);
			return predicate(generator);
		}
	}
}