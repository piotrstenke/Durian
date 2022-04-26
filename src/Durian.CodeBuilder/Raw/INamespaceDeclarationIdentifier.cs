// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Builder.Raw;

namespace Durian.Builder.Raw
{
	/// <summary>
	/// Provides methods for building a namespace declaration identifier.
	/// </summary>
	public interface INamespaceDeclarationIdentifier : IIdentifier<INamespaceDeclarationDelimiter>
	{
	}
}

namespace Durian.Builder
{
	public partial class RawCodeBuilder
	{
		internal partial class Adapter : INamespaceDeclarationIdentifier
		{
		}
	}
}
