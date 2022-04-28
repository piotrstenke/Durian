// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Extensions
{
	/// <summary>
	/// Contains various extension methods for the <see cref="ISymbol"/>-derived interfaces.
	/// </summary>
	public static class SymbolExtensions
	{
		/// <summary>
		/// Determines whether the specified <paramref name="type"/> can inherit from other types.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check if can inherit.</param>
		public static bool CanInherit(this INamedTypeSymbol type)
		{
			return type.TypeKind == TypeKind.Class && !type.IsStatic;
		}

		/// <summary>
		/// Determines whether the <paramref name="child"/> is contained withing the <paramref name="parent"/> at any nesting level.
		/// </summary>
		/// <param name="parent">Parent <see cref="ISymbol"/>.</param>
		/// <param name="child">Child <see cref="ISymbol"/>.</param>
		/// <returns>True if the <paramref name="parent"/> contains the <paramref name="child"/> or the <paramref name="parent"/> is equivalent to <paramref name="child"/>, otherwise false.</returns>
		public static bool ContainsSymbol(this ISymbol parent, ISymbol child)
		{
			ISymbol? current = child;

			while (current is not null)
			{
				if (SymbolEqualityComparer.Default.Equals(current, parent))
				{
					return true;
				}

				current = current.ContainingSymbol;
			}

			return false;
		}

		/// <summary>
		/// Creates a <see cref="TypeSyntax"/> representing the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to get the <see cref="TypeSyntax"/> for.</param>
		public static TypeSyntax CreateTypeSyntax(this INamedTypeSymbol type)
		{
			TypeSyntax syntax;

			if (type.IsGenericType)
			{
				List<TypeSyntax> arguments = new(type.TypeArguments.Length);

				foreach (ITypeSymbol t in type.TypeArguments)
				{
					arguments.Add(t.CreateTypeSyntax());
				}

				syntax = SyntaxFactory.GenericName(
					SyntaxFactory.Identifier(type.Name),
					SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(arguments)));
			}
			else if (type.GetPredefineTypeSyntax() is PredefinedTypeSyntax predefined)
			{
				syntax = predefined;
			}
			else
			{
				syntax = SyntaxFactory.IdentifierName(type.Name);
			}

			return ApplyAnnotation(syntax, NullableAnnotation.Annotated);
		}

		/// <summary>
		/// Creates a <see cref="TypeSyntax"/> representing the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="IArrayTypeSymbol"/> to get the <see cref="TypeSyntax"/> for.</param>
		public static TypeSyntax CreateTypeSyntax(this IArrayTypeSymbol type)
		{
			ITypeSymbol[] elementTypes = type.GetElementTypes().ToArray();
			TypeSyntax elementSyntax = elementTypes[0].CreateTypeSyntax();

			List<ArrayRankSpecifierSyntax> ranks = new(elementTypes.Length - 1);

			for (int i = 1; i < elementTypes.Length; i++)
			{
				IArrayTypeSymbol array = (IArrayTypeSymbol)elementTypes[i];
				ranks.Add(GetArrayRank(array));

				if (array.NullableAnnotation == NullableAnnotation.Annotated)
				{
					elementSyntax = SyntaxFactory.NullableType(SyntaxFactory.ArrayType(elementSyntax, SyntaxFactory.List(ranks)));
					ranks.Clear();
				}
			}

			ranks.Add(GetArrayRank(type));

			return ApplyAnnotation(SyntaxFactory.ArrayType(elementSyntax, SyntaxFactory.List(ranks)), type.NullableAnnotation);

			static ArrayRankSpecifierSyntax GetArrayRank(IArrayTypeSymbol array)
			{
				return SyntaxFactory.ArrayRankSpecifier(SyntaxFactory.SeparatedList<ExpressionSyntax>(
					array.Sizes.Select(s => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(s)))));
			}
		}

		/// <summary>
		/// Creates a <see cref="TypeSyntax"/> representing the specified <paramref name="functionPointer"/>.
		/// </summary>
		/// <param name="functionPointer"><see cref="IFunctionPointerTypeSymbol"/> to get the <see cref="TypeSyntax"/> for.</param>
		public static TypeSyntax CreateTypeSyntax(this IFunctionPointerTypeSymbol functionPointer)
		{
			IMethodSymbol signature = functionPointer.Signature;

			FunctionPointerCallingConventionSyntax? callingConvention;

			if (signature.CallingConvention == SignatureCallingConvention.Unmanaged)
			{
				FunctionPointerUnmanagedCallingConventionListSyntax? list = signature.UnmanagedCallingConventionTypes.Length > 0 ?
					SyntaxFactory.FunctionPointerUnmanagedCallingConventionList(SyntaxFactory.SeparatedList(signature.UnmanagedCallingConventionTypes.Select(u =>
						SyntaxFactory.FunctionPointerUnmanagedCallingConvention(SyntaxFactory.Identifier(u.Name)))))
					: default;

				callingConvention = SyntaxFactory.FunctionPointerCallingConvention(SyntaxFactory.Token(SyntaxKind.UnmanagedKeyword), list);
			}
			else
			{
				callingConvention = default;
			}

			List<FunctionPointerParameterSyntax> parameters = new(signature.Parameters.Length + 1);

			foreach (IParameterSymbol parameter in signature.Parameters)
			{
				TypeSyntax parameterType = parameter.Type.CreateTypeSyntax();
				parameters.Add(GetParameterSyntax(parameterType, parameter.RefKind, false));
			}

			parameters.Add(GetParameterSyntax(signature.ReturnType.CreateTypeSyntax(), signature.RefKind, true));

			return SyntaxFactory.FunctionPointerType(callingConvention, SyntaxFactory.FunctionPointerParameterList(SyntaxFactory.SeparatedList(parameters)));

			static FunctionPointerParameterSyntax GetParameterSyntax(TypeSyntax parameterType, RefKind refKind, bool allowRefReadonly)
			{
				switch (refKind)
				{
					case RefKind.None:
						return SyntaxFactory.FunctionPointerParameter(parameterType);

					case RefKind.RefReadOnly:

						if (!allowRefReadonly)
						{
							goto default;
						}

						return SyntaxFactory.FunctionPointerParameter(default, SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.RefKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)), parameterType);

					default:
						SyntaxKind kind = AnalysisUtilities.RefKindToSyntax(refKind);
						return SyntaxFactory.FunctionPointerParameter(default, SyntaxFactory.TokenList(SyntaxFactory.Token(kind)), parameterType);
				}
			}
		}

		/// <summary>
		/// Creates a <see cref="TypeSyntax"/> representing the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="IDynamicTypeSymbol"/> to get the <see cref="TypeSyntax"/> for.</param>
		public static TypeSyntax CreateTypeSyntax(this IDynamicTypeSymbol type)
		{
			return ApplyAnnotation(SyntaxFactory.IdentifierName("dynamic"), type.NullableAnnotation);
		}

		/// <summary>
		/// Creates a <see cref="TypeSyntax"/> representing the specified <paramref name="typeParameter"/>.
		/// </summary>
		/// <param name="typeParameter"><see cref="ITypeParameterSymbol"/> to get the <see cref="TypeSyntax"/> for.</param>
		public static TypeSyntax CreateTypeSyntax(this ITypeParameterSymbol typeParameter)
		{
			return ApplyAnnotation(SyntaxFactory.IdentifierName(typeParameter.Name), typeParameter.NullableAnnotation);
		}

		/// <summary>
		/// Creates a <see cref="TypeSyntax"/> representing the specified <paramref name="pointer"/>.
		/// </summary>
		/// <param name="pointer"><see cref="IPointerTypeSymbol"/> to get the <see cref="TypeSyntax"/> for.</param>
		public static TypeSyntax CreateTypeSyntax(this IPointerTypeSymbol pointer)
		{
			return SyntaxFactory.PointerType(pointer.PointedAtType.CreateTypeSyntax());
		}

		/// <summary>
		/// Creates a <see cref="TypeSyntax"/> representing the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="ITypeSymbol"/> to get the <see cref="TypeSyntax"/> for.</param>
		public static TypeSyntax CreateTypeSyntax(this ITypeSymbol type)
		{
			return type switch
			{
				INamedTypeSymbol named => named.CreateTypeSyntax(),
				IDynamicTypeSymbol dynamic => dynamic.CreateTypeSyntax(),
				IArrayTypeSymbol array => array.CreateTypeSyntax(),
				ITypeParameterSymbol typeParameter => typeParameter.CreateTypeSyntax(),
				IPointerTypeSymbol pointer => pointer.CreateTypeSyntax(),
				IFunctionPointerTypeSymbol functionPointer => functionPointer.CreateTypeSyntax(),
				_ => ApplyAnnotation(SyntaxFactory.IdentifierName(type.Name), type.NullableAnnotation),
			};
		}

		/// <summary>
		/// Creates a <see cref="TypeSyntax"/> representing the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the <see cref="TypeSyntax"/> for.</param>
		public static TypeSyntax CreateTypeSyntax(this ISymbol symbol)
		{
			if (symbol is ITypeSymbol type)
			{
				return type.CreateTypeSyntax();
			}

			if (symbol is IMethodSymbol method)
			{
				return method.CreateTypeSyntax();
			}
			return SyntaxFactory.IdentifierName(symbol.Name);
		}

		/// <summary>
		/// Creates a <see cref="SimpleNameSyntax"/> representing the specified <paramref name="method"/>.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to get the <see cref="SimpleNameSyntax"/> for.</param>
		public static TypeSyntax CreateTypeSyntax(this IMethodSymbol method)
		{
			if (!method.IsGenericMethod)
			{
				return SyntaxFactory.IdentifierName(method.Name);
			}

			List<TypeSyntax> arguments = new(method.TypeArguments.Length);

			foreach (ITypeSymbol type in method.TypeArguments)
			{
				arguments.Add(type.CreateTypeSyntax());
			}

			return SyntaxFactory.GenericName(
				SyntaxFactory.Identifier(method.Name),
				SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(arguments)));
		}

		/// <inheritdoc cref="GetAllMembers(INamedTypeSymbol, string)"/>
		public static IEnumerable<ISymbol> GetAllMembers(this INamedTypeSymbol type)
		{
			return type.GetMembers().Concat(GetBaseTypes(type).SelectMany(t => t.GetMembers()));
		}

		/// <summary>
		/// Returns all members of the specified <paramref name="type"/> including the members that are declared in base types of this <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="ITypeSymbol"/> to get the members of.</param>
		/// <param name="name">Name of the members to find.</param>
		public static IEnumerable<ISymbol> GetAllMembers(this INamedTypeSymbol type, string name)
		{
			if (type.TypeKind == TypeKind.Interface)
			{
				return type.GetMembers(name)
					.Concat(type.AllInterfaces
						.SelectMany(intf => intf.GetMembers(name)));
			}

			return type.GetMembers(name)
				.Concat(GetBaseTypes(type)
					.SelectMany(t => t.GetMembers(name)));
		}

		/// <summary>
		/// Returns an <see cref="AttributeData"/> associated with the <paramref name="attrSymbol"/> and defined on the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol">Target <see cref="ISymbol"/>.</param>
		/// <param name="attrSymbol">Type of attribute to look for.</param>
		/// <returns>The <see cref="AttributeData"/> associated with the <paramref name="attrSymbol"/> and defined on the specified <paramref name="symbol"/>. -or- <see langword="null"/> if no such <see cref="AttributeData"/> found.</returns>
		public static AttributeData? GetAttribute(this ISymbol symbol, INamedTypeSymbol attrSymbol)
		{
			return symbol.GetAttributes()
				.FirstOrDefault(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attrSymbol));
		}

		/// <summary>
		/// Returns an <see cref="AttributeData"/> associated with the <paramref name="syntax"/> defined on the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol">Target <see cref="ISymbol"/>.</param>
		/// <param name="syntax"><see cref="AttributeSyntax"/> to get the data of.</param>
		/// <returns>The <see cref="AttributeData"/> associated with the <paramref name="syntax"/>. -or- <see langword="null"/> if no such <see cref="AttributeData"/> found.</returns>
		public static AttributeData? GetAttribute(this ISymbol symbol, AttributeSyntax syntax)
		{
			foreach (AttributeData attr in symbol.GetAttributes())
			{
				SyntaxReference? reference = attr.ApplicationSyntaxReference;

				if (reference is null)
				{
					continue;
				}

				if (reference.Span == syntax.Span)
				{
					return attr;
				}
			}

			return null;
		}

		/// <summary>
		/// Returns a collection of <see cref="AttributeData"/>s associated with the <paramref name="attrSymbol"/> and defined on the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol">Target <see cref="ISymbol"/>.</param>
		/// <param name="attrSymbol">Type of attributes to look for.</param>
		public static IEnumerable<AttributeData> GetAttributes(this ISymbol symbol, INamedTypeSymbol attrSymbol)
		{
			foreach (AttributeData attr in symbol.GetAttributes())
			{
				if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attrSymbol))
				{
					yield return attr;
				}
			}
		}

		/// <summary>
		/// Returns the backing field of the specified <paramref name="property"/> or <see langword="null"/> if the <paramref name="property"/> is not auto-implemented.
		/// </summary>
		/// <param name="property"><see cref="IPropertySymbol"/> to get the backing field of.</param>
		public static IFieldSymbol? GetBackingField(this IPropertySymbol property)
		{
			return property.ContainingType?
				.GetMembers()
				.OfType<IFieldSymbol>()
				.FirstOrDefault(f => SymbolEqualityComparer.Default.Equals(f.AssociatedSymbol, property));
		}

		/// <summary>
		/// Returns all <see cref="IMethodSymbol"/> this <paramref name="method"/> overrides.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to get the base methods of.</param>
		public static IEnumerable<IMethodSymbol> GetBaseMethods(this IMethodSymbol method)
		{
			IMethodSymbol? m = method;

			while ((m = m!.OverriddenMethod) is not null)
			{
				yield return m;
			}
		}

		/// <summary>
		/// Returns all types the specified <paramref name="type"/> inherits from.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to get the base types of.</param>
		/// <param name="includeSelf">Determines whether to include the <paramref name="type"/> in the returned collection.</param>
		public static IEnumerable<INamedTypeSymbol> GetBaseTypes(this INamedTypeSymbol type, bool includeSelf = false)
		{
			if (includeSelf)
			{
				yield return type;
			}

			INamedTypeSymbol? currentType = type.BaseType;

			if (currentType is not null)
			{
				yield return currentType;

				while ((currentType = currentType!.BaseType) is not null)
				{
					yield return currentType;
				}
			}
		}

		/// <summary>
		/// Returns generic constraints applied to type parameters of the specified <paramref name="method"/>.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to get the generic constraints of.</param>
		/// <param name="includeParentParameters">
		/// If a <see cref="ITypeParameterSymbol"/> is constrained to another <see cref="ITypeParameterSymbol"/>,
		/// determines whether to also include constraints of that <see cref="ITypeParameterSymbol"/>'s base parameter.
		/// </param>
		public static GenericConstraint[] GetConstraints(this IMethodSymbol method, bool includeParentParameters = false)
		{
			if (!method.IsGenericMethod)
			{
				return Array.Empty<GenericConstraint>();
			}

			return method.TypeParameters.Select(p => p.GetConstraints(includeParentParameters)).ToArray();
		}

		/// <summary>
		/// Returns generic constraints applied to type parameters of the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to get the generic constraints of.</param>
		/// <param name="includeParentParameters">
		/// If a <see cref="ITypeParameterSymbol"/> is constrained to another <see cref="ITypeParameterSymbol"/>,
		/// determines whether to also include constraints of that <see cref="ITypeParameterSymbol"/>'s base parameter.
		/// </param>
		public static GenericConstraint[] GetConstraints(this INamedTypeSymbol type, bool includeParentParameters = false)
		{
			if (!type.IsGenericType)
			{
				return Array.Empty<GenericConstraint>();
			}

			return type.TypeParameters.Select(p => p.GetConstraints(includeParentParameters)).ToArray();
		}

		/// <summary>
		/// Returns generic constraints applied to the specified <paramref name="parameter"/>.
		/// </summary>
		/// <param name="parameter"><see cref="ITypeParameterSymbol"/> to get the generic constraint of.</param>
		/// <param name="includeParentParameter">
		/// If the <paramref name="parameter"/> is constrained to another <see cref="ITypeParameterSymbol"/>,
		/// determines whether to also include constraints of the <paramref name="parameter"/>'s base parameter.
		/// </param>
		public static GenericConstraint GetConstraints(this ITypeParameterSymbol parameter, bool includeParentParameter = false)
		{
			GenericConstraint constraint = default;

			if (parameter.HasReferenceTypeConstraint)
			{
				constraint |= GenericConstraint.Class;
			}

			if (parameter.HasValueTypeConstraint)
			{
				constraint |= GenericConstraint.Struct;
			}

			if (parameter.HasUnmanagedTypeConstraint)
			{
				constraint |= GenericConstraint.Unmanaged;
			}

			if (parameter.HasConstructorConstraint)
			{
				constraint |= GenericConstraint.New;
			}

			if (parameter.HasNotNullConstraint)
			{
				constraint |= GenericConstraint.NotNull;
			}

			if (parameter.ConstraintTypes.Length > 0)
			{
				constraint |= GenericConstraint.Type;

				if (includeParentParameter && parameter.ConstraintTypes.FirstOrDefault(p => p.TypeKind == TypeKind.TypeParameter) is ITypeParameterSymbol baseParameter)
				{
					constraint |= baseParameter.GetConstraints(true);
				}
			}

			return constraint;
		}

		/// <summary>
		/// Returns all <see cref="INamespaceSymbol"/>s that contain the target <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the parent namespaces of.</param>
		/// <param name="includeGlobal">Determines whether to return the global namespace as well.</param>
		/// <param name="order">Specifies ordering of the returned members.</param>
		public static IEnumerable<INamespaceSymbol> GetContainingNamespaces(this ISymbol symbol, bool includeGlobal = false, ReturnOrder order = ReturnOrder.Root)
		{
			IEnumerable<INamespaceSymbol> namespaces = ReturnByOrder(GetNamespaces(), order);

			if (!includeGlobal)
			{
				namespaces = namespaces.Where(n => !n.IsGlobalNamespace);
			}

			return namespaces;

			IEnumerable<INamespaceSymbol> GetNamespaces()
			{
				INamespaceSymbol parent = symbol.ContainingNamespace;

				if (parent is not null)
				{
					yield return parent;

					while ((parent = parent!.ContainingNamespace) is not null)
					{
						yield return parent;
					}
				}
			}
		}

		/// <summary>
		/// Returns all <see cref="INamespaceOrTypeSymbol"/>s contain the target <paramref name="symbol"/> in namespace-first order.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the parent types and namespaces of.</param>
		/// <param name="includeGlobal">Determines whether to return the global namespace as well</param>
		public static IEnumerable<INamespaceOrTypeSymbol> GetContainingNamespacesAndTypes(this ISymbol symbol, bool includeGlobal = false)
		{
			return GetNamespacesAndTypes();

			IEnumerable<INamespaceOrTypeSymbol> GetNamespacesAndTypes()
			{
				foreach (INamespaceSymbol s in GetContainingNamespaces(symbol, includeGlobal))
				{
					yield return s;
				}

				foreach (INamedTypeSymbol s in GetContainingTypes(symbol))
				{
					yield return s;
				}
			}
		}

		/// <summary>
		/// Returns all <see cref="INamedTypeSymbol"/>s that contain the target <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the parent types of.</param>
		/// <param name="includeSelf">Determines whether to include the <paramref name="symbol"/> in the returned collection if its a <see cref="INamedTypeSymbol"/>.</param>
		/// <param name="order">Specifies ordering of the returned members.</param>
		public static IEnumerable<INamedTypeSymbol> GetContainingTypes(this ISymbol symbol, bool includeSelf = false, ReturnOrder order = ReturnOrder.Root)
		{
			return ReturnByOrder(GetTypes(), order);

			IEnumerable<INamedTypeSymbol> GetTypes()
			{
				if (includeSelf && symbol is INamedTypeSymbol t)
				{
					yield return t;
				}

				INamedTypeSymbol parent = symbol.ContainingType;

				if (parent is not null)
				{
					yield return parent;

					while ((parent = parent!.ContainingType) is not null)
					{
						yield return parent;
					}
				}
			}
		}

		/// <summary>
		/// Returns all <see cref="ITypeData"/>s that contain the target <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the parent types of.</param>
		/// <param name="compilation">Current <see cref="ICompilationData"/>.</param>
		/// <param name="includeSelf">Determines whether to include the <paramref name="symbol"/> in the returned collection if its a <see cref="INamedTypeSymbol"/>.</param>
		/// <param name="order">Specifies ordering of the returned members.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static IEnumerable<ITypeData> GetContainingTypesAsData(this ISymbol symbol, ICompilationData compilation, bool includeSelf = false, ReturnOrder order = ReturnOrder.Root)
		{
			INamedTypeSymbol[] parentSymbols = GetContainingTypes(symbol, includeSelf, order).ToArray();
			List<ITypeData> parentList = new(parentSymbols.Length);

			return parentSymbols.Select<INamedTypeSymbol, ITypeData>(parent =>
			{
				if(parent.GetMemberData(compilation) is not ITypeData data)
				{
					throw new InvalidOperationException($"Invalid type kind of '{parent}'");
				}

				parentList.Add(data);
				return data;
			});
		}

		/// <summary>
		/// Returns the default constructor of the specified <paramref name="type"/> if it has one.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to get the default constructor of.</param>
		public static IMethodSymbol? GetDefaultConstructor(this INamedTypeSymbol type)
		{
			return type.InstanceConstructors.FirstOrDefault(ctor => ctor.IsImplicitlyDeclared && ctor.Parameters.Length == 0);
		}

		/// <summary>
		/// Returns the effective <see cref="Accessibility"/> of the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the effective <see cref="Accessibility"/> of.</param>
		public static Accessibility GetEffectiveAccessibility(this ISymbol symbol)
		{
			ISymbol? s = symbol;
			Accessibility lowest = Accessibility.Public;

			while (s is not null)
			{
				Accessibility current = s.DeclaredAccessibility;

				if (current == Accessibility.Private)
				{
					return current;
				}

				if (current != Accessibility.NotApplicable && current < lowest)
				{
					lowest = current;
				}

				s = s.ContainingSymbol;
			}

			return lowest;
		}

		/// <summary>
		/// Returns the effective underlaying element type of the <paramref name="array"/> or any of its element array types.
		/// </summary>
		/// <param name="array"><see cref="IArrayTypeSymbol"/> to get the effective underlaying type of.</param>
		/// <returns>The effective underlaying type the <paramref name="array"/> or any of its element array types. -or- <paramref name="array"/> if no such type was found.</returns>
		public static ITypeSymbol GetEffectiveElementType(this IArrayTypeSymbol array)
		{
			ITypeSymbol? a = array;

			while (a is IArrayTypeSymbol t)
			{
				a = t.ElementType;
			}

			if (a is null)
			{
				return array;
			}

			return a;
		}

		/// <summary>
		/// Returns the effective underlaying type the <paramref name="pointer"/> or any of its child pointers point to.
		/// </summary>
		/// <param name="pointer"><see cref="IPointerTypeSymbol"/> to get the effective underlaying type of.</param>
		/// <returns>The effective underlaying type the <paramref name="pointer"/> or any of its child pointers point to. -or- <paramref name="pointer"/> if no such type was found.</returns>
		public static ITypeSymbol GetEffectivePointerAtType(this IPointerTypeSymbol pointer)
		{
			ITypeSymbol? p = pointer;

			while (p is IPointerTypeSymbol t)
			{
				p = t.PointedAtType;
			}

			if (p is null)
			{
				return pointer;
			}

			return p;
		}

		/// <summary>
		/// Returns all underlaying element types of the specified <paramref name="array"/>.
		/// </summary>
		/// <param name="array"><see cref="IArrayTypeSymbol"/> to get the element types of.</param>
		/// <param name="order">Specifies ordering of the returned members.</param>
		public static IEnumerable<ITypeSymbol> GetElementTypes(this IArrayTypeSymbol array, ReturnOrder order = ReturnOrder.Root)
		{
			return ReturnByOrder(Yield(), order);

			IEnumerable<ITypeSymbol> Yield()
			{
				ITypeSymbol element = array.ElementType;

				yield return element;

				while (element is IArrayTypeSymbol array)
				{
					yield return array.ElementType;
					element = array.ElementType;
				}
			}
		}

		/// <summary>
		/// Returns a <see cref="string"/> containing generic identifier of the specified <paramref name="symbol"/> or name of the <paramref name="symbol"/> if it is not an <see cref="IMethodSymbol"/> or <see cref="INamedTypeSymbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the generic name of.</param>
		/// <param name="substitution">Configures how generic type parameters are substituted.</param>
		public static string GetGenericName(this ISymbol symbol, GenericSubstitution substitution = default)
		{
			StringBuilder builder = new();
			symbol.GetGenericNameInto(builder, substitution);
			return builder.ToString();
		}

		/// <summary>
		/// Writes a <see cref="string"/> containing generic identifier of the specified <paramref name="symbol"/> or name of the <paramref name="symbol"/> if it is not an <see cref="IMethodSymbol"/> or <see cref="INamedTypeSymbol"/> to the specified <paramref name="builder"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the generic name of.</param>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <param name="substitution">Configures how generic type parameters are substituted.</param>
		public static void GetGenericNameInto(this ISymbol symbol, StringBuilder builder, GenericSubstitution substitution = default)
		{
			if (symbol is INamedTypeSymbol t)
			{
				t.GetGenericNameInto(builder, substitution);
			}
			else if (symbol is IMethodSymbol m)
			{
				m.GetGenericNameInto(builder, substitution);
			}
			else
			{
				builder.Append(symbol.Name);
			}
		}

		/// <summary>
		/// Returns a <see cref="string"/> containing generic identifier of the specified <paramref name="method"/>.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to get the generic name of.</param>
		/// <param name="substitution">Configures how generic type parameters are substituted.</param>
		public static string GetGenericName(this IMethodSymbol method, GenericSubstitution substitution = default)
		{
			StringBuilder builder = new();
			method.GetGenericNameInto(builder, substitution);
			return builder.ToString();
		}

		/// <summary>
		/// Writes a <see cref="string"/> containing generic identifier of the specified <paramref name="method"/> to the specified <paramref name="builder"/>.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to get the generic name of.</param>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <param name="substitution">Configures how generic type parameters are substituted.</param>
		public static void GetGenericNameInto(this IMethodSymbol method, StringBuilder builder, GenericSubstitution substitution = default)
		{
			if(substitution.HasFlag(GenericSubstitution.TypeArguments))
			{
				method.TypeArguments.GetGenericNameInto(builder, method.Name);
			}
			else
			{
				method.TypeParameters.GetGenericNameInto(builder, method.Name, substitution.HasFlag(GenericSubstitution.Variance));
			}

			if (substitution.HasFlag(GenericSubstitution.ParameterList))
			{
				method.GetParameterListInto(builder, substitution);
			}
		}

		/// <summary>
		/// Returns a <see cref="string"/> containing generic identifier of the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to get the generic name of.</param>
		/// <param name="substitution">Configures how generic type parameters are substituted.</param>
		public static string GetGenericName(this INamedTypeSymbol type, GenericSubstitution substitution = default)
		{
			StringBuilder builder = new();
			type.GetGenericNameInto(builder, substitution);
			return builder.ToString();
		}

		/// <summary>
		/// Returns a <see cref="string"/> containing generic identifier of the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to get the generic name of.</param>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <param name="substitution">Configures how generic type parameters are substituted.</param>
		public static void GetGenericNameInto(this INamedTypeSymbol type, StringBuilder builder, GenericSubstitution substitution = default)
		{
			string typeName = type.GetTypeKeyword() ?? type.Name;

			if(substitution.HasFlag(GenericSubstitution.TypeArguments))
			{
				type.TypeArguments.GetGenericNameInto(builder, typeName);
			}
			else
			{
				type.TypeParameters.GetGenericNameInto(builder, typeName, substitution.HasFlag(GenericSubstitution.Variance));
			}

			if (substitution.HasFlag(GenericSubstitution.ParameterList) && type.DelegateInvokeMethod is not null)
			{
				type.DelegateInvokeMethod.GetParameterListInto(builder, substitution);
			}
		}

		/// <summary>
		/// Returns a <see cref="string"/> containing the generic part of an identifier created from the collection of <paramref name="typeParameters"/>.
		/// </summary>
		/// <param name="typeParameters">Type parameters.</param>
		/// <param name="includeVariance">Determines whether to include variance of the <paramref name="typeParameters"/>.</param>
		/// <exception cref="InvalidOperationException">Pointers can't be used as generic arguments.</exception>
		public static string GetGenericName(this IEnumerable<ITypeParameterSymbol> typeParameters, bool includeVariance = false)
		{
			return typeParameters.GetGenericName(null, includeVariance);
		}

		/// <summary>
		/// Writes a <see cref="string"/> containing the generic part of an identifier created from the collection of <paramref name="typeParameters"/> to the specified <paramref name="builder"/>.
		/// </summary>
		/// <param name="typeParameters">Type parameters.</param>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <param name="includeVariance">Determines whether to include variance of the <paramref name="typeParameters"/>.</param>
		/// <exception cref="InvalidOperationException">Pointers can't be used as generic arguments.</exception>
		public static void GetGenericNameInto(this IEnumerable<ITypeParameterSymbol> typeParameters, StringBuilder builder, bool includeVariance = false)
		{
			typeParameters.GetGenericNameInto(builder, null, includeVariance);
		}

		/// <summary>
		/// Returns a <see cref="string"/> containing generic identifier combined of the specified <paramref name="name"/> and the collection of <paramref name="typeParameters"/>.
		/// </summary>
		/// <param name="typeParameters">Type parameters.</param>
		/// <param name="name">Actual member identifier.</param>
		/// <param name="includeVariance">Determines whether to include variance of the <paramref name="typeParameters"/>.</param>
		public static string GetGenericName(this IEnumerable<ITypeParameterSymbol> typeParameters, string? name, bool includeVariance = false)
		{
			if (includeVariance)
			{
				return AnalysisUtilities.GetGenericName(typeParameters.Select(p =>
				{
					if (p.Variance == VarianceKind.Out || p.Variance == VarianceKind.In)
					{
						return $"{p.Variance.ToString().ToLower()} {p.Name}";
					}

					return p.Name;
				}),
				name);
			}

			return AnalysisUtilities.GetGenericName(typeParameters.Select(p => p.Name), name);
		}

		/// <summary>
		/// Writes a <see cref="string"/> containing generic identifier combined of the specified <paramref name="name"/> and the collection of <paramref name="typeParameters"/> to the specified <paramref name="builder"/>.
		/// </summary>
		/// <param name="typeParameters">Type parameters.</param>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <param name="name">Actual member identifier.</param>
		/// <param name="includeVariance">Determines whether to include variance of the <paramref name="typeParameters"/>.</param>
		public static void GetGenericNameInto(this IEnumerable<ITypeParameterSymbol> typeParameters, StringBuilder builder, string? name, bool includeVariance = false)
		{
			if (includeVariance)
			{
				AnalysisUtilities.GetGenericNameInto(typeParameters.Select(p =>
				{
					if (p.Variance == VarianceKind.Out || p.Variance == VarianceKind.In)
					{
						return $"{p.Variance.ToString().ToLower()} {p.Name}";
					}

					return p.Name;
				}),
				name,
				builder);
			}

			AnalysisUtilities.GetGenericNameInto(typeParameters.Select(p => p.Name), name, builder);
		}

		/// <summary>
		/// Returns a <see cref="string"/> containing the generic part of an identifier created from the collection of <paramref name="typeArguments"/>.
		/// </summary>
		/// <param name="typeArguments">Type arguments.</param>
		/// <exception cref="InvalidOperationException">Pointers can't be used as generic arguments.</exception>
		public static string GetGenericName(this IEnumerable<ITypeSymbol> typeArguments)
		{
			StringBuilder builder = new();
			typeArguments.GetGenericNameInto(builder);
			return builder.ToString();
		}

		/// <summary>
		/// Writes a <see cref="string"/> containing the generic part of an identifier created from the collection of <paramref name="typeArguments"/> to the specified <paramref name="builder"/>.
		/// </summary>
		/// <param name="typeArguments">Type arguments.</param>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <exception cref="InvalidOperationException">Pointers can't be used as generic arguments.</exception>
		public static void GetGenericNameInto(this IEnumerable<ITypeSymbol> typeArguments, StringBuilder builder)
		{
			if (typeArguments is IEnumerable<ITypeParameterSymbol> parameters)
			{
				parameters.GetGenericNameInto(builder);
				return;
			}

			ITypeSymbol[] symbols = typeArguments.ToArray();

			if (symbols.Length == 0)
			{
				return;
			}

			CodeBuilder cb = new(builder);

			builder.Append('<');

			foreach (ITypeSymbol argument in symbols)
			{
				if (argument is null)
				{
					continue;
				}

				if (argument is IPointerTypeSymbol or IFunctionPointerTypeSymbol)
				{
					throw new InvalidOperationException("Pointers can't be used as generic arguments!");
				}

				cb.Type(argument);

				builder.Append(", ");
			}

			builder.Remove(builder.Length - 2, 2);
			builder.Append('>');
		}

		/// <summary>
		/// Returns a <see cref="string"/> containing generic identifier combined of the specified <paramref name="name"/> and the collection of <paramref name="typeArguments"/>.
		/// </summary>
		/// <param name="typeArguments">Type arguments.</param>
		/// <param name="name">Actual member identifier.</param>
		public static string GetGenericName(this IEnumerable<ITypeSymbol> typeArguments, string? name)
		{
			StringBuilder builder = new();
			typeArguments.GetGenericNameInto(builder, name);
			return builder.ToString();
		}

		/// <summary>
		/// Writes a <see cref="string"/> containing generic identifier combined of the specified <paramref name="name"/> and the collection of <paramref name="typeArguments"/> to the specified <paramref name="builder"/>.
		/// </summary>
		/// <param name="typeArguments">Type arguments.</param>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <param name="name">Actual member identifier.</param>
		public static void GetGenericNameInto(this IEnumerable<ITypeSymbol> typeArguments, StringBuilder builder, string? name)
		{
			if (typeArguments is IEnumerable<ITypeParameterSymbol> parameters)
			{
				parameters.GetGenericNameInto(builder, name);
				return;
			}

			if(!string.IsNullOrWhiteSpace(name))
			{
				builder.Append(name);
			}

			typeArguments.GetGenericNameInto(builder);
		}

		/// <summary>
		/// Creates an <c>&lt;inheritdoc/&gt;</c> tag from the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the <c>&lt;inheritdoc/&gt;</c> tag from.</param>
		/// <param name="forceUnsupported">Determines whether to return the <c>&lt;inheritdoc/&gt;</c> event if it cannot be referenced by other symbols.</param>
		/// <returns>A <see cref="string"/> containing the created <c>&lt;inheritdoc/&gt;</c> tag -or- <see langword="null"/> if <paramref name="symbol"/> has no documentation comment.</returns>
		public static string? GetInheritdocIfHasDocumentation(this ISymbol symbol, bool forceUnsupported = false)
		{
			if (forceUnsupported)
			{
				if (!symbol.HasDocumentation())
				{
					return default;
				}
			}
			else if (!symbol.HasInheritableDocumentation())
			{
				return default;
			}

			return AutoGenerated.GetInheritdoc(symbol.GetXmlParentTypes(true, true));
		}

		/// <summary>
		/// Returns a collection of all inner types of the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to get the inner types of.</param>
		public static IEnumerable<INamedTypeSymbol> GetInnerTypes(this INamedTypeSymbol symbol)
		{
			return GetInnerTypes_Internal(symbol);
		}

		/// <summary>
		/// Returns a collection of all inner types of the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="INamespaceSymbol"/> to get the inner types of.</param>
		public static IEnumerable<INamedTypeSymbol> GetInnerTypes(this INamespaceSymbol symbol)
		{
			return Yield(symbol);

			static IEnumerable<INamedTypeSymbol> Yield(INamespaceSymbol @namespace)
			{
				foreach (INamedTypeSymbol type in @namespace.GetTypeMembers())
				{
					yield return type;

					foreach (INamedTypeSymbol inner in GetInnerTypes_Internal(type))
					{
						yield return inner;
					}
				}

				foreach (INamespaceSymbol n in @namespace.GetNamespaceMembers())
				{
					foreach (INamedTypeSymbol type in Yield(n))
					{
						yield return type;
					}
				}
			}
		}

		/// <summary>
		/// Determines whether the specified <paramref name="event"/> is a field-like <see langword="event"/>.
		/// </summary>
		/// <param name="event"><see cref="IEventSymbol"/> to check if is a field-like <see langword="event"/>.</param>
		public static bool IsFieldEvent(this IEventSymbol @event)
		{
			if(@event.AddMethod is not null && !@event.AddMethod.IsImplicitlyDeclared)
			{
				return false;
			}

			if(@event.RemoveMethod is not null && !@event.RemoveMethod.IsImplicitlyDeclared)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Returns a keyword used to refer to the specified <paramref name="symbol"/> inside an attribute list.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the associated attribute target keyword of.</param>
		/// <param name="targetKind">Determines which keyword to return when there is more than one option (e.g '<see langword="method"/>' and '<see langword="return"/>' for methods).</param>
		/// <exception cref="ArgumentException"><paramref name="symbol"/> does not support attribute targets.</exception>
		public static string GetAttributeTarget(this ISymbol symbol, AttributeTargetKind targetKind = default)
		{
			if(symbol is IMethodSymbol method)
			{
				if(method.MethodKind.GetAttributeTarget(targetKind) is string methodTarget)
				{
					return methodTarget;
				}
			}
			else if(symbol is IEventSymbol @event && !@event.IsFieldEvent())
			{
				return "event";
			}
			else if(symbol.Kind.GetAttributeTarget(targetKind) is string target)
			{
				return target;
			}

			throw new ArgumentException($"Symbol '{symbol}' does not support attribute targets", nameof(symbol));
		}

		/// <summary>
		/// Returns a keyword used to refer to a <see cref="ISymbol"/> of the specified <paramref name="kind"/> inside an attribute list.
		/// </summary>
		/// <param name="kind">Kind of method to get the keyword for.</param>
		/// <param name="targetKind">Determines which keyword to return when there is more than one option.</param>
		public static string? GetAttributeTarget(this SymbolKind kind, AttributeTargetKind targetKind = default)
		{
			return kind switch
			{
				SymbolKind.NamedType => "type",
				SymbolKind.Field => "field",
				SymbolKind.Method => targetKind == AttributeTargetKind.FieldOrReturn ? "return" : "method",
				SymbolKind.Property => targetKind == AttributeTargetKind.FieldOrReturn ? "field" : "property",
				SymbolKind.Event => targetKind switch
				{
					AttributeTargetKind.FieldOrReturn => "field",
					AttributeTargetKind.MethodOrParam => "method",
					_ => "event"
				},
				SymbolKind.TypeParameter => "typevar",
				SymbolKind.Parameter => "param",
				SymbolKind.Assembly => "assembly",
				SymbolKind.NetModule => "module",
				_ => default
			};
		}

		/// <summary>
		/// Returns a keyword used to refer to a <see cref="IMethodSymbol"/> of the specified <paramref name="kind"/> inside an attribute list.
		/// </summary>
		/// <param name="kind">Kind of method to get the keyword for.</param>
		/// <param name="targetKind">Determines which keyword to return when there is more than one option.</param>
		public static string? GetAttributeTarget(this MethodKind kind, AttributeTargetKind targetKind = default)
		{
			return kind switch
			{
				MethodKind.EventAdd or
				MethodKind.EventRemove or
				MethodKind.PropertySet => targetKind switch
				{
					AttributeTargetKind.FieldOrReturn => "return",
					AttributeTargetKind.MethodOrParam => "param",
					_ => "method"
				},
				_ => targetKind == AttributeTargetKind.FieldOrReturn ? "return" : "method"
			};
		}

		/// <summary>
		/// Determines whether the specified <paramref name="method"/> is a constructor (either instance or static).
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to check if is a constructor.</param>
		public static bool IsConstructor(this IMethodSymbol method)
		{
			return method.MethodKind is MethodKind.Constructor or MethodKind.StaticConstructor;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="method"/> is an operator.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to check if is an operator.</param>
		public static bool IsOperator(this IMethodSymbol method)
		{
			return method.MethodKind is MethodKind.Conversion or MethodKind.BuiltinOperator or MethodKind.UserDefinedOperator;
		}

		/// <summary>
		/// Returns a keyword used to declare the specified <paramref name="symbol"/> (e.g. <see langword="class"/>, <see langword="event"/>).
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the keyword of.</param>
		public static string? GetDeclaredKeyword(this ISymbol symbol)
		{
			return symbol switch
			{
				INamedTypeSymbol type => type.GetDeclaredKeyword(),
				IEventSymbol => "event",
				IMethodSymbol method when method.IsOperator() => "operator",
				_ => default
			};
		}

		/// <summary>
		/// Returns a keyword used to declare the specified <paramref name="type"/> (e.g. <see langword="class"/>, <see langword="enum"/>).
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to get the keyword of.</param>
		public static string? GetDeclaredKeyword(this INamedTypeSymbol type)
		{
			return type.TypeKind switch
			{
				TypeKind.Class => type.IsRecord ? "record" : "class",
				TypeKind.Struct => type.IsRecord ? "record struct" : "struct",
				TypeKind.Interface => "interface",
				TypeKind.Enum => "enum",
				TypeKind.Delegate => "delegate",
				_ => default
			};
		}

		/// <summary>
		/// Returns new <see cref="IMemberData"/> created for the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to create the <see cref="IMemberData"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="IMemberData"/> from.</param>
		public static IMemberData GetMemberData(this INamedTypeSymbol type, ICompilationData compilation)
		{
			if (type.IsRecord)
			{
				return new RecordData(type, compilation);
			}

			return type.TypeKind switch
			{
				TypeKind.Class => new ClassData(type, compilation),
				TypeKind.Struct => new StructData(type, compilation),
				TypeKind.Interface => new InterfaceData(type, compilation),
				TypeKind.Delegate => new DelegateData(type, compilation),
				TypeKind.Enum => new EnumData(type, compilation),
				_ => new UnknownTypeData(type, compilation)
			};
		}

		/// <summary>
		/// Returns new <see cref="IMethodData"/> created for the specified <paramref name="method"/>.
		/// </summary>
		/// <param name="method"><see cref="IMethodData"/> to create the <see cref="IMethodSymbol"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="IMethodSymbol"/> from.</param>
		public static IMethodData GetMemberData(this IMethodSymbol method, ICompilationData compilation)
		{
			return method.MethodKind switch
			{
				MethodKind.Ordinary => new MethodData(method, compilation),
				MethodKind.BuiltinOperator or MethodKind.UserDefinedOperator => new OperatorData(method, compilation),
				MethodKind.Constructor or MethodKind.StaticConstructor => new ConstructorData(method, compilation),
				MethodKind.Destructor => new DestructorData(method, compilation),
				MethodKind.LocalFunction => new LocalFunctionData(method, compilation),
				MethodKind.Conversion => new ConversionOperatorData(method, compilation),
				_ => new UnknownMethodData(method, compilation)
			};
		}

		/// <summary>
		/// Returns new <see cref="IMemberData"/> created for the specified <paramref name="field"/>.
		/// </summary>
		/// <param name="field"><see cref="IFieldSymbol"/> to create the <see cref="IMemberData"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="IMemberData"/> from.</param>
		public static IMemberData GetMemberData(this IFieldSymbol field, ICompilationData compilation)
		{
			return new FieldData(field, compilation);
		}

		/// <summary>
		/// Returns new <see cref="IMemberData"/> created for the specified <paramref name="event"/>.
		/// </summary>
		/// <param name="event"><see cref="IEventSymbol"/> to create the <see cref="IMemberData"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="IMemberData"/> from.</param>
		public static IMemberData GetMemberData(this IEventSymbol @event, ICompilationData compilation)
		{
			return new EventData(@event, compilation);
		}

		/// <summary>
		/// Returns new <see cref="IMemberData"/> created for the specified <paramref name="property"/>.
		/// </summary>
		/// <param name="property"><see cref="IPropertySymbol"/> to create the <see cref="IMemberData"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="IMemberData"/> from.</param>
		public static IMemberData GetMemberData(this IPropertySymbol property, ICompilationData compilation)
		{
			if (property.IsIndexer)
			{
				return new IndexerData(property, compilation);
			}

			return new PropertyData(property, compilation);
		}

		/// <summary>
		/// Returns new <see cref="IMemberData"/> created for the specified <paramref name="namespace"/>.
		/// </summary>
		/// <param name="namespace"><see cref="INamespaceSymbol"/> to create the <see cref="IMemberData"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="IMemberData"/> from.</param>
		public static IMemberData GetMembetData(this INamespaceSymbol @namespace, ICompilationData compilation)
		{
			return new NamespaceData(@namespace, compilation);
		}

		/// <summary>
		/// Returns new <see cref="IMemberData"/> created for the specified <paramref name="local"/>.
		/// </summary>
		/// <param name="local"><see cref="ILocalSymbol"/> to create the <see cref="IMemberData"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="IMemberData"/> from.</param>
		public static IMemberData GetMemberData(this ILocalSymbol local, ICompilationData compilation)
		{
			return new LocalData(local, compilation);
		}

		/// <summary>
		/// Returns new <see cref="IMemberData"/> created for the specified <paramref name="typeParameter"/>.
		/// </summary>
		/// <param name="typeParameter"><see cref="ITypeParameterSymbol"/> to create the <see cref="IMemberData"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="IMemberData"/> from.</param>
		public static IMemberData GetMemberData(this ITypeParameterSymbol typeParameter, ICompilationData compilation)
		{
			return new TypeParameterData(typeParameter, compilation);
		}

		/// <summary>
		/// Returns new <see cref="IMemberData"/> created for the specified <paramref name="parameter"/>.
		/// </summary>
		/// <param name="parameter"><see cref="IParameterSymbol"/> to create the <see cref="IMemberData"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="IMemberData"/> from.</param>
		public static IMemberData GetMemberData(this IParameterSymbol parameter, ICompilationData compilation)
		{
			return new ParameterData(parameter, compilation);
		}

		/// <summary>
		/// Returns new <see cref="IMemberData"/> created for the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="INamespaceOrTypeSymbol"/> to create the <see cref="IMemberData"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="IMemberData"/> from.</param>
		public static IMemberData GetMemberData(this INamespaceOrTypeSymbol symbol, ICompilationData compilation)
		{
			if(symbol is INamespaceSymbol @namespace)
			{
				return @namespace.GetMemberData(compilation);
			}

			if(symbol is INamedTypeSymbol type)
			{
				return type.GetMemberData(compilation);
			}

			if(symbol is ITypeParameterSymbol typeParameter)
			{
				return typeParameter.GetMemberData(compilation);
			}

			if(symbol is ITypeSymbol unknownType)
			{
				return new UnknownTypeData(unknownType, compilation);
			}

			return new MemberData(symbol, compilation);
		}

		/// <summary>
		/// Returns new <see cref="IMemberData"/> created for the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to create the <see cref="IMemberData"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="IMemberData"/> from.</param>
		public static IMemberData GetMemberData(this ISymbol symbol, ICompilationData compilation)
		{
			return symbol switch
			{
				ITypeParameterSymbol typeParameter => typeParameter.GetMemberData(compilation),
				INamedTypeSymbol type => type.GetMemberData(compilation),
				IMethodSymbol method => method.GetMemberData(compilation),
				IPropertySymbol property => property.GetMemberData(compilation),
				IFieldSymbol field => field.GetMemberData(compilation),
				IEventSymbol @event => @event.GetMemberData(compilation),
				IParameterSymbol parameter => parameter.GetMemberData(compilation),
				INamespaceSymbol @namespace => @namespace.GetMemberData(compilation),
				ILocalSymbol local => local.GetMemberData(compilation),
				ITypeSymbol unknownType => new UnknownTypeData(unknownType, compilation),
				_ => new MemberData(symbol, compilation)
			};
		}

		/// <summary>
		/// Returns modifiers applied to the target <see cref="INamedTypeSymbol"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to get the modifiers of.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static IEnumerable<SyntaxToken> GetModifiers(this INamedTypeSymbol type, CancellationToken cancellationToken = default)
		{
			return type.DeclaringSyntaxReferences
				.Select(e => e.GetSyntax(cancellationToken))
				.Cast<TypeDeclarationSyntax>()
				.GetModifiers();
		}

		/// <summary>
		/// Returns text of operator this <paramref name="method"/> overloads.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to get the kind of the overloaded operator.</param>
		public static string? GetOperatorText(this IMethodSymbol method)
		{
			if (method.MethodKind != MethodKind.UserDefinedOperator && method.MethodKind != MethodKind.BuiltinOperator)
			{
				return default;
			}

			return AnalysisUtilities.GetOperatorText(method.Name);
		}

		/// <summary>
		/// Returns kind of operator this <paramref name="method"/> overloads.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to get the kind of the overloaded operator.</param>
		public static OverloadableOperator GetOperatorType(this IMethodSymbol method)
		{
			if (method.MethodKind != MethodKind.UserDefinedOperator && method.MethodKind != MethodKind.BuiltinOperator)
			{
				return default;
			}

			return AnalysisUtilities.GetOperatorType(method.Name);
		}

		/// <summary>
		/// Returns the parameterless constructor of the specified <paramref name="type"/> if it has one.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to get the parameterless constructor of.</param>
		public static IMethodSymbol? GetParameterlessConstructor(this INamedTypeSymbol type)
		{
			return type.InstanceConstructors.FirstOrDefault(ctor => ctor.Parameters.Length == 0);
		}

		/// <summary>
		/// Returns a <see cref="string"/> that represents the parameter signature of the <paramref name="method"/>.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to get the signature of.</param>
		/// <param name="substitution">Configures how generic type parameters are substituted.</param>
		public static string GetParameterList(this IMethodSymbol method, GenericSubstitution substitution = default)
		{
			StringBuilder builder = new();
			method.GetParameterListInto(builder, substitution);
			return builder.ToString();
		}

		/// <summary>
		/// Writes a <see cref="string"/> that represents the parameter signature of the <paramref name="method"/> to the specified <paramref name="builder"/>.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to get the signature of.</param>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <param name="substitution">Configures how generic type parameters are substituted.</param>
		public static void GetParameterListInto(this IMethodSymbol method, StringBuilder builder, GenericSubstitution substitution = default)
		{
			ImmutableArray<IParameterSymbol> parameters = substitution.HasFlag(GenericSubstitution.TypeArguments) || method.ConstructedFrom is null
				? method.Parameters
				: method.ConstructedFrom.Parameters;

			CodeBuilder cd = new(builder);

			builder.Append('(');

			if (parameters.Length > 0)
			{
				cd.Parameter(parameters[0]);

				for (int i = 1; i < parameters.Length; i++)
				{
					builder.Append(", ");
					cd.Parameter(parameters[i]);
				}
			}

			builder.Append(')');
		}

		/// <summary>
		/// Returns a <see cref="string"/> that contains all the parent types of the specified <paramref name="symbol"/> and the <paramref name="symbol"/>'s name separated by the dot ('.') character.
		/// </summary>
		/// <remarks>If the <paramref name="symbol"/> is not contained within a type, an empty <see cref="string"/> is returned instead.</remarks>
		/// <param name="symbol"><see cref="ISymbol"/> to get the <see cref="string"/> of.</param>
		/// <param name="includeSelf">Determines whether to include the <paramref name="symbol"/> in the returned <see cref="string"/>.</param>
		/// <param name="includeParameters">If the value of <paramref name="symbol"/> is a <see cref="IMethodSymbol"/>, determines whether to include the method's parameters in the returned <see cref="string"/>.</param>
		public static string GetParentTypesString(this ISymbol symbol, bool includeSelf = true, bool includeParameters = false)
		{
			StringBuilder builder = new();
			symbol.GetParentTypesStringInto(builder, includeSelf, includeParameters);
			return builder.ToString();
		}

		/// <summary>
		/// Writes a <see cref="string"/> that contains all the parent types of the specified <paramref name="symbol"/> and the <paramref name="symbol"/>'s name separated by the dot ('.') character to the specified <paramref name="builder"/>.
		/// </summary>
		/// <remarks>If the <paramref name="symbol"/> is not contained within a type, an empty <see cref="string"/> is returned instead.</remarks>
		/// <param name="symbol"><see cref="ISymbol"/> to get the <see cref="string"/> of.</param>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <param name="includeSelf">Determines whether to include the <paramref name="symbol"/> in the returned <see cref="string"/>.</param>
		/// <param name="includeParameters">If the value of <paramref name="symbol"/> is a <see cref="IMethodSymbol"/>, determines whether to include the method's parameters in the returned <see cref="string"/>.</param>
		public static void GetParentTypesStringInto(this ISymbol symbol, StringBuilder builder, bool includeSelf = true, bool includeParameters = false)
		{
			foreach (INamedTypeSymbol type in symbol.GetContainingTypes())
			{
				type.GetGenericNameInto(builder);
				builder.Append('.');
			}

			if (includeSelf)
			{
				symbol.GetGenericNameInto(builder, includeParameters ? GenericSubstitution.ParameterList : GenericSubstitution.None);
			}
			else if (builder.Length > 0)
			{
				builder.Remove(builder.Length - 1, 1);
			}
		}

		/// <summary>
		/// Returns all <see cref="TypeDeclarationSyntax"/>es of the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to get the <see cref="TypeDeclarationSyntax"/>es of.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static IEnumerable<T> GetPartialDeclarations<T>(this INamedTypeSymbol type, CancellationToken cancellationToken = default) where T : TypeDeclarationSyntax
		{
			return type.DeclaringSyntaxReferences.Select(e => e.GetSyntax(cancellationToken)).OfType<T>();
		}

		/// <summary>
		/// Returns a <see cref="PredefinedTypeSyntax"/> if the specified <paramref name="type"/> is a keyword type, <see langword="null"/> otherwise.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to get the <see cref="PredefinedTypeSyntax"/> for.</param>
		public static PredefinedTypeSyntax? GetPredefineTypeSyntax(this INamedTypeSymbol type)
		{
			if (type.SpecialType == SpecialType.None)
			{
				return default;
			}

			SyntaxKind kind = type.SpecialType switch
			{
				SpecialType.System_Byte => SyntaxKind.ByteKeyword,
				SpecialType.System_Char => SyntaxKind.CharKeyword,
				SpecialType.System_Boolean => SyntaxKind.BoolKeyword,
				SpecialType.System_Decimal => SyntaxKind.DecimalKeyword,
				SpecialType.System_Double => SyntaxKind.DoubleKeyword,
				SpecialType.System_Int16 => SyntaxKind.ShortKeyword,
				SpecialType.System_Int32 => SyntaxKind.IntKeyword,
				SpecialType.System_Int64 => SyntaxKind.LongKeyword,
				SpecialType.System_Object => SyntaxKind.ObjectKeyword,
				SpecialType.System_SByte => SyntaxKind.SByteKeyword,
				SpecialType.System_Single => SyntaxKind.FloatKeyword,
				SpecialType.System_String => SyntaxKind.StringKeyword,
				SpecialType.System_UInt16 => SyntaxKind.UShortKeyword,
				SpecialType.System_UInt32 => SyntaxKind.UIntKeyword,
				SpecialType.System_UInt64 => SyntaxKind.ULongKeyword,
				SpecialType.System_Void => SyntaxKind.VoidKeyword,
				_ => default
			};

			if (kind == default)
			{
				return default;
			}

			return SyntaxFactory.PredefinedType(SyntaxFactory.Token(kind));
		}

		/// <summary>
		/// Returns root namespace of the <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the root namespaces of.</param>
		/// <param name="includeGlobal">Determines whether to return the global namespace as well.</param>
		/// <returns>The root <see cref="INamespaceSymbol"/> -or- <see langword="null"/> if root <see cref="INamespaceSymbol"/> was not found.</returns>
		public static INamespaceSymbol? GetRootNamespace(this ISymbol symbol, bool includeGlobal = false)
		{
			return GetContainingNamespaces(symbol, includeGlobal).FirstOrDefault();
		}

		/// <summary>
		/// Returns a <see cref="CSharpSyntaxNode"/> of type <typeparamref name="T"/> associated with the specified <paramref name="method"/>.
		/// </summary>
		/// <typeparam name="T">Type of <see cref="CSharpSyntaxNode"/> to return.</typeparam>
		/// <param name="method"><see cref="IMethodSymbol"/> to get the <see cref="CSharpSyntaxNode"/> associated with.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <exception cref="InvalidOperationException"><paramref name="method"/> is not associated with a syntax node of type <typeparamref name="T"/>.</exception>
		public static T? GetSyntax<T>(this IMethodSymbol method, CancellationToken cancellationToken = default) where T : CSharpSyntaxNode
		{
			if (!method.TryGetSyntax<T>(out T? declaration, cancellationToken))
			{
				throw new InvalidOperationException($"Method '{method}' is not associated with a syntax node of type '{typeof(T).Name}'");
			}

			return declaration;
		}

		/// <summary>
		/// Returns a <see cref="MethodDeclarationSyntax"/> associated with the specified <paramref name="method"/>.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to get the <see cref="MethodDeclarationSyntax"/> associated with.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <exception cref="InvalidOperationException"><paramref name="method"/> is not associated with a <see cref="MethodDeclarationSyntax"/>.</exception>
		public static MethodDeclarationSyntax? GetSyntax(this IMethodSymbol method, CancellationToken cancellationToken = default)
		{
			return method.GetSyntax<MethodDeclarationSyntax>(cancellationToken);
		}

		/// <summary>
		/// Returns a new <see cref="UsingDirectiveSyntax"/> build for the specified <paramref name="namespace"/> symbol.
		/// </summary>
		/// <param name="namespace"><see cref="INamespaceSymbol"/> to built the <see cref="UsingDirectiveSyntax"/> from.</param>
		public static UsingDirectiveSyntax GetUsingDirective(this INamespaceSymbol @namespace)
		{
			NameSyntax name;

			if (@namespace.GetContainingNamespaces().JoinIntoQualifiedName() is QualifiedNameSyntax q)
			{
				name = q;
			}
			else
			{
				name = SyntaxFactory.IdentifierName(@namespace.Name);
			}

			return SyntaxFactory.UsingDirective(name);
		}

		/// <summary>
		/// Returns a new <see cref="UsingDirectiveSyntax"/> build for the specified <paramref name="namespaces"/> symbol.
		/// </summary>
		/// <param name="namespaces">A collection of <see cref="INamespaceSymbol"/>s to build the <see cref="UsingDirectiveSyntax"/> from.</param>
		/// <exception cref="ArgumentException"><paramref name="namespaces"/> cannot be empty.</exception>
		public static UsingDirectiveSyntax GetUsingDirective(this IEnumerable<INamespaceSymbol> namespaces)
		{
			NameSyntax name;

			if (namespaces.JoinIntoQualifiedName() is QualifiedNameSyntax q)
			{
				name = q;
			}
			else if (namespaces.FirstOrDefault() is INamespaceSymbol first)
			{
				name = SyntaxFactory.IdentifierName(first.Name);
			}
			else
			{
				throw new ArgumentException($"'{nameof(namespaces)}' cannot be empty");
			}

			return SyntaxFactory.UsingDirective(name);
		}

		/// <summary>
		/// Returns name of the specified <paramref name="symbol"/> compatible with 'inheritdoc' tags.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the compatible name of.</param>
		/// <param name="includeParameters">If the value of <paramref name="symbol"/> is a <see cref="IMethodSymbol"/>, determines whether to include the method's parameters in the returned <see cref="string"/>.</param>
		public static string GetXmlCompatibleName(this ISymbol symbol, bool includeParameters = false)
		{
			return symbol switch
			{
				IPropertySymbol property => property.GetXmlCompatibleName(),
				IMethodSymbol method => method.GetXmlCompatibleName(includeParameters),
				_ => AnalysisUtilities.ToXmlCompatible(symbol.GetGenericName(includeParameters ? GenericSubstitution.ParameterList : GenericSubstitution.None)),
			};
		}

		/// <summary>
		/// Returns name of the specified <paramref name="property"/> compatible with 'inheritdoc' tags.
		/// </summary>
		public static string GetXmlCompatibleName(this IPropertySymbol property)
		{
			if (property.IsIndexer)
			{
				return "this";
			}

			return property.Name;
		}

		/// <summary>
		/// Returns name of the specified <paramref name="method"/> compatible with 'inheritdoc' tags.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to get the compatible name of.</param>
		/// <param name="includeParameters">If the value of <paramref name="method"/> is a <see cref="IMethodSymbol"/>, determines whether to include the method's parameters in the returned <see cref="string"/>.</param>
		public static string GetXmlCompatibleName(this IMethodSymbol method, bool includeParameters = false)
		{
			string name;

			switch (method.MethodKind)
			{
				case MethodKind.Constructor:
					name = method.ContainingType.Name;
					break;

				case MethodKind.UserDefinedOperator:
					name = $"operator {method.GetOperatorText()}";
					break;

				case MethodKind.Conversion:

					if (method.IsImplicitOperator())
					{
						name = $"implicit operator {method.ReturnType.GetXmlFullyQualifiedName()}";
					}
					else if (method.IsExplicitOperator())
					{
						name = $"explicit operator {method.ReturnType.GetXmlFullyQualifiedName()}";
					}
					else
					{
						goto default;
					}

					break;

				default:
					name = AnalysisUtilities.ToXmlCompatible(method.GetGenericName());
					break;
			}

			if (includeParameters)
			{
				name += method.GetXmlParameterList();
			}

			return name;
		}

		/// <summary>
		/// Returns a <see cref="string"/> representing the fully qualified name of the <paramref name="symbol"/> that can be used in the XML documentation.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the fully qualified name of.</param>
		public static string GetXmlFullyQualifiedName(this ISymbol symbol)
		{
			return AnalysisUtilities.ToXmlCompatible(symbol.ToString());
		}

		/// <summary>
		/// Returns a <see cref="string"/> that represents the parameter signature of the <paramref name="method"/> compatible with 'inheritdoc' tags.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to get the signature of.</param>
		/// <param name="substitution">Configures how generic type parameters are substituted.</param>
		public static string GetXmlParameterList(this IMethodSymbol method, GenericSubstitution substitution = default)
		{
			string parameterList = method.GetParameterList(substitution);

			return AnalysisUtilities.ToXmlCompatible(parameterList);
		}

		/// <summary>
		/// Returns a <see cref="string"/> that contains all the parent types of the specified <paramref name="symbol"/> and the <paramref name="symbol"/>'s separated by the dot ('.') character. Can be used in XML documentation.
		/// </summary>
		/// <param name="symbol"><see cref="IMemberData"/> to get the <see cref="string"/> of.</param>
		/// <param name="includeSelf">Determines whether to include the <paramref name="symbol"/> in the returned <see cref="string"/>.</param>
		/// <param name="includeParameters">If the value of <paramref name="symbol"/> is a <see cref="IMethodSymbol"/>, determines whether to include the method's parameters in the returned <see cref="string"/>.</param>
		public static string GetXmlParentTypes(this ISymbol symbol, bool includeSelf = true, bool includeParameters = false)
		{
			StringBuilder builder = new();
			symbol.GetXmlParentTypesInto(builder, includeSelf, includeParameters);
			return builder.ToString();
		}

		/// <summary>
		/// Writes a <see cref="string"/> that contains all the parent types of the specified <paramref name="symbol"/> and the <paramref name="symbol"/>'s separated by the dot ('.') character to the specified <paramref name="builder"/>. Can be used in XML documentation.
		/// </summary>
		/// <param name="symbol"><see cref="IMemberData"/> to get the <see cref="string"/> of.</param>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <param name="includeSelf">Determines whether to include the <paramref name="symbol"/> in the returned <see cref="string"/>.</param>
		/// <param name="includeParameters">If the value of <paramref name="symbol"/> is a <see cref="IMethodSymbol"/>, determines whether to include the method's parameters in the returned <see cref="string"/>.</param>
		public static void GetXmlParentTypesInto(this ISymbol symbol, StringBuilder builder, bool includeSelf = true, bool includeParameters = false)
		{
			foreach (INamedTypeSymbol type in symbol.GetContainingTypes())
			{
				builder.Append(AnalysisUtilities.ToXmlCompatible(type.GetGenericName())).Append('.');
			}

			if (includeSelf)
			{
				builder.Append(symbol.GetXmlCompatibleName(includeParameters));
			}
			else if (builder.Length > 0)
			{
				builder.Remove(builder.Length - 1, 1);
			}
		}

		/// <summary>
		/// Checks if an attribute of type <paramref name="attrSymbol"/> is defined on the target <paramref name="symbol"/>
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to check if contains the specified attribute.</param>
		/// <param name="attrSymbol"><see cref="INamedTypeSymbol"/> of attribute to check for.</param>
		public static bool HasAttribute(this ISymbol symbol, INamedTypeSymbol attrSymbol)
		{
			return GetAttribute(symbol, attrSymbol) is not null;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> has at least one generic constraint.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check if has at least one generic constraint.</param>
		public static bool HasConstraint(this INamedTypeSymbol type)
		{
			return type.IsGenericType && type.TypeParameters.Any(p => p.HasConstraint());
		}

		/// <summary>
		/// Determines whether the specified <paramref name="method"/> has at least one generic constraint.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to check if has at least one generic constraint.</param>
		public static bool HasConstraint(this IMethodSymbol method)
		{
			return method.IsGenericMethod && method.TypeParameters.Any(p => p.HasConstraint());
		}

		/// <summary>
		/// Determines whether the specified <paramref name="parameter"/> has at least one generic constraint.
		/// </summary>
		/// <param name="parameter"><see cref="ITypeParameterSymbol"/> to check if has at least one generic constraint.</param>
		public static bool HasConstraint(this ITypeParameterSymbol parameter)
		{
			return
				parameter.HasConstructorConstraint ||
				parameter.HasValueTypeConstraint ||
				parameter.HasReferenceTypeConstraint ||
				parameter.HasUnmanagedTypeConstraint ||
				parameter.HasNotNullConstraint ||
				parameter.ConstraintTypes.Length > 0;
		}

		/// <summary>
		/// Determines whether the given <paramref name="parameter"/> has a specific <paramref name="constraint"/> applied.
		/// </summary>
		/// <param name="parameter"><see cref="ITypeParameterSymbol"/> to check if has a specific <paramref name="constraint"/> applied.</param>
		/// <param name="constraint"><see cref="GenericConstraint"/> to check if is applied to the <paramref name="parameter"/>.</param>
		/// <param name="strict">Determines whether to only include constraints that are explicitly applied to the <paramref name="parameter"/>.</param>
		public static bool HasConstraint(this ITypeParameterSymbol parameter, GenericConstraint constraint, bool strict = true)
		{
			switch (constraint)
			{
				case GenericConstraint.Class:

					if (parameter.HasReferenceTypeConstraint)
					{
						return true;
					}

					if (strict)
					{
						return false;
					}

					return parameter.ConstraintTypes.Any(type =>
					{
						if (type.TypeKind == TypeKind.Class)
						{
							return true;
						}

						if (type.TypeKind == TypeKind.TypeParameter)
						{
							return (type as ITypeParameterSymbol)?.HasConstraint(GenericConstraint.Class, false) ?? false;
						}

						return true;
					});

				case GenericConstraint.Struct:

					if (parameter.HasValueTypeConstraint)
					{
						return true;
					}

					return !strict && parameter.HasUnmanagedTypeConstraint;

				case GenericConstraint.Unmanaged:
					return parameter.HasUnmanagedTypeConstraint;

				case GenericConstraint.Type:
					return parameter.ConstraintTypes.Length > 0;

				case GenericConstraint.NotNull:
					return parameter.HasNotNullConstraint;

				case GenericConstraint.New:

					if (parameter.HasConstructorConstraint)
					{
						return true;
					}

					return !strict && (parameter.HasConstructorConstraint || parameter.HasUnmanagedTypeConstraint);

				//case GenericConstraint.Default:
				//	break;

				case GenericConstraint.None:
					return !parameter.HasConstraint();

				default:
					return false;
			}
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> has a documentation.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to check if has documentation.</param>
		public static bool HasDocumentation(this ISymbol symbol)
		{
			return !string.IsNullOrEmpty(symbol.GetDocumentationCommentXml());
		}

		/// <summary>
		/// Determines whether the <paramref name="first"/> <see cref="IMethodSymbol"/> has equivalent parameters to the <paramref name="second"/> <see cref="IMethodSymbol"/>.
		/// </summary>
		/// <param name="first">First <see cref="IMethodSymbol"/>.</param>
		/// <param name="second">Second <see cref="IMethodSymbol"/>.</param>
		public static bool HasEquivalentParameters(this IMethodSymbol first, IMethodSymbol second)
		{
			ImmutableArray<IParameterSymbol> firstParameters = first.Parameters;
			ImmutableArray<IParameterSymbol> secondParameters = second.Parameters;

			if (firstParameters.Length != secondParameters.Length)
			{
				return false;
			}

			for (int i = 0; i < firstParameters.Length; i++)
			{
				if (!IsEquivalentTo(firstParameters[i], secondParameters[i]))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> has an explicitly specified base type.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check if has an explicit base type.</param>
		/// <param name="compilation"><see cref="CSharpCompilation"/> that provides a <see cref="INamedTypeSymbol"/> for the <see langword="object"/> type.</param>
		public static bool HasExplicitBaseType(this INamedTypeSymbol type, CSharpCompilation compilation)
		{
			if (type.BaseType is null || type.TypeKind != TypeKind.Class)
			{
				return false;
			}

			return !SymbolEqualityComparer.Default.Equals(type.BaseType, compilation.ObjectType);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="method"/> has an implementation in code.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to check if has implementation.</param>
		public static bool HasImplementation(this IMethodSymbol method)
		{
			return !(method.IsExtern || method.IsAbstract || method.IsImplicitlyDeclared || method.IsPartialDefinition);
		}

		/// <summary>
		/// Determines whether the documentation of the specified <paramref name="symbol"/> can be references in an 'inheritdoc' tag.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to check whether has inheritable documentation.</param>
		public static bool HasInheritableDocumentation(this ISymbol symbol)
		{
			bool canHaveDocumentation = symbol switch
			{
				IMethodSymbol method => method.MethodKind is
					MethodKind.Ordinary or
					MethodKind.Constructor or
					MethodKind.Destructor or
					MethodKind.Conversion or
					MethodKind.UserDefinedOperator,

				INamedTypeSymbol type => type.TypeKind is
					TypeKind.Class or
					TypeKind.Struct or
					TypeKind.Delegate or
					TypeKind.Enum or
					TypeKind.Interface,

				_ => symbol is
					IPropertySymbol or
					IFieldSymbol or
					IEventSymbol
			};

			return canHaveDocumentation && symbol.HasDocumentation();
		}

		/// <summary>
		/// Determines whether the target <paramref name="type"/> inherits the <paramref name="baseType"/>.
		/// </summary>
		/// <param name="type">Type to check if inherits the <paramref name="baseType"/>.</param>
		/// <param name="baseType">Base type to check if is inherited by the target <paramref name="type"/>.</param>
		/// <param name="toReturnIfSame">Determines what to return when the <paramref name="type"/> and <paramref name="baseType"/> are the same.</param>
		public static bool InheritsFrom(this ITypeSymbol type, ITypeSymbol baseType, bool toReturnIfSame = true)
		{
			if (SymbolEqualityComparer.Default.Equals(type, baseType))
			{
				return toReturnIfSame;
			}

			if (baseType.TypeKind == TypeKind.Interface)
			{
				if (type.AllInterfaces.IsDefaultOrEmpty)
				{
					return false;
				}

				foreach (INamedTypeSymbol intf in type.AllInterfaces)
				{
					if (SymbolEqualityComparer.Default.Equals(baseType, intf))
					{
						return true;
					}
				}
			}
			else
			{
				INamedTypeSymbol? current = type.BaseType;

				while (current is not null)
				{
					if (SymbolEqualityComparer.Default.Equals(current, baseType))
					{
						return true;
					}

					current = current.BaseType;
				}
			}

			return false;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="method"/> is an accessor of the given <paramref name="event"/>.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to check if is an accessor of the given <paramref name="event"/>.</param>
		/// <param name="event"><see cref="IEventSymbol"/> to check if the <paramref name="method"/> is an accessor of.</param>
		public static bool IsAccessor(this IMethodSymbol method, IEventSymbol @event)
		{
			return SymbolEqualityComparer.Default.Equals(method.AssociatedSymbol, @event);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="method"/> is an accessor of the given <paramref name="property"/>.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to check if is an accessor of the given <paramref name="property"/>.</param>
		/// <param name="property"><see cref="IEventSymbol"/> to check if the <paramref name="method"/> is an accessor of.</param>
		public static bool IsAccessor(this IMethodSymbol method, IPropertySymbol property)
		{
			return SymbolEqualityComparer.Default.Equals(method.AssociatedSymbol, property);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="method"/> is a property or event accessor.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to check if is a property or event accessor.</param>
		public static bool IsAccessor(this IMethodSymbol method)
		{
			return method.AssociatedSymbol is not null && method.AssociatedSymbol is IPropertySymbol or IEventSymbol;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> is an attribute type.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is an attribute type.</param>
		/// <param name="compilation"><see cref="CSharpCompilation"/> that is used to resolve the <see cref="Attribute"/> type.</param>
		/// <exception cref="InvalidOperationException">Type '<see cref="Attribute"/>' could not be resolved.</exception>
		public static bool IsAttribute(this INamedTypeSymbol symbol, CSharpCompilation compilation)
		{
			if (compilation.GetTypeByMetadataName("System.Attribute") is not INamedTypeSymbol attr)
			{
				throw new InvalidOperationException("Type 'System.Attribute' could not be resolved!");
			}

			return symbol.InheritsFrom(attr);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="property"/> is auto-implemented.
		/// </summary>
		/// <param name="property"><see cref="IPropertySymbol"/> to check if is auto-implemented.</param>
		public static bool IsAutoProperty(this IPropertySymbol property)
		{
			return !property.IsIndexer && property.GetBackingField() is not null;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="method"/> is an accessor of an auto-implemented property.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to check if is an accessor of an auto-implemented property.</param>
		public static bool IsAutoPropertyAccessor(this IMethodSymbol method)
		{
			return method.AssociatedSymbol is IPropertySymbol property && property.GetBackingField() is not null;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="field"/> is a backing field of a property.
		/// </summary>
		/// <param name="field"><see cref="IFieldSymbol"/> to check if is a backing field.</param>
		public static bool IsBackingField(this IFieldSymbol field)
		{
			return field.AssociatedSymbol is not null;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="field"/> is a backing field of the given <paramref name="property"/>.
		/// </summary>
		/// <param name="field"><see cref="IFieldSymbol"/> to check if is a backing field of the given <paramref name="property"/>.</param>
		/// <param name="property"><see cref="IParameterSymbol"/> to check if the specified <paramref name="field"/> is a backing field of.</param>
		public static bool IsBackingField(this IFieldSymbol field, IPropertySymbol property)
		{
			return SymbolEqualityComparer.Default.Equals(field.AssociatedSymbol, property);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is constructed from a generic type.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check if is constructed from a generic type.</param>
		public static bool IsConstructed(this INamedTypeSymbol type)
		{
			return type.ConstructedFrom is not null && !SymbolEqualityComparer.Default.Equals(type, type.ConstructedFrom);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="method"/> is constructed from a generic type.
		/// </summary>
		/// <param name="method"><see cref="INamedTypeSymbol"/> to check if is constructed from a generic type.</param>
		public static bool IsConstructed(this IMethodSymbol method)
		{
			return method.ConstructedFrom is not null && !SymbolEqualityComparer.Default.Equals(method, method.ConstructedFrom);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> was constructed from the <paramref name="target"/> type.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check if is constructed from the <paramref name="target"/> type.</param>
		/// <param name="target"><see cref="INamedTypeSymbol"/> to check if the <paramref name="type"/> is constructed from.</param>
		public static bool IsConstructedFrom(this INamedTypeSymbol type, INamedTypeSymbol target)
		{
			return type.ConstructedFrom is not null && SymbolEqualityComparer.Default.Equals(type.ConstructedFrom, target);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="method"/> was constructed from the <paramref name="target"/> method.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to check if is constructed from the <paramref name="target"/> method.</param>
		/// <param name="target"><see cref="IMethodSymbol"/> to check if the <paramref name="method"/> is constructed from.</param>
		public static bool IsConstructedFrom(this IMethodSymbol method, IMethodSymbol target)
		{
			return method.ConstructedFrom is not null && SymbolEqualityComparer.Default.Equals(method.ConstructedFrom, target);
		}

		/// <summary>
		/// Determines whether the <paramref name="first"/> <see cref="IParameterSymbol"/> is equivalent to the <paramref name="second"/> <see cref="IParameterSymbol"/>.
		/// </summary>
		/// <param name="first">First <see cref="IParameterSymbol"/>.</param>
		/// <param name="second">Second <see cref="IParameterSymbol"/>.</param>
		public static bool IsEquivalentTo(this IParameterSymbol first, IParameterSymbol second)
		{
			if (AnalysisUtilities.IsValidRefKindForOverload(first.RefKind, second.RefKind))
			{
				return false;
			}

			if (!SymbolEqualityComparer.Default.Equals(first.Type, second.Type))
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="method"/> is an event accessor.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to check if is an event accessor.</param>
		public static bool IsEventAccessor(this IMethodSymbol method)
		{
			return method.AssociatedSymbol is IEventSymbol;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> is an exception type.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is an exception type.</param>
		/// <param name="compilation"><see cref="CSharpCompilation"/> that is used to resolve the <see cref="Exception"/> type.</param>
		/// <exception cref="InvalidOperationException">Type '<see cref="Exception"/>' could not be resolved.</exception>
		public static bool IsException(this INamedTypeSymbol symbol, CSharpCompilation compilation)
		{
			if (compilation.GetTypeByMetadataName("System.Exception") is not INamedTypeSymbol exc)
			{
				throw new InvalidOperationException("Type 'System.Exception' could not be resolved!");
			}

			return symbol.InheritsFrom(exc);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="method"/> is an explicit conversion operator.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to check if is an explicit conversion operator.</param>
		public static bool IsExplicitOperator(this IMethodSymbol method)
		{
			return method.MethodKind == MethodKind.Conversion && method.Name == "op_Explicit";
		}

		/// <summary>
		/// Determines whether the <paramref name="symbol"/> was generated from the <paramref name="target"/> <see cref="ISymbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to check.</param>
		/// <param name="target"><see cref="ISymbol"/> to check if the <paramref name="symbol"/> is generated from.</param>
		/// <param name="compilation"><see cref="CompilationWithEssentialSymbols"/> to get the needed <see cref="INamedTypeSymbol"/> from.</param>
		/// <exception cref="InvalidOperationException">Target <paramref name="compilation"/> has errors.</exception>
		public static bool IsGeneratedFrom(this ISymbol symbol, ISymbol target, CompilationWithEssentialSymbols compilation)
		{
			return IsGeneratedFrom(symbol, target?.ToString()!, compilation);
		}

		/// <summary>
		/// Determines whether the <paramref name="symbol"/> was generated from the <paramref name="target"/> member.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to check.</param>
		/// <param name="target"><see cref="string"/> representing a <see cref="ISymbol"/> to check if the <paramref name="symbol"/> was generated from.</param>
		/// <param name="compilation"><see cref="CompilationWithEssentialSymbols"/> to get the needed <see cref="INamedTypeSymbol"/> from.</param>
		public static bool IsGeneratedFrom(this ISymbol symbol, string target, CompilationWithEssentialSymbols compilation)
		{
			AttributeData? attribute = symbol.GetAttribute(compilation.DurianGeneratedAttribute!);

			if (attribute is null)
			{
				return false;
			}

			return attribute.ConstructorArguments.FirstOrDefault().Value is string value && value == target;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> is generic.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to check if is generic.</param>
		public static bool IsGeneric(this ISymbol symbol)
		{
			return symbol switch
			{
				IMethodSymbol method => method.IsGenericMethod,
				INamedTypeSymbol type => type.IsGenericType,
				_ => false
			};
		}

		/// <summary>
		/// Determines whether the specified <paramref name="method"/> is an implicit conversion operator.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to check if is an implicit conversion operator.</param>
		public static bool IsImplicitOperator(this IMethodSymbol method)
		{
			return method.MethodKind == MethodKind.Conversion && method.Name == "op_Implicit";
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is an inner type.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check if is an inner type.</param>
		public static bool IsInnerType(this INamedTypeSymbol type)
		{
			return type.ContainingType is not null;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is the <paramref name="typeParameter"/> or if it uses it as its element type (for <see cref="IArrayTypeSymbol"/>) or pointed at type (for <see cref="IPointerTypeSymbol"/>).
		/// </summary>
		/// <param name="type"><see cref="ITypeSymbol"/> to check.</param>
		/// <param name="typeParameter"><see cref="ITypeParameterSymbol"/> to check if is used by the target <paramref name="type"/>.</param>
		public static bool IsOrUsesTypeParameter(this ITypeSymbol type, ITypeParameterSymbol typeParameter)
		{
			if (SymbolEqualityComparer.Default.Equals(type, typeParameter))
			{
				return true;
			}

			ITypeSymbol symbol;

			if (type is IArrayTypeSymbol array)
			{
				symbol = array.GetEffectiveElementType();
			}
			else if (type is IPointerTypeSymbol pointer)
			{
				symbol = pointer.GetEffectivePointerAtType();
			}
			else
			{
				return false;
			}

			if (SymbolEqualityComparer.Default.Equals(symbol, typeParameter))
			{
				return true;
			}

			if (symbol is INamedTypeSymbol t && t.Arity > 0)
			{
				foreach (ITypeSymbol s in t.TypeArguments)
				{
					if (IsOrUsesTypeParameter(s, typeParameter))
					{
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is <see langword="partial"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check if is <see langword="partial"/>.</param>
		public static bool IsPartial(this INamedTypeSymbol type)
		{
			if (type.Locations.Length > 1)
			{
				return true;
			}

			if (type.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is TypeDeclarationSyntax syntax)
			{
				return syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
			}

			return false;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="method"/> is <see langword="partial"/>.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to check if is <see langword="partial"/>.</param>
		/// <param name="declarationRetriever">Function that returns a <see cref="MethodDeclarationSyntax"/> of the specified <paramref name="method"/> if it is needed.</param>
		public static bool IsPartial(this IMethodSymbol method, Func<MethodDeclarationSyntax?> declarationRetriever)
		{
			if (method.MethodKind != MethodKind.Ordinary)
			{
				return false;
			}

			if (method.IsPartialDefinition)
			{
				return true;
			}

			if (method.MethodKind != MethodKind.Ordinary)
			{
				return false;
			}

			if (method.DeclaringSyntaxReferences.Length > 1 ||
				method.PartialImplementationPart is not null ||
				method.PartialDefinitionPart is not null)
			{
				return true;
			}

			if (declarationRetriever() is MethodDeclarationSyntax declaration)
			{
				return declaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
			}

			return false;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="method"/> is <see langword="partial"/>.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to check if is <see langword="partial"/>.</param>
		/// <param name="declaration">Main declaration of this <paramref name="method"/>.</param>
		/// <remarks>If <paramref name="declaration"/> is <see langword="null"/>, a <see cref="MethodDeclarationSyntax"/> is retrieved using the <see cref="ISymbol.DeclaringSyntaxReferences"/> property.</remarks>
		public static bool IsPartial(this IMethodSymbol method, MethodDeclarationSyntax? declaration = default)
		{
			return method.IsPartial(() => declaration ?? method?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as MethodDeclarationSyntax);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is <see langword="partial"/> and all its containing types are also <see langword="partial"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check if is <see langword="partial"/>.</param>
		public static bool IsPartialContext(this INamedTypeSymbol type)
		{
			return type.IsPartial() && type.GetContainingTypes().All(t => t.IsPartial());
		}

		/// <summary>
		/// Determines whether the <paramref name="method"/> is <see langword="partial"/> and all its containing types are also <see langword="partial"/>.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to check.</param>
		/// <param name="declaration">Main declaration of this <paramref name="method"/>.</param>
		/// <remarks>If <paramref name="declaration"/> is <see langword="null"/>, a <see cref="MethodDeclarationSyntax"/> is retrieved using the <see cref="ISymbol.DeclaringSyntaxReferences"/> property.</remarks>
		public static bool IsPartialContext(this IMethodSymbol method, MethodDeclarationSyntax? declaration = default)
		{
			return method.IsPartial(declaration) && method.GetContainingTypes().All(t => t.IsPartial());
		}

		/// <summary>
		/// Determines whether the specified <paramref name="method"/> is <see langword="partial"/> and all its containing types are also <see langword="partial"/>..
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to check if is <see langword="partial"/>.</param>
		/// <param name="declarationRetriever">Function that returns a <see cref="MethodDeclarationSyntax"/> of the specified <paramref name="method"/> if it is needed.</param>
		public static bool IsPartialContext(this IMethodSymbol method, Func<MethodDeclarationSyntax?> declarationRetriever)
		{
			return method.IsPartial(declarationRetriever) && method.GetContainingTypes().All(t => t.IsPartial());
		}

		/// <summary>
		/// Determines whether the <paramref name="type"/> is a predefined type (any primitive, <see cref="string"/>, <see cref="void"/>, <see cref="object"/>).
		/// </summary>
		/// <param name="type">Type to check.</param>
		public static bool IsPredefined(this ITypeSymbol type)
		{
			if (type.SpecialType == SpecialType.None)
			{
				return false;
			}

			return
				type.SpecialType is
				SpecialType.System_Void or
				SpecialType.System_String or
				SpecialType.System_Int32 or
				SpecialType.System_Int64 or
				SpecialType.System_Boolean or
				SpecialType.System_Single or
				SpecialType.System_Double or
				SpecialType.System_Decimal or
				SpecialType.System_Char or
				SpecialType.System_Int16 or
				SpecialType.System_Byte or
				SpecialType.System_UInt16 or
				SpecialType.System_UInt32 or
				SpecialType.System_UInt64 or
				SpecialType.System_SByte or
				SpecialType.System_Object;
		}

		/// <summary>
		/// Determines whether the <paramref name="type"/> is a predefined type (any primitive, <see cref="string"/>, <see cref="void"/>, <see cref="object"/>) or a dynamic type.
		/// </summary>
		/// <param name="type">Type to check.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to get the dynamic symbol from.</param>
		/// <returns><see langword="true"/> if the type is predefined or dynamic, otherwise <see langword="false"/>.</returns>
		public static bool IsPredefinedOrDynamic(this ITypeSymbol type, ICompilationData compilation)
		{
			return IsPredefined(type) || SymbolEqualityComparer.Default.Equals(type, compilation.Compilation.DynamicType);
		}

		/// <summary>
		/// Determines whether the <paramref name="type"/> is a predefined type (any primitive, <see cref="string"/>, <see cref="void"/>, <see cref="object"/>) or a dynamic type.
		/// </summary>
		/// <param name="type">Type to check.</param>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the dynamic symbol from.</param>
		/// <returns><see langword="true"/> if the type is predefined or dynamic, otherwise <see langword="false"/>.</returns>
		public static bool IsPredefinedOrDynamic(this ITypeSymbol type, CSharpCompilation compilation)
		{
			return IsPredefined(type) || SymbolEqualityComparer.Default.Equals(type, compilation.DynamicType);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="method"/> is a property accessor.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to check if is a property accessor.</param>
		public static bool IsPropertyAccessor(this IMethodSymbol method)
		{
			return method.AssociatedSymbol is IPropertySymbol;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is of sealed kind (struct or sealed/static class).
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check if is sealed.</param>
		public static bool IsSealedKind(this INamedTypeSymbol type)
		{
			return type.IsSealed || type.IsValueType || type.IsStatic;
		}

		/// <summary>
		/// Determines whether the <paramref name="type"/> can be applied to the <paramref name="parameter"/>.
		/// </summary>
		/// <param name="type"><see cref="ITypeSymbol"/> to check if is valid for the <paramref name="parameter"/>.</param>
		/// <param name="parameter">Target <see cref="ITypeParameterSymbol"/>.</param>
		/// <remarks>Symbols other than <see cref="INamedTypeSymbol"/>, <see cref="IArrayTypeSymbol"/>, <see cref="ITypeParameterSymbol"/> and <see cref="IDynamicTypeSymbol"/> will never be valid.</remarks>
		public static bool IsValidForTypeParameter(this ITypeSymbol type, ITypeParameterSymbol parameter)
		{
			if (type is not INamedTypeSymbol and not IArrayTypeSymbol and not ITypeParameterSymbol and not IDynamicTypeSymbol)
			{
				return false;
			}

			if (type.IsStatic || type.IsRefLikeType || type is IErrorTypeSymbol)
			{
				return false;
			}

			if (type is INamedTypeSymbol s && s.IsUnboundGenericType)
			{
				return false;
			}

			if (parameter.HasReferenceTypeConstraint)
			{
				if (!type.IsReferenceType)
				{
					return false;
				}
			}
			else if (parameter.HasUnmanagedTypeConstraint)
			{
				if (!type.IsUnmanagedType)
				{
					return false;
				}
			}
			else if (parameter.HasValueTypeConstraint)
			{
				if (!type.IsValueType)
				{
					return false;
				}
			}

			if (parameter.HasConstructorConstraint)
			{
				if (type is INamedTypeSymbol n)
				{
					if (!n.InstanceConstructors.Any(ctor => ctor.Parameters.Length == 0 && ctor.DeclaredAccessibility == Accessibility.Public))
					{
						return false;
					}
				}
				else if (type is not IDynamicTypeSymbol)
				{
					return false;
				}
			}

			foreach (ITypeSymbol t in parameter.ConstraintTypes)
			{
				if (t is ITypeParameterSymbol p)
				{
					if (!IsValidForTypeParameter(type, p))
					{
						return false;
					}
				}
				else if (!InheritsFrom(type, t))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Returns a <see cref="QualifiedNameSyntax"/> created from the specified <paramref name="namespaces"/>.
		/// </summary>
		/// <param name="namespaces">A collection of <see cref="INamespaceSymbol"/>s to create the <see cref="QualifiedNameSyntax"/> from.</param>
		/// <returns>A <see cref="QualifiedNameSyntax"/> created by combining the <paramref name="namespaces"/>. -or- <see langword="null"/> if there were less then 2 <paramref name="namespaces"/> provided.</returns>
		public static QualifiedNameSyntax? JoinIntoQualifiedName(this IEnumerable<INamespaceSymbol> namespaces)
		{
			return AnalysisUtilities.GetQualifiedName(namespaces.Select(n => n.Name));
		}

		/// <summary>
		/// Returns a <see cref="string"/> that is created by joining the names of the namespaces the provided <paramref name="symbol"/> is contained in.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the containing namespaces of.</param>
		/// <param name="includeSelf">Determines whether to include name of the <paramref name="symbol"/> if its a <see cref="INamespaceSymbol"/>.</param>
		public static string JoinNamespaces(this ISymbol symbol, bool includeSelf = true)
		{
			StringBuilder builder = new();
			symbol.JoinNamespacesInto(builder, includeSelf);
			return builder.ToString();
		}

		/// <summary>
		/// Writes a <see cref="string"/> that is created by joining the names of the namespaces the provided <paramref name="symbol"/> is contained in to the specified <paramref name="builder"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the containing namespaces of.</param>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <param name="includeSelf">Determines whether to include name of the <paramref name="symbol"/> if its a <see cref="INamespaceSymbol"/>.</param>
		public static void JoinNamespacesInto(this ISymbol symbol, StringBuilder builder, bool includeSelf = true)
		{
			if(symbol is INamespaceSymbol @namespace)
			{
				@namespace.JoinNamespacesInto(builder, includeSelf);
				return;
			}

			if (symbol.ContainingNamespace is null || symbol.ContainingNamespace.IsGlobalNamespace)
			{
				return;
			}

			symbol.GetContainingNamespaces().JoinNamespacesInto(builder);
		}

		/// <summary>
		/// Returns a <see cref="string"/> that is created by joining the names of parent namespaces of the provided <paramref name="namespace"/>.
		/// </summary>
		/// <param name="namespace">A collection of <see cref="INamespaceSymbol"/>s create the <see cref="string"/> from.</param>
		/// <param name="includeSelf">Determines whether to include name of the <paramref name="namespace"/>.</param>
		public static string JoinNamespaces(this INamespaceSymbol @namespace, bool includeSelf = true)
		{
			StringBuilder builder = new();
			@namespace.JoinNamespacesInto(builder, includeSelf);
			return builder.ToString();
		}

		/// <summary>
		/// Writes a <see cref="string"/> that is created by joining the names of parent namespaces of the provided <paramref name="namespace"/> to the specified <paramref name="builder"/>.
		/// </summary>
		/// <param name="namespace">A collection of <see cref="INamespaceSymbol"/>s create the <see cref="string"/> from.</param>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <param name="includeSelf">Determines whether to include name of the <paramref name="namespace"/>.</param>
		public static void JoinNamespacesInto(this INamespaceSymbol @namespace, StringBuilder builder, bool includeSelf = true)
		{
			@namespace.GetContainingNamespaces().JoinNamespacesInto(builder);

			if(includeSelf)
			{
				builder.Append('.').Append(@namespace.Name);
			}
		}

		/// <summary>
		/// Returns a <see cref="string"/> that is created by joining the names of the provided <paramref name="namespaces"/>.
		/// </summary>
		/// <param name="namespaces">A collection of <see cref="INamespaceSymbol"/>s create the <see cref="string"/> from.</param>
		public static string JoinNamespaces(this IEnumerable<INamespaceSymbol> namespaces)
		{
			StringBuilder builder = new();
			namespaces.JoinNamespacesInto(builder);
			return builder.ToString();
		}

		/// <summary>
		/// Writes a <see cref="string"/> that is created by joining the names of the provided <paramref name="namespaces"/> to the specified <paramref name="builder"/>.
		/// </summary>
		/// <param name="namespaces">A collection of <see cref="INamespaceSymbol"/>s create the <see cref="string"/> from.</param>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		public static void JoinNamespacesInto(this IEnumerable<INamespaceSymbol> namespaces, StringBuilder builder)
		{
			bool any = false;

			foreach (INamespaceSymbol n in namespaces)
			{
				if (n is null)
				{
					continue;
				}

				any = true;

				builder.Append(n.Name).Append('.');
			}

			if(any)
			{
				builder.Remove(builder.Length - 1, 1);
			}
		}

		/// <summary>
		/// Determines whether the specified <paramref name="method"/> is overrides the <paramref name="other"/> symbol either directly or through additional child types.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to check if overrides the <paramref name="other"/> symbol.</param>
		/// <param name="other"><see cref="IMethodSymbol"/> to check if is overridden by the specified <paramref name="method"/>.</param>
		public static bool Overrides(this IMethodSymbol method, IMethodSymbol other)
		{
			if (SymbolEqualityComparer.Default.Equals(method, other))
			{
				return false;
			}

			IMethodSymbol? baseMethod = method.OverriddenMethod;

			while (baseMethod is not null)
			{
				if (SymbolEqualityComparer.Default.Equals(other, baseMethod))
				{
					return true;
				}

				baseMethod = baseMethod.OverriddenMethod;
			}

			return false;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> can have an explicit base type.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check whether can have an explicit base type.</param>
		public static bool SupportsExplicitBaseType(this INamedTypeSymbol type)
		{
			return type.TypeKind == TypeKind.Class && !type.IsStatic && !type.IsSealed;
		}

		/// <summary>
		/// Attempts to return a <see cref="CSharpSyntaxNode"/> of type <typeparamref name="T"/> associated with the specified <paramref name="symbol"/>.
		/// </summary>
		/// <typeparam name="T">Type of <see cref="CSharpSyntaxNode"/> to return.</typeparam>
		/// <param name="symbol"><see cref="ISymbol"/> to get the <see cref="CSharpSyntaxNode"/> associated with.</param>
		/// <param name="syntax"><see cref="CSharpSyntaxNode"/> associated with the specified <paramref name="symbol"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static bool TryGetSyntax<T>(this ISymbol symbol, [NotNullWhen(true)] out T? syntax, CancellationToken cancellationToken = default) where T : CSharpSyntaxNode
		{
			syntax = symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken) as T;
			return syntax is not null;
		}

		/// <summary>
		/// Returns a <see cref="string"/> representation of a C# keyword associated with the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="ITypeSymbol"/> to get the keyword associated with.</param>
		public static string GetTypeKeyword(this ITypeSymbol type)
		{
			if (type is IDynamicTypeSymbol)
			{
				return "dynamic";
			}

			if (type.SpecialType == SpecialType.None)
			{
				return type.Name;
			}

			return type.SpecialType.GetTypeKeyword() ?? throw new ArgumentException($"Type '{type}' does not have a type keyword", nameof(type));
		}

		/// <summary>
		/// Returns a <see cref="string"/> representation of a C# keyword associated with the specified <paramref name="specialType"/> value.
		/// </summary>
		/// <param name="specialType">Value of <see cref="SpecialType"/> to get the C# keyword associated with.</param>
		public static string? GetTypeKeyword(this SpecialType specialType)
		{
			if(specialType == SpecialType.None)
			{
				return default;
			}

			return specialType switch
			{
				SpecialType.System_Byte => "byte",
				SpecialType.System_Char => "char",
				SpecialType.System_Boolean => "bool",
				SpecialType.System_Decimal => "decimal",
				SpecialType.System_Double => "double",
				SpecialType.System_Int16 => "short",
				SpecialType.System_Int32 => "int",
				SpecialType.System_Int64 => "long",
				SpecialType.System_Object => "object",
				SpecialType.System_SByte => "sbyte",
				SpecialType.System_Single => "float",
				SpecialType.System_String => "string",
				SpecialType.System_UInt16 => "ushort",
				SpecialType.System_UInt32 => "uint",
				SpecialType.System_UInt64 => "ulong",
				SpecialType.System_IntPtr => "nint",
				SpecialType.System_UIntPtr => "nuint",
				SpecialType.System_Void => "void",
				_ => default
			};
		}

		private static TypeSyntax ApplyAnnotation(TypeSyntax syntax, NullableAnnotation annotation)
		{
			if (annotation == NullableAnnotation.Annotated)
			{
				return SyntaxFactory.NullableType(syntax);
			}

			return syntax;
		}

		private static IEnumerable<INamedTypeSymbol> GetInnerTypes_Internal(INamedTypeSymbol symbol)
		{
			foreach (INamedTypeSymbol s in symbol.GetTypeMembers())
			{
				yield return s;

				foreach (INamedTypeSymbol inner in GetInnerTypes_Internal(s))
				{
					yield return inner;
				}
			}
		}

		private static IEnumerable<T> ReturnByOrder<T>(IEnumerable<T> collection, ReturnOrder order)
		{
			if (order == ReturnOrder.Root)
			{
				return collection.Reverse();
			}

			return collection;
		}
	}
}
