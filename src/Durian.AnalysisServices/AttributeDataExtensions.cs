﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis;

/// <summary>
/// Contains various extension methods for the <see cref="AttributeData"/> class.
/// </summary>
public static class AttributeDataExtensions
{
	/// <summary>
	/// Returns a <see cref="TypedConstant"/> representing the named argument at the specified <paramref name="position"/>.
	/// </summary>
	/// <param name="attribute"><see cref="AttributeData"/> to get the <see cref="TypedConstant"/> from.</param>
	/// <param name="position">Position where the target argument is to be found.</param>
	public static TypedConstant GetConstructorArgument(this AttributeData attribute, int position)
	{
		attribute.TryGetConstructorArgument(position, out TypedConstant value);
		return value;
	}

	/// <summary>
	/// Returns an <see cref="ImmutableArray"/> of <see cref="TypedConstant"/>s representing the <see cref="Array"/> value of the constructor argument at the specified <paramref name="position"/>.
	/// </summary>
	/// <param name="attribute"><see cref="AttributeData"/> to get the values from.</param>
	/// <param name="position">Position where the target argument is to be found.</param>
	public static ImmutableArray<TypedConstant> GetConstructorArgumentArrayValue(this AttributeData attribute, int position)
	{
		attribute.TryGetConstructorArgumentArrayValue(position, out ImmutableArray<TypedConstant> array);
		return array;
	}

	/// <summary>
	/// Returns an <see cref="ImmutableArray{T}"/> of <see cref="TypedConstant"/>s representing the <see cref="Array"/> value of the constructor argument at the specified <paramref name="position"/>.
	/// </summary>
	/// <typeparam name="T">Type of array's elements.</typeparam>
	/// <param name="attribute"><see cref="AttributeData"/> to get the values from.</param>
	/// <param name="position">Position where the target argument is to be found.</param>
	public static ImmutableArray<T> GetConstructorArgumentArrayValue<T>(this AttributeData attribute, int position)
	{
		attribute.TryGetConstructorArgumentArrayValue(position, out ImmutableArray<T> array);
		return array;
	}

	/// <summary>
	/// Returns the enum value of the constructor argument at the specified <paramref name="position"/>.
	/// </summary>
	/// <typeparam name="TEnum">Type of enum value to return.</typeparam>
	/// <param name="attribute"><see cref="AttributeData"/> to get the value from.</param>
	/// <param name="position">Position where the target argument is to be found.</param>
	/// <exception cref="InvalidOperationException">Type size mismatch.</exception>
	public static TEnum GetConstructorArgumentEnumValue<TEnum>(this AttributeData attribute, int position) where TEnum : unmanaged, Enum
	{
		attribute.TryGetConstructorArgumentEnumValue(position, out TEnum value);
		return value;
	}

	/// <summary>
	/// Returns the enum value of the constructor argument at the specified <paramref name="position"/>.
	/// </summary>
	/// <typeparam name="TEnum">Type of enum value to return.</typeparam>
	/// <typeparam name="TType">Type the <typeparamref name="TEnum"/> type extends.</typeparam>
	/// <param name="attribute"><see cref="AttributeData"/> to get the value from.</param>
	/// <param name="position">Position where the target argument is to be found.</param>
	/// <exception cref="InvalidOperationException">Type size mismatch.</exception>
	public static TEnum GetConstructorArgumentEnumValue<TEnum, TType>(this AttributeData attribute, int position)
		where TEnum : unmanaged, Enum
		where TType : unmanaged
	{
		attribute.TryGetConstructorArgumentEnumValue<TEnum, TType>(position, out TEnum value);
		return value;
	}

