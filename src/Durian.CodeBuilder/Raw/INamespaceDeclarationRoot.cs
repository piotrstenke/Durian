// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Builder.Raw;

namespace Durian.Builder.Raw
{
	/// <summary>
	/// Provides methods for building a namespace declaration root.
	/// </summary>
	public interface INamespaceDeclarationRoot :
		IExternAlias<INamespaceDeclarationRoot>,
		INamespaceDeclarationUsingsList
	{
	}
}

namespace Durian.Builder
{
	public partial class RawCodeBuilder
	{
		internal partial class Adapter : INamespaceDeclarationRoot
		{
		}
	}
}