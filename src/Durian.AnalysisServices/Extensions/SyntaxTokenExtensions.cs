// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;
using Durian.Analysis.CodeGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.Extensions
{
	/// <summary>
	/// Contains extension methods for the <see cref="SyntaxToken"/> and <see cref="SyntaxTokenList"/> structs.
	/// </summary>
	public static class SyntaxTokenExtensions
	{
		/// <summary>
		/// Returns the <see cref="Accessibility"/> of the specified <paramref name="token"/>.
		/// </summary>
		/// <param name="token"><see cref="SyntaxToken"/> to get the accessibility of.</param>
		public static Accessibility GetAccessibility(this SyntaxToken token)
		{
			return ((SyntaxKind)token.RawKind).GetAccessibility();
		}

		/// <summary>
		/// Returns the <see cref="Accessibility"/> of the specified <paramref name="tokenList"/>.
		/// </summary>
		/// <param name="tokenList"><see cref="SyntaxTokenList"/> to get the accessibility of.</param>
		public static Accessibility GetAccessibility(this SyntaxTokenList tokenList)
		{
			Accessibility current = default;

			foreach (SyntaxToken token in tokenList)
			{
				switch ((SyntaxKind)token.RawKind)
				{
					case SyntaxKind.PrivateKeyword:

						if (current == Accessibility.Protected)
						{
							return Accessibility.ProtectedAndInternal;
						}

						current = Accessibility.Private;
						break;

					case SyntaxKind.PublicKeyword:
						return Accessibility.Public;

					case SyntaxKind.ProtectedKeyword:

						if (current == Accessibility.Private)
						{
							return Accessibility.ProtectedAndInternal;
						}

						if (current == Accessibility.Internal)
						{
							return Accessibility.ProtectedOrInternal;
						}

						current = Accessibility.Protected;
						break;

					case SyntaxKind.InternalKeyword:

						if (current == Accessibility.Protected)
						{
							return Accessibility.ProtectedOrInternal;
						}

						current = Accessibility.Internal;

						break;
				}
			}

			return current;
		}

		/// <summary>
		/// Returns the <see cref="Accessor"/> represented by the specified <paramref name="token"/>.
		/// </summary>
		/// <param name="token"><see cref="SyntaxToken"/> to get the <see cref="Accessor"/> represented by.</param>
		public static Accessor GetAccessor(this SyntaxToken token)
		{
			return ((SyntaxKind)token.RawKind).GetAccessor();
		}

		/// <summary>
		/// Returns the <see cref="AttributeTarget"/> represented by the specified <paramref name="token"/>.
		/// </summary>
		/// <param name="token"><see cref="SyntaxToken"/> to get the <see cref="AttributeTarget"/> represented by.</param>
		public static AttributeTarget GetAttributeTarget(this SyntaxToken token)
		{
			return ((SyntaxKind)token.RawKind).GetAttributeTarget();
		}

		/// <summary>
		/// Returns the <see cref="GenericConstraint"/> represented by the specified <paramref name="token"/>.
		/// </summary>
		/// <param name="token"><see cref="SyntaxToken"/> to get the <see cref="GenericConstraint"/> represented by.</param>
		public static GenericConstraint GetConstraint(this SyntaxToken token)
		{
			GenericConstraint constraint = ((SyntaxKind)token.RawKind).GetConstraint();

			if (constraint == GenericConstraint.Type)
			{
				if (token.Text == "notnull")
				{
					return GenericConstraint.NotNull;
				}

				if (token.Text == "unmanaged")
				{
					return GenericConstraint.Unmanaged;
				}
			}

			return constraint;
		}

		/// <summary>
		/// Returns the <see cref="ConstructorInitializer"/> represented by the specified <paramref name="token"/>.
		/// </summary>
		/// <param name="token"><see cref="SyntaxToken"/> to get the <see cref="ConstructorInitializer"/> represented by.</param>
		public static ConstructorInitializer GetConstructorInitializer(this SyntaxToken token)
		{
			return ((SyntaxKind)token.RawKind).GetConstructorInitializer();
		}

		/// <summary>
		/// Returns the <see cref="DecimalLiteralSuffix"/> applied to the specified <paramref name="token"/>.
		/// </summary>
		/// <param name="token"><see cref="SyntaxToken"/> to get the <see cref="DecimalLiteralSuffix"/> applied to.</param>
		public static DecimalLiteralSuffix GetDecimalSuffix(this SyntaxToken token)
		{
			if (!token.IsKind(SyntaxKind.NumericLiteralToken) || token.Text.Length == 0)
			{
				return default;
			}

			char last = token.Text[token.Text.Length - 1];

			return last switch
			{
				'f' => DecimalLiteralSuffix.FloatLower,
				'F' => DecimalLiteralSuffix.FloatUpper,
				'd' => DecimalLiteralSuffix.DoubleLower,
				'D' => DecimalLiteralSuffix.DoubleUpper,
				'm' => DecimalLiteralSuffix.DecimalLower,
				'M' => DecimalLiteralSuffix.DecimalUpper,
				_ => default,
			};
		}

		/// <summary>
		/// Returns the <see cref="DecimalValueType"/> represented by the specified <paramref name="token"/>.
		/// </summary>
		/// <param name="token"><see cref="SyntaxToken"/> to get the <see cref="DecimalValueType"/> represented by.</param>
		public static DecimalValueType GetDecimalType(this SyntaxToken token)
		{
			return ((SyntaxKind)token.RawKind).GetDecimalType();
		}

		/// <summary>
		/// Returns the <see cref="EventAccessor"/> represented by the specified <paramref name="token"/>.
		/// </summary>
		/// <param name="token"><see cref="SyntaxToken"/> to get the <see cref="EventAccessor"/> represented by.</param>
		public static EventAccessor GetEventAccessor(this SyntaxToken token)
		{
			return ((SyntaxKind)token.RawKind).GetEventAccessor();
		}

		/// <summary>
		/// Returns the <see cref="ExponentialStyle"/> applied to the specified <paramref name="token"/>.
		/// </summary>
		/// <param name="token"><see cref="SyntaxToken"/> to get the <see cref="ExponentialStyle"/> applied to.</param>
		public static ExponentialStyle GetExponentialStyle(this SyntaxToken token)
		{
			if (!token.IsKind(SyntaxKind.NumericLiteralToken))
			{
				return default;
			}

			foreach (char c in token.Text)
			{
				if (c == 'e')
				{
					return ExponentialStyle.Lowercase;
				}

				if (c == 'E')
				{
					return ExponentialStyle.Uppercase;
				}
			}

			return default;
		}

		/// <summary>
		/// Returns the <see cref="IntegerValueType"/> represented by the specified <paramref name="token"/>.
		/// </summary>
		/// <param name="token"><see cref="SyntaxToken"/> to get the <see cref="IntegerValueType"/> represented by.</param>
		public static IntegerValueType GetIntegerType(this SyntaxToken token)
		{
			return ((SyntaxKind)token.RawKind).GetIntegerType();
		}

		/// <summary>
		/// Returns the literal value of type <typeparamref name="T"/> the <paramref name="token"/> represents.
		/// </summary>
		/// <typeparam name="T">Type of literal value this <paramref name="token"/> represents.</typeparam>
		/// <param name="token"><see cref="SyntaxToken"/> to get the literal value of.</param>
		public static T GetLiteralValue<T>(this SyntaxToken token) where T : unmanaged
		{
			return token.Value is T value ? value : default;
		}

		/// <summary>
		/// Returns the <see cref="NamespaceStyle"/> applied to the specified <paramref name="token"/>.
		/// </summary>
		/// <param name="token"><see cref="SyntaxToken"/> to get the <see cref="NamespaceStyle"/> applied to.</param>
		public static NamespaceStyle GetNamespaceStyle(this SyntaxToken token)
		{
			return ((SyntaxKind)token.RawKind).GetNamespaceStyle();
		}

		/// <summary>
		/// Returns the <see cref="NumericLiteralPrefix"/> applied to the specified <paramref name="token"/>.
		/// </summary>
		/// <param name="token"><see cref="SyntaxToken"/> to get the <see cref="NumericLiteralPrefix"/> applied to.</param>
		public static NumericLiteralPrefix GetNumericPrefix(this SyntaxToken token)
		{
			if (!token.IsKind(SyntaxKind.NumericLiteralToken) || token.Text.Length < 2 || token.Text[0] != '0')
			{
				return default;
			}

			return token.Text[1] switch
			{
				'x' => NumericLiteralPrefix.HexadecimalLower,
				'X' => NumericLiteralPrefix.HexadecimalUpper,
				'b' => NumericLiteralPrefix.BinaryLower,
				'B' => NumericLiteralPrefix.BinaryUpper,
				_ => default
			};
		}

		/// <summary>
		/// Returns the <see cref="NumericLiteralSuffix"/> applied to the specified <paramref name="token"/>.
		/// </summary>
		/// <param name="token"><see cref="SyntaxToken"/> to get the <see cref="NumericLiteralSuffix"/> applied to.</param>
		public static NumericLiteralSuffix GetNumericSuffix(this SyntaxToken token)
		{
			if (!token.IsKind(SyntaxKind.NumericLiteralToken) || token.Text.Length == 0)
			{
				return default;
			}

			char last = token.Text[token.Text.Length - 1];

			return last switch
			{
				'u' => GetNextChar() switch
				{
					'l' => NumericLiteralSuffix.UnsignedLowerLongLower,
					'L' => NumericLiteralSuffix.UnsignedLowerLongUpper,
					_ => NumericLiteralSuffix.UnsignedLower
				},
				'U' => GetNextChar() switch
				{
					'l' => NumericLiteralSuffix.UnsignedUpperLongLower,
					'L' => NumericLiteralSuffix.UnsignedUpperLongUpper,
					_ => NumericLiteralSuffix.UnsignedUpper
				},
				'l' => GetNextChar() switch
				{
					'u' => NumericLiteralSuffix.LongLowerUnsignedLower,
					'U' => NumericLiteralSuffix.LongLowerUnsignedUpper,
					_ => NumericLiteralSuffix.LongLower
				},
				'L' => GetNextChar() switch
				{
					'u' => NumericLiteralSuffix.LongUpperUnsignedLower,
					'U' => NumericLiteralSuffix.LongUpperUnsignedUpper,
					_ => NumericLiteralSuffix.LongUpper
				},
				'f' => NumericLiteralSuffix.FloatLower,
				'F' => NumericLiteralSuffix.FloatUpper,
				'd' => NumericLiteralSuffix.DoubleLower,
				'D' => NumericLiteralSuffix.DoubleUpper,
				'm' => NumericLiteralSuffix.DecimalLower,
				'M' => NumericLiteralSuffix.DecimalUpper,
				_ => default,
			};

			char GetNextChar()
			{
				if (token.Text.Length < 2)
				{
					return char.MinValue;
				}

				return token.Text[token.Text.Length - 2];
			}
		}

		/// <summary>
		/// Returns the <see cref="OverloadableOperator"/> represented by the specified <paramref name="token"/>.
		/// </summary>
		/// <param name="token"><see cref="SyntaxToken"/> to get the <see cref="OverloadableOperator"/> represented by.</param>
		public static OverloadableOperator GetOperator(this SyntaxToken token)
		{
			return (SyntaxKind)token.RawKind switch
			{
				SyntaxKind.PlusToken or
				SyntaxKind.PlusEqualsToken
					=> OverloadableOperator.Addition,

				SyntaxKind.MinusToken or
				SyntaxKind.MinusEqualsToken
					=> OverloadableOperator.Subtraction,

				SyntaxKind.AsteriskToken or
				SyntaxKind.AsteriskEqualsToken
					=> OverloadableOperator.Multiplication,

				SyntaxKind.SlashToken or
				SyntaxKind.SlashEqualsToken
					=> OverloadableOperator.Division,

				SyntaxKind.PercentToken or
				SyntaxKind.PercentEqualsToken
					=> OverloadableOperator.Remainder,

				SyntaxKind.PlusPlusToken
					=> OverloadableOperator.Increment,

				SyntaxKind.MinusMinusToken
					=> OverloadableOperator.Decrement,

				SyntaxKind.EqualsEqualsToken
					=> OverloadableOperator.Equality,

				SyntaxKind.ExclamationEqualsToken
					=> OverloadableOperator.Inequality,

				SyntaxKind.CaretToken or
				SyntaxKind.CaretEqualsToken
					=> OverloadableOperator.LogicalXor,

				SyntaxKind.AmpersandToken or
				SyntaxKind.AmpersandEqualsToken
					=> OverloadableOperator.LogicalAnd,

				SyntaxKind.BarToken or
				SyntaxKind.BarEqualsToken
					=> OverloadableOperator.LogicalOr,

				SyntaxKind.ExclamationToken
					=> OverloadableOperator.Negation,

				SyntaxKind.TrueKeyword
					=> OverloadableOperator.True,

				SyntaxKind.FalseKeyword
					=> OverloadableOperator.False,

				SyntaxKind.TildeToken
					=> OverloadableOperator.Complement,

				SyntaxKind.GreaterThanToken
					=> OverloadableOperator.GreaterThan,

				SyntaxKind.GreaterThanEqualsToken
					=> OverloadableOperator.GreaterThanOrEqual,

				SyntaxKind.LessThanToken
					=> OverloadableOperator.LessThan,

				SyntaxKind.LessThanEqualsToken
					=> OverloadableOperator.LessThanOrEqual,

				SyntaxKind.GreaterThanGreaterThanToken or
				SyntaxKind.GreaterThanGreaterThanEqualsToken
					=> OverloadableOperator.RightShift,

				SyntaxKind.LessThanLessThanToken or
				SyntaxKind.LessThanLessThanToken
					=> OverloadableOperator.LeftShift,

				_ => default
			};
		}

		/// <summary>
		/// Returns the <see cref="PropertyAccessor"/> represented by the specified <paramref name="token"/>.
		/// </summary>
		/// <param name="token"><see cref="SyntaxToken"/> to get the <see cref="PropertyAccessor"/> represented by.</param>
		public static PropertyAccessor GetPropertyAccessor(this SyntaxToken token)
		{
			return ((SyntaxKind)token.RawKind).GetPropertyAccessor();
		}

		/// <summary>
		/// Returns the <see cref="RefKind"/> represented by the specified <paramref name="token"/>.
		/// </summary>
		/// <param name="token"><see cref="SyntaxToken"/> to get the <see cref="RefKind"/> represented by.</param>
		public static RefKind GetRefKind(this SyntaxToken token)
		{
			return ((SyntaxKind)token.RawKind).GetRefKind();
		}

		/// <summary>
		/// Returns the <see cref="StringModifiers"/> applied to the specified <paramref name="token"/>.
		/// </summary>
		/// <param name="token"><see cref="SyntaxToken"/> to get the <see cref="StringModifiers"/> applied to.</param>
		public static StringModifiers GetStringModifiers(this SyntaxToken token)
		{
			if (!token.IsKind(SyntaxKind.StringLiteralToken) || token.Text.Length == 0)
			{
				return default;
			}

			string text = token.Text;

			StringModifiers modifiers = default;

			int length = token.Text.Length == 1 ? 1 : 2;

			for (int i = 0; i < length; i++)
			{
				if (text[i] == '@')
				{
					modifiers |= StringModifiers.Verbatim;
				}
				else if (text[i] == '$')
				{
					modifiers |= StringModifiers.Interpolation;
				}
			}

			return modifiers;
		}

		/// <summary>
		/// Returns the <see cref="TypeKeyword"/> represented by the specified <paramref name="token"/>.
		/// </summary>
		/// <param name="token"><see cref="SyntaxToken"/> to get the <see cref="TypeKeyword"/> represented by.</param>
		public static TypeKeyword GetTypeKeyword(this SyntaxToken token)
		{
			return ((SyntaxKind)token.RawKind).GetTypeKeyword();
		}

		/// <summary>
		/// Determines whether the specified <paramref name="tokenlist"/> contains the <see langword="abstract"/> token.
		/// </summary>
		/// <param name="tokenlist">Determines whether the specified <paramref name="tokenlist"/> contains the <see langword="abstract"/> token.</param>
		public static bool IsAbstract(this SyntaxTokenList tokenlist)
		{
			return tokenlist.Any(SyntaxKind.AbstractKeyword);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="tokenList"/> contains the <see langword="async"/> token.
		/// </summary>
		/// <param name="tokenList">Determines whether the specified <paramref name="tokenList"/> contains the <see langword="async"/> token.</param>
#pragma warning disable RCS1047 // Non-asynchronous method name should not end with 'Async'.

		public static bool IsAsync(this SyntaxTokenList tokenList)
#pragma warning restore RCS1047 // Non-asynchronous method name should not end with 'Async'.
		{
			return tokenList.Any(SyntaxKind.AsyncKeyword);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="tokenList"/> contains the <see langword="extern"/> modifier.
		/// </summary>
		/// <param name="tokenList">Determines whether the specified <paramref name="tokenList"/> contains the <see langword="extern"/> modifier.</param>
		public static bool IsExtern(this SyntaxTokenList tokenList)
		{
			return tokenList.Any(SyntaxKind.ExternKeyword);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="tokenList"/> contains the <see langword="fixed"/> modifier.
		/// </summary>
		/// <param name="tokenList">Determines whether the specified <paramref name="tokenList"/> contains the <see langword="fixed"/> modifier.</param>
		public static bool IsFixed(this SyntaxTokenList tokenList)
		{
			return tokenList.Any(SyntaxKind.FixedKeyword);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="tokenList"/> contains the <see langword="in"/> modifier.
		/// </summary>
		/// <param name="tokenList">Determines whether the specified <paramref name="tokenList"/> contains the <see langword="in"/> modifier.</param>
		public static bool IsIn(this SyntaxTokenList tokenList)
		{
			return tokenList.Any(SyntaxKind.InKeyword);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="tokenList"/> contains the <see langword="new"/> token.
		/// </summary>
		/// <param name="tokenList">Determines whether the specified <paramref name="tokenList"/> contains the <see langword="new"/> token.</param>
		public static bool IsNew(this SyntaxTokenList tokenList)
		{
			return tokenList.Any(SyntaxKind.NewKeyword);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="tokenList"/> contains the <see langword="override"/> modifier.
		/// </summary>
		/// <param name="tokenList">Determines whether the specified <paramref name="tokenList"/> contains the <see langword="override"/> modifier.</param>
		public static bool IsOverride(this SyntaxTokenList tokenList)
		{
			return tokenList.Any(SyntaxKind.OverrideKeyword);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="tokenList"/> contains the <see langword="params"/> modifier.
		/// </summary>
		/// <param name="tokenList">Determines whether the specified <paramref name="tokenList"/> contains the <see langword="params"/> modifier.</param>
		public static bool IsParams(this SyntaxTokenList tokenList)
		{
			return tokenList.Any(SyntaxKind.ParamsKeyword);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="tokenList"/> contains the <see langword="partial"/> token.
		/// </summary>
		/// <param name="tokenList">Determines whether the specified <paramref name="tokenList"/> contains the <see langword="parital"/> token.</param>
		public static bool IsPartial(this SyntaxTokenList tokenList)
		{
			return tokenList.Any(SyntaxKind.PartialKeyword);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="tokenList"/> contains the <see langword="readonly"/> modifier (<see langword="ref"/> <see langword="readonly"/> does not count).
		/// </summary>
		/// <param name="tokenList">Determines whether the specified <paramref name="tokenList"/> contains the <see langword="readonly"/> or <see langword="in"/>  modifier.</param>
		public static bool IsReadOnly(this SyntaxTokenList tokenList)
		{
			SyntaxTokenList tokens = tokenList;
			int index = tokens.IndexOf(SyntaxKind.ReadOnlyKeyword);

			if (index == -1)
			{
				return false;
			}

			if (index == 0)
			{
				return true;
			}

			return !tokens[index - 1].IsKind(SyntaxKind.RefKeyword);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="tokenList"/> contains the <see langword="ref"/> modifier.
		/// </summary>
		/// <param name="tokenList">Determines whether the specified <paramref name="tokenList"/> contains the <see langword="ref"/> modifier.</param>
		public static bool IsRef(this SyntaxTokenList tokenList)
		{
			return tokenList.Any(SyntaxKind.RefKeyword);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="tokenList"/> contains the <see langword="ref"/> and <see langword="readonly"/> modifier.
		/// </summary>
		/// <param name="tokenList">Determines whether the specified <paramref name="tokenList"/> contains the <see langword="ref"/> and <see langword="readonly"/> modifier.</param>
		public static bool IsRefReadOnly(this SyntaxTokenList tokenList)
		{
			SyntaxTokenList tokens = tokenList;

			return tokens.Any(SyntaxKind.RefKeyword) && tokens.Any(SyntaxKind.ReadOnlyKeyword);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="tokenList"/> contains the <see langword="sealed"/> modifier.
		/// </summary>
		/// <param name="tokenList">Determines whether the specified <paramref name="tokenList"/> contains the <see langword="sealed"/> modifier.</param>
		public static bool IsSealed(this SyntaxTokenList tokenList)
		{
			return tokenList.Any(SyntaxKind.SealedKeyword);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="tokenList"/> contains the <see langword="static"/> modifier.
		/// </summary>
		/// <param name="tokenList">Determines whether the specified <paramref name="tokenList"/> contains the <see langword="static"/> modifier.</param>
		public static bool IsStatic(this SyntaxTokenList tokenList)
		{
			return tokenList.Any(SyntaxKind.StaticKeyword);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="tokenList"/> contains the <see langword="this"/> modifier.
		/// </summary>
		/// <param name="tokenList">Determines whether the specified <paramref name="tokenList"/> contains the <see langword="this"/> modifier.</param>
		public static bool IsThis(this SyntaxTokenList tokenList)
		{
			return tokenList.Any(SyntaxKind.ThisKeyword);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="tokenList"/> contains the <see langword="unsafe"/> modifier.
		/// </summary>
		/// <param name="tokenList">Determines whether the specified <paramref name="tokenList"/> contains the <see langword="unsafe"/> modifier.</param>
		public static bool IsUnsafe(this SyntaxTokenList tokenList)
		{
			return tokenList.Any(SyntaxKind.UnsafeKeyword);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="tokenList"/> contains the <see langword="virtual"/> token.
		/// </summary>
		/// <param name="tokenList">Determines whether the specified <paramref name="tokenList"/> contains the <see langword="virtual"/> token.</param>
		public static bool IsVirtual(this SyntaxTokenList tokenList)
		{
			return tokenList.Any(SyntaxKind.VirtualKeyword);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="tokenList"/> contains the <see langword="volatile"/> modifier.
		/// </summary>
		/// <param name="tokenList">Determines whether the specified <paramref name="tokenList"/> contains the <see langword="volatile"/> modifier.</param>
		public static bool IsVolatile(this SyntaxTokenList tokenList)
		{
			return tokenList.Any(SyntaxKind.VolatileKeyword);
		}
	}
}
