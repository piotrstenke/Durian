// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Builder.Markers;
using Durian.Builder.Raw;

namespace Durian.Builder.Raw
{
	/// <summary>
	/// Provides methods for building a static or aliased using directive.
	/// </summary>
	/// <typeparam name="TReturn">Type of returned builder.</typeparam>
	public interface IUsingDirectiveIdentifier<out TReturn> : IIdentifier<IUsingDirectiveDelimiter<TReturn>> where TReturn : class
	{
	}
}

namespace Durian.Builder
{
	public partial class RawCodeBuilder
	{
		internal partial class Adapter : IUsingDirectiveIdentifier<IWhiteSpace>
		{
		}
	}
}
