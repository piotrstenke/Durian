// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis
{
	/// <summary>
	/// Type of namespace declaration.
	/// </summary>
	public enum NamespaceType
	{
		/// <summary>
		/// Default namespace declaration (e.g. <c>namespace System { }</c>).
		/// </summary>
		Default,

		/// <summary>
		/// File-scoped namespace declaration (e.g. <c>namespace System;</c>).
		/// </summary>
		FileScoped
	}
}
