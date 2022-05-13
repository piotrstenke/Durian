// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.CodeGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.Extensions
{
	/// <summary>
	/// Contains extension methods for various enum types.
	/// </summary>
	public static class EnumExtensions
	{
		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="AnonymousFunctionBody"/> value.
		/// </summary>
		/// <param name="value"><see cref="MethodBody"/> to convert.</param>
		public static AnonymousFunctionBody AsAnonymousFunction(this MethodBody value)
		{
			return (AnonymousFunctionBody)value;
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="DecimalLiteralSuffix"/> value.
		/// </summary>
		/// <param name="value"><see cref="NumericLiteralSuffix"/> to convert.</param>
		public static DecimalLiteralSuffix AsDecimal(this NumericLiteralSuffix value)
		{
			int number = (int)value - (int)NumericLiteralSuffix.LongUpperUnsignedUpper;

			if (number < 0)
			{
				return default;
			}

			return (DecimalLiteralSuffix)number;
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="MethodBody"/> value.
		/// </summary>
		/// <param name="value"><see cref="AnonymousFunctionBody"/> to convert.</param>
		public static MethodBody AsMethod(this AnonymousFunctionBody value)
		{
			if (value == AnonymousFunctionBody.Method)
			{
				return MethodBody.Block;
			}

			return (MethodBody)value;
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="NumericLiteralSuffix"/> value.
		/// </summary>
		/// <param name="value"><see cref="DecimalLiteralSuffix"/> to convert.</param>
		public static NumericLiteralSuffix AsNumeric(this DecimalLiteralSuffix value)
		{
			return value == DecimalLiteralSuffix.None
				? NumericLiteralSuffix.None
				: (NumericLiteralSuffix)(value + (int)NumericLiteralSuffix.LongUpperUnsignedUpper);
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/>to an associated <see cref="RefKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="SyntaxKind"/> to convert.</param>
		public static RefKind AsRef(this SyntaxKind value)
		{
			return value switch
			{
				SyntaxKind.RefKeyword => RefKind.Ref,
				SyntaxKind.InKeyword => RefKind.In,
				SyntaxKind.OutKeyword => RefKind.Out,
				_ => RefKind.None
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="GenericSubstitution"/> value.
		/// </summary>
		/// <param name="value"><see cref="SymbolName"/> to convert.</param>
		public static GenericSubstitution AsSubstitution(this SymbolName value)
		{
			if(value == SymbolName.Substituted)
			{
				return GenericSubstitution.TypeArguments;
			}

			return GenericSubstitution.None;
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="SyntaxKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="RefKind"/> to convert.</param>
		public static SyntaxKind AsSyntax(this RefKind value)
		{
			return value switch
			{
				RefKind.Ref => SyntaxKind.RefKeyword,
				RefKind.Out => SyntaxKind.OutKeyword,
				RefKind.In => SyntaxKind.InKeyword,
				_ => SyntaxKind.None
			};
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
				SymbolKind.NamedType => targetKind == AttributeTargetKind.Value ? "return" : "type",
				SymbolKind.Field => "field",
				SymbolKind.Method => targetKind == AttributeTargetKind.Value ? "return" : "method",
				SymbolKind.Property => targetKind == AttributeTargetKind.Value ? "field" : "property",
				SymbolKind.Event => targetKind switch
				{
					AttributeTargetKind.This => "event",
					AttributeTargetKind.Value => "field",
					AttributeTargetKind.Handler => "method",
					_ => default
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
		public static string GetAttributeTarget(this MethodKind kind, AttributeTargetKind targetKind = default)
		{
			return kind switch
			{
				MethodKind.EventAdd or
				MethodKind.EventRemove or
				MethodKind.PropertySet => targetKind switch
				{
					AttributeTargetKind.Value => "return",
					AttributeTargetKind.Handler => "param",
					_ => "method"
				},
				_ => targetKind == AttributeTargetKind.Value ? "return" : "method"
			};
		}

		/// <summary>
		/// Returns a <see cref="string"/> representation of a C# keyword associated with the specified <paramref name="specialType"/> value.
		/// </summary>
		/// <param name="specialType">Value of <see cref="SpecialType"/> to get the C# keyword associated with.</param>
		public static string? GetKeyword(this SpecialType specialType)
		{
			if (specialType == SpecialType.None)
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

		/// <summary>
		/// Converts the specified <see cref="Accessibility"/> <paramref name="value"/> to its <see cref="string"/> representation.
		/// </summary>
		/// <param name="value"><see cref="Accessibility"/> to convert to a <see cref="string"/> representation.</param>
		public static string? GetText(this Accessibility value)
		{
			return value switch
			{
				Accessibility.Public => "public",
				Accessibility.Protected => "protected",
				Accessibility.Internal => "internal",
				Accessibility.ProtectedOrInternal => "protected internal",
				Accessibility.ProtectedAndInternal => "private protected",
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <see cref="OverloadableOperator"/> <paramref name="value"/> to its <see cref="string"/> representation.
		/// </summary>
		/// <param name="value"><see cref="OverloadableOperator"/> to convert to a <see cref="string"/> representation.</param>
		public static string? GetText(this OverloadableOperator value)
		{
			return value switch
			{
				OverloadableOperator.Addition or OverloadableOperator.UnaryPlus => "+",
				OverloadableOperator.Subtraction or OverloadableOperator.UnaryMinus => "-",
				OverloadableOperator.Multiplication => "*",
				OverloadableOperator.Division => "/",
				OverloadableOperator.Equality => "==",
				OverloadableOperator.Inequality => "!=",
				OverloadableOperator.Negation => "!",
				OverloadableOperator.GreaterThan => ">",
				OverloadableOperator.GreaterThanOrEqual => ">=",
				OverloadableOperator.LessThan => "<",
				OverloadableOperator.LestThanOrEqual => "<=",
				OverloadableOperator.Complement => "~",
				OverloadableOperator.LogicalAnd => "&",
				OverloadableOperator.LogicalOr => "|",
				OverloadableOperator.LogicalXor => "^",
				OverloadableOperator.Remainder => "%",
				OverloadableOperator.Increment => "++",
				OverloadableOperator.Decrement => "--",
				OverloadableOperator.False => "false",
				OverloadableOperator.True => "true",
				OverloadableOperator.RightShift => ">>",
				OverloadableOperator.LeftShift => "<<",
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <see cref="RefKind"/> <paramref name="value"/> to its <see cref="string"/> representation.
		/// </summary>
		/// <param name="value"><see cref="RefKind"/> to convert to a <see cref="string"/> representation.</param>
		/// <param name="useIn">Determines whether to return <see langword="in"/> instead of <see langword="ref"/> <see langword="readonly"/>.</param>
		public static string? GetText(this RefKind value, bool useIn = true)
		{
			return value switch
			{
				RefKind.In => useIn ? "in" : "ref readonly",
				RefKind.Ref => "ref",
				RefKind.Out => "out",
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <see cref="VarianceKind"/> <paramref name="value"/> to its <see cref="string"/> representation.
		/// </summary>
		/// <param name="value"><see cref="VarianceKind"/> to convert to a <see cref="string"/> representation.</param>
		public static string? GetText(this VarianceKind value)
		{
			return value switch
			{
				VarianceKind.In => "in",
				VarianceKind.Out => "out",
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <see cref="NumericLiteralPrefix"/> <paramref name="value"/> to its <see cref="string"/> representation.
		/// </summary>
		/// <param name="value"><see cref="NumericLiteralPrefix"/> to convert to a <see cref="string"/> representation.</param>
		public static string? GetText(this NumericLiteralPrefix value)
		{
			return value switch
			{
				NumericLiteralPrefix.HexadecimalLower => "0x",
				NumericLiteralPrefix.HexadecimalUpper => "0X",
				NumericLiteralPrefix.BinaryLower => "0b",
				NumericLiteralPrefix.BinaryUpper => "0B",
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <see cref="DecimalLiteralSuffix"/> <paramref name="value"/> to its <see cref="string"/> representation.
		/// </summary>
		/// <param name="value"><see cref="DecimalLiteralSuffix"/> to convert to a <see cref="string"/> representation.</param>
		public static string? GetText(this DecimalLiteralSuffix value)
		{
			return value.AsNumeric().GetText();
		}

		/// <summary>
		/// Converts the specified <see cref="NumericLiteralSuffix"/> <paramref name="value"/> to its <see cref="string"/> representation.
		/// </summary>
		/// <param name="value"><see cref="NumericLiteralSuffix"/> to convert to a <see cref="string"/> representation.</param>
		public static string? GetText(this NumericLiteralSuffix value)
		{
			return value switch
			{
				NumericLiteralSuffix.UnsignedLower => "u",
				NumericLiteralSuffix.UnsignedUpper => "U",
				NumericLiteralSuffix.LongLower => "l",
				NumericLiteralSuffix.LongUpper => "L",
				NumericLiteralSuffix.FloatLower => "f",
				NumericLiteralSuffix.FloatUpper => "F",
				NumericLiteralSuffix.DoubleLower => "d",
				NumericLiteralSuffix.DoubleUpper => "D",
				NumericLiteralSuffix.DecimalLower => "m",
				NumericLiteralSuffix.DecimalUpper => "M",
				NumericLiteralSuffix.UnsignedLowerLongLower => "ul",
				NumericLiteralSuffix.UnsignedLowerLongUpper => "uL",
				NumericLiteralSuffix.UnsignedUpperLongLower => "Ul",
				NumericLiteralSuffix.UnsignedUpperLongUpper => "UL",
				NumericLiteralSuffix.LongLowerUnsignedLower => "lu",
				NumericLiteralSuffix.LongLowerUnsignedUpper => "lU",
				NumericLiteralSuffix.LongUpperUnsignedLower => "Lu",
				NumericLiteralSuffix.LongUpperUnsignedUpper => "LU",
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <see cref="AttributeTarget"/> <paramref name="value"/> to its <see cref="string"/> representation.
		/// </summary>
		/// <param name="value"><see cref="AttributeTarget"/> to convert to a <see cref="string"/> representation.</param>
		public static string? GetText(this AttributeTarget value)
		{
			return value switch
			{
				AttributeTarget.Assembly => "assembly",
				AttributeTarget.Field => "field",
				AttributeTarget.Return => "return",
				AttributeTarget.Method => "method",
				AttributeTarget.Property => "property",
				AttributeTarget.Event => "event",
				AttributeTarget.Type => "type",
				AttributeTarget.TypeVar => "typevar",
				AttributeTarget.Param => "param",
				AttributeTarget.Module => "module",
				_ => default
			};
		}

		/// <summary>
		/// Determines whether the specified <paramref name="kind"/> is a declaration kind.
		/// </summary>
		/// <param name="kind"><see cref="TypeKind"/> to determine whether is a declaration kind.</param>
		public static bool IsDeclarationKind(this TypeKind kind)
		{
			return kind is
				TypeKind.Class or
				TypeKind.Struct or
				TypeKind.Enum or
				TypeKind.Interface or
				TypeKind.Delegate;
		}

		/// <summary>
		/// Determines whether the specified ?<paramref name="specialType"/> can be represented using a C# keyword.
		/// </summary>
		/// <param name="specialType"><see cref="SpecialType"/> to determine whether can be represented using a C# keyword.</param>
		public static bool IsKeyword(this SpecialType specialType)
		{
			if(specialType == SpecialType.None)
			{
				return false;
			}

			if(specialType == SpecialType.System_Object)
			{
				return true;
			}

			return specialType >= SpecialType.System_Void && specialType <= SpecialType.System_UIntPtr;
		}

		/// <summary>
		/// Determines whether the <paramref name="first"/> <see cref="RefKind"/> is valid on a method when there is an overload that takes the same parameter, but with the <paramref name="second"/> <see cref="RefKind"/>.
		/// </summary>
		/// <param name="first">First <see cref="RefKind"/>.</param>
		/// <param name="second">Second <see cref="RefKind"/>.</param>
		public static bool IsValidForOverload(this RefKind first, RefKind second)
		{
			return first == RefKind.None
				? second != RefKind.None
				: second == RefKind.None;
		}
	}
}
