using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Durian.Tests.AnalysisServices.FieldData
{
	public sealed class GetUnderlayingFields : CompilationTest
	{
		[Fact]
		public void ReturnsSelf_When_HasNoOtherFieldsOnDeclaration()
		{
			Generator.Data.FieldData data = GetField("class Test { int field; }");
			Generator.Data.FieldData[] other = data.GetUnderlayingFields().ToArray();

			Assert.True(other.Length == 1 && data == other[0]);
		}

		[Fact]
		public void CanReturnMultipleFields()
		{
			Generator.Data.FieldData data = GetField("class Test { int field1, field2; }");
			Generator.Data.FieldData[] other = data.GetUnderlayingFields().ToArray();

			Assert.True(other.Length == 2 && other.Any(d => d.Name == "field1") && other.Any(d => d.Name == "field2"));
		}

		[Fact]
		public void ReturnsItselfAmongOtherFields()
		{
			Generator.Data.FieldData data = GetField("class Test { int field1, field2; }");
			Generator.Data.FieldData[] other = data.GetUnderlayingFields().ToArray();

			Assert.True(other.Length == 2 && other.Any(d => d == data) && other.Any(d => d.Name == "field2"));
		}
	}
}