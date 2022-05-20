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
		/// Converts the specified <paramref name="value"/> to an associated <see cref="Accessor"/> value.
		/// </summary>
		/// <param name="value"><see cref="MethodKind"/> to convert.</param>
		public static Accessor AsAccessor(this MethodKind value)
		{
			return value switch
			{
				MethodKind.PropertyGet => Accessor.Get,
				MethodKind.PropertySet => Accessor.Set,
				MethodKind.EventAdd => Accessor.Add,
				MethodKind.EventRemove => Accessor.Remove,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="Accessor"/> value.
		/// </summary>
		/// <param name="value"><see cref="PropertyAccessor"/> to convert.</param>
		public static Accessor AsAccessor(this PropertyAccessor value)
		{
			int n = (int)value;

			if (n <= (int)Accessor.Init)
			{
				return (Accessor)value;
			}

			return default;
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="Accessor"/> value.
		/// </summary>
		/// <param name="value"><see cref="EventAccessor"/> to convert.</param>
		public static Accessor AsAccessor(this EventAccessor value)
		{
			const int min = (int)Accessor.Init;

			if (value == default)
			{
				return default;
			}

			int n = (int)value + min;

			if (n <= min)
			{
				return default;
			}

			return (Accessor)n;
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
		/// Converts the specified <paramref name="value"/> to an associated <see cref="DecimalValueType"/> value.
		/// </summary>
		/// <param name="value"><see cref="SpecialType"/> to convert.</param>
		public static DecimalValueType AsDecimal(this SpecialType value)
		{
			return value switch
			{
				SpecialType.System_Single => DecimalValueType.Float,
				SpecialType.System_Double => DecimalValueType.Double,
				SpecialType.System_Decimal => DecimalValueType.Decimal,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="DecimalValueType"/> value.
		/// </summary>
		/// <param name="value"><see cref="KeywordType"/> to convert.</param>
		public static DecimalValueType AsDecimal(this KeywordType value)
		{
			return value switch
			{
				KeywordType.Float => DecimalValueType.Float,
				KeywordType.Double => DecimalValueType.Double,
				KeywordType.Decimal => DecimalValueType.Decimal,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="EventAccessor"/> value.
		/// </summary>
		/// <param name="value"><see cref="Accessor"/> to convert.</param>
		public static EventAccessor AsEvent(this Accessor value)
		{
			if (value == default)
			{
				return default;
			}

			return (EventAccessor)(value - (int)Accessor.Init);
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="IntegerValueType"/> value.
		/// </summary>
		/// <param name="value"><see cref="SpecialType"/> to convert.</param>
		public static IntegerValueType AsInteger(this SpecialType value)
		{
			return value switch
			{
				SpecialType.System_Int16 => IntegerValueType.Int16,
				SpecialType.System_Int32 => IntegerValueType.Int32,
				SpecialType.System_Int64 => IntegerValueType.Int64,
				SpecialType.System_UInt16 => IntegerValueType.UInt16,
				SpecialType.System_UInt32 => IntegerValueType.UInt32,
				SpecialType.System_UInt64 => IntegerValueType.UInt64,
				SpecialType.System_Byte => IntegerValueType.Byte,
				SpecialType.System_SByte => IntegerValueType.SByte,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="IntegerValueType"/> value.
		/// </summary>
		/// <param name="value"><see cref="KeywordType"/> to convert.</param>
		public static IntegerValueType AsInteger(this KeywordType value)
		{
			int n = (int)value;

			if (n <= 0 || n > (int)KeywordType.SByte)
			{
				return default;
			}

			return (IntegerValueType)n;
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="KeywordType"/> value.
		/// </summary>
		/// <param name="value"><see cref="SpecialType"/> to convert.</param>
		public static KeywordType AsKeyword(this SpecialType value)
		{
			return value switch
			{
				SpecialType.System_Int16 => KeywordType.Short,
				SpecialType.System_Int32 => KeywordType.Int,
				SpecialType.System_Int64 => KeywordType.Long,
				SpecialType.System_UInt16 => KeywordType.UShort,
				SpecialType.System_UInt32 => KeywordType.UInt,
				SpecialType.System_UInt64 => KeywordType.ULong,
				SpecialType.System_Byte => KeywordType.Byte,
				SpecialType.System_SByte => KeywordType.SByte,
				SpecialType.System_Single => KeywordType.Float,
				SpecialType.System_Double => KeywordType.Double,
				SpecialType.System_Decimal => KeywordType.Decimal,
				SpecialType.System_String => KeywordType.String,
				SpecialType.System_Char => KeywordType.Char,
				SpecialType.System_Boolean => KeywordType.Bool,
				SpecialType.System_Object => KeywordType.Object,
				SpecialType.System_IntPtr => KeywordType.NInt,
				SpecialType.System_UIntPtr => KeywordType.NUInt,
				SpecialType.System_Void => KeywordType.Void,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="KeywordType"/> value.
		/// </summary>
		/// <param name="value"><see cref="IntegerValueType"/> to convert.</param>
		public static KeywordType AsKeyword(this IntegerValueType value)
		{
			return value switch
			{
				IntegerValueType.Int16 => KeywordType.Short,
				IntegerValueType.Int32 => KeywordType.Int,
				IntegerValueType.Int64 => KeywordType.Long,
				IntegerValueType.UInt16 => KeywordType.UShort,
				IntegerValueType.UInt32 => KeywordType.UInt,
				IntegerValueType.UInt64 => KeywordType.ULong,
				IntegerValueType.Byte => KeywordType.Byte,
				IntegerValueType.SByte => KeywordType.SByte,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="KeywordType"/> value.
		/// </summary>
		/// <param name="value"><see cref="DecimalValueType"/> to convert.</param>
		public static KeywordType AsKeyword(this DecimalValueType value)
		{
			return value switch
			{
				DecimalValueType.Float => KeywordType.Float,
				DecimalValueType.Double => KeywordType.Double,
				DecimalValueType.Decimal => KeywordType.Decimal,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="LambdaStyle"/> value.
		/// </summary>
		/// <param name="value"><see cref="MethodStyle"/> to convert.</param>
		public static LambdaStyle AsLambda(this MethodStyle value)
		{
			return (LambdaStyle)value;
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="MethodStyle"/> value.
		/// </summary>
		/// <param name="value"><see cref="LambdaStyle"/> to convert.</param>
		public static MethodStyle AsMethod(this LambdaStyle value)
		{
			if (value == LambdaStyle.Method)
			{
				return MethodStyle.Block;
			}

			return (MethodStyle)value;
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="MethodKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="Accessor"/> to convert.</param>
		public static MethodKind AsMethod(this Accessor value)
		{
			return value switch
			{
				Accessor.Get => MethodKind.PropertyGet,
				Accessor.Set => MethodKind.PropertySet,
				Accessor.Init => MethodKind.PropertySet,
				Accessor.Add => MethodKind.EventAdd,
				Accessor.Remove => MethodKind.EventRemove,
				_ => default
			};
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
		/// Converts the specified <paramref name="value"/> to an associated <see cref="PropertyAccessor"/> value.
		/// </summary>
		/// <param name="value"><see cref="Accessor"/> to convert.</param>
		public static PropertyAccessor AsProperty(this Accessor value)
		{
			return (PropertyAccessor)value;
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
		/// Converts the specified <paramref name="value"/> to an associated <see cref="SpecialType"/> value.
		/// </summary>
		/// <param name="value"><see cref="KeywordType"/> to convert.</param>
		public static SpecialType AsSpecialType(this KeywordType value)
		{
			return value switch
			{
				KeywordType.Short => SpecialType.System_Int16,
				KeywordType.Int => SpecialType.System_Int32,
				KeywordType.Long => SpecialType.System_Int64,
				KeywordType.UShort => SpecialType.System_UInt16,
				KeywordType.UInt => SpecialType.System_UInt32,
				KeywordType.ULong => SpecialType.System_UInt64,
				KeywordType.Byte => SpecialType.System_Byte,
				KeywordType.SByte => SpecialType.System_SByte,
				KeywordType.Float => SpecialType.System_Single,
				KeywordType.Double => SpecialType.System_Double,
				KeywordType.Decimal => SpecialType.System_Decimal,
				KeywordType.Bool => SpecialType.System_Boolean,
				KeywordType.Char => SpecialType.System_Char,
				KeywordType.String => SpecialType.System_String,
				KeywordType.Void => SpecialType.System_Void,
				KeywordType.Object or KeywordType.Dynamic => SpecialType.System_Object,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="SpecialType"/> value.
		/// </summary>
		/// <param name="value"><see cref="DecimalValueType"/> to convert.</param>
		public static SpecialType AsSpecialType(this DecimalValueType value)
		{
			return value switch
			{
				DecimalValueType.Float => SpecialType.System_Single,
				DecimalValueType.Double => SpecialType.System_Double,
				DecimalValueType.Decimal => SpecialType.System_Decimal,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="SpecialType"/> value.
		/// </summary>
		/// <param name="value"><see cref="IntegerValueType"/> to convert.</param>
		public static SpecialType AsSpecialType(this IntegerValueType value)
		{
			return value switch
			{
				IntegerValueType.Int16 => SpecialType.System_Int16,
				IntegerValueType.Int32 => SpecialType.System_Int32,
				IntegerValueType.Int64 => SpecialType.System_Int64,
				IntegerValueType.UInt16 => SpecialType.System_UInt16,
				IntegerValueType.UInt32 => SpecialType.System_UInt32,
				IntegerValueType.UInt64 => SpecialType.System_UInt64,
				IntegerValueType.Byte => SpecialType.System_Byte,
				IntegerValueType.SByte => SpecialType.System_SByte,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="NumericLiteralSuffix"/> value.
		/// </summary>
		/// <remarks>The returned value is considered unsigned-first uppercase (see <see cref="IsUpper(NumericLiteralPrefix)"/> and <see cref="UnsignedFirst(NumericLiteralSuffix)"/>for more details).</remarks>
		/// <param name="value"><see cref="KeywordType"/> to convert.</param>
		public static NumericLiteralSuffix AsSuffix(this KeywordType value)
		{
			return value switch
			{
				KeywordType.Long => NumericLiteralSuffix.LongUpper,
				KeywordType.ULong => NumericLiteralSuffix.UnsignedUpperLongUpper,
				KeywordType.UInt => NumericLiteralSuffix.UnsignedUpper,
				KeywordType.Float => NumericLiteralSuffix.FloatUpper,
				KeywordType.Double => NumericLiteralSuffix.DoubleUpper,
				KeywordType.Decimal => NumericLiteralSuffix.DecimalUpper,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="NumericLiteralSuffix"/> value.
		/// </summary>
		/// <remarks>The returned value is considered unsigned-first uppercase (see <see cref="IsUpper(NumericLiteralPrefix)"/> and <see cref="UnsignedFirst(NumericLiteralSuffix)"/>for more details).</remarks>
		/// <param name="value"><see cref="SpecialType"/> to convert.</param>
		public static NumericLiteralSuffix AsSuffix(this SpecialType value)
		{
			return value switch
			{
				SpecialType.System_Int64 => NumericLiteralSuffix.LongUpper,
				SpecialType.System_UInt64 => NumericLiteralSuffix.UnsignedUpperLongUpper,
				SpecialType.System_UInt32 => NumericLiteralSuffix.UnsignedUpper,
				SpecialType.System_Single => NumericLiteralSuffix.FloatUpper,
				SpecialType.System_Double => NumericLiteralSuffix.DoubleUpper,
				SpecialType.System_Decimal => NumericLiteralSuffix.DecimalUpper,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="NumericLiteralSuffix"/> value.
		/// </summary>
		/// <remarks>The returned value is considered unsigned-first uppercase (see <see cref="IsUpper(NumericLiteralPrefix)"/> and <see cref="UnsignedFirst(NumericLiteralSuffix)"/>for more details).</remarks>
		/// <param name="value"><see cref="IntegerValueType"/> to convert.</param>
		public static NumericLiteralSuffix AsSuffix(this IntegerValueType value)
		{
			return value switch
			{
				IntegerValueType.Int64 => NumericLiteralSuffix.LongUpper,
				IntegerValueType.UInt64 => NumericLiteralSuffix.UnsignedUpperLongUpper,
				IntegerValueType.UInt32 => NumericLiteralSuffix.UnsignedUpper,
				_ => default,
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="DecimalLiteralSuffix"/> value.
		/// </summary>
		/// <remarks>The returned value is considered unsigned-first uppercase (see <see cref="IsUpper(NumericLiteralPrefix)"/> and <see cref="UnsignedFirst(NumericLiteralSuffix)"/>for more details).</remarks>
		/// <param name="value"><see cref="DecimalValueType"/> to convert.</param>
		public static DecimalLiteralSuffix AsSuffix(this DecimalValueType value)
		{
			return value switch
			{
				DecimalValueType.Float => DecimalLiteralSuffix.FloatUpper,
				DecimalValueType.Double => DecimalLiteralSuffix.DoubleUpper,
				DecimalValueType.Decimal => DecimalLiteralSuffix.DecimalUpper,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="SyntaxKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="DecimalValueType"/> to convert.</param>
		public static SyntaxKind AsSyntax(this DecimalValueType value)
		{
			return value switch
			{
				DecimalValueType.Float => SyntaxKind.FloatKeyword,
				DecimalValueType.Double => SyntaxKind.DoubleKeyword,
				DecimalValueType.Decimal => SyntaxKind.DecimalKeyword,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="SyntaxKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="IntegerValueType"/> to convert.</param>
		public static SyntaxKind AsSyntax(this IntegerValueType value)
		{
			return value switch
			{
				IntegerValueType.Int16 => SyntaxKind.ShortKeyword,
				IntegerValueType.Int32 => SyntaxKind.IntKeyword,
				IntegerValueType.Int64 => SyntaxKind.LongKeyword,
				IntegerValueType.UInt16 => SyntaxKind.UShortKeyword,
				IntegerValueType.UInt32 => SyntaxKind.UIntKeyword,
				IntegerValueType.UInt64 => SyntaxKind.ULongKeyword,
				IntegerValueType.Byte => SyntaxKind.ByteKeyword,
				IntegerValueType.SByte => SyntaxKind.SByteKeyword,
				_ => default
			};
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
		/// Converts the specified <paramref name="value"/> to an associated <see cref="SyntaxKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="ConstructorInitializer"/> to convert.</param>
		public static SyntaxKind AsSyntax(this ConstructorInitializer value)
		{
			return value switch
			{
				ConstructorInitializer.Base => SyntaxKind.BaseConstructorInitializer,
				ConstructorInitializer.This => SyntaxKind.ThisConstructorInitializer,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="SyntaxKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="AttributeTarget"/> to convert.</param>
		public static SyntaxKind AsSyntax(this AttributeTarget value)
		{
			return value switch
			{
				AttributeTarget.Assembly => SyntaxKind.AssemblyKeyword,
				AttributeTarget.Method => SyntaxKind.MethodKeyword,
				AttributeTarget.Return => SyntaxKind.ReturnKeyword,
				AttributeTarget.Property => SyntaxKind.PropertyKeyword,
				AttributeTarget.Field => SyntaxKind.FieldKeyword,
				AttributeTarget.Event => SyntaxKind.EventKeyword,
				AttributeTarget.Type => SyntaxKind.TypeKeyword,
				AttributeTarget.TypeVar => SyntaxKind.TypeVarKeyword,
				AttributeTarget.Param => SyntaxKind.ParamKeyword,
				AttributeTarget.Module => SyntaxKind.ModuleKeyword,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="SyntaxKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="OverloadableOperator"/> to convert.</param>
		public static SyntaxKind AsSyntax(this OverloadableOperator value)
		{
			return value switch
			{
				OverloadableOperator.UnaryPlus => SyntaxKind.PlusToken,
				OverloadableOperator.UnaryMinus => SyntaxKind.MinusGreaterThanToken,
				OverloadableOperator.Negation => SyntaxKind.ExclamationToken,
				OverloadableOperator.Complement => SyntaxKind.TildeToken,
				OverloadableOperator.Increment => SyntaxKind.PlusPlusToken,
				OverloadableOperator.Decrement => SyntaxKind.MinusMinusToken,
				OverloadableOperator.True => SyntaxKind.TrueKeyword,
				OverloadableOperator.False => SyntaxKind.FalseKeyword,
				OverloadableOperator.Addition => SyntaxKind.PlusToken,
				OverloadableOperator.Subtraction => SyntaxKind.MinusToken,
				OverloadableOperator.Multiplication => SyntaxKind.AsteriskToken,
				OverloadableOperator.Division => SyntaxKind.SlashToken,
				OverloadableOperator.Remainder => SyntaxKind.PercentToken,
				OverloadableOperator.LogicalAnd => SyntaxKind.AmpersandAmpersandToken,
				OverloadableOperator.LogicalOr => SyntaxKind.BarBarToken,
				OverloadableOperator.LogicalXor => SyntaxKind.CaretToken,
				OverloadableOperator.LeftShift => SyntaxKind.LessThanLessThanToken,
				OverloadableOperator.RightShift => SyntaxKind.GreaterThanGreaterThanToken,
				OverloadableOperator.Equality => SyntaxKind.EqualsEqualsToken,
				OverloadableOperator.Inequality => SyntaxKind.ExclamationEqualsToken,
				OverloadableOperator.LessThan => SyntaxKind.LessThanToken,
				OverloadableOperator.GreaterThan => SyntaxKind.GreaterThanToken,
				OverloadableOperator.LestThanOrEqual => SyntaxKind.LessThanEqualsToken,
				OverloadableOperator.GreaterThanOrEqual => SyntaxKind.GreaterThanEqualsToken,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="SyntaxKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="NamespaceStyle"/> to convert.</param>
		public static SyntaxKind AsSyntax(this GenericConstraint value)
		{
			return value switch
			{
				GenericConstraint.Type => SyntaxKind.TypeConstraint,
				GenericConstraint.New => SyntaxKind.ConstructorConstraint,
				GenericConstraint.Class => SyntaxKind.ClassConstraint,
				GenericConstraint.Struct => SyntaxKind.StructConstraint,
				GenericConstraint.Unmanaged => SyntaxKind.UnmanagedKeyword,
				GenericConstraint.NotNull => SyntaxKind.NotKeyword,
				GenericConstraint.Default => SyntaxKind.DefaultConstraint,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="SyntaxKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="NamespaceStyle"/> to convert.</param>
		public static SyntaxKind AsSyntax(this NamespaceStyle value)
		{
			return value switch
			{
				NamespaceStyle.Default or NamespaceStyle.Nested => SyntaxKind.NamespaceDeclaration,
				NamespaceStyle.File => SyntaxKind.FileScopedNamespaceDeclaration,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="SyntaxKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="Accessor"/> to convert.</param>
		public static SyntaxKind AsSyntax(this Accessor value)
		{
			return value switch
			{
				Accessor.Get => SyntaxKind.GetAccessorDeclaration,
				Accessor.Set => SyntaxKind.SetAccessorDeclaration,
				Accessor.Init => SyntaxKind.InitAccessorDeclaration,
				Accessor.Add => SyntaxKind.AddAccessorDeclaration,
				Accessor.Remove => SyntaxKind.RemoveAccessorDeclaration,
				_ => default
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
		/// Returns the value that is considered default for the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="KeywordType"/> to get the default value of.</param>
		public static object? GetDefaultValue(this KeywordType type)
		{
			return type switch
			{
				KeywordType.Short => default(short),
				KeywordType.Int => default(int),
				KeywordType.Long => default(long),
				KeywordType.UShort => default(ushort),
				KeywordType.UInt => default(uint),
				KeywordType.ULong => default(ulong),
				KeywordType.Byte => default(byte),
				KeywordType.SByte => default(sbyte),
				KeywordType.Float => default(float),
				KeywordType.Double => default(double),
				KeywordType.Decimal => default(decimal),
				KeywordType.Char => default(char),
				KeywordType.String => default(string),
				KeywordType.Bool => default(bool),
				KeywordType.NInt => default(nint),
				KeywordType.NUInt => default(nuint),
				KeywordType.Object => default(object),
				KeywordType.Dynamic => default(dynamic),
				// KeywordType.Void => default,
				_ => default
			};
		}

		/// <summary>
		/// Returns a <see cref="string"/> representation of a C# keyword associated with the specified <paramref name="specialType"/> value.
		/// </summary>
		/// <param name="specialType">Value of <see cref="SpecialType"/> to get the C# keyword associated with.</param>
		public static string? GetText(this SpecialType specialType)
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
		/// Converts the specified <see cref="KeywordType"/> <paramref name="value"/> to its <see cref="string"/> representation.
		/// </summary>
		/// <param name="value"><see cref="KeywordType"/> to convert to a <see cref="string"/> representation.</param>
		public static string? GetText(this KeywordType value)
		{
			return value switch
			{
				KeywordType.Short => "short",
				KeywordType.Int => "int",
				KeywordType.Long => "long",
				KeywordType.UShort => "ushort",
				KeywordType.UInt => "uint",
				KeywordType.ULong => "ulong",
				KeywordType.Byte => "byte",
				KeywordType.SByte => "sbyte",
				KeywordType.Float => "float",
				KeywordType.Double => "double",
				KeywordType.Decimal => "decimal",
				KeywordType.Bool => "bool",
				KeywordType.String => "string",
				KeywordType.Char => "char",
				KeywordType.NInt => "nint",
				KeywordType.NUInt => "nuint",
				KeywordType.Void => "void",
				KeywordType.Object => "object",
				KeywordType.Dynamic => "dynamic",
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
		/// Converts the specified <see cref="StringModifiers"/> <paramref name="value"/> to its <see cref="string"/> representation.
		/// </summary>
		/// <param name="value"><see cref="StringModifiers"/> to convert to a <see cref="string"/> representation.</param>
		public static string? GetText(this StringModifiers value)
		{
			return value switch
			{
				StringModifiers.Verbatim => "@",
				StringModifiers.Interpolation => "$",
				StringModifiers.Verbatim | StringModifiers.Interpolation => "$@",
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <see cref="Exponential"/> <paramref name="value"/> to its <see cref="string"/> representation.
		/// </summary>
		/// <param name="value"><see cref="Exponential"/> to convert to a <see cref="string"/> representation.</param>
		public static string? GetText(this Exponential value)
		{
			return value switch
			{
				Exponential.Lowercase => "e",
				Exponential.Uppercase => "E",
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <see cref="Accessor"/> <paramref name="value"/> to its <see cref="string"/> representation.
		/// </summary>
		/// <param name="value"><see cref="Accessor"/> to convert to a <see cref="string"/> representation.</param>
		public static string? GetText(this Accessor value)
		{
			return value switch
			{
				Accessor.Get => "get",
				Accessor.Set => "set",
				Accessor.Init => "init",
				Accessor.Add => "add",
				Accessor.Remove => "remove",
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <see cref="GenericConstraint"/> <paramref name="value"/> to its <see cref="string"/> representation.
		/// </summary>
		/// <param name="value"><see cref="GenericConstraint"/> to convert to a <see cref="string"/> representation.</param>
		public static string? GetText(this GenericConstraint value)
		{
			return value switch
			{
				GenericConstraint.New => "new()",
				GenericConstraint.Class => "class",
				GenericConstraint.Struct => "struct",
				GenericConstraint.Unmanaged => "unmanaged",
				GenericConstraint.NotNull => "notnull",
				GenericConstraint.Default => "default",
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <see cref="IntegerValueType"/> <paramref name="value"/> to its <see cref="string"/> representation.
		/// </summary>
		/// <param name="value"><see cref="IntegerValueType"/> to convert to a <see cref="string"/> representation.</param>
		public static string? GetText(this IntegerValueType value)
		{
			return value switch
			{
				IntegerValueType.Int16 => "short",
				IntegerValueType.Int32 => "int",
				IntegerValueType.Int64 => "long",
				IntegerValueType.UInt16 => "ushort",
				IntegerValueType.UInt32 => "uint",
				IntegerValueType.UInt64 => "ulong",
				IntegerValueType.Byte => "byte",
				IntegerValueType.SByte => "sbyte",
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <see cref="DecimalValueType"/> <paramref name="value"/> to its <see cref="string"/> representation.
		/// </summary>
		/// <param name="value"><see cref="DecimalValueType"/> to convert to a <see cref="string"/> representation.</param>
		public static string? GetText(this DecimalValueType value)
		{
			return value switch
			{
				DecimalValueType.Float => "float",
				DecimalValueType.Double => "double",
				DecimalValueType.Decimal => "decimal",
				_ => default
			};
		}

		/// <summary>
		/// Determines whether the specified <see cref="NumericLiteralPrefix"/> <paramref name="value"/> is considered to be binary, that is either:
		/// <list type="bullet">
		/// <item><see cref="NumericLiteralPrefix.BinaryLower"/></item>
		/// <item><see cref="NumericLiteralPrefix.BinaryUpper"/></item>
		/// </list>
		/// </summary>
		/// <param name="value"><see cref="NumericLiteralSuffix"/> to determine whether is considered to be binary.</param>
		public static bool IsBinary(this NumericLiteralPrefix value)
		{
			return value is NumericLiteralPrefix.BinaryLower or NumericLiteralPrefix.BinaryUpper;
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
		/// Determines whether the specified <paramref name="accessor"/> is an event accessor.
		/// </summary>
		/// <param name="accessor"><see cref="Accessor"/> to determine whether is an event accessor.</param>
		public static bool IsEventAccessor(this Accessor accessor)
		{
			return accessor is Accessor.Add or Accessor.Remove;
		}

		/// <summary>
		/// Determines whether the specified <see cref="NumericLiteralPrefix"/> <paramref name="value"/> is considered to be hexadecimal, that is either:
		/// <list type="bullet">
		/// <item><see cref="NumericLiteralPrefix.HexadecimalLower"/></item>
		/// <item><see cref="NumericLiteralPrefix.HexadecimalUpper"/></item>
		/// </list>
		/// </summary>
		/// <param name="value"><see cref="NumericLiteralSuffix"/> to determine whether is considered to be hexadecimal.</param>
		public static bool IsHexadecimal(this NumericLiteralPrefix value)
		{
			return value is NumericLiteralPrefix.HexadecimalLower or NumericLiteralPrefix.HexadecimalUpper;
		}

		/// <summary>
		/// Determines whether the specified ?<paramref name="specialType"/> can be represented using a C# keyword.
		/// </summary>
		/// <param name="specialType"><see cref="SpecialType"/> to determine whether can be represented using a C# keyword.</param>
		public static bool IsKeyword(this SpecialType specialType)
		{
			if (specialType == SpecialType.None)
			{
				return false;
			}

			if (specialType == SpecialType.System_Object)
			{
				return true;
			}

			return specialType >= SpecialType.System_Void && specialType <= SpecialType.System_UIntPtr;
		}

		/// <summary>
		/// Determines whether the specified <see cref="NumericLiteralSuffix"/> <paramref name="value"/> is considered to be a <see cref="long"/>, that is either:
		/// <list type="bullet">
		/// <item><see cref="NumericLiteralSuffix.LongLower"/></item>
		/// <item><see cref="NumericLiteralSuffix.LongLowerUnsignedLower"/></item>
		/// <item><see cref="NumericLiteralSuffix.LongLowerUnsignedUpper"/></item>
		/// <item><see cref="NumericLiteralSuffix.LongUpper"/></item>
		/// <item><see cref="NumericLiteralSuffix.LongUpperUnsignedLower"/></item>
		/// <item><see cref="NumericLiteralSuffix.LongUpperUnsignedUpper"/></item>
		/// <item><see cref="NumericLiteralSuffix.UnsignedLowerLongLower"/></item>
		/// <item><see cref="NumericLiteralSuffix.UnsignedLowerLongUpper"/></item>
		/// <item><see cref="NumericLiteralSuffix.UnsignedUpperLongLower"/></item>
		/// <item><see cref="NumericLiteralSuffix.UnsignedUpperLongUpper"/></item>
		/// </list>
		/// </summary>
		/// <param name="value"><see cref="NumericLiteralSuffix"/> to determine whether is considered to be a <see cref="long"/>.</param>
		public static bool IsLong(this NumericLiteralSuffix value)
		{
			return value is NumericLiteralSuffix.LongLower or NumericLiteralSuffix.LongUpper || value.IsUnsignedLong();
		}

		/// <summary>
		/// Determines whether the specified <see cref="NumericLiteralSuffix"/> value is considered to be <see cref="long"/>-first, that is either:
		/// <list type="bullet">
		/// <item><see cref="NumericLiteralSuffix.LongLower"/></item>
		/// <item><see cref="NumericLiteralSuffix.LongLowerUnsignedLower"/></item>
		/// <item><see cref="NumericLiteralSuffix.LongLowerUnsignedUpper"/></item>
		/// <item><see cref="NumericLiteralSuffix.LongUpper"/></item>
		/// <item><see cref="NumericLiteralSuffix.LongUpperUnsignedLower"/></item>
		/// <item><see cref="NumericLiteralSuffix.LongUpperUnsignedUpper"/></item>
		/// </list>
		/// </summary>
		/// <param name="value"><see cref="NumericLiteralSuffix"/> value to determine whether is considered to be <see cref="long"/>-first.</param>
		public static bool IsLongFirst(this NumericLiteralSuffix value)
		{
			return value >= NumericLiteralSuffix.LongLower && value <= NumericLiteralSuffix.LongUpperUnsignedUpper;
		}

		/// <summary>
		/// Determines whether the specified <see cref="NumericLiteralPrefix"/> <paramref name="value"/> is considered to be all-lowercase, that is either:
		/// <list type="bullet">
		/// <item><see cref="NumericLiteralPrefix.BinaryLower"/></item>
		/// <item><see cref="NumericLiteralPrefix.HexadecimalLower"/></item>
		/// </list>
		/// </summary>
		/// <param name="value"><see cref="NumericLiteralPrefix"/> to determine whether is considered to be all-lowercase.</param>
		public static bool IsLower(this NumericLiteralPrefix value)
		{
			return value is NumericLiteralPrefix.BinaryLower or NumericLiteralPrefix.HexadecimalLower;
		}

		/// <summary>
		/// Determines whether the specified <see cref="DecimalLiteralSuffix"/> <paramref name="value"/> is considered to be all-lowercase, that is either:
		/// <list type="bullet">
		/// <item><see cref="DecimalLiteralSuffix.FloatLower"/></item>
		/// <item><see cref="DecimalLiteralSuffix.DoubleLower"/></item>
		/// <item><see cref="DecimalLiteralSuffix.DecimalLower"/></item>
		/// </list>
		/// </summary>
		/// <param name="value"><see cref="DecimalLiteralSuffix"/> to determine whether is considered to be all-lowercase.</param>
		public static bool IsLower(this DecimalLiteralSuffix value)
		{
			return value is DecimalLiteralSuffix.FloatLower or DecimalLiteralSuffix.DoubleLower or DecimalLiteralSuffix.DecimalLower;
		}

		/// <summary>
		/// Determines whether the specified <see cref="NumericLiteralSuffix"/> <paramref name="value"/> is considered to be all-lowercase, that is either:
		/// <list type="bullet">
		/// <item><see cref="NumericLiteralSuffix.LongLower"/></item>
		/// <item><see cref="NumericLiteralSuffix.LongLowerUnsignedLower"/></item>
		/// <item><see cref="NumericLiteralSuffix.UnsignedLower"/></item>
		/// <item><see cref="NumericLiteralSuffix.UnsignedLowerLongLower"/></item>
		/// <item><see cref="NumericLiteralSuffix.FloatLower"/></item>
		/// <item><see cref="NumericLiteralSuffix.DoubleLower"/></item>
		/// <item><see cref="NumericLiteralSuffix.DecimalLower"/></item>
		/// </list>
		/// </summary>
		/// <param name="value"><see cref="NumericLiteralSuffix"/> to determine whether is considered to be all-lowercase.</param>
		public static bool IsLower(this NumericLiteralSuffix value)
		{
			return value is
				NumericLiteralSuffix.LongLower or
				NumericLiteralSuffix.LongLowerUnsignedLower or
				NumericLiteralSuffix.UnsignedLower or
				NumericLiteralSuffix.UnsignedLowerLongLower or
				NumericLiteralSuffix.FloatLower or
				NumericLiteralSuffix.DoubleLower or
				NumericLiteralSuffix.DecimalLower;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="accessor"/> is a property accessor.
		/// </summary>
		/// <param name="accessor"><see cref="Accessor"/> to determine whether is a property accessor.</param>
		public static bool IsPropertyAccessor(this Accessor accessor)
		{
			return accessor is Accessor.Get or Accessor.Set or Accessor.Init;
		}

		/// <summary>
		/// Determines whether the given <see cref="DecimalLiteralSuffix"/> <paramref name="value"/> is considered to be of the specified floating-point <paramref name="type"/>.
		/// </summary>
		/// <param name="value"><see cref="DecimalLiteralSuffix"/> to determine whether is considered to be of the specified floating-point <paramref name="type"/>.</param>
		/// <param name="type">Floating-point type to check against.</param>
		public static bool IsType(this DecimalLiteralSuffix value, DecimalValueType type)
		{
			return type switch
			{
				DecimalValueType.Float => value is DecimalLiteralSuffix.FloatLower or DecimalLiteralSuffix.FloatUpper,
				DecimalValueType.Double => value is DecimalLiteralSuffix.DoubleLower or DecimalLiteralSuffix.DoubleUpper,
				DecimalValueType.Decimal => value is DecimalLiteralSuffix.DecimalLower or DecimalLiteralSuffix.DecimalUpper,
				_ => false
			};
		}

		/// <summary>
		/// Determines whether the specified <see cref="NumericLiteralSuffix"/> <paramref name="value"/> is considered to be unsigned, that is either:
		/// <list type="bullet">
		/// <item><see cref="NumericLiteralSuffix.UnsignedLower"/></item>
		/// <item><see cref="NumericLiteralSuffix.UnsignedLowerLongLower"/></item>
		/// <item><see cref="NumericLiteralSuffix.UnsignedLowerLongUpper"/></item>
		/// <item><see cref="NumericLiteralSuffix.UnsignedUpper"/></item>
		/// <item><see cref="NumericLiteralSuffix.UnsignedUpperLongLower"/></item>
		/// <item><see cref="NumericLiteralSuffix.UnsignedUpperLongUpper"/></item>
		/// <item><see cref="NumericLiteralSuffix.LongLowerUnsignedLower"/></item>
		/// <item><see cref="NumericLiteralSuffix.LongLowerUnsignedUpper"/></item>
		/// <item><see cref="NumericLiteralSuffix.LongUpperUnsignedLower"/></item>
		/// <item><see cref="NumericLiteralSuffix.LongUpperUnsignedUpper"/></item>
		/// </list>
		/// </summary>
		/// <param name="value"><see cref="NumericLiteralSuffix"/> to determine whether is considered to be unsigned.</param>
		public static bool IsUnsigned(this NumericLiteralSuffix value)
		{
			return value is NumericLiteralSuffix.UnsignedLower or NumericLiteralSuffix.UnsignedUpper || value.IsUnsignedLong();
		}

		/// <summary>
		/// Determines whether the specified <see cref="NumericLiteralSuffix"/> value is considered to be unsigned-first, that is either:
		/// <list type="bullet">
		/// <item><see cref="NumericLiteralSuffix.UnsignedLower"/></item>
		/// <item><see cref="NumericLiteralSuffix.UnsignedLowerLongLower"/></item>
		/// <item><see cref="NumericLiteralSuffix.UnsignedLowerLongUpper"/></item>
		/// <item><see cref="NumericLiteralSuffix.UnsignedUpper"/></item>
		/// <item><see cref="NumericLiteralSuffix.UnsignedUpperLongLower"/></item>
		/// <item><see cref="NumericLiteralSuffix.UnsignedUpperLongUpper"/></item>
		/// </list>
		/// </summary>
		/// <param name="value"><see cref="NumericLiteralSuffix"/> value to determine whether is considered to be unsigned-first.</param>
		public static bool IsUnsignedFirst(this NumericLiteralSuffix value)
		{
			return value >= NumericLiteralSuffix.UnsignedLower && value <= NumericLiteralSuffix.UnsignedUpperLongUpper;
		}

		/// <summary>
		/// Determines whether the specified <see cref="NumericLiteralSuffix"/> <paramref name="value"/> is considered to be an <see cref="ulong"/>, that is either:
		/// <list type="bullet">
		/// <item><see cref="NumericLiteralSuffix.LongLowerUnsignedLower"/></item>
		/// <item><see cref="NumericLiteralSuffix.LongLowerUnsignedUpper"/></item>
		/// <item><see cref="NumericLiteralSuffix.LongUpperUnsignedLower"/></item>
		/// <item><see cref="NumericLiteralSuffix.LongUpperUnsignedUpper"/></item>
		/// <item><see cref="NumericLiteralSuffix.UnsignedLowerLongLower"/></item>
		/// <item><see cref="NumericLiteralSuffix.UnsignedLowerLongUpper"/></item>
		/// <item><see cref="NumericLiteralSuffix.UnsignedUpperLongLower"/></item>
		/// <item><see cref="NumericLiteralSuffix.UnsignedUpperLongUpper"/></item>
		/// </list>
		/// </summary>
		/// <param name="value"><see cref="NumericLiteralSuffix"/> to determine whether is considered to be an <see cref="ulong"/>.</param>
		public static bool IsUnsignedLong(this NumericLiteralSuffix value)
		{
			return value is
				NumericLiteralSuffix.LongLowerUnsignedLower or
				NumericLiteralSuffix.LongLowerUnsignedUpper or
				NumericLiteralSuffix.LongUpperUnsignedLower or
				NumericLiteralSuffix.LongUpperUnsignedUpper or
				NumericLiteralSuffix.UnsignedLowerLongLower or
				NumericLiteralSuffix.UnsignedLowerLongUpper or
				NumericLiteralSuffix.UnsignedUpperLongLower or
				NumericLiteralSuffix.UnsignedUpperLongUpper;
		}

		/// <summary>
		/// Determines whether the specified <see cref="NumericLiteralPrefix"/> <paramref name="value"/> is considered to be all-uppercase, that is either:
		/// <list type="bullet">
		/// <item><see cref="NumericLiteralPrefix.BinaryUpper"/></item>
		/// <item><see cref="NumericLiteralPrefix.HexadecimalUpper"/></item>
		/// </list>
		/// </summary>
		/// <param name="value"><see cref="NumericLiteralPrefix"/> to determine whether is considered to be all-uppercase.</param>
		public static bool IsUpper(this NumericLiteralPrefix value)
		{
			return value is NumericLiteralPrefix.BinaryUpper or NumericLiteralPrefix.HexadecimalUpper;
		}

		/// <summary>
		/// Determines whether the specified <see cref="DecimalLiteralSuffix"/> <paramref name="value"/> is considered to be all-uppercase, that is either:
		/// <list type="bullet">
		/// <item><see cref="DecimalLiteralSuffix.FloatUpper"/></item>
		/// <item><see cref="DecimalLiteralSuffix.DoubleUpper"/></item>
		/// <item><see cref="DecimalLiteralSuffix.DecimalUpper"/></item>
		/// </list>
		/// </summary>
		/// <param name="value"><see cref="DecimalLiteralSuffix"/> to determine whether is considered to be all-uppercase.</param>
		public static bool IsUpper(this DecimalLiteralSuffix value)
		{
			return value is DecimalLiteralSuffix.FloatUpper or DecimalLiteralSuffix.DoubleUpper or DecimalLiteralSuffix.DecimalUpper;
		}

		/// <summary>
		/// Determines whether the specified <see cref="NumericLiteralSuffix"/> <paramref name="value"/> is considered to be all-uppercase, that is either:
		/// <list type="bullet">
		/// <item><see cref="NumericLiteralSuffix.LongUpper"/></item>
		/// <item><see cref="NumericLiteralSuffix.LongUpperUnsignedUpper"/></item>
		/// <item><see cref="NumericLiteralSuffix.UnsignedUpper"/></item>
		/// <item><see cref="NumericLiteralSuffix.UnsignedUpperLongUpper"/></item>
		/// <item><see cref="NumericLiteralSuffix.FloatUpper"/></item>
		/// <item><see cref="NumericLiteralSuffix.DoubleUpper"/></item>
		/// <item><see cref="NumericLiteralSuffix.DecimalUpper"/></item>
		/// </list>
		/// </summary>
		/// <param name="value"><see cref="NumericLiteralSuffix"/> to determine whether is considered to be all-uppercase.</param>
		public static bool IsUpper(this NumericLiteralSuffix value)
		{
			return value is
				NumericLiteralSuffix.LongUpper or
				NumericLiteralSuffix.LongUpperUnsignedUpper or
				NumericLiteralSuffix.UnsignedUpper or
				NumericLiteralSuffix.UnsignedUpperLongUpper or
				NumericLiteralSuffix.FloatUpper or
				NumericLiteralSuffix.DoubleUpper or
				NumericLiteralSuffix.DecimalUpper;
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

		/// <summary>
		/// Converts the specified <see cref="NumericLiteralSuffix"/> value to its variant with the long suffix ('l' or 'L') before the unsigned suffix ('u' or 'U').
		/// </summary>
		/// <param name="value"><see cref="NumericLiteralSuffix"/> value to convert to its unsigned-first variant.</param>
		public static NumericLiteralSuffix LongFirst(this NumericLiteralSuffix value)
		{
			return value switch
			{
				NumericLiteralSuffix.UnsignedLowerLongLower => NumericLiteralSuffix.LongLowerUnsignedLower,
				NumericLiteralSuffix.UnsignedLowerLongUpper => NumericLiteralSuffix.LongUpperUnsignedLower,
				NumericLiteralSuffix.UnsignedUpperLongLower => NumericLiteralSuffix.LongLowerUnsignedUpper,
				NumericLiteralSuffix.UnsignedUpperLongUpper => NumericLiteralSuffix.LongUpperUnsignedUpper,
				_ => value
			};
		}

		/// <summary>
		/// Converts the specified <see cref="DecimalLiteralSuffix"/> <paramref name="value"/> into its all-lowercase variant.
		/// </summary>
		/// <param name="value"><see cref="DecimalLiteralSuffix"/> value to convert into its all-lowercase variant.</param>
		public static DecimalLiteralSuffix ToLower(this DecimalLiteralSuffix value)
		{
			return (DecimalLiteralSuffix)ToLower((int)value, (int)DecimalLiteralSuffix.DecimalUpper);
		}

		/// <summary>
		/// Converts the specified <see cref="NumericLiteralPrefix"/> <paramref name="value"/> into its all-lowercase variant.
		/// </summary>
		/// <param name="value"><see cref="NumericLiteralPrefix"/> value to convert into its all-lowercase variant.</param>
		public static NumericLiteralPrefix ToLower(this NumericLiteralPrefix value)
		{
			return (NumericLiteralPrefix)ToLower((int)value, (int)NumericLiteralPrefix.BinaryUpper);
		}

		/// <summary>
		/// Converts the specified <see cref="NumericLiteralSuffix"/> <paramref name="value"/> into its all-lowercase variant.
		/// </summary>
		/// <param name="value"><see cref="NumericLiteralSuffix"/> value to convert into its all-lowercase variant.</param>
		public static NumericLiteralSuffix ToLower(this NumericLiteralSuffix value)
		{
#pragma warning disable IDE0066 // Convert switch statement to expression
			switch (value)
			{
				case NumericLiteralSuffix.LongLower:
				case NumericLiteralSuffix.UnsignedLower:
				case NumericLiteralSuffix.LongLowerUnsignedLower:
				case NumericLiteralSuffix.UnsignedLowerLongLower:
				case NumericLiteralSuffix.FloatLower:
				case NumericLiteralSuffix.DecimalLower:
				case NumericLiteralSuffix.DoubleLower:
					return value;

				case NumericLiteralSuffix.UnsignedUpper:
					return NumericLiteralSuffix.UnsignedLower;

				case NumericLiteralSuffix.LongUpper:
					return NumericLiteralSuffix.LongLower;

				case NumericLiteralSuffix.LongUpperUnsignedLower:
				case NumericLiteralSuffix.LongUpperUnsignedUpper:
				case NumericLiteralSuffix.LongLowerUnsignedUpper:
					return NumericLiteralSuffix.LongLowerUnsignedLower;

				case NumericLiteralSuffix.UnsignedUpperLongLower:
				case NumericLiteralSuffix.UnsignedUpperLongUpper:
				case NumericLiteralSuffix.UnsignedLowerLongUpper:
					return NumericLiteralSuffix.UnsignedLowerLongLower;

				case NumericLiteralSuffix.DecimalUpper:
					return NumericLiteralSuffix.DecimalLower;

				case NumericLiteralSuffix.DoubleUpper:
					return NumericLiteralSuffix.DoubleLower;

				case NumericLiteralSuffix.FloatUpper:
					return NumericLiteralSuffix.FloatLower;

				default:
					return default;
			}
#pragma warning restore IDE0066 // Convert switch statement to expression
		}

		/// <summary>
		/// Converts the specified <see cref="DecimalLiteralSuffix"/> <paramref name="value"/> into its all-uppercase variant.
		/// </summary>
		/// <param name="value"><see cref="DecimalLiteralSuffix"/> value to convert into its all-uppercase variant.</param>
		public static DecimalLiteralSuffix ToUpper(this DecimalLiteralSuffix value)
		{
			return (DecimalLiteralSuffix)ToUpper((int)value, (int)DecimalLiteralSuffix.DecimalUpper);
		}

		/// <summary>
		/// Converts the specified <see cref="NumericLiteralPrefix"/> <paramref name="value"/> into its all-uppercase variant.
		/// </summary>
		/// <param name="value"><see cref="NumericLiteralPrefix"/> value to convert into its all-uppercase variant.</param>
		public static NumericLiteralPrefix ToUpper(this NumericLiteralPrefix value)
		{
			return (NumericLiteralPrefix)ToUpper((int)value, (int)NumericLiteralPrefix.BinaryUpper);
		}

		/// <summary>
		/// Converts the specified <see cref="NumericLiteralSuffix"/> <paramref name="value"/> into its all-uppercase variant.
		/// </summary>
		/// <param name="value"><see cref="NumericLiteralSuffix"/> value to convert into its all-uppercase variant.</param>
		public static NumericLiteralSuffix ToUpper(this NumericLiteralSuffix value)
		{
#pragma warning disable IDE0066 // Convert switch statement to expression
			switch (value)
			{
				case NumericLiteralSuffix.LongUpper:
				case NumericLiteralSuffix.UnsignedUpper:
				case NumericLiteralSuffix.LongUpperUnsignedUpper:
				case NumericLiteralSuffix.UnsignedUpperLongUpper:
				case NumericLiteralSuffix.FloatUpper:
				case NumericLiteralSuffix.DecimalUpper:
				case NumericLiteralSuffix.DoubleUpper:
					return value;

				case NumericLiteralSuffix.UnsignedLower:
					return NumericLiteralSuffix.UnsignedUpper;

				case NumericLiteralSuffix.LongLower:
					return NumericLiteralSuffix.LongUpper;

				case NumericLiteralSuffix.LongLowerUnsignedLower:
				case NumericLiteralSuffix.LongLowerUnsignedUpper:
				case NumericLiteralSuffix.LongUpperUnsignedLower:
					return NumericLiteralSuffix.LongUpperUnsignedUpper;

				case NumericLiteralSuffix.UnsignedLowerLongLower:
				case NumericLiteralSuffix.UnsignedLowerLongUpper:
				case NumericLiteralSuffix.UnsignedUpperLongLower:
					return NumericLiteralSuffix.UnsignedUpperLongUpper;

				case NumericLiteralSuffix.DecimalLower:
					return NumericLiteralSuffix.DecimalUpper;

				case NumericLiteralSuffix.DoubleLower:
					return NumericLiteralSuffix.DoubleLower;

				case NumericLiteralSuffix.FloatLower:
					return NumericLiteralSuffix.FloatUpper;

				default:
					return default;
			}
#pragma warning restore IDE0066 // Convert switch statement to expression
		}

		/// <summary>
		/// Converts the specified <see cref="NumericLiteralSuffix"/> value to its variant with the unsigned suffix ('u' or 'U') before the long suffix ('l' or 'L').
		/// </summary>
		/// <param name="value"><see cref="NumericLiteralSuffix"/> value to convert to its unsigned-first variant.</param>
		public static NumericLiteralSuffix UnsignedFirst(this NumericLiteralSuffix value)
		{
			return value switch
			{
				NumericLiteralSuffix.LongLowerUnsignedLower => NumericLiteralSuffix.UnsignedLowerLongLower,
				NumericLiteralSuffix.LongLowerUnsignedUpper => NumericLiteralSuffix.UnsignedUpperLongLower,
				NumericLiteralSuffix.LongUpperUnsignedLower => NumericLiteralSuffix.UnsignedLowerLongUpper,
				NumericLiteralSuffix.LongUpperUnsignedUpper => NumericLiteralSuffix.UnsignedUpperLongUpper,
				_ => value
			};
		}

		private static int ToLower(int value, int maxValue)
		{
			if (value <= 0 || value > maxValue)
			{
				return default;
			}

			if (value % 2 == 0)
			{
				return value - 1;
			}

			return value;
		}

		private static int ToUpper(int value, int maxValue)
		{
			if (value <= 0 || value > maxValue)
			{
				return default;
			}

			if (value % 2 == 0)
			{
				return value;
			}

			return value + 1;
		}
	}
}