	/// <summary>
	/// Returns location of attribute argument at the specified <paramref name="position"/>
	/// or location of the <paramref name="attribute"/> is no appropriate argument was found.
	/// <para>If for some reason source syntax of the <paramref name="attribute"/> is not accessible, <see langword="null"/> is returned instead.</para>
	/// </summary>
	/// <param name="attribute"><see cref="AttributeSyntax"/> to get the location of argument of.</param>
	/// <param name="position">Position of argument to get.</param>
	public static Location? GetConstructorArgumentLocation(this AttributeData attribute, int position)
	{
		if (attribute.ApplicationSyntaxReference is null)
		{
			return null;
		}

		SyntaxNode node = attribute.ApplicationSyntaxReference.GetSyntax();

		if (node is not AttributeSyntax attr)
		{
			return node.GetLocation();
		}

		return attr.GetArgumentLocation(position);
	}

	/// <summary>
	/// Returns a <see cref="ITypeSymbol"/> representing the <see cref="Type"/> value of the constructor argument at the specified <paramref name="position"/>.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="ITypeSymbol"/> to return.</typeparam>
	/// <param name="attribute"><see cref="AttributeData"/> to get the value from.</param>
	/// <param name="position">Position where the target argument is to be found.</param>
	public static T? GetConstructorArgumentTypeValue<T>(this AttributeData attribute, int position) where T : ITypeSymbol
	{
		attribute.TryGetConstructorArgumentTypeValue(position, out T? symbol);
		return symbol;
	}

	/// <summary>
	/// Returns the value of the constructor argument at the specified <paramref name="position"/>.
	/// </summary>
	/// <typeparam name="T">Type of value to return.</typeparam>
	/// <param name="attribute"><see cref="AttributeData"/> to get the value from.</param>
	/// <param name="position">Position where the target argument is to be found.</param>
	public static T? GetConstructorArgumentValue<T>(this AttributeData attribute, int position)
	{
		attribute.TryGetConstructorArgumentValue(position, out T? value);
		return value;
	}

	/// <summary>
	/// Returns the <see cref="Location"/> of the specified <paramref name="attribute"/>.
	/// </summary>
	public static Location? GetLocation(this AttributeData attribute)
	{
		if (attribute.ApplicationSyntaxReference is null)
		{
			return null;
		}

		return Location.Create(attribute.ApplicationSyntaxReference.SyntaxTree, attribute.ApplicationSyntaxReference.Span);
	}

	/// <summary>
	/// Returns a <see cref="TypedConstant"/> representing the named argument with the specified <paramref name="argumentName"/>.
	/// </summary>
	/// <param name="attribute"><see cref="AttributeData"/> to get the <see cref="TypedConstant"/> from.</param>
	/// <param name="argumentName">Name of the argument to get the <see cref="TypedConstant"/> of.</param>
	public static TypedConstant GetNamedArgument(this AttributeData attribute, string argumentName)
	{
		attribute.TryGetNamedArgument(argumentName, out TypedConstant value);
		return value;
	}

	/// <summary>
	/// Returns an <see cref="ImmutableArray"/> of <see cref="TypedConstant"/>s representing the <see cref="Array"/> value of the named argument with the specified <paramref name="argumentName"/>.
	/// </summary>
	/// <param name="attribute"><see cref="AttributeData"/> to get the values from.</param>
	/// <param name="argumentName">Name of the argument to get the values of.</param>
	public static ImmutableArray<TypedConstant> GetNamedArgumentArrayValue(this AttributeData attribute, string argumentName)
	{
		attribute.TryGetNamedArgumentArrayValue(argumentName, out ImmutableArray<TypedConstant> array);
		return array;
	}

	/// <summary>
	/// Returns an <see cref="ImmutableArray"/> of <see cref="TypedConstant"/>s representing the <see cref="Array"/> value of the named argument with the specified <paramref name="argumentName"/>.
	/// </summary>
	/// <typeparam name="T">Type of array's elements.</typeparam>
	/// <param name="attribute"><see cref="AttributeData"/> to get the values from.</param>
	/// <param name="argumentName">Name of the argument to get the values of.</param>
	public static ImmutableArray<T> GetNamedArgumentArrayValue<T>(this AttributeData attribute, string argumentName)
	{
		attribute.TryGetNamedArgumentArrayValue(argumentName, out ImmutableArray<T> array);
		return array;
	}

