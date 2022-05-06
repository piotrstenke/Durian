// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Extensions
{
	/// <summary>
	/// Contains various extension methods for the <see cref="SemanticModel"/> class.
	/// </summary>
	public static class SemanticModelExtensions
	{
		/// <summary>
		/// Looks for <see cref="AttributeSyntax"/> that corresponds to the <paramref name="attrSymbol"/> and is defined on the specified <paramref name="syntaxNode"/>.
		/// </summary>
		/// <param name="semanticModel">Parent <see cref="SemanticModel"/>.</param>
		/// <param name="syntaxNode"><see cref="MemberDeclarationSyntax"/> the attribute is declared on.</param>
		/// <param name="attrSymbol">Type of attribute to look for.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static AttributeSyntax? GetAttribute(
			this SemanticModel semanticModel,
			MemberDeclarationSyntax syntaxNode,
			INamedTypeSymbol attrSymbol,
			CancellationToken cancellationToken = default
		)
		{
			return GetAllAttributes_Internal(semanticModel, attrSymbol, () => syntaxNode.AttributeLists, cancellationToken).FirstOrDefault();
		}

		/// <summary>
		/// Looks for <see cref="AttributeSyntax"/> that corresponds to the <paramref name="attrSymbol"/> and is defined on the specified <paramref name="syntaxNode"/>.
		/// </summary>
		/// <param name="semanticModel">Parent <see cref="SemanticModel"/>.</param>
		/// <param name="syntaxNode"><see cref="TypeParameterSyntax"/> the attribute is declared on.</param>
		/// <param name="attrSymbol">Type of attribute to look for.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static AttributeSyntax? GetAttribute(
			this SemanticModel semanticModel,
			TypeParameterSyntax syntaxNode,
			INamedTypeSymbol attrSymbol,
			CancellationToken cancellationToken = default
		)
		{
			return GetAllAttributes_Internal(semanticModel, attrSymbol, () => syntaxNode.AttributeLists, cancellationToken).FirstOrDefault();
		}

		/// <summary>
		/// Looks for <see cref="AttributeSyntax"/> that corresponds to the <paramref name="attrSymbol"/> and is defined on the specified <paramref name="syntaxNode"/>.
		/// </summary>
		/// <param name="semanticModel">Parent <see cref="SemanticModel"/>.</param>
		/// <param name="syntaxNode"><see cref="ParameterSyntax"/> the attribute is declared on.</param>
		/// <param name="attrSymbol">Type of attribute to look for.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static AttributeSyntax? GetAttribute(
			this SemanticModel semanticModel,
			ParameterSyntax syntaxNode,
			INamedTypeSymbol attrSymbol,
			CancellationToken cancellationToken = default
		)
		{
			return GetAllAttributes_Internal(semanticModel, attrSymbol, () => syntaxNode.AttributeLists, cancellationToken).FirstOrDefault();
		}

		/// <summary>
		/// Looks for all <see cref="AttributeSyntax"/>ex that correspond to the <paramref name="attrSymbol"/> and are defined on the specified <paramref name="syntaxNode"/>.
		/// </summary>
		/// <param name="semanticModel">Parent <see cref="SemanticModel"/>.</param>
		/// <param name="syntaxNode"><see cref="MemberDeclarationSyntax"/> the attributes are declared on.</param>
		/// <param name="attrSymbol">Type of attributes to look for.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static IEnumerable<AttributeSyntax> GetAttributes(
			this SemanticModel semanticModel,
			MemberDeclarationSyntax syntaxNode,
			INamedTypeSymbol attrSymbol,
			CancellationToken cancellationToken = default
		)
		{
			return GetAllAttributes_Internal(semanticModel, attrSymbol, () => syntaxNode.AttributeLists, cancellationToken);
		}

		/// <summary>
		/// Looks for all <see cref="AttributeSyntax"/>ex that correspond to the <paramref name="attrSymbol"/> and are defined on the specified <paramref name="syntaxNode"/>.
		/// </summary>
		/// <param name="semanticModel">Parent <see cref="SemanticModel"/>.</param>
		/// <param name="syntaxNode"><see cref="TypeParameterSyntax"/> the attributes are declared on.</param>
		/// <param name="attrSymbol">Type of attributes to look for.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static IEnumerable<AttributeSyntax> GetAttributes(
			this SemanticModel semanticModel,
			TypeParameterSyntax syntaxNode,
			INamedTypeSymbol attrSymbol,
			CancellationToken cancellationToken = default
		)
		{
			return GetAllAttributes_Internal(semanticModel, attrSymbol, () => syntaxNode.AttributeLists, cancellationToken);
		}

		/// <summary>
		/// Looks for all <see cref="AttributeSyntax"/>es that correspond to the <paramref name="attrSymbol"/> and are defined on the specified <paramref name="syntaxNode"/>.
		/// </summary>
		/// <param name="semanticModel">Parent <see cref="SemanticModel"/>.</param>
		/// <param name="syntaxNode"><see cref="ParameterSyntax"/> the attributes are declared on.</param>
		/// <param name="attrSymbol">Type of attributes to look for.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static IEnumerable<AttributeSyntax> GetAttributes(
			this SemanticModel semanticModel,
			ParameterSyntax syntaxNode,
			INamedTypeSymbol attrSymbol,
			CancellationToken cancellationToken = default
		)
		{
			return GetAllAttributes_Internal(semanticModel, attrSymbol, () => syntaxNode.AttributeLists, cancellationToken);
		}

		/// <summary>
		/// Returns the <see cref="INamespaceSymbol"/> that directly contains the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="semanticModel">Parent <see cref="SemanticModel"/>.</param>
		/// <param name="node"><see cref="SyntaxNode"/> to get the parent namespace of.</param>
		/// <param name="compilationData"><see cref="ICompilationData"/> the specified <paramref name="node"/> is defined in.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static INamespaceSymbol GetContainingNamespace(
			this SemanticModel semanticModel,
			SyntaxNode node,
			ICompilationData compilationData,
			CancellationToken cancellationToken = default
		)
		{
			return GetContainingNamespace(semanticModel, node, compilationData.Compilation.Assembly.GlobalNamespace, cancellationToken);
		}

		/// <summary>
		/// Returns the <see cref="INamespaceSymbol"/> that directly contains the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="semanticModel">Parent <see cref="SemanticModel"/>.</param>
		/// <param name="node"><see cref="SyntaxNode"/> to get the parent namespace of.</param>
		/// <param name="compilation"><see cref="CSharpCompilation"/> the specified <paramref name="node"/> is defined in.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static INamespaceSymbol GetContainingNamespace(
			this SemanticModel semanticModel,
			SyntaxNode node,
			CSharpCompilation compilation,
			CancellationToken cancellationToken = default
		)
		{
			return GetContainingNamespace(semanticModel, node, compilation.Assembly.GlobalNamespace, cancellationToken);
		}

		/// <summary>
		/// Returns the <see cref="INamespaceSymbol"/> that directly contains the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="semanticModel">Parent <see cref="SemanticModel"/>.</param>
		/// <param name="node"><see cref="SyntaxNode"/> to get the parent namespace of.</param>
		/// <param name="assembly"><see cref="IAssemblySymbol"/> the specified <paramref name="node"/> is defined in.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static INamespaceSymbol GetContainingNamespace(
			this SemanticModel semanticModel,
			SyntaxNode node,
			IAssemblySymbol assembly,
			CancellationToken cancellationToken = default
		)
		{
			return GetContainingNamespace(semanticModel, node, assembly.GlobalNamespace, cancellationToken);
		}

		/// <summary>
		/// Returns the <see cref="INamespaceSymbol"/> that directly contains the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="semanticModel">Parent <see cref="SemanticModel"/>.</param>
		/// <param name="node"><see cref="SyntaxNode"/> to get the parent namespace of.</param>
		/// <param name="globalNamespace"><see cref="INamespaceSymbol"/> that represents the assembly's global namespace.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <exception cref="ArgumentException"><paramref name="globalNamespace"/> is not an actual global namespace.</exception>
		public static INamespaceSymbol GetContainingNamespace(
			this SemanticModel semanticModel,
			SyntaxNode node,
			INamespaceSymbol globalNamespace,
			CancellationToken cancellationToken = default
		)
		{
			if (!globalNamespace.IsGlobalNamespace)
			{
				throw new ArgumentException($"Namespace '{globalNamespace}' is not a global namespace!");
			}

			SyntaxNode? current = node;

			while ((current = current!.Parent) is not null)
			{
				if (current is NamespaceDeclarationSyntax decl && semanticModel.GetDeclaredSymbol(decl, cancellationToken) is INamespaceSymbol symbol)
				{
					return symbol;
				}
			}

			return globalNamespace;
		}

		/// <summary>
		/// Returns all the <see cref="INamespaceSymbol"/>s that contains the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="semanticModel">Target <see cref="SemanticModel"/>.</param>
		/// <param name="node"><see cref="SyntaxNode"/> to get the namespaces used by.</param>
		/// <param name="compilationData"><see cref="ICompilationData"/> the specified <paramref name="node"/> is defined in.</param>
		/// <param name="includeGlobal">Determines whether to return the global namespace as well.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static IEnumerable<INamespaceSymbol> GetContainingNamespaces(
			this SemanticModel semanticModel,
			SyntaxNode node,
			ICompilationData compilationData,
			bool includeGlobal = false,
			CancellationToken cancellationToken = default
		)
		{
			return GetContainingNamespaces(semanticModel, node, compilationData.Compilation.Assembly.GlobalNamespace, includeGlobal, cancellationToken);
		}

		/// <summary>
		/// Returns all the <see cref="INamespaceSymbol"/>s that contains the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="semanticModel">Target <see cref="SemanticModel"/>.</param>
		/// <param name="node"><see cref="SyntaxNode"/> to get the namespaces used by.</param>
		/// <param name="assembly"><see cref="IAssemblySymbol"/> the specified <paramref name="node"/> is defined in.</param>
		/// <param name="includeGlobal">Determines whether to return the global namespace as well.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static IEnumerable<INamespaceSymbol> GetContainingNamespaces(
			this SemanticModel semanticModel,
			SyntaxNode node,
			IAssemblySymbol assembly,
			bool includeGlobal = false,
			CancellationToken cancellationToken = default
		)
		{
			return GetContainingNamespaces(semanticModel, node, assembly.GlobalNamespace, includeGlobal, cancellationToken);
		}

		/// <summary>
		/// Returns all the <see cref="INamespaceSymbol"/>s that contains the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="semanticModel">Target <see cref="SemanticModel"/>.</param>
		/// <param name="node"><see cref="SyntaxNode"/> to get the namespaces used by.</param>
		/// <param name="compilation"><see cref="CSharpCompilation"/> the specified <paramref name="node"/> is defined in.</param>
		/// <param name="includeGlobal">Determines whether to return the global namespace as well.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>tion>
		public static IEnumerable<INamespaceSymbol> GetContainingNamespaces(
			this SemanticModel semanticModel,
			SyntaxNode node,
			CSharpCompilation compilation,
			bool includeGlobal = false,
			CancellationToken cancellationToken = default
		)
		{
			return GetContainingNamespaces(semanticModel, node, compilation.Assembly.GlobalNamespace, includeGlobal, cancellationToken);
		}

		/// <summary>
		/// Returns all the <see cref="INamespaceSymbol"/>s that contains the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="semanticModel">Target <see cref="SemanticModel"/>.</param>
		/// <param name="node"><see cref="SyntaxNode"/> to get the namespaces used by.</param>
		/// <param name="globalNamespace"><see cref="INamespaceSymbol"/> that represents the assembly's global namespace.</param>
		/// <param name="includeGlobal">Determines whether to return the global namespace as well.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <exception cref="ArgumentException"><paramref name="globalNamespace"/> is not an actual global namespace.</exception>
		public static IEnumerable<INamespaceSymbol> GetContainingNamespaces(
			this SemanticModel semanticModel,
			SyntaxNode node,
			INamespaceSymbol globalNamespace,
			bool includeGlobal = false,
			CancellationToken cancellationToken = default
		)
		{
			INamespaceSymbol n = GetContainingNamespace(semanticModel, node, globalNamespace, cancellationToken);

			if (n.IsGlobalNamespace)
			{
				if (includeGlobal)
				{
					return new INamespaceSymbol[] { n };
				}
				else
				{
					return Array.Empty<INamespaceSymbol>();
				}
			}

			return Yield().Reverse();

			IEnumerable<INamespaceSymbol> Yield()
			{
				INamespaceSymbol current = n;

				yield return current;

				if (includeGlobal)
				{
					while ((current = current!.ContainingNamespace) is not null)
					{
						yield return current;
					}
				}
				else
				{
					while ((current = current!.ContainingNamespace) is not null)
					{
						if (current.IsGlobalNamespace)
						{
							yield break;
						}

						yield return current;
					}
				}
			}
		}

		/// <summary>
		/// Returns root namespace of the <paramref name="node"/>.
		/// </summary>
		/// <param name="semanticModel">Target <see cref="SemanticModel"/>.</param>
		/// <param name="node"><see cref="SyntaxNode"/> to get the namespaces used by.</param>
		/// <param name="compilationData"><see cref="ICompilationData"/> the specified <paramref name="node"/> is defined in.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static INamespaceSymbol GetRootNamespace(
			this SemanticModel semanticModel,
			SyntaxNode node,
			ICompilationData compilationData,
			CancellationToken cancellationToken = default
		)
		{
			return GetRootNamespace(semanticModel, node, compilationData.Compilation.Assembly.GlobalNamespace, cancellationToken);
		}

		/// <summary>
		/// Returns root namespace of the <paramref name="node"/>.
		/// </summary>
		/// <param name="semanticModel">Target <see cref="SemanticModel"/>.</param>
		/// <param name="node"><see cref="SyntaxNode"/> to get the namespaces used by.</param>
		/// <param name="compilation"><see cref="CSharpCompilation"/> the specified <paramref name="node"/> is defined in.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static INamespaceSymbol GetRootNamespace(
			this SemanticModel semanticModel,
			SyntaxNode node,
			CSharpCompilation compilation,
			CancellationToken cancellationToken = default
		)
		{
			return GetRootNamespace(semanticModel, node, compilation.Assembly.GlobalNamespace, cancellationToken);
		}

		/// <summary>
		/// Returns root namespace of the <paramref name="node"/>.
		/// </summary>
		/// <param name="semanticModel">Target <see cref="SemanticModel"/>.</param>
		/// <param name="node"><see cref="SyntaxNode"/> to get the namespaces used by.</param>
		/// <param name="assembly"><see cref="IAssemblySymbol"/> the specified <paramref name="node"/> is defined in.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static INamespaceSymbol GetRootNamespace(
			this SemanticModel semanticModel,
			SyntaxNode node,
			IAssemblySymbol assembly,
			CancellationToken cancellationToken = default
		)
		{
			return GetRootNamespace(semanticModel, node, assembly.GlobalNamespace, cancellationToken);
		}

		/// <summary>
		/// Returns root namespace of the <paramref name="node"/>.
		/// </summary>
		/// <param name="semanticModel">Target <see cref="SemanticModel"/>.</param>
		/// <param name="node"><see cref="SyntaxNode"/> to get the namespaces used by.</param>
		/// <param name="globalNamespace"><see cref="INamespaceSymbol"/> that represents the assembly's global namespace.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <exception cref="ArgumentException"><paramref name="globalNamespace"/> is not an actual global namespace.</exception>
		public static INamespaceSymbol GetRootNamespace(
			this SemanticModel semanticModel,
			SyntaxNode node,
			INamespaceSymbol globalNamespace,
			CancellationToken cancellationToken = default
		)
		{
			INamespaceSymbol parentNamespace = GetContainingNamespace(semanticModel, node, globalNamespace, cancellationToken);

			if (parentNamespace.IsGlobalNamespace)
			{
				return parentNamespace;
			}

			return parentNamespace.GetContainingNamespaces(false).FirstOrDefault() ?? parentNamespace;
		}

		/// <summary>
		/// Returns all namespaces that are used by this <see cref="MemberDeclarationSyntax"/>.
		/// </summary>
		/// <param name="semanticModel">Target <see cref="SemanticModel"/>.</param>
		/// <param name="node"><see cref="SyntaxNode"/> to get the namespaces used by.</param>
		/// <param name="compilationData"><see cref="ICompilationData"/> the specified <paramref name="node"/> is defined in.</param>
		/// <param name="skipQualifiedNames">Determines whether to skip nodes that reside inside a qualified name.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static IEnumerable<string> GetUsedNamespaces(this SemanticModel semanticModel,
			SyntaxNode node,
			ICompilationData compilationData,
			bool skipQualifiedNames = false,
			CancellationToken cancellationToken = default
		)
		{
			return GetUsedNamespaces(semanticModel, node, compilationData.Compilation.Assembly.GlobalNamespace, skipQualifiedNames, cancellationToken);
		}

		/// <summary>
		/// Returns all namespaces that are used by this <see cref="MemberDeclarationSyntax"/>.
		/// </summary>
		/// <param name="semanticModel">Target <see cref="SemanticModel"/>.</param>
		/// <param name="node"><see cref="SyntaxNode"/> to get the namespaces used by.</param>
		/// <param name="compilation"><see cref="CSharpCompilation"/> the specified <paramref name="node"/> is defined in.</param>
		/// <param name="skipQualifiedNames">Determines whether to skip nodes that reside inside a qualified name.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static IEnumerable<string> GetUsedNamespaces(
			this SemanticModel semanticModel,
			SyntaxNode node,
			CSharpCompilation compilation,
			bool skipQualifiedNames = false,
			CancellationToken cancellationToken = default
		)
		{
			return GetUsedNamespaces(semanticModel, node, compilation.Assembly.GlobalNamespace, skipQualifiedNames, cancellationToken);
		}

		/// <summary>
		/// Returns all namespaces that are used by this <see cref="MemberDeclarationSyntax"/>.
		/// </summary>
		/// <param name="semanticModel">Target <see cref="SemanticModel"/>.</param>
		/// <param name="node"><see cref="SyntaxNode"/> to get the namespaces used by.</param>
		/// <param name="assembly"><see cref="IAssemblySymbol"/> the specified <paramref name="node"/> is defined in.</param>
		/// <param name="skipQualifiedNames">Determines whether to skip nodes that reside inside a qualified name.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static IEnumerable<string> GetUsedNamespaces(
			this SemanticModel semanticModel,
			SyntaxNode node,
			IAssemblySymbol assembly,
			bool skipQualifiedNames = false,
			CancellationToken cancellationToken = default
		)
		{
			return GetUsedNamespaces(semanticModel, node, assembly.GlobalNamespace, skipQualifiedNames, cancellationToken);
		}

		/// <summary>
		/// Returns all namespaces that are used by this <see cref="MemberDeclarationSyntax"/>.
		/// </summary>
		/// <param name="semanticModel">Target <see cref="SemanticModel"/>.</param>
		/// <param name="node"><see cref="SyntaxNode"/> to get the namespaces used by.</param>
		/// <param name="globalNamespace"><see cref="INamespaceSymbol"/> that represents the assembly's global namespace.</param>
		/// <param name="skipQualifiedNames">Determines whether to skip nodes that reside inside a qualified name.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <exception cref="ArgumentException"><paramref name="globalNamespace"/> is not an actual global namespace.</exception>
		public static IEnumerable<string> GetUsedNamespaces(
			this SemanticModel semanticModel,
			SyntaxNode node,
			INamespaceSymbol globalNamespace,
			bool skipQualifiedNames = false,
			CancellationToken cancellationToken = default
		)
		{
			return GetUsedNamespacesWithoutDistinct(semanticModel, node, globalNamespace, skipQualifiedNames, cancellationToken).Distinct();
		}

		/// <summary>
		/// Returns all namespaces that are used by this <see cref="MemberDeclarationSyntax"/>. Returned namespaces can repeat multiple times, according to the input <paramref name="node"/>.
		/// </summary>
		/// <param name="semanticModel">Target <see cref="SemanticModel"/>.</param>
		/// <param name="node"><see cref="SyntaxNode"/> to get the namespaces used by.</param>
		/// <param name="compilationData"><see cref="ICompilationData"/> the specified <paramref name="node"/> is defined in.</param>
		/// <param name="skipQualifiedNames">Determines whether to skip nodes that reside inside a qualified name.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static IEnumerable<string> GetUsedNamespacesWithoutDistinct(
			this SemanticModel semanticModel,
			SyntaxNode node,
			ICompilationData compilationData,
			bool skipQualifiedNames = false,
			CancellationToken cancellationToken = default
		)
		{
			return GetUsedNamespacesWithoutDistinct(semanticModel, node, compilationData.Compilation.Assembly.GlobalNamespace, skipQualifiedNames, cancellationToken);
		}

		/// <summary>
		/// Returns all namespaces that are used by this <see cref="MemberDeclarationSyntax"/>. Returned namespaces can repeat multiple times, according to the input <paramref name="node"/>.
		/// </summary>
		/// <param name="semanticModel">Target <see cref="SemanticModel"/>.</param>
		/// <param name="node"><see cref="SyntaxNode"/> to get the namespaces used by.</param>
		/// <param name="compilation"><see cref="CSharpCompilation"/> the specified <paramref name="node"/> is defined in.</param>
		/// <param name="skipQualifiedNames">Determines whether to skip nodes that reside inside a qualified name.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static IEnumerable<string> GetUsedNamespacesWithoutDistinct(this SemanticModel semanticModel,
			SyntaxNode node,
			CSharpCompilation compilation,
			bool skipQualifiedNames = false,
			CancellationToken cancellationToken = default
		)
		{
			return GetUsedNamespacesWithoutDistinct(semanticModel, node, compilation.Assembly.GlobalNamespace, skipQualifiedNames, cancellationToken);
		}

		/// <summary>
		/// Returns all namespaces that are used by this <see cref="MemberDeclarationSyntax"/>. Returned namespaces can repeat multiple times, according to the input <paramref name="node"/>.
		/// </summary>
		/// <param name="semanticModel">Target <see cref="SemanticModel"/>.</param>
		/// <param name="node"><see cref="SyntaxNode"/> to get the namespaces used by.</param>
		/// <param name="assembly"><see cref="IAssemblySymbol"/> the specified <paramref name="node"/> is defined in.</param>
		/// <param name="skipQualifiedNames">Determines whether to skip nodes that reside inside a qualified name.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static IEnumerable<string> GetUsedNamespacesWithoutDistinct(
			this SemanticModel semanticModel,
			SyntaxNode node,
			IAssemblySymbol assembly,
			bool skipQualifiedNames = false,
			CancellationToken cancellationToken = default
		)
		{
			return GetUsedNamespacesWithoutDistinct(semanticModel, node, assembly.GlobalNamespace, skipQualifiedNames, cancellationToken);
		}

		/// <summary>
		/// Returns all namespaces that are used by this <see cref="MemberDeclarationSyntax"/>. Returned namespaces can repeat multiple times, according to the input <paramref name="node"/>.
		/// </summary>
		/// <param name="semanticModel">Target <see cref="SemanticModel"/>.</param>
		/// <param name="node"><see cref="SyntaxNode"/> to get the namespaces used by.</param>
		/// <param name="globalNamespace"><see cref="INamespaceSymbol"/> that represents the assembly's global namespace.</param>
		/// <param name="skipQualifiedNames">Determines whether to skip nodes that reside inside a qualified name.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <exception cref="ArgumentException"><paramref name="globalNamespace"/> is not an actual global namespace.</exception>
		public static IEnumerable<string> GetUsedNamespacesWithoutDistinct(
			this SemanticModel semanticModel,
			SyntaxNode node,
			INamespaceSymbol globalNamespace,
			bool skipQualifiedNames = false,
			CancellationToken cancellationToken = default
		)
		{
			INamespaceSymbol rootNamespace = GetRootNamespace(semanticModel, node, globalNamespace, cancellationToken)!;

			return Yield();

			IEnumerable<string> Yield()
			{
				IEnumerable<SyntaxNode> nodes = node.DescendantNodes(descendIntoTrivia: true).Where(n => n is IdentifierNameSyntax or GenericNameSyntax);

				if (skipQualifiedNames)
				{
					nodes = nodes.Where(n => n.Parent is not QualifiedNameSyntax and not QualifiedCrefSyntax and not AliasQualifiedNameSyntax);
				}

				foreach (SyntaxNode n in nodes)
				{
					SymbolInfo info = semanticModel.GetSymbolInfo(n, cancellationToken);

					if (info.Symbol is null)
					{
						continue;
					}

					INamespaceSymbol? parentNamespace = info.Symbol?.ContainingNamespace;

					if (parentNamespace is null || parentNamespace.IsGlobalNamespace)
					{
						continue;
					}

					if (info.Symbol is INamedTypeSymbol t)
					{
						if (t.IsPredefined())
						{
							continue;
						}
					}
					else if (info.Symbol is not IMethodSymbol || n.Parent is not AttributeSyntax)
					{
						continue;
					}

					if (!rootNamespace.ContainsSymbol(parentNamespace))
					{
						yield return parentNamespace.ToString();
					}
				}
			}
		}

		private static IEnumerable<AttributeSyntax> GetAllAttributes_Internal(
			SemanticModel semanticModel,
			INamedTypeSymbol attrSymbol,
			Func<SyntaxList<AttributeListSyntax>> attrProvider,
			CancellationToken cancellationToken
		)
		{
			foreach (AttributeListSyntax attrList in attrProvider())
			{
				foreach (AttributeSyntax attr in attrList.Attributes)
				{
					if (semanticModel.GetSymbolInfo(attr, cancellationToken).Symbol is not IMethodSymbol attrCtor)
					{
						continue;
					}

					INamedTypeSymbol attrType = attrCtor.ContainingType;

					if (SymbolEqualityComparer.Default.Equals(attrType, attrSymbol))
					{
						yield return attr;
					}
				}
			}
		}
	}
}
