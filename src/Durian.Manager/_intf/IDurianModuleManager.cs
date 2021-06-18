// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis;
using Durian.Info;

namespace Durian.Manager
{
	/// <summary>
	/// Represents a module-specific analysis manager.
	/// </summary>
	public interface IDurianModuleManager : IAnalyzerInfo
	{
		/// <summary>
		/// <see cref="DurianModule"/> this manager acts on.
		/// </summary>
		DurianModule Module { get; }
	}
}
