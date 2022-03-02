﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Logging;
using Durian.TestServices;

namespace Durian.Analysis.DefaultParam.Tests
{
    public abstract class DefaultParamGeneratorTestBase : DurianGeneratorTest<DefaultParamGenerator>
    {
        public DefaultParamGeneratorTestBase() : base(true)
        {
        }

        protected override ITestableGenerator CreateTestableGenerator(string testName)
        {
            return Generator.CreateTestable(testName);
        }

        protected override DefaultParamGenerator CreateUnderlayingGenerator(LoggingConfiguration configuration)
        {
            return new DefaultParamGenerator(configuration);
        }
    }
}