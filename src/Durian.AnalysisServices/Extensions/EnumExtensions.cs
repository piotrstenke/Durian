// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Durian.Analysis.CodeGeneration;
using Durian.Analysis.Logging;
using Durian.Analysis.SymbolContainers;
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
		/// Converts the specified <paramref name="value"/> to an associated <see cref="Accessibility"/> value.
		/// </summary>
		/// <param name="value"><see cref="SyntaxKind"/> to convert.</param>
		public static Accessibility GetAccessibility(this SyntaxKind value)
		{
			return value switch
			{
				SyntaxKind.PublicKeyword => Accessibility.Public,
				SyntaxKind.PrivateKeyword => Accessibility.Private,
				SyntaxKind.ProtectedKeyword => Accessibility.Protected,
				SyntaxKind.InternalKeyword => Accessibility.Internal,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="AccessorKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="SyntaxKind"/> to convert.</param>
		public static AccessorKind GetAccessorKind(this SyntaxKind value)
		{
			return value switch
			{
				SyntaxKind.GetKeyword or SyntaxKind.GetAccessorDeclaration => AccessorKind.Get,
				SyntaxKind.SetKeyword or SyntaxKind.SetAccessorDeclaration => AccessorKind.Set,
				SyntaxKind.InitKeyword or SyntaxKind.InitAccessorDeclaration => AccessorKind.Init,
				SyntaxKind.AddKeyword or SyntaxKind.AddAccessorDeclaration => AccessorKind.Add,
				SyntaxKind.RemoveKeyword or SyntaxKind.RemoveAccessorDeclaration => AccessorKind.Remove,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="AccessorKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="MethodKind"/> to convert.</param>
		public static AccessorKind GetAccessorKind(this MethodKind value)
		{
			return value switch
			{
				MethodKind.PropertyGet => AccessorKind.Get,
				MethodKind.PropertySet => AccessorKind.Set,
				MethodKind.EventAdd => AccessorKind.Add,
				MethodKind.EventRemove => AccessorKind.Remove,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="AccessorKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="PropertyAccessorKind"/> to convert.</param>
		public static AccessorKind GetAccessorKind(this PropertyAccessorKind value)
		{
			int n = (int)value;

			if (n <= (int)AccessorKind.Init)
			{
				return (AccessorKind)value;
			}

			return default;
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="AccessorKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="EventAccessorKind"/> to convert.</param>
		public static AccessorKind GetAccessorKind(this EventAccessorKind value)
		{
			const int min = (int)AccessorKind.Init;

			if (value == default)
			{
				return default;
			}

			int n = (int)value + min;

			if (n <= min)
			{
				return default;
			}

			return (AccessorKind)n;
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="AccessorKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="AutoPropertyKind"/> to convert.</param>
		public static AccessorKind GetAccessorKind(this AutoPropertyKind value)
		{
			return value switch
			{
				AutoPropertyKind.GetOnly => AccessorKind.Get,
				AutoPropertyKind.SetOnly => AccessorKind.Set,
				AutoPropertyKind.InitOnly => AccessorKind.Init,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="AttributeTarget"/> value.
		/// </summary>
		/// <param name="value"><see cref="SyntaxKind"/> to convert.</param>
		public static AttributeTarget GetAttributeTarget(this SyntaxKind value)
		{
			return value switch
			{
				SyntaxKind.AssemblyKeyword => AttributeTarget.Assembly,
				SyntaxKind.ReturnKeyword => AttributeTarget.Return,
				SyntaxKind.FieldKeyword => AttributeTarget.Field,
				SyntaxKind.MethodKeyword => AttributeTarget.Method,
				SyntaxKind.EventKeyword => AttributeTarget.Event,
				SyntaxKind.PropertyKeyword => AttributeTarget.Property,
				SyntaxKind.ModuleKeyword => AttributeTarget.Module,
				SyntaxKind.ParamKeyword => AttributeTarget.Param,
				SyntaxKind.TypeKeyword => AttributeTarget.Type,
				SyntaxKind.TypeVarKeyword => AttributeTarget.TypeVar,
				_ => default
			};
		}

		/// <summary>
		/// Returns a keyword used to refer to a <see cref="ISymbol"/> of the specified <paramref name="kind"/> inside an attribute list.
		/// </summary>
		/// <param name="kind">Kind of method to get the keyword for.</param>
		/// <param name="targetKind">Determines which keyword to return when there is more than one option.</param>
		public static AttributeTarget GetAttributeTarget(this SymbolKind kind, AttributeTargetKind targetKind = default)
		{
			return kind switch
			{
				SymbolKind.NamedType => targetKind == AttributeTargetKind.Value ? AttributeTarget.Return : AttributeTarget.Type,
				SymbolKind.Field => AttributeTarget.Field,
				SymbolKind.Method => targetKind == AttributeTargetKind.Value ? AttributeTarget.Return : AttributeTarget.Method,
				SymbolKind.Property => targetKind == AttributeTargetKind.Value ? AttributeTarget.Field : AttributeTarget.Property,
				SymbolKind.Event => targetKind switch
				{
					AttributeTargetKind.This => AttributeTarget.Event,
					AttributeTargetKind.Value => AttributeTarget.Field,
					AttributeTargetKind.Handler => AttributeTarget.Method,
					_ => default
				},
				SymbolKind.TypeParameter => AttributeTarget.TypeVar,
				SymbolKind.Parameter => AttributeTarget.Param,
				SymbolKind.Assembly => AttributeTarget.Assembly,
				SymbolKind.NetModule => AttributeTarget.Module,
				_ => default
			};
		}

		/// <summary>
		/// Returns a keyword used to refer to a <see cref="IMethodSymbol"/> of the specified <paramref name="kind"/> inside an attribute list.
		/// </summary>
		/// <param name="kind">Kind of method to get the keyword for.</param>
		/// <param name="targetKind">Determines which keyword to return when there is more than one option.</param>
		public static AttributeTarget GetAttributeTarget(this MethodKind kind, AttributeTargetKind targetKind = default)
		{
			return kind switch
			{
				MethodKind.EventAdd or
				MethodKind.EventRemove or
				MethodKind.PropertySet => targetKind switch
				{
					AttributeTargetKind.Value => AttributeTarget.Return,
					AttributeTargetKind.Handler => AttributeTarget.Param,
					_ => AttributeTarget.Method
				},
				_ => targetKind == AttributeTargetKind.Value ? AttributeTarget.Return : AttributeTarget.Method
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="AutoPropertyKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="PropertyAccessorKind"/> to convert.</param>
		public static AutoPropertyKind GetAutoPropertyKind(this PropertyAccessorKind value)
		{
			return value switch
			{
				PropertyAccessorKind.Get => AutoPropertyKind.GetOnly,
				PropertyAccessorKind.Set => AutoPropertyKind.SetOnly,
				PropertyAccessorKind.Init => AutoPropertyKind.InitOnly,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="AutoPropertyKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="PropertyAccessorKind"/> to convert.</param>
		public static AutoPropertyKind GetAutoPropertyKind(this AccessorKind value)
		{
			return value switch
			{
				AccessorKind.Get => AutoPropertyKind.GetOnly,
				AccessorKind.Set => AutoPropertyKind.SetOnly,
				AccessorKind.Init => AutoPropertyKind.InitOnly,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="AutoPropertyKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="PropertyAccessorKind"/> to convert.</param>
		public static AutoPropertyKind GetAutoPropertyKind(this SyntaxKind value)
		{
			return value switch
			{
				SyntaxKind.GetKeyword or SyntaxKind.GetAccessorDeclaration => AutoPropertyKind.GetOnly,
				SyntaxKind.SetKeyword or SyntaxKind.SetAccessorDeclaration => AutoPropertyKind.SetOnly,
				SyntaxKind.InitKeyword or SyntaxKind.InitAccessorDeclaration => AutoPropertyKind.InitOnly,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="GenericConstraint"/> value.
		/// </summary>
		/// <param name="value"><see cref="SyntaxKind"/> to convert.</param>
		public static GenericConstraint GetConstraint(this SyntaxKind value)
		{
			return value switch
			{
				SyntaxKind.TypeConstraint => GenericConstraint.Type,
				SyntaxKind.ClassConstraint or SyntaxKind.ClassKeyword => GenericConstraint.Class,
				SyntaxKind.StructConstraint or SyntaxKind.StructConstraint => GenericConstraint.Struct,
				SyntaxKind.ConstructorConstraint or SyntaxKind.NewKeyword => GenericConstraint.New,
				SyntaxKind.UnmanagedKeyword => GenericConstraint.Unmanaged,
				SyntaxKind.DefaultConstraint or SyntaxKind.DefaultKeyword => GenericConstraint.Default,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="ConstructorInitializer"/> value.
		/// </summary>
		/// <param name="value"><see cref="SyntaxKind"/> to convert.</param>
		public static ConstructorInitializer GetConstructorInitializer(this SyntaxKind value)
		{
			return value switch
			{
				SyntaxKind.ThisKeyword or SyntaxKind.ThisConstructorInitializer => ConstructorInitializer.This,
				SyntaxKind.BaseKeyword or SyntaxKind.BaseConstructorInitializer => ConstructorInitializer.Base,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="DecimalLiteralSuffix"/> value.
		/// </summary>
		/// <param name="value"><see cref="NumericLiteralSuffix"/> to convert.</param>
		public static DecimalLiteralSuffix GetDecimalSuffix(this NumericLiteralSuffix value)
		{
			int number = (int)value - (int)NumericLiteralSuffix.LongUpperUnsignedUpper;

			if (number < 0)
			{
				return default;
			}

			return (DecimalLiteralSuffix)number;
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="DecimalLiteralSuffix"/> value.
		/// </summary>
		/// <remarks>The returned value is considered uppercase (see <see cref="IsUpper(DecimalLiteralSuffix)"/> for more details).</remarks>
		/// <param name="value"><see cref="SpecialType"/> to convert.</param>
		public static DecimalLiteralSuffix GetDecimalSuffix(this SpecialType value)
		{
			return value switch
			{
				SpecialType.System_Single => DecimalLiteralSuffix.FloatUpper,
				SpecialType.System_Double => DecimalLiteralSuffix.DoubleUpper,
				SpecialType.System_Decimal => DecimalLiteralSuffix.DecimalUpper,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="DecimalLiteralSuffix"/> value.
		/// </summary>
		/// <remarks>The returned value is considered uppercase (see <see cref="IsUpper(DecimalLiteralSuffix)"/> for more details).</remarks>
		/// <param name="value"><see cref="TypeKeyword"/> to convert.</param>
		public static DecimalLiteralSuffix GetDecimalSuffix(this TypeKeyword value)
		{
			return value switch
			{
				TypeKeyword.Float => DecimalLiteralSuffix.FloatUpper,
				TypeKeyword.Double => DecimalLiteralSuffix.DoubleUpper,
				TypeKeyword.Decimal => DecimalLiteralSuffix.DecimalUpper,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="DecimalValueType"/> value.
		/// </summary>
		/// <param name="value"><see cref="SpecialType"/> to convert.</param>
		public static DecimalValueType GetDecimalType(this SpecialType value)
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
		/// <param name="value"><see cref="TypeKeyword"/> to convert.</param>
		public static DecimalValueType GetDecimalType(this TypeKeyword value)
		{
			return value switch
			{
				TypeKeyword.Float => DecimalValueType.Float,
				TypeKeyword.Double => DecimalValueType.Double,
				TypeKeyword.Decimal => DecimalValueType.Decimal,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="DecimalValueType"/> value.
		/// </summary>
		/// <param name="value"><see cref="SyntaxKind"/> to convert.</param>
		public static DecimalValueType GetDecimalType(this SyntaxKind value)
		{
			return value switch
			{
				SyntaxKind.FloatKeyword => DecimalValueType.Float,
				SyntaxKind.DoubleKeyword => DecimalValueType.Double,
				SyntaxKind.DecimalKeyword => DecimalValueType.Decimal,
				_ => default
			};
		}

		/// <summary>
		/// Returns the value that is considered default for the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="TypeKeyword"/> to get the default value of.</param>
		public static object? GetDefaultValue(this TypeKeyword type)
		{
			return type switch
			{
				TypeKeyword.Short => default(short),
				TypeKeyword.Int => default(int),
				TypeKeyword.Long => default(long),
				TypeKeyword.UShort => default(ushort),
				TypeKeyword.UInt => default(uint),
				TypeKeyword.ULong => default(ulong),
				TypeKeyword.Byte => default(byte),
				TypeKeyword.SByte => default(sbyte),
				TypeKeyword.Float => default(float),
				TypeKeyword.Double => default(double),
				TypeKeyword.Decimal => default(decimal),
				TypeKeyword.Char => default(char),
				TypeKeyword.String => default(string),
				TypeKeyword.Bool => default(bool),
				TypeKeyword.NInt => default(nint),
				TypeKeyword.NUInt => default(nuint),
				TypeKeyword.Object => default(object),
				TypeKeyword.Dynamic => default(dynamic),
				// TypeKeyword.Void => default,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="EventAccessorKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="SyntaxKind"/> to convert.</param>
		public static EventAccessorKind GetEventAccessorKind(this SyntaxKind value)
		{
			return value switch
			{
				SyntaxKind.AddKeyword or SyntaxKind.AddAccessorDeclaration => EventAccessorKind.Add,
				SyntaxKind.RemoveKeyword or SyntaxKind.RemoveAccessorDeclaration => EventAccessorKind.Remove,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="EventAccessorKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="AccessorKind"/> to convert.</param>
		public static EventAccessorKind GetEventAccessorKind(this AccessorKind value)
		{
			if (value == default)
			{
				return default;
			}

			return (EventAccessorKind)(value - (int)AccessorKind.Init);
		}

		/// <summary>
		/// Returns an array of all flags contained within the specified <see cref="GenericConstraint"/> <paramref name="value"/>.
		/// </summary>
		/// <param name="value"><see cref="GenericConstraint"/> to get the flags of.</param>
		public static GenericConstraint[] GetFlags(this GenericConstraint value)
		{
			ReadOnlySpan<GenericConstraint> all = stackalloc GenericConstraint[]
			{
				GenericConstraint.Type,
				GenericConstraint.New,
				GenericConstraint.Class,
				GenericConstraint.Struct,
				GenericConstraint.Unmanaged,
				GenericConstraint.NotNull,
				GenericConstraint.Default
			};

			List<GenericConstraint> list = new(all.Length);

			foreach (GenericConstraint element in all)
			{
				if (value.HasFlag(element))
				{
					list.Add(element);
				}
			}

			return list.ToArray();
		}

		/// <summary>
		/// Returns an array of all flags contained within the specified <see cref="EnumStyle"/> <paramref name="value"/>.
		/// </summary>
		/// <param name="value"><see cref="EnumStyle"/> to get the flags of.</param>
		public static EnumStyle[] GetFlags(this EnumStyle value)
		{
			ReadOnlySpan<EnumStyle> all = stackalloc EnumStyle[]
			{
				EnumStyle.ExplicitValues,
				EnumStyle.ExplicitInt32
			};

			List<EnumStyle> list = new(all.Length);

			foreach (EnumStyle element in all)
			{
				if (value.HasFlag(element))
				{
					list.Add(element);
				}
			}

			return list.ToArray();
		}

		/// <summary>
		/// Returns an array of all flags contained within the specified <see cref="GeneratorLogs"/> <paramref name="value"/>.
		/// </summary>
		/// <param name="value"><see cref="GeneratorLogs"/> to get the flags of.</param>
		public static GeneratorLogs[] GetFlags(this GeneratorLogs value)
		{
			ReadOnlySpan<GeneratorLogs> all = stackalloc GeneratorLogs[]
			{
				GeneratorLogs.Exception,
				GeneratorLogs.InputOutput,
				GeneratorLogs.Node,
				GeneratorLogs.Diagnostics
			};

			List<GeneratorLogs> list = new(all.Length);

			foreach (GeneratorLogs element in all)
			{
				if (value.HasFlag(element))
				{
					list.Add(element);
				}
			}

			return list.ToArray();
		}

		/// <summary>
		/// Returns an array of all flags contained within the specified <see cref="InterfaceMemberStyle"/> <paramref name="value"/>.
		/// </summary>
		/// <param name="value"><see cref="InterfaceMemberStyle"/> to get the flags of.</param>
		public static InterfaceMemberStyle[] GetFlags(this InterfaceMemberStyle value)
		{
			ReadOnlySpan<InterfaceMemberStyle> all = stackalloc InterfaceMemberStyle[]
			{
				InterfaceMemberStyle.ExplicitVirtual,
				InterfaceMemberStyle.ExplicitAccess
			};

			List<InterfaceMemberStyle> list = new(all.Length);

			foreach (InterfaceMemberStyle element in all)
			{
				if (value.HasFlag(element))
				{
					list.Add(element);
				}
			}

			return list.ToArray();
		}

		/// <summary>
		/// Returns an array of all flags contained within the specified <see cref="OperatorRequirements"/> <paramref name="value"/>.
		/// </summary>
		/// <param name="value"><see cref="OperatorRequirements"/> to get the flags of.</param>
		public static OperatorRequirements[] GetFlags(this OperatorRequirements value)
		{
			ReadOnlySpan<OperatorRequirements> all = stackalloc OperatorRequirements[]
			{
				OperatorRequirements.MatchingOperator,
				OperatorRequirements.IntOperand,
				OperatorRequirements.ReturnBoolean,
				OperatorRequirements.ReturnParameterType
			};

			List<OperatorRequirements> list = new(all.Length);

			foreach (OperatorRequirements element in all)
			{
				if (value.HasFlag(element))
				{
					list.Add(element);
				}
			}

			return list.ToArray();
		}

		/// <summary>
		/// Returns an array of all flags contained within the specified <see cref="RecordStyle"/> <paramref name="value"/>.
		/// </summary>
		/// <param name="value"><see cref="RecordStyle"/> to get the flags of.</param>
		public static RecordStyle[] GetFlags(this RecordStyle value)
		{
			ReadOnlySpan<RecordStyle> all = stackalloc RecordStyle[]
			{
				RecordStyle.ExplicitClass,
				RecordStyle.PrimaryConstructor
			};

			List<RecordStyle> list = new(all.Length);

			foreach (RecordStyle element in all)
			{
				if (value.HasFlag(element))
				{
					list.Add(element);
				}
			}

			return list.ToArray();
		}

		/// <summary>
		/// Returns an array of all flags contained within the specified <see cref="StringModifiers"/> <paramref name="value"/>.
		/// </summary>
		/// <param name="value"><see cref="StringModifiers"/> to get the flags of.</param>
		public static StringModifiers[] GetFlags(this StringModifiers value)
		{
			ReadOnlySpan<StringModifiers> all = stackalloc StringModifiers[]
			{
				StringModifiers.Verbatim,
				StringModifiers.Interpolation
			};

			List<StringModifiers> list = new(all.Length);

			foreach (StringModifiers element in all)
			{
				if (value.HasFlag(element))
				{
					list.Add(element);
				}
			}

			return list.ToArray();
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="GoToKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="SyntaxKind"/> to convert.</param>
		public static GoToKind GetGoToKind(this SyntaxKind value)
		{
			return value switch
			{
				SyntaxKind.GotoStatement or SyntaxKind.GotoKeyword => GoToKind.Label,
				SyntaxKind.GotoCaseStatement => GoToKind.Case,
				SyntaxKind.GotoDefaultStatement => GoToKind.DefaultCase,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="IntegerValueType"/> value.
		/// </summary>
		/// <param name="value"><see cref="SpecialType"/> to convert.</param>
		public static IntegerValueType GetIntegerType(this SpecialType value)
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
		/// <param name="value"><see cref="TypeKeyword"/> to convert.</param>
		public static IntegerValueType GetIntegerType(this TypeKeyword value)
		{
			int n = (int)value;

			if (n <= 0 || n > (int)TypeKeyword.SByte)
			{
				return default;
			}

			return (IntegerValueType)n;
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="IntegerValueType"/> value.
		/// </summary>
		/// <param name="value"><see cref="SyntaxKind"/> to convert.</param>
		public static IntegerValueType GetIntegerType(this SyntaxKind value)
		{
			return value switch
			{
				SyntaxKind.IntKeyword => IntegerValueType.Int32,
				SyntaxKind.LongKeyword => IntegerValueType.Int64,
				SyntaxKind.ShortKeyword => IntegerValueType.Int16,
				SyntaxKind.ByteKeyword => IntegerValueType.Byte,
				SyntaxKind.ULongKeyword => IntegerValueType.UInt64,
				SyntaxKind.UIntKeyword => IntegerValueType.UInt32,
				SyntaxKind.SByteKeyword => IntegerValueType.SByte,
				SyntaxKind.UShortKeyword => IntegerValueType.UInt16,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="TypeKeyword"/> value.
		/// </summary>
		/// <param name="value"><see cref="IntegerValueType"/> to convert.</param>
		public static TypeKeyword GetKeyword(this IntegerValueType value)
		{
			return value switch
			{
				IntegerValueType.Int16 => TypeKeyword.Short,
				IntegerValueType.Int32 => TypeKeyword.Int,
				IntegerValueType.Int64 => TypeKeyword.Long,
				IntegerValueType.UInt16 => TypeKeyword.UShort,
				IntegerValueType.UInt32 => TypeKeyword.UInt,
				IntegerValueType.UInt64 => TypeKeyword.ULong,
				IntegerValueType.Byte => TypeKeyword.Byte,
				IntegerValueType.SByte => TypeKeyword.SByte,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="TypeKeyword"/> value.
		/// </summary>
		/// <param name="value"><see cref="DecimalValueType"/> to convert.</param>
		public static TypeKeyword GetKeyword(this DecimalValueType value)
		{
			return value switch
			{
				DecimalValueType.Float => TypeKeyword.Float,
				DecimalValueType.Double => TypeKeyword.Double,
				DecimalValueType.Decimal => TypeKeyword.Decimal,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="TypeKeyword"/> value.
		/// </summary>
		/// <param name="value"><see cref="SpecialType"/> to convert.</param>
		public static TypeKeyword GetKeyword(this SpecialType value)
		{
			return value switch
			{
				SpecialType.System_Int16 => TypeKeyword.Short,
				SpecialType.System_Int32 => TypeKeyword.Int,
				SpecialType.System_Int64 => TypeKeyword.Long,
				SpecialType.System_UInt16 => TypeKeyword.UShort,
				SpecialType.System_UInt32 => TypeKeyword.UInt,
				SpecialType.System_UInt64 => TypeKeyword.ULong,
				SpecialType.System_Byte => TypeKeyword.Byte,
				SpecialType.System_SByte => TypeKeyword.SByte,
				SpecialType.System_Single => TypeKeyword.Float,
				SpecialType.System_Double => TypeKeyword.Double,
				SpecialType.System_Decimal => TypeKeyword.Decimal,
				SpecialType.System_String => TypeKeyword.String,
				SpecialType.System_Char => TypeKeyword.Char,
				SpecialType.System_Boolean => TypeKeyword.Bool,
				SpecialType.System_Object => TypeKeyword.Object,
				SpecialType.System_IntPtr => TypeKeyword.NInt,
				SpecialType.System_UIntPtr => TypeKeyword.NUInt,
				SpecialType.System_Void => TypeKeyword.Void,
				_ => default
			};
		}

		/// <summary>
		/// Returns a <see cref="string"/> representation of a C# keyword associated with the specified <paramref name="specialType"/> value.
		/// </summary>
		/// <param name="specialType">Value of <see cref="SpecialType"/> to get the C# keyword associated with.</param>
		public static string? GetKeywordText(this SpecialType specialType)
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
		/// Converts the specified <paramref name="value"/> to an associated <see cref="LambdaStyle"/> value.
		/// </summary>
		/// <param name="value"><see cref="MethodStyle"/> to convert.</param>
		public static LambdaStyle GetLambdaStyle(this MethodStyle value)
		{
			if ((int)value == (int)LambdaStyle.Method)
			{
				return default;
			}

			return (LambdaStyle)value;
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="LiteralKind"/> value.
		/// </summary>
		/// <remarks><b>Note:</b> if the <paramref name="value"/> is <see cref="SyntaxKind.BoolKeyword"/>, <see cref="LiteralKind.False"/> is returned.</remarks>
		/// <param name="value"><see cref="SyntaxKind"/> to convert.</param>
		public static LiteralKind GetLiteralKind(this SyntaxKind value)
		{
			return value switch
			{
				SyntaxKind.NumericLiteralExpression or
				SyntaxKind.NumericLiteralToken or
				SyntaxKind.IntKeyword or
				SyntaxKind.UIntKeyword or
				SyntaxKind.LongKeyword or
				SyntaxKind.ULongKeyword or
				SyntaxKind.ShortKeyword or
				SyntaxKind.UShortKeyword or
				SyntaxKind.ByteKeyword or
				SyntaxKind.SByteKeyword or
				SyntaxKind.FloatKeyword or
				SyntaxKind.DoubleKeyword or
				SyntaxKind.DecimalKeyword
					=> LiteralKind.Number,

				SyntaxKind.StringLiteralExpression or
				SyntaxKind.StringLiteralToken or
				SyntaxKind.InterpolatedStringExpression or
				SyntaxKind.InterpolatedStringToken or
				SyntaxKind.InterpolatedStringText or
				SyntaxKind.InterpolatedStringTextToken or
				SyntaxKind.InterpolatedStringStartToken or
				SyntaxKind.InterpolatedStringEndToken or
				SyntaxKind.InterpolatedVerbatimStringStartToken or
				SyntaxKind.StringKeyword or
				SyntaxKind.DoubleQuoteToken
					=> LiteralKind.String,

				SyntaxKind.CharacterLiteralExpression or
				SyntaxKind.CharacterLiteralToken or
				SyntaxKind.CharKeyword or
				SyntaxKind.SingleQuoteToken
					=> LiteralKind.Character,

				SyntaxKind.DefaultLiteralExpression or
				SyntaxKind.DefaultExpression or
				SyntaxKind.DefaultKeyword
					=> LiteralKind.Default,

				SyntaxKind.TrueLiteralExpression or
				SyntaxKind.TrueKeyword
					=> LiteralKind.True,

				SyntaxKind.FalseLiteralExpression or
				SyntaxKind.FalseKeyword or
				SyntaxKind.BoolKeyword
					=> LiteralKind.False,

				SyntaxKind.NullLiteralExpression or
				SyntaxKind.NullKeyword or
				SyntaxKind.ObjectKeyword
					=> LiteralKind.Null,

				SyntaxKind.ArgListExpression or
				SyntaxKind.ArgListKeyword
					=> LiteralKind.ArgList,

				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="LiteralKind"/> value.
		/// </summary>
		/// <remarks><b>Note:</b> if the <paramref name="value"/> is <see cref="TypeKeyword.Bool"/>, <see cref="LiteralKind.False"/> is returned.</remarks>
		/// <param name="value"><see cref="TypeKeyword"/> to convert.</param>
		public static LiteralKind GetLiteralKind(this TypeKeyword value)
		{
			if (value.IsNumberType())
			{
				return LiteralKind.Number;
			}

			return value switch
			{
				TypeKeyword.Char => LiteralKind.Character,
				TypeKeyword.String => LiteralKind.String,
				TypeKeyword.Bool => LiteralKind.False,
				TypeKeyword.Object => LiteralKind.Null,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="LiteralKind"/> value.
		/// </summary>
		/// <remarks><b>Note:</b> if the <paramref name="value"/> is <see cref="SpecialType.System_Boolean"/>, <see cref="LiteralKind.False"/> is returned.</remarks>
		/// <param name="value"><see cref="SpecialType"/> to convert.</param>
		public static LiteralKind GetLiteralKind(this SpecialType value)
		{
			if (value.IsNumberType())
			{
				return LiteralKind.Number;
			}

			switch (value)
			{
				case SpecialType.System_Boolean:
					return LiteralKind.False;

				case SpecialType.System_Char:
					return LiteralKind.Character;

				case SpecialType.System_String:
					return LiteralKind.String;

				case SpecialType.System_ArgIterator:
					return LiteralKind.ArgList;

				default:

					if (value.IsReferenceType())
					{
						return LiteralKind.Null;
					}

					return default;
			}
		}

		/// <summary>
		/// Returns the matching accessor kind of the specified <see cref="EventAccessorKind"/>.
		/// </summary>
		/// <param name="value"><see cref="EventAccessorKind"/> to get the matching accessor of.</param>
		public static EventAccessorKind GetMachingAccessor(this EventAccessorKind value)
		{
			return value switch
			{
				EventAccessorKind.Add => EventAccessorKind.Remove,
				EventAccessorKind.Remove => EventAccessorKind.Add,
				_ => default
			};
		}

		/// <summary>
		/// Returns the matching accessor kind of the specified <see cref="PropertyAccessorKind"/>.
		/// </summary>
		/// <param name="value"><see cref="PropertyAccessorKind"/> to get the matching accessor of.</param>
		/// <param name="initOnly">Determines whether to return the <see langword="init"/> accessor instead of <see langword="set"/>.</param>
		public static PropertyAccessorKind GetMachingAccessor(this PropertyAccessorKind value, bool initOnly = false)
		{
			return value switch
			{
				PropertyAccessorKind.Get => initOnly ? PropertyAccessorKind.Init : PropertyAccessorKind.Set,
				PropertyAccessorKind.Set or PropertyAccessorKind.Init => PropertyAccessorKind.Get,
				_ => default
			};
		}

		/// <summary>
		/// Returns the matching accessor kind of the specified <see cref="AccessorKind"/>.
		/// </summary>
		/// <param name="value"><see cref="AccessorKind"/> to get the matching accessor of.</param>
		/// <param name="initOnly">Determines whether to return the <see langword="init"/> accessor instead of <see langword="set"/>.</param>
		public static AccessorKind GetMatchingAccessor(this AccessorKind value, bool initOnly = false)
		{
			return value switch
			{
				AccessorKind.Get => initOnly ? AccessorKind.Init : AccessorKind.Set,
				AccessorKind.Set or AccessorKind.Init => AccessorKind.Get,
				AccessorKind.Add => AccessorKind.Remove,
				AccessorKind.Remove => AccessorKind.Add,
				_ => default
			};
		}

		/// <summary>
		/// Returns the required matching <see cref="OverloadableOperator"/> when the specified <paramref name="operator"/> kind is declared.
		/// </summary>
		/// <param name="operator"><see cref="OverloadableOperator"/> to get the matching operator kind of.</param>
		public static OverloadableOperator GetMatchingOperator(this OverloadableOperator @operator)
		{
			return @operator switch
			{
				OverloadableOperator.Equality => OverloadableOperator.Inequality,
				OverloadableOperator.Inequality => OverloadableOperator.Equality,
				OverloadableOperator.LessThan => OverloadableOperator.GreaterThan,
				OverloadableOperator.GreaterThan => OverloadableOperator.LessThan,
				OverloadableOperator.LessThanOrEqual => OverloadableOperator.GreaterThanOrEqual,
				OverloadableOperator.False => OverloadableOperator.True,
				OverloadableOperator.True => OverloadableOperator.False,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="MethodKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="AccessorKind"/> to convert.</param>
		public static MethodKind GetMethodKind(this AccessorKind value)
		{
			return value switch
			{
				AccessorKind.Get => MethodKind.PropertyGet,
				AccessorKind.Set => MethodKind.PropertySet,
				AccessorKind.Init => MethodKind.PropertySet,
				AccessorKind.Add => MethodKind.EventAdd,
				AccessorKind.Remove => MethodKind.EventRemove,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="MethodStyle"/> value.
		/// </summary>
		/// <param name="value"><see cref="LambdaStyle"/> to convert.</param>
		public static MethodStyle GetMethodStyle(this LambdaStyle value)
		{
			if (value == LambdaStyle.Method)
			{
				return MethodStyle.Block;
			}

			return (MethodStyle)value;
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="NamespaceStyle"/> value.
		/// </summary>
		/// <param name="value"><see cref="SyntaxKind"/> to convert.</param>
		public static NamespaceStyle GetNamespaceStyle(this SyntaxKind value)
		{
			if (value == SyntaxKind.FileScopedNamespaceDeclaration)
			{
				return NamespaceStyle.File;
			}

			return NamespaceStyle.Default;
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="NumericLiteralSuffix"/> value.
		/// </summary>
		/// <param name="value"><see cref="DecimalLiteralSuffix"/> to convert.</param>
		public static NumericLiteralSuffix GetNumericSuffix(this DecimalLiteralSuffix value)
		{
			return value == DecimalLiteralSuffix.None
				? NumericLiteralSuffix.None
				: (NumericLiteralSuffix)(value + (int)NumericLiteralSuffix.LongUpperUnsignedUpper);
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="OverloadableOperator"/> value.
		/// </summary>
		/// <param name="value"><see cref="SyntaxKind"/> to convert.</param>
		public static OverloadableOperator GetOperator(this SyntaxKind value)
		{
			return value switch
			{
				SyntaxKind.UnaryPlusExpression
					=> OverloadableOperator.UnaryPlus,

				SyntaxKind.UnaryMinusExpression
					=> OverloadableOperator.UnaryMinus,

				SyntaxKind.AddExpression or
				SyntaxKind.AddAssignmentExpression or
				SyntaxKind.PlusToken or
				SyntaxKind.PlusEqualsToken
					=> OverloadableOperator.Addition,

				SyntaxKind.SubtractExpression or
				SyntaxKind.SubtractAssignmentExpression or
				SyntaxKind.MinusToken or
				SyntaxKind.MinusEqualsToken
					=> OverloadableOperator.Subtraction,

				SyntaxKind.MultiplyExpression or
				SyntaxKind.MultiplyAssignmentExpression or
				SyntaxKind.AsteriskToken or
				SyntaxKind.AsteriskEqualsToken
					=> OverloadableOperator.Multiplication,

				SyntaxKind.DivideExpression or
				SyntaxKind.DivideAssignmentExpression or
				SyntaxKind.SlashToken or
				SyntaxKind.SlashEqualsToken
					=> OverloadableOperator.Division,

				SyntaxKind.ModuloExpression or
				SyntaxKind.ModuloAssignmentExpression or
				SyntaxKind.PercentToken or
				SyntaxKind.PercentEqualsToken
					=> OverloadableOperator.Remainder,

				SyntaxKind.PreIncrementExpression or
				SyntaxKind.PostDecrementExpression or
				SyntaxKind.PlusPlusToken
					=> OverloadableOperator.Increment,

				SyntaxKind.PreDecrementExpression or
				SyntaxKind.PostDecrementExpression or
				SyntaxKind.MinusMinusToken
					=> OverloadableOperator.Decrement,

				SyntaxKind.EqualsExpression or
				SyntaxKind.EqualsEqualsToken
					=> OverloadableOperator.Equality,

				SyntaxKind.NotEqualsExpression or
				SyntaxKind.ExclamationEqualsToken
					=> OverloadableOperator.Inequality,

				SyntaxKind.ExclusiveOrExpression or
				SyntaxKind.ExclusiveOrAssignmentExpression or
				SyntaxKind.CaretToken or
				SyntaxKind.CaretEqualsToken
					=> OverloadableOperator.LogicalXor,

				SyntaxKind.LogicalAndExpression or
				SyntaxKind.BitwiseAndExpression or
				SyntaxKind.AndAssignmentExpression or
				SyntaxKind.AmpersandToken or
				SyntaxKind.AmpersandEqualsToken
					=> OverloadableOperator.LogicalAnd,

				SyntaxKind.LogicalOrExpression or
				SyntaxKind.BitwiseOrExpression or
				SyntaxKind.OrAssignmentExpression or
				SyntaxKind.BarToken or
				SyntaxKind.BarEqualsToken
					=> OverloadableOperator.LogicalOr,

				SyntaxKind.LogicalNotExpression or
				SyntaxKind.ExclamationToken
					=> OverloadableOperator.Negation,

				SyntaxKind.TrueLiteralExpression or
				SyntaxKind.TrueKeyword
					=> OverloadableOperator.True,

				SyntaxKind.FalseLiteralExpression or
				SyntaxKind.FalseKeyword
					=> OverloadableOperator.False,

				SyntaxKind.BitwiseNotExpression or
				SyntaxKind.TildeToken
					=> OverloadableOperator.Complement,

				SyntaxKind.GreaterThanExpression or
				SyntaxKind.GreaterThanToken
					=> OverloadableOperator.GreaterThan,

				SyntaxKind.GreaterThanOrEqualExpression or
				SyntaxKind.GreaterThanEqualsToken
					=> OverloadableOperator.GreaterThanOrEqual,

				SyntaxKind.LessThanExpression or
				SyntaxKind.LessThanToken
					=> OverloadableOperator.LessThan,

				SyntaxKind.LessThanOrEqualExpression or
				SyntaxKind.LessThanEqualsToken
					=> OverloadableOperator.LessThanOrEqual,

				SyntaxKind.RightShiftExpression or
				SyntaxKind.RightShiftAssignmentExpression or
				SyntaxKind.GreaterThanGreaterThanToken or
				SyntaxKind.GreaterThanGreaterThanEqualsToken
					=> OverloadableOperator.RightShift,

				SyntaxKind.LeftShiftExpression or
				SyntaxKind.LeftShiftAssignmentExpression or
				SyntaxKind.LessThanLessThanToken or
				SyntaxKind.LessThanLessThanToken
					=> OverloadableOperator.LeftShift,

				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="PropertyAccessorKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="SyntaxKind"/> to convert.</param>
		public static PropertyAccessorKind GetPropertyAccessorKind(this SyntaxKind value)
		{
			return value switch
			{
				SyntaxKind.GetKeyword or SyntaxKind.GetAccessorDeclaration => PropertyAccessorKind.Get,
				SyntaxKind.SetKeyword or SyntaxKind.SetAccessorDeclaration => PropertyAccessorKind.Set,
				SyntaxKind.InitKeyword or SyntaxKind.InitAccessorDeclaration => PropertyAccessorKind.Init,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="PropertyAccessorKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="AccessorKind"/> to convert.</param>
		public static PropertyAccessorKind GetPropertyAccessorKind(this AccessorKind value)
		{
			return (PropertyAccessorKind)value;
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="PropertyAccessorKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="AutoPropertyKind"/> to convert.</param>
		public static PropertyAccessorKind GetPropertyAccessorKind(this AutoPropertyKind value)
		{
			return value switch
			{
				AutoPropertyKind.GetOnly => PropertyAccessorKind.Get,
				AutoPropertyKind.SetOnly => PropertyAccessorKind.Set,
				AutoPropertyKind.InitOnly => PropertyAccessorKind.Init,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/>to an associated <see cref="RefKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="SyntaxKind"/> to convert.</param>
		public static RefKind GetRefKind(this SyntaxKind value)
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
		/// Returns the a value representing special requirements a declaration of the specified <paramref name="operator"/> kind must fulfill.
		/// </summary>
		/// <param name="operator"><see cref="OverloadableOperator"/> to get the <see cref="OperatorRequirements"/> of.</param>
		public static OperatorRequirements GetSpecialRequirements(this OverloadableOperator @operator)
		{
			return @operator switch
			{
				OverloadableOperator.LessThan or
				OverloadableOperator.LessThanOrEqual or
				OverloadableOperator.GreaterThan or
				OverloadableOperator.GreaterThanOrEqual or
				OverloadableOperator.Equality or
				OverloadableOperator.Inequality
					=> OperatorRequirements.MatchingOperator,

				OverloadableOperator.True or
				OverloadableOperator.False
					=> OperatorRequirements.ReturnBoolean | OperatorRequirements.MatchingOperator,

				OverloadableOperator.Increment or
				OverloadableOperator.Decrement
					=> OperatorRequirements.ReturnParameterType,

				OverloadableOperator.RightShift or
				OverloadableOperator.LeftShift
					=> OperatorRequirements.IntOperand,

				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="SpecialType"/> value.
		/// </summary>
		/// <param name="value"><see cref="LiteralKind"/> to convert.</param>
		public static SpecialType GetSpecialType(this LiteralKind value)
		{
			return value switch
			{
				LiteralKind.Number => SpecialType.System_Int32,
				LiteralKind.String => SpecialType.System_String,
				LiteralKind.Character => SpecialType.System_Char,

				LiteralKind.False or LiteralKind.True => SpecialType.System_Boolean,
				LiteralKind.Null or LiteralKind.Default => SpecialType.System_Object,

				LiteralKind.ArgList => SpecialType.System_ArgIterator,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="SpecialType"/> value.
		/// </summary>
		/// <param name="value"><see cref="TypeKeyword"/> to convert.</param>
		public static SpecialType GetSpecialType(this TypeKeyword value)
		{
			return value switch
			{
				TypeKeyword.Int => SpecialType.System_Int32,
				TypeKeyword.Bool => SpecialType.System_Boolean,
				TypeKeyword.String => SpecialType.System_String,
				TypeKeyword.Float => SpecialType.System_Single,
				TypeKeyword.Double => SpecialType.System_Double,
				TypeKeyword.Void => SpecialType.System_Void,
				TypeKeyword.Object or TypeKeyword.Dynamic => SpecialType.System_Object,
				TypeKeyword.Char => SpecialType.System_Char,
				TypeKeyword.Decimal => SpecialType.System_Decimal,
				TypeKeyword.Long => SpecialType.System_Int64,
				TypeKeyword.Short => SpecialType.System_Int16,
				TypeKeyword.UShort => SpecialType.System_UInt16,
				TypeKeyword.UInt => SpecialType.System_UInt32,
				TypeKeyword.ULong => SpecialType.System_UInt64,
				TypeKeyword.Byte => SpecialType.System_Byte,
				TypeKeyword.SByte => SpecialType.System_SByte,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="SpecialType"/> value.
		/// </summary>
		/// <param name="value"><see cref="DecimalValueType"/> to convert.</param>
		public static SpecialType GetSpecialType(this DecimalValueType value)
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
		public static SpecialType GetSpecialType(this IntegerValueType value)
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
		/// <param name="value"><see cref="TypeKeyword"/> to convert.</param>
		public static NumericLiteralSuffix GetSuffix(this TypeKeyword value)
		{
			return value switch
			{
				TypeKeyword.Long => NumericLiteralSuffix.LongUpper,
				TypeKeyword.ULong => NumericLiteralSuffix.UnsignedUpperLongUpper,
				TypeKeyword.UInt => NumericLiteralSuffix.UnsignedUpper,
				TypeKeyword.Float => NumericLiteralSuffix.FloatUpper,
				TypeKeyword.Double => NumericLiteralSuffix.DoubleUpper,
				TypeKeyword.Decimal => NumericLiteralSuffix.DecimalUpper,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="NumericLiteralSuffix"/> value.
		/// </summary>
		/// <remarks>The returned value is considered unsigned-first uppercase (see <see cref="IsUpper(NumericLiteralPrefix)"/> and <see cref="UnsignedFirst(NumericLiteralSuffix)"/>for more details).</remarks>
		/// <param name="value"><see cref="SpecialType"/> to convert.</param>
		public static NumericLiteralSuffix GetSuffix(this SpecialType value)
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
		public static NumericLiteralSuffix GetSuffix(this IntegerValueType value)
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
		/// <remarks>The returned value is considered unsigned-first uppercase (see <see cref="IsUpper(NumericLiteralPrefix)"/> for more details).</remarks>
		/// <param name="value"><see cref="DecimalValueType"/> to convert.</param>
		public static DecimalLiteralSuffix GetSuffix(this DecimalValueType value)
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
		/// <param name="value"><see cref="Virtuality"/> to convert.</param>
		public static SyntaxKind GetSyntaxKind(this Virtuality value)
		{
			return value switch
			{
				Virtuality.Virtual => SyntaxKind.VirtualKeyword,
				Virtuality.Abstract => SyntaxKind.AbstractKeyword,
				Virtuality.Sealed => SyntaxKind.SealedKeyword,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="SyntaxKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="AutoPropertyKind"/> to convert.</param>
		public static SyntaxKind GetSyntaxKind(this AutoPropertyKind value)
		{
			return value switch
			{
				AutoPropertyKind.GetOnly => SyntaxKind.GetAccessorDeclaration,
				AutoPropertyKind.SetOnly => SyntaxKind.SetAccessorDeclaration,
				AutoPropertyKind.InitOnly => SyntaxKind.InitAccessorDeclaration,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="SyntaxKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="SpecialType"/> to convert.</param>
		public static SyntaxKind GetSyntaxKind(this SpecialType value)
		{
			return value switch
			{
				SpecialType.System_Int32 => SyntaxKind.IntKeyword,
				SpecialType.System_Boolean => SyntaxKind.BoolKeyword,
				SpecialType.System_String => SyntaxKind.StringKeyword,
				SpecialType.System_Single => SyntaxKind.FloatKeyword,
				SpecialType.System_Double => SyntaxKind.DoubleKeyword,
				SpecialType.System_Int64 => SyntaxKind.LongKeyword,
				SpecialType.System_Char => SyntaxKind.CharKeyword,
				SpecialType.System_Void => SyntaxKind.VoidKeyword,
				SpecialType.System_Object => SyntaxKind.ObjectKeyword,
				SpecialType.System_Int16 => SyntaxKind.ShortKeyword,
				SpecialType.System_Decimal => SyntaxKind.DecimalKeyword,
				SpecialType.System_Byte => SyntaxKind.ByteKeyword,
				SpecialType.System_UInt16 => SyntaxKind.UShortKeyword,
				SpecialType.System_UInt32 => SyntaxKind.UIntKeyword,
				SpecialType.System_UInt64 => SyntaxKind.ULongKeyword,
				SpecialType.System_SByte => SyntaxKind.SByteKeyword,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="SyntaxKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="DecimalValueType"/> to convert.</param>
		public static SyntaxKind GetSyntaxKind(this DecimalValueType value)
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
		public static SyntaxKind GetSyntaxKind(this IntegerValueType value)
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
		public static SyntaxKind GetSyntaxKind(this RefKind value)
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
		public static SyntaxKind GetSyntaxKind(this ConstructorInitializer value)
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
		public static SyntaxKind GetSyntaxKind(this AttributeTarget value)
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
		public static SyntaxKind GetSyntaxKind(this OverloadableOperator value)
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
				OverloadableOperator.LessThanOrEqual => SyntaxKind.LessThanEqualsToken,
				OverloadableOperator.GreaterThanOrEqual => SyntaxKind.GreaterThanEqualsToken,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="SyntaxKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="LiteralKind"/> to convert.</param>
		public static SyntaxKind GetSyntaxKind(this LiteralKind value)
		{
			return value switch
			{
				LiteralKind.Number => SyntaxKind.NumericLiteralToken,
				LiteralKind.String => SyntaxKind.StringLiteralToken,
				LiteralKind.Character => SyntaxKind.CharacterLiteralToken,
				LiteralKind.True => SyntaxKind.TrueLiteralExpression,
				LiteralKind.False => SyntaxKind.FalseLiteralExpression,
				LiteralKind.Null => SyntaxKind.NullLiteralExpression,
				LiteralKind.Default => SyntaxKind.DefaultLiteralExpression,
				LiteralKind.ArgList => SyntaxKind.ArgListExpression,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="SyntaxKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="GoToKind"/> to convert.</param>
		public static SyntaxKind GetSyntaxKind(this GoToKind value)
		{
			return value switch
			{
				GoToKind.Label => SyntaxKind.GotoStatement,
				GoToKind.Case => SyntaxKind.GotoCaseStatement,
				GoToKind.DefaultCase => SyntaxKind.GotoDefaultStatement,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="SyntaxKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="UsingKind"/> to convert.</param>
		public static SyntaxKind GetSyntaxKind(this UsingKind value)
		{
			return value switch
			{
				UsingKind.Ordinary => SyntaxKind.UsingKeyword,
				UsingKind.Static => SyntaxKind.StaticKeyword,
				UsingKind.Alias => SyntaxKind.AliasKeyword,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="SyntaxKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="NamespaceStyle"/> to convert.</param>
		public static SyntaxKind GetSyntaxKind(this GenericConstraint value)
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
		public static SyntaxKind GetSyntaxKind(this NamespaceStyle value)
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
		/// <param name="value"><see cref="PropertyAccessorKind"/> to convert.</param>
		public static SyntaxKind GetSyntaxKind(this PropertyAccessorKind value)
		{
			return value switch
			{
				PropertyAccessorKind.Get => SyntaxKind.GetKeyword,
				PropertyAccessorKind.Set => SyntaxKind.SetKeyword,
				PropertyAccessorKind.Init => SyntaxKind.InitKeyword,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="SyntaxKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="EventAccessorKind"/> to convert.</param>
		public static SyntaxKind GetSyntaxKind(this EventAccessorKind value)
		{
			return value switch
			{
				EventAccessorKind.Add => SyntaxKind.AddKeyword,
				EventAccessorKind.Remove => SyntaxKind.RemoveKeyword,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="SyntaxKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="AccessorKind"/> to convert.</param>
		public static SyntaxKind GetSyntaxKind(this AccessorKind value)
		{
			return value switch
			{
				AccessorKind.Get => SyntaxKind.GetKeyword,
				AccessorKind.Set => SyntaxKind.SetKeyword,
				AccessorKind.Init => SyntaxKind.InitKeyword,
				AccessorKind.Add => SyntaxKind.AddKeyword,
				AccessorKind.Remove => SyntaxKind.RemoveKeyword,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="SyntaxKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="Accessibility"/> to convert.</param>
		public static SyntaxKind GetSyntaxKind(this Accessibility value)
		{
			return value switch
			{
				Accessibility.Public => SyntaxKind.PublicKeyword,
				Accessibility.Private => SyntaxKind.PrivateKeyword,
				Accessibility.Protected => SyntaxKind.ProtectedKeyword,
				Accessibility.Internal => SyntaxKind.InternalKeyword,
				_ => default
			};
		}

		/// <summary>
		/// Returns the name of system type the specified <see cref="TypeKeyword"/> represents.
		/// </summary>
		/// <param name="value"><see cref="TypeKeyword"/> to return the name of system type represented by.</param>
		public static string? GetSystemName(this TypeKeyword value)
		{
			return value switch
			{
				TypeKeyword.Short => "Int16",
				TypeKeyword.Int => "Int32",
				TypeKeyword.Long => "Int64",
				TypeKeyword.UShort => "UInt16",
				TypeKeyword.UInt => "UInt32",
				TypeKeyword.ULong => "UInt64",
				TypeKeyword.Byte => "Byte",
				TypeKeyword.SByte => "SByte",
				TypeKeyword.NInt => "IntPtr",
				TypeKeyword.NUInt => "UIntPtr",
				TypeKeyword.Float => "Single",
				TypeKeyword.Double => "Double",
				TypeKeyword.Decimal => "Decimal",
				TypeKeyword.Bool => "Boolean",
				TypeKeyword.Char => "Char",
				TypeKeyword.String => "String",
				TypeKeyword.Void => "Void",
				TypeKeyword.Object or TypeKeyword.Dynamic => "Object",
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <see cref="Virtuality"/> <paramref name="value"/> to its <see cref="string"/> representation.
		/// </summary>
		/// <param name="value"><see cref="Virtuality"/> to convert to a <see cref="string"/> representation.</param>
		public static string? GetText(this Virtuality value)
		{
			return value switch
			{
				Virtuality.Virtual => "virtual",
				Virtuality.Abstract => "abstract",
				Virtuality.Sealed => "sealed",
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <see cref="UsingKind"/> <paramref name="value"/> to its <see cref="string"/> representation.
		/// </summary>
		/// <param name="value"><see cref="UsingKind"/> to convert to a <see cref="string"/> representation.</param>
		public static string? GetText(this UsingKind value)
		{
			return value switch
			{
				UsingKind.Ordinary => "using",
				UsingKind.Static => "using static",
				UsingKind.Alias => "using",
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <see cref="TypeKeyword"/> <paramref name="value"/> to its <see cref="string"/> representation.
		/// </summary>
		/// <param name="value"><see cref="TypeKeyword"/> to convert to a <see cref="string"/> representation.</param>
		public static string? GetText(this TypeKeyword value)
		{
			return value switch
			{
				TypeKeyword.Short => "short",
				TypeKeyword.Int => "int",
				TypeKeyword.Long => "long",
				TypeKeyword.UShort => "ushort",
				TypeKeyword.UInt => "uint",
				TypeKeyword.ULong => "ulong",
				TypeKeyword.Byte => "byte",
				TypeKeyword.SByte => "sbyte",
				TypeKeyword.Float => "float",
				TypeKeyword.Double => "double",
				TypeKeyword.Decimal => "decimal",
				TypeKeyword.Bool => "bool",
				TypeKeyword.String => "string",
				TypeKeyword.Char => "char",
				TypeKeyword.NInt => "nint",
				TypeKeyword.NUInt => "nuint",
				TypeKeyword.Void => "void",
				TypeKeyword.Object => "object",
				TypeKeyword.Dynamic => "dynamic",
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
				Accessibility.Private => "private",
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
				OverloadableOperator.LessThanOrEqual => "<=",
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
		/// Converts the specified <see cref="GoToKind"/> <paramref name="value"/> to its <see cref="string"/> representation.
		/// </summary>
		/// <param name="value"><see cref="GoToKind"/> to convert to a <see cref="string"/> representation.</param>
		public static string? GetText(this GoToKind value)
		{
			return value switch
			{
				GoToKind.Label => "goto",
				GoToKind.Case => "goto case",
				GoToKind.DefaultCase => "goto default",
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
			return value switch
			{
				DecimalLiteralSuffix.FloatLower => "f",
				DecimalLiteralSuffix.FloatUpper => "F",
				DecimalLiteralSuffix.DoubleLower => "d",
				DecimalLiteralSuffix.DoubleUpper => "D",
				DecimalLiteralSuffix.DecimalLower => "m",
				DecimalLiteralSuffix.DecimalUpper => "M",
				_ => default
			};
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
		/// Converts the specified <see cref="ExponentialStyle"/> <paramref name="value"/> to its <see cref="string"/> representation.
		/// </summary>
		/// <param name="value"><see cref="ExponentialStyle"/> to convert to a <see cref="string"/> representation.</param>
		public static string? GetText(this ExponentialStyle value)
		{
			return value switch
			{
				ExponentialStyle.Lowercase => "e",
				ExponentialStyle.Uppercase => "E",
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <see cref="PropertyAccessorKind"/> <paramref name="value"/> to its <see cref="string"/> representation.
		/// </summary>
		/// <param name="value"><see cref="PropertyAccessorKind"/> to convert to a <see cref="string"/> representation.</param>
		public static string? GetText(this PropertyAccessorKind value)
		{
			return value switch
			{
				PropertyAccessorKind.Get => "get",
				PropertyAccessorKind.Set => "set",
				PropertyAccessorKind.Init => "init",
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <see cref="EventAccessorKind"/> <paramref name="value"/> to its <see cref="string"/> representation.
		/// </summary>
		/// <param name="value"><see cref="EventAccessorKind"/> to convert to a <see cref="string"/> representation.</param>
		public static string? GetText(this EventAccessorKind value)
		{
			return value switch
			{
				EventAccessorKind.Add => "add",
				EventAccessorKind.Remove => "remove",
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <see cref="AccessorKind"/> <paramref name="value"/> to its <see cref="string"/> representation.
		/// </summary>
		/// <param name="value"><see cref="AccessorKind"/> to convert to a <see cref="string"/> representation.</param>
		public static string? GetText(this AccessorKind value)
		{
			return value switch
			{
				AccessorKind.Get => "get",
				AccessorKind.Set => "set",
				AccessorKind.Init => "init",
				AccessorKind.Add => "add",
				AccessorKind.Remove => "remove",
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
		/// Converts the specified <see cref="LiteralKind"/> <paramref name="value"/> to its <see cref="string"/> representation.
		/// </summary>
		/// <param name="value"><see cref="LiteralKind"/> to convert to a <see cref="string"/> representation.</param>
		public static string? GetText(this LiteralKind value)
		{
			return value switch
			{
				LiteralKind.True => "true",
				LiteralKind.False => "false",
				LiteralKind.Default => "default",
				LiteralKind.Null => "null",
				LiteralKind.ArgList => "__arglist",
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
		/// Converts the specified <paramref name="value"/> to an associated <see cref="TypeKeyword"/> value.
		/// </summary>
		/// <param name="value"><see cref="SyntaxKind"/> to convert.</param>
		public static TypeKeyword GetTypeKeyword(this SyntaxKind value)
		{
			return value switch
			{
				SyntaxKind.IntKeyword => TypeKeyword.Int,
				SyntaxKind.LongKeyword => TypeKeyword.Long,
				SyntaxKind.FloatKeyword => TypeKeyword.Float,
				SyntaxKind.DoubleKeyword => TypeKeyword.Double,
				SyntaxKind.StringKeyword => TypeKeyword.String,
				SyntaxKind.BoolKeyword => TypeKeyword.Bool,
				SyntaxKind.ByteKeyword => TypeKeyword.Byte,
				SyntaxKind.CharKeyword => TypeKeyword.Char,
				SyntaxKind.DecimalKeyword => TypeKeyword.Decimal,
				SyntaxKind.ObjectKeyword => TypeKeyword.Object,
				SyntaxKind.VoidKeyword => TypeKeyword.Void,
				SyntaxKind.ShortKeyword => TypeKeyword.Short,
				SyntaxKind.UIntKeyword => TypeKeyword.UInt,
				SyntaxKind.UShortKeyword => TypeKeyword.UShort,
				SyntaxKind.ULongKeyword => TypeKeyword.ULong,
				SyntaxKind.SByteKeyword => TypeKeyword.SByte,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="TypeKeyword"/> value.
		/// </summary>
		/// <param name="value"><see cref="LiteralKind"/> to convert.</param>
		public static TypeKeyword GetTypeKeyword(this LiteralKind value)
		{
			return value switch
			{
				LiteralKind.Number => TypeKeyword.Int,
				LiteralKind.String => TypeKeyword.String,
				LiteralKind.Character => TypeKeyword.Char,
				LiteralKind.True or LiteralKind.False => TypeKeyword.Bool,
				LiteralKind.Default or LiteralKind.Null or LiteralKind.ArgList => TypeKeyword.Object,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="UsingKind"/> value.
		/// </summary>
		/// <param name="value"><see cref="SyntaxKind"/> to convert.</param>
		public static UsingKind GetUsingKind(this SyntaxKind value)
		{
			return value switch
			{
				SyntaxKind.UsingDirective or SyntaxKind.UsingKeyword => UsingKind.Ordinary,
				SyntaxKind.StaticKeyword => UsingKind.Static,
				SyntaxKind.AliasKeyword => UsingKind.Alias,
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an associated <see cref="Virtuality"/> value.
		/// </summary>
		/// <param name="value"><see cref="SyntaxKind"/> to convert.</param>
		public static Virtuality GetVirtuality(this SyntaxKind value)
		{
			return value switch
			{
				SyntaxKind.VirtualKeyword => Virtuality.Virtual,
				SyntaxKind.SealedKeyword => Virtuality.Sealed,
				SyntaxKind.AbstractKeyword => Virtuality.Abstract,
				_ => default
			};
		}

		/// <summary>
		/// Determines whether the specified <see cref="AccessorKind"/> represents an accessor with an input parameter.
		/// </summary>
		/// <param name="value"><see cref="AccessorKind"/> to determine whether represents an accessor an input parameter.</param>
		public static bool HasParameter(this AccessorKind value)
		{
			return value is AccessorKind.Set or AccessorKind.Init or AccessorKind.Add or AccessorKind.Remove;
		}

		/// <summary>
		/// Determines whether the specified <see cref="PropertyAccessorKind"/> represents an accessor with an input parameter.
		/// </summary>
		/// <param name="value"><see cref="PropertyAccessorKind"/> to determine whether represents an accessor an input parameter.</param>
		public static bool HasParameter(this PropertyAccessorKind value)
		{
			return value is PropertyAccessorKind.Set or PropertyAccessorKind.Init;
		}

		/// <summary>
		/// Determines whether the specified <see cref="AccessorKind"/> represents an accessor with return type.
		/// </summary>
		/// <param name="value"><see cref="AccessorKind"/> to determine whether represents an accessor with return type.</param>
		public static bool HasReturnType(this AccessorKind value)
		{
			return value == AccessorKind.Get;
		}

		/// <summary>
		/// Determines whether the specified <see cref="PropertyAccessorKind"/> represents an accessor with return type.
		/// </summary>
		/// <param name="value"><see cref="PropertyAccessorKind"/> to determine whether represents an accessor with return type.</param>
		public static bool HasReturnType(this PropertyAccessorKind value)
		{
			return value == PropertyAccessorKind.Get;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="operator"/> is a binary operator.
		/// </summary>
		/// <param name="operator"><see cref="OverloadableOperator"/> to determine whether is a binary operator.</param>
		public static bool IsBinary(this OverloadableOperator @operator)
		{
			return @operator is
				OverloadableOperator.Addition or
				OverloadableOperator.Division or
				OverloadableOperator.Subtraction or
				OverloadableOperator.Multiplication or
				OverloadableOperator.Remainder or
				OverloadableOperator.Equality or
				OverloadableOperator.Inequality or
				OverloadableOperator.GreaterThan or
				OverloadableOperator.GreaterThanOrEqual or
				OverloadableOperator.LessThan or
				OverloadableOperator.LessThanOrEqual or
				OverloadableOperator.RightShift or
				OverloadableOperator.LeftShift or
				OverloadableOperator.LogicalAnd or
				OverloadableOperator.LogicalOr or
				OverloadableOperator.LogicalXor;
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
		/// Determines whether the specified <paramref name="value"/> is a declaration kind.
		/// </summary>
		/// <param name="value"><see cref="MethodKind"/> to determine whether is a declaration kind.</param>
		public static bool IsDeclarationKind(this MethodKind value)
		{
			return value is
				MethodKind.Ordinary or
				MethodKind.LocalFunction or
				MethodKind.Constructor or
				MethodKind.Conversion or
				MethodKind.Destructor or
				MethodKind.EventAdd or
				MethodKind.EventRaise or
				MethodKind.PropertyGet or
				MethodKind.PropertySet or
				MethodKind.StaticConstructor or
				MethodKind.UserDefinedOperator;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="value"/> is a declaration kind.
		/// </summary>
		/// <param name="value"><see cref="TypeKind"/> to determine whether is a declaration kind.</param>
		public static bool IsDeclarationKind(this TypeKind value)
		{
			return value is
				TypeKind.Class or
				TypeKind.Struct or
				TypeKind.Enum or
				TypeKind.Interface or
				TypeKind.Delegate;
		}

		/// <summary>
		/// Determines whether the specified <see cref="SpecialType"/> is a delegate type (including <see cref="System.Delegate"/> and <see cref="System.MulticastDelegate"/>).
		/// </summary>
		/// <param name="value"><see cref="SpecialType"/> to determine whether is a delegate type.</param>
		public static bool IsDelegateType(this SpecialType value)
		{
			return value is
				SpecialType.System_AsyncCallback or
				SpecialType.System_Delegate or
				SpecialType.System_MulticastDelegate;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="accessor"/> is an event accessor.
		/// </summary>
		/// <param name="accessor"><see cref="AccessorKind"/> to determine whether is an event accessor.</param>
		public static bool IsEventAccessor(this AccessorKind accessor)
		{
			return accessor is AccessorKind.Add or AccessorKind.Remove;
		}

		/// <summary>
		/// Determines whether the specified <see cref="SpecialType"/> is a floating point numeric type.
		/// </summary>
		/// <param name="value"><see cref="SpecialType"/> to determine whether is a floating point numeric type.</param>
		public static bool IsFloatingPointType(this SpecialType value)
		{
			return value is SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal;
		}

		/// <summary>
		/// Determines whether the specified <see cref="TypeKeyword"/> is a floating point numeric type.
		/// </summary>
		/// <param name="value"><see cref="TypeKeyword"/> to determine whether is a floating point numeric type.</param>
		public static bool IsFloatingPointType(this TypeKeyword value)
		{
			return value is TypeKeyword.Float or TypeKeyword.Double or TypeKeyword.Decimal;
		}

		/// <summary>
		/// Determines whether the specified <see cref="SpecialType"/> is a generic type.
		/// </summary>
		/// <param name="value"><see cref="SpecialType"/> to determine whether is a generic type.</param>
		public static bool IsGenericType(this SpecialType value)
		{
			return value is
				SpecialType.System_Collections_Generic_ICollection_T or
				SpecialType.System_Collections_Generic_IEnumerable_T or
				SpecialType.System_Collections_Generic_IEnumerator_T or
				SpecialType.System_Collections_Generic_IList_T or
				SpecialType.System_Collections_Generic_IReadOnlyCollection_T or
				SpecialType.System_Collections_Generic_IReadOnlyList_T or
				SpecialType.System_Nullable_T;
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
		/// Determines whether the specified <see cref="TypeKeyword"/> represents an integer type.
		/// </summary>
		/// <param name="value"><see cref="TypeKeyword"/> to determine whether represents an integer type.</param>
		public static bool IsIntegerType(this TypeKeyword value)
		{
			return value >= TypeKeyword.Short && value <= TypeKeyword.SByte;
		}

		/// <summary>
		/// Determines whether the specified <see cref="SpecialType"/> represents an integer type.
		/// </summary>
		/// <param name="value"><see cref="SpecialType"/> to determine whether represents an integer type.</param>
		public static bool IsIntegerType(this SpecialType value)
		{
			return value >= SpecialType.System_SByte && value <= SpecialType.System_UInt64;
		}

		/// <summary>
		/// Determines whether the specified <see cref="SpecialType"/> is an interface type.
		/// </summary>
		/// <param name="value"><see cref="SpecialType"/> to determine whether is an interface type.</param>
		public static bool IsInterfaceType(this SpecialType value)
		{
			return value is
				SpecialType.System_Collections_Generic_ICollection_T or
				SpecialType.System_Collections_Generic_IEnumerable_T or
				SpecialType.System_Collections_Generic_IEnumerator_T or
				SpecialType.System_Collections_Generic_IList_T or
				SpecialType.System_Collections_Generic_IReadOnlyCollection_T or
				SpecialType.System_Collections_Generic_IReadOnlyList_T or
				SpecialType.System_Collections_IEnumerable or
				SpecialType.System_Collections_IEnumerator or
				SpecialType.System_IAsyncResult or
				SpecialType.System_IDisposable;
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
		/// Determines whether the specified <see cref="TypeKeyword"/> represents a native integer type.
		/// </summary>
		/// <param name="value"><see cref="TypeKeyword"/> to determine whether represents a native integer type.</param>
		public static bool IsNativeIntegerType(this TypeKeyword value)
		{
			return value is TypeKeyword.NInt or TypeKeyword.NUInt;
		}

		/// <summary>
		/// Determines whether the specified <see cref="SpecialType"/> represents a native integer type.
		/// </summary>
		/// <param name="value"><see cref="SpecialType"/> to determine whether represents a native integer type.</param>
		public static bool IsNativeIntegerType(this SpecialType value)
		{
			return value is SpecialType.System_IntPtr or SpecialType.System_UIntPtr;
		}

		/// <summary>
		/// Determines whether the specified <see cref="TypeKeyword"/> represents a number type.
		/// </summary>
		/// <param name="value"><see cref="TypeKeyword"/> to determine whether represents a number type.</param>
		public static bool IsNumberType(this TypeKeyword value)
		{
			return value >= TypeKeyword.Short && value <= TypeKeyword.SByte;
		}

		/// <summary>
		/// Determines whether the specified <see cref="SpecialType"/> represents a number type.
		/// </summary>
		/// <param name="value"><see cref="SpecialType"/> to determine whether represents a number type.</param>
		public static bool IsNumberType(this SpecialType value)
		{
			return value >= SpecialType.System_SByte && value <= SpecialType.System_Double;
		}

		/// <summary>
		/// Determines whether the specified <see cref="TypeKeyword"/> represents a primitive type of a fixed size.
		/// </summary>
		/// <param name="value"><see cref="TypeKeyword"/> to determine whether is primitive.</param>
		public static bool IsPrimitive(this SpecialType value)
		{
			if (value >= SpecialType.System_Boolean && value <= SpecialType.System_UInt64)
			{
				return true;
			}

			return value is SpecialType.System_Single or SpecialType.System_Double;
		}

		/// <summary>
		/// Determines whether the specified <see cref="TypeKeyword"/> represents a primitive type of a fixed size.
		/// </summary>
		/// <param name="value"><see cref="TypeKeyword"/> to determine whether is primitive.</param>
		public static bool IsPrimitive(this TypeKeyword value)
		{
			if (value >= TypeKeyword.Short && value <= TypeKeyword.SByte)
			{
				return true;
			}

			return value is
				TypeKeyword.Float or
				TypeKeyword.Double or
				TypeKeyword.Char or
				TypeKeyword.Bool;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="accessor"/> is a property accessor.
		/// </summary>
		/// <param name="accessor"><see cref="AccessorKind"/> to determine whether is a property accessor.</param>
		public static bool IsPropertyAccessor(this AccessorKind accessor)
		{
			return accessor is AccessorKind.Get or AccessorKind.Set or AccessorKind.Init;
		}

		/// <summary>
		/// Determines whether the specified <see cref="TypeKeyword"/> is a reference type.
		/// </summary>
		/// <param name="value"><see cref="TypeKeyword"/> to determine whether is a reference type.</param>
		public static bool IsReferenceType(this TypeKeyword value)
		{
			return value is TypeKeyword.String or TypeKeyword.Object or TypeKeyword.Dynamic;
		}

		/// <summary>
		/// Determines whether the specified <see cref="SpecialType"/> is a reference type.
		/// </summary>
		/// <param name="value"><see cref="SpecialType"/> to determine whether is a reference type.</param>
		public static bool IsReferenceType(this SpecialType value)
		{
			if (value.IsInterfaceType())
			{
				return true;
			}

			if (value.IsDelegateType())
			{
				return true;
			}

			return value is
				SpecialType.System_Array or
				SpecialType.System_Enum or
				SpecialType.System_Object or
				SpecialType.System_Runtime_CompilerServices_IsVolatile or
				SpecialType.System_Runtime_CompilerServices_PreserveBaseOverridesAttribute or
				SpecialType.System_Runtime_CompilerServices_RuntimeFeature or
				SpecialType.System_ValueType;
		}

		/// <summary>
		/// Determines whether the specified <see cref="IntegerValueType"/> represents a signed integer type.
		/// </summary>
		/// <param name="value"><see cref="IntegerValueType"/> to determine whether represents a signed integer type.</param>
		public static bool IsSigned(this IntegerValueType value)
		{
			return value is
				IntegerValueType.Int16 or
				IntegerValueType.Int32 or
				IntegerValueType.Int64 or
				IntegerValueType.SByte;
		}

		/// <summary>
		/// Determines whether the specified <see cref="SpecialType"/> represents a signed integer type.
		/// </summary>
		/// <param name="value"><see cref="SpecialType"/> to determine whether represents a signed integer type.</param>
		public static bool IsSigned(this SpecialType value)
		{
			return value is
				SpecialType.System_Int64 or
				SpecialType.System_Int32 or
				SpecialType.System_Int16 or
				SpecialType.System_SByte or
				SpecialType.System_IntPtr;
		}

		/// <summary>
		/// Determines whether the specified <see cref="TypeKeyword"/> represents a signed integer type.
		/// </summary>
		/// <param name="value"><see cref="TypeKeyword"/> to determine whether represents a signed integer type.</param>
		public static bool IsSigned(this TypeKeyword value)
		{
			return value is
				TypeKeyword.Short or
				TypeKeyword.Int or
				TypeKeyword.Long or
				TypeKeyword.SByte or
				TypeKeyword.NInt;
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
		/// Determines whether the specified <paramref name="operator"/> is an unary operator.
		/// </summary>
		/// <param name="operator"><see cref="OverloadableOperator"/> to determine whether is an unary operator.</param>
		public static bool IsUnary(this OverloadableOperator @operator)
		{
			return @operator is
				OverloadableOperator.UnaryPlus or
				OverloadableOperator.UnaryMinus or
				OverloadableOperator.Negation or
				OverloadableOperator.Complement or
				OverloadableOperator.Increment or
				OverloadableOperator.Decrement;
		}

		/// <summary>
		/// Determines whether the specified <see cref="SpecialType"/> represents an unsigned integer type.
		/// </summary>
		/// <param name="value"><see cref="SpecialType"/> to determine whether represents an unsigned integer type.</param>
		public static bool IsUnsigned(this SpecialType value)
		{
			return value is
				SpecialType.System_UInt16 or
				SpecialType.System_UInt32 or
				SpecialType.System_UInt64 or
				SpecialType.System_Byte or
				SpecialType.System_UIntPtr;
		}

		/// <summary>
		/// Determines whether the specified <see cref="TypeKeyword"/> represents an unsigned integer type.
		/// </summary>
		/// <param name="value"><see cref="TypeKeyword"/> to determine whether represents an unsigned integer type.</param>
		public static bool IsUnsigned(this TypeKeyword value)
		{
			return value is
				TypeKeyword.UInt or
				TypeKeyword.UShort or
				TypeKeyword.ULong or
				TypeKeyword.Byte or
				TypeKeyword.NUInt;
		}

		/// <summary>
		/// Determines whether the specified <see cref="IntegerValueType"/> represents an unsigned integer type.
		/// </summary>
		/// <param name="value"><see cref="IntegerValueType"/> to determine whether represents an unsigned integer type.</param>
		public static bool IsUnsigned(this IntegerValueType value)
		{
			return value is
				IntegerValueType.UInt16 or
				IntegerValueType.UInt32 or
				IntegerValueType.UInt64 or
				IntegerValueType.Byte;
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
		/// Determines whether the specified <see cref="SpecialType"/> is a value type.
		/// </summary>
		/// <param name="value"><see cref="SpecialType"/> to determine whether is a value type.</param>
		public static bool IsValueType(this SpecialType value)
		{
			return value is
				SpecialType.System_Void or
				SpecialType.System_Boolean or
				SpecialType.System_Char or
				SpecialType.System_Byte or
				SpecialType.System_SByte or
				SpecialType.System_Int16 or
				SpecialType.System_UInt16 or
				SpecialType.System_Int32 or
				SpecialType.System_UInt32 or
				SpecialType.System_Int64 or
				SpecialType.System_UInt64 or
				SpecialType.System_Single or
				SpecialType.System_Double or
				SpecialType.System_Decimal or
				SpecialType.System_IntPtr or
				SpecialType.System_UIntPtr or
				SpecialType.System_Nullable_T or
				SpecialType.System_DateTime or
				SpecialType.System_TypedReference or
				SpecialType.System_ArgIterator or
				SpecialType.System_RuntimeArgumentHandle or
				SpecialType.System_RuntimeFieldHandle or
				SpecialType.System_RuntimeMethodHandle or
				SpecialType.System_RuntimeTypeHandle;
		}

		/// <summary>
		/// Determines whether the specified <see cref="TypeKeyword"/> is a value type.
		/// </summary>
		/// <param name="value"><see cref="TypeKeyword"/> to determine whether is a value type.</param>
		public static bool IsValueType(this TypeKeyword value)
		{
			return (value >= TypeKeyword.Short && value <= TypeKeyword.Char) || value == TypeKeyword.Void;
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
		/// Reverses the return order.
		/// </summary>
		/// <param name="order"><see cref="ReturnOrder"/> to reverse.</param>
		public static ReturnOrder Reverse(this ReturnOrder order)
		{
			if (order == ReturnOrder.Parent)
			{
				return ReturnOrder.Root;
			}

			return ReturnOrder.Root;
		}

		/// <summary>
		/// Returns the size in bytes of the type the specified <see cref="SpecialType"/> represents.
		/// </summary>
		/// <param name="value"><see cref="SpecialType"/> to get the size in bytes of.</param>
		public static int SizeInBytes(this SpecialType value)
		{
			return value switch
			{
				SpecialType.System_SByte => sizeof(sbyte),
				SpecialType.System_Byte => sizeof(byte),
				SpecialType.System_Int16 => sizeof(short),
				SpecialType.System_UInt16 => sizeof(ushort),
				SpecialType.System_Int32 => sizeof(int),
				SpecialType.System_UInt32 => sizeof(uint),
				SpecialType.System_Int64 => sizeof(long),
				SpecialType.System_UInt64 => sizeof(ulong),
				SpecialType.System_Char => sizeof(char),
				SpecialType.System_Single => sizeof(float),
				SpecialType.System_Double => sizeof(double),
				SpecialType.System_Boolean => sizeof(bool),
				SpecialType.System_Decimal => sizeof(decimal),
				_ => default
			};
		}

		/// <summary>
		/// Returns the size in bytes of the type the specified <see cref="TypeKeyword"/> represents.
		/// </summary>
		/// <param name="value"><see cref="TypeKeyword"/> to get the size in bytes of.</param>
		public static int SizeInBytes(this TypeKeyword value)
		{
			return value switch
			{
				TypeKeyword.SByte => sizeof(sbyte),
				TypeKeyword.Byte => sizeof(byte),
				TypeKeyword.Int => sizeof(int),
				TypeKeyword.UInt => sizeof(uint),
				TypeKeyword.Short => sizeof(short),
				TypeKeyword.UShort => sizeof(ushort),
				TypeKeyword.Long => sizeof(long),
				TypeKeyword.ULong => sizeof(ulong),
				TypeKeyword.Float => sizeof(float),
				TypeKeyword.Double => sizeof(double),
				TypeKeyword.Char => sizeof(char),
				TypeKeyword.Bool => sizeof(bool),
				TypeKeyword.Decimal => sizeof(decimal),
				_ => default
			};
		}

		/// <summary>
		/// Returns the size in bytes of the type the specified <see cref="IntegerValueType"/> represents.
		/// </summary>
		/// <param name="value"><see cref="IntegerValueType"/> to get the size in bytes of.</param>
		public static int SizeInBytes(this IntegerValueType value)
		{
			return value switch
			{
				IntegerValueType.SByte => sizeof(sbyte),
				IntegerValueType.Byte => sizeof(byte),
				IntegerValueType.Int16 => sizeof(short),
				IntegerValueType.UInt16 => sizeof(ushort),
				IntegerValueType.Int32 => sizeof(int),
				IntegerValueType.UInt32 => sizeof(uint),
				IntegerValueType.Int64 => sizeof(long),
				IntegerValueType.UInt64 => sizeof(ulong),
				_ => default
			};
		}

		/// <summary>
		/// Returns the size in bytes of the type the specified <see cref="DecimalValueType"/> represents.
		/// </summary>
		/// <param name="value"><see cref="DecimalValueType"/> to get the size in bytes of.</param>
		public static int SizeInBytes(this DecimalValueType value)
		{
			return value switch
			{
				DecimalValueType.Float => sizeof(float),
				DecimalValueType.Double => sizeof(double),
				DecimalValueType.Decimal => sizeof(decimal),
				_ => default
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
			return value switch
			{
				NumericLiteralSuffix.LongLower or
				NumericLiteralSuffix.UnsignedLower or
				NumericLiteralSuffix.LongLowerUnsignedLower or
				NumericLiteralSuffix.UnsignedLowerLongLower or
				NumericLiteralSuffix.FloatLower or
				NumericLiteralSuffix.DecimalLower or
				NumericLiteralSuffix.DoubleLower
					=> value,

				NumericLiteralSuffix.UnsignedUpper
					=> NumericLiteralSuffix.UnsignedLower,

				NumericLiteralSuffix.LongUpper
					=> NumericLiteralSuffix.LongLower,

				NumericLiteralSuffix.LongUpperUnsignedLower or
				NumericLiteralSuffix.LongUpperUnsignedUpper or
				NumericLiteralSuffix.LongLowerUnsignedUpper
					=> NumericLiteralSuffix.LongLowerUnsignedLower,

				NumericLiteralSuffix.UnsignedUpperLongLower or
				NumericLiteralSuffix.UnsignedUpperLongUpper or
				NumericLiteralSuffix.UnsignedLowerLongUpper
					=> NumericLiteralSuffix.UnsignedLowerLongLower,

				NumericLiteralSuffix.DecimalUpper
					=> NumericLiteralSuffix.DecimalLower,

				NumericLiteralSuffix.DoubleUpper
					=> NumericLiteralSuffix.DoubleLower,

				NumericLiteralSuffix.FloatUpper
					=> NumericLiteralSuffix.FloatLower,

				_ => default,
			};
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
			return value switch
			{
				NumericLiteralSuffix.LongUpper or
				NumericLiteralSuffix.UnsignedUpper or
				NumericLiteralSuffix.LongUpperUnsignedUpper or
				NumericLiteralSuffix.UnsignedUpperLongUpper or
				NumericLiteralSuffix.FloatUpper or
				NumericLiteralSuffix.DecimalUpper or
				NumericLiteralSuffix.DoubleUpper
					=> value,

				NumericLiteralSuffix.UnsignedLower
					=> NumericLiteralSuffix.UnsignedUpper,

				NumericLiteralSuffix.LongLower
					=> NumericLiteralSuffix.LongUpper,

				NumericLiteralSuffix.LongLowerUnsignedLower or
				NumericLiteralSuffix.LongLowerUnsignedUpper or
				NumericLiteralSuffix.LongUpperUnsignedLower
					=> NumericLiteralSuffix.LongUpperUnsignedUpper,

				NumericLiteralSuffix.UnsignedLowerLongLower or
				NumericLiteralSuffix.UnsignedLowerLongUpper or
				NumericLiteralSuffix.UnsignedUpperLongLower
					=> NumericLiteralSuffix.UnsignedUpperLongUpper,

				NumericLiteralSuffix.DecimalLower
					=> NumericLiteralSuffix.DecimalUpper,

				NumericLiteralSuffix.DoubleLower
					=> NumericLiteralSuffix.DoubleLower,

				NumericLiteralSuffix.FloatLower
					=> NumericLiteralSuffix.FloatUpper,
				_ => default,
			};
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
