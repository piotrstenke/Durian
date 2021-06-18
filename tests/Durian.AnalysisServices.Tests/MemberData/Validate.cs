// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.TestServices;
using Xunit;

namespace Durian.Analysis.Tests.MemberData
{
	public sealed class Validate : CompilationTest
	{
		[Fact]
		public void ReturnsTrueByDefault()
		{
			Analysis.Data.MemberData data = GetMember("class Test { }");
			Assert.True(data.Validate(RoslynUtilities.CreateExecutionContext()));
		}
	}
}
