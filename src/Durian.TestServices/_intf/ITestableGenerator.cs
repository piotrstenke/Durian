// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;

namespace Durian.TestServices
{
	/// <summary>
	/// A wrapper for <see cref="ISourceGenerator"/> that offers better logging experience.
	/// </summary>
	public interface ITestableGenerator : ILoggableGenerator
	{
		/// <summary>
		/// <see cref="ISourceGenerator"/> that is used to actually generate sources.
		/// </summary>
		ILoggableGenerator UnderlayingGenerator { get; }
	}
}
