using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Durian.Extensions
{
	/// <summary>
	/// Contains various extension methods for the <see cref="AttributeData"/> class.
	/// </summary>
	public static class AttributeDataExtensions
	{
		/// <summary>
		/// Checks if the target <paramref name="attribute"/> defines a named argument with the specified <paramref name="argumentName"/>. If so, also returns a <see cref="TypedConstant"/> that represents that argument.
		/// </summary>
		/// <param name="attribute"><see cref="AttributeData"/> to get the <see cref="TypedConstant"/> from.</param>
		/// <param name="argumentName">Name of the argument to get the <see cref="TypedConstant"/> of.</param>
		/// <param name="value">Returned <see cref="TypedConstant"/> that represents the argument with the specified <paramref name="argumentName"/>.</param>
		public static bool TryGetNamedArgument(this AttributeData attribute, string argumentName, out TypedConstant value)
		{
			TypedConstant? arg = attribute?.NamedArguments.FirstOrDefault(arg => arg.Key == argumentName).Value;

			if (arg is not null)
			{
				value = arg.Value;
				return true;
			}

			value = default;
			return false;
		}

		/// <summary>
		/// Returns a <see cref="TypedConstant"/> representing the named argument with the specified <paramref name="argumentName"/>.
		/// </summary>
		/// <param name="attribute"><see cref="AttributeData"/> to get the <see cref="TypedConstant"/> from.</param>
		/// <param name="argumentName">Name of the argument to get the <see cref="TypedConstant"/> of.</param>
		public static TypedConstant GetNamedArgument(this AttributeData attribute, string argumentName)
		{
			TryGetNamedArgument(attribute, argumentName, out TypedConstant value);
			return value;
		}

		/// <summary>
		/// Checks if the target <paramref name="attribute"/> defines a named argument with the specified <paramref name="argumentName"/>. If so, also returns the <paramref name="value"/> of that argument.
		/// </summary>
		/// <typeparam name="T">Type of value to return.</typeparam>
		/// <param name="attribute"><see cref="AttributeData"/> to get the <paramref name="value"/> from.</param>
		/// <param name="argumentName">Name of the argument to get the <paramref name="value"/> of.</param>
		/// <param name="value">Value of the argument.</param>
		public static bool TryGetNamedArgumentValue<T>(this AttributeData attribute, string argumentName, out T value)
		{
			object? obj = attribute?.NamedArguments.FirstOrDefault(arg => arg.Key == argumentName).Value.Value;

			if (obj is not null and T t)
			{
				value = t;
				return true;
			}

			value = default!;
			return false;
		}

		/// <summary>
		/// Returns the value of the named argument with the specified <paramref name="argumentName"/>.
		/// </summary>
		/// <typeparam name="T">Type of value to return.</typeparam>
		/// <param name="attribute"><see cref="AttributeData"/> to get the value from.</param>
		/// <param name="argumentName">Name of the argument to get the value of.</param>
		public static T GetNamedArgumentValue<T>(this AttributeData attribute, string argumentName)
		{
			TryGetNamedArgumentValue(attribute, argumentName, out T value);
			return value;
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
			ITypeSymbol? obj = attribute?.NamedArguments.FirstOrDefault(arg => arg.Key == argumentName).Value.Type;

			if (obj is not null and T t)
			{
				symbol = t;
				return true;
			}

			symbol = default;
			return false;
		}

		/// <summary>
		/// Returns a <see cref="ITypeSymbol"/> representing the <see cref="Type"/> value of the named argument with the specified <paramref name="argumentName"/>.
		/// </summary>
		/// <typeparam name="T">Type of <see cref="ITypeSymbol"/> to return.</typeparam>
		/// <param name="attribute"><see cref="AttributeData"/> to get the value from.</param>
		/// <param name="argumentName">Name of the argument to get the value of.</param>
		public static T? GetNamedArgumentTypeValue<T>(this AttributeData attribute, string argumentName) where T : ITypeSymbol
		{
			TryGetNamedArgumentTypeValue(attribute, argumentName, out T? symbol);
			return symbol;
		}

		/// <summary>
		/// Checks if the target <paramref name="attribute"/> defines a named argument with the specified <paramref name="argumentName"/>. If so, also returns the <paramref name="values"/> of contained within the argument's array value.
		/// </summary>
		/// <param name="attribute"><see cref="AttributeData"/> to get the values from.</param>
		/// <param name="argumentName">Name of the argument to get the values of.</param>
		/// <param name="values">Values contained within array value of the argument.</param>
		public static bool TryGetNamedArgumentArrayValue(this AttributeData attribute, string argumentName, out ImmutableArray<TypedConstant> values)
		{
			ImmutableArray<TypedConstant>? array = attribute?.NamedArguments.FirstOrDefault(arg => arg.Key == argumentName).Value.Values;

			if (array is not null)
			{
				values = array.Value;
				return true;
			}

			values = ImmutableArray.Create<TypedConstant>();
			return false;
		}

		/// <summary>
		/// Returns an <see cref="ImmutableArray"/> of <see cref="TypedConstant"/>s representing the <see cref="Array"/> value of the named argument with the specified <paramref name="argumentName"/>.
		/// </summary>
		/// <param name="attribute"><see cref="AttributeData"/> to get the values from.</param>
		/// <param name="argumentName">Name of the argument to get the values of.</param>
		public static ImmutableArray<TypedConstant> GetNamedArgumentArrayValue(this AttributeData attribute, string argumentName)
		{
			TryGetNamedArgumentArrayValue(attribute, argumentName, out ImmutableArray<TypedConstant> array);
			return array;
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
		/// Returns an <see cref="ImmutableArray"/> of <see cref="TypedConstant"/>s representing the <see cref="Array"/> value of the named argument with the specified <paramref name="argumentName"/>.
		/// </summary>
		/// <typeparam name="T">Type of array's elements.</typeparam>
		/// <param name="attribute"><see cref="AttributeData"/> to get the values from.</param>
		/// <param name="argumentName">Name of the argument to get the values of.</param>
		public static ImmutableArray<T> GetNamedArgumentArrayValue<T>(this AttributeData attribute, string argumentName)
		{
			TryGetNamedArgumentArrayValue(attribute, argumentName, out ImmutableArray<T> array);
			return array;
		}
	}
}
