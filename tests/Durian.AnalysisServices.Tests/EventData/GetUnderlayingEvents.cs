// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Linq;
using Durian.TestServices;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Durian.Analysis.Tests.EventData
{
	public sealed class GetUnderlayingEvents : CompilationTest
	{
		[Fact]
		public void CanReturnMultipleEventFields()
		{
			Analysis.Data.EventData data = GetEventField("class Test { event System.Action e1, e2; }");
			Analysis.Data.EventData[] other = data.GetUnderlayingEvents().ToArray();

			Assert.True(other.Length == 2 && other.Any(d => d.Name == "e1") && other.Any(d => d.Name == "e2"));
		}

		[Fact]
		public void ReturnsEmpty_When_IsNotEventField()
		{
			Analysis.Data.EventData data = GetEventProperty("class Test { event System.Action e { add { } remove { } }");
			Analysis.Data.EventData[] other = data.GetUnderlayingEvents().ToArray();

			Assert.Empty(other);
		}

		[Fact]
		public void ReturnsItselfAmongOtherEventFields()
		{
			Analysis.Data.EventData data = GetEventField("class Test { event System.Action e1, e2; }");
			Analysis.Data.EventData[] other = data.GetUnderlayingEvents().ToArray();

			Assert.True(other.Length == 2 && other.Any(d => d == data) && other.Any(d => d.Name == "e2"));
		}

		[Fact]
		public void ReturnsSelf_When_HasNoOtherEventFieldsOnDeclaration()
		{
			Analysis.Data.EventData data = GetEventField("class Test { event System.Action e; }");
			Analysis.Data.EventData[] other = data.GetUnderlayingEvents().ToArray();

			Assert.True(other.Length == 1 && data == other[0]);
		}
	}
}
