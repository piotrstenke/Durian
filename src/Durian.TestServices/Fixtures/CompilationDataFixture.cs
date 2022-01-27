// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.TestServices.Fixtures
{
	/// <summary>
	/// A simple class that contains a single <see cref="TestableCompilationData"/> property. Useful when using the <c>Xunit.IClassFixture{T}</c> interface.
	/// </summary>
	[DebuggerDisplay("{Compilation}")]
	public class CompilationDataFixture
	{
		/// <summary>
		/// A <see cref="CSharpCompilation"/> that is created by calling the <see cref="TestableCompilationData.Create(bool)"/> method.
		/// </summary>
		public TestableCompilationData Compilation { get; } = TestableCompilationData.Create();

		/// <summary>
		/// Initializes a new instance of the <see cref="CompilationDataFixture"/> class.
		/// </summary>
		public CompilationDataFixture()
		{
			Compilation = TestableCompilationData.Create();
		}
	}
}