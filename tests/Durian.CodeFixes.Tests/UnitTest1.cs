using System;
using Xunit;
using Durian.Tests;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.CodeFixes.Tests
{
	public class RemoveCodeGenerationAttributes : CompilationTest
	{
		[Fact]
		public void RemovesDurianGeneratedAttribute()
		{
			DiagnosticBag bag = new();

			IMethodSymbol symbol = GetSymbol<IMethodSymbol, MethodDeclarationSyntax>("class Test { [Durian.Generator.DurianGenerated]void Method() { } }");
			DurianDiagnostics.AttributeCannotBeAppliedToMembersWithAttribute(bag, symbol, null, "DurianGenerated");
		}
	}
}
