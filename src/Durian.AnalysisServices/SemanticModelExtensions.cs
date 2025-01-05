using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Durian.Analysis.Data;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis;

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
	/// Returns the base constructor of the specified <paramref name="ctor"/>.
	/// </summary>
	/// <param name="semanticModel">Parent <see cref="SemanticModel"/>.</param>
	/// <param name="ctor">Constructor to get the base contructor of.</param>
	public static IMethodSymbol? GetBaseConstructor(this SemanticModel semanticModel, IMethodSymbol ctor)
	{
		if (ctor.MethodKind != MethodKind.Constructor || ctor.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is not ConstructorDeclarationSyntax decl)
		{
			return null;
		}

		return semanticModel.GetBaseConstructor(decl);
	}

	/// <summary>
	/// Returns the base constructor of the specified <paramref name="ctor"/>.
	/// </summary>
	/// <param name="semanticModel">Parent <see cref="SemanticModel"/>.</param>
	/// <param name="ctor">Constructor to get the base contructor of.</param>
	/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
	public static IMethodSymbol? GetBaseConstructor(this SemanticModel semanticModel, ConstructorDeclarationSyntax ctor, CancellationToken cancellationToken = default)
	{
		if (ctor.Initializer is null)
		{
			if (ctor.Parent is null || semanticModel.GetDeclaredSymbol(ctor.Parent, cancellationToken) is not INamedTypeSymbol type)
			{
				return null;
			}

			return GetVisibleDefaultConstructor(type);
		}

		return semanticModel.GetSymbolInfo(ctor.Initializer, cancellationToken).Symbol as IMethodSymbol;
	}

	/// <summary>
	/// Returns the base constructor of the specified <paramref name="ctor"/>.
	/// </summary>
	/// <param name="semanticModel">Parent <see cref="SemanticModel"/>.</param>
	/// <param name="ctor">Constructor to get the base contructor of.</param>
	/// <param name="parentType">Parent type of the <paramref name="ctor"/>.</param>
	/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
	public static IMethodSymbol? GetBaseConstructor(this SemanticModel semanticModel, ConstructorDeclarationSyntax ctor, INamedTypeSymbol parentType, CancellationToken cancellationToken = default)
	{
		if (ctor.Initializer is null)
		{
			return GetVisibleDefaultConstructor(parentType);
		}

		return semanticModel.GetSymbolInfo(ctor.Initializer, cancellationToken).Symbol as IMethodSymbol;
	}

	/// <summary>
	/// Returns all the base constructor of the specified <paramref name="ctor"/>.
	/// </summary>
	/// <param name="semanticModel">Parent <see cref="SemanticModel"/>.</param>
	/// <param name="ctor">Constructor to get the base contructors of.</param>
	/// <param name="includeSelf">Determines whether to include the <paramref name="ctor"/> in the returned collection.</param>
	/// <param name="order">Specifies ordering of the returned members.</param>
	public static IReturnOrderEnumerable<IMethodSymbol> GetBaseConstructors(this SemanticModel semanticModel, IMethodSymbol ctor, bool includeSelf = false, ReturnOrder order = ReturnOrder.ChildToParent)
	{
		return Yield().OrderBy(order, ReturnOrder.ParentToChild);

		IEnumerable<IMethodSymbol> Yield()
		{
			if (includeSelf)
			{
				yield return ctor;
			}

			IMethodSymbol current = ctor;

			while (semanticModel.GetBaseConstructor(current) is IMethodSymbol c)
			{
				yield return c;
				current = c;
			}
		}
	}

	/// <summary>
	/// Returns all the base constructor of the specified <paramref name="ctor"/>.
	/// </summary>
	/// <param name="semanticModel">Parent <see cref="SemanticModel"/>.</param>
	/// <param name="ctor">Constructor to get the base contructors of.</param>
	/// <param name="includeSelf">Determines whether to include the <paramref name="ctor"/> in the returned collection.</param>
	/// <param name="order">Specifies ordering of the returned members.</param>
	public static IReturnOrderEnumerable<IMethodSymbol> GetBaseConstructors(this SemanticModel semanticModel, ConstructorDeclarationSyntax ctor, bool includeSelf = false, ReturnOrder order = ReturnOrder.ChildToParent)
	{
		return Yield().OrderBy(order, ReturnOrder.ParentToChild);

		IEnumerable<IMethodSymbol> Yield()
		{
			IMethodSymbol? current;

			if (includeSelf)
			{
				if (semanticModel.GetDeclaredSymbol(ctor) is IMethodSymbol symbol)
				{
					yield return symbol;
				}
				else
				{
					yield break;
				}

				current = GetFirstCtor(symbol.ContainingType);
			}
			else
			{
				current = GetFirstCtor(null);
			}

			if (current is null)
			{
				yield break;
			}

			yield return current;

			while (semanticModel.GetBaseConstructor(current) is IMethodSymbol c)
			{
				yield return c;
				current = c;
			}

			IMethodSymbol? GetFirstCtor(INamedTypeSymbol? containingType)
			{
				if (containingType is null)
				{
					return semanticModel.GetBaseConstructor(ctor);
				}

				return semanticModel.GetBaseConstructor(ctor, containingType);
			}
		}
	}

	/// <summary>
	/// Returns <see cref="ISymbol"/>s of all varialbes captured by the specified <paramref name="method"/> representing an anonymous or local function.
	/// </summary>
	/// <param name="semanticModel">Parent <see cref="SemanticModel"/>.</param>
	/// <param name="method"><see cref="IMethodSymbol"/> to get the captured variables of.</param>
	public static IEnumerable<ISymbol> GetCapturedVariables(this SemanticModel semanticModel, IMethodSymbol method)
	{
		if (method.MethodKind is not
			MethodKind.LambdaMethod and not
			MethodKind.AnonymousFunction and not
			MethodKind.LocalFunction)
		{
			return Array.Empty<ISymbol>();
		}

		SyntaxNode? node = method.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();

		return node switch
		{
			AnonymousFunctionExpressionSyntax lambda => semanticModel.GetCapturedVariables(lambda),
			LocalFunctionStatementSyntax local => semanticModel.GetCapturedVariables(local),
			_ => Array.Empty<ISymbol>()
		};
	}

	/// <summary>
	/// Returns <see cref="ISymbol"/>s of all varialbes captured by the specified <see cref="AnonymousFunctionExpressionSyntax"/>.
	/// </summary>
	/// <param name="semanticModel">Parent <see cref="SemanticModel"/>.</param>
	/// <param name="node"><see cref="AnonymousFunctionExpressionSyntax"/> to get the captured variables of.</param>
	public static IEnumerable<ISymbol> GetCapturedVariables(this SemanticModel semanticModel, AnonymousFunctionExpressionSyntax node)
	{
		DataFlowAnalysis? dataFlow = semanticModel.AnalyzeDataFlow(node);

		if (dataFlow is null)
		{
			return Array.Empty<ISymbol>();
		}

		return dataFlow.Captured;
	}

	/// <summary>
	/// Returns <see cref="ISymbol"/>s of all varialbes captured by the specified <see cref="LocalFunctionStatementSyntax"/>.
	/// </summary>
	/// <param name="semanticModel">Parent <see cref="SemanticModel"/>.</param>
	/// <param name="node"><see cref="LocalFunctionStatementSyntax"/> to get the captured variables of.</param>
	public static IEnumerable<ISymbol> GetCapturedVariables(this SemanticModel semanticModel, LocalFunctionStatementSyntax node)
	{
		DataFlowAnalysis? dataFlow = semanticModel.AnalyzeDataFlow(node);

		if (dataFlow is null)
		{
			return Array.Empty<ISymbol>();
		}

		return dataFlow.Captured;
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
	/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
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
	/// Retrieves an <see cref="ISymbol"/> from the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="semanticModel"><see cref="SemanticModel"/> to retrieve the <see cref="ISymbol"/> with.</param>
	/// <param name="node"><see cref="SyntaxNode"/> to retrieve the <see cref="ISymbol"/> from.</param>
	/// <exception cref="ArgumentException"><paramref name="node"/> does not represent any symbol.</exception>
	public static ISymbol GetSymbol(this SemanticModel semanticModel, SyntaxNode node)
	{
		if (!semanticModel.TryGetSymbol(node, out ISymbol? symbol))
		{
			throw new ArgumentException("Syntax node doesn't represent any symbol", nameof(node));
		}

		return symbol;
	}

	/// <summary>
	/// Retrieves an <see cref="ISymbol"/> from the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="semanticModel"><see cref="SemanticModel"/> to retrieve the <see cref="ISymbol"/> with.</param>
	/// <param name="node"><see cref="MemberDeclarationSyntax"/> to retrieve the <see cref="ISymbol"/> from.</param>
	/// <exception cref="ArgumentException"><paramref name="node"/> does not represent any symbol.</exception>
	public static ISymbol GetSymbol(this SemanticModel semanticModel, MemberDeclarationSyntax node)
	{
		if (!semanticModel.TryGetSymbol(node, out ISymbol? symbol))
		{
			throw new ArgumentException("Syntax node doesn't represent any symbol", nameof(node));
		}

		return symbol;
	}

	/// <summary>
	/// Returns all namespaces that are used by this <see cref="MemberDeclarationSyntax"/>.
	/// </summary>
	/// <param name="semanticModel">Target <see cref="SemanticModel"/>.</param>
	/// <param name="node"><see cref="SyntaxNode"/> to get the namespaces used by.</param>
	/// <param name="compilationData"><see cref="ICompilationData"/> the specified <paramref name="node"/> is defined in.</param>
	/// <param name="skipQualifiedNames">Determines whether to skip nodes that reside inside a qualified name.</param>
	/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
	public static IEnumerable<string> GetUsedNamespaces(
		this SemanticModel semanticModel,
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
	public static IEnumerable<string> GetUsedNamespacesWithoutDistinct(
		this SemanticModel semanticModel,
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
					if (t.IsKeyword())
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

	/// <summary>
	/// Attempts to retrieve a <paramref name="symbol"/> from the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="semanticModel"><see cref="SemanticModel"/> to retrieve the <paramref name="symbol"/> with.</param>
	/// <param name="node"><see cref="SyntaxNode"/> to retrieve the <paramref name="symbol"/> from.</param>
	/// <param name="symbol">Retrieved <see cref="ISymbol"/>.</param>
	public static bool TryGetSymbol(this SemanticModel semanticModel, SyntaxNode node, [NotNullWhen(true)] out ISymbol? symbol)
	{
		if (node is MemberDeclarationSyntax member)
		{
			return semanticModel.TryGetSymbol(member, out symbol);
		}

		ISymbol? s = semanticModel.GetDeclaredSymbol(node);

		if (s is null)
		{
			SymbolInfo info = semanticModel.GetSymbolInfo(node);
			s = info.Symbol;

			if (s is null)
			{
				symbol = default;
				return false;
			}
		}

		symbol = s;
		return true;
	}

	/// <summary>
	/// Attempts to retrieve a <paramref name="symbol"/> from the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="semanticModel"><see cref="SemanticModel"/> to retrieve the <paramref name="symbol"/> with.</param>
	/// <param name="node"><see cref="MemberDeclarationSyntax"/> to retrieve the <paramref name="symbol"/> from.</param>
	/// <param name="symbol">Retrieved <see cref="ISymbol"/>.</param>
	public static bool TryGetSymbol(this SemanticModel semanticModel, MemberDeclarationSyntax node, [NotNullWhen(true)] out ISymbol? symbol)
	{
		symbol = semanticModel.GetDeclaredSymbol(node);
		return symbol is not null;
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

	private static IMethodSymbol? GetVisibleDefaultConstructor(INamedTypeSymbol parentType)
	{
		if (!parentType.HasExplicitBaseType())
		{
			return null;
		}

		IMethodSymbol? parentCtor = parentType.ContainingType!.InstanceConstructors.FirstOrDefault(ctor => ctor.IsParameterlessConstructor());

		if (parentCtor is null || parentCtor.DeclaredAccessibility == Accessibility.Private)
		{
			return null;
		}

		return parentCtor;
	}
}
