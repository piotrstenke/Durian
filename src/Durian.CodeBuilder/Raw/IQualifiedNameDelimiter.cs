// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Builder.Markers;
using Durian.Builder.Raw;

namespace Durian.Builder.Raw
{
	/// <summary>
	/// Provides methods for building a qualified name delimiter.
	/// </summary>
	/// <typeparam name="TDelimiter">Type of builder that is the delimiter.</typeparam>
	public interface IQualifiedNameDelimiter<out TDelimiter> :
		IDotToken<IIdentifier<IQualifiedNameDelimiter<TDelimiter>>>,
		IDelimiter<TDelimiter>
		where TDelimiter : class
	{
	}
}

namespace Durian.Builder
{
	public partial class RawCodeBuilder
	{
		internal partial class Adapter : IQualifiedNameDelimiter<IWhiteSpace>
		{
		}
	}
}