	/// <summary>
	/// Returns the enum value of the named argument with the specified <paramref name="argumentName"/>.
	/// </summary>
	/// <typeparam name="TEnum">Type of enum value to return.</typeparam>
	/// <param name="attribute"><see cref="AttributeData"/> to get the value from.</param>
	/// <param name="argumentName">Name of the argument to get the values of.</param>
	/// <exception cref="InvalidOperationException">Type size mismatch.</exception>
	public static TEnum GetNamedArgumentEnumValue<TEnum>(this AttributeData attribute, string argumentName) where TEnum : unmanaged, Enum
	{
		attribute.TryGetNamedArgumentEnumValue(argumentName, out TEnum value);
		return value;
	}

	/// <summary>
	/// Returns the enum value of the named argument with the specified <paramref name="argumentName"/>.
	/// </summary>
	/// <typeparam name="TEnum">Type of enum value to return.</typeparam>
	/// <typeparam name="TType">Type the <typeparamref name="TEnum"/> type extends.</typeparam>
	/// <param name="attribute"><see cref="AttributeData"/> to get the value from.</param>
	/// <param name="argumentName">Name of the argument to get the values of.</param>
	/// <exception cref="InvalidOperationException">Type size mismatch.</exception>
	public static TEnum GetNamedArgumentEnumValue<TEnum, TType>(this AttributeData attribute, string argumentName)
		where TEnum : unmanaged, Enum
		where TType : unmanaged
	{
		attribute.TryGetNamedArgumentEnumValue<TEnum, TType>(argumentName, out TEnum value);
		return value;
	}

	/// <summary>
	/// Returns location of attribute argument with the specified <paramref name="argumentName"/>
	/// or location of the <paramref name="attribute"/> if no argument with the <paramref name="argumentName"/> was found.
	/// <para>If for some reason source syntax of the <paramref name="attribute"/> is not accessible, <see langword="null"/> is returned instead.</para>
	/// </summary>
	/// <param name="attribute"><see cref="AttributeData"/> to get the location of argument of.</param>
	/// <param name="argumentName">Name of argument to get the location of.</param>
	public static Location? GetNamedArgumentLocation(this AttributeData attribute, string argumentName)
	{
		if (attribute.ApplicationSyntaxReference is null)
		{
			return null;
		}

		SyntaxNode node = attribute.ApplicationSyntaxReference.GetSyntax();

		if (node is not AttributeSyntax attr)
		{
			return node.GetLocation();
		}

		return attr.GetArgumentLocation(argumentName);
	}

	/// <summary>
	/// Returns a <see cref="ITypeSymbol"/> representing the <see cref="Type"/> value of the named argument with the specified <paramref name="argumentName"/>.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="ITypeSymbol"/> to return.</typeparam>
	/// <param name="attribute"><see cref="AttributeData"/> to get the value from.</param>
	/// <param name="argumentName">Name of the argument to get the value of.</param>
	public static T? GetNamedArgumentTypeValue<T>(this AttributeData attribute, string argumentName) where T : ITypeSymbol
	{
		attribute.TryGetNamedArgumentTypeValue(argumentName, out T? symbol);
		return symbol;
	}

	/// <summary>
	/// Returns the value of the named argument with the specified <paramref name="argumentName"/>.
	/// </summary>
	/// <typeparam name="T">Type of value to return.</typeparam>
	/// <param name="attribute"><see cref="AttributeData"/> to get the value from.</param>
	/// <param name="argumentName">Name of the argument to get the value of.</param>
	public static T? GetNamedArgumentValue<T>(this AttributeData attribute, string argumentName)
	{
		attribute.TryGetNamedArgumentValue(argumentName, out T? value);
		return value;
	}

	/// <summary>
	/// Returns the kind of <see cref="NullableAnnotationAttribute"/> this <paramref name="attribute"/> represents.
	/// </summary>
	/// <param name="attribute"><see cref="AttributeData"/> to get the <see cref="NullableAnnotationAttribute"/> kind of.</param>
	public static NullableAnnotationAttribute GetNullableAnnotationAttributeKind(this AttributeData attribute)
	{
		return attribute.AttributeClass?.GetNullableAnnotationAttributeKind() ?? default;
	}

