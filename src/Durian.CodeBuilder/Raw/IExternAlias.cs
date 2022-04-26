// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Builder.Markers;
using Durian.Builder.Raw;

namespace Durian.Builder.Raw
{
	/// <summary>
	/// Provides methods for building an extern alias directive.
	/// </summary>
	public interface IExternAlias<out TReturn> : IExternKeyword<IWhiteSpace<IAliasKeyword<IWhiteSpace<IIdentifier<ISemicolonToken<TReturn>>>>>> where TReturn : class
	{
	}
}

namespace Durian.Builder
{
	public partial class RawCodeBuilder
	{
		internal partial class Adapter : IExternAlias<IWhiteSpace>
		{
		}
	}
}
