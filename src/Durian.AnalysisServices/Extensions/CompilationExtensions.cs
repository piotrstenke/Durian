using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Extensions;

/// <summary>
/// Contains various extension methods for the <see cref="Compilation"/> class.
/// </summary>
public static class CompilationExtensions
{
	/// <summary>
	/// Returns a collection of all <see cref="INamespaceSymbol"/>s contained withing the specified <paramref name="compilation"/>.
	/// </summary>
	/// <param name="compilation"><see cref="Compilation"/> to get the namespaces from.</param>
	/// <param name="includeExternal">Determines whether to include namespaces from referenced assemblies.</param>
	public static IEnumerable<INamespaceSymbol> GetAllNamespaces(this Compilation compilation, bool includeExternal = false)
	{
		INamespaceSymbol globalNamespace = includeExternal ? compilation.GlobalNamespace : compilation.Assembly.GlobalNamespace;

		Stack<INamespaceSymbol> namespaces = new(32);

		foreach (INamespaceSymbol @namespace in globalNamespace.GetNamespaceMembers())
		{
			namespaces.Push(@namespace);
		}

		while (namespaces.Count > 0)
		{
			INamespaceSymbol current = namespaces.Pop();

			yield return current;

			foreach (INamespaceSymbol child in current.GetNamespaceMembers().Reverse())
			{
				namespaces.Push(child);
			}
		}
	}

	/// <summary>
	/// Returns a collection of all types declared in the specified <paramref name="compilation"/>.
	/// </summary>
	/// <param name="compilation"><see cref="Compilation"/> to get the types from.</param>
	/// <param name="includeExternal">Determines whether to include types from referenced assemblies.</param>
	public static IEnumerable<INamedTypeSymbol> GetAllTypes(this Compilation compilation, bool includeExternal = false)
	{
		const int CAPACITY = 32;
		INamespaceSymbol globalNamespace = includeExternal ? compilation.GlobalNamespace : compilation.Assembly.GlobalNamespace;

		Stack<INamedTypeSymbol> innerTypes = new(CAPACITY);

		foreach (INamedTypeSymbol globalType in globalNamespace.GetTypeMembers())
		{
			yield return globalType;

			if (FillStack(globalType))
			{
				while (innerTypes.Count > 0)
				{
					yield return PushChildren();
				}
			}
		}

		foreach (INamespaceSymbol @namespace in compilation.GetAllNamespaces(includeExternal))
		{
			if (FillStack(@namespace))
			{
				while (innerTypes.Count > 0)
				{
					yield return PushChildren();
				}
			}
		}

		bool FillStack(INamespaceOrTypeSymbol currentSymbol)
		{
			ImmutableArray<INamedTypeSymbol> array = currentSymbol.GetTypeMembers();

			if (array.Length == 0)
			{
				return false;
			}

			foreach (INamedTypeSymbol t in array.Reverse())
			{
				innerTypes.Push(t);
			}

			return true;
		}

		INamedTypeSymbol PushChildren()
		{
			INamedTypeSymbol t = innerTypes.Pop();
			ImmutableArray<INamedTypeSymbol> array = t.GetTypeMembers();

			if (array.Length > 0)
			{
				foreach (INamedTypeSymbol child in array.Reverse())
				{
					innerTypes.Push(child);
				}
			}

			return t;
		}
	}

	/// <summary>
	/// Returns a <see cref="SemanticModel"/> and an <see cref="ISymbol"/> associated with the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="compilation">Current <see cref="Compilation"/>.</param>
	/// <param name="node"><see cref="MemberDeclarationSyntax"/> to get the <see cref="ISymbol"/> and <see cref="SemanticModel"/> of.</param>
	/// <param name="ignoreAccessibility"><see langword="true"/> if the <see cref="SemanticModel"/> should ignore accessibility rules when answering semantic questions.</param>
	public static SemanticModel GetSemanticModel(this Compilation compilation, SyntaxNode node, bool ignoreAccessibility = true)
	{
		return compilation.GetSemanticModel(node.SyntaxTree, ignoreAccessibility);
	}