	/// <summary>
	/// Returns the kind of <see cref="SpecialAttribute"/> this <paramref name="attribute"/> represents.
	/// </summary>
	/// <param name="attribute"><see cref="AttributeData"/> to get the <see cref="SpecialAttribute"/> kind of.</param>
	public static SpecialAttribute GetSpecialAttributeKind(this AttributeData attribute)
	{
		return attribute.AttributeClass?.GetSpecialAttributeKind() ?? default;
	}

	/// <summary>
	/// Returns target of the specified <paramref name="attribute"/>.
	/// </summary>
	/// <param name="attribute"><see cref="AttributeData"/> to get the target of.</param>
	public static AttributeTarget GetTarget(this AttributeData attribute)
	{
		if (attribute.ApplicationSyntaxReference?.GetSyntax() is not AttributeSyntax node || node.Parent is not AttributeListSyntax list || list.Target is not AttributeTargetSpecifierSyntax target)
		{
			return AttributeTarget.None;
		}

		return target.Identifier.ValueText switch
		{
			"assembly" => AttributeTarget.Assembly,
			"return" => AttributeTarget.Return,
			"field" => AttributeTarget.Field,
			"event" => AttributeTarget.Event,
			"method" => AttributeTarget.Method,
			"type" => AttributeTarget.Type,
			"property" => AttributeTarget.Property,
			"param" => AttributeTarget.Param,
			"module" => AttributeTarget.Module,
			"typevar" => AttributeTarget.TypeVar,
			_ => AttributeTarget.None
		};
	}

	/// <summary>
	/// Checks if the target <paramref name="attribute"/> has a constructor argument at the specified <paramref name="position"/>. If so, also returns a <see cref="TypedConstant"/> that represents that argument.
	/// </summary>
	/// <param name="attribute"><see cref="AttributeData"/> to get the <see cref="TypedConstant"/> from.</param>
	/// <param name="position">Position where the target argument is to be found.</param>
	/// <param name="value">Returned <see cref="TypedConstant"/> that represents the argument with at the specified <paramref name="position"/>.</param>
	public static bool TryGetConstructorArgument(this AttributeData attribute, int position, out TypedConstant value)
	{
		ImmutableArray<TypedConstant> arguments = attribute.ConstructorArguments;

		if (arguments.Length == 0 || arguments.Length <= position)
		{
			value = default;
			return false;
		}

		value = arguments[position];
		return true;
	}

	/// <summary>
	/// Checks if the target <paramref name="attribute"/> defines a constructor argument at the specified <paramref name="position"/>. If so, also returns the <paramref name="values"/> of contained within the argument's array value.
	/// </summary>
	/// <param name="attribute"><see cref="AttributeData"/> to get the values from.</param>
	/// <param name="position">Position where the target argument is to be found.</param>
	/// <param name="values">Values contained within array value of the argument.</param>
	public static bool TryGetConstructorArgumentArrayValue(this AttributeData attribute, int position, out ImmutableArray<TypedConstant> values)
	{
		if (attribute.TryGetConstructorArgument(position, out TypedConstant constant))
		{
			if (constant.Values.IsDefault)
			{
				values = ImmutableArray.Create<TypedConstant>();
			}
			else
			{
				values = constant.Values;
			}

			return true;
		}

		values = ImmutableArray.Create<TypedConstant>();
		return false;
	}

	/// <summary>
	/// Checks if the target <paramref name="attribute"/> defines a constructor argument at the specified <paramref name="position"/>. If so, also returns the <paramref name="values"/> of contained within the argument's array value.
	/// </summary>
	/// <typeparam name="T">Type of array's elements.</typeparam>
	/// <param name="attribute"><see cref="AttributeData"/> to get the values from.</param>
	/// <param name="position">Position where the target argument is to be found.</param>
	/// <param name="values">Values contained within array value of the argument.</param>
	public static bool TryGetConstructorArgumentArrayValue<T>(this AttributeData attribute, int position, out ImmutableArray<T> values)
	{
		if (!attribute.TryGetConstructorArgumentArrayValue(position, out ImmutableArray<TypedConstant> constants))
		{
			values = ImmutableArray.Create<T>();
			return false;
		}

		int length = constants.Length;

		if (length == 0)
		{
			values = ImmutableArray.Create<T>();
			return true;
		}

		T[] array = new T[length];

		for (int i = 0; i < length; i++)
		{
			if (constants[i].Value is T t)
			{
				array[i] = t;
			}
			else
			{
				array[i] = default!;
			}
		}

		values = ImmutableArray.Create(array);
		return true;
	}

