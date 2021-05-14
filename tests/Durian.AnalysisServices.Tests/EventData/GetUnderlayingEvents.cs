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
			Generator.Data.EventData data = GetEventProperty("class Test { event System.Action e { add { } remove { } }");
			Generator.Data.EventData[] other = data.GetUnderlayingEvents().ToArray();

			Assert.Empty(other);
		}

		[Fact]
		public void ReturnsSelf_When_HasNoOtherEventFieldsOnDeclaration()
		{
			Generator.Data.EventData data = GetEventField("class Test { event System.Action e; }");
			Generator.Data.EventData[] other = data.GetUnderlayingEvents().ToArray();

			Assert.True(other.Length == 1 && data == other[0]);
		}

		[Fact]
		public void CanReturnMultipleEventFields()
		{
			Generator.Data.EventData data = GetEventField("class Test { event System.Action e1, e2; }");
			Generator.Data.EventData[] other = data.GetUnderlayingEvents().ToArray();

			Assert.True(other.Length == 2 && other.Any(d => d.Name == "e1") && other.Any(d => d.Name == "e2"));
		}

		[Fact]
		public void ReturnsItselfAmongOtherEventFields()
		{
			Generator.Data.EventData data = GetEventField("class Test { event System.Action e1, e2; }");
			Generator.Data.EventData[] other = data.GetUnderlayingEvents().ToArray();

			Assert.True(other.Length == 2 && other.Any(d => d == data) && other.Any(d => d.Name == "e2"));
		}
	}
}
