// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;

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
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
        public static bool CanInherit(this INamedTypeSymbol type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return type.TypeKind == TypeKind.Class && !type.IsStatic;
        }

        /// <summary>
        /// Determines whether the <paramref name="child"/> is contained withing the <paramref name="parent"/> at any nesting level.
        /// </summary>
        /// <param name="parent">Parent <see cref="ISymbol"/>.</param>
        /// <param name="child">Child <see cref="ISymbol"/>.</param>
        /// <returns>True if the <paramref name="parent"/> contains the <paramref name="child"/> or the <paramref name="parent"/> is equivalent to <paramref name="child"/>, otherwise false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="parent"/> is <see langword="null"/>. -or- <paramref name="child"/> is <see langword="null"/>.</exception>
        public static bool ContainsSymbol(this ISymbol parent, ISymbol child)
        {
            if (parent is null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            if (child is null)
            {
                throw new ArgumentNullException(nameof(child));
            }

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

        /// <inheritdoc cref="GetAllMembers(INamedTypeSymbol, string)"/>
        public static IEnumerable<ISymbol> GetAllMembers(this INamedTypeSymbol type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return type.GetMembers().Concat(GetBaseTypes(type).SelectMany(t => t.GetMembers()));
        }

        /// <summary>
        /// Returns all members of the specified <paramref name="type"/> including the members that are declared in base types of this <paramref name="type"/>.
        /// </summary>
        /// <param name="type"><see cref="ITypeSymbol"/> to get the members of.</param>
        /// <param name="name">Name of the members to find.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
        public static IEnumerable<ISymbol> GetAllMembers(this INamedTypeSymbol type, string name)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

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
        /// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>. -or- <paramref name="attrSymbol"/> is <see langword="null"/>.</exception>
        public static AttributeData? GetAttribute(this ISymbol symbol, INamedTypeSymbol attrSymbol)
        {
            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            if (attrSymbol is null)
            {
                throw new ArgumentNullException(nameof(attrSymbol));
            }

            return symbol.GetAttributes()
                .FirstOrDefault(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attrSymbol));
        }

        /// <summary>
        /// Returns an <see cref="AttributeData"/> associated with the <paramref name="syntax"/> defined on the specified <paramref name="symbol"/>.
        /// </summary>
        /// <param name="symbol">Target <see cref="ISymbol"/>.</param>
        /// <param name="syntax"><see cref="AttributeSyntax"/> to get the data of.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
        /// <returns>The <see cref="AttributeData"/> associated with the <paramref name="syntax"/>. -or- <see langword="null"/> if no such <see cref="AttributeData"/> found.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>. -or- <paramref name="syntax"/> is <see langword="null"/>.</exception>
        public static AttributeData? GetAttribute(this ISymbol symbol, AttributeSyntax syntax, CancellationToken cancellationToken = default)
        {
            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            if (syntax is null)
            {
                throw new ArgumentNullException(nameof(syntax));
            }

            foreach (AttributeData attr in symbol.GetAttributes())
            {
                SyntaxReference? reference = attr.ApplicationSyntaxReference;

                if (reference is null)
                {
                    continue;
                }

                if (reference.GetSyntax(cancellationToken).IsEquivalentTo(syntax))
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
        /// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>. -or- <paramref name="attrSymbol"/> is <see langword="null"/>.</exception>
        public static IEnumerable<AttributeData> GetAttributes(this ISymbol symbol, INamedTypeSymbol attrSymbol)
        {
            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            if (attrSymbol is null)
            {
                throw new ArgumentNullException(nameof(attrSymbol));
            }

            return Yield();

            IEnumerable<AttributeData> Yield()
            {
                foreach (AttributeData attr in symbol.GetAttributes())
                {
                    if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attrSymbol))
                    {
                        yield return attr;
                    }
                }
            }
        }

        /// <summary>
        /// Returns the backing field of the specified <paramref name="property"/> or <see langword="null"/> if the <paramref name="property"/> is not auto-implemented.
        /// </summary>
        /// <param name="property"><see cref="IPropertySymbol"/> to get the backing field of.</param>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> is <see langword="null"/>.</exception>
        public static IFieldSymbol? GetBackingField(this IPropertySymbol property)
        {
            if (property is null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            return property.ContainingType?
                .GetMembers()
                .OfType<IFieldSymbol>()
                .FirstOrDefault(f => SymbolEqualityComparer.Default.Equals(f.AssociatedSymbol, property));
        }

        /// <summary>
        /// Returns all <see cref="IMethodSymbol"/> this <paramref name="method"/> overrides.
        /// </summary>
        /// <param name="method"><see cref="IMethodSymbol"/> to get the base methods of.</param>
        /// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>.</exception>
        public static IEnumerable<IMethodSymbol> GetBaseMethods(this IMethodSymbol method)
        {
            if (method is null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            return Yield();

            IEnumerable<IMethodSymbol> Yield()
            {
                IMethodSymbol? m = method;

                while ((m = m!.OverriddenMethod) is not null)
                {
                    yield return m;
                }
            }
        }

        /// <summary>
        /// Returns all types the specified <paramref name="type"/> inherits from.
        /// </summary>
        /// <param name="type"><see cref="INamedTypeSymbol"/> to get the base types of.</param>
        /// <param name="includeSelf">Determines whether to include the <paramref name="type"/> in the returned collection.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
        public static IEnumerable<INamedTypeSymbol> GetBaseTypes(this INamedTypeSymbol type, bool includeSelf = false)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return Yield();

            IEnumerable<INamedTypeSymbol> Yield()
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
        }

        /// <summary>
        /// Returns all <see cref="INamespaceSymbol"/>s that contain the target <paramref name="symbol"/>.
        /// </summary>
        /// <param name="symbol"><see cref="ISymbol"/> to get the parent namespaces of.</param>
        /// <param name="includeGlobal">Determines whether to return the global namespace as well.</param>
        /// <param name="order">Specifies ordering of the returned members.</param>
        /// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
        public static IEnumerable<INamespaceSymbol> GetContainingNamespaces(this ISymbol symbol, bool includeGlobal = false, ReturnOrder order = ReturnOrder.Root)
        {
            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            IEnumerable<INamespaceSymbol> namespaces = AnalysisUtilities.ReturnByOrder(GetNamespaces(), order);

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
        /// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
        public static IEnumerable<INamespaceOrTypeSymbol> GetContainingNamespacesAndTypes(this ISymbol symbol, bool includeGlobal = false)
        {
            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            return GetNamespacesAndTypes();

            IEnumerable<INamespaceOrTypeSymbol> GetNamespacesAndTypes()
            {
                foreach (INamespaceSymbol s in GetContainingNamespaces(symbol, includeGlobal))
                {
                    yield return s;
                }

                foreach (INamedTypeSymbol s in GetContainingTypeSymbols(symbol))
                {
                    yield return s;
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
        public static IEnumerable<ITypeData> GetContainingTypes(this ISymbol symbol, ICompilationData compilation, bool includeSelf = false, ReturnOrder order = ReturnOrder.Root)
        {
            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            if (compilation is null)
            {
                throw new ArgumentNullException(nameof(compilation));
            }

            INamedTypeSymbol[] parentSymbols = GetContainingTypeSymbols(symbol, includeSelf, order).ToArray();
            List<ITypeData> parentList = new(parentSymbols.Length);

            return parentSymbols.Select<INamedTypeSymbol, ITypeData>(parent =>
            {
                if (parent.IsRecord)
                {
                    RecordData data = new(parent, compilation) { _containingTypes = parentList.ToArray() };
                    parentList.Add(data);
                    return data;
                }

                switch (parent.TypeKind)
                {
                    case TypeKind.Class:
                        {
                            ClassData data = new(parent, compilation) { _containingTypes = parentList.ToArray() };
                            parentList.Add(data);
                            return data;
                        }

                    case TypeKind.Interface:
                        {
                            InterfaceData data = new(parent, compilation) { _containingTypes = parentList.ToArray() };
                            parentList.Add(data);
                            return data;
                        }

                    case TypeKind.Struct:
                        {
                            StructData data = new(parent, compilation) { _containingTypes = parentList.ToArray() };
                            parentList.Add(data);
                            return data;
                        }

                    case TypeKind.Enum:
                        {
                            EnumData data = new(parent, compilation) { _containingTypes = parentList.ToArray() };
                            parentList.Add(data);
                            return data;
                        }

                    default:
                        throw new InvalidOperationException($"Invalid type kind of '{parent}'");
                }
            });
        }

        /// <summary>
        /// Returns all <see cref="INamedTypeSymbol"/>s that contain the target <paramref name="symbol"/>.
        /// </summary>
        /// <param name="symbol"><see cref="ISymbol"/> to get the parent types of.</param>
        /// <param name="includeSelf">Determines whether to include the <paramref name="symbol"/> in the returned collection if its a <see cref="INamedTypeSymbol"/>.</param>
        /// <param name="order">Specifies ordering of the returned members.</param>
        /// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
        public static IEnumerable<INamedTypeSymbol> GetContainingTypeSymbols(this ISymbol symbol, bool includeSelf = false, ReturnOrder order = ReturnOrder.Root)
        {
            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            return AnalysisUtilities.ReturnByOrder(GetTypes(), order);

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
        /// Returns the default constructor of the specified <paramref name="type"/> if it has one.
        /// </summary>
        /// <param name="type"><see cref="INamedTypeSymbol"/> to get the default constructor of.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
        public static IMethodSymbol? GetDefaultConstructor(this INamedTypeSymbol type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return type.InstanceConstructors.FirstOrDefault(ctor => ctor.IsImplicitlyDeclared && ctor.Parameters.Length == 0);
        }

        /// <summary>
        /// Returns the effective <see cref="Accessibility"/> of the specified <paramref name="symbol"/>.
        /// </summary>
        /// <param name="symbol"><see cref="ISymbol"/> to get the effective <see cref="Accessibility"/> of.</param>
        /// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
        public static Accessibility GetEffectiveAccessibility(this ISymbol symbol)
        {
            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

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
        /// Returns a <see cref="string"/> containing generic identifier of the specified <paramref name="symbol"/> -or- name of the <paramref name="symbol"/> if it is not an <see cref="IMethodSymbol"/> or <see cref="INamedTypeSymbol"/>.
        /// </summary>
        /// <param name="symbol"><see cref="ISymbol"/> to get the generic name of.</param>
        /// <param name="paramsOrArgs">
        /// Determines whether to write the type parameters (e.g. "T") or the type arguments the parameters were substituted by;
        /// <see langword="true"/> for parameters, <see langword="false"/> for arguments.
        /// </param>
        /// <param name="includeParameters">If the <paramref name="symbol"/> is a <see cref="IMethodSymbol"/>, determines whether to include the method's parameters in the returned <see cref="string"/>.</param>
        /// <param name="includeVariance">Determines whether to include variance of the <paramref name="symbol"/>'s type parameters.</param>
        /// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
        public static string GetGenericName(this ISymbol symbol, bool paramsOrArgs, bool includeParameters = false, bool includeVariance = false)
        {
            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            if (symbol is INamedTypeSymbol t)
            {
                if (paramsOrArgs)
                {
                    return GetGenericName(t.TypeParameters, t.Name, includeVariance);
                }
                else
                {
                    return GetGenericName(t.TypeArguments, t.Name);
                }
            }
            else if (symbol is IMethodSymbol m)
            {
                return GetGenericName(m, paramsOrArgs, includeParameters);
            }

            return symbol.Name;
        }

        /// <summary>
        /// Returns a <see cref="string"/> containing generic identifier of the specified <paramref name="method"/>.
        /// </summary>
        /// <param name="method"><see cref="IMethodSymbol"/> to get the generic name of.</param>
        /// <param name="paramsOrArgs">
        /// Determines whether to write the type parameters (e.g. "T") or the type arguments the parameters were substituted by;
        /// <see langword="true"/> for parameters, <see langword="false"/> for arguments.
        /// </param>
        /// <param name="includeParameters">Determines whether to include the method's parameters in the returned <see cref="string"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>.</exception>
        public static string GetGenericName(this IMethodSymbol method, bool paramsOrArgs, bool includeParameters = false)
        {
            if (method is null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            string name = GetGenericName(paramsOrArgs ? method.TypeParameters : method.TypeArguments, method.Name);

            if (includeParameters)
            {
                name = $"{name}{GetParameterSignature(method)}";
            }

            return name;
        }

        /// <summary>
        /// Returns a <see cref="string"/> containing generic identifier of the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type"><see cref="INamedTypeSymbol"/> to get the generic name of.</param>
        /// <param name="paramsOrArgs">
        /// Determines whether to write the type parameters (e.g. "T") or the type arguments the parameters were substituted by;
        /// <see langword="true"/> for parameters, <see langword="false"/> for arguments.
        /// </param>
        /// <param name="includeVariance">Determines whether to include variance of the <paramref name="type"/>'s type parameters.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
        public static string GetGenericName(this INamedTypeSymbol type, bool paramsOrArgs, bool includeVariance = false)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (paramsOrArgs)
            {
                return GetGenericName(type.TypeParameters, type.Name, includeVariance);
            }
            else
            {
                return GetGenericName(type.TypeArguments, type.Name);
            }
        }

        /// <summary>
        /// Returns a <see cref="string"/> containing the generic part of an identifier created from the collection of <paramref name="typeParameters"/>.
        /// </summary>
        /// <param name="typeParameters">Type parameters.</param>
        /// <param name="includeVariance">Determines whether to include variance of the <paramref name="typeParameters"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="typeParameters"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Pointers can't be used as generic arguments.</exception>
        public static string GetGenericName(this IEnumerable<ITypeParameterSymbol> typeParameters, bool includeVariance = false)
        {
            return GetGenericName(typeParameters, null, includeVariance);
        }

        /// <summary>
        /// Returns a <see cref="string"/> containing the generic part of an identifier created from the collection of <paramref name="typeArguments"/>.
        /// </summary>
        /// <param name="typeArguments">Type arguments.</param>
        /// <exception cref="ArgumentNullException"><paramref name="typeArguments"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Pointers can't be used as generic arguments.</exception>
        public static string GetGenericName(this IEnumerable<ITypeSymbol> typeArguments)
        {
            if (typeArguments is null)
            {
                throw new ArgumentNullException(nameof(typeArguments));
            }

            if (typeArguments is IEnumerable<ITypeParameterSymbol> enumerable)
            {
                return GetGenericName(enumerable);
            }

            ITypeSymbol[] symbols = typeArguments.ToArray();

            if (symbols.Length == 0)
            {
                return string.Empty;
            }

            StringBuilder sb = new();
            sb.Append('<');

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

                AnalysisUtilities.WriteTypeNameOfParameter(argument, sb);

                sb.Append(", ");
            }

            sb.Remove(sb.Length - 2, 2);
            sb.Append('>');

            return sb.ToString();
        }

        /// <summary>
        /// Returns a <see cref="string"/> containing generic identifier combined of the specified <paramref name="name"/> and the collection of <paramref name="typeParameters"/>.
        /// </summary>
        /// <param name="typeParameters">Type parameters.</param>
        /// <param name="name">Actual member identifier.</param>
        /// <param name="includeVariance">Determines whether to include variance of the <paramref name="typeParameters"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="typeParameters"/> is <see langword="null"/>.</exception>
        public static string GetGenericName(this IEnumerable<ITypeParameterSymbol> typeParameters, string? name, bool includeVariance = false)
        {
            if (typeParameters is null)
            {
                throw new ArgumentNullException(nameof(typeParameters));
            }

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
        /// Returns a <see cref="string"/> containing generic identifier combined of the specified <paramref name="name"/> and the collection of <paramref name="typeArguments"/>.
        /// </summary>
        /// <param name="typeArguments">Type arguments.</param>
        /// <param name="name">Actual member identifier.</param>
        /// <exception cref="ArgumentNullException"><paramref name="typeArguments"/> is <see langword="null"/>.</exception>
        public static string GetGenericName(this IEnumerable<ITypeSymbol> typeArguments, string? name)
        {
            if (typeArguments is null)
            {
                throw new ArgumentNullException(nameof(typeArguments));
            }

            return $"{name ?? string.Empty}{GetGenericName(typeArguments)}";
        }

        /// <summary>
        /// Returns a collection of all inner types of the specified <paramref name="symbol"/>.
        /// </summary>
        /// <param name="symbol"><see cref="INamedTypeSymbol"/> to get the inner types of.</param>
        /// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
        public static IEnumerable<INamedTypeSymbol> GetInnerTypes(this INamedTypeSymbol symbol)
        {
            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            return GetInnerTypesInternal(symbol);
        }

        /// <summary>
        /// Returns a collection of all inner types of the specified <paramref name="symbol"/>.
        /// </summary>
        /// <param name="symbol"><see cref="INamespaceSymbol"/> to get the inner types of.</param>
        /// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
        public static IEnumerable<INamedTypeSymbol> GetInnerTypes(this INamespaceSymbol symbol)
        {
            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            return Yield(symbol);

            static IEnumerable<INamedTypeSymbol> Yield(INamespaceSymbol @namespace)
            {
                foreach (INamedTypeSymbol type in @namespace.GetTypeMembers())
                {
                    yield return type;

                    foreach (INamedTypeSymbol inner in GetInnerTypesInternal(type))
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
        /// Returns new <see cref="IMemberData"/> created for the specified <paramref name="symbol"/>.
        /// </summary>
        /// <param name="symbol"><see cref="ISymbol"/> to create the <see cref="IMemberData"/> for.</param>
        /// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="IMemberData"/> from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
        public static IMemberData GetMemberData(this ISymbol symbol, ICompilationData compilation)
        {
            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            if (compilation is null)
            {
                throw new ArgumentNullException(nameof(compilation));
            }

            if (symbol is INamedTypeSymbol type)
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
                    _ => throw new InvalidOperationException($"Invalid type kind of '{type}")
                };
            }
            else if (symbol is IMethodSymbol method)
            {
                return new MethodData(method, compilation);
            }
            else if (symbol is IFieldSymbol field)
            {
                return new FieldData(field, compilation);
            }
            else if (symbol is IEventSymbol e)
            {
                return new EventData(e, compilation);
            }
            else if (symbol is IPropertySymbol p)
            {
                return new PropertyData(p, compilation);
            }
            else
            {
                return new MemberData(symbol, compilation);
            }
        }

        /// <summary>
        /// Returns modifiers applied to the target <see cref="INamedTypeSymbol"/>.
        /// </summary>
        /// <param name="type"><see cref="INamedTypeSymbol"/> to get the modifiers of.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
        public static IEnumerable<SyntaxToken> GetModifiers(this INamedTypeSymbol type, CancellationToken cancellationToken = default)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return GetModifiers(type.DeclaringSyntaxReferences.Select(e => e.GetSyntax(cancellationToken)).Cast<TypeDeclarationSyntax>());
        }

        /// <summary>
        /// Returns modifiers contained withing the given collection of <see cref="TypeDeclarationSyntax"/>es.
        /// </summary>
        /// <param name="decl">Collection of <see cref="TypeDeclarationSyntax"/>es to get the modifiers from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="decl"/> is <see langword="null"/>.</exception>
        public static IEnumerable<SyntaxToken> GetModifiers(this IEnumerable<MemberDeclarationSyntax> decl)
        {
            if (decl is null)
            {
                throw new ArgumentNullException(nameof(decl));
            }

            return Yield();

            IEnumerable<SyntaxToken> Yield()
            {
                List<SyntaxToken> tokens = new();

                foreach (TypeDeclarationSyntax d in decl)
                {
                    if (d is null)
                    {
                        continue;
                    }

                    foreach (SyntaxToken modifier in d.Modifiers)
                    {
                        if (!tokens.Exists(m => m.IsKind(modifier.Kind())))
                        {
                            tokens.Add(modifier);
                            yield return modifier;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the parameterless constructor of the specified <paramref name="type"/> if it has one.
        /// </summary>
        /// <param name="type"><see cref="INamedTypeSymbol"/> to get the parameterless constructor of.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
        public static IMethodSymbol? GetParameterlessConstructor(this INamedTypeSymbol type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return type.InstanceConstructors.FirstOrDefault(ctor => ctor.Parameters.Length == 0);
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the parameter signature of the <paramref name="method"/>.
        /// </summary>
        /// <param name="method"><see cref="IMethodSymbol"/> to get the signature of.</param>
        /// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Function pointers are not supported.</exception>
        public static string GetParameterSignature(this IMethodSymbol method)
        {
            if (method is null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            StringBuilder sb = new();

            sb.Append('(');

            if (method.Parameters.Length > 0)
            {
                foreach (IParameterSymbol parameter in method.Parameters)
                {
                    if (parameter.RefKind != RefKind.None)
                    {
                        switch (parameter.RefKind)
                        {
                            case RefKind.In:
                                sb.Append("in ");
                                break;

                            case RefKind.Out:
                                sb.Append("out ");
                                break;

                            case RefKind.Ref:
                                sb.Append("ref ");
                                break;
                        }
                    }

                    AnalysisUtilities.WriteTypeNameOfParameter(parameter.Type, sb);

                    sb.Append(", ");
                }

                sb.Remove(sb.Length - 2, 2);
            }

            sb.Append(')');
            return sb.ToString();
        }

        /// <summary>
        /// Returns all <see cref="TypeDeclarationSyntax"/>es of the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type"><see cref="INamedTypeSymbol"/> to get the <see cref="TypeDeclarationSyntax"/>es of.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
        public static IEnumerable<T> GetPartialDeclarations<T>(this INamedTypeSymbol type, CancellationToken cancellationToken = default) where T : TypeDeclarationSyntax
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return type.DeclaringSyntaxReferences.Select(e => e.GetSyntax(cancellationToken)).OfType<T>();
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
        /// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="method"/> is not associated with a <see cref="MethodDeclarationSyntax"/>.</exception>
        public static MethodDeclarationSyntax? GetSyntax(this IMethodSymbol method, CancellationToken cancellationToken = default)
        {
            return method.GetSyntax<MethodDeclarationSyntax>(cancellationToken);
        }

        /// <summary>
        /// Returns the effective underlaying element type of the <paramref name="array"/> or any of its element array types.
        /// </summary>
        /// <param name="array"><see cref="IArrayTypeSymbol"/> to get the effective underlaying type of.</param>
        /// <returns>The effective underlaying type the <paramref name="array"/> or any of its element array types. -or- <paramref name="array"/> if no such type was found.</returns>
        public static ITypeSymbol GetUnderlayingElementType(this IArrayTypeSymbol array)
        {
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array));
            }

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
        public static ITypeSymbol GetUnderlayingPointedAtType(this IPointerTypeSymbol pointer)
        {
            if (pointer is null)
            {
                throw new ArgumentNullException(nameof(pointer));
            }

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
        /// Returns a new <see cref="UsingDirectiveSyntax"/> build for the specified <paramref name="namespace"/> symbol.
        /// </summary>
        /// <param name="namespace"><see cref="INamespaceSymbol"/> to built the <see cref="UsingDirectiveSyntax"/> from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="namespace"/> is <see langword="null"/>.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="namespaces"/> is <see langword="null"/>.</exception>
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
        /// Returns a <see cref="string"/> representing the fully qualified name of the <paramref name="symbol"/> that can be used in the XML documentation.
        /// </summary>
        /// <param name="symbol"><see cref="ISymbol"/> to get the fully qualified name of.</param>
        /// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
        public static string GetXmlFullyQualifiedName(this ISymbol symbol)
        {
            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            return AnalysisUtilities.ConvertFullyQualifiedNameToXml(symbol.ToString());
        }

        /// <summary>
        /// Checks if an attribute of type <paramref name="attrSymbol"/> is defined on the target <paramref name="symbol"/>
        /// </summary>
        /// <param name="symbol"><see cref="ISymbol"/> to check if contains the specified attribute.</param>
        /// <param name="attrSymbol"><see cref="INamedTypeSymbol"/> of attribute to check for.</param>
        /// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>. -or- <paramref name="attrSymbol"/> is <see langword="null"/>.</exception>
        public static bool HasAttribute(this ISymbol symbol, INamedTypeSymbol attrSymbol)
        {
            return GetAttribute(symbol, attrSymbol) is not null;
        }

        /// <summary>
        /// Determines whether the <paramref name="first"/> <see cref="IMethodSymbol"/> has equivalent parameters to the <paramref name="second"/> <see cref="IMethodSymbol"/>.
        /// </summary>
        /// <param name="first">First <see cref="IMethodSymbol"/>.</param>
        /// <param name="second">Second <see cref="IMethodSymbol"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="first"/> is <see langword="null"/>. -or <paramref name="second"/> is <see langword="null"/>.</exception>
        public static bool HasEquivalentParameters(this IMethodSymbol first, IMethodSymbol second)
        {
            if (first is null)
            {
                throw new ArgumentNullException(nameof(first));
            }

            if (second is null)
            {
                throw new ArgumentNullException(nameof(second));
            }

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
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
        public static bool HasExplicitBaseType(this INamedTypeSymbol type, CSharpCompilation compilation)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (compilation is null)
            {
                throw new ArgumentNullException(nameof(compilation));
            }

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
        /// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>.</exception>
        public static bool HasImplementation(this IMethodSymbol method)
        {
            if (method is null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            return !(method.IsExtern || method.IsAbstract || method.IsImplicitlyDeclared || method.IsPartialDefinition);
        }

        /// <summary>
        /// Determines whether the target <paramref name="type"/> inherits the <paramref name="baseType"/>.
        /// </summary>
        /// <param name="type">Type to check if inherits the <paramref name="baseType"/>.</param>
        /// <param name="baseType">Base type to check if is inherited by the target <paramref name="type"/>.</param>
        /// <param name="toReturnIfSame">Determines what to return when the <paramref name="type"/> and <paramref name="baseType"/> are the same.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>. -or - <paramref name="baseType"/> is <see langword="null"/>.</exception>
        public static bool InheritsFrom(this ITypeSymbol type, ITypeSymbol baseType, bool toReturnIfSame = true)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (baseType is null)
            {
                throw new ArgumentNullException(nameof(baseType));
            }

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
        /// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>. -or- <paramref name="event"/> is <see langword="null"/>.</exception>
        public static bool IsAccessor(this IMethodSymbol method, IEventSymbol @event)
        {
            if (method is null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            if (@event is null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            return SymbolEqualityComparer.Default.Equals(method.AssociatedSymbol, @event);
        }

        /// <summary>
        /// Determines whether the specified <paramref name="method"/> is an accessor of the given <paramref name="property"/>.
        /// </summary>
        /// <param name="method"><see cref="IMethodSymbol"/> to check if is an accessor of the given <paramref name="property"/>.</param>
        /// <param name="property"><see cref="IEventSymbol"/> to check if the <paramref name="method"/> is an accessor of.</param>
        /// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>. -or- <paramref name="property"/> is <see langword="null"/>.</exception>
        public static bool IsAccessor(this IMethodSymbol method, IPropertySymbol property)
        {
            if (method is null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            if (property is null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            return SymbolEqualityComparer.Default.Equals(method.AssociatedSymbol, property);
        }

        /// <summary>
        /// Determines whether the specified <paramref name="method"/> is a property or event accessor.
        /// </summary>
        /// <param name="method"><see cref="IMethodSymbol"/> to check if is a property or event accessor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>.</exception>
        public static bool IsAccessor(this IMethodSymbol method)
        {
            if (method is null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            return method.AssociatedSymbol is not null && method.AssociatedSymbol is IPropertySymbol or IEventSymbol;
        }

        /// <summary>
        /// Determines whether the specified <paramref name="symbol"/> is an attribute type.
        /// </summary>
        /// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is an attribute type.</param>
        /// <param name="compilation"><see cref="CSharpCompilation"/> that is used to resolve the <see cref="Attribute"/> type.</param>
        /// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Type '<see cref="Attribute"/>' could not be resolved.</exception>
        public static bool IsAttribute(this INamedTypeSymbol symbol, CSharpCompilation compilation)
        {
            if (compilation is null)
            {
                throw new ArgumentNullException(nameof(compilation));
            }

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
        /// <exception cref="ArgumentNullException"><paramref name="property"/> is <see langword="null"/>.</exception>
        public static bool IsAutoProperty(this IPropertySymbol property)
        {
            if (property is null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            return !property.IsIndexer && property.GetBackingField() is not null;
        }

        /// <summary>
        /// Determines whether the specified <paramref name="method"/> is an accessor of an auto-implemented property.
        /// </summary>
        /// <param name="method"><see cref="IMethodSymbol"/> to check if is an accessor of an auto-implemented property.</param>
        /// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>.</exception>
        public static bool IsAutoPropertyAccessor(this IMethodSymbol method)
        {
            if (method is null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            return method.AssociatedSymbol is IPropertySymbol property && property.GetBackingField() is not null;
        }

        /// <summary>
        /// Determines whether the specified <paramref name="field"/> is a backing field of a property.
        /// </summary>
        /// <param name="field"><see cref="IFieldSymbol"/> to check if is a backing field.</param>
        /// <exception cref="ArgumentNullException"><paramref name="field"/> is <see langword="null"/>.</exception>
        public static bool IsBackingField(this IFieldSymbol field)
        {
            if (field is null)
            {
                throw new ArgumentNullException(nameof(field));
            }

            return field.AssociatedSymbol is not null;
        }

        /// <summary>
        /// Determines whether the specified <paramref name="field"/> is a backing field of the given <paramref name="property"/>.
        /// </summary>
        /// <param name="field"><see cref="IFieldSymbol"/> to check if is a backing field of the given <paramref name="property"/>.</param>
        /// <param name="property"><see cref="IParameterSymbol"/> to check if the specified <paramref name="field"/> is a backing field of.</param>
        /// <exception cref="ArgumentNullException"><paramref name="field"/> is <see langword="null"/>. -or- <paramref name="property"/> is <see langword="null"/>.</exception>
        public static bool IsBackingField(this IFieldSymbol field, IPropertySymbol property)
        {
            if (field is null)
            {
                throw new ArgumentNullException(nameof(field));
            }

            if (property is null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            return SymbolEqualityComparer.Default.Equals(field.AssociatedSymbol, property);
        }

        /// <summary>
        /// Determines whether the <paramref name="first"/> <see cref="IParameterSymbol"/> is equivalent to the <paramref name="second"/> <see cref="IParameterSymbol"/>.
        /// </summary>
        /// <param name="first">First <see cref="IParameterSymbol"/>.</param>
        /// <param name="second">Second <see cref="IParameterSymbol"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="first"/> is <see langword="null"/>. -or <paramref name="second"/> is <see langword="null"/>.</exception>
        public static bool IsEquivalentTo(this IParameterSymbol first, IParameterSymbol second)
        {
            if (first is null)
            {
                throw new ArgumentNullException(nameof(first));
            }

            if (second is null)
            {
                throw new ArgumentNullException(nameof(second));
            }

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
        /// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>.</exception>
        public static bool IsEventAccessor(this IMethodSymbol method)
        {
            if (method is null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            return method.AssociatedSymbol is IEventSymbol;
        }

        /// <summary>
        /// Determines whether the specified <paramref name="symbol"/> is an exception type.
        /// </summary>
        /// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is an exception type.</param>
        /// <param name="compilation"><see cref="CSharpCompilation"/> that is used to resolve the <see cref="Exception"/> type.</param>
        /// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Type '<see cref="Exception"/>' could not be resolved.</exception>
        public static bool IsException(this INamedTypeSymbol symbol, CSharpCompilation compilation)
        {
            if (compilation is null)
            {
                throw new ArgumentNullException(nameof(compilation));
            }

            if (compilation.GetTypeByMetadataName("System.Exception") is not INamedTypeSymbol exc)
            {
                throw new InvalidOperationException("Type 'System.Exception' could not be resolved!");
            }

            return symbol.InheritsFrom(exc);
        }

        /// <summary>
        /// Determines whether the <paramref name="symbol"/> was generated from the <paramref name="target"/> <see cref="ISymbol"/>.
        /// </summary>
        /// <param name="symbol"><see cref="ISymbol"/> to check.</param>
        /// <param name="target"><see cref="ISymbol"/> to check if the <paramref name="symbol"/> is generated from.</param>
        /// <param name="compilation"><see cref="CompilationDataWithSymbols"/> to get the needed <see cref="INamedTypeSymbol"/> from.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="symbol"/> is <see langword="null"/>. -or-
        /// <paramref name="target"/> is <see langword="null"/>. -or-
        /// <paramref name="compilation"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">Target <paramref name="compilation"/> has errors.</exception>
        public static bool IsGeneratedFrom(this ISymbol symbol, ISymbol target, CompilationDataWithSymbols compilation)
        {
            return IsGeneratedFrom(symbol, target?.ToString()!, compilation);
        }

        /// <summary>
        /// Determines whether the <paramref name="symbol"/> was generated from the <paramref name="target"/> member.
        /// </summary>
        /// <param name="symbol"><see cref="ISymbol"/> to check.</param>
        /// <param name="target"><see cref="string"/> representing a <see cref="ISymbol"/> to check if the <paramref name="symbol"/> was generated from.</param>
        /// <param name="compilation"><see cref="CompilationDataWithSymbols"/> to get the needed <see cref="INamedTypeSymbol"/> from.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="symbol"/> is <see langword="null"/>. -or-
        /// <paramref name="target"/> is <see langword="null"/>. -or-
        /// <paramref name="compilation"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">Target <paramref name="compilation"/> has errors.</exception>
        public static bool IsGeneratedFrom(this ISymbol symbol, string target, CompilationDataWithSymbols compilation)
        {
            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            if (target is null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (compilation is null)
            {
                throw new ArgumentNullException(nameof(compilation));
            }

            if (compilation.HasErrors)
            {
                throw new InvalidOperationException($"Target {nameof(compilation)} has errors!");
            }

            AttributeData? attribute = symbol.GetAttribute(compilation.DurianGeneratedAttribute);

            if (attribute is null)
            {
                return false;
            }

            return attribute.ConstructorArguments.FirstOrDefault().Value is string value && value == target;
        }

        /// <summary>
        /// Determines whether the specified <paramref name="type"/> is an inner type.
        /// </summary>
        /// <param name="type"><see cref="INamedTypeSymbol"/> to check if is an inner type.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
        public static bool IsInnerType(this INamedTypeSymbol type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return type.ContainingType is not null;
        }

        /// <summary>
        /// Checks if the specified <paramref name="type"/> is the <paramref name="typeParameter"/> or if it uses it as its element type (for <see cref="IArrayTypeSymbol"/>) or pointed at type (for <see cref="IPointerTypeSymbol"/>).
        /// </summary>
        /// <param name="type"><see cref="ITypeSymbol"/> to check.</param>
        /// <param name="typeParameter"><see cref="ITypeParameterSymbol"/> to check if is used by the target <paramref name="type"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>. -or- <paramref name="typeParameter"/> is <see langword="null"/>.</exception>
        public static bool IsOrUsesTypeParameter(this ITypeSymbol type, ITypeParameterSymbol typeParameter)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (typeParameter is null)
            {
                throw new ArgumentNullException(nameof(typeParameter));
            }

            if (SymbolEqualityComparer.Default.Equals(type, typeParameter))
            {
                return true;
            }

            ITypeSymbol symbol;

            if (type is IArrayTypeSymbol array)
            {
                symbol = array.GetUnderlayingElementType();
            }
            else if (type is IPointerTypeSymbol pointer)
            {
                symbol = pointer.GetUnderlayingPointedAtType();
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
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
        public static bool IsPartial(this INamedTypeSymbol type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

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
        /// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>. -or- <paramref name="declarationRetriever"/> is <see langword="null"/>.</exception>
        public static bool IsPartial(this IMethodSymbol method, Func<MethodDeclarationSyntax?> declarationRetriever)
        {
            if (method is null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            if (declarationRetriever is null)
            {
                throw new ArgumentNullException(nameof(declarationRetriever));
            }

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
        /// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>.</exception>
        public static bool IsPartial(this IMethodSymbol method, MethodDeclarationSyntax? declaration = default)
        {
            return method.IsPartial(() => declaration ?? method?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as MethodDeclarationSyntax);
        }

        /// <summary>
        /// Determines whether the specified <paramref name="type"/> is <see langword="partial"/> and all its containing types are also <see langword="partial"/>.
        /// </summary>
        /// <param name="type"><see cref="INamedTypeSymbol"/> to check if is <see langword="partial"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
        public static bool IsPartialContext(this INamedTypeSymbol type)
        {
            return type.IsPartial() && type.GetContainingTypeSymbols().All(t => t.IsPartial());
        }

        /// <summary>
        /// Determines whether the <paramref name="method"/> is <see langword="partial"/> and all its containing types are also <see langword="partial"/>.
        /// </summary>
        /// <param name="method"><see cref="IMethodSymbol"/> to check.</param>
        /// <param name="declaration">Main declaration of this <paramref name="method"/>.</param>
        /// <remarks>If <paramref name="declaration"/> is <see langword="null"/>, a <see cref="MethodDeclarationSyntax"/> is retrieved using the <see cref="ISymbol.DeclaringSyntaxReferences"/> property.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>.</exception>
        public static bool IsPartialContext(this IMethodSymbol method, MethodDeclarationSyntax? declaration = default)
        {
            return method.IsPartial(declaration) && method.GetContainingTypeSymbols().All(t => t.IsPartial());
        }

        /// <summary>
        /// Determines whether the specified <paramref name="method"/> is <see langword="partial"/> and all its containing types are also <see langword="partial"/>..
        /// </summary>
        /// <param name="method"><see cref="IMethodSymbol"/> to check if is <see langword="partial"/>.</param>
        /// <param name="declarationRetriever">Function that returns a <see cref="MethodDeclarationSyntax"/> of the specified <paramref name="method"/> if it is needed.</param>
        /// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>. -or- <paramref name="declarationRetriever"/> is <see langword="null"/>.</exception>
        public static bool IsPartialContext(this IMethodSymbol method, Func<MethodDeclarationSyntax?> declarationRetriever)
        {
            return method.IsPartial(declarationRetriever) && method.GetContainingTypeSymbols().All(t => t.IsPartial());
        }

        /// <summary>
        /// Determines whether the <paramref name="type"/> is a predefined type (any primitive, <see cref="string"/>, <see cref="void"/>, <see cref="object"/>).
        /// </summary>
        /// <param name="type">Type to check.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
        public static bool IsPredefined(this ITypeSymbol type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

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
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
        /// <returns><see langword="true"/> if the type is predefined or dynamic, otherwise <see langword="false"/>.</returns>
        public static bool IsPredefinedOrDynamic(this ITypeSymbol type, ICompilationData compilation)
        {
            if (compilation is null)
            {
                throw new ArgumentNullException(nameof(compilation));
            }

            return IsPredefined(type) || SymbolEqualityComparer.Default.Equals(type, compilation.Compilation.DynamicType);
        }

        /// <summary>
        /// Determines whether the <paramref name="type"/> is a predefined type (any primitive, <see cref="string"/>, <see cref="void"/>, <see cref="object"/>) or a dynamic type.
        /// </summary>
        /// <param name="type">Type to check.</param>
        /// <param name="compilation"><see cref="CSharpCompilation"/> to get the dynamic symbol from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
        /// <returns><see langword="true"/> if the type is predefined or dynamic, otherwise <see langword="false"/>.</returns>
        public static bool IsPredefinedOrDynamic(this ITypeSymbol type, CSharpCompilation compilation)
        {
            if (compilation is null)
            {
                throw new ArgumentNullException(nameof(compilation));
            }

            return IsPredefined(type) || SymbolEqualityComparer.Default.Equals(type, compilation.DynamicType);
        }

        /// <summary>
        /// Determines whether the specified <paramref name="method"/> is a property accessor.
        /// </summary>
        /// <param name="method"><see cref="IMethodSymbol"/> to check if is a property accessor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>.</exception>
        public static bool IsPropertyAccessor(this IMethodSymbol method)
        {
            if (method is null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            return method.AssociatedSymbol is IPropertySymbol;
        }

        /// <summary>
        /// Determines whether the specified <paramref name="type"/> is of sealed kind (struct or sealed/static class).
        /// </summary>
        /// <param name="type"><see cref="INamedTypeSymbol"/> to check if is sealed.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
        public static bool IsSealedKind(this INamedTypeSymbol type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return type.IsSealed || type.IsValueType || type.IsStatic;
        }

        /// <summary>
        /// Determines whether the <paramref name="type"/> can be applied to the <paramref name="parameter"/>.
        /// </summary>
        /// <param name="type"><see cref="INamedTypeSymbol"/> to check if is valid for the <paramref name="parameter"/>.</param>
        /// <param name="parameter">Target <see cref="ITypeParameterSymbol"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>. -or - <paramref name="parameter"/> is <see langword="null"/>.</exception>
        public static bool IsValidForTypeParameter(this INamedTypeSymbol type, ITypeParameterSymbol parameter)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (parameter is null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            return IsValidForTypeParameter_Internal(type, parameter);
        }

        /// <summary>
        /// Determines whether the <paramref name="arrayType"/> can be applied to the <paramref name="parameter"/>.
        /// </summary>
        /// <param name="arrayType"><see cref="IArrayTypeSymbol"/> to check if is valid for the <paramref name="parameter"/>.</param>
        /// <param name="parameter">Target <see cref="ITypeParameterSymbol"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="arrayType"/> is <see langword="null"/>. -or - <paramref name="parameter"/> is <see langword="null"/>.</exception>
        public static bool IsValidForTypeParameter(this IArrayTypeSymbol arrayType, ITypeParameterSymbol parameter)
        {
            if (arrayType is null)
            {
                throw new ArgumentNullException(nameof(arrayType));
            }

            if (parameter is null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            return IsValidForTypeParameter_Internal(arrayType, parameter);
        }

        /// <summary>
        /// Determines whether the <paramref name="dynamicType"/> can be applied to the <paramref name="parameter"/>.
        /// </summary>
        /// <param name="dynamicType"><see cref="IDynamicTypeSymbol"/> to check if is valid for the <paramref name="parameter"/>.</param>
        /// <param name="parameter">Target <see cref="ITypeParameterSymbol"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="dynamicType"/> is <see langword="null"/>. -or - <paramref name="parameter"/> is <see langword="null"/>.</exception>
        public static bool IsValidForTypeParameter(this IDynamicTypeSymbol dynamicType, ITypeParameterSymbol parameter)
        {
            if (dynamicType is null)
            {
                throw new ArgumentNullException(nameof(dynamicType));
            }

            if (parameter is null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            return IsValidForTypeParameter_Internal(dynamicType, parameter);
        }

        /// <summary>
        /// Determines whether the <paramref name="type"/> can be applied to the <paramref name="parameter"/>.
        /// </summary>
        /// <param name="type"><see cref="ITypeSymbol"/> to check if is valid for the <paramref name="parameter"/>.</param>
        /// <param name="parameter">Target <see cref="ITypeParameterSymbol"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>. -or - <paramref name="parameter"/> is <see langword="null"/>.</exception>
        /// <remarks>Symbols other than <see cref="INamedTypeSymbol"/>, <see cref="IArrayTypeSymbol"/>, <see cref="ITypeParameterSymbol"/> and <see cref="IDynamicTypeSymbol"/> will never be valid.</remarks>
        public static bool IsValidForTypeParameter(this ITypeSymbol type, ITypeParameterSymbol parameter)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (parameter is null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (type is INamedTypeSymbol or IArrayTypeSymbol or ITypeParameterSymbol or IDynamicTypeSymbol)
            {
                return IsValidForTypeParameter_Internal(type, parameter);
            }

            return false;
        }

        /// <summary>
        /// Returns a <see cref="QualifiedNameSyntax"/> created from the specified <paramref name="namespaces"/>.
        /// </summary>
        /// <param name="namespaces">A collection of <see cref="INamespaceSymbol"/>s to create the <see cref="QualifiedNameSyntax"/> from.</param>
        /// <returns>A <see cref="QualifiedNameSyntax"/> created by combining the <paramref name="namespaces"/>. -or- <see langword="null"/> if there were less then 2 <paramref name="namespaces"/> provided.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="namespaces"/> is <see langword="null"/>.</exception>
        public static QualifiedNameSyntax? JoinIntoQualifiedName(this IEnumerable<INamespaceSymbol> namespaces)
        {
            if (namespaces is null)
            {
                throw new ArgumentNullException(nameof(namespaces));
            }

            return AnalysisUtilities.JoinIntoQualifiedName(namespaces.Select(n => n.Name));
        }

        /// <summary>
        /// Returns a <see cref="string"/> that is created by joining the names of the namespaces the provided <paramref name="symbol"/> is contained in.
        /// </summary>
        /// <param name="symbol"><see cref="ISymbol"/> to get the containing namespaces of.</param>
        /// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
        public static string JoinNamespaces(this ISymbol symbol)
        {
            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            if (symbol.ContainingNamespace is null || symbol.ContainingNamespace.IsGlobalNamespace)
            {
                return string.Empty;
            }

            return JoinNamespaces(symbol.GetContainingNamespaces(false));
        }

        /// <summary>
        /// Returns a <see cref="string"/> that is created by joining the names of the provided <paramref name="namespaces"/>.
        /// </summary>
        /// <param name="namespaces">A collection of <see cref="INamespaceSymbol"/>s create the <see cref="string"/> from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="namespaces"/> is <see langword="null"/>.</exception>
        public static string JoinNamespaces(this IEnumerable<INamespaceSymbol> namespaces)
        {
            if (namespaces is null)
            {
                throw new ArgumentNullException(nameof(namespaces));
            }

            StringBuilder sb = new();

            foreach (INamespaceSymbol n in namespaces)
            {
                if (n is null)
                {
                    continue;
                }

                sb.Append(n.Name).Append('.');
            }

            return sb.ToString().TrimEnd('.');
        }

        /// <summary>
        /// Determines whether the specified <paramref name="method"/> is overrides the <paramref name="other"/> symbol either directly or through additional child types.
        /// </summary>
        /// <param name="method"><see cref="IMethodSymbol"/> to check if overrides the <paramref name="other"/> symbol.</param>
        /// <param name="other"><see cref="IMethodSymbol"/> to check if is overridden by the specified <paramref name="method"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>. -or- <paramref name="other"/> is <see langword="null"/>.</exception>
        public static bool Overrides(this IMethodSymbol method, IMethodSymbol other)
        {
            if (method is null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

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
        /// Attempts to return a <see cref="CSharpSyntaxNode"/> of type <typeparamref name="T"/> associated with the specified <paramref name="method"/>.
        /// </summary>
        /// <typeparam name="T">Type of <see cref="CSharpSyntaxNode"/> to return.</typeparam>
        /// <param name="method"><see cref="IMethodSymbol"/> to get the <see cref="CSharpSyntaxNode"/> associated with.</param>
        /// <param name="syntax"><see cref="CSharpSyntaxNode"/> associated with the specified <paramref name="method"/>.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
        /// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>.</exception>
        public static bool TryGetSyntax<T>(this IMethodSymbol method, [NotNullWhen(true)] out T? syntax, CancellationToken cancellationToken = default) where T : CSharpSyntaxNode
        {
            if (method is null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            syntax = method.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken) as T;
            return syntax is not null;
        }

        /// <summary>
        /// Attempts to return a <see cref="MethodDeclarationSyntax"/> associated with the specified <paramref name="method"/>.
        /// </summary>
        /// <param name="method"><see cref="IMethodSymbol"/> to get the <see cref="MethodDeclarationSyntax"/> associated with.</param>
        /// <param name="syntax"><see cref="CSharpSyntaxNode"/> associated with the specified <paramref name="method"/>.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
        /// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>.</exception>
        public static bool TryGetSyntax(this IMethodSymbol method, [NotNullWhen(true)] out MethodDeclarationSyntax? syntax, CancellationToken cancellationToken = default)
        {
            return method.TryGetSyntax<MethodDeclarationSyntax>(out syntax, cancellationToken);
        }

        private static IEnumerable<INamedTypeSymbol> GetInnerTypesInternal(INamedTypeSymbol symbol)
        {
            foreach (INamedTypeSymbol s in symbol.GetTypeMembers())
            {
                yield return s;

                foreach (INamedTypeSymbol inner in GetInnerTypesInternal(s))
                {
                    yield return inner;
                }
            }
        }

        private static bool IsValidForTypeParameter_Internal(ITypeSymbol type, ITypeParameterSymbol parameter)
        {
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
                    if (!IsValidForTypeParameter_Internal(type, p))
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
    }
}