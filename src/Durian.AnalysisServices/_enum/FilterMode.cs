// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Generator
{
	/// <summary>
	/// Determines what information should a <see cref="IGeneratorSyntaxFilterWithDiagnostics"/> emit.
	/// </summary>
	public enum FilterMode
	{
		/// <summary>
		/// Filter emits no additional information.
		/// </summary>
		None = 0,

		/// <summary>
		/// Filter reports <see cref="Diagnostic"/>s for the invalid <see cref="CSharpSyntaxNode"/>s.
		/// </summary>
		Diagnostics = 1,

		/// <summary>
		/// Filter creates log files for the invalid <see cref="CSharpSyntaxNode"/>s.
		/// </summary>
		Logs = 2,

		/// <summary>
		/// Filter both creates log files and reports <see cref="Diagnostic"/>s for the invalid <see cref="CSharpSyntaxNode"/>s.
		/// </summary>
		Both = 3
	}
}
