// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Builder.Markers;
using Durian.Builder.Raw;

namespace Durian.Builder.Raw
{
	/// <summary>
	/// Provides methods for building a using directive with a <see langword="global"/> keyword.
	/// </summary>
	public interface IUsingDirectiveGlobal :
		IGlobalKeyword<IWhiteSpace<IUsingDirective<ICompilationUnitUsingsListGlobal>>,
		IUsingDirective<ICompilationUnitUsingsListGlobal>
	{
	}
}

namespace Durian.Builder
{
	public partial class RawCodeBuilder
	{
		internal partial class Adapter : IUsingDirectiveGlobal
		{
		}
	}
}