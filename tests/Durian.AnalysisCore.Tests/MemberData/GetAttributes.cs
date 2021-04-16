using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Durian.Tests.CorePackage.MemberData
{
	public sealed class GetAttributes : CompilationTest
	{
		public GetAttributes() : base(Utilities.TestAttribute, Utilities.OtherAttribute)
		{

		}

		[Fact]
		public void ReturnsEmpty_When_HasNoAttributes()
		{
			Data.MemberData data = GetMember("class Test { }");
			Assert.Empty(data.GetAttributes());
		}

		[Fact]
		public void CanReturnSingleAttribute()
		{
			Data.MemberData data = GetMember("[Test]class Test { }");
			AttributeData[] attributes = data.GetAttributes().ToArray();
			Assert.True(attributes.Length == 1 && SymbolEqualityComparer.Default.Equals(attributes[0].AttributeClass, Compilation.CurrentCompilation.GetTypeByMetadataName("TestAttribute")!));
		}

		[Fact]
		public void CanReturnMultipleAttributes()
		{
			Data.MemberData data = GetMember("[Test][Other]class Test { }");
			AttributeData[] attributes = data.GetAttributes().ToArray();

			Assert.True(
				attributes.Length == 2 &&
				attributes.Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, Compilation.CurrentCompilation.GetTypeByMetadataName("TestAttribute")!)) &&
				attributes.Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, Compilation.CurrentCompilation.GetTypeByMetadataName("OtherAttribute")!))
			);
		}
	}
}