	/// <summary>
	/// Checks if the target <paramref name="attribute"/> defines a constructor argument at specified <paramref name="position"/>. If so, also returns the enum value of that argument.
	/// </summary>
	/// <typeparam name="TEnum">Type of enum value to return.</typeparam>
	/// <param name="attribute"><see cref="AttributeData"/> to get the value from.</param>
	/// <param name="position">Position where the target argument is to be found.</param>
	/// <param name="value">Returned enum value.</param>
	/// <exception cref="InvalidOperationException">Type size mismatch.</exception>
	public static bool TryGetConstructorArgumentEnumValue<TEnum>(this AttributeData attribute, int position, out TEnum value) where TEnum : unmanaged, Enum
	{
		return attribute.TryGetConstructorArgumentEnumValue<TEnum, int>(position, out value);
	}

	/// <summary>
	/// Checks if the target <paramref name="attribute"/> defines a constructor argument at specified <paramref name="position"/>. If so, also returns the enum value of that argument.
	/// </summary>
	/// <typeparam name="TEnum">Type of enum value to return.</typeparam>
	/// <typeparam name="TType">Type the <typeparamref name="TEnum"/> type extends.</typeparam>
	/// <param name="attribute"><see cref="AttributeData"/> to get the value from.</param>
	/// <param name="position">Position where the target argument is to be found.</param>
	/// <param name="value">Returned enum value.</param>
	/// <exception cref="InvalidOperationException">Type size mismatch.</exception>
	public static unsafe bool TryGetConstructorArgumentEnumValue<TEnum, TType>(this AttributeData attribute, int position, out TEnum value)
		where TEnum : unmanaged, Enum
		where TType : unmanaged
	{
		if (sizeof(TType) != sizeof(TEnum))
		{
			throw new InvalidOperationException($"Type size mismatch. TEnum is {sizeof(TEnum)}, while TType is {sizeof(TType)}");
		}

		if (attribute.TryGetConstructorArgumentValue(position, out TType n))
		{
			// For some reason this line causes assembly version conflicts.
			//value = Unsafe.As<TType, TEnum>(ref n);

			value = (TEnum)(object)n;
			return true;
		}

		value = default;
		return false;
	}

	/// <summary>
	/// Checks if the target <paramref name="attribute"/> defines a constructor argument at specified <paramref name="position"/>. If so, also returns the <paramref name="symbol"/> represented by value of that argument.
	/// </summary>
	/// <typeparam name="TType">Type of <see cref="ITypeSymbol"/> to return.</typeparam>
	/// <param name="attribute"><see cref="AttributeData"/> to get the value from.</param>
	/// <param name="position">Position where the target argument is to be found.</param>
	/// <param name="symbol">Symbol that represents the <see cref="Type"/> value of the argument.</param>
	public static bool TryGetConstructorArgumentTypeValue<TType>(this AttributeData attribute, int position, out TType? symbol) where TType : ITypeSymbol
	{
		if (attribute.TryGetConstructorArgument(position, out TypedConstant value))
		{
			if (value.Value is TType t)
			{
				symbol = t;
			}
			else
			{
				symbol = default;
			}

			return true;
		}

		symbol = default;
		return false;
	}

