// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;

namespace Durian
{
	/// <summary>
	/// Specifies external diagnostic that should be included in this module.
	/// </summary>
	[Conditional("DEBUG")]
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
	public sealed class IncludeDiagnosticsAttribute : Attribute
	{
		/// <summary>
		/// Array of ids of external diagnostics to include in this module.
		/// </summary>
		public string[] DiagnosticIds { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="IncludeDiagnosticsAttribute"/> class.
		/// </summary>
		/// <param name="diagnosticIds">Array of ids of external diagnostics to include in this module.</param>
		public IncludeDiagnosticsAttribute(params string[]? diagnosticIds)
		{
			DiagnosticIds = diagnosticIds ?? Array.Empty<string>();
		}
	}
}