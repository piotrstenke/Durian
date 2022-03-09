// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics;

namespace Durian.TestServices.Fixtures
{
	/// <summary>
	/// A simple class that contains a single <see cref="CSharpCompilation"/> property. Useful when using the <c>Xunit.IClassFixture{T}</c> interface.
	/// </summary>
	[DebuggerDisplay("{Compilation}")]
	public class CompilationFixture
	{
		/// <summary>
		/// A <see cref="CSharpCompilation"/> that is created by calling the <see cref="RoslynUtilities.CreateBaseCompilation(bool)"/> method.
		/// </summary>
		public CSharpCompilation Compilation { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CompilationFixture"/> class.
		/// </summary>
		public CompilationFixture()
		{
			Compilation = RoslynUtilities.CreateBaseCompilation();
		}
	}
}