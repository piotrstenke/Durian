// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Builder.Markers;
using Durian.Builder.Raw;

namespace Durian.Builder.Raw
{
	/// <summary>
	/// Specifies that the current node has reached its end, thus control should be returned to the original node.
	/// </summary>
	/// <typeparam name="TDelimiter">Type of builder that is the delimiter.</typeparam>
	public interface IDelimiter<out TDelimiter> where TDelimiter : class
	{
	}
}

namespace Durian.Builder
{
	public partial class RawCodeBuilder
	{
		internal partial class Adapter : IDelimiter<IWhiteSpace>
		{
		}
	}
}
