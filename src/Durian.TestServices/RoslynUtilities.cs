// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.TestServices
{
	/// <summary>
	/// Contains various utility methods that help with unit testing Roslyn-based projects.
	/// </summary>
	public static class RoslynUtilities
	{
		/// <summary>
		/// Name applied to a created <see cref="Compilation"/> when no other name is specified.
		/// </summary>
		public static string DefaultCompilationName => "TestCompilation";

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> that contains <see cref="MetadataReference"/>s of all the essential .NET and Durian assemblies.
		/// </summary>
		public static CSharpCompilation CreateBaseCompilation()
		{
			return CreateCompilationWithReferences(sources: null, GetBaseReferences());
		}

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> that contains a <see cref="CSharpSyntaxTree"/> created from the specified <paramref name="source"/>
		/// and <see cref="MetadataReference"/>s to the assemblies that contain the specified <paramref name="types"/>.
		/// </summary>
		/// <param name="source">A <see cref="string"/> that will be parsed as a <see cref="CSharpSyntaxTree"/> and added to the output <see cref="CSharpCompilation"/>.</param>
		/// <param name="types">A collection of <see cref="Type"/>s to get the assemblies to reference to.</param>
		public static CSharpCompilation CreateCompilation(string? source, params Type[]? types)
		{
			return CreateCompilationWithReferences(source is not null ? CSharpSyntaxTree.ParseText(source) as CSharpSyntaxTree : null, GetReferences(types).ToArray());
		}

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> that contains a <see cref="CSharpSyntaxTree"/> created from the specified <paramref name="sources"/>
		/// and <see cref="MetadataReference"/>s to the assemblies that contain the specified <paramref name="types"/>.
		/// </summary>
		/// <param name="sources">A collection of <see cref="string"/>s that will be parsed as <see cref="CSharpSyntaxTree"/>s and added to the output <see cref="CSharpCompilation"/>.</param>
		/// <param name="types">A collection of <see cref="Type"/>s to get the assemblies to reference to.</param>
		public static CSharpCompilation CreateCompilation(IEnumerable<string>? sources, params Type[]? types)
		{
			return CreateCompilationWithReferences(GetSyntaxTrees(sources).ToArray(), GetReferences(types).ToArray());
		}

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> that contains the specified <see cref="CSharpSyntaxTree"/>
		/// and <see cref="MetadataReference"/>s to the assemblies that contain the specified <paramref name="types"/>.
		/// </summary>
		/// <param name="tree">A <see cref="CSharpSyntaxTree"/> that will be added to the output <see cref="CSharpCompilation"/>.</param>
		/// <param name="types">A collection of <see cref="Type"/>s to get the assemblies to reference to.</param>
		public static CSharpCompilation CreateCompilation(CSharpSyntaxTree? tree, params Type[]? types)
		{
			return CreateCompilationWithReferences(tree, GetReferences(types).ToArray());
		}

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> that contains the specified <see cref="CSharpSyntaxTree"/>s
		/// and <see cref="MetadataReference"/>s to the assemblies that contain the specified <paramref name="types"/>.
		/// </summary>
		/// <param name="trees">A collection of <see cref="CSharpSyntaxTree"/>s that will be added to the output <see cref="CSharpCompilation"/>.</param>
		/// <param name="types">A collection of <see cref="Type"/>s to get the assemblies to reference to.</param>
		public static CSharpCompilation CreateCompilation(IEnumerable<CSharpSyntaxTree>? trees, params Type[] types)
		{
			return CreateCompilationWithReferences(trees, GetReferences(types).ToArray());
		}

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> that contains a <see cref="CSharpSyntaxTree"/> created from the specified <paramref name="source"/>
		/// and <see cref="MetadataReference"/>s to the specified <paramref name="assemblies"/>.
		/// </summary>
		/// <param name="source">A <see cref="string"/> that will be parsed as a <see cref="CSharpSyntaxTree"/> and added to the output <see cref="CSharpCompilation"/>.</param>
		/// <param name="assemblies">An array of <see cref="Assembly"/> instances to be referenced by the output <see cref="CSharpCompilation"/>.</param>
		public static CSharpCompilation CreateCompilationWithAssemblies(string? source, params Assembly[]? assemblies)
		{
			return CreateCompilationWithReferences(source is not null ? CSharpSyntaxTree.ParseText(source) as CSharpSyntaxTree : null, GetReferences(assemblies).ToArray());
		}

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> that contains <see cref="CSharpSyntaxTree"/>s created from the specified <paramref name="sources"/>
		/// and <see cref="MetadataReference"/>s to the specified <paramref name="assemblies"/>.
		/// </summary>
		/// <param name="sources">A collection of <see cref="string"/>s that will be parsed as <see cref="CSharpSyntaxTree"/>s and added to the output <see cref="CSharpCompilation"/>.</param>
		/// <param name="assemblies">An array of <see cref="Assembly"/> instances to be referenced by the output <see cref="CSharpCompilation"/>.</param>
		public static CSharpCompilation CreateCompilationWithAssemblies(IEnumerable<string>? sources, params Assembly[]? assemblies)
		{
			return CreateCompilationWithReferences(GetSyntaxTrees(sources).ToArray(), GetReferences(assemblies).ToArray());
		}

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> that contains the specified <see cref="CSharpSyntaxTree"/>
		/// and <see cref="MetadataReference"/>s to the specified <paramref name="assemblies"/>.
		/// </summary>
		/// <param name="tree">A <see cref="CSharpSyntaxTree"/> that will be added to the output <see cref="CSharpCompilation"/>.</param>
		/// <param name="assemblies">An array of <see cref="Assembly"/> instances to be referenced by the output <see cref="CSharpCompilation"/>.</param>
		public static CSharpCompilation CreateCompilationWithAssemblies(CSharpSyntaxTree? tree, params Assembly[]? assemblies)
		{
			return CreateCompilationWithReferences(tree, GetReferences(assemblies).ToArray());
		}

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> that contains the specified <see cref="CSharpSyntaxTree"/>s
		/// and <see cref="MetadataReference"/>s to the specified <paramref name="assemblies"/>.
		/// </summary>
		/// <param name="trees">A collection of <see cref="CSharpSyntaxTree"/>s that will be added to the output <see cref="CSharpCompilation"/>.</param>
		/// <param name="assemblies">An array of <see cref="Assembly"/> instances to be referenced by the output <see cref="CSharpCompilation"/>.</param>
		public static CSharpCompilation CreateCompilationWithAssemblies(IEnumerable<CSharpSyntaxTree>? trees, params Assembly[]? assemblies)
		{
			return CreateCompilationWithReferences(trees, GetReferences(assemblies).ToArray());
		}

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> that contains a <see cref="CSharpSyntaxTree"/> created from the specified <paramref name="source"/>
		/// and all the given <see cref="MetadataReference"/>s.
		/// </summary>
		/// <param name="source">A <see cref="string"/> that will be parsed as a <see cref="CSharpSyntaxTree"/> and added to the output <see cref="CSharpCompilation"/>.</param>
		/// <param name="references">An array of <see cref="MetadataReference"/> to be added to the output <see cref="CSharpCompilation"/>.</param>
		public static CSharpCompilation CreateCompilationWithReferences(string? source, params MetadataReference[]? references)
		{
			return CreateCompilationWithReferences(source is not null ? CSharpSyntaxTree.ParseText(source) as CSharpSyntaxTree : null, references);
		}

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> that contains <see cref="CSharpSyntaxTree"/>s created from the specified <paramref name="sources"/>
		/// and all the given <see cref="MetadataReference"/>s.
		/// </summary>
		/// <param name="sources">A collection of <see cref="string"/>s that will be parsed as <see cref="CSharpSyntaxTree"/>s and added to the output <see cref="CSharpCompilation"/>.</param>
		/// <param name="references">An array of <see cref="MetadataReference"/> to be added to the output <see cref="CSharpCompilation"/>.</param>
		public static CSharpCompilation CreateCompilationWithReferences(IEnumerable<string>? sources, params MetadataReference[]? references)
		{
			return CreateCompilationWithReferences(GetSyntaxTrees(sources).ToArray(), references);
		}

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> that contains the specified <see cref="CSharpSyntaxTree"/> and all the given <see cref="MetadataReference"/>s.
		/// </summary>
		/// <param name="tree">A <see cref="CSharpSyntaxTree"/> that will be added to the output <see cref="CSharpCompilation"/>.</param>
		/// <param name="references">An array of <see cref="MetadataReference"/> to be added to the output <see cref="CSharpCompilation"/>.</param>
		public static CSharpCompilation CreateCompilationWithReferences(CSharpSyntaxTree? tree, params MetadataReference[]? references)
		{
			return CreateCompilationWithReferences(tree is not null ? new CSharpSyntaxTree[] { tree } : null, references);
		}

		/// <summary>
		/// Creates a <see cref="CSharpCompilation"/> that contains the specified <see cref="CSharpSyntaxTree"/> and all the given <see cref="MetadataReference"/>s.
		/// </summary>
		/// <param name="trees">A collection of <see cref="CSharpSyntaxTree"/>s that will be added to the output <see cref="CSharpCompilation"/>.</param>
		/// <param name="references">An array of <see cref="MetadataReference"/> to be added to the output <see cref="CSharpCompilation"/>.</param>
		public static CSharpCompilation CreateCompilationWithReferences(IEnumerable<CSharpSyntaxTree>? trees, params MetadataReference[]? references)
		{
			return CSharpCompilation.Create(
				assemblyName: DefaultCompilationName,
				syntaxTrees: trees,
				references: references,
				options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
			);
		}

		/// <summary>
		/// Creates a new, non-defaulted <see cref="GeneratorExecutionContext"/> using a <see cref="CSharpGeneratorDriver"/>.
		/// </summary>
		public static GeneratorExecutionContext CreateExecutionContext()
		{
			return CreateExecutionContext(CreateBaseCompilation());
		}

		/// <inheritdoc cref="CreateExecutionContext(Compilation, GeneratorInitialize)"/>
		public static GeneratorExecutionContext CreateExecutionContext(Compilation? compilation)
		{
			SourceGeneratorProxy proxy = new();
			CSharpGeneratorDriver driver = CSharpGeneratorDriver.Create(proxy);
			_ = driver.RunGenerators(compilation ?? CreateBaseCompilation());
			return proxy.ExecutionContext;
		}

		/// <summary>
		/// Creates a new, non-defaulted <see cref="GeneratorExecutionContext"/> using a <see cref="CSharpGeneratorDriver"/>.
		/// </summary>
		/// <param name="compilation">A <see cref="Compilation"/> to be assigned to the newly-created <see cref="GeneratorExecutionContext"/>.</param>
		/// <param name="onInitialize">Action to be performed instead of the proper <see cref="ISourceGenerator.Initialize(GeneratorInitializationContext)"/> method.</param>
		/// <remarks>
		/// This method accepts <see cref="Compilation"/> instead of <see cref="CSharpCompilation"/>
		/// to allow the user to intentionally pass invalid <see cref="Compilation"/> to the <see cref="ISourceGenerator.Execute(GeneratorExecutionContext)"/>
		/// to perform proper unit tests regarding validation.
		/// </remarks>
		public static GeneratorExecutionContext CreateExecutionContext(Compilation? compilation, GeneratorInitialize? onInitialize)
		{
			SourceGeneratorProxy proxy = new();

			if (onInitialize is not null)
			{
				proxy.OnInitialize += onInitialize;
			}

			CSharpGeneratorDriver driver = CSharpGeneratorDriver.Create(proxy);
			_ = driver.RunGenerators(compilation ?? CreateBaseCompilation());
			return proxy.ExecutionContext;
		}

		/// <summary>
		/// Creates a new, non-defaulted <see cref="GeneratorInitializationContext"/> using <c>System.Reflection</c>.
		/// </summary>
		public static GeneratorInitializationContext CreateInitializationContext()
		{
			return (GeneratorInitializationContext)Activator.CreateInstance(
				type: typeof(GeneratorInitializationContext),
				bindingAttr: BindingFlags.Instance | BindingFlags.NonPublic,
				binder: null,
				args: new object[] { default(CancellationToken) },
				culture: CultureInfo.InvariantCulture
			)!;
		}

		/// <summary>
		/// Returns an array of <see cref="MetadataReference"/>s of all essential .NET and Durian assemblies. 
		/// </summary>
		public static MetadataReference[] GetBaseReferences()
		{
			string directory = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

			return new[]
			{
				MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
				MetadataReference.CreateFromFile(typeof(File).Assembly.Location),
				MetadataReference.CreateFromFile(typeof(BigInteger).Assembly.Location),
				MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
				MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location),
				MetadataReference.CreateFromFile(Path.Combine(directory, "System.Runtime.dll")),
				MetadataReference.CreateFromFile(Path.Combine(directory, "netstandard.dll")),
				MetadataReference.CreateFromFile(typeof(Generator.DurianGeneratedAttribute).Assembly.Location),
			};
		}

		/// <summary>
		/// Returns a collection of <see cref="MetadataReference"/>s created from the provided <paramref name="types"/>.
		/// </summary>
		/// <param name="types">A collection of <see cref="Type"/>s to get the assemblies to reference to.</param>
		/// <param name="includeBase">Determines whether to automatically include the most essential .NET assemblies.</param>
		public static IEnumerable<MetadataReference> GetReferences(IEnumerable<Type>? types, bool includeBase = true)
		{
			if (includeBase)
			{
				foreach (MetadataReference m in GetBaseReferences())
				{
					yield return m;
				}
			}

			if (types is null)
			{
				yield break;
			}

			foreach (Type type in types)
			{
				if (type is null)
				{
					continue;
				}

				yield return MetadataReference.CreateFromFile(type.Assembly.Location);
			}
		}

		/// <summary>
		/// Returns a collection of <see cref="MetadataReference"/>s created from the provided <paramref name="assemblies"/>.
		/// </summary>
		/// <param name="assemblies">A collection of <see cref="Assembly"/> instances to create the <see cref="MetadataReference"/>s from.</param>
		/// <param name="includeBase">Determines whether to automatically include the most essential .NET assemblies.</param>
		public static IEnumerable<MetadataReference> GetReferences(IEnumerable<Assembly>? assemblies, bool includeBase = true)
		{
			if (includeBase)
			{
				foreach (MetadataReference m in GetBaseReferences())
				{
					yield return m;
				}
			}

			if (assemblies is null)
			{
				yield break;
			}

			foreach (Assembly assembly in assemblies)
			{
				if (assembly is null)
				{
					continue;
				}

				yield return MetadataReference.CreateFromFile(assembly.Location);
			}
		}

		/// <summary>
		/// Returns a collection of <see cref="CSharpSyntaxTree"/>s created from the specified <paramref name="sources"/>.
		/// </summary>
		/// <param name="sources">A collection of <see cref="string"/>s that represent the sources to parse and create the <see cref="SyntaxTree"/>s from.</param>
		public static IEnumerable<CSharpSyntaxTree> GetSyntaxTrees(IEnumerable<string>? sources)
		{
			if (sources is null)
			{
				yield break;
			}
			else
			{
				foreach (string s in sources)
				{
					if (s is null)
					{
						continue;
					}

					SyntaxTree tree = CSharpSyntaxTree.ParseText(s, encoding: Encoding.UTF8);

					if (tree is CSharpSyntaxTree t)
					{
						yield return t;
					}
				}
			}
		}

		/// <summary>
		/// Parses a <see cref="CSharpSyntaxTree"/> from the specified <paramref name="source"/> and returns the first <see cref="CSharpSyntaxNode"/>  of type <typeparamref name="TNode"/> in that tree.
		/// </summary>
		/// <param name="source">A <see cref="string"/> to be parsed and converted to a <see cref="CSharpSyntaxTree"/>.</param>
		/// <typeparam name="TNode">Type of <see cref="CSharpSyntaxNode"/> to find and return.</typeparam>
		/// <returns>
		/// The first <see cref="CSharpSyntaxNode"/> of type <typeparamref name="TNode"/> found in the parsed <see cref="CSharpSyntaxTree"/> -or-
		/// <see langword="null"/> if no such <see cref="CSharpSyntaxNode"/> exists.
		/// </returns>
		public static TNode? ParseNode<TNode>(string? source) where TNode : CSharpSyntaxNode
		{
			return ParseNode<TNode>(source, 0);
		}

		/// <summary>
		/// Parses a <see cref="CSharpSyntaxTree"/> from the specified <paramref name="source"/> and returns the <see cref="CSharpSyntaxNode"/> of type <typeparamref name="TNode"/> at the specified index in that tree.
		/// </summary>
		/// <param name="source">A <see cref="string"/> to be parsed and converted to a <see cref="CSharpSyntaxTree"/>.</param>
		/// <param name="index">Index at which the <see cref="CSharpSyntaxNode"/> should be returned. Can be thought of as a number of <see cref="CSharpSyntaxNode"/> of type <typeparamref name="TNode"/> to skip before returning.</param>
		/// <typeparam name="TNode">Type of <see cref="CSharpSyntaxNode"/> to find and return.</typeparam>
		/// <returns>
		/// The <see cref="CSharpSyntaxNode"/> of type <typeparamref name="TNode"/> found at the specified index in the parsed <see cref="CSharpSyntaxTree"/> -or-
		/// <see langword="null"/> if no such <see cref="CSharpSyntaxNode"/> exists.
		/// </returns>
		public static TNode? ParseNode<TNode>(string? source, int index) where TNode : CSharpSyntaxNode
		{
			if (source is null)
			{
				return null;
			}

			return ParseNode<TNode>(CSharpSyntaxTree.ParseText(source, encoding: Encoding.UTF8) as CSharpSyntaxTree, index);
		}

		/// <summary>
		/// Returns the first <see cref="CSharpSyntaxNode"/> of type <typeparamref name="TNode"/> in the specified <paramref name="tree"/>.
		/// </summary>
		/// <param name="tree"><see cref="CSharpSyntaxTree"/> to find the <see cref="CSharpSyntaxNode"/> of type <typeparamref name="TNode"/> in.</param>
		/// <typeparam name="TNode">Type of <see cref="CSharpSyntaxNode"/> to find and return.</typeparam>
		/// <returns>
		/// The <see cref="CSharpSyntaxNode"/> of type <typeparamref name="TNode"/> found at the specified index in the parsed <see cref="CSharpSyntaxTree"/> -or-
		/// <see langword="null"/> if no such <see cref="CSharpSyntaxNode"/> exists.
		/// </returns>
		public static TNode? ParseNode<TNode>(CSharpSyntaxTree? tree) where TNode : CSharpSyntaxNode
		{
			return ParseNode<TNode>(tree, 0);
		}

		/// <summary>
		/// Returns the <see cref="CSharpSyntaxNode"/> of type <typeparamref name="TNode"/> at the specified index in the specified <paramref name="syntaxTree"/>.
		/// </summary>
		/// <param name="syntaxTree"><see cref="CSharpSyntaxTree"/> to find the <see cref="CSharpSyntaxNode"/> of type <typeparamref name="TNode"/> in.</param>
		/// <param name="index">Index at which the <see cref="CSharpSyntaxNode"/> should be returned. Can be thought of as a number of <see cref="CSharpSyntaxNode"/> of type <typeparamref name="TNode"/> to skip before returning.</param>
		/// <typeparam name="TNode">Type of <see cref="CSharpSyntaxNode"/> to find and return.</typeparam>
		/// <returns>
		/// The <see cref="CSharpSyntaxNode"/> of type <typeparamref name="TNode"/> found at the specified index in the specified <paramref name="syntaxTree"/> -or-
		/// <see langword="null"/> if no such <see cref="CSharpSyntaxNode"/> exists.
		/// </returns>
		public static TNode? ParseNode<TNode>(CSharpSyntaxTree? syntaxTree, int index) where TNode : CSharpSyntaxNode
		{
			if (syntaxTree is null)
			{
				return null;
			}

			int count = 0;

			foreach (SyntaxNode node in syntaxTree.GetCompilationUnitRoot().DescendantNodes())
			{
				if (node is TNode n)
				{
					if (count == index)
					{
						return n;
					}

					count++;
				}
			}

			return null;
		}
	}
}