	/// <summary>
	/// Returns a <see cref="SemanticModel"/> and an <see cref="ISymbol"/> associated with the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="compilation">Current <see cref="Compilation"/>.</param>
	/// <param name="node"><see cref="MemberDeclarationSyntax"/> to get the <see cref="ISymbol"/> and <see cref="SemanticModel"/> of.</param>
	/// <param name="symbol">Returned <see cref="ISymbol"/>.</param>
	/// <param name="ignoreAccessibility"><see langword="true"/> if the <see cref="SemanticModel"/> should ignore accessibility rules when answering semantic questions.</param>
	/// <exception cref="ArgumentException"><paramref name="node"/> doesn't represent any symbol.</exception>
	public static SemanticModel GetSemanticModel(this Compilation compilation, SyntaxNode node, out ISymbol symbol, bool ignoreAccessibility = true)
	{
		SemanticModel semanticModel = compilation.GetSemanticModel(node, ignoreAccessibility);
		symbol = semanticModel.GetSymbol(node);
		return semanticModel;
	}

	/// <summary>
	/// Returns a <see cref="SemanticModel"/> and an <see cref="ISymbol"/> associated with the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="compilation">Current <see cref="Compilation"/>.</param>
	/// <param name="node"><see cref="MemberDeclarationSyntax"/> to get the <see cref="ISymbol"/> and <see cref="SemanticModel"/> of.</param>
	/// <param name="symbol">Returned <see cref="ISymbol"/>.</param>
	/// <param name="ignoreAccessibility"><see langword="true"/> if the <see cref="SemanticModel"/> should ignore accessibility rules when answering semantic questions.</param>
	/// <exception cref="ArgumentException"><paramref name="node"/> doesn't represent any symbol.</exception>
	public static SemanticModel GetSemanticModel(this Compilation compilation, MemberDeclarationSyntax node, out ISymbol symbol, bool ignoreAccessibility = true)
	{
		SemanticModel semanticModel = compilation.GetSemanticModel(node, ignoreAccessibility);
		symbol = semanticModel.GetSymbol(node);
		return semanticModel;
	}

	/// <summary>
	/// Returns a <see cref="SemanticModel"/> and an <see cref="ISymbol"/> associated with the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="compilation">Current <see cref="Compilation"/>.</param>
	/// <param name="node"><see cref="MemberDeclarationSyntax"/> to get the <see cref="ISymbol"/> and <see cref="SemanticModel"/> of.</param>
	/// <param name="symbol">Returned <see cref="ISymbol"/>.</param>
	/// <param name="ignoreAccessibility"><see langword="true"/> if the <see cref="SemanticModel"/> should ignore accessibility rules when answering semantic questions.</param>
	/// <typeparam name="T">Type of symbol to return.</typeparam>
	/// <exception cref="ArgumentException">
	/// Specified <paramref name="node"/> doesn't represent any symbols. -or-
	/// Specified <paramref name="node"/> is not compatible with the <typeparamref name="T"/> symbol type.
	/// </exception>
	public static SemanticModel GetSemanticModel<T>(this Compilation compilation, SyntaxNode node, out ISymbol symbol, bool ignoreAccessibility = true) where T : ISymbol
	{
		SemanticModel semanticModel = compilation.GetSemanticModel(node, out ISymbol s, ignoreAccessibility);
		symbol = InitSymbol<T>(s);
		return semanticModel;
	}

	/// <summary>
	/// Returns a <see cref="SemanticModel"/> and an <see cref="ISymbol"/> associated with the specified <paramref name="node"/>.
	/// </summary>
	/// <param name="compilation">Current <see cref="Compilation"/>.</param>
	/// <param name="node"><see cref="MemberDeclarationSyntax"/> to get the <see cref="ISymbol"/> and <see cref="SemanticModel"/> of.</param>
	/// <param name="symbol">Returned <see cref="ISymbol"/>.</param>
	/// <param name="ignoreAccessibility"><see langword="true"/> if the <see cref="SemanticModel"/> should ignore accessibility rules when answering semantic questions.</param>
	/// <typeparam name="T">Type of symbol to return.</typeparam>
	/// <exception cref="ArgumentException">
	/// Specified <paramref name="node"/> doesn't represent any symbols. -or-
	/// Specified <paramref name="node"/> is not compatible with the <typeparamref name="T"/> symbol type.
	/// </exception>
	public static SemanticModel GetSemanticModel<T>(this Compilation compilation, MemberDeclarationSyntax node, out T symbol, bool ignoreAccessibility = true) where T : ISymbol
	{
		SemanticModel semanticModel = compilation.GetSemanticModel(node, out ISymbol s, ignoreAccessibility);
		symbol = InitSymbol<T>(s);
		return semanticModel;
	}

