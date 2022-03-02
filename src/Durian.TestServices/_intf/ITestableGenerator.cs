// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis;
using Durian.Analysis.Logging;

namespace Durian.TestServices
{
    /// <summary>
    /// <see cref="IDurianGenerator"/> that provides better test-related logging experience.
    /// </summary>
    public interface ITestableGenerator : IDurianGenerator, ILoggableGenerator
    {
        /// <summary>
        /// <see cref="IDurianGenerator"/> that is used to actually generate sources.
        /// </summary>
        IDurianGenerator UnderlayingGenerator { get; }
    }
}