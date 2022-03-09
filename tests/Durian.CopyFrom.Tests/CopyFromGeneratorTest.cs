// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Logging;
using Durian.TestServices;

namespace Durian.Analysis.CopyFrom.Tests
{
	public abstract class CopyFromGeneratorTest : DurianGeneratorTest<CopyFromGenerator>
	{
		protected override ITestableGenerator CreateTestableGenerator(string testName)
		{
			return UnderlayingGenerator.CreateTestable(testName);
		}

		protected override CopyFromGenerator CreateUnderlayingGenerator(LoggingConfiguration configuration)
		{
			return new CopyFromGenerator(configuration);
		}
	}
}