	/// <summary>
	/// Returns the <see cref="INamedTypeSymbol"/> represented by the specified <see cref="SpecialAttribute"/>.
	/// </summary>
	/// <param name="compilation"><see cref="Compilation"/> to get the <see cref="INamedTypeSymbol"/> from.</param>
	/// <param name="type"><see cref="SpecialAttribute"/> to get.</param>
	public static INamedTypeSymbol? GetSpecialType(this Compilation compilation, SpecialAttribute type)
	{
		string? @namespace = type.GetNamespaceName();

		if (@namespace is null)
		{
			return null;
		}

		string? name = type.GetAttributeName();

		if (name is null)
		{
			return null;
		}

		return compilation.GetTypeByMetadataName($"{@namespace}.{name}");
	}

	/// <summary>
	/// Returns the <see cref="INamedTypeSymbol"/> represented by the specified <see cref="NullableAnnotationAttribute"/>.
	/// </summary>
	/// <param name="compilation"><see cref="Compilation"/> to get the <see cref="INamedTypeSymbol"/> from.</param>
	/// <param name="type"><see cref="NullableAnnotationAttribute"/> to get.</param>
	public static INamedTypeSymbol? GetSpecialType(this Compilation compilation, NullableAnnotationAttribute type)
	{
		string? @namespace = type.GetNamespaceName();

		if (@namespace is null)
		{
			return null;
		}

		string? name = type.GetAttributeName();

		if (name is null)
		{
			return null;
		}

		return compilation.GetTypeByMetadataName($"{@namespace}.{name}");
	}

	/// <summary>
	/// Returns the <see cref="INamedTypeSymbol"/> represented by the specified <see cref="DecimalValueType"/>.
	/// </summary>
	/// <param name="compilation"><see cref="Compilation"/> to get the <see cref="INamedTypeSymbol"/> from.</param>
	/// <param name="type"><see cref="DecimalValueType"/> to get.</param>
	public static INamedTypeSymbol? GetSpecialType(this Compilation compilation, DecimalValueType type)
	{
		SpecialType specialType = type.GetSpecialType();
		return compilation.GetSpecialType(specialType);
	}

	/// <summary>
	/// Returns the <see cref="INamedTypeSymbol"/> represented by the specified <see cref="IntegerValueType"/>.
	/// </summary>
	/// <param name="compilation"><see cref="Compilation"/> to get the <see cref="INamedTypeSymbol"/> from.</param>
	/// <param name="type"><see cref="IntegerValueType"/> to get.</param>
	public static INamedTypeSymbol? GetSpecialType(this Compilation compilation, IntegerValueType type)
	{
		SpecialType specialType = type.GetSpecialType();
		return compilation.GetSpecialType(specialType);
	}

	/// <summary>
	/// Returns the <see cref="INamedTypeSymbol"/> represented by the specified <see cref="TypeKeyword"/>.
	/// </summary>
	/// <param name="compilation"><see cref="Compilation"/> to get the <see cref="INamedTypeSymbol"/> from.</param>
	/// <param name="type"><see cref="TypeKeyword"/> to get.</param>
	public static INamedTypeSymbol? GetSpecialType(this Compilation compilation, TypeKeyword type)
	{
		SpecialType specialType = type.GetSpecialType();
		return compilation.GetSpecialType(specialType);
	}

	/// <summary>
	/// Returns all special types in the <paramref name="compilation"/>. See: <see cref="SpecialType"/>.
	/// </summary>
	/// <param name="compilation"><see cref="Compilation"/> to get the special types from.</param>
	public static IEnumerable<INamedTypeSymbol> GetSpecialTypes(this Compilation compilation)
	{
		Array array = Enum.GetValues(typeof(SpecialType));
		int length = array.Length;

		for (int i = 1; i < length; i++)
		{
			yield return compilation.GetSpecialType((SpecialType)array.GetValue(i)!);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static T InitSymbol<T>(ISymbol symbol)
	{
		if (symbol is not T t)
		{
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
			throw new ArgumentException($"Syntax node is not compatible with the '{nameof(T)}' symbol type!", "node");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
		}

		return t;
	}
}
