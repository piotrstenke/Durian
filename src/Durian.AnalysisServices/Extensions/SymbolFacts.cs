// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections;
using System.Collections.Generic;
using Durian.Generator;
using System.Diagnostics.CodeAnalysis;
using Durian.Analysis.CodeGeneration;
using Durian.Analysis.SymbolContainers;

#if ENABLE_REFLECTION

using System.Reflection;

#endif

namespace Durian.Analysis.Extensions
{
	/// <summary>
	/// Contains methods that determine state and properties is a given <see cref="ISymbol"/>.
	/// </summary>
	public static class SymbolFacts
	{
		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> can potentially hide the <paramref name="other"/> symbol using the <see langword="new"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to check can hide the <paramref name="other"/> symbol.</param>
		/// <param name="other"><see cref="ISymbol"/> to check if can be hidden.</param>
		public static bool CanHideSymbol(this ISymbol symbol, ISymbol other)
		{
			return symbol switch
			{
				INamedTypeSymbol type => type.CanHideSymbol(other),
				IMethodSymbol method => method.CanHideSymbol(other),
				IPropertySymbol property => property.CanHideSymbol(other),
				IFieldSymbol field => field.CanHideSymbol(other),
				IEventSymbol @event => @event.CanHideSymbol(other),
				_ => false
			};
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> can potentially hide the <paramref name="other"/> symbol using the <see langword="new"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check can hide the <paramref name="other"/> symbol.</param>
		/// <param name="other"><see cref="ISymbol"/> to check if can be hidden.</param>
		public static bool CanHideSymbol(this INamedTypeSymbol type, ISymbol other)
		{
			if (!type.IsDeclarationKind())
			{
				return false;
			}

			return other switch
			{
				INamedTypeSymbol otherType => type.Arity == otherType.Arity && type.Name == otherType.Name,
				IMethodSymbol method => CanHideSymbol_Internal(type, method),
				IPropertySymbol property => CanHideSymbol_Internal(type, property),
				IEventSymbol or IFieldSymbol => type.Arity == 0 && type.Name == other.Name,
				_ => false
			};
		}

		/// <summary>
		/// Determines whether the specified <paramref name="method"/> can potentially hide the <paramref name="other"/> symbol using the <see langword="new"/>.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to check can hide the <paramref name="other"/> symbol.</param>
		/// <param name="other"><see cref="ISymbol"/> to check if can be hidden.</param>
		public static bool CanHideSymbol(this IMethodSymbol method, ISymbol other)
		{
			if (method.MethodKind != MethodKind.Ordinary)
			{
				return false;
			}

			return other switch
			{
				INamedTypeSymbol type => CanHideSymbol_Internal(type, method),
				IMethodSymbol otherMethod => CanHideSymbol_Internal(method, otherMethod),
				IPropertySymbol property => CanHideSymbol_Internal(method, property),
				IEventSymbol or IFieldSymbol => method.Arity == 0 && method.Name == other.Name,
				_ => false
			};
		}

		/// <summary>
		/// Determines whether the specified <paramref name="property"/> can potentially hide the <paramref name="other"/> symbol using the <see langword="new"/>.
		/// </summary>
		/// <param name="property"><see cref="IPropertySymbol"/> to check can hide the <paramref name="other"/> symbol.</param>
		/// <param name="other"><see cref="ISymbol"/> to check if can be hidden.</param>
		public static bool CanHideSymbol(this IPropertySymbol property, ISymbol other)
		{
			if (property.IsIndexer)
			{
				if (other is IPropertySymbol { IsIndexer: true } indexer)
				{
					return ParametersAreEquivalent(property.Parameters, indexer.Parameters);
				}

				return false;
			}

			return other switch
			{
				INamedTypeSymbol type => CanHideSymbol_Internal(type, property),
				IMethodSymbol method => CanHideSymbol_Internal(method, property),
				IPropertySymbol otherProperty => !otherProperty.IsIndexer && property.Name == otherProperty.Name,
				IEventSymbol or IFieldSymbol => property.Name == other.Name,
				_ => false
			};
		}

		/// <summary>
		/// Determines whether the specified <paramref name="field"/> can potentially hide the <paramref name="other"/> symbol using the <see langword="new"/>.
		/// </summary>
		/// <param name="field"><see cref="IFieldSymbol"/> to check can hide the <paramref name="other"/> symbol.</param>
		/// <param name="other"><see cref="ISymbol"/> to check if can be hidden.</param>
		public static bool CanHideSymbol(this IFieldSymbol field, ISymbol other)
		{
			return other switch
			{
				INamedTypeSymbol type => type.Arity == 0 && type.Name == field.Name,
				IMethodSymbol method => method.Arity == 0 && method.Name == field.Name,
				IPropertySymbol property => !property.IsIndexer && property.Name == field.Name,
				IEventSymbol or IFieldSymbol => other.Name == field.Name,
				_ => false
			};
		}

		/// <summary>
		/// Determines whether the specified <paramref name="event"/> can potentially hide the <paramref name="other"/> symbol using the <see langword="new"/>.
		/// </summary>
		/// <param name="event"><see cref="IEventSymbol"/> to check can hide the <paramref name="other"/> symbol.</param>
		/// <param name="other"><see cref="ISymbol"/> to check if can be hidden.</param>
		public static bool CanHideSymbol(this IEventSymbol @event, ISymbol other)
		{
			return other switch
			{
				INamedTypeSymbol type => type.Arity == 0 && type.Name == @event.Name,
				IMethodSymbol method => method.Arity == 0 && method.Name == @event.Name,
				IPropertySymbol property => !property.IsIndexer && property.Name == @event.Name,
				IEventSymbol or IFieldSymbol => other.Name == @event.Name,
				_ => false
			};
		}

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
		/// Checks if an attribute of type <paramref name="attrSymbol"/> is defined on the target <paramref name="symbol"/>
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to check if contains the specified attribute.</param>
		/// <param name="attrSymbol"><see cref="INamedTypeSymbol"/> of attribute to check for.</param>
		public static bool HasAttribute(this ISymbol symbol, INamedTypeSymbol attrSymbol)
		{
			return symbol.GetAttribute(attrSymbol) is not null;
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
		/// Determines whether the specified <paramref name="type"/> has an explicitly specified base type.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check if has an explicit base type.</param>
		public static bool HasExplicitBaseType(this INamedTypeSymbol type)
		{
			if (type.BaseType is null || type.TypeKind != TypeKind.Class)
			{
				return false;
			}

			return type.BaseType.SpecialType != SpecialType.System_Object;
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
		/// Determine whether the specified <paramref name="type"/> is asynchronously disposable (implements the <see cref="IAsyncDisposable"/> interface).
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check if is asynchronously disposable.</param>
		public static bool IsAsyncDisposable(this INamedTypeSymbol type)
		{
			return type.AllInterfaces.Any(intf => IsSpecialInterface(intf, nameof(IAsyncDisposable)) && type.IsWithinNamespace(nameof(System), false));
		}

		/// <summary>
		/// Determine whether the specified <paramref name="type"/> is asynchronously enumerable.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check if is asynchronously enumerable.</param>
		/// <param name="requireInterface">Determines whether the <see cref="INamedTypeSymbol"/> must implement the <see cref="IAsyncEnumerable{T}"/> interface.</param>
		public static bool IsAsyncEnumerable(this INamedTypeSymbol type, bool requireInterface = false)
		{
			if (type.AllInterfaces.Any(intf => IsSpecialInterface(intf, "IAsyncEnumerable") && intf.Arity == 1 && intf.IsWithinNamespace("System.Collections.Generic")))
			{
				return true;
			}

			if (requireInterface)
			{
				return false;
			}

			return type
				.GetAllMembers("GetAsyncEnumerator")
				.OfType<IMethodSymbol>()
				.Any(method => method.IsSpecialMember(SpecialMember.GetAsyncEnumerator));
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is an asynchronous enumerator type.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check whether is an asynchronous enumerator.</param>
		/// <param name="requireInterface">Determines whether the <see cref="INamedTypeSymbol"/> must implement the <see cref="IAsyncEnumerator{T}"/> interface.</param>
		public static bool IsAsyncEnumerator(this INamedTypeSymbol type, bool requireInterface = false)
		{
			if (type.AllInterfaces.Any(intf => IsSpecialInterface(intf, "IAsyncEnumerator") && intf.Arity == 1 && intf.IsWithinNamespace("System.Collections.Generic")))
			{
				return true;
			}

			if (requireInterface)
			{
				return false;
			}

			bool hasCurrent = false;
			bool hasMoveNext = false;

			foreach (ISymbol symbol in type.GetAllMembers())
			{
				if (!hasCurrent && symbol.IsSpecialMember(SpecialMember.Current))
				{
					if (hasMoveNext)
					{
						return true;
					}

					hasCurrent = true;
				}
				else if (!hasMoveNext && symbol.IsSpecialMember(SpecialMember.MoveNextAsync))
				{
					if (hasCurrent)
					{
						return true;
					}

					hasMoveNext = true;
				}
			}

			return false;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="method"/> is an asynchronous iterator.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to determine whether is an asynchronous iterator.</param>
		/// <param name="allowEnumerator">Determines whether the method can be an asynchronous iterator which returns <see cref="IAsyncEnumerator{T}"/> instead of <see cref="IAsyncEnumerable{T}"/>.</param>
		public static bool IsAsyncIterator(this IMethodSymbol method, bool allowEnumerator = true)
		{
			if (allowEnumerator)
			{
				return IsIterator(method, "IAsyncEnumerable", "IAsyncEnumerator", true);
			}

			return IsIterator(method, "IAsyncEnumerable", null, true);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is an attribute type.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check if is an attribute type.</param>
		public static bool IsAttribute(this INamedTypeSymbol type)
		{
			return type.GetBaseTypes(true).Any(t => t.Name == nameof(Attribute) && t.IsWithinNamespace(nameof(System), false));
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
		/// Determines whether the specified <paramref name="type"/> is awaitable.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check whether is awaitable.</param>
		/// <param name="requireVoid">Determines whether the awaiter must return <see langword="void"/> as result.</param>
		public static bool IsAwaitable(this INamedTypeSymbol type, bool requireVoid = false)
		{
			IMethodSymbol? method = type
				.GetAllMembers()
				.OfType<IMethodSymbol>()
				.FirstOrDefault(method => method.IsSpecialMember(SpecialMember.GetAwaiter));

			if (method is null)
			{
				return false;
			}

			if (requireVoid && !method.ReturnsVoid)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is awaitable with a given type of <paramref name="resultType"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check whether is awaitable with a given type of <paramref name="resultType"/>.</param>
		/// <param name="resultType">Type of result of awaiting the <paramref name="type"/>.</param>
		public static bool IsAwaitable(this INamedTypeSymbol type, INamedTypeSymbol resultType)
		{
			return type
				.GetAllMembers()
				.OfType<IMethodSymbol>()
				.Any(method => method.DeclaredAccessibility == Accessibility.Public && IsGetAwaiter(method, resultType));
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is awaitable with a given type of <paramref name="resultType"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check whether is awaitable with a given type of <paramref name="resultType"/>.</param>
		/// <param name="resultType">Type of result of awaiting the <paramref name="type"/>.</param>
		public static bool IsAwaitable(this INamedTypeSymbol type, SpecialType resultType)
		{
			return type
				.GetAllMembers()
				.OfType<IMethodSymbol>()
				.Any(method =>
				{
					if (method.DeclaredAccessibility != Accessibility.Public)
					{
						return false;
					}

					if (!method.IsGetAwaiterRaw(out INamedTypeSymbol? returnType))
					{
						return false;
					}

					return returnType.IsAwaiter(resultType);
				});
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is an awaiter.
		/// </summary>
		/// <param name="type">Determines whether the specified <paramref name="type"/> is an awaiter.</param>
		/// <param name="requireVoid">Determines whether the awaiter must return <see langword="void"/> as result.</param>
		public static bool IsAwaiter(this INamedTypeSymbol type, bool requireVoid = false)
		{
			if (!type.IsINotifyCompletion())
			{
				return false;
			}

			bool hasIsCompleted = false;
			bool hasGetResult = false;

			foreach (ISymbol symbol in type.GetAllMembers())
			{
				if (!hasIsCompleted && symbol.IsSpecialMember(SpecialMember.IsCompleted))
				{
					if (hasGetResult)
					{
						return true;
					}

					hasIsCompleted = true;
				}
				else if (!hasGetResult && symbol.IsSpecialMember(SpecialMember.GetResult))
				{
					if (requireVoid && !(symbol as IMethodSymbol)!.ReturnsVoid)
					{
						continue;
					}

					if (hasIsCompleted)
					{
						return true;
					}

					hasGetResult = true;
				}
			}

			return false;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is an awaiter with a given type of <paramref name="resultType"/>.
		/// </summary>
		/// <param name="type">Determines whether the specified <paramref name="type"/> is an awaiter with a given type of <paramref name="resultType"/>.</param>
		/// <param name="resultType">Type of result of the awaiter.</param>
		public static bool IsAwaiter(this INamedTypeSymbol type, SpecialType resultType)
		{
			if (!type.IsINotifyCompletion())
			{
				return false;
			}

			bool hasIsCompleted = false;
			bool hasGetResult = false;

			foreach (ISymbol symbol in type.GetAllMembers())
			{
				if (!hasIsCompleted && symbol.IsSpecialMember(SpecialMember.IsCompleted))
				{
					if (hasGetResult)
					{
						return true;
					}

					hasIsCompleted = true;
					continue;
				}

				if (!hasGetResult && symbol.IsSpecialMember(SpecialMember.GetResult) && (symbol as IMethodSymbol)!.ReturnType.SpecialType == resultType)
				{
					if (hasIsCompleted)
					{
						return true;
					}

					hasGetResult = true;
				}
			}

			return false;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is an awaiter with a given type of <paramref name="resultType"/>.
		/// </summary>
		/// <param name="type">Determines whether the specified <paramref name="type"/> is an awaiter with a given type of <paramref name="resultType"/>.</param>
		/// <param name="resultType">Type of result of the awaiter.</param>
		public static bool IsAwaiter(this INamedTypeSymbol type, ITypeSymbol resultType)
		{
			if (!type.IsINotifyCompletion())
			{
				return false;
			}

			bool hasIsCompleted = false;
			bool hasGetResult = false;

			foreach (ISymbol symbol in type.GetAllMembers())
			{
				if (!hasIsCompleted && symbol.IsSpecialMember(SpecialMember.IsCompleted))
				{
					if (hasGetResult)
					{
						return true;
					}

					hasIsCompleted = true;
					continue;
				}

				if (!hasGetResult && symbol.IsSpecialMember(SpecialMember.GetResult) && SymbolEqualityComparer.Default.Equals((symbol as IMethodSymbol)!.ReceiverType, resultType))
				{
					if (hasIsCompleted)
					{
						return true;
					}

					hasGetResult = true;
				}
			}

			return false;
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
		/// Determines whether the specified <paramref name="method"/> can be used as a collection initializer.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to check whether can be used as a collection initializer.</param>
		public static bool IsCollectionInitializer(this IMethodSymbol method)
		{
			return method.IsSpecialMember(SpecialMember.Add);
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
		/// Determines whether the specified <paramref name="method"/> is a constructor (either instance or static).
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to check if is a constructor.</param>
		public static bool IsConstructor(this IMethodSymbol method)
		{
			return method.MethodKind is MethodKind.Constructor or MethodKind.StaticConstructor;
		}

		/// <summary>
		/// Determines whether the <see cref="TypeKind"/> of the specified <paramref name="type"/> is a declaration kind.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to determine whether is of a declaration kind.</param>
		public static bool IsDeclarationKind(this INamedTypeSymbol type)
		{
			return type.TypeKind.IsDeclarationKind();
		}

		/// <summary>
		/// Determines whether the specified <paramref name="method"/> is a compiler-generated parameterless constructor.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to check whether is a compiler-generated parameterless constructor.</param>
		public static bool IsDefaultConstructor(this IMethodSymbol method)
		{
			return method.IsImplicitlyDeclared && method.IsParameterlessConstructor();
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is disposable (implements the <see cref="IDisposable"/> interface).
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check if is disposable.</param>
		public static bool IsDisposable(this INamedTypeSymbol type)
		{
			return type.AllInterfaces.Any(intf => intf.SpecialType == SpecialType.System_IDisposable);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> was generated using a <c>Durian</c> tool.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to check if was generated using a <c>Durian</c> tool.</param>
		public static bool IsDurianGenerated(this ISymbol symbol)
		{
			return symbol.GetAttributes().Any(IsDurianGeneratedAttribute);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> was generated from the <paramref name="target"/> <see cref="ISymbol"/> using a <c>Durian</c> tool.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to check.</param>
		/// <param name="target"><see cref="ISymbol"/> to check if the <paramref name="symbol"/> is generated from.</param>
		public static bool IsDurianGeneratedFrom(this ISymbol symbol, ISymbol target)
		{
			return symbol.IsDurianGeneratedFrom(target?.ToString()!);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> was generated from the <paramref name="target"/> member using a <c>Durian</c> tool.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to check.</param>
		/// <param name="target"><see cref="string"/> representing a <see cref="ISymbol"/> to check if the <paramref name="symbol"/> was generated from.</param>
		public static bool IsDurianGeneratedFrom(this ISymbol symbol, string target)
		{
			AttributeData? attribute = symbol.GetAttributes().FirstOrDefault(IsDurianGeneratedAttribute);

			if (attribute is null)
			{
				return false;
			}

			return attribute.ConstructorArguments.FirstOrDefault().Value is string value && value == target;
		}

		/// <summary>
		/// Determine whether the specified <paramref name="type"/> is enumerable.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check if is enumerable.</param>
		/// <param name="requireInterface">Determines whether the <see cref="INamedTypeSymbol"/> must implement the <see cref="IEnumerable"/> interface.</param>
		public static bool IsEnumerable(this INamedTypeSymbol type, bool requireInterface = false)
		{
			if (type.AllInterfaces.Any(intf => intf.SpecialType is SpecialType.System_Collections_IEnumerable or SpecialType.System_Collections_Generic_IEnumerable_T))
			{
				return true;
			}

			if (requireInterface)
			{
				return false;
			}

			return type
				.GetAllMembers("GetEnumerator")
				.OfType<IMethodSymbol>()
				.Any(method => method.IsSpecialMember(SpecialMember.GetEnumerator));
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is an enumerator type.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check whether is an enumerator.</param>
		/// <param name="requireInterface">Determines whether the <see cref="INamedTypeSymbol"/> must implement the <see cref="IEnumerator"/> interface.</param>
		public static bool IsEnumerator(this INamedTypeSymbol type, bool requireInterface = false)
		{
			if (type.AllInterfaces.Any(intf => intf.SpecialType is SpecialType.System_Collections_IEnumerator or SpecialType.System_Collections_Generic_IEnumerator_T))
			{
				return true;
			}

			if (requireInterface)
			{
				return false;
			}

			bool hasCurrent = false;
			bool hasMoveNext = false;

			foreach (ISymbol symbol in type.GetAllMembers())
			{
				if (!hasCurrent && symbol.IsSpecialMember(SpecialMember.Current))
				{
					if (hasMoveNext)
					{
						return true;
					}

					hasCurrent = true;
				}
				else if (!hasMoveNext && symbol.IsSpecialMember(SpecialMember.MoveNext))
				{
					if (hasCurrent)
					{
						return true;
					}

					hasMoveNext = true;
				}
			}

			return false;
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
		/// Determines whether the specified <paramref name="type"/> is an exception type.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check if is an exception type.</param>
		public static bool IsException(this INamedTypeSymbol type)
		{
			return type.GetBaseTypes(true).Any(t => t.Name == nameof(Exception) && t.IsWithinNamespace(nameof(System), false));
		}

		/// <summary>
		/// Determines whether the specified <paramref name="method"/> is an explicit conversion operator.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to check if is an explicit conversion operator.</param>
		public static bool IsExplicitOperator(this IMethodSymbol method)
		{
			return method.MethodKind == MethodKind.Conversion && method.Name == WellKnownMemberNames.ExplicitConversionName;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="event"/> is a field-like <see langword="event"/>.
		/// </summary>
		/// <param name="event"><see cref="IEventSymbol"/> to check if is a field-like <see langword="event"/>.</param>
		public static bool IsFieldEvent(this IEventSymbol @event)
		{
			if (@event.AddMethod is not null && !@event.AddMethod.IsImplicitlyDeclared)
			{
				return false;
			}

			if (@event.RemoveMethod is not null && !@event.RemoveMethod.IsImplicitlyDeclared)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is an enum with the <see cref="FlagsAttribute"/> applied.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to determine whether is a flags enum.</param>
		public static bool IsFlagsEnum(this INamedTypeSymbol type)
		{
			if(type.TypeKind != TypeKind.Enum)
			{
				return false;
			}

			return type.GetAttributes().Any(attr =>
				attr.AttributeClass is not null &&
				attr.AttributeClass.MetadataName == "FlagsAttribute" &&
				attr.AttributeClass.IsWithinNamespace("System", false)
			);
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
			return method.MethodKind == MethodKind.Conversion && method.Name == WellKnownMemberNames.ImplicitConversionName;
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
		/// Determines whether the specified <paramref name="method"/> is an iterator.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to determine whether is an iterator.</param>
		/// <param name="allowEnumerator">Determines whether the method can be an iterator which returns <see cref="IEnumerator"/> instead of <see cref="IEnumerable"/>.</param>
		public static bool IsIterator(this IMethodSymbol method, bool allowEnumerator = true)
		{
			if (allowEnumerator)
			{
				return IsIterator(method, nameof(IEnumerable), nameof(IEnumerator), false);
			}

			return IsIterator(method, nameof(IEnumerable), null, false);
		}

		/// <summary>
		/// Determines whether the <paramref name="type"/> is a predefined keyword type (e.g. <see cref="string"/>, <see cref="int"/>, <see langword="dynamic"/>).
		/// </summary>
		/// <param name="type">Type to check.</param>
		public static bool IsKeyword(this ITypeSymbol type)
		{
			if (type is IDynamicTypeSymbol)
			{
				return true;
			}

			return type.SpecialType.IsKeyword();
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> is declared using the <see langword="new"/> keyword.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to check whether is declared using the <see langword="new"/> keyword.</param>
		public static bool IsNew(this ISymbol symbol)
		{
			if (symbol.IsTopLevel())
			{
				return false;
			}

#if ENABLE_REFLECTION
			if (CheckReflectionBool(symbol, "IsNew"))
			{
				return true;
			}
#endif

			return symbol.DeclaringSyntaxReferences
				.Select(r => r.GetSyntax())
				.OfType<MemberDeclarationSyntax>()
				.Any(m => m.Modifiers.Any(mod => mod.IsKind(SyntaxKind.NewKeyword)));
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is nullable.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check whether is nullable.</param>
		public static bool IsNullable(this INamedTypeSymbol type)
		{
			if (type.NullableAnnotation == NullableAnnotation.Annotated)
			{
				return true;
			}

			return
				type.IsValueType &&
				type.ConstructedFrom is not null &&
				type.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T &&
				type.ConstructedFrom.TypeArguments.Length > 0;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is a nullable reference type.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check whether is a nullable reference type.</param>
		public static bool IsNullableReferenceType(this INamedTypeSymbol type)
		{
			return type.NullableAnnotation == NullableAnnotation.Annotated && type.IsReferenceType;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is a nullable value type.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check whether is a nullable value type.</param>
		public static bool IsNullableValueType(this INamedTypeSymbol type)
		{
			if (!type.IsValueType)
			{
				return false;
			}

			if (type.NullableAnnotation == NullableAnnotation.Annotated)
			{
				return true;
			}

			return
				type.ConstructedFrom is not null &&
				type.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T &&
				type.ConstructedFrom.TypeArguments.Length > 0;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> is obsolete.
		/// </summary>
		/// <param name="symbol">Determines whether the specified <paramref name="symbol"/> is obsolete.</param>
		/// <param name="checkParent">Determines whether to also check all the containing types of the <paramref name="symbol"/>.</param>
		public static bool IsObsolete(this ISymbol symbol, bool checkParent = true)
		{
			bool isObsolete = HasObsoleteAttribute(symbol);

			if (!checkParent)
			{
				return isObsolete;
			}

			if (isObsolete)
			{
				return true;
			}

			return symbol.GetContainingTypes().Any(HasObsoleteAttribute);

			static bool HasObsoleteAttribute(ISymbol symbol)
			{
				return symbol.GetAttributes().Any(attr => attr.AttributeClass is
				{
					MetadataName: nameof(ObsoleteAttribute),
					ContainingNamespace.Name: nameof(System)
				});
			}
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
		/// Determines whether the specified <paramref name="method"/> is parameterless.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to check if is parameterless.</param>
		public static bool IsParameterless(this IMethodSymbol method)
		{
			return method.Parameters.Length == 0 && !method.IsVararg;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is parameterless (applies only to delegates).
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check if is parameterless.</param>
		public static bool IsParameterless(this INamedTypeSymbol type)
		{
			return type.DelegateInvokeMethod is not null && type.DelegateInvokeMethod.IsParameterless();
		}

		/// <summary>
		/// Determines whether the specified <paramref name="method"/> is a parameterless constructor.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to check whether is a parameterless constructor.</param>
		public static bool IsParameterlessConstructor(this IMethodSymbol method)
		{
			return method.MethodKind == MethodKind.Constructor && method.IsParameterless();
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> is <see langword="partial"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to check if is <see langword="partial"/>.</param>
		public static bool IsPartial(this ISymbol symbol)
		{
			return symbol switch
			{
				INamedTypeSymbol type => type.IsPartial(),
				IMethodSymbol method => method.IsPartial(true),
				_ => false
			};
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

#if ENABLE_REFLECTION
			if (CheckReflectionBool(type, "IsPartial"))
			{
				return true;
			}
#endif

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
		/// <param name="retrieveNode">Determines whether to call the <see cref="SyntaxReference.GetSyntax(CancellationToken)"/> method when partiality of the <paramref name="method"/> cannot be determined by other means.</param>
		public static bool IsPartial(this IMethodSymbol method, bool retrieveNode = false)
		{
#if ENABLE_REFLECTION
			if (CheckReflectionBool(method, "IsPartial"))
			{
				return true;
			}
#endif
			if (retrieveNode)
			{
				return IsPartial_Internal(method, () => method.DeclaringSyntaxReferences[0].GetSyntax() as MethodDeclarationSyntax);
			}

			return IsPartial_Internal(method, default);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="method"/> is <see langword="partial"/>.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to check if is <see langword="partial"/>.</param>
		/// <param name="declaration">Main declaration of this <paramref name="method"/>.</param>
		public static bool IsPartial(this IMethodSymbol method, MethodDeclarationSyntax declaration)
		{
			return IsPartial_Internal(method, () => declaration);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> is <see langword="partial"/> and all its containing types are also <see langword="partial"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to check if is <see langword="partial"/>.</param>
		public static bool IsPartialContext(this ISymbol symbol)
		{
			return symbol switch
			{
				INamedTypeSymbol type => type.IsPartialContext(),
				IMethodSymbol method => method.IsPartialContext(),
				_ => false
			};
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
		/// Determines whether the specified <paramref name="method"/> is <see langword="partial"/> and all its containing types are also <see langword="partial"/>..
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to check if is <see langword="partial"/>.</param>
		/// <param name="retrieveNode">Determines whether to call the <see cref="SyntaxReference.GetSyntax(CancellationToken)"/> method when partiality of the <paramref name="method"/> cannot be determined by other means.</param>
		public static bool IsPartialContext(this IMethodSymbol method, bool retrieveNode = false)
		{
			return method.IsPartial(retrieveNode) && method.GetContainingTypes().All(t => t.IsPartial());
		}

		/// <summary>
		/// Determines whether the specified <paramref name="method"/> is <see langword="partial"/> and all its containing types are also <see langword="partial"/>..
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to check if is <see langword="partial"/>.</param>
		/// <param name="declaration">Main declaration of this <paramref name="method"/>.</param>
		public static bool IsPartialContext(this IMethodSymbol method, MethodDeclarationSyntax declaration)
		{
			return method.IsPartial(declaration) && method.GetContainingTypes().All(t => t.IsPartial());
		}

		/// <summary>
		/// Determines whether the specified <paramref name="ctor"/> is a record primary constructor.
		/// </summary>
		/// <param name="ctor"><see cref="IMethodSymbol"/> to check whether is a primary constructor.</param>
		public static bool IsPrimaryConstructor(this IMethodSymbol ctor)
		{
			if (ctor.MethodKind != MethodKind.Constructor || ctor.DeclaredAccessibility != Accessibility.Public || ctor.IsImplicitlyDeclared)
			{
				return false;
			}

#if ENABLE_REFLECTION
			if (ctor.GetType().Name == "SynthesizedRecordConstructor")
			{
				return true;
			}
#endif
			return ctor.DeclaringSyntaxReferences.Any(r => r.GetSyntax() is RecordDeclarationSyntax);
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
		/// Determines whether the specified <paramref name="property"/> is declared using the <see langword="readonly"/> keyword.
		/// </summary>
		/// <param name="property"><see cref="IPropertySymbol"/> to check whether is declared using the <see langword="readonly"/> keyword.</param>
		public static bool IsReadOnlyContext(this IPropertySymbol property)
		{
			if (property.GetMethod?.IsReadOnly == false)
			{
				return false;
			}

			if (property.SetMethod is null)
			{
				return true;
			}

			return property.SetMethod.IsReadOnly;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="event"/> is declared using the <see langword="readonly"/> keyword.
		/// </summary>
		/// <param name="event"><see cref="IEventSymbol"/> to check whether is declared using the <see langword="readonly"/> keyword.</param>
		public static bool IsReadOnlyContext(this IEventSymbol @event)
		{
			if (@event.AddMethod?.IsReadOnly == false)
			{
				return false;
			}

			if (@event.RemoveMethod is null)
			{
				return true;
			}

			return @event.RemoveMethod.IsReadOnly;
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
		/// Determines whether the specified <paramref name="symbol"/> is a kind of <paramref name="specialMember"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to check if is a special member.</param>
		/// <param name="specialMember">King of special member.</param>
		public static bool IsSpecialMember(this ISymbol symbol, SpecialMember specialMember)
		{
			switch (specialMember)
			{
				case SpecialMember.Current:
				{
					return
						symbol is IPropertySymbol property &&
						property.DeclaredAccessibility == Accessibility.Public &&
						property.Name == "Current" &&
						property.GetMethod is not null;
				}

				case SpecialMember.MoveNext:
				{
					return
						symbol is IMethodSymbol method &&
						method.DeclaredAccessibility == Accessibility.Public &&
						method.Arity == 0 &&
						method.Name == "MoveNext" &&
						method.ReturnType.SpecialType == SpecialType.System_Boolean &&
						method.IsParameterless();
				}

				case SpecialMember.MoveNextAsync:
				{
					return
						symbol is IMethodSymbol method &&
						method.DeclaredAccessibility == Accessibility.Public &&
						method.Arity == 0 &&
						method.Name == "MoveNextAsync" &&
						method.IsParameterless() &&
						method.ReturnType is INamedTypeSymbol returnType &&
						returnType.IsAwaitable(SpecialType.System_Boolean);
				}

				case SpecialMember.GetEnumerator:
				{
					return
						symbol is IMethodSymbol method &&
						method.DeclaredAccessibility == Accessibility.Public &&
						method.Arity == 0 &&
						!method.ReturnsVoid &&
						method.Name == "GetEnumerator" &&
						method.IsParameterless() &&
						method.ReturnType is INamedTypeSymbol returnType &&
						returnType.IsEnumerator(true);
				}

				case SpecialMember.GetAsyncEnumerator:
				{
					return
						symbol is IMethodSymbol method &&
						method.DeclaredAccessibility == Accessibility.Public &&
						method.Arity == 0 &&
						!method.ReturnsVoid &&
						method.Name == "GetAsyncEnumerator" &&
						method.IsParameterless() &&
						method.ReturnType is INamedTypeSymbol returnType &&
						returnType.IsAsyncEnumerator();
				}

				case SpecialMember.GetAwaiter:
				{
					return
						symbol is IMethodSymbol method &&
						IsGetAwaiter(method, default);
				}

				case SpecialMember.GetResult:
				{
					return
						symbol is IMethodSymbol method &&
						method.DeclaredAccessibility == Accessibility.Public &&
						method.Arity == 0 &&
						method.Name == "GetResult" &&
						method.IsParameterless();
				}

				case SpecialMember.IsCompleted:
				{
					return
						symbol is IPropertySymbol property &&
						property.DeclaredAccessibility == Accessibility.Public &&
						property.Name == "IsCompleted" &&
						property.Type.SpecialType == SpecialType.System_Boolean;
				}

				case SpecialMember.Add:
				{
					return
						symbol is IMethodSymbol method &&
						method.Name == "Add" &&
						!method.IsParameterless();
				}

				case SpecialMember.Deconstruct:
				{
					return
						symbol is IMethodSymbol method &&
						method.DeclaredAccessibility == Accessibility.Public &&
						method.Arity == 0 &&
						method.Name == "Deconstruct" &&
						method.ReturnsVoid &&
						!method.IsParameterless() &&
						method.Parameters.All(p => p.RefKind == RefKind.Out);
				}

				case SpecialMember.Length:
				{
					return
						symbol is IPropertySymbol property &&
						property.DeclaredAccessibility == Accessibility.Public &&
						property.Type.SpecialType == SpecialType.System_Int32 &&
						property.GetMethod is not null &&
						property.Name == "Length";
				}

				case SpecialMember.Count:
				{
					return
						symbol is IPropertySymbol property &&
						property.DeclaredAccessibility == Accessibility.Public &&
						property.Type.SpecialType == SpecialType.System_Int32 &&
						property.GetMethod is not null &&
						property.Name == "Count";
				}

				case SpecialMember.Slice:
				{
					return
						symbol is IMethodSymbol method &&
						method.DeclaredAccessibility == Accessibility.Public &&
						method.Parameters.Length == 2 &&
						method.Name == "Slice" &&
						method.Parameters[0].Type.SpecialType == SpecialType.System_Int32 &&
						method.Parameters[1].Type.SpecialType == SpecialType.System_Int32;
				}

				case SpecialMember.PrintMembers:
				{
					if (symbol is not IMethodSymbol method || !method.ContainingType.IsRecord || method.ReturnType.SpecialType != SpecialType.System_Boolean || method.Parameters.Length != 1)
					{
						return false;
					}

					if (!method.ContainingType.IsSealedKind() && !(method.IsVirtual || method.IsAbstract))
					{
						return false;
					}

					IParameterSymbol parameter = method.Parameters[0];

					return
						parameter.Type.MetadataName == "StringBuilder" &&
						parameter.Type.IsWithinNamespace("System.Text");
				}

				default:
					return false;
			}
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> is a top-level symbol (is not contained within a type).
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to check whether is a top-level type.</param>
		public static bool IsTopLevel(this ISymbol symbol)
		{
			return symbol.ContainingSymbol is INamespaceSymbol;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="namespace"/> is a top-level namespace (is not contained within other namespace).
		/// </summary>
		/// <param name="namespace"><see cref="INamespaceSymbol"/> to check whether is a top-level type.</param>
		public static bool IsTopLevel(this INamespaceSymbol @namespace)
		{
			return @namespace.ContainingSymbol is not INamespaceSymbol parent || parent.IsGlobalNamespace;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> is <see langword="unsafe"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to check whether is <see langword="unsafe"/>.</param>
		public static bool IsUnsafe(this ISymbol symbol)
		{
#if ENABLE_REFLECTION
			if (CheckReflectionBool(symbol, "IsUnsafe"))
			{
				return true;
			}
#endif

			ImmutableArray<SyntaxReference> references = symbol.DeclaringSyntaxReferences;

			if (references.Length == 0)
			{
				return false;
			}

			Func<SyntaxReference, MemberDeclarationSyntax?> function;

			if (symbol is IFieldSymbol or IEventSymbol)
			{
				function = r => r.GetSyntax().Parent?.Parent as MemberDeclarationSyntax;
			}
			else
			{
				function = r => r.GetSyntax() as MemberDeclarationSyntax;
			}

			return references
				.Select(function)
				.Any(m => m is not null && m.Modifiers.Any(mod => mod.IsKind(SyntaxKind.UnsafeKeyword)));
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
		/// Determines whether the specified <paramref name="symbol"/> is contained within a <see langword="namespace"/> with the specified name.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to check if is contained within the specified <paramref name="namespace"/>.</param>
		/// <param name="namespace">Name of target namespace.</param>
		/// <param name="split">Determines whether to split the namespace name by the dot '.' character.</param>
		/// <param name="topLevel">Determines whether the matched namespace must be top-level, i.e. not have a parent namespace (other than the <see langword="global"/> namespace).</param>
		/// <param name="lookupOuter">Determines whether to lookup all parent namespaces of the <paramref name="symbol"/>.</param>
		public static bool IsWithinNamespace(this ISymbol symbol, string @namespace, bool split = true, bool topLevel = true, bool lookupOuter = false)
		{
			if (symbol.ContainingNamespace is null || symbol.ContainingNamespace.IsGlobalNamespace)
			{
				return false;
			}

			if (split)
			{
				string[] parts = @namespace.Split('.');

				if (parts.Length > 1)
				{
					Array.Reverse(parts);
					return IsWithinNamespace_Internal(symbol, parts, topLevel, lookupOuter);
				}
			}

			if (symbol.ContainingNamespace.Name == @namespace)
			{
				if (topLevel)
				{
					INamespaceSymbol? root = symbol.ContainingNamespace.ContainingNamespace;
					return root is null || root.IsGlobalNamespace;
				}

				return true;
			}

			if (topLevel || !lookupOuter)
			{
				return false;
			}

			return symbol.ContainingNamespace.GetContainingNamespaces(order: ReturnOrder.Parent).Any(n => n.Name == @namespace);
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
		/// Determines whether the specified <paramref name="symbol"/> supports more than one attribute target kind.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to determine whether supports more than one attribute target kind.</param>
		public static bool SupportsAlternativeAttributeTargets(this ISymbol symbol)
		{
			return symbol switch
			{
				IMethodSymbol method => method.SupportsAlternativeAttributeTargets(),
				IPropertySymbol property => property.SupportsAlternativeAttributeTargets(),
				IEventSymbol @event => @event.SupportsAlternativeAttributeTargets(),
				INamedTypeSymbol type => type.SupportsAlternativeAttributeTargets(),
				_ => false,
			};
		}

		/// <summary>
		/// Determines whether the specified <paramref name="method"/> supports more than one attribute target kind.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to determine whether supports more than one attribute target kind.</param>
		public static bool SupportsAlternativeAttributeTargets(this IMethodSymbol method)
		{
			return
				method.SupportsAttributeTargets() &&
				!method.IsConstructor() &&
				method.MethodKind != MethodKind.Destructor;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="property"/> supports more than one attribute target kind.
		/// </summary>
		/// <param name="property"><see cref="IPropertySymbol"/> to determine whether supports more than one attribute target kind.</param>
		public static bool SupportsAlternativeAttributeTargets(this IPropertySymbol property)
		{
			return property.IsAutoProperty();
		}

		/// <summary>
		/// Determines whether the specified <paramref name="event"/> supports more than one attribute target kind.
		/// </summary>
		/// <param name="event"><see cref="IEventSymbol"/> to determine whether supports more than one attribute target kind.</param>
		public static bool SupportsAlternativeAttributeTargets(this IEventSymbol @event)
		{
			return @event.IsFieldEvent();
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> supports more than one attribute target kind.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to determine whether supports more than one attribute target kind.</param>
		public static bool SupportsAlternativeAttributeTargets(this INamedTypeSymbol type)
		{
			return type.TypeKind == TypeKind.Delegate;
		}

		/// <summary>
		/// Determines whether any attribute target can be applied to the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to determine whether any attribute target can be applied to.</param>
		public static bool SupportsAttributeTargets(this ISymbol symbol)
		{
			if (symbol is IMethodSymbol method)
			{
				return method.SupportsAttributeTargets();
			}

			return symbol is
				INamedTypeSymbol or
				ITypeParameterSymbol or
				IPropertySymbol or
				IEventSymbol or
				IFieldSymbol or
				IAssemblySymbol or
				IModuleSymbol or
				IParameterSymbol;
		}

		/// <summary>
		/// Determines whether any attribute target can be applied to the specified <paramref name="method"/>.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to determine whether any attribute target can be applied to.</param>
		public static bool SupportsAttributeTargets(this IMethodSymbol method)
		{
			return method.MethodKind is
				not MethodKind.BuiltinOperator and
				not MethodKind.FunctionPointerSignature and
				not MethodKind.EventRaise;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> supports collection initializers.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check whether supports collection initializers.</param>
		public static bool SupportsCollectionInitializer(this INamedTypeSymbol type)
		{
			if (!type.IsEnumerable(true))
			{
				return false;
			}

			return type
				.GetAllMembers()
				.OfType<IMethodSymbol>()
				.Any(IsCollectionInitializer);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> supports collection initializers.
		/// </summary>
		/// <param name="type"><see cref="ITypeSymbol"/> to check whether supports collection initializers.</param>
		public static bool SupportsCollectionInitializer(this ITypeSymbol type)
		{
			return type switch
			{
				IArrayTypeSymbol => true,
				INamedTypeSymbol named => named.SupportsCollectionInitializer(),
				_ => false
			};
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> can have an explicit base type.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check whether can have an explicit base type.</param>
		public static bool SupportsExplicitBaseType(this INamedTypeSymbol type)
		{
			return type.TypeKind == TypeKind.Class && !type.IsStatic && !type.IsSealed;
		}

		internal static bool CanHideSymbol_Internal(IMethodSymbol method, IMethodSymbol other)
		{
			if (other.MethodKind != MethodKind.Ordinary)
			{
				return false;
			}

			if (method.Arity != other.Arity || method.Name != other.Name)
			{
				return false;
			}

			if (method.IsVararg)
			{
				if (!other.IsVararg)
				{
					return false;
				}
			}
			else if (other.IsVararg)
			{
				return false;
			}

			return ParametersAreEquivalent(method.Parameters, other.Parameters);
		}

		internal static bool IsGetAwaiterRaw(this IMethodSymbol method, [NotNullWhen(true)] out INamedTypeSymbol? returnType)
		{
			bool value =
				method.Arity == 0 &&
				method.Name == "GetAwaiter" &&
				!method.ReturnsVoid &&
				method.ReturnType is INamedTypeSymbol &&
				method.IsParameterless();

			returnType = value ? (method.ReturnType as INamedTypeSymbol)! : default;

			return value;
		}

		internal static bool IsINotifyCompletion(this INamedTypeSymbol type)
		{
			return type.AllInterfaces.Any(intf =>
				IsSpecialInterface(intf, "INotifyCompletion") &&
				intf.Arity == 0 &&
				intf.IsWithinNamespace("System.Runtime.CompilerServices"));
		}

		internal static bool ParametersAreEquivalent(ImmutableArray<IParameterSymbol> firstParameters, ImmutableArray<IParameterSymbol> secondParameters)
		{
			if (firstParameters.Length != secondParameters.Length)
			{
				return false;
			}

			for (int i = 0; i < firstParameters.Length; i++)
			{
				IParameterSymbol first = firstParameters[i];
				IParameterSymbol second = secondParameters[i];

				if (first.RefKind.IsValidForOverload(second.RefKind))
				{
					return false;
				}

				if (!SymbolEqualityComparer.Default.Equals(first.Type, second.Type))
				{
					return false;
				}
			}

			return true;
		}

		private static bool CanHideSymbol_Internal(INamedTypeSymbol type, IMethodSymbol method)
		{
			return type.Arity == method.Arity && type.Name == method.Name;
		}

		private static bool CanHideSymbol_Internal(INamedTypeSymbol type, IPropertySymbol property)
		{
			if (property.IsIndexer)
			{
				return false;
			}

			return type.Arity == 0 && type.Name == property.Name;
		}

		private static bool CanHideSymbol_Internal(IMethodSymbol method, IPropertySymbol property)
		{
			if (property.IsIndexer)
			{
				return false;
			}

			return method.Arity == 0 && method.Name == property.Name;
		}

#if ENABLE_REFLECTION

		private static bool CheckReflectionBool(ISymbol symbol, string propertyName)
		{
			if (symbol.GetType().GetProperty(propertyName) is PropertyInfo property && property.GetValue(symbol) is bool value)
			{
				return value;
			}

			return false;
		}

#endif

		private static bool IsDurianGeneratedAttribute(AttributeData attr)
		{
			return
				attr.AttributeClass is INamedTypeSymbol type &&
				type.MetadataName == nameof(DurianGeneratedAttribute) &&
				type.IsWithinNamespace(DurianStrings.GeneratorNamespace);
		}

		private static bool IsGetAwaiter(IMethodSymbol method, ITypeSymbol? resultType)
		{
			if (!method.IsGetAwaiterRaw(out INamedTypeSymbol? returnType))
			{
				return false;
			}

			if (resultType is null)
			{
				return returnType.IsAwaiter();
			}

			return returnType.IsAwaiter(resultType);
		}

		private static bool IsIterator(IMethodSymbol method, string iteratorType1, string? iteratorType2, bool requireTypeParameter)
		{
			if (method.ReturnsVoid || method.ReturnsByRef || method.ReturnsByRefReadonly || method.ReturnType is not INamedTypeSymbol)
			{
				return false;
			}

			if (method.MetadataName != iteratorType1)
			{
				if (iteratorType1 is null || method.MetadataName != iteratorType2)
				{
					return false;
				}
			}

			if (!method.IsWithinNamespace(nameof(System), false))
			{
				return false;
			}

			if (method.Arity != 1)
			{
				if (requireTypeParameter || method.Arity != 0)
				{
					return false;
				}
			}

#if ENABLE_REFLECTION
			if (CheckReflectionBool(method, "IsIterator"))
			{
				return true;
			}
#endif

			if (method.GetSyntax() is not CSharpSyntaxNode node)
			{
				return false;
			}

			return node
				.DescendantNodes(node => node.Kind() is
					not SyntaxKind.LocalFunctionStatement and
					not SyntaxKind.AnonymousMethodExpression and
					not SyntaxKind.SimpleLambdaExpression and
					not SyntaxKind.ParenthesizedLambdaExpression &&
					node is not ExpressionSyntax
				)
				.Any(n => n is YieldStatementSyntax);
		}

		private static bool IsPartial_Internal(IMethodSymbol method, Func<MethodDeclarationSyntax?>? declarationRetriever)
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
				method.PartialDefinitionPart is not null
			)
			{
				return true;
			}

			if (method.DeclaringSyntaxReferences.Length == 0)
			{
				return false;
			}

			if (declarationRetriever is not null && declarationRetriever() is MethodDeclarationSyntax declaration)
			{
				return declaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
			}

			return false;
		}

		private static bool IsSpecialInterface(ITypeSymbol type, string interfaceName)
		{
			return
				type is INamedTypeSymbol &&
				type.TypeKind == TypeKind.Interface &&
				type.MetadataName == interfaceName;
		}

		private static bool IsWithinNamespace_Internal(ISymbol symbol, string[] @namespace, bool topLevel, bool lookupOuter)
		{
			int current = 0;
			IEnumerator<INamespaceSymbol> all = symbol.GetContainingNamespaces(order: ReturnOrder.Parent).GetEnumerator();

			while (current < @namespace.Length)
			{
				if (!all.MoveNext())
				{
					return false;
				}

				if (all.Current.Name == @namespace[current])
				{
					if (all.Current.IsGlobalNamespace)
					{
						return false;
					}

					current++;
				}
				else
				{
					if (!lookupOuter || topLevel)
					{
						return false;
					}

					current = 0;
				}
			}

			if (topLevel)
			{
				return !all.MoveNext();
			}

			return true;
		}
	}
}
