// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Builder.Raw;

namespace Durian.Builder.Raw
{
	/// <summary>
	/// Provides methods for building a compilation unit root.
	/// </summary>
	public interface ICompilationUnit : IExternAlias<ICompilationUnit>, ICompilationUnitUsingsListGlobal
	{
	}
}

namespace Durian.Builder
{
	public partial class RawCodeBuilder
	{
		internal partial class Adapter : ICompilationUnit
		{
		}
	}
}
