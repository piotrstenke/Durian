using System;
using Xunit;

namespace Durian.Tests.AnalysisCore.AnalysisUtilities
{
	public sealed class JoinNamespaces
	{
		[Fact]
		public void ThrowsArgumentNullException_When_NamespacesIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => Durian.AnalysisUtilities.JoinNamespaces(null!));
		}

		[Fact]
		public void ReturnsEmptyString_When_NamespacesIsEmpty()
		{
			Assert.True(Durian.AnalysisUtilities.JoinNamespaces(Array.Empty<string>()) == string.Empty);
		}

		[Fact]
		public void ReturnsSingleNamespace_When_NamespacesContainsOneElement()
		{
			Assert.True(Durian.AnalysisUtilities.JoinNamespaces(new string[] { "System" }) == "System");
		}

		[Fact]
		public void ReturnsMultipleNamespaces_When_NamespacesContainsMultipleElements()
		{
			Assert.True(Durian.AnalysisUtilities.JoinNamespaces(new string[] { "System", "Collections", "Generic" }) == "System.Collections.Generic");
		}
	}
}
