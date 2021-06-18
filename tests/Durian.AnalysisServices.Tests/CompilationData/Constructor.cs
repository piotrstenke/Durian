// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Durian.TestServices;
using Xunit;

namespace Durian.Analysis.Tests.CompilationData
{
	public sealed class Constructor
	{
		[Fact]
		public void CompilationIsNotNullAfterConstructor()
		{
			Analysis.Data.CompilationData data = new(RoslynUtilities.CreateBaseCompilation());
			Assert.True(data.Compilation is not null);
		}

		[Fact]
		public void ConstructorThrowsArgumentNullException_When_CompilationIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => new Analysis.Data.CompilationData(null!));
		}

		[Fact]
		public void HasErrorsReturnsFalseByDefault()
		{
			Analysis.Data.CompilationData data = new(RoslynUtilities.CreateBaseCompilation());
			Assert.False(data.HasErrors);
		}
	}
}
