// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Builder.Raw;

namespace Durian.Builder.Raw
{
	/// <summary>
	/// Provides methods for building a list of usings inside a namespace declaration.
	/// </summary>
	public interface INamespaceDeclarationUsingsList
	{
	}
}

namespace Durian.Builder
{
	public partial class RawCodeBuilder
	{
		internal partial class Adapter : INamespaceDeclarationUsingsList
		{
		}
	}
}
