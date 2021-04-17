using System;
using System.Linq;
using Xunit;

namespace Durian.Tests.AnalysisCore.AnalysisUtilities
{
	public sealed class SortUsings
	{
		[Fact]
		public void ThrowsArgumentNullException_When_CollectionIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => Durian.AnalysisUtilities.SortUsings(null!));
		}

		[Fact]
		public void ReturnsSameAmountOfElementsAsInput()
		{
			string[] input = { "Xunit", "Durian", "Durian.Extensions" };

			Assert.True(Durian.AnalysisUtilities.SortUsings(input).Count() == input.Length);
		}

		[Fact]
		public void IsSortedAlphabetically_When_HasNoSystemUsings()
		{
			string[] input = { "Xunit", "Durian", "Durian.Extensions" };
			string[] output = Durian.AnalysisUtilities.SortUsings(input).ToArray();
			string[] expected = output.OrderBy(n => n).ToArray();

			int index = 0;

			Assert.True(output[index] == expected[index++]);
		}

		[Fact]
		public void SystemUsingsAreFirst_When_HasSystemUsings()
		{
			string[] input = { "Xunit", "Durian", "System", "Durian.Extensions", "System.Collections.Generic" };
			string[] output = Durian.AnalysisUtilities.SortUsings(input).ToArray();

			Assert.True(output.Length > 2 && output[0] == "System" && output[1] == "System.Collections.Generic");
		}

		[Fact]
		public void UsingsThatOnlyStartWithSystemAreNotFirst()
		{
			string[] input = { "Xunit", "SystemTest", "Durian", "Durian.Extensions" };
			string? first = Durian.AnalysisUtilities.SortUsings(input).FirstOrDefault();

			Assert.True(first is not null and not "SystemTest");
		}
	}
}
