// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Builder.Raw;

namespace Durian.Builder.Raw
{
	/// <summary>
	/// Provides methods for building a list of global usings inside the compilation unit root.
	/// </summary>
	public interface ICompilationUnitUsingsListGlobal : IUsingDirectiveGlobal, ICompilationUnitUsingsList
	{
	}
}

namespace Durian.Builder
{
	public partial class RawCodeBuilder
	{
		internal partial class Adapter : ICompilationUnitUsingsListGlobal
		{
		}
	}
}
