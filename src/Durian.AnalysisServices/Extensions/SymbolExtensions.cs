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
using Durian.Analysis.CodeGeneration;
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
					SyntaxFactory.Identifier(type.GetVerbatimName()),
					SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(arguments)));
			}
			else if (GetPredefineTypeSyntax(type) is PredefinedTypeSyntax predefined)
			{
				syntax = predefined;
			}
			else
			{
				syntax = SyntaxFactory.IdentifierName(type.GetVerbatimName());
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
			return ApplyAnnotation(SyntaxFactory.IdentifierName(typeParameter.GetVerbatimName()), typeParameter.NullableAnnotation);
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
				_ => ApplyAnnotation(SyntaxFactory.IdentifierName(type.GetVerbatimName()), type.NullableAnnotation),
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

			return SyntaxFactory.IdentifierName(symbol.GetVerbatimName());
		}

		/// <summary>
		/// Creates a <see cref="SimpleNameSyntax"/> representing the specified <paramref name="method"/>.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to get the <see cref="SimpleNameSyntax"/> for.</param>
		public static TypeSyntax CreateTypeSyntax(this IMethodSymbol method)
		{
			if (!method.IsGenericMethod)
			{
				return SyntaxFactory.IdentifierName(method.GetVerbatimName());
			}

			List<TypeSyntax> arguments = new(method.TypeArguments.Length);

			foreach (ITypeSymbol type in method.TypeArguments)
			{
				arguments.Add(type.CreateTypeSyntax());
			}

			return SyntaxFactory.GenericName(
				SyntaxFactory.Identifier(method.GetVerbatimName()),
				SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(arguments)));
		}

		/// <inheritdoc cref="GetAllMembers(INamedTypeSymbol, string, ReturnOrder)"/>
		public static IEnumerable<ISymbol> GetAllMembers(this INamedTypeSymbol type, ReturnOrder order = ReturnOrder.Parent)
		{
			return type.GetBaseTypes(true, order).SelectMany(t => t.GetMembers());
		}

		/// <summary>
		/// Returns all members of the specified <paramref name="type"/> including the members that are declared in base types of this <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="ITypeSymbol"/> to get the members of.</param>
		/// <param name="name">Name of the members to find.</param>
		/// <param name="order">Specifies ordering of the returned members.</param>
		public static IEnumerable<ISymbol> GetAllMembers(this INamedTypeSymbol type, string name, ReturnOrder order = ReturnOrder.Parent)
		{
			return type.GetBaseTypes(true, order).SelectMany(t => t.GetMembers(name));
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
		/// Returns a keyword used to refer to the specified <paramref name="symbol"/> inside an attribute list.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the associated attribute target keyword of.</param>
		/// <param name="targetKind">Determines which keyword to return when there is more than one option (e.g '<see langword="method"/>' and '<see langword="return"/>' for methods).</param>
		/// <exception cref="ArgumentException"><paramref name="symbol"/> does not support attribute targets.</exception>
		public static string GetAttributeTarget(this ISymbol symbol, AttributeTargetKind targetKind = default)
		{
			if (symbol is IMethodSymbol method)
			{
				if (AnalysisUtilities.GetAttributeTarget(method.MethodKind, targetKind) is string methodTarget)
				{
					return methodTarget;
				}
			}
			else if (symbol is IEventSymbol @event && !@event.IsFieldEvent())
			{
				return "event";
			}
			else if (AnalysisUtilities.GetAttributeTarget(symbol.Kind, targetKind) is string target)
			{
				return target;
			}

			throw new ArgumentException($"Symbol '{symbol}' does not support attribute targets", nameof(symbol));
		}

		/// <summary>
		/// Returns the type of result of awaiting on the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to get the type of result of awaiting on.</param>
		public static ITypeSymbol? GetAwaitResult(this INamedTypeSymbol type)
		{
			bool hasIsCompleted = false;
			bool hasGetResult = false;

			ITypeSymbol? resultType = default;

			if (!type.IsINotifyCompletion())
			{
				foreach (IMethodSymbol method in type.GetAllMembers().OfType<IMethodSymbol>())
				{
					if (SymbolFacts.IsGetAwaiterRaw(method, out INamedTypeSymbol? returnType) && HandleAwaiterType(type))
					{
						return resultType;
					}
				}

				return default;
			}

			List<INamedTypeSymbol> awaiters = new();

			foreach (ISymbol symbol in type.GetAllMembers())
			{
				bool? handleResultType = HandleResultType(symbol);

				if (handleResultType.HasValue)
				{
					if (handleResultType.Value)
					{
						return resultType;
					}
				}
				else if (symbol is IMethodSymbol method && SymbolFacts.IsGetAwaiterRaw(method, out INamedTypeSymbol? awaiter))
				{
					awaiters.Add(awaiter);
				}
			}

			foreach (INamedTypeSymbol awaiter in awaiters)
			{
				if (HandleAwaiterType(awaiter))
				{
					return resultType;
				}
			}

			return default;

			bool HandleAwaiterType(INamedTypeSymbol type)
			{
				if (!type.IsINotifyCompletion())
				{
					return false;
				}

				resultType = default;

				hasIsCompleted = false;
				hasGetResult = false;

				foreach (ISymbol symbol in type.GetAllMembers())
				{
					bool? handleResultType = HandleResultType(symbol);

					if (handleResultType == true)
					{
						return true;
					}
				}

				return false;
			}

			bool? HandleResultType(ISymbol symbol)
			{
				if (!hasIsCompleted && symbol.IsSpecialMember(SpecialMember.IsCompleted))
				{
					if (hasGetResult)
					{
						return true;
					}

					hasIsCompleted = true;

					return false;
				}
				else if (!hasGetResult && symbol.IsSpecialMember(SpecialMember.GetResult))
				{
					IMethodSymbol method = (symbol as IMethodSymbol)!;
					resultType = method.ReturnsVoid ? default : method.ReturnType;

					if (hasIsCompleted)
					{
						return true;
					}

					hasGetResult = true;

					return false;
				}

				return default;
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
		/// Returns all types the specified <paramref name="type"/> inherits from.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to get the base types of.</param>
		/// <param name="includeSelf">Determines whether to include the <paramref name="type"/> in the returned collection.</param>
		/// <param name="order">Specifies ordering of the returned members.</param>
		public static IEnumerable<INamedTypeSymbol> GetBaseTypes(this INamedTypeSymbol type, bool includeSelf = false, ReturnOrder order = ReturnOrder.Parent)
		{
			return AnalysisUtilities.ByOrder(Yield(), order);

			IEnumerable<INamedTypeSymbol> Yield()
			{
				if (includeSelf)
				{
					yield return type;
				}

				if (type.TypeKind == TypeKind.Interface)
				{
					foreach (INamedTypeSymbol t in type.AllInterfaces)
					{
						yield return t;
					}

					yield break;
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
			IEnumerable<INamespaceSymbol> namespaces = AnalysisUtilities.ByOrder(GetNamespaces(), order);

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
		/// <param name="order">Specifies ordering of the returned members.</param>
		public static IEnumerable<INamespaceOrTypeSymbol> GetContainingNamespacesAndTypes(this ISymbol symbol, bool includeGlobal = false, ReturnOrder order = ReturnOrder.Root)
		{
			IEnumerable<INamespaceOrTypeSymbol> first;
			IEnumerable<INamespaceOrTypeSymbol> second;

			if (order == ReturnOrder.Root)
			{
				first = symbol.GetContainingNamespaces(includeGlobal, order);
				second = symbol.GetContainingTypes(false, order);
			}
			else
			{
				first = symbol.GetContainingTypes(false, order);
				second = symbol.GetContainingNamespaces(includeGlobal, order);
			}

			return first.Concat(second);
		}

		/// <summary>
		/// Returns all <see cref="INamedTypeSymbol"/>s that contain the target <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the parent types of.</param>
		/// <param name="includeSelf">Determines whether to include the <paramref name="symbol"/> in the returned collection if its a <see cref="INamedTypeSymbol"/>.</param>
		/// <param name="order">Specifies ordering of the returned members.</param>
		public static IEnumerable<INamedTypeSymbol> GetContainingTypes(this ISymbol symbol, bool includeSelf = false, ReturnOrder order = ReturnOrder.Root)
		{
			return AnalysisUtilities.ByOrder(Yield(), order);

			IEnumerable<INamedTypeSymbol> Yield()
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
		public static IEnumerable<ITypeData> GetContainingTypesAsData(this ISymbol symbol, ICompilationData compilation, bool includeSelf = false, ReturnOrder order = ReturnOrder.Root)
		{
			INamedTypeSymbol[] parentSymbols = GetContainingTypes(symbol, includeSelf, order).ToArray();
			List<ITypeData> parentList = new(parentSymbols.Length);

			return parentSymbols.Select(parent =>
			{
				if (parent.ToData(compilation) is not ITypeData data)
				{
					throw new InvalidOperationException($"Invalid type kind of '{parent}'");
				}

				parentList.Add(data);
				return data;
			});
		}

		/// <summary>
		/// Returns a <see cref="string"/> that contains all the parent types of the specified <paramref name="symbol"/> and the <paramref name="symbol"/>'s name separated by the dot ('.') character.
		/// </summary>
		/// <remarks>If the <paramref name="symbol"/> is not contained within a type, an empty <see cref="string"/> is returned instead.</remarks>
		/// <param name="symbol"><see cref="ISymbol"/> to get the <see cref="string"/> of.</param>
		/// <param name="includeSelf">Determines whether to include the <paramref name="symbol"/> in the returned <see cref="string"/>.</param>
		/// <param name="includeParameters">If the value of <paramref name="symbol"/> is a <see cref="IMethodSymbol"/>, determines whether to include the method's parameters in the returned <see cref="string"/>.</param>
		public static string GetContainingTypesAsString(this ISymbol symbol, bool includeSelf = true, bool includeParameters = false)
		{
			StringBuilder builder = new();
			builder.WriteContainingTypes(symbol, includeSelf, includeParameters);
			return builder.ToString();
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
			return AnalysisUtilities.ByOrder(Yield(), order);

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
		/// Returns fully qualified name of the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the fully qualified name of.</param>
		/// <param name="metadata">Determines whether the format the created fully qualified name to using metadata naming conventions.</param>
		public static string GetFullyQualifiedName(this ISymbol symbol, bool metadata = false)
		{
			if (metadata)
			{
				return symbol.ToString();
			}

			StringBuilder builder = new();
			builder.WriteFullyQualifiedName(symbol);
			return builder.ToString();
		}

		/// <summary>
		/// Returns a <see cref="string"/> containing generic identifier of the specified <paramref name="symbol"/> or name of the <paramref name="symbol"/> if it is not an <see cref="IMethodSymbol"/> or <see cref="INamedTypeSymbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the generic name of.</param>
		/// <param name="substitution">Configures how generic type parameters are substituted.</param>
		public static string GetGenericName(this ISymbol symbol, GenericSubstitution substitution = default)
		{
			StringBuilder builder = new();
			builder.WriteGenericName(symbol, substitution);
			return builder.ToString();
		}

		/// <summary>
		/// Returns a <see cref="string"/> containing generic identifier of the specified <paramref name="method"/>.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to get the generic name of.</param>
		/// <param name="substitution">Configures how generic type parameters are substituted.</param>
		public static string GetGenericName(this IMethodSymbol method, GenericSubstitution substitution = default)
		{
			StringBuilder builder = new();
			builder.WriteGenericName(method, substitution);
			return builder.ToString();
		}

		/// <summary>
		/// Returns a <see cref="string"/> containing generic identifier of the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to get the generic name of.</param>
		/// <param name="substitution">Configures how generic type parameters are substituted.</param>
		public static string GetGenericName(this INamedTypeSymbol type, GenericSubstitution substitution = default)
		{
			StringBuilder builder = new();
			builder.WriteGenericName(type, substitution);
			return builder.ToString();
		}

		/// <summary>
		/// Returns a <see cref="string"/> containing the generic part of an identifier created from the collection of <paramref name="typeParameters"/>.
		/// </summary>
		/// <param name="typeParameters">Type parameters.</param>
		/// <param name="includeVariance">Determines whether to include variance of the <paramref name="typeParameters"/>.</param>
		/// <exception cref="InvalidOperationException">Pointers can't be used as generic arguments.</exception>
		public static string GetGenericName(this IEnumerable<ITypeParameterSymbol> typeParameters, bool includeVariance = false)
		{
			return GetGenericName(typeParameters, null, includeVariance);
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
						return $"{p.Variance.ToString().ToLower()} {p.GetVerbatimName()}";
					}

					return p.GetVerbatimName();
				}),
				name);
			}

			return AnalysisUtilities.GetGenericName(typeParameters.Select(p => p.GetVerbatimName()), name);
		}

		/// <summary>
		/// Returns a <see cref="string"/> containing the generic part of an identifier created from the collection of <paramref name="typeArguments"/>.
		/// </summary>
		/// <param name="typeArguments">Type arguments.</param>
		/// <exception cref="InvalidOperationException">Pointers can't be used as generic arguments.</exception>
		public static string GetGenericName(this IEnumerable<ITypeSymbol> typeArguments)
		{
			StringBuilder builder = new();
			builder.WriteGenericName(typeArguments);
			return builder.ToString();
		}

		/// <summary>
		/// Returns a <see cref="string"/> containing generic identifier combined of the specified <paramref name="name"/> and the collection of <paramref name="typeArguments"/>.
		/// </summary>
		/// <param name="typeArguments">Type arguments.</param>
		/// <param name="name">Actual member identifier.</param>
		public static string GetGenericName(this IEnumerable<ITypeSymbol> typeArguments, string? name)
		{
			StringBuilder builder = new();
			builder.WriteGenericName(typeArguments, name);
			return builder.ToString();
		}

		/// <summary>
		/// Returns the <see cref="ISymbol"/> hidden by the specified <paramref name="symbol"/> using the <see langword="new"/> keyword.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the symbol hidden by.</param>
		public static ISymbol? GetHiddenSymbol(this ISymbol symbol)
		{
			return symbol switch
			{
				INamedTypeSymbol type => type.GetHiddenSymbol(),
				IMethodSymbol method => method.GetHiddenSymbol(),
				IPropertySymbol property => property.GetHiddenSymbol(),
				IFieldSymbol field => field.GetHiddenSymbol(),
				IEventSymbol @event => @event.GetHiddenSymbol(),
				_ => default
			};
		}

		/// <summary>
		/// Returns the <see cref="ISymbol"/> hidden by the specified <paramref name="method"/> using the <see langword="new"/> keyword.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to get the symbol hidden by.</param>
		public static ISymbol? GetHiddenSymbol(this IMethodSymbol method)
		{
			if (method.MethodKind != MethodKind.Ordinary || method.ContainingType is null)
			{
				return default;
			}

			if (method.Arity == 0)
			{
				return method.ContainingType
					.GetAllMembers(method.GetVerbatimName())
					.FirstOrDefault(member => member switch
					{
						INamedTypeSymbol type => type.Arity == 0,
						IMethodSymbol other => SymbolFacts.CanHideSymbol_Internal(method, other),
						IPropertySymbol property => !property.IsIndexer,
						IFieldSymbol or IFieldSymbol => true,
						_ => false
					});
			}

			return method.ContainingType
				.GetAllMembers(method.GetVerbatimName())
				.FirstOrDefault(member => member switch
				{
					INamedTypeSymbol type => type.Arity == method.Arity,
					IMethodSymbol other => SymbolFacts.CanHideSymbol_Internal(method, other),
					_ => false
				});
		}

		/// <summary>
		/// Returns the <see cref="ISymbol"/> hidden by the specified <paramref name="type"/> using the <see langword="new"/> keyword.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to get the symbol hidden by.</param>
		public static ISymbol? GetHiddenSymbol(this INamedTypeSymbol type)
		{
			if (type.ContainingType is null || !type.IsDeclarationKind())
			{
				return default;
			}

			if (type.Arity == 0)
			{
				return GetHiddenSymbol_Internal(type);
			}

			return type.ContainingType
				.GetAllMembers(type.GetVerbatimName())
				.FirstOrDefault(member => member switch
				{
					INamedTypeSymbol other => other.Arity == type.Arity,
					IMethodSymbol method => method.Arity == type.Arity,
					_ => false
				});
		}

		/// <summary>
		/// Returns the <see cref="ISymbol"/> hidden by the specified <paramref name="property"/> using the <see langword="new"/> keyword.
		/// </summary>
		/// <param name="property"><see cref="IPropertySymbol"/> to get the symbol hidden by.</param>
		public static ISymbol? GetHiddenSymbol(this IPropertySymbol property)
		{
			if (property.ContainingType is null)
			{
				return default;
			}

			if (property.IsIndexer)
			{
				return property.ContainingType
					.GetAllMembers()
					.OfType<IPropertySymbol>()
					.FirstOrDefault(p => p.IsIndexer && SymbolFacts.ParametersAreEquivalent(property.Parameters, p.Parameters));
			}

			return GetHiddenSymbol_Internal(property);
		}

		/// <summary>
		/// Returns the <see cref="ISymbol"/> hidden by the specified <paramref name="field"/> using the <see langword="new"/> keyword.
		/// </summary>
		/// <param name="field"><see cref="IFieldSymbol"/> to get the symbol hidden by.</param>
		public static ISymbol? GetHiddenSymbol(this IFieldSymbol field)
		{
			if (field.ContainingType is null)
			{
				return default;
			}

			return GetHiddenSymbol_Internal(field);
		}

		/// <summary>
		/// Returns the <see cref="ISymbol"/> hidden by the specified <paramref name="event"/> using the <see langword="new"/> keyword.
		/// </summary>
		/// <param name="event"><see cref="IEventSymbol"/> to get the symbol hidden by.</param>
		public static ISymbol? GetHiddenSymbol(this IEventSymbol @event)
		{
			if (@event.ContainingType is null)
			{
				return default;
			}

			return @event.ContainingType
				.GetAllMembers(@event.GetVerbatimName())
				.FirstOrDefault(member => member switch
				{
					INamedTypeSymbol type => type.Arity == 0,
					IMethodSymbol method => method.Arity == 0,
					IPropertySymbol property => !property.IsIndexer,
					IFieldSymbol or IFieldSymbol => true,
					_ => false
				});
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

			return AutoGenerated.GetInheritdoc(AnalysisUtilities.ToXmlCompatible(symbol.GetContainingTypesAsString(true, true)));
		}

		/// <summary>
		/// Returns a collection of all inner types of the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to get the inner types of.</param>
		/// <param name="includeSelf">Determines whether to include the <paramref name="type"/> in the returned collection.</param>
		/// <param name="order">Specifies ordering of the returned members.</param>
		public static IEnumerable<INamedTypeSymbol> GetInnerTypes(this INamedTypeSymbol type, bool includeSelf = false, ReturnOrder order = ReturnOrder.Parent)
		{
			const int capacity = 32;

			return AnalysisUtilities.ByOrder(Yield(), order);

			IEnumerable<INamedTypeSymbol> Yield()
			{
				if (includeSelf)
				{
					yield return type;
				}

				ImmutableArray<INamedTypeSymbol> array = type.GetTypeMembers();

				if (array.Length == 0)
				{
					yield break;
				}

				Stack<INamedTypeSymbol> innerTypes = new(array.Length > capacity ? array.Length : capacity);

				foreach (INamedTypeSymbol t in array)
				{
					innerTypes.Push(t);
				}

				while (innerTypes.Count > 0)
				{
					INamedTypeSymbol t = innerTypes.Pop();
					yield return t;

					array = t.GetTypeMembers();

					if (array.Length == 0)
					{
						continue;
					}

					foreach (INamedTypeSymbol child in array.Reverse())
					{
						innerTypes.Push(child);
					}
				}
			}
		}

		/// <summary>
		/// Returns modifiers applied to the target <see cref="INamedTypeSymbol"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to get the modifiers of.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static IEnumerable<string> GetModifiers(this INamedTypeSymbol type, CancellationToken cancellationToken = default)
		{
			return type.DeclaringSyntaxReferences
				.Select(e => e.GetSyntax(cancellationToken))
				.Cast<TypeDeclarationSyntax>()
				.GetModifiers()
				.Select(m => m.ValueText);
		}

		/// <summary>
		/// Returns text of operator this <paramref name="method"/> overloads.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to get the kind of the overloaded operator.</param>
		public static string? GetOperatorToken(this IMethodSymbol method)
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
		/// Returns the <see cref="ISymbol"/> overridden by the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the symbol overridden by.</param>
		public static ISymbol? GetOverriddenSymbol(this ISymbol symbol)
		{
			return symbol switch
			{
				IMethodSymbol method => method.OverriddenMethod,
				IPropertySymbol property => property.OverriddenProperty,
				IEventSymbol @event => @event.OverriddenEvent,
				_ => default
			};
		}

		/// <summary>
		/// Returns all <see cref="IMethodSymbol"/>s overridden by the specified <paramref name="method"/>.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to get the methods overridden by.</param>
		public static IEnumerable<IMethodSymbol> GetOverriddenSymbols(this IMethodSymbol method)
		{
			IMethodSymbol? m = method;

			while ((m = m!.OverriddenMethod) is not null)
			{
				yield return m;
			}
		}

		/// <summary>
		/// Returns all <see cref="IPropertySymbol"/>s overridden by the specified <paramref name="property"/>.
		/// </summary>
		/// <param name="property"><see cref="IPropertySymbol"/> to get the properties overridden by.</param>
		public static IEnumerable<IPropertySymbol> GetOverriddenSymbols(this IPropertySymbol property)
		{
			IPropertySymbol? p = property;

			while ((p = p!.OverriddenProperty) is not null)
			{
				yield return p;
			}
		}

		/// <summary>
		/// Returns all <see cref="IEventSymbol"/>s overridden by the specified <paramref name="event"/>.
		/// </summary>
		/// <param name="event"><see cref="IEventSymbol"/> to get the events overridden by.</param>
		public static IEnumerable<IEventSymbol> GetOverriddenSymbols(this IEventSymbol @event)
		{
			IEventSymbol? e = @event;

			while ((e = e!.OverriddenEvent) is not null)
			{
				yield return e;
			}
		}

		/// <summary>
		/// Returns all <see cref="ISymbol"/>s overridden by the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the symbols overridden by.</param>
		public static IEnumerable<ISymbol> GetOverriddenSymbols(this ISymbol symbol)
		{
			return symbol switch
			{
				IMethodSymbol method => method.GetOverriddenSymbols(),
				IPropertySymbol property => property.GetOverriddenSymbols(),
				IEventSymbol @event => @event.GetOverriddenSymbols(),
				_ => Array.Empty<ISymbol>()
			};
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
		/// Returns an <see cref="ISymbol"/> representing the given kind of <paramref name="specialMember"/> available from the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to get the special member from.</param>
		/// <param name="specialMember">Kind of special member to return.</param>
		public static ISymbol? GetSpecialMember(this INamedTypeSymbol type, SpecialMember specialMember)
		{
			return type.GetAllMembers().FirstOrDefault(member => member.IsSpecialMember(specialMember));
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
		/// Returns a <see cref="string"/> representation of a C# keyword associated with the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="ITypeSymbol"/> to get the keyword associated with.</param>
		public static string? GetTypeKeyword(this ITypeSymbol type)
		{
			if (type is IDynamicTypeSymbol)
			{
				return "dynamic";
			}

			return AnalysisUtilities.GetTypeKeyword(type.SpecialType);
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
				name = SyntaxFactory.IdentifierName(@namespace.GetVerbatimName());
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
				name = SyntaxFactory.IdentifierName(first.GetVerbatimName());
			}
			else
			{
				throw new ArgumentException($"'{nameof(namespaces)}' cannot be empty");
			}

			return SyntaxFactory.UsingDirective(name);
		}

		/// <summary>
		/// Returns name of the <paramref name="symbol"/> with a verbatim identifier '@' token applied if necessary.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the effective name of.</param>
		public static string GetVerbatimName(this ISymbol symbol)
		{
			string value = symbol.Name;
			AnalysisUtilities.ApplyVerbatimIfNecessary(ref value);
			return value;
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
			StringBuilder builder = new();

			switch (method.MethodKind)
			{
				case MethodKind.Constructor:
					builder.Append(method.ContainingType.Name);
					break;

				case MethodKind.UserDefinedOperator:
					builder.Append("operator ");
					builder.Append(method.GetOperatorToken());
					break;

				case MethodKind.Conversion:

					string keyword;

					if (method.IsImplicitOperator())
					{
						keyword = "implicit";
					}
					else if (method.IsExplicitOperator())
					{
						keyword = "explicit";
					}
					else
					{
						goto default;
					}

					builder.Append("implicit ");
					builder.Append(keyword);
					builder.Append(' ');
					builder.WriteFullyQualifiedName(method.ReturnType);

					break;

				default:
					builder.WriteGenericName(method);
					break;
			}

			if (includeParameters)
			{
				builder.WriteParameterList(method);
			}

			return AnalysisUtilities.ToXmlCompatible(builder.ToString());
		}

		/// <summary>
		/// Returns a <see cref="QualifiedNameSyntax"/> created from the specified <paramref name="namespaces"/>.
		/// </summary>
		/// <param name="namespaces">A collection of <see cref="INamespaceSymbol"/>s to create the <see cref="QualifiedNameSyntax"/> from.</param>
		/// <returns>A <see cref="QualifiedNameSyntax"/> created by combining the <paramref name="namespaces"/>. -or- <see langword="null"/> if there were less then 2 <paramref name="namespaces"/> provided.</returns>
		public static QualifiedNameSyntax? JoinIntoQualifiedName(this IEnumerable<INamespaceSymbol> namespaces)
		{
			return AnalysisUtilities.GetQualifiedName(namespaces.Select(n => n.GetVerbatimName()));
		}

		/// <summary>
		/// Returns a <see cref="string"/> that is created by joining the names of the namespaces the provided <paramref name="symbol"/> is contained in.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the containing namespaces of.</param>
		/// <param name="includeSelf">Determines whether to include name of the <paramref name="symbol"/> if its a <see cref="INamespaceSymbol"/>.</param>
		public static string JoinNamespaces(this ISymbol symbol, bool includeSelf = true)
		{
			StringBuilder builder = new();
			builder.WriteNamespacesOf(symbol, includeSelf);
			return builder.ToString();
		}

		/// <summary>
		/// Returns a <see cref="string"/> that is created by joining the names of parent namespaces of the provided <paramref name="namespace"/>.
		/// </summary>
		/// <param name="namespace">A collection of <see cref="INamespaceSymbol"/>s create the <see cref="string"/> from.</param>
		/// <param name="includeSelf">Determines whether to include name of the <paramref name="namespace"/>.</param>
		public static string JoinNamespaces(this INamespaceSymbol @namespace, bool includeSelf = true)
		{
			StringBuilder builder = new();
			builder.WriteNamespace(@namespace, includeSelf);
			return builder.ToString();
		}

		/// <summary>
		/// Returns a <see cref="string"/> that is created by joining the names of the provided <paramref name="namespaces"/>.
		/// </summary>
		/// <param name="namespaces">A collection of <see cref="INamespaceSymbol"/>s create the <see cref="string"/> from.</param>
		public static string JoinNamespaces(this IEnumerable<INamespaceSymbol> namespaces)
		{
			StringBuilder builder = new();
			builder.WriteNamespaces(namespaces);
			return builder.ToString();
		}

		/// <summary>
		/// Returns new <see cref="IMemberData"/> created for the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to create the <see cref="IMemberData"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="IMemberData"/> from.</param>
		public static IMemberData ToData(this INamedTypeSymbol type, ICompilationData compilation)
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
		public static IMethodData ToData(this IMethodSymbol method, ICompilationData compilation)
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
		public static IMemberData ToData(this IFieldSymbol field, ICompilationData compilation)
		{
			return new FieldData(field, compilation);
		}

		/// <summary>
		/// Returns new <see cref="IMemberData"/> created for the specified <paramref name="event"/>.
		/// </summary>
		/// <param name="event"><see cref="IEventSymbol"/> to create the <see cref="IMemberData"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="IMemberData"/> from.</param>
		public static IMemberData ToData(this IEventSymbol @event, ICompilationData compilation)
		{
			return new EventData(@event, compilation);
		}

		/// <summary>
		/// Returns new <see cref="IMemberData"/> created for the specified <paramref name="property"/>.
		/// </summary>
		/// <param name="property"><see cref="IPropertySymbol"/> to create the <see cref="IMemberData"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="IMemberData"/> from.</param>
		public static IMemberData ToData(this IPropertySymbol property, ICompilationData compilation)
		{
			if (property.IsIndexer)
			{
				return new IndexerData(property, compilation);
			}

			return new PropertyData(property, compilation);
		}

		/// <summary>
		/// Returns new <see cref="IMemberData"/> created for the specified <paramref name="local"/>.
		/// </summary>
		/// <param name="local"><see cref="ILocalSymbol"/> to create the <see cref="IMemberData"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="IMemberData"/> from.</param>
		public static IMemberData ToData(this ILocalSymbol local, ICompilationData compilation)
		{
			return new LocalData(local, compilation);
		}

		/// <summary>
		/// Returns new <see cref="IMemberData"/> created for the specified <paramref name="typeParameter"/>.
		/// </summary>
		/// <param name="typeParameter"><see cref="ITypeParameterSymbol"/> to create the <see cref="IMemberData"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="IMemberData"/> from.</param>
		public static IMemberData ToData(this ITypeParameterSymbol typeParameter, ICompilationData compilation)
		{
			return new TypeParameterData(typeParameter, compilation);
		}

		/// <summary>
		/// Returns new <see cref="IMemberData"/> created for the specified <paramref name="parameter"/>.
		/// </summary>
		/// <param name="parameter"><see cref="IParameterSymbol"/> to create the <see cref="IMemberData"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="IMemberData"/> from.</param>
		public static IMemberData ToData(this IParameterSymbol parameter, ICompilationData compilation)
		{
			return new ParameterData(parameter, compilation);
		}

		/// <summary>
		/// Returns new <see cref="IMemberData"/> created for the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="INamespaceOrTypeSymbol"/> to create the <see cref="IMemberData"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="IMemberData"/> from.</param>
		public static IMemberData ToData(this INamespaceOrTypeSymbol symbol, ICompilationData compilation)
		{
			if (symbol is INamespaceSymbol @namespace)
			{
				return @namespace.ToData(compilation);
			}

			if (symbol is INamedTypeSymbol type)
			{
				return type.ToData(compilation);
			}

			if (symbol is ITypeParameterSymbol typeParameter)
			{
				return typeParameter.ToData(compilation);
			}

			if (symbol is ITypeSymbol unknownType)
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
		public static IMemberData ToData(this ISymbol symbol, ICompilationData compilation)
		{
			return symbol switch
			{
				ITypeParameterSymbol typeParameter => typeParameter.ToData(compilation),
				INamedTypeSymbol type => type.ToData(compilation),
				IMethodSymbol method => method.ToData(compilation),
				IPropertySymbol property => property.ToData(compilation),
				IFieldSymbol field => field.ToData(compilation),
				IEventSymbol @event => @event.ToData(compilation),
				IParameterSymbol parameter => parameter.ToData(compilation),
				INamespaceSymbol @namespace => @namespace.ToData(compilation),
				ILocalSymbol local => local.ToData(compilation),
				ITypeSymbol unknownType => new UnknownTypeData(unknownType, compilation),
				_ => new MemberData(symbol, compilation)
			};
		}

		/// <summary>
		/// Returns new <see cref="IMemberData"/> created for the specified <paramref name="namespace"/>.
		/// </summary>
		/// <param name="namespace"><see cref="INamespaceSymbol"/> to create the <see cref="IMemberData"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="IMemberData"/> from.</param>
		public static IMemberData ToData(this INamespaceSymbol @namespace, ICompilationData compilation)
		{
			return new NamespaceData(@namespace, compilation);
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

		private static TypeSyntax ApplyAnnotation(TypeSyntax syntax, NullableAnnotation annotation)
		{
			if (annotation == NullableAnnotation.Annotated)
			{
				return SyntaxFactory.NullableType(syntax);
			}

			return syntax;
		}

		private static ISymbol? GetHiddenSymbol_Internal(ISymbol symbol)
		{
			return symbol.ContainingType
				.GetAllMembers(symbol.GetVerbatimName())
				.FirstOrDefault(member => member switch
				{
					INamedTypeSymbol type => type.Arity == 0,
					IMethodSymbol method => method.Arity == 0,
					IPropertySymbol property => !property.IsIndexer,
					IFieldSymbol or IFieldSymbol => true,
					_ => false
				});
		}
	}
}
