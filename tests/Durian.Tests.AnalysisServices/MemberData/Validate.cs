// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Xunit;

namespace Durian.Tests.AnalysisServices.MemberData
{
	public sealed class Validate : CompilationTest
	{
		[Fact]
		public void ReturnsTrueByDefault()
		{
			Generator.Data.MemberData data = GetMember("class Test { }");
			Assert.True(data.Validate(RoslynUtilities.CreateExecutionContext()));
		}
	}
}
