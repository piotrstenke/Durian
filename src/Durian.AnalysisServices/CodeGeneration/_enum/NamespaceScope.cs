// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis.CodeGeneration
{
	/// <summary>
	/// Type of namespace declaration.
	/// </summary>
	public enum NamespaceScope
	{
		/// <summary>
		/// Default namespace declaration (e.g. <c>namespace System { }</c>).
		/// </summary>
		Default = 0,

		/// <summary>
		/// File-scoped namespace declaration (e.g. <c>namespace System;</c>).
		/// </summary>
		File = 1
	}
}
