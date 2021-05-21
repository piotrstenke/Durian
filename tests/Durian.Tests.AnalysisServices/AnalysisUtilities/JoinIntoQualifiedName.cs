using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Durian.Tests.AnalysisServices.AnalysisUtilities
{
	public sealed class JoinIntoQualifiedName
	{
		[Fact]
		public void ThrowsArgumentNullException_When_NamesIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => Durian.Generator.AnalysisUtilities.JoinIntoQualifiedName(null!));
		}

		[Fact]
		public void ReturnsNull_When_ThereWereLessThan2Names()
		{
			Assert.True(Durian.Generator.AnalysisUtilities.JoinIntoQualifiedName(new string[] { "System" }) is null);
		}

		[Fact]
		public void ReturnsQualifiedName_When_ThereWere2OrMoreNames()
		{
			QualifiedNameSyntax? syntax = Durian.Generator.AnalysisUtilities.JoinIntoQualifiedName(new string[] { "System", "Collections", "Generic" });
			Assert.True(syntax is not null && syntax.ToFullString() == "System.Collections.Generic");
		}
	}
}
