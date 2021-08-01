// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;

namespace Durian
{
	/// <summary>
	/// Informs that the specified files relative to the project directory contain definitions of diagnostics.
	/// </summary>
	[Conditional("DEBUG")]
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
	public sealed class DiagnosticFilesAttribute : Attribute
	{
		/// <summary>
		/// Files that contain definitions of diagnostics.
		/// </summary>
		public string[] Files { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DiagnosticFilesAttribute"/> class.
		/// </summary>
		/// <param name="files">Files that contain definitions of diagnostics.</param>
		public DiagnosticFilesAttribute(params string[]? files)
		{
			Files = files ?? Array.Empty<string>();
		}
	}
}
