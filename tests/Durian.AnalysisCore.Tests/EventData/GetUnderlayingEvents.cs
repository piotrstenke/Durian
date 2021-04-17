using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Durian.Tests.AnalysisCore.EventData
{
	public sealed class GetUnderlayingEvents : CompilationTest
	{
		[Fact]
		public void ReturnsEmpty_When_IsNotEventField()
		{
			Data.EventData data = GetEventProperty("class Test { event System.Action e { add { } remove { } }");
			Data.EventData[] other = data.GetUnderlayingEvents().ToArray();

			Assert.Empty(other);
		}

		[Fact]
		public void ReturnsSelf_When_HasNoOtherEventFieldsOnDeclaration()
		{
			Data.EventData data = GetEventField("class Test { event System.Action e; }");
			Data.EventData[] other = data.GetUnderlayingEvents().ToArray();

			Assert.True(other.Length == 1 && data == other[0]);
		}

		[Fact]
		public void CanReturnMultipleEventFields()
		{
			Data.EventData data = GetEventField("class Test { event System.Action e1, e2; }");
			Data.EventData[] other = data.GetUnderlayingEvents().ToArray();

			Assert.True(other.Length == 2 && other.Any(d => d.Name == "e1") && other.Any(d => d.Name == "e2"));
		}

		[Fact]
		public void ReturnsItselfAmongOtherEventFields()
		{
			Data.EventData data = GetEventField("class Test { event System.Action e1, e2; }");
			Data.EventData[] other = data.GetUnderlayingEvents().ToArray();

			Assert.True(other.Length == 2 && other.Any(d => d == data) && other.Any(d => d.Name == "e2"));
		}
	}
}