	/// <summary>
	/// Checks if the target <paramref name="attribute"/> has a constructor argument at the specified <paramref name="position"/>. If so, also returns the <paramref name="value"/> of that argument.
	/// </summary>
	/// <typeparam name="T">Type of value to return.</typeparam>
	/// <param name="attribute"><see cref="AttributeData"/> to get the <paramref name="value"/> from.</param>
	/// <param name="position">Position where the target argument is to be found.</param>
	/// <param name="value">Value of the argument.</param>
	public static bool TryGetConstructorArgumentValue<T>(this AttributeData attribute, int position, out T? value)
	{
		if (attribute.TryGetConstructorArgument(position, out TypedConstant arg))
		{
			if (arg.Value is T t)
			{
				value = t;
			}
			else
			{
				value = default;
			}

			return true;
		}

		value = default;
		return false;
	}

	/// <summary>
	/// Checks if the target <paramref name="attribute"/> defines a named argument with the specified <paramref name="argumentName"/>. If so, also returns a <see cref="TypedConstant"/> that represents that argument.
	/// </summary>
	/// <param name="attribute"><see cref="AttributeData"/> to get the <see cref="TypedConstant"/> from.</param>
	/// <param name="argumentName">Name of the argument to get the <see cref="TypedConstant"/> of.</param>
	/// <param name="value">Returned <see cref="TypedConstant"/> that represents the argument with the specified <paramref name="argumentName"/>.</param>
	public static bool TryGetNamedArgument(this AttributeData attribute, string argumentName, out TypedConstant value)
	{
		foreach (KeyValuePair<string, TypedConstant> arg in attribute.NamedArguments)
		{
			if (arg.Key == argumentName)
			{
				value = arg.Value;
				return true;
			}
		}

		value = default;
		return false;
	}

	/// <summary>
	/// Checks if the target <paramref name="attribute"/> defines a named argument with the specified <paramref name="argumentName"/>. If so, also returns the <paramref name="values"/> of contained within the argument's array value.
	/// </summary>
	/// <param name="attribute"><see cref="AttributeData"/> to get the values from.</param>
	/// <param name="argumentName">Name of the argument to get the values of.</param>
	/// <param name="values">Values contained within array value of the argument.</param>
	public static bool TryGetNamedArgumentArrayValue(this AttributeData attribute, string argumentName, out ImmutableArray<TypedConstant> values)
	{
		foreach (KeyValuePair<string, TypedConstant> arg in attribute.NamedArguments)
		{
			if (arg.Key == argumentName)
			{
				if (arg.Value.Values.IsDefault)
				{
					values = ImmutableArray.Create<TypedConstant>();
				}
				else
				{
					values = arg.Value.Values;
				}

				return true;
			}
		}

		values = ImmutableArray.Create<TypedConstant>();
		return false;
	}

	/// <summary>
	/// Checks if the target <paramref name="attribute"/> defines a named argument with the specified <paramref name="argumentName"/>. If so, also returns the <paramref name="values"/> of contained within the argument's array value.
	/// </summary>
	/// <typeparam name="T">Type of array's elements.</typeparam>
	/// <param name="attribute"><see cref="AttributeData"/> to get the values from.</param>
	/// <param name="argumentName">Name of the argument to get the values of.</param>
	/// <param name="values">Values contained within array value of the argument.</param>
	public static bool TryGetNamedArgumentArrayValue<T>(this AttributeData attribute, string argumentName, out ImmutableArray<T> values)
	{
		if (!attribute.TryGetNamedArgumentArrayValue(argumentName, out ImmutableArray<TypedConstant> constants))
		{
			values = ImmutableArray.Create<T>();
			return false;
		}

		int length = constants.Length;

		if (length == 0)
		{
			values = ImmutableArray.Create<T>();
			return true;
		}

		T[] array = new T[length];

		for (int i = 0; i < length; i++)
		{
			if (constants[i].Value is T t)
			{
				array[i] = t;
			}
			else
			{
				array[i] = default!;
			}
		}

		values = ImmutableArray.Create(array);
		return true;
	}

