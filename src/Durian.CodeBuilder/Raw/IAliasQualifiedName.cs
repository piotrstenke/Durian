// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Builder.Markers;
using Durian.Builder.Raw;

namespace Durian.Builder.Raw
{
	/// <summary>
	/// Provides methods for building a qualified name with an alias.
	/// </summary>
	/// <typeparam name="TDelimiter">Type of builder that is the delimiter.</typeparam>
	public interface IAliasQualifiedName<out TDelimiter> :
		IGlobalAlias<IQualifiedName<TDelimiter>>,
		IIdentifier<IAliasOrIdentifier<TDelimiter>>
		where TDelimiter : class
	{
	}
}

namespace Durian.Builder
{
	public partial class RawCodeBuilder
	{
		internal partial class Adapter : IAliasQualifiedName<IWhiteSpace>
		{
		}
	}
}
