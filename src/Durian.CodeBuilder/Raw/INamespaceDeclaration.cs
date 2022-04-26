// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Builder.Markers;
using Durian.Builder.Raw;

namespace Durian.Builder.Raw
{
	/// <summary>
	/// Provides methods for building a namespace declaration.
	/// </summary>
	public interface INamespaceDeclaration : INamespaceKeyword<IWhiteSpace<INamespaceDeclarationIdentifier>>
	{
	}
}

namespace Durian.Builder
{
	public partial class RawCodeBuilder
	{
		internal partial class Adapter : INamespaceDeclaration
		{
		}
	}
}
