// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis.CodeGeneration
{
	/// <summary>
	/// Type of namespace declaration.
	/// </summary>
	public enum NamespaceStyle
	{
		/// <summary>
		/// Default namespace declaration (e.g. <c>namespace System.Collections { }</c>).
		/// </summary>
		Default = 0,

		/// <summary>
		/// Nested namespace declaration (e.g. <c>namespace System { namespace Collections { } }</c>.
		/// </summary>
		Nested = 1,

		/// <summary>
		/// File-scoped namespace declaration (e.g. <c>namespace System.Collections;</c>).
		/// </summary>
		File = 2
	}
}