	/// <summary>
	/// Checks if the target <paramref name="attribute"/> defines a named argument with the specified <paramref name="argumentName"/>. If so, also returns the enum <paramref name="value"/> of that argument.
	/// </summary>
	/// <typeparam name="TEnum">Type of enum value to return.</typeparam>
	/// <param name="attribute"><see cref="AttributeData"/> to get the value from.</param>
	/// <param name="argumentName">Name of the argument to get the <paramref name="value"/> of.</param>
	/// <param name="value">Returned enum value.</param>
	/// <exception cref="InvalidOperationException">Type size mismatch.</exception>
	public static bool TryGetNamedArgumentEnumValue<TEnum>(this AttributeData attribute, string argumentName, out TEnum value) where TEnum : unmanaged, Enum
	{
		return attribute.TryGetNamedArgumentEnumValue<TEnum, int>(argumentName, out value);
	}

	/// <summary>
	/// Checks if the target <paramref name="attribute"/> defines a named argument with the specified <paramref name="argumentName"/>. If so, also returns the enum <paramref name="value"/> of that argument.
	/// </summary>
	/// <typeparam name="TEnum">Type of enum value to return.</typeparam>
	/// <typeparam name="TType">Type the <typeparamref name="TEnum"/> type extends.</typeparam>
	/// <param name="attribute"><see cref="AttributeData"/> to get the value from.</param>
	/// <param name="argumentName">Name of the argument to get the <paramref name="value"/> of.</param>
	/// <param name="value">Returned enum value.</param>
	/// <exception cref="InvalidOperationException">Type size mismatch.</exception>
	public static unsafe bool TryGetNamedArgumentEnumValue<TEnum, TType>(this AttributeData attribute, string argumentName, out TEnum value)
		where TEnum : unmanaged, Enum
		where TType : unmanaged
	{
		if (sizeof(TType) != sizeof(TEnum))
		{
			throw new InvalidOperationException($"Type size mismatch. TEnum is {sizeof(TEnum)}, while TType is {sizeof(TType)}");
		}

		if (attribute.TryGetNamedArgumentValue(argumentName, out TType n))
		{
			// For some reason this line causes assembly version conflicts.
			//value = Unsafe.As<TType, TEnum>(ref n);

			value = (TEnum)(object)n;
			return true;
		}

		value = default;
		return false;
	}

	/// <summary>
	/// Checks if the target <paramref name="attribute"/> defines a named argument with the specified <paramref name="argumentName"/>. If so, also returns the <paramref name="symbol"/> represented by value of that argument.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="ITypeSymbol"/> to return.</typeparam>
	/// <param name="attribute"><see cref="AttributeData"/> to get the value from.</param>
	/// <param name="argumentName">Name of the argument to get the value of.</param>
	/// <param name="symbol">Symbol that represents the <see cref="Type"/> value of the argument.</param>
	public static bool TryGetNamedArgumentTypeValue<T>(this AttributeData attribute, string argumentName, out T? symbol) where T : ITypeSymbol
	{
		foreach (KeyValuePair<string, TypedConstant> arg in attribute.NamedArguments)
		{
			if (arg.Key == argumentName)
			{
				if (arg.Value.Value is T t)
				{
					symbol = t;
				}
				else
				{
					symbol = default;
				}

				return true;
			}
		}

		symbol = default;
		return false;
	}

	/// <summary>
	/// Checks if the target <paramref name="attribute"/> defines a named argument with the specified <paramref name="argumentName"/>. If so, also returns the <paramref name="value"/> of that argument.
	/// </summary>
	/// <typeparam name="T">Type of value to return.</typeparam>
	/// <param name="attribute"><see cref="AttributeData"/> to get the <paramref name="value"/> from.</param>
	/// <param name="argumentName">Name of the argument to get the <paramref name="value"/> of.</param>
	/// <param name="value">Value of the argument.</param>
	public static bool TryGetNamedArgumentValue<T>(this AttributeData attribute, string argumentName, out T? value)
	{
		foreach (KeyValuePair<string, TypedConstant> arg in attribute.NamedArguments)
		{
			if (arg.Key == argumentName)
			{
				if (arg.Value.Value is T t)
				{
					value = t;
				}
				else
				{
					value = default!;
				}

				return true;
			}
		}

		value = default!;
		return false;
	}
}
