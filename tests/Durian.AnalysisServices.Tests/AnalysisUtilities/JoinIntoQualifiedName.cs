// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Durian.Analysis.Tests.AnalysisUtilities
{
	public sealed class JoinIntoQualifiedName
	{
		[Fact]
		public void ReturnsNull_When_ThereWereLessThan2Names()
		{
			Assert.True(Analysis.AnalysisUtilities.JoinIntoQualifiedName(new string[] { "System" }) is null);
		}

		[Fact]
		public void ReturnsQualifiedName_When_ThereWere2OrMoreNames()
		{
			QualifiedNameSyntax? syntax = Analysis.AnalysisUtilities.JoinIntoQualifiedName(new string[] { "System", "Collections", "Generic" });
			Assert.True(syntax is not null && syntax.ToFullString() == "System.Collections.Generic");
		}

		[Fact]
		public void ThrowsArgumentNullException_When_NamesIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => Analysis.AnalysisUtilities.JoinIntoQualifiedName(null!));
		}
	}
}