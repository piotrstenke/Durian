// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Builder.Markers;
using Durian.Builder.Raw;

namespace Durian.Builder.Raw
{
	/// <summary>
	/// Provides methods for building a qualified name or an alias.
	/// </summary>
	/// <typeparam name="TDelimiter">Type of builder that is the delimiter.</typeparam>
	public interface IAliasOrIdentifier<out TDelimiter> :
		IColonColonToken<IQualifiedName<TDelimiter>>,
		IQualifiedNameDelimiter<TDelimiter>
		where TDelimiter : class
	{
	}
}

namespace Durian.Builder
{
	public partial class RawCodeBuilder
	{
		internal partial class Adapter : IAliasOrIdentifier<IWhiteSpace>
		{
		}
	}
}
