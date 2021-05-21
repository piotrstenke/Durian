using System;
using System.Linq;
using Durian.Generator.Data;
using Durian.Generator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Durian.Tests.AnalysisServices.SemanticModelExtensions
{
	public sealed class GetUsedNamespaces : CompilationTest
	{
		[Fact]
		public void ThrowsArgumentNullException_When_SemanticModelIsNull()
		{
			IMemberData member = GetClass("class Test { }");
			SemanticModel? semanticModel = null;
			Assert.Throws<ArgumentNullException>(() => semanticModel!.GetUsedNamespaces(member.Declaration, Compilation));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_SyntaxNodeIsNull()
		{
			IMemberData member = GetClass("class Test { }");
			Assert.Throws<ArgumentNullException>(() => member.SemanticModel.GetUsedNamespaces(null!, Compilation));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_CompilationDataIsNull()
		{
			IMemberData member = GetClass("class Test { }");
			Assert.Throws<ArgumentNullException>(() => member.SemanticModel.GetUsedNamespaces(member.Declaration, compilationData: null!));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_CompilationIsNull()
		{
			IMemberData member = GetClass("class Test { }");
			Assert.Throws<ArgumentNullException>(() => member.SemanticModel.GetUsedNamespaces(member.Declaration, compilation: null!));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_AssemblySymbolIsNull()
		{
			IMemberData member = GetClass("class Test { }");
			Assert.Throws<ArgumentNullException>(() => member.SemanticModel.GetUsedNamespaces(member.Declaration, assembly: null!));
		}

		[Fact]
		public void ThrowsArgumentNullException_When_GlobalNamespaceIsNull()
		{
			IMemberData member = GetClass("class Test { }");
			Assert.Throws<ArgumentNullException>(() => member.SemanticModel.GetUsedNamespaces(member.Declaration, globalNamespace: null!));
		}

		[Fact]
		public void ThrowsArgumentException_When_GlobalNamespaceIsNotActuallyGlobal()
		{
			IMemberData member = GetClass("namespace N { class Test { } }");
			Assert.Throws<ArgumentException>(() => member.SemanticModel.GetUsedNamespaces(member.Declaration, member.Symbol.ContainingNamespace));
		}

		[Fact]
		public void ReturnsEmpty_When_UsesBuiltInTypesOnly()
		{
			Assert.True(Execute("class Test { int a; }").Length == 0);
		}

		[Fact]
		public void ReturnsEmpty_When_UsesTypesFromTheSameNamespace()
		{
			Assert.True(Execute("namespace N { class Test1 { int a; } class T2 { T1 t; } }", 1).Length == 0);
		}

		[Fact]
		public void ReturnsEmpty_When_UsesTypesFromGlobalNamespace()
		{
			Assert.True(Execute("class Test1 { int a; } namespace N { class T2 { T1 t; } }", 1).Length == 0);
		}

		[Fact]
		public void ReturnsEmpty_When_UsesBuiltInTypesAsCSharpTypes()
		{
			Assert.True(Execute("class Test { System.Int32 a; }").Length == 0);
		}

		[Fact]
		public void ReturnsEmpty_When_UsingIsUnnecessary()
		{
			Assert.True(Execute("using System; class Test { int a; }").Length == 0);
		}

		[Fact]
		public void ReturnsEmpty_When_UsesTypeFromParentNamespaceOfCurrentNamespace()
		{
			Assert.True(Execute("namespace N1 { class Test1 { } namespace N2 { class Test2 { Test1 t; } } }", 1).Length == 0);
		}

		[Fact]
		public void ReturnsEmpty_When_HasDynamic()
		{
			Assert.True(Execute("class Test { dynamic Method() { return null; } }").Length == 0);
		}

		[Fact]
		public void IsSuccess_When_HasUsings()
		{
			string[] namespaces = Execute("using System; class Test { DateTime a; }");
			Assert.True(namespaces.Length == 1 && namespaces[0] == "System");
		}

		[Fact]
		public void IsSuccess_When_UsesFullyQualifiedName()
		{
			string[] namespaces = Execute("class Test { System.DateTime a; }");
			Assert.True(namespaces.Length == 1 && namespaces[0] == "System");
		}

		[Fact]
		public void IsSuccess_When_UsesMultipleNamespaces()
		{
			string[] namespaces = Execute("using System.Collections.Generic; class Test { System.DateTime a; List<int> list; }");
			Assert.True(namespaces.Length == 2 && namespaces.Contains("System") && namespaces.Contains("System.Collections.Generic"));
		}

		[Fact]
		public void SkipsUsing_When_SkipQualfiedNamesIsTrue_And_NodeIsInsideQualifiedName()
		{
			string[] namespaces = Execute("using System.Collections.Generic; class Test { System.DateTime a; List<int> list; }", skipQualifiedNames: true);
			Assert.True(namespaces.Length == 1 && namespaces[0] == "System.Collections.Generic");
		}


		[Fact]
		public void SkipsUsing_When_SkipQualfiedNamesIsTrue_And_NodeIsInsideCrefQualifiedName()
		{
			string[] namespaces = Execute(
@"using System.Collections.Generic; 

class Test 
{ 
	/// <inheritdoc cref=""List{T}""/> 
	List<int> list;
}",
skipQualifiedNames: true);

			Assert.True(namespaces.Length == 1 && namespaces[0] == "System.Collections.Generic");
		}

		[Fact]
		public void SkipsUsing_When_SkipQualfiedNamesIsTrue_And_NodeIsInsideAliasQualifiedName()
		{
			SyntaxTree tree = CSharpSyntaxTree.ParseText("using D = System.Collections; using System.Collections.Generic; class Test { D::IEnumerable a; List<int> list; }");
			Compilation.UpdateCompilation((CSharpSyntaxTree)tree);
			SemanticModel semanticModel = Compilation.CurrentCompilation.GetSemanticModel(tree);
			string[] namespaces = semanticModel.GetUsedNamespaces(tree.GetRoot(), Compilation, true).ToArray();

			Assert.True(namespaces.Length == 1 && namespaces[0] == "System.Collections.Generic");
		}

		[Fact]
		public void HandlesNamespaceOfAttribute()
		{
			string[] namespaces = Execute("using System; [Serializable]class Test { }");
			Assert.True(namespaces.Length == 1 && namespaces[0] == "System");
		}

		private string[] Execute(string src, int index = 0, bool skipQualifiedNames = false)
		{
			IMemberData member = GetClass(src, index);
			return member.SemanticModel.GetUsedNamespaces(member.Declaration, Compilation, skipQualifiedNames).ToArray();
		}
	}
}