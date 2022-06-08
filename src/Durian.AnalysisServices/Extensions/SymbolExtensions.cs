// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Threading;
using Durian.Analysis.CodeGeneration;
using Durian.Analysis.Data;
using Durian.Analysis.SymbolContainers;
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
						SyntaxKind kind = refKind.GetSyntaxKind();
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

		/// <summary>
		/// Returns the kind of accessor the specified <paramref name="method"/> represents.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to get the accessor kind of.</param>
		public static AccessorKind GetAccessorKind(this IMethodSymbol method)
		{
			return method.MethodKind.GetAccessorKind();
		}

		/// <inheritdoc cref="GetAllMembers(INamedTypeSymbol, string, ReturnOrder)"/>
		public static IReturnOrderEnumerable<ISymbol> GetAllMembers(this INamedTypeSymbol type, ReturnOrder order = ReturnOrder.Parent)
		{
			return type.GetBaseTypes_Internal(true).SelectMany(t => t.GetMembers()).OrderBy(order);
		}

		/// <summary>
		/// Returns all members of the specified <paramref name="type"/> including the members that are declared in base types of this <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="ITypeSymbol"/> to get the members of.</param>
		/// <param name="name">Name of the members to find.</param>
		/// <param name="order">Specifies ordering of the returned members.</param>
		public static IReturnOrderEnumerable<ISymbol> GetAllMembers(this INamedTypeSymbol type, string name, ReturnOrder order = ReturnOrder.Parent)
		{
			return type.GetBaseTypes_Internal(true).SelectMany(t => t.GetMembers(name)).OrderBy(order);
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
		/// Returns the <see cref="ISymbol"/> that is accessed by using the specified <paramref name="target"/> in the context of the given <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the attribute target symbol of.</param>
		/// <param name="target">Kind of attribute target.</param>
		/// <remarks><b>Note:</b> In actual code the <see langword="method"/> target applied to an event refers to both the <see langword="add"/> and <see langword="remove"/> accessors. In such case, this method returns the event itself.</remarks>
		public static ISymbol? GetAttributeTarget(this ISymbol symbol, AttributeTarget target)
		{
			switch (target)
			{
				case AttributeTarget.Assembly:
					return symbol as IAssemblySymbol;

				case AttributeTarget.Return:
				{
					if (symbol is IMethodSymbol method)
					{
						if (!method.IsConstructor() && method.MethodKind != MethodKind.Destructor)
						{
							return method.ReturnType;
						}
					}
					else if (symbol is INamedTypeSymbol type)
					{
						if (type.DelegateInvokeMethod is not null)
						{
							return type.DelegateInvokeMethod.ReturnType;
						}
					}

					return default;
				}

				case AttributeTarget.Field:
				{
					if (symbol is IPropertySymbol property)
					{
						if (property.GetBackingField() is IFieldSymbol backingField)
						{
							return backingField;
						}
					}
					else if (symbol is IEventSymbol @event)
					{
						if (@event.GetBackingField() is IFieldSymbol backingField)
						{
							return backingField;
						}
					}

					return symbol as IFieldSymbol;
				}

				case AttributeTarget.Method:
					return symbol is IMethodSymbol or IEventSymbol ? symbol : default;

				case AttributeTarget.Property:
					return symbol as IPropertySymbol;

				case AttributeTarget.Event:
					return symbol as IEventSymbol;

				case AttributeTarget.Type:
					return symbol as INamedTypeSymbol;

				case AttributeTarget.Param:
				{
					if (symbol is IMethodSymbol method && method.MethodKind is MethodKind.PropertySet or MethodKind.EventAdd or MethodKind.EventRemove)
					{
						return method.Parameters[0];
					}

					return symbol as IParameterSymbol;
				}

				case AttributeTarget.Module:
					return symbol as IModuleSymbol;

				case AttributeTarget.TypeVar:
					return symbol as ITypeParameterSymbol;

				default:
					return default;
			}
		}

		/// <summary>
		/// Returns the <see cref="ISymbol"/> that is accessed by using an attribute target if the specified <paramref name="kind"/> in the context of the given <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the attribute target symbol of.</param>
		/// <param name="kind">Kind of attribute target.</param>
		/// <remarks><b>Note:</b> In actual code the <see langword="method"/> target applied to an event refers to both the <see langword="add"/> and <see langword="remove"/> accessors. In such case, this method returns the event itself.</remarks>
		public static ISymbol? GetAttributeTarget(this ISymbol symbol, AttributeTargetKind kind)
		{
			return symbol switch
			{
				IAssemblySymbol => ThisOrDefault(),
				IFieldSymbol => ThisOrDefault(),
				IMethodSymbol method => kind switch
				{
					AttributeTargetKind.This => method.SupportsAttributeTargets() ? method : default,
					AttributeTargetKind.Value => method.SupportsAlternativeAttributeTargets() ? method.ReturnType : default,
					AttributeTargetKind.Handler when method.MethodKind == MethodKind.PropertySet || method.IsEventAccessor() => method.Parameters[0],
					_ => default
				},
				IPropertySymbol property => kind switch
				{
					AttributeTargetKind.This => property,
					AttributeTargetKind.Value => property.GetBackingField(),
					_ => default
				},
				IEventSymbol @event => kind switch
				{
					AttributeTargetKind.This => @event,
					AttributeTargetKind.Value => @event.GetBackingField(),
					AttributeTargetKind.Handler when @event.IsFieldEvent() => @event,
					_ => default
				},
				INamedTypeSymbol type => kind switch
				{
					AttributeTargetKind.This => type,
					AttributeTargetKind.Value when type.DelegateInvokeMethod is IMethodSymbol method => method.ReturnType,
					_ => default
				},
				IParameterSymbol => ThisOrDefault(),
				ITypeParameterSymbol => ThisOrDefault(),
				IModuleSymbol => ThisOrDefault(),
				_ => default
			};

			ISymbol? ThisOrDefault()
			{
				return kind == AttributeTargetKind.This ? symbol : default;
			}
		}

		/// <summary>
		/// Determines the kind of the specified attribute <paramref name="target"/> used in context of the given <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the kind of attribute target of.</param>
		/// <param name="target">Attribute target to get the kind of.</param>
		public static AttributeTargetKind GetAttributeTargetKind(this ISymbol symbol, AttributeTarget target)
		{
			return target switch
			{
				AttributeTarget.Assembly => symbol is IAssemblySymbol ? AttributeTargetKind.This : default,
				AttributeTarget.Field => symbol switch
				{
					IFieldSymbol => AttributeTargetKind.This,
					IPropertySymbol property => property.IsAutoProperty() ? AttributeTargetKind.Value : default,
					IEventSymbol @event when @event.IsFieldEvent() => AttributeTargetKind.Value,
					_ => default
				},
				AttributeTarget.Return => symbol switch
				{
					IMethodSymbol method => method.SupportsAlternativeAttributeTargets() ? AttributeTargetKind.Value : default,
					INamedTypeSymbol type when type.SupportsAlternativeAttributeTargets() => AttributeTargetKind.Value,
					_ => default
				},
				AttributeTarget.Method => symbol switch
				{
					IMethodSymbol => AttributeTargetKind.This,
					IEventSymbol @event when @event.IsFieldEvent() => AttributeTargetKind.Handler,
					_ => default
				},
				AttributeTarget.Property => symbol is IPropertySymbol ? AttributeTargetKind.This : default,
				AttributeTarget.Event => symbol is IEventSymbol ? AttributeTargetKind.This : default,
				AttributeTarget.Type => symbol is INamedTypeSymbol ? AttributeTargetKind.This : default,
				AttributeTarget.Param => symbol switch
				{
					IParameterSymbol => AttributeTargetKind.This,
					IMethodSymbol method when method.MethodKind == MethodKind.PropertySet || method.IsEventAccessor() => AttributeTargetKind.Handler,
					_ => default
				},
				AttributeTarget.TypeVar => symbol is ITypeParameterSymbol ? AttributeTargetKind.This : default,
				AttributeTarget.Module => symbol is IModuleSymbol ? AttributeTargetKind.This : default,
				_ => default
			};
		}

		/// <summary>
		/// Returns an attribute target keyword used to refer to the specified <paramref name="symbol"/> inside an attribute list.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the associated attribute target keyword of.</param>
		/// <param name="kind">Determines which keyword to return when there is more than one option (e.g '<see langword="method"/>' and '<see langword="return"/>' for methods).</param>
		public static AttributeTarget GetAttributeTargetKind(this ISymbol symbol, AttributeTargetKind kind = AttributeTargetKind.This)
		{
			return symbol switch
			{
				IAssemblySymbol => ThisOrDefault(AttributeTarget.Assembly),
				IFieldSymbol => ThisOrDefault(AttributeTarget.Field),
				IMethodSymbol method => kind switch
				{
					AttributeTargetKind.This => method.SupportsAttributeTargets() ? AttributeTarget.Method : default,
					AttributeTargetKind.Value => method.SupportsAlternativeAttributeTargets() && method.MethodKind != MethodKind.PropertySet ? AttributeTarget.Return : default,
					AttributeTargetKind.Handler when method.MethodKind == MethodKind.PropertySet || method.IsEventAccessor() => AttributeTarget.Param,
					_ => default
				},
				IPropertySymbol property => kind switch
				{
					AttributeTargetKind.This => AttributeTarget.Property,
					AttributeTargetKind.Value when property.IsAutoProperty() => AttributeTarget.Field,
					_ => default
				},
				IEventSymbol @event => kind switch
				{
					AttributeTargetKind.This => AttributeTarget.Event,
					AttributeTargetKind.Value => @event.IsFieldEvent() ? AttributeTarget.Field : default,
					AttributeTargetKind.Handler when @event.IsFieldEvent() => AttributeTarget.Method,
					_ => default
				},
				INamedTypeSymbol type => kind switch
				{
					AttributeTargetKind.This => AttributeTarget.Type,
					AttributeTargetKind.Value when type.SupportsAlternativeAttributeTargets() => AttributeTarget.Return,
					_ => default
				},
				IParameterSymbol => ThisOrDefault(AttributeTarget.Param),
				ITypeParameterSymbol => ThisOrDefault(AttributeTarget.TypeVar),
				IModuleSymbol => ThisOrDefault(AttributeTarget.Module),
				_ => default,
			};

			AttributeTarget ThisOrDefault(AttributeTarget @this)
			{
				return kind == AttributeTargetKind.This ? @this : default;
			}
		}

		/// <summary>
		/// Returns the <see cref="AutoPropertyKind"/> of the specified <paramref name="property"/>.
		/// </summary>
		/// <param name="property"><see cref="IPropertySymbol"/> to get the <see cref="AutoPropertyKind"/> of.</param>
		/// <param name="includeAbstract">Determines whether to return <see cref="AutoPropertyKind"/> even if the <paramref name="property"/> is <see langword="abstract"/> or <see langword="extern"/>.</param>
		public static AutoPropertyKind GetAutoPropertyKind(this IPropertySymbol property, bool includeAbstract = false)
		{
			if (includeAbstract)
			{
				if (property.GetMethod is null)
				{
					if (property.SetMethod is not null)
					{
						if (property.SetMethod.IsInitOnly)
						{
							return AutoPropertyKind.InitOnly;
						}

						return AutoPropertyKind.SetOnly;
					}

					return default;
				}
			}
			else if (property.IsAbstract || property.IsExtern || !property.IsAutoProperty() || property.GetMethod is null)
			{
				return default;
			}

			if (property.SetMethod is null)
			{
				return AutoPropertyKind.GetOnly;
			}

			if (property.SetMethod.IsInitOnly)
			{
				return AutoPropertyKind.GetInit;
			}

			return AutoPropertyKind.GetSet;
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
					if (method.IsGetAwaiterRaw(out INamedTypeSymbol? returnType) && HandleAwaiterType(type))
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
				else if (symbol is IMethodSymbol method && method.IsGetAwaiterRaw(out INamedTypeSymbol? awaiter))
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
			if (property.IsIndexer || property.IsAbstract || property.IsExtern)
			{
				return default;
			}

			return property.ContainingType?
				.GetMembers()
				.OfType<IFieldSymbol>()
				.FirstOrDefault(f => SymbolEqualityComparer.Default.Equals(f.AssociatedSymbol, property));
		}

		/// <summary>
		/// Returns the backing field of the specified <paramref name="event"/> or <see langword="null"/> if the <paramref name="event"/> is not a field-like event.
		/// </summary>
		/// <param name="event"><see cref="IEventSymbol"/> to get the backing field of.</param>
		public static IFieldSymbol? GetBackingField(this IEventSymbol @event)
		{
			return @event.ContainingType?
				.GetMembers()
				.OfType<IFieldSymbol>()
				.FirstOrDefault(f => SymbolEqualityComparer.Default.Equals(f.AssociatedSymbol, @event));
		}

		/// <summary>
		/// Returns all types the specified <paramref name="type"/> inherits from.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to get the base types of.</param>
		/// <param name="includeSelf">Determines whether to include the <paramref name="type"/> in the returned collection.</param>
		/// <param name="order">Specifies ordering of the returned members.</param>
		public static IReturnOrderEnumerable<INamedTypeSymbol> GetBaseTypes(this INamedTypeSymbol type, bool includeSelf = false, ReturnOrder order = ReturnOrder.Parent)
		{
			return GetBaseTypes_Internal(type, includeSelf).OrderBy(order);
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
		/// Returns value representing special kind of the specified <paramref name="method"/>.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to get the special constructor kind of.</param>
		public static SpecialConstructor GetConstructorKind(this IMethodSymbol method)
		{
			if (method.MethodKind == MethodKind.StaticConstructor)
			{
				return SpecialConstructor.Static;
			}

			if (method.IsVararg)
			{
				return default;
			}

			if (method.Parameters.Length == 0)
			{
				return method.IsImplicitlyDeclared ? SpecialConstructor.Default : SpecialConstructor.Parameterless;
			}

			if (method.Parameters.Length == 1 && method.Parameters[0].RefKind == RefKind.None && SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, method.ContainingType))
			{
				return SpecialConstructor.Copy;
			}

			return default;
		}

		/// <summary>
		/// Returns all <see cref="INamespaceSymbol"/>s that contain the target <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the parent namespaces of.</param>
		/// <param name="includeGlobal">Determines whether to return the global namespace as well.</param>
		/// <param name="order">Specifies ordering of the returned members.</param>
		public static IReturnOrderEnumerable<INamespaceSymbol> GetContainingNamespaces(this ISymbol symbol, bool includeGlobal = false, ReturnOrder order = ReturnOrder.Root)
		{
			IEnumerable<INamespaceSymbol> namespaces = Yield(symbol);

			if (!includeGlobal)
			{
				namespaces = namespaces.Where(n => !n.IsGlobalNamespace);
			}

			return namespaces.OrderBy(order);

			static IEnumerable<INamespaceSymbol> Yield(ISymbol symbol)
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
		public static IReturnOrderEnumerable<INamespaceOrTypeSymbol> GetContainingNamespacesAndTypes(this ISymbol symbol, bool includeGlobal = false, ReturnOrder order = ReturnOrder.Root)
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

			return first.Concat(second).OrderBy(order, false);
		}

		/// <summary>
		/// Returns all <see cref="INamedTypeSymbol"/>s that contain the target <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the parent types of.</param>
		/// <param name="includeSelf">Determines whether to include the <paramref name="symbol"/> in the returned collection if its a <see cref="INamedTypeSymbol"/>.</param>
		/// <param name="order">Specifies ordering of the returned members.</param>
		public static IReturnOrderEnumerable<INamedTypeSymbol> GetContainingTypes(this ISymbol symbol, bool includeSelf = false, ReturnOrder order = ReturnOrder.Root)
		{
			return Yield(symbol, includeSelf).OrderBy(order);

			static IEnumerable<INamedTypeSymbol> Yield(ISymbol symbol, bool includeSelf)
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
		/// <param name="includeSecondary">Determines whether to include the <see langword="class"/> keyword for record classes.</param>
		public static string? GetDeclaredKeyword(this INamedTypeSymbol type, bool includeSecondary = false)
		{
			return type.TypeKind switch
			{
				TypeKind.Class => type.IsRecord ? (includeSecondary ? "record class" : "record") : "class",
				TypeKind.Struct => type.IsRecord ? "record struct" : "struct",
				TypeKind.Interface => "interface",
				TypeKind.Enum => "enum",
				TypeKind.Delegate => "delegate",
				_ => default
			};
		}

		/// <summary>
		/// Returns the default <see cref="Accessibility"/> in the context of the specified <paramref name="symbol"/>, that is:
		/// <list type="bullet">
		/// <item>For top-level types: <see cref="Accessibility.Internal"/>.</item>
		/// <item>For interface members other than partial methods: <see cref="Accessibility.Public"/>.</item>
		/// <item>For property/event accessors: accessibility of the parent property/event.</item>
		/// <item>For all other members: <see cref="Accessibility.Private"/>.</item>
		/// </list>
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the default accessibility of.</param>
		/// <param name="includeAssociated">Determines whether accessibility of an associated member of the <paramref name="symbol"/> (e.g. parent property of an accessor) should be treated as default.</param>
		public static Accessibility GetDefaultAccessibility(this ISymbol symbol, bool includeAssociated = true)
		{
			if (symbol.IsTopLevel())
			{
				return Accessibility.Internal;
			}

			if (symbol.ContainingSymbol is INamedTypeSymbol type && type.TypeKind == TypeKind.Interface)
			{
				if (symbol is IMethodSymbol intfMethod && intfMethod.IsPartial(true))
				{
					return Accessibility.Private;
				}

				return Accessibility.Public;
			}

			if (includeAssociated && symbol is IMethodSymbol method && method.IsAccessor())
			{
				return method.AssociatedSymbol!.DeclaredAccessibility;
			}

			return Accessibility.Private;
		}

		/// <summary>
		/// Returns all interface members that have default implementations.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> of an interface to get the default implementations of.</param>
		/// <param name="baseInterfaces">Determines whether to also include default implementations from the implemented interfaces.</param>
		public static IEnumerable<ISymbol> GetDefaultImplementations(this INamedTypeSymbol type, bool baseInterfaces = false)
		{
			if (type.TypeKind != TypeKind.Interface)
			{
				return Array.Empty<ISymbol>();
			}

			IEnumerable<ISymbol> members = baseInterfaces
				? type.GetAllMembers()
				: type.GetMembers();

			return members.Where(m => m.IsDefaultImplementation());
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
		/// Returns the effective underlaying element type of the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="ITypeSymbol"/> to get the effective underlaying type of.</param>
		public static ITypeSymbol? GetEffectiveElementType(this ITypeSymbol type)
		{
			return type switch
			{
				IArrayTypeSymbol array => array.GetEffectiveElementType(),
				IPointerTypeSymbol pointer => pointer.GetEffectiveElementType(),
				_ => type.GetNullableUnderlayingType() is ITypeSymbol nullable ? nullable : default,
			};
		}

		/// <summary>
		/// Returns the effective underlaying element type of the <paramref name="array"/>.
		/// </summary>
		/// <param name="array"><see cref="IArrayTypeSymbol"/> to get the effective underlaying type of.</param>
		public static ITypeSymbol GetEffectiveElementType(this IArrayTypeSymbol array)
		{
			return array.GetEffectiveElementType(out _);
		}

		/// <summary>
		/// Returns the effective underlaying element type of the <paramref name="array"/>.
		/// </summary>
		/// <param name="array"><see cref="IArrayTypeSymbol"/> to get the effective underlaying type of.</param>
		/// <param name="nestingLevel">Number of inner <see cref="IArrayTypeSymbol"/>s (including the <paramref name="array"/> itself).</param>
		public static ITypeSymbol GetEffectiveElementType(this IArrayTypeSymbol array, out int nestingLevel)
		{
			int count = 0;
			ITypeSymbol? a = array;

			while (a is IArrayTypeSymbol t)
			{
				a = t.ElementType;
				count++;
			}

			nestingLevel = count;
			return a;
		}

		/// <summary>
		/// Returns the effective underlaying type the <paramref name="pointer"/>.
		/// </summary>
		/// <param name="pointer"><see cref="IPointerTypeSymbol"/> to get the effective underlaying type of.</param>
		public static ITypeSymbol GetEffectiveElementType(this IPointerTypeSymbol pointer)
		{
			return pointer.GetEffectiveElementType(out _);
		}

		/// <summary>
		/// Returns the effective underlaying type the <paramref name="pointer"/>.
		/// </summary>
		/// <param name="pointer"><see cref="IPointerTypeSymbol"/> to get the effective underlaying type of.</param>
		/// <param name="nestingLevel">Number of inner <see cref="IPointerTypeSymbol"/>s (including the <paramref name="pointer"/> itself).</param>
		public static ITypeSymbol GetEffectiveElementType(this IPointerTypeSymbol pointer, out int nestingLevel)
		{
			ITypeSymbol? p = pointer;
			int count = 0;

			while (p is IPointerTypeSymbol t)
			{
				p = t.PointedAtType;
				count++;
			}

			nestingLevel = count;
			return p;
		}

		/// <summary>
		/// Returns all underlaying element types of the specified <paramref name="array"/>.
		/// </summary>
		/// <param name="array"><see cref="IArrayTypeSymbol"/> to get the element types of.</param>
		/// <param name="order">Specifies ordering of the returned members.</param>
		public static IReturnOrderEnumerable<ITypeSymbol> GetElementTypes(this IArrayTypeSymbol array, ReturnOrder order = ReturnOrder.Root)
		{
			return Yield().OrderBy(order);

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
		/// Returns all underlaying element types of the specified <paramref name="pointer"/>.
		/// </summary>
		/// <param name="pointer"><see cref="IPointerTypeSymbol"/> to get the element types of.</param>
		/// <param name="order">Specifies ordering of the returned members.</param>
		public static IReturnOrderEnumerable<ITypeSymbol> GetElementTypes(this IPointerTypeSymbol pointer, ReturnOrder order = ReturnOrder.Root)
		{
			return Yield().OrderBy(order);

			IEnumerable<ITypeSymbol> Yield()
			{
				ITypeSymbol element = pointer.PointedAtType;

				yield return element;

				while (element is IPointerTypeSymbol pointer)
				{
					yield return pointer.PointedAtType;
					element = pointer.PointedAtType;
				}
			}
		}

		/// <summary>
		/// Returns fully qualified name of the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the fully qualified name of.</param>
		/// <param name="format">Determines format of the returned qualified name.</param>
		public static string GetFullyQualifiedName(this ISymbol symbol, QualifiedName format = default)
		{
			if (format == QualifiedName.Metadata)
			{
				return symbol.ToString();
			}

			CodeBuilder builder = new(false);
			builder.QualifiedName(symbol);
			string value = builder.ToString();

			if (format == QualifiedName.Xml)
			{
				return AnalysisUtilities.ToXmlCompatible(value);
			}

			return value;
		}

		/// <summary>
		/// Returns a <see cref="string"/> containing generic identifier of the specified <paramref name="symbol"/> or name of the <paramref name="symbol"/> if it is not an <see cref="IMethodSymbol"/> or <see cref="INamedTypeSymbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the generic name of.</param>
		/// <param name="substituted">Determines whether to use type arguments instead of type parameters.</param>
		public static string GetGenericName(this ISymbol symbol, bool substituted = false)
		{
			CodeBuilder builder = new(false);
			builder.Name(symbol, substituted ? SymbolName.Substituted : SymbolName.Generic);
			return builder.ToString();
		}

		/// <summary>
		/// Returns a <see cref="string"/> containing generic identifier of the specified <paramref name="method"/>.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to get the generic name of.</param>
		/// <param name="substituted">Determines whether to use type arguments instead of type parameters.</param>
		public static string GetGenericName(this IMethodSymbol method, bool substituted = false)
		{
			CodeBuilder builder = new(false);
			builder.Name(method, substituted ? SymbolName.Substituted : SymbolName.Generic);
			return builder.ToString();
		}

		/// <summary>
		/// Returns a <see cref="string"/> containing generic identifier of the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to get the generic name of.</param>
		/// <param name="substituted">Determines whether to use type arguments instead of type parameters.</param>
		public static string GetGenericName(this INamedTypeSymbol type, bool substituted = false)
		{
			CodeBuilder builder = new(false);
			builder.Name(type, substituted ? SymbolName.Substituted : SymbolName.Generic);
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
		public static string GetGenericName(this IEnumerable<ITypeSymbol> typeArguments)
		{
			CodeBuilder builder = new(false);
			builder.TypeArgumentList(typeArguments);
			return builder.ToString();
		}

		/// <summary>
		/// Returns a <see cref="string"/> containing generic identifier combined of the specified <paramref name="name"/> and the collection of <paramref name="typeArguments"/>.
		/// </summary>
		/// <param name="typeArguments">Type arguments.</param>
		/// <param name="name">Actual member identifier.</param>
		public static string GetGenericName(this IEnumerable<ITypeSymbol> typeArguments, string? name)
		{
			CodeBuilder builder = new(false);

			if (!string.IsNullOrWhiteSpace(name))
			{
				builder.Write(name!);
			}

			builder.TypeArgumentList(typeArguments);
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
					IFieldSymbol or IEventSymbol => true,
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

			CodeBuilder builder = new(false);

			foreach (INamedTypeSymbol type in symbol.GetContainingTypes())
			{
				builder.XmlName(type);
				builder.Write('.');
			}

			builder.XmlName(symbol);

			return AutoGenerated.GetInheritdoc(builder.ToString());
		}

		/// <summary>
		/// Returns a collection of all inner types of the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to get the inner types of.</param>
		/// <param name="includeSelf">Determines whether to include the <paramref name="type"/> in the returned collection.</param>
		public static IEnumerable<INamedTypeSymbol> GetInnerTypes(this INamedTypeSymbol type, bool includeSelf = false)
		{
			const int capacity = 32;

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

		/// <summary>
		/// Returns modifiers applied to the target <see cref="ISymbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the modifiers of.</param>
		public static string[] GetModifiers(this ISymbol symbol)
		{
			return symbol switch
			{
				INamedTypeSymbol type => type.GetModifiers(),
				IMethodSymbol method => method.GetModifiers(),
				IPropertySymbol property => property.GetModifiers(),
				IFieldSymbol field => field.GetModifiers(),
				IEventSymbol @event => @event.GetModifiers(),
				_ => Array.Empty<string>()
			};
		}

		/// <summary>
		/// Returns modifiers applied to the target <see cref="IFieldSymbol"/>.
		/// </summary>
		/// <param name="field"><see cref="IFieldSymbol"/> to get the modifiers of.</param>
		public static string[] GetModifiers(this IFieldSymbol field)
		{
			List<string> modifiers = new(4);

			AddAccessibilityModifiers(modifiers, field.DeclaredAccessibility);

			if (field.IsConst)
			{
				modifiers.Add("const");
			}
			else if (field.IsStatic)
			{
				modifiers.Add("static");
			}

			if (field.IsReadOnly)
			{
				modifiers.Add("readonly");
			}

			if (field.IsUnsafe())
			{
				modifiers.Add("unsafe");
			}

			if (field.IsVolatile)
			{
				modifiers.Add("volatile");
			}

			if (field.IsFixedSizeBuffer)
			{
				modifiers.Add("fixed");
			}

			return modifiers.ToArray();
		}

		/// <summary>
		/// Returns modifiers applied to the target <see cref="ILocalSymbol"/>.
		/// </summary>
		/// <param name="local"><see cref="ILocalSymbol"/> to get the modifiers of.</param>
		public static string[] GetModifiers(this ILocalSymbol local)
		{
			List<string> modifiers = new(2);

			if (local.IsConst)
			{
				modifiers.Add("const");
			}

			switch (local.RefKind)
			{
				case RefKind.Ref:
					modifiers.Add("ref");
					break;

				case RefKind.RefReadOnly:
					modifiers.Add("ref");
					modifiers.Add("readonly");
					break;
			}

			return modifiers.ToArray();
		}

		/// <summary>
		/// Returns modifiers applied to the target <see cref="ILocalSymbol"/>.
		/// </summary>
		/// <param name="parameter"><see cref="ILocalSymbol"/> to get the modifiers of.</param>
		public static string[] GetModifiers(this IParameterSymbol parameter)
		{
			List<string> modifiers = new(2);

			if (parameter.IsThis)
			{
				modifiers.Add("this");
			}

			if (parameter.RefKind.GetText() is string refKind)
			{
				modifiers.Add(refKind);
			}

			if (parameter.IsParams)
			{
				modifiers.Add("params");
			}

			return modifiers.ToArray();
		}

		/// <summary>
		/// Returns modifiers applied to the target <see cref="IPropertySymbol"/>.
		/// </summary>
		/// <param name="property"><see cref="IPropertySymbol"/> to get the modifiers of.</param>
		public static string[] GetModifiers(this IPropertySymbol property)
		{
			List<string> modifiers = new(8);

			AddAccessibilityModifiers(modifiers, property.DeclaredAccessibility);
			AddBasicMethodModifiers(modifiers, property);

			if (property.IsReadOnly)
			{
				CheckAccessor(property.GetMethod);
			}
			else if (property.IsWriteOnly)
			{
				CheckAccessor(property.SetMethod);
			}
			else if (property.IsReadOnlyContext())
			{
				modifiers.Add("readonly");
			}

			if (property.IsUnsafe())
			{
				modifiers.Add("unsafe");
			}

			return modifiers.ToArray();

			void CheckAccessor(IMethodSymbol? accessor)
			{
				if (accessor is not null && accessor.IsReadOnly)
				{
					modifiers.Add("readonly");
				}
			}
		}

		/// <summary>
		/// Returns modifiers applied to the target <see cref="IEventSymbol"/>.
		/// </summary>
		/// <param name="event"><see cref="IEventSymbol"/> to get the modifiers of.</param>
		public static string[] GetModifiers(this IEventSymbol @event)
		{
			List<string> modifiers = new(8);

			AddAccessibilityModifiers(modifiers, @event.DeclaredAccessibility);
			AddBasicMethodModifiers(modifiers, @event);

			if (@event.IsReadOnlyContext())
			{
				modifiers.Add("readonly");
			}

			if (@event.IsUnsafe())
			{
				modifiers.Add("unsafe");
			}

			return modifiers.ToArray();
		}

		/// <summary>
		/// Returns modifiers applied to the target <see cref="IMethodSymbol"/>.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to get the modifiers of.</param>
		public static string[] GetModifiers(this IMethodSymbol method)
		{
			List<string> modifiers = new(8);

			AddAccessibilityModifiers(modifiers, method.DeclaredAccessibility);
			AddBasicMethodModifiers(modifiers, method);

			if (method.IsReadOnly)
			{
				modifiers.Add("readonly");
			}

			if (method.IsUnsafe())
			{
				modifiers.Add("unsafe");
			}

			if (method.IsAsync)
			{
				modifiers.Add("async");
			}

			if (method.IsPartial(true))
			{
				modifiers.Add("partial");
			}

			return modifiers.ToArray();
		}

		/// <summary>
		/// Returns modifiers applied to the target <see cref="INamedTypeSymbol"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to get the modifiers of.</param>
		public static string[] GetModifiers(this INamedTypeSymbol type)
		{
			List<string> modifiers = new(8);

			AddAccessibilityModifiers(modifiers, type.DeclaredAccessibility);

			if (type.IsStatic)
			{
				modifiers.Add("static");
			}

			if (type.IsNew())
			{
				modifiers.Add("new");
			}

			if (type.IsAbstract)
			{
				modifiers.Add("abstract");
			}
			else if (type.IsRefLikeType)
			{
				modifiers.Add("ref");
			}
			else if (type.IsSealed)
			{
				modifiers.Add("sealed");
			}

			if (type.IsReadOnly)
			{
				modifiers.Add("readonly");
			}

			if (type.IsUnsafe())
			{
				modifiers.Add("unsafe");
			}

			if (type.IsPartial())
			{
				modifiers.Add("partial");
			}

			return modifiers.ToArray();
		}

		/// <summary>
		/// Returns the underlaying <see cref="ITypeSymbol"/> of a nullable <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="ITypeSymbol"/> to get the underlaying <see cref="ITypeSymbol"/> of.</param>
		public static ITypeSymbol? GetNullableUnderlayingType(this ITypeSymbol type)
		{
			if (type.NullableAnnotation == NullableAnnotation.Annotated)
			{
				return type;
			}

			if (type is not INamedTypeSymbol named || !named.IsValueType || named.ConstructedFrom is null || named.ConstructedFrom.SpecialType != SpecialType.System_Nullable_T)
			{
				return default;
			}

			ImmutableArray<ITypeSymbol> arguments = named.TypeArguments;

			if (arguments.Length != 1)
			{
				return default;
			}

			return arguments[0];
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

			SyntaxKind kind = type.SpecialType.GetSyntaxKind();

			if (kind == default)
			{
				return default;
			}

			return SyntaxFactory.PredefinedType(SyntaxFactory.Token(kind));
		}

		/// <summary>
		/// Returns the primary constructor of the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to get the primary constructor of.</param>
		public static IMethodSymbol? GetPrimaryConstructor(this INamedTypeSymbol type)
		{
			return type.InstanceConstructors.FirstOrDefault(ctor => ctor.IsPrimaryConstructor());
		}

		/// <summary>
		/// Returns a <see cref="QualifiedNameSyntax"/> created from the specified <paramref name="namespaces"/>.
		/// </summary>
		/// <param name="namespaces">A collection of <see cref="INamespaceSymbol"/>s to create the <see cref="QualifiedNameSyntax"/> from.</param>
		/// <returns>A <see cref="QualifiedNameSyntax"/> created by combining the <paramref name="namespaces"/>. -or- <see langword="null"/> if there were less then 2 <paramref name="namespaces"/> provided.</returns>
		public static QualifiedNameSyntax? GetQualifiedName(this IEnumerable<INamespaceSymbol> namespaces)
		{
			return AnalysisUtilities.GetQualifiedName(namespaces.Select(n => n.GetVerbatimName()));
		}

		/// <summary>
		/// Returns all struct members with the <see langword="readonly"/> modifier applied.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to get all members with the <see langword="readonly"/> modifier applied.</param>
		/// <remarks>If the <paramref name="type"/> is not a <see langword="struct"/>, empty collection is returned.</remarks>
		public static IEnumerable<ISymbol> GetReadOnlyMembers(this INamedTypeSymbol type)
		{
			if(type.TypeKind != TypeKind.Struct)
			{
				return Array.Empty<ISymbol>();
			}

			return type
				.GetMembers()
				.Where(m => m.IsStructReadOnly());
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
		/// Returns an <see cref="IMethodSymbol"/> representing the given kind of special constructor available from the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to get the special member from.</param>
		/// <param name="kind">Kind of special constructor to return.</param>
		public static IMethodSymbol? GetSpecialConstructor(this INamedTypeSymbol type, SpecialConstructor kind)
		{
			return kind switch
			{
				SpecialConstructor.Static => type.StaticConstructors.FirstOrDefault(),
				SpecialConstructor.None => default,
				_ => type.InstanceConstructors.FirstOrDefault(ctor => ctor.GetConstructorKind() == kind),
			};
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
		/// Returns a <see cref="CSharpSyntaxNode"/> of type <typeparamref name="T"/> associated with the specified <paramref name="symbol"/>.
		/// </summary>
		/// <typeparam name="T">Type of <see cref="CSharpSyntaxNode"/> to return.</typeparam>
		/// <param name="symbol"><see cref="ISymbol"/> to get the <see cref="CSharpSyntaxNode"/> associated with.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <exception cref="InvalidOperationException"><paramref name="symbol"/> is not associated with a syntax node of type <typeparamref name="T"/>.</exception>
		public static T GetSyntax<T>(this ISymbol symbol, CancellationToken cancellationToken = default) where T : CSharpSyntaxNode
		{
			if (!symbol.TryGetSyntax(out T? declaration, cancellationToken))
			{
				throw Exc_SymbolNotAssociatedWithNode(symbol, typeof(T));
			}

			return declaration;
		}

		/// <summary>
		/// Returns a <see cref="BaseMethodDeclarationSyntax"/> associated with the specified <paramref name="method"/>.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to get the <see cref="BaseMethodDeclarationSyntax"/> associated with.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <exception cref="InvalidOperationException"><paramref name="method"/> is not associated with a <see cref="BaseMethodDeclarationSyntax"/>.</exception>
		public static BaseMethodDeclarationSyntax GetSyntax(this IMethodSymbol method, CancellationToken cancellationToken = default)
		{
			return method.GetSyntax<BaseMethodDeclarationSyntax>(cancellationToken);
		}

		/// <summary>
		/// Returns a <see cref="BaseTypeDeclarationSyntax"/> associated with the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to get the <see cref="BaseTypeDeclarationSyntax"/> associated with.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <exception cref="InvalidOperationException"><paramref name="type"/> is not associated with a <see cref="BaseTypeDeclarationSyntax"/>.</exception>
		public static BaseTypeDeclarationSyntax GetSyntax(this INamedTypeSymbol type, CancellationToken cancellationToken = default)
		{
			return type.GetSyntax<BaseTypeDeclarationSyntax>(cancellationToken);
		}

		/// <summary>
		/// Returns a <see cref="ParameterSyntax"/> associated with the specified <paramref name="parameter"/>.
		/// </summary>
		/// <param name="parameter"><see cref="IParameterSymbol"/> to get the <see cref="ParameterSyntax"/> associated with.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <exception cref="InvalidOperationException"><paramref name="parameter"/> is not associated with a <see cref="ParameterSyntax"/>.</exception>
		public static ParameterSyntax GetSyntax(this IParameterSymbol parameter, CancellationToken cancellationToken = default)
		{
			return parameter.GetSyntax<ParameterSyntax>(cancellationToken);
		}

		/// <summary>
		/// Returns a <see cref="TypeParameterSyntax"/> associated with the specified <paramref name="parameter"/>.
		/// </summary>
		/// <param name="parameter"><see cref="INamedTypeSymbol"/> to get the <see cref="TypeParameterSyntax"/> associated with.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <exception cref="TypeParameterSyntax"><paramref name="parameter"/> is not associated with a <see cref="TypeParameterSyntax"/>.</exception>
		public static TypeParameterSyntax GetSyntax(this ITypeParameterSymbol parameter, CancellationToken cancellationToken = default)
		{
			return parameter.GetSyntax<TypeParameterSyntax>(cancellationToken);
		}

		/// <summary>
		/// Returns a <see cref="EventFieldDeclarationSyntax"/> associated with the specified <paramref name="event"/>.
		/// </summary>
		/// <param name="event"><see cref="IEventSymbol"/> to get the <see cref="EventFieldDeclarationSyntax"/> associated with.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <exception cref="InvalidOperationException"><paramref name="event"/> is not associated with a <see cref="EventFieldDeclarationSyntax"/>.</exception>
		public static EventFieldDeclarationSyntax GetSyntax(this IEventSymbol @event, CancellationToken cancellationToken = default)
		{
			if (!@event.TryGetSyntax(out VariableDeclaratorSyntax? syntax, cancellationToken) || syntax.Parent?.Parent is not EventFieldDeclarationSyntax decl)
			{
				throw Exc_SymbolNotAssociatedWithNode(@event, typeof(EventFieldDeclarationSyntax));
			}

			return decl;
		}

		/// <summary>
		/// Returns a <see cref="FieldDeclarationSyntax"/> associated with the specified <paramref name="field"/>.
		/// </summary>
		/// <param name="field"><see cref="IFieldSymbol"/> to get the <see cref="FieldDeclarationSyntax"/> associated with.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <exception cref="InvalidOperationException"><paramref name="field"/> is not associated with a <see cref="FieldDeclarationSyntax"/>.</exception>
		public static FieldDeclarationSyntax GetSyntax(this IFieldSymbol field, CancellationToken cancellationToken = default)
		{
			if (!field.TryGetSyntax(out VariableDeclaratorSyntax? syntax, cancellationToken) || syntax.Parent?.Parent is not FieldDeclarationSyntax decl)
			{
				throw Exc_SymbolNotAssociatedWithNode(field, typeof(FieldDeclarationSyntax));
			}

			return decl;
		}

		/// <summary>
		/// Returns a <see cref="LocalDeclarationStatementSyntax"/> associated with the specified <paramref name="local"/>.
		/// </summary>
		/// <param name="local"><see cref="ILocalSymbol"/> to get the <see cref="LocalDeclarationStatementSyntax"/> associated with.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <exception cref="InvalidOperationException"><paramref name="local"/> is not associated with a <see cref="LocalDeclarationStatementSyntax"/>.</exception>
		public static LocalDeclarationStatementSyntax GetSyntax(this ILocalSymbol local, CancellationToken cancellationToken = default)
		{
			if (!local.TryGetSyntax(out VariableDeclaratorSyntax? syntax, cancellationToken) || syntax.Parent?.Parent is not LocalDeclarationStatementSyntax decl)
			{
				throw Exc_SymbolNotAssociatedWithNode(local, typeof(LocalDeclarationStatementSyntax));
			}

			return decl;
		}

		/// <summary>
		/// Returns a <see cref="BasePropertyDeclarationSyntax"/> associated with the specified <paramref name="property"/>.
		/// </summary>
		/// <param name="property"><see cref="IPropertySymbol"/> to get the <see cref="BasePropertyDeclarationSyntax"/> associated with.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <exception cref="InvalidOperationException"><paramref name="property"/> is not associated with a <see cref="BasePropertyDeclarationSyntax"/>.</exception>
		public static BasePropertyDeclarationSyntax GetSyntax(this IPropertySymbol property, CancellationToken cancellationToken = default)
		{
			return property.GetSyntax<BasePropertyDeclarationSyntax>(cancellationToken);
		}

		/// <summary>
		/// Returns a <see cref="LabeledStatementSyntax"/> associated with the specified <paramref name="label"/>.
		/// </summary>
		/// <param name="label"><see cref="ILabelSymbol"/> to get the <see cref="LabeledStatementSyntax"/> associated with.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <exception cref="InvalidOperationException"><paramref name="label"/> is not associated with a <see cref="LabeledStatementSyntax"/>.</exception>
		public static LabeledStatementSyntax GetSyntax(this ILabelSymbol label, CancellationToken cancellationToken = default)
		{
			return label.GetSyntax<LabeledStatementSyntax>(cancellationToken);
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

			return type.SpecialType.GetKeywordText();
		}

		/// <summary>
		/// Returns a new <see cref="UsingDirectiveSyntax"/> build for the specified <paramref name="namespace"/> symbol.
		/// </summary>
		/// <param name="namespace"><see cref="INamespaceSymbol"/> to built the <see cref="UsingDirectiveSyntax"/> from.</param>
		public static UsingDirectiveSyntax GetUsingDirective(this INamespaceSymbol @namespace)
		{
			NameSyntax name;

			if (@namespace.GetContainingNamespaces().GetQualifiedName() is QualifiedNameSyntax q)
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

			if (namespaces.GetQualifiedName() is QualifiedNameSyntax q)
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
		/// Returns the <see cref="Virtuality"/> of the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the virtuality of.</param>
		public static Virtuality GetVirtuality(this ISymbol symbol)
		{
			if(symbol.IsAbstract)
			{
				return Virtuality.Abstract;
			}

			if(symbol.IsSealed)
			{
				return Virtuality.Sealed;
			}

			if(symbol.IsVirtual)
			{
				return Virtuality.Virtual;
			}

			return default;
		}

		/// <summary>
		/// Returns new <see cref="IMemberData"/> created for the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="ITypeSymbol"/> to create the <see cref="IMemberData"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="IMemberData"/> from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static IMemberData ToData(this ITypeSymbol type, ICompilationData compilation)
		{
			switch (type)
			{
				case INamedTypeSymbol named:
					return named.ToData(compilation);

				case ITypeParameterSymbol typeParameter:
					return typeParameter.ToData(compilation);

				default:

					if(type is null)
					{
						throw new ArgumentNullException(nameof(type));
					}

					if(compilation is null)
					{
						throw new ArgumentNullException(nameof(compilation));
					}

					return new UnknownTypeData(type, compilation);
			}
		}

		/// <summary>
		/// Returns new <see cref="IMemberData"/> created for the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to create the <see cref="IMemberData"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="IMemberData"/> from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static IMemberData ToData(this INamedTypeSymbol type, ICompilationData compilation)
		{
			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

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
		/// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static IMethodData ToData(this IMethodSymbol method, ICompilationData compilation)
		{
			if (method is null)
			{
				throw new ArgumentNullException(nameof(method));
			}

			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			return method.MethodKind switch
			{
				MethodKind.Ordinary => new MethodData(method, compilation),
				MethodKind.UserDefinedOperator => new OperatorData(method, compilation),
				MethodKind.Constructor or MethodKind.StaticConstructor => new ConstructorData(method, compilation),
				MethodKind.Destructor => new DestructorData(method, compilation),
				MethodKind.LocalFunction => new LocalFunctionData(method, compilation),
				MethodKind.Conversion => new ConversionOperatorData(method, compilation),
				MethodKind.AnonymousFunction => new LambdaData(method, compilation),
				_ => method.IsAccessor()
					? new AccessorData(method, compilation)
					: new UnknownMethodData(method, compilation)
			};
		}

		/// <summary>
		/// Returns new <see cref="IMemberData"/> created for the specified <paramref name="field"/>.
		/// </summary>
		/// <param name="field"><see cref="IFieldSymbol"/> to create the <see cref="IMemberData"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="IMemberData"/> from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="field"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static IMemberData ToData(this IFieldSymbol field, ICompilationData compilation)
		{
			if (field is null)
			{
				throw new ArgumentNullException(nameof(field));
			}

			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			return new FieldData(field, compilation);
		}

		/// <summary>
		/// Returns new <see cref="IMemberData"/> created for the specified <paramref name="event"/>.
		/// </summary>
		/// <param name="event"><see cref="IEventSymbol"/> to create the <see cref="IMemberData"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="IMemberData"/> from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="event"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static IMemberData ToData(this IEventSymbol @event, ICompilationData compilation)
		{
			if (@event is null)
			{
				throw new ArgumentNullException(nameof(@event));
			}

			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			return new EventData(@event, compilation);
		}

		/// <summary>
		/// Returns new <see cref="IMemberData"/> created for the specified <paramref name="property"/>.
		/// </summary>
		/// <param name="property"><see cref="IPropertySymbol"/> to create the <see cref="IMemberData"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="IMemberData"/> from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="property"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static IMemberData ToData(this IPropertySymbol property, ICompilationData compilation)
		{
			if (property is null)
			{
				throw new ArgumentNullException(nameof(property));
			}

			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

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
		/// <exception cref="ArgumentNullException"><paramref name="local"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static IMemberData ToData(this ILocalSymbol local, ICompilationData compilation)
		{
			if (local is null)
			{
				throw new ArgumentNullException(nameof(local));
			}

			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			return new LocalData(local, compilation);
		}

		/// <summary>
		/// Returns new <see cref="IMemberData"/> created for the specified <paramref name="typeParameter"/>.
		/// </summary>
		/// <param name="typeParameter"><see cref="ITypeParameterSymbol"/> to create the <see cref="IMemberData"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="IMemberData"/> from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="typeParameter"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static IMemberData ToData(this ITypeParameterSymbol typeParameter, ICompilationData compilation)
		{
			if (typeParameter is null)
			{
				throw new ArgumentNullException(nameof(typeParameter));
			}

			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			return new TypeParameterData(typeParameter, compilation);
		}

		/// <summary>
		/// Returns new <see cref="IMemberData"/> created for the specified <paramref name="parameter"/>.
		/// </summary>
		/// <param name="parameter"><see cref="IParameterSymbol"/> to create the <see cref="IMemberData"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="IMemberData"/> from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="parameter"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static IMemberData ToData(this IParameterSymbol parameter, ICompilationData compilation)
		{
			if (parameter is null)
			{
				throw new ArgumentNullException(nameof(parameter));
			}

			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			return new ParameterData(parameter, compilation);
		}

		/// <summary>
		/// Returns new <see cref="IMemberData"/> created for the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="INamespaceOrTypeSymbol"/> to create the <see cref="IMemberData"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="IMemberData"/> from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static IMemberData ToData(this INamespaceOrTypeSymbol symbol, ICompilationData compilation)
		{
			switch (symbol)
			{
				case INamespaceSymbol @namespace:
					return @namespace.ToData(compilation);

				case ITypeSymbol type:
					return type.ToData(compilation);

				default:

					if(symbol is null)
					{
						throw new ArgumentNullException(nameof(symbol));
					}

					if(compilation is null)
					{
						throw new ArgumentNullException(nameof(compilation));
					}

					return new NamespaceOrTypeData(symbol, compilation);
			}
		}

		/// <summary>
		/// Returns new <see cref="IMemberData"/> created for the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to create the <see cref="IMemberData"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="IMemberData"/> from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static IMemberData ToData(this ISymbol symbol, ICompilationData compilation)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

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
				null => throw new ArgumentNullException(nameof(symbol)),
				_ => new MemberData(symbol, compilation)
			};
		}

		/// <summary>
		/// Returns new <see cref="IMemberData"/> created for the specified <paramref name="namespace"/>.
		/// </summary>
		/// <param name="namespace"><see cref="INamespaceSymbol"/> to create the <see cref="IMemberData"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="IMemberData"/> from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="namespace"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static IMemberData ToData(this INamespaceSymbol @namespace, ICompilationData compilation)
		{
			if (@namespace is null)
			{
				throw new ArgumentNullException(nameof(@namespace));
			}

			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			return new NamespaceData(@namespace, compilation);
		}

		/// <summary>
		/// Returns new <see cref="ISymbolOrMember"/> created for the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to create the <see cref="ISymbolOrMember"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="ISymbolOrMember"/> from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public static ISymbolOrMember ToDataOrSymbol(this ISymbol symbol, ICompilationData? compilation = default)
		{
			return symbol switch
			{
				ITypeParameterSymbol typeParameter => typeParameter.ToDataOrSymbol(compilation),
				INamedTypeSymbol type => type.ToDataOrSymbol(compilation),
				IMethodSymbol method => method.ToDataOrSymbol(compilation),
				IPropertySymbol property => property.ToDataOrSymbol(compilation),
				IFieldSymbol field => field.ToDataOrSymbol(compilation),
				IEventSymbol @event => @event.ToDataOrSymbol(compilation),
				IParameterSymbol parameter => parameter.ToDataOrSymbol(compilation),
				INamespaceSymbol @namespace => @namespace.ToDataOrSymbol(compilation),
				ILocalSymbol local => local.ToDataOrSymbol(compilation),
				ITypeSymbol unknownType => new SymbolOrMemberWrapper<ITypeSymbol, ITypeData>(unknownType, compilation),
				null => throw new ArgumentNullException(nameof(symbol)),
				_ => new SymbolOrMemberWrapper<ISymbol, IMemberData>(symbol, compilation)
			};
		}

		/// <summary>
		/// Returns new <see cref="ISymbolOrMember"/> created for the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="ITypeSymbol"/> to create the <see cref="ISymbolOrMember"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="ISymbolOrMember"/> from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
		public static ISymbolOrMember<ITypeSymbol> ToDataOrSymbol(this ITypeSymbol type, ICompilationData? compilation = default)
		{
			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			return type.ToDataOrSymbolInternal(compilation);
		}

		/// <summary>
		/// Returns new <see cref="ISymbolOrMember"/> created for the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="INamespaceOrTypeSymbol"/> to create the <see cref="ISymbolOrMember"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="ISymbolOrMember"/> from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public static ISymbolOrMember<INamespaceOrTypeSymbol> ToDataOrSymbol(this INamespaceOrTypeSymbol symbol, ICompilationData? compilation = default)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			return symbol.ToDataOrSymbolInternal(compilation);
		}

		/// <summary>
		/// Returns new <see cref="ISymbolOrMember"/> created for the specified <paramref name="parameter"/>.
		/// </summary>
		/// <param name="parameter"><see cref="IParameterSymbol"/> to create the <see cref="ISymbolOrMember"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="ISymbolOrMember"/> from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="parameter"/> is <see langword="null"/>.</exception>
		public static ISymbolOrMember<IParameterSymbol> ToDataOrSymbol(this IParameterSymbol parameter, ICompilationData? compilation = default)
		{
			if (parameter is null)
			{
				throw new ArgumentNullException(nameof(parameter));
			}

			return parameter.ToDataOrSymbolInternal(compilation);
		}

		/// <summary>
		/// Returns new <see cref="ISymbolOrMember"/> created for the specified <paramref name="method"/>.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to create the <see cref="ISymbolOrMember"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="ISymbolOrMember"/> from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>.</exception>
		public static ISymbolOrMember<IMethodSymbol, IMethodData> ToDataOrSymbol(this IMethodSymbol method, ICompilationData? compilation = default)
		{
			if (method is null)
			{
				throw new ArgumentNullException(nameof(method));
			}

			return method.ToDataOrSymbolInternal(compilation);
		}

		/// <summary>
		/// Returns new <see cref="ISymbolOrMember"/> created for the specified <paramref name="typeParameter"/>.
		/// </summary>
		/// <param name="typeParameter"><see cref="ITypeParameterSymbol"/> to create the <see cref="ISymbolOrMember"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="ISymbolOrMember"/> from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="typeParameter"/> is <see langword="null"/>.</exception>
		public static ISymbolOrMember<ITypeParameterSymbol> ToDataOrSymbol(this ITypeParameterSymbol typeParameter, ICompilationData? compilation = default)
		{
			if (typeParameter is null)
			{
				throw new ArgumentNullException(nameof(typeParameter));
			}

			return typeParameter.ToDataOrSymbolInternal(compilation);
		}

		/// <summary>
		/// Returns new <see cref="ISymbolOrMember"/> created for the specified <paramref name="namespace"/>.
		/// </summary>
		/// <param name="namespace"><see cref="INamespaceSymbol"/> to create the <see cref="ISymbolOrMember"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="ISymbolOrMember"/> from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="namespace"/> is <see langword="null"/>.</exception>
		public static ISymbolOrMember<INamespaceSymbol> ToDataOrSymbol(this INamespaceSymbol @namespace, ICompilationData? compilation = default)
		{
			if (@namespace is null)
			{
				throw new ArgumentNullException(nameof(@namespace));
			}

			return @namespace.ToDataOrSymbolInternal(compilation);
		}

		/// <summary>
		/// Returns new <see cref="ISymbolOrMember"/> created for the specified <paramref name="event"/>.
		/// </summary>
		/// <param name="event"><see cref="IEventSymbol"/> to create the <see cref="ISymbolOrMember"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="ISymbolOrMember"/> from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="event"/> is <see langword="null"/>.</exception>
		public static ISymbolOrMember<IEventSymbol> ToDataOrSymbol(this IEventSymbol @event, ICompilationData? compilation = default)
		{
			if (@event is null)
			{
				throw new ArgumentNullException(nameof(@event));
			}

			return @event.ToDataOrSymbolInternal(compilation);
		}

		/// <summary>
		/// Returns new <see cref="ISymbolOrMember"/> created for the specified <paramref name="field"/>.
		/// </summary>
		/// <param name="field"><see cref="IFieldSymbol"/> to create the <see cref="ISymbolOrMember"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="ISymbolOrMember"/> from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="field"/> is <see langword="null"/>.</exception>
		public static ISymbolOrMember<IFieldSymbol> ToDataOrSymbol(this IFieldSymbol field, ICompilationData? compilation = default)
		{
			if (field is null)
			{
				throw new ArgumentNullException(nameof(field));
			}

			return field.ToDataOrSymbolInternal(compilation);
		}

		/// <summary>
		/// Returns new <see cref="ISymbolOrMember"/> created for the specified <paramref name="property"/>.
		/// </summary>
		/// <param name="property"><see cref="IPropertySymbol"/> to create the <see cref="ISymbolOrMember"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="ISymbolOrMember"/> from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="property"/> is <see langword="null"/>.</exception>
		public static ISymbolOrMember<IPropertySymbol> ToDataOrSymbol(this IPropertySymbol property, ICompilationData? compilation = default)
		{
			if (property is null)
			{
				throw new ArgumentNullException(nameof(property));
			}

			return property.ToDataOrSymbolInternal(compilation);
		}

		/// <summary>
		/// Returns new <see cref="ISymbolOrMember"/> created for the specified <paramref name="local"/>.
		/// </summary>
		/// <param name="local"><see cref="ILocalSymbol"/> to create the <see cref="ISymbolOrMember"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="ISymbolOrMember"/> from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="local"/> is <see langword="null"/>.</exception>
		public static ISymbolOrMember<ILocalSymbol> ToDataOrSymbol(this ILocalSymbol local, ICompilationData? compilation = default)
		{
			if (local is null)
			{
				throw new ArgumentNullException(nameof(local));
			}

			return local.ToDataOrSymbolInternal(compilation);
		}

		/// <summary>
		/// Returns new <see cref="ISymbolOrMember"/> created for the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to create the <see cref="ISymbolOrMember"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to create the <see cref="ISymbolOrMember"/> from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
		public static ISymbolOrMember<INamedTypeSymbol> ToDataOrSymbol(this INamedTypeSymbol type, ICompilationData? compilation = default)
		{
			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			return type.ToDataOrSymbolInternal(compilation);
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

		internal static IReturnOrderEnumerable<T> OrderBy<T>(this IEnumerable<T> collection, ReturnOrder order, bool reverse = true)
		{
			if (reverse && order == ReturnOrder.Root)
			{
				collection = collection.Reverse();
			}

			return new ReturnOrderEnumerable<T>(collection, order);
		}

		[return: NotNullIfNotNull("symbol")]
		internal static SymbolOrMemberWrapper<ISymbol, IMemberData>? ToDataOrSymbolInternal(this ISymbol? symbol, ICompilationData? compilation)
		{
			if (symbol is null)
			{
				return null;
			}

			return new SymbolOrMemberWrapper<ISymbol, IMemberData>(symbol, compilation);
		}

		[return: NotNullIfNotNull("type")]
		internal static SymbolOrMemberWrapper<ITypeSymbol, IMemberData>? ToDataOrSymbolInternal(this ITypeSymbol type, ICompilationData? compilation)
		{
			if (type is null)
			{
				return null;
			}

			return new SymbolOrMemberWrapper<ITypeSymbol, IMemberData>(type, compilation);
		}

		[return: NotNullIfNotNull("symbol")]
		internal static SymbolOrMemberWrapper<INamespaceOrTypeSymbol, IMemberData>? ToDataOrSymbolInternal(this INamespaceOrTypeSymbol? symbol, ICompilationData? compilation)
		{
			if (symbol is null)
			{
				return null;
			}

			return new SymbolOrMemberWrapper<INamespaceOrTypeSymbol, IMemberData>(symbol, compilation);
		}

		[return: NotNullIfNotNull("parameter")]
		internal static SymbolOrMemberWrapper<IParameterSymbol, IMemberData>? ToDataOrSymbolInternal(this IParameterSymbol? parameter, ICompilationData? compilation)
		{
			if (parameter is null)
			{
				return null;
			}

			return new SymbolOrMemberWrapper<IParameterSymbol, IMemberData>(parameter, compilation);
		}

		[return: NotNullIfNotNull("method")]
		internal static SymbolOrMemberWrapper<IMethodSymbol, IMethodData>? ToDataOrSymbolInternal(this IMethodSymbol? method, ICompilationData? compilation)
		{
			if (method is null)
			{
				return null;
			}

			return new SymbolOrMemberWrapper<IMethodSymbol, IMethodData>(method, compilation);
		}

		[return: NotNullIfNotNull("typeParameter")]
		internal static SymbolOrMemberWrapper<ITypeParameterSymbol, IMemberData>? ToDataOrSymbolInternal(this ITypeParameterSymbol? typeParameter, ICompilationData? compilation)
		{
			if (typeParameter is null)
			{
				return null;
			}

			return new SymbolOrMemberWrapper<ITypeParameterSymbol, IMemberData>(typeParameter, compilation);
		}

		[return: NotNullIfNotNull("namespace")]
		internal static SymbolOrMemberWrapper<INamespaceSymbol, IMemberData>? ToDataOrSymbolInternal(this INamespaceSymbol? @namespace, ICompilationData? compilation)
		{
			if (@namespace is null)
			{
				return null;
			}

			return new SymbolOrMemberWrapper<INamespaceSymbol, IMemberData>(@namespace, compilation);
		}

		[return: NotNullIfNotNull("event")]
		internal static SymbolOrMemberWrapper<IEventSymbol, IMemberData>? ToDataOrSymbolInternal(this IEventSymbol? @event, ICompilationData? compilation)
		{
			if (@event is null)
			{
				return null;
			}

			return new SymbolOrMemberWrapper<IEventSymbol, IMemberData>(@event, compilation);
		}

		[return: NotNullIfNotNull("field")]
		internal static SymbolOrMemberWrapper<IFieldSymbol, IMemberData>? ToDataOrSymbolInternal(this IFieldSymbol? field, ICompilationData? compilation)
		{
			if (field is null)
			{
				return null;
			}

			return new SymbolOrMemberWrapper<IFieldSymbol, IMemberData>(field, compilation);
		}

		[return: NotNullIfNotNull("property")]
		internal static SymbolOrMemberWrapper<IPropertySymbol, IMemberData>? ToDataOrSymbolInternal(this IPropertySymbol? property, ICompilationData? compilation)
		{
			if (property is null)
			{
				return null;
			}

			return new SymbolOrMemberWrapper<IPropertySymbol, IMemberData>(property, compilation);
		}

		[return: NotNullIfNotNull("local")]
		internal static SymbolOrMemberWrapper<ILocalSymbol, IMemberData>? ToDataOrSymbolInternal(this ILocalSymbol? local, ICompilationData? compilation)
		{
			if (local is null)
			{
				return null;
			}

			return new SymbolOrMemberWrapper<ILocalSymbol, IMemberData>(local, compilation);
		}

		[return: NotNullIfNotNull("type")]
		internal static SymbolOrMemberWrapper<INamedTypeSymbol, IMemberData>? ToDataOrSymbolInternal(this INamedTypeSymbol? type, ICompilationData? compilation)
		{
			if (type is null)
			{
				return null;
			}

			return new SymbolOrMemberWrapper<INamedTypeSymbol, IMemberData>(type, compilation);
		}

		private static void AddAccessibilityModifiers(List<string> modifiers, Accessibility accessibility)
		{
			switch (accessibility)
			{
				case Accessibility.Public:
					modifiers.Add("public");
					break;

				case Accessibility.Private:
					modifiers.Add("private");
					break;

				case Accessibility.Protected:
					modifiers.Add("protected");
					break;

				case Accessibility.Internal:
					modifiers.Add("internal");
					break;

				case Accessibility.ProtectedOrInternal:
					modifiers.Add("protected");
					modifiers.Add("internal");
					break;

				case Accessibility.ProtectedAndInternal:
					modifiers.Add("private");
					modifiers.Add("protected");
					break;
			}
		}

		private static void AddBasicMethodModifiers(List<string> modifiers, ISymbol symbol)
		{
			if (symbol.IsStatic)
			{
				modifiers.Add("static");
			}

			if (symbol.IsExtern)
			{
				modifiers.Add("extern");
			}

			if (symbol.IsNew())
			{
				modifiers.Add("new");
			}

			if (symbol.IsAbstract)
			{
				modifiers.Add("abstract");
			}
			else if (symbol.IsSealed)
			{
				modifiers.Add("sealed");
			}
			else if (symbol.IsVirtual)
			{
				modifiers.Add("virtual");
			}

			if (symbol.IsOverride)
			{
				modifiers.Add("override");
			}
		}

		private static TypeSyntax ApplyAnnotation(TypeSyntax syntax, NullableAnnotation annotation)
		{
			if (annotation == NullableAnnotation.Annotated)
			{
				return SyntaxFactory.NullableType(syntax);
			}

			return syntax;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static InvalidOperationException Exc_SymbolNotAssociatedWithNode(ISymbol symbol, Type type)
		{
			return new InvalidOperationException($"Method '{symbol}' is not associated with a syntax node of type '{type.Name}'");
		}

		private static IEnumerable<INamedTypeSymbol> GetBaseTypes_Internal(this INamedTypeSymbol type, bool includeSelf)
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
