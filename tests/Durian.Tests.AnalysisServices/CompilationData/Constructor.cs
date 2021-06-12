// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Xunit;

namespace Durian.Tests.AnalysisServices.CompilationData
{
	public sealed class Constructor
	{
		[Fact]
		public void CompilationIsNotNullAfterConstructor()
		{
			Generator.Data.CompilationData data = new(RoslynUtilities.CreateBaseCompilation());
			Assert.True(data.Compilation is not null);
		}

		[Fact]
		public void ConstructorThrowsArgumentNullException_When_CompilationIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => new Generator.Data.CompilationData(null!));
		}

		[Fact]
		public void HasErrorsReturnsFalseByDefault()
		{
			Generator.Data.CompilationData data = new(RoslynUtilities.CreateBaseCompilation());
			Assert.False(data.HasErrors);
		}
	}
}
