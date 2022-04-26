// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Builder.Markers;
using Durian.Builder.Raw;

namespace Durian.Builder.Raw
{
	/// <summary>
	/// Provides methods for building either an alias or an identifier in a using directive.
	/// </summary>
	/// <typeparam name="TReturn">Type of returned builder.</typeparam>
	public interface IUsingDirectiveAliasOrIdentifier<out TReturn> :
		IUsingDirectiveDelimiter<TReturn>,
		IEqualsToken<IUsingDirectiveIdentifier<TReturn>>
		where TReturn : class
	{
	}
}

namespace Durian.Builder
{
	public partial class RawCodeBuilder
	{
		internal partial class Adapter : IUsingDirectiveAliasOrIdentifier<IWhiteSpace>
		{
		}
	}
}
