using System;
using Xunit;

namespace Durian.Tests.AnalysisCore.CompilationData
{
	public sealed class Constructor
	{
		[Fact]
		public void ConstructorThrowsArgumentNullException_When_CompilationIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => new Data.CompilationData(null!));
		}

		[Fact]
		public void CompilationIsNotNullAfterConstructor()
		{
			Data.CompilationData data = new(RoslynUtilities.CreateBaseCompilation());
			Assert.True(data.Compilation is not null);
		}

		[Fact]
		public void HasErrorsReturnsFalseByDefault()
		{
			Data.CompilationData data = new(RoslynUtilities.CreateBaseCompilation());
			Assert.False(data.HasErrors);
		}
	}
}
