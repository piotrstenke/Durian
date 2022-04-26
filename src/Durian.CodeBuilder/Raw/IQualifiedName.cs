// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Builder.Markers;
using Durian.Builder.Raw;

namespace Durian.Builder.Raw
{
	/// <summary>
	/// Provides methods for building a qualified name.
	/// </summary>
	/// <typeparam name="TDelimiter">Type of builder that is the delimiter.</typeparam>
	public interface IQualifiedName<out TDelimiter> : IIdentifier<IQualifiedNameDelimiter<TDelimiter>> where TDelimiter : class
	{
	}
}

namespace Durian.Builder
{
	public partial class RawCodeBuilder
	{
		internal partial class Adapter : IQualifiedName<IWhiteSpace>
		{
		}
	}
}
