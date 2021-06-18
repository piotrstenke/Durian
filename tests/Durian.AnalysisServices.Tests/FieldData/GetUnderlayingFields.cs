// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Linq;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Durian.Analysis.Tests.FieldData
{
	public sealed class GetUnderlayingFields : CompilationTest
	{
		[Fact]
		public void CanReturnMultipleFields()
		{
			Analysis.Data.FieldData data = GetField("class Test { int field1, field2; }");
			Analysis.Data.FieldData[] other = data.GetUnderlayingFields().ToArray();

			Assert.True(other.Length == 2 && other.Any(d => d.Name == "field1") && other.Any(d => d.Name == "field2"));
		}

		[Fact]
		public void ReturnsItselfAmongOtherFields()
		{
			Analysis.Data.FieldData data = GetField("class Test { int field1, field2; }");
			Analysis.Data.FieldData[] other = data.GetUnderlayingFields().ToArray();

			Assert.True(other.Length == 2 && other.Any(d => d == data) && other.Any(d => d.Name == "field2"));
		}

		[Fact]
		public void ReturnsSelf_When_HasNoOtherFieldsOnDeclaration()
		{
			Analysis.Data.FieldData data = GetField("class Test { int field; }");
			Analysis.Data.FieldData[] other = data.GetUnderlayingFields().ToArray();

			Assert.True(other.Length == 1 && data == other[0]);
		}
	}
}
