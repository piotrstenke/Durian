// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Builder.Markers;
using Durian.Builder.Raw;

namespace Durian.Builder.Raw
{
	/// <summary>
	/// Provides methods for building a namespace declaration delimiter (either open brace '{' or semicolon ';').
	/// </summary>
	public interface INamespaceDeclarationDelimiter :
		IOpenBraceToken<INamespaceDeclarationRoot>,
		ISemicolonToken<INamespaceDeclarationRoot>,
		IDotToken<INamespaceDeclarationIdentifier>
	{
	}
}

namespace Durian.Builder
{
	public partial class RawCodeBuilder
	{
		internal partial class Adapter : INamespaceDeclarationDelimiter
		{
		}
	}
}
