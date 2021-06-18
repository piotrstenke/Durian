// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Extensions
{
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
		/// <exception cref="ArgumentNullException"><paramref name="attribute"/> is <see langword="null"/>.</exception>
		public static TypedConstant GetConstructorArgument(this AttributeData attribute, int position)
		{
			TryGetConstructorArgument(attribute, position, out TypedConstant value);
			return value;
		}

		/// <summary>
		/// Returns an <see cref="ImmutableArray"/> of <see cref="TypedConstant"/>s representing the <see cref="Array"/> value of the constructor argument at the specified <paramref name="position"/>.
		/// </summary>
		/// <param name="attribute"><see cref="AttributeData"/> to get the values from.</param>
		/// <param name="position">Position where the target argument is to be found.</param>
		/// <exception cref="ArgumentNullException"><paramref name="attribute"/> is <see langword="null"/>.</exception>
		public static ImmutableArray<TypedConstant> GetConstructorArgumentArrayValue(this AttributeData attribute, int position)
		{
			TryGetConstructorArgumentArrayValue(attribute, position, out ImmutableArray<TypedConstant> array);
			return array;
		}

		/// <summary>
		/// Returns an <see cref="ImmutableArray"/> of <see cref="TypedConstant"/>s representing the <see cref="Array"/> value of the constructor argument at the specified <paramref name="position"/>.
		/// </summary>
		/// <typeparam name="T">Type of array's elements.</typeparam>
		/// <param name="attribute"><see cref="AttributeData"/> to get the values from.</param>
		/// <param name="position">Position where the target argument is to be found.</param>
		/// <exception cref="ArgumentNullException"><paramref name="attribute"/> is <see langword="null"/>.</exception>
		public static ImmutableArray<T> GetConstructorArgumentArrayValue<T>(this AttributeData attribute, int position)
		{
			TryGetConstructorArgumentArrayValue(attribute, position, out ImmutableArray<T> array);
			return array;
		}

		/// <summary>
		/// Returns a <see cref="ITypeSymbol"/> representing the <see cref="Type"/> value of the constructor argument at the specified <paramref name="position"/>.
		/// </summary>
		/// <typeparam name="T">Type of <see cref="ITypeSymbol"/> to return.</typeparam>
		/// <param name="attribute"><see cref="AttributeData"/> to get the value from.</param>
		/// <param name="position">Position where the target argument is to be found.</param>
		/// <exception cref="ArgumentNullException"><paramref name="attribute"/> is <see langword="null"/>.</exception>
		public static T? GetConstructorArgumentTypeValue<T>(this AttributeData attribute, int position) where T : ITypeSymbol
		{
			TryGetConstructorArgumentTypeValue(attribute, position, out T? symbol);
			return symbol;
		}

		/// <summary>
		/// Returns the value of the constructor argument at the specified <paramref name="position"/>.
		/// </summary>
		/// <typeparam name="T">Type of value to return.</typeparam>
		/// <param name="attribute"><see cref="AttributeData"/> to get the value from.</param>
		/// <param name="position">Position where the target argument is to be found.</param>
		/// <exception cref="ArgumentNullException"><paramref name="attribute"/> is <see langword="null"/>.</exception>
		public static T? GetConstructorArgumentValue<T>(this AttributeData attribute, int position)
		{
			TryGetConstructorArgumentValue(attribute, position, out T? value);
			return value;
		}

		/// <summary>
		/// Returns a <see cref="TypedConstant"/> representing the named argument with the specified <paramref name="argumentName"/>.
		/// </summary>
		/// <param name="attribute"><see cref="AttributeData"/> to get the <see cref="TypedConstant"/> from.</param>
		/// <param name="argumentName">Name of the argument to get the <see cref="TypedConstant"/> of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="attribute"/> is <see langword="null"/>.</exception>
		public static TypedConstant GetNamedArgument(this AttributeData attribute, string argumentName)
		{
			TryGetNamedArgument(attribute, argumentName, out TypedConstant value);
			return value;
		}

		/// <summary>
		/// Returns an <see cref="ImmutableArray"/> of <see cref="TypedConstant"/>s representing the <see cref="Array"/> value of the named argument with the specified <paramref name="argumentName"/>.
		/// </summary>
		/// <param name="attribute"><see cref="AttributeData"/> to get the values from.</param>
		/// <param name="argumentName">Name of the argument to get the values of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="attribute"/> is <see langword="null"/>.</exception>
		public static ImmutableArray<TypedConstant> GetNamedArgumentArrayValue(this AttributeData attribute, string argumentName)
		{
			TryGetNamedArgumentArrayValue(attribute, argumentName, out ImmutableArray<TypedConstant> array);
			return array;
		}

		/// <summary>
		/// Returns an <see cref="ImmutableArray"/> of <see cref="TypedConstant"/>s representing the <see cref="Array"/> value of the named argument with the specified <paramref name="argumentName"/>.
		/// </summary>
		/// <typeparam name="T">Type of array's elements.</typeparam>
		/// <param name="attribute"><see cref="AttributeData"/> to get the values from.</param>
		/// <param name="argumentName">Name of the argument to get the values of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="attribute"/> is <see langword="null"/>.</exception>
		public static ImmutableArray<T> GetNamedArgumentArrayValue<T>(this AttributeData attribute, string argumentName)
		{
			TryGetNamedArgumentArrayValue(attribute, argumentName, out ImmutableArray<T> array);
			return array;
		}

		/// <summary>
		/// Returns a <see cref="ITypeSymbol"/> representing the <see cref="Type"/> value of the named argument with the specified <paramref name="argumentName"/>.
		/// </summary>
		/// <typeparam name="T">Type of <see cref="ITypeSymbol"/> to return.</typeparam>
		/// <param name="attribute"><see cref="AttributeData"/> to get the value from.</param>
		/// <param name="argumentName">Name of the argument to get the value of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="attribute"/> is <see langword="null"/>.</exception>
		public static T? GetNamedArgumentTypeValue<T>(this AttributeData attribute, string argumentName) where T : ITypeSymbol
		{
			TryGetNamedArgumentTypeValue<T>(attribute, argumentName, out T? symbol);
			return symbol;
		}

		/// <summary>
		/// Returns the value of the named argument with the specified <paramref name="argumentName"/>.
		/// </summary>
		/// <typeparam name="T">Type of value to return.</typeparam>
		/// <param name="attribute"><see cref="AttributeData"/> to get the value from.</param>
		/// <param name="argumentName">Name of the argument to get the value of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="attribute"/> is <see langword="null"/>.</exception>
		public static T? GetNamedArgumentValue<T>(this AttributeData attribute, string argumentName)
		{
			TryGetNamedArgumentValue(attribute, argumentName, out T? value);
			return value;
		}

		/// <summary>
		/// Checks if the target <paramref name="attribute"/> has a constructor argument at the specified <paramref name="position"/>. If so, also returns a <see cref="TypedConstant"/> that represents that argument.
		/// </summary>
		/// <param name="attribute"><see cref="AttributeData"/> to get the <see cref="TypedConstant"/> from.</param>
		/// <param name="position">Position where the target argument is to be found.</param>
		/// <param name="value">Returned <see cref="TypedConstant"/> that represents the argument with at the specified <paramref name="position"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="attribute"/> is <see langword="null"/>.</exception>
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
		/// <exception cref="ArgumentNullException"><paramref name="attribute"/> is <see langword="null"/>.</exception>
		public static bool TryGetConstructorArgumentArrayValue(this AttributeData attribute, int position, out ImmutableArray<TypedConstant> values)
		{
			if (attribute is null)
			{
				throw new ArgumentNullException(nameof(attribute));
			}

			if (TryGetConstructorArgument(attribute, position, out TypedConstant constant))
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
		/// <exception cref="ArgumentNullException"><paramref name="attribute"/> is <see langword="null"/>.</exception>
		public static bool TryGetConstructorArgumentArrayValue<T>(this AttributeData attribute, int position, out ImmutableArray<T> values)
		{
			if (!TryGetConstructorArgumentArrayValue(attribute, position, out ImmutableArray<TypedConstant> constants))
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
		/// Checks if the target <paramref name="attribute"/> defines a constructor argument at specified <paramref name="position"/>. If so, also returns the <paramref name="symbol"/> represented by value of that argument.
		/// </summary>
		/// <typeparam name="T">Type of <see cref="ITypeSymbol"/> to return.</typeparam>
		/// <param name="attribute"><see cref="AttributeData"/> to get the value from.</param>
		/// <param name="position">Position where the target argument is to be found.</param>
		/// <param name="symbol">Symbol that represents the <see cref="Type"/> value of the argument.</param>
		/// <exception cref="ArgumentNullException"><paramref name="attribute"/> is <see langword="null"/>.</exception>
		public static bool TryGetConstructorArgumentTypeValue<T>(this AttributeData attribute, int position, out T? symbol) where T : ITypeSymbol
		{
			if (attribute is null)
			{
				throw new ArgumentNullException(nameof(attribute));
			}

			if (TryGetConstructorArgument(attribute, position, out TypedConstant value))
			{
				if (value.Value is T t)
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
		/// <exception cref="ArgumentNullException"><paramref name="attribute"/> is <see langword="null"/>.</exception>
		public static bool TryGetConstructorArgumentValue<T>(this AttributeData attribute, int position, out T? value)
		{
			if (attribute is null)
			{
				throw new ArgumentNullException(nameof(attribute));
			}

			if (TryGetConstructorArgument(attribute, position, out TypedConstant arg))
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
		/// <exception cref="ArgumentNullException"><paramref name="attribute"/> is <see langword="null"/>.</exception>
		public static bool TryGetNamedArgument(this AttributeData attribute, string argumentName, out TypedConstant value)
		{
			if (attribute is null)
			{
				throw new ArgumentNullException(nameof(attribute));
			}

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
		/// <exception cref="ArgumentNullException"><paramref name="attribute"/> is <see langword="null"/>.</exception>
		public static bool TryGetNamedArgumentArrayValue(this AttributeData attribute, string argumentName, out ImmutableArray<TypedConstant> values)
		{
			if (attribute is null)
			{
				throw new ArgumentNullException(nameof(attribute));
			}

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
		/// <exception cref="ArgumentNullException"><paramref name="attribute"/> is <see langword="null"/>.</exception>
		public static bool TryGetNamedArgumentArrayValue<T>(this AttributeData attribute, string argumentName, out ImmutableArray<T> values)
		{
			if (!TryGetNamedArgumentArrayValue(attribute, argumentName, out ImmutableArray<TypedConstant> constants))
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
		/// Checks if the target <paramref name="attribute"/> defines a named argument with the specified <paramref name="argumentName"/>. If so, also returns the <paramref name="symbol"/> represented by value of that argument.
		/// </summary>
		/// <typeparam name="T">Type of <see cref="ITypeSymbol"/> to return.</typeparam>
		/// <param name="attribute"><see cref="AttributeData"/> to get the value from.</param>
		/// <param name="argumentName">Name of the argument to get the value of.</param>
		/// <param name="symbol">Symbol that represents the <see cref="Type"/> value of the argument.</param>
		/// <exception cref="ArgumentNullException"><paramref name="attribute"/> is <see langword="null"/>.</exception>
		public static bool TryGetNamedArgumentTypeValue<T>(this AttributeData attribute, string argumentName, out T? symbol) where T : ITypeSymbol
		{
			if (attribute is null)
			{
				throw new ArgumentNullException(nameof(attribute));
			}

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
		/// <exception cref="ArgumentNullException"><paramref name="attribute"/> is <see langword="null"/>.</exception>
		public static bool TryGetNamedArgumentValue<T>(this AttributeData attribute, string argumentName, out T? value)
		{
			if (attribute is null)
			{
				throw new ArgumentNullException(nameof(attribute));
			}

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
}
