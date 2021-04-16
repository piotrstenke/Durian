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
		/// Returns a <see cref="TypedConstant"/> representing the named argument with the specified <paramref name="argumentName"/>.
		/// </summary>
		/// <param name="attribute"><see cref="AttributeData"/> to get the <see cref="TypedConstant"/> from.</param>
		/// <param name="argumentName">Name of the argument to get the <see cref="TypedConstant"/> of.</param>
		public static TypedConstant GetNamedArgument(this AttributeData attribute, string argumentName)
		{
			return attribute?.NamedArguments.FirstOrDefault(arg => arg.Key == argumentName).Value ?? default;
		}

		/// <summary>
		/// Returns the value of the named argument with the specified <paramref name="argumentName"/>.
		/// </summary>
		/// <typeparam name="T">Type of value to return.</typeparam>
		/// <param name="attribute"><see cref="AttributeData"/> to get the value from.</param>
		/// <param name="argumentName">Name of the argument to get the value of.</param>
		public static T GetNamedArgumentValue<T>(this AttributeData attribute, string argumentName)
		{
			if (attribute?.NamedArguments.FirstOrDefault(arg => arg.Key == argumentName).Value.Value is T t)
			{
				return t;
			}

			return default!;
		}

		/// <summary>
		/// Returns a <see cref="ITypeSymbol"/> representing the <see cref="Type"/> value of the named argument with the specified <paramref name="argumentName"/>.
		/// </summary>
		/// <typeparam name="T">Type of value to return.</typeparam>
		/// <param name="attribute"><see cref="AttributeData"/> to get the value from.</param>
		/// <param name="argumentName">Name of the argument to get the value of.</param>
		public static T? GetNamedArgumentTypeValue<T>(this AttributeData attribute, string argumentName) where T : ITypeSymbol
		{
			if (attribute?.NamedArguments.FirstOrDefault(arg => arg.Key == argumentName).Value.Type is T t)
			{
				return t;
			}

			return default;
		}

		/// <summary>
		/// Returns an <see cref="ImmutableArray"/> of <see cref="TypedConstant"/>s representing the <see cref="Array"/> value of the named argument with the specified <paramref name="argumentName"/>.
		/// </summary>
		/// <param name="attribute"><see cref="AttributeData"/> to get the value from.</param>
		/// <param name="argumentName">Name of the argument to get the value of.</param>
		public static ImmutableArray<TypedConstant> GetNamedArgumentArrayValue(this AttributeData attribute, string argumentName)
		{
			return attribute?.NamedArguments.FirstOrDefault(arg => arg.Key == argumentName).Value.Values ?? ImmutableArray.Create<TypedConstant>();
		}

		/// <summary>
		/// Returns an <see cref="ImmutableArray"/> of <see cref="TypedConstant"/>s representing the <see cref="Array"/> value of the named argument with the specified <paramref name="argumentName"/>.
		/// </summary>
		/// <typeparam name="T">Type of array's elements.</typeparam>
		/// <param name="attribute"><see cref="AttributeData"/> to get the value from.</param>
		/// <param name="argumentName">Name of the argument to get the value of.</param>
		public static ImmutableArray<T> GetNamedArgumentArrayValue<T>(this AttributeData attribute, string argumentName)
		{
			ImmutableArray<TypedConstant> constants = GetNamedArgumentArrayValue(attribute, argumentName);

			int length = constants.Length;

			if (length == 0)
			{
				return ImmutableArray.Create<T>();
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

			return ImmutableArray.Create(array);
		}
	}
}
