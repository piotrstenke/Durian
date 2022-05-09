// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using Durian.Analysis.CodeGeneration;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis
{
	/// <summary>
	/// Contains some utility methods for code analysis.
	/// </summary>
	public static class AnalysisUtilities
	{
		private static readonly string[] _keywords = new string[]
		{
			"__arglist",    "__makeref",    "__reftype",    "__refvalue",
			"abstract",     "as",           "base",         "bool",
			"break",        "byte",         "case",         "catch",
			"char",         "checked",      "class",        "const",
			"continue",     "decimal",      "default",      "delegate",
			"do",           "double",       "else",         "enum",
			"event",        "explicit",     "extern",       "false",
			"finally",      "fixed",        "float",        "for",
			"foreach",      "goto",         "if",           "implicit",
			"in",           "int",          "interface",    "internal",
			"is",           "lock",         "long",         "namespace",
			"new",          "null",         "object",       "operator",
			"out",          "override",     "params",       "private",
			"protected",    "public",       "readonly",     "ref",
			"return",       "sbyte",        "sealed",       "short",
			"sizeof",       "stackalloc",   "static",       "string",
			"struct",       "switch",       "this",         "throw",
			"true",         "try",          "typeof",       "uint",
			"ulong",        "unchecked",    "unsafe",       "ushort",
			"using",        "virtual",      "volatile",     "void",
			"while"
		};

		private static readonly HashSet<string> _keywordsHashed = new(_keywords);

		internal static int MainThreadId { get; }

		static AnalysisUtilities()
		{
			MainThreadId = Environment.CurrentManagedThreadId;
		}

		/// <summary>
		/// Applies the verbatim identifier '@' token if the <paramref name="value"/> is equivalent to an existing C# keyword.
		/// </summary>
		/// <param name="value">Value to apply the verbatim identifier '@' token to.</param>
		public static bool ApplyVerbatimIfNecessary(ref string value)
		{
			if(IsKeyword(value))
			{
				value = '@' + value;
				return true;
			}

			return false;
		}

		/// <summary>
		/// Converts the specified <paramref name="suffix"/> to an associated <see cref="DecimalLiteralSuffix"/> value.
		/// </summary>
		/// <param name="suffix"><see cref="NumericLiteralSuffix"/> to convert.</param>
		public static DecimalLiteralSuffix ConvertSuffix(NumericLiteralSuffix suffix)
		{
			int value = (int)suffix - (int)NumericLiteralSuffix.LongUpperUnsignedUpper;

			if (value < 0)
			{
				return default;
			}

			return (DecimalLiteralSuffix)value;
		}

		/// <summary>
		/// Converts the specified <paramref name="suffix"/> to an associated <see cref="NumericLiteralSuffix"/> value.
		/// </summary>
		/// <param name="suffix"><see cref="DecimalLiteralSuffix"/> to convert.</param>
		public static NumericLiteralSuffix ConvertSuffix(DecimalLiteralSuffix suffix)
		{
			return suffix == DecimalLiteralSuffix.None
				? NumericLiteralSuffix.None
				: (NumericLiteralSuffix)(suffix + (int)NumericLiteralSuffix.LongUpperUnsignedUpper);
		}

		/// <summary>
		/// Returns a <see cref="string"/> representing a dot-separated name.
		/// </summary>
		/// <param name="parts">Parts of the name. Each part will be separated by a dot.</param>
		/// <returns>A <see cref="string"/> representing a dot-separated name. -or- <see cref="string.Empty"/> if the <paramref name="parts"/> were null or empty or white-space only.</returns>
		public static string CreateName(params string[]? parts)
		{
			StringBuilder builder = new();
			builder.WriteName(parts);
			return builder.ToString();
		}

		/// <summary>
		/// Converts the specified <paramref name="accessibility"/> into its <see cref="string"/> representation.
		/// </summary>
		/// <param name="accessibility"><see cref="Accessibility"/> to convert to text.</param>
		public static string? GetAccessiblityText(Accessibility accessibility)
		{
			return accessibility switch
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
		/// Returns a keyword used to refer to a <see cref="ISymbol"/> of the specified <paramref name="kind"/> inside an attribute list.
		/// </summary>
		/// <param name="kind">Kind of method to get the keyword for.</param>
		/// <param name="targetKind">Determines which keyword to return when there is more than one option.</param>
		public static string? GetAttributeTarget(SymbolKind kind, AttributeTargetKind targetKind = default)
		{
			return kind switch
			{
				SymbolKind.NamedType => "type",
				SymbolKind.Field => "field",
				SymbolKind.Method => targetKind == AttributeTargetKind.FieldOrReturn ? "return" : "method",
				SymbolKind.Property => targetKind == AttributeTargetKind.FieldOrReturn ? "field" : "property",
				SymbolKind.Event => targetKind switch
				{
					AttributeTargetKind.FieldOrReturn => "field",
					AttributeTargetKind.MethodOrParam => "method",
					_ => "event"
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
		public static string GetAttributeTarget(MethodKind kind, AttributeTargetKind targetKind = default)
		{
			return kind switch
			{
				MethodKind.EventAdd or
				MethodKind.EventRemove or
				MethodKind.PropertySet => targetKind switch
				{
					AttributeTargetKind.FieldOrReturn => "return",
					AttributeTargetKind.MethodOrParam => "param",
					_ => "method"
				},
				_ => targetKind == AttributeTargetKind.FieldOrReturn ? "return" : "method"
			};
		}

		/// <summary>
		/// Returns a <see cref="string"/> containing generic identifier combined of the specified <paramref name="name"/> and the collection of <paramref name="typeParameters"/>.
		/// </summary>
		/// <param name="typeParameters">Type parameters.</param>
		/// <param name="name">Actual member identifier.</param>
		/// <exception cref="ArgumentNullException"><paramref name="typeParameters"/> is <see langword="null"/>.</exception>
		public static string GetGenericName(IEnumerable<string> typeParameters, string? name)
		{
			StringBuilder builder = new();
			builder.WriteGenericName(typeParameters, name);
			return builder.ToString();
		}

		/// <summary>
		/// Returns a <see cref="string"/> containing the generic part of an identifier created from the collection of <paramref name="typeParameters"/>.
		/// </summary>
		/// <param name="typeParameters">Type parameters.</param>
		public static string GetGenericName(IEnumerable<string> typeParameters)
		{
			StringBuilder builder = new();
			builder.WriteGenericName(typeParameters);
			return builder.ToString();
		}

		/// <summary>
		/// Returns all reserved keywords of the C# language.
		/// </summary>
		public static string[] GetKeywords()
		{
			string[] keywords = new string[_keywords.Length];
			Array.Copy(_keywords, keywords, _keywords.Length);
			return keywords;
		}

		/// <summary>
		/// Converts the specified <see cref="string"/> <paramref name="value"/> into a <see cref="string"/> representation of the operator.
		/// </summary>
		/// <param name="value"><see cref="string"/> to convert.</param>
		public static string? GetOperatorText(string? value)
		{
			string? operatorName = value?.Trim();

			if (string.IsNullOrWhiteSpace(operatorName))
			{
				return default;
			}

			if (operatorName!.StartsWith("op_"))
			{
				operatorName = operatorName.Substring(3);
			}

			return operatorName switch
			{
				"Addition" or "UnaryPlus" => "+",
				"Subtraction" or "UnaryMinus" => "-",
				"Multiplication" => "*",
				"Division" => "/",
				"Equality" => "==",
				"Inequality" => "!=",
				"Negation" => "!",
				"GreaterThan" => ">",
				"GreaterThanOrEqual" => ">=",
				"LessThan" => "<",
				"LestThanOrEqual" => "<=",
				"Complement" => "~",
				"LogicalAnd" => "&",
				"LogicalOr" => "|",
				"LogicalXor" => "^",
				"Remainder" => "%",
				"Increment" => "++",
				"Decrement" => "--",
				"False" => "false",
				"True" => "true",
				"RightShift" => ">>",
				"LeftShift" => "<<",
				_ => default
			};
		}

		/// <summary>
		/// Converts the specified <see cref="OverloadableOperator"/> <paramref name="value"/> to a <see cref="string"/> representation.
		/// </summary>
		/// <param name="value"><see cref="OverloadableOperator"/> to convert to a <see cref="string"/> representation.</param>
		public static string? GetOperatorText(OverloadableOperator value)
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
		/// Converts the specified <see cref="string"/> <paramref name="value"/> to a <see cref="OverloadableOperator"/>.
		/// </summary>
		/// <param name="value">Value to convert to a <see cref="OverloadableOperator"/>.</param>
		public static OverloadableOperator GetOperatorType(string? value)
		{
			string? operatorName = value?.Trim();

			if (string.IsNullOrWhiteSpace(operatorName))
			{
				return default;
			}

			if (char.IsLetter(operatorName![0]))
			{
				if (operatorName!.StartsWith("op_"))
				{
					operatorName = operatorName.Substring(3);
				}

				return operatorName switch
				{
					"Addition" => OverloadableOperator.Addition,
					"Subtraction" => OverloadableOperator.Subtraction,
					"Multiplication" => OverloadableOperator.Multiplication,
					"Division" => OverloadableOperator.Division,
					"Equality" => OverloadableOperator.Equality,
					"Inequality" => OverloadableOperator.Inequality,
					"Negation" => OverloadableOperator.Negation,
					"GreaterThan" => OverloadableOperator.GreaterThan,
					"GreaterThanOrEqual" => OverloadableOperator.GreaterThanOrEqual,
					"LessThan" => OverloadableOperator.LessThan,
					"LestThanOrEqual" => OverloadableOperator.LestThanOrEqual,
					"Complement" => OverloadableOperator.Complement,
					"LogicalAnd" => OverloadableOperator.LogicalAnd,
					"LogicalOr" => OverloadableOperator.LogicalOr,
					"LogicalXor" => OverloadableOperator.LogicalXor,
					"Remainder" => OverloadableOperator.Remainder,
					"Increment" => OverloadableOperator.Increment,
					"Decrement" => OverloadableOperator.Decrement,
					"False" => OverloadableOperator.False,
					"True" => OverloadableOperator.True,
					"UnaryPlus" => OverloadableOperator.UnaryPlus,
					"UnaryMinus" => OverloadableOperator.UnaryMinus,
					"RightShift" => OverloadableOperator.RightShift,
					"LeftShift" => OverloadableOperator.LeftShift,
					_ => default
				};
			}

			return operatorName switch
			{
				"+" => OverloadableOperator.Addition,
				"-" => OverloadableOperator.Subtraction,
				"*" => OverloadableOperator.Multiplication,
				"/" => OverloadableOperator.Division,
				"==" => OverloadableOperator.Equality,
				"!=" => OverloadableOperator.Inequality,
				"!" => OverloadableOperator.Negation,
				">" => OverloadableOperator.GreaterThan,
				">=" => OverloadableOperator.GreaterThanOrEqual,
				"<" => OverloadableOperator.LessThan,
				"<=" => OverloadableOperator.LestThanOrEqual,
				"~" => OverloadableOperator.Complement,
				"&" => OverloadableOperator.LogicalAnd,
				"|" => OverloadableOperator.LogicalOr,
				"^" => OverloadableOperator.LogicalXor,
				"%" => OverloadableOperator.Remainder,
				"++" => OverloadableOperator.Increment,
				"__" => OverloadableOperator.Decrement,
				"false" => OverloadableOperator.False,
				"true" => OverloadableOperator.True,
				">>" => OverloadableOperator.RightShift,
				"<<" => OverloadableOperator.LeftShift,
				_ => default
			};
		}

		/// <summary>
		/// Returns the actual <see cref="string"/> value represented by the specified numeric literal <paramref name="prefix"/>.
		/// </summary>
		/// <param name="prefix"><see cref="NumericLiteralPrefix"/> to get the <see cref="string"/> represented by.</param>
		public static string? GetPrefix(NumericLiteralPrefix prefix)
		{
			return prefix switch
			{
				NumericLiteralPrefix.HexadecimalLower => "0x",
				NumericLiteralPrefix.HexadecimalUpper => "0X",
				NumericLiteralPrefix.BinaryLower => "0b",
				NumericLiteralPrefix.BinaryUpper => "0B",
				_ => default
			};
		}

		/// <summary>
		/// Joins the collection of <see cref="string"/>s into a <see cref="QualifiedNameSyntax"/>.
		/// </summary>
		/// <param name="names">A collection of <see cref="string"/>s to join into a <see cref="QualifiedNameSyntax"/>.</param>
		/// <returns>A <see cref="QualifiedNameSyntax"/> created by combining the <paramref name="names"/>. -or- <see langword="null"/> if there were less then 2 <paramref name="names"/> provided.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="names"/> is <see langword="null"/>.</exception>
		public static QualifiedNameSyntax? GetQualifiedName(IEnumerable<string> names)
		{
			if (names is null)
			{
				throw new ArgumentNullException(nameof(names));
			}

			string[] n = names.ToArray();
			int length = n.Length;

			if (length < 2)
			{
				return null;
			}

			QualifiedNameSyntax q = SyntaxFactory.QualifiedName(SyntaxFactory.IdentifierName(n[0]), SyntaxFactory.IdentifierName(n[1]));

			for (int i = 2; i < length; i++)
			{
				q = SyntaxFactory.QualifiedName(q, SyntaxFactory.IdentifierName(n[i]));
			}

			return q;
		}

		/// <summary>
		/// Returns the actual <see cref="string"/> value represented by the specified numeric literal <paramref name="suffix"/>.
		/// </summary>
		/// <param name="suffix"><see cref="DecimalLiteralSuffix"/> to get the <see cref="string"/> represented by.</param>
		public static string? GetSuffix(DecimalLiteralSuffix suffix)
		{
			return GetSuffix(ConvertSuffix(suffix));
		}

		/// <summary>
		/// Returns the actual <see cref="string"/> value represented by the specified numeric literal <paramref name="suffix"/>.
		/// </summary>
		/// <param name="suffix"><see cref="NumericLiteralSuffix"/> to get the <see cref="string"/> represented by.</param>
		public static string? GetSuffix(NumericLiteralSuffix suffix)
		{
			return suffix switch
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
		/// Returns a <see cref="SemanticModel"/> and an <see cref="ISymbol"/> associated with the specified <paramref name="syntaxNode"/>.
		/// </summary>
		/// <param name="syntaxNode"><see cref="CSharpSyntaxNode"/> to get the <see cref="ISymbol"/> and <see cref="SemanticModel"/> of.</param>
		/// <param name="compilation">Current <see cref="ICompilationData"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="syntaxNode"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Specified <paramref name="syntaxNode"/> doesn't represent any symbols.
		/// </exception>
		public static (SemanticModel semanticModel, ISymbol symbol) GetSymbolAndSemanticModel(CSharpSyntaxNode syntaxNode, ICompilationData compilation, CancellationToken cancellationToken = default)
		{
			if (syntaxNode is null)
			{
				throw new ArgumentNullException(nameof(syntaxNode));
			}

			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(syntaxNode));
			}

			SemanticModel semanticModel = compilation.Compilation.GetSemanticModel(syntaxNode.SyntaxTree);
			ISymbol? symbol = semanticModel.GetDeclaredSymbol(syntaxNode, cancellationToken);

			if (symbol is null)
			{
				throw new ArgumentException($"Syntax node '{nameof(syntaxNode)}' doesn't represent any symbols!");
			}

			return (semanticModel, symbol);
		}

		/// <summary>
		/// Returns a <see cref="SemanticModel"/> and an <see cref="ISymbol"/> associated with the specified <paramref name="syntaxNode"/>.
		/// </summary>
		/// <typeparam name="TSymbol">Type of <see cref="ISymbol"/> to return.</typeparam>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="syntaxNode"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Specified <paramref name="syntaxNode"/> doesn't represent any symbols. -or-
		/// Specified <paramref name="syntaxNode"/> is not compatible with the <typeparamref name="TSymbol"/> symbol type.
		/// </exception>
		public static (SemanticModel semanticModel, TSymbol symbol) GetSymbolAndSemanticModel<TSymbol>(CSharpSyntaxNode syntaxNode, ICompilationData compilation) where TSymbol : class, ISymbol
		{
			(SemanticModel semanticModel, ISymbol s) = GetSymbolAndSemanticModel(syntaxNode, compilation);

			if (s is not TSymbol symbol)
			{
				throw new ArgumentException($"Specified syntax node is not compatible with the '{nameof(TSymbol)}' symbol type!");
			}

			return (semanticModel, symbol);
		}

		/// <summary>
		/// Returns a <see cref="string"/> representation of a C# keyword associated with the specified <paramref name="specialType"/> value.
		/// </summary>
		/// <param name="specialType">Value of <see cref="SpecialType"/> to get the C# keyword associated with.</param>
		public static string? GetTypeKeyword(SpecialType specialType)
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
		/// Determines whether the specified <see cref="TypeKind"/> a declaration kind.
		/// </summary>
		/// <param name="kind"><see cref="TypeKind"/> to determine whether is a declaration kind.</param>
		public static bool IsDeclarationKind(TypeKind kind)
		{
			return kind is
				TypeKind.Class or
				TypeKind.Struct or
				TypeKind.Enum or
				TypeKind.Interface or
				TypeKind.Delegate;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="value"/> is a reserved C# keyword.
		/// </summary>
		/// <param name="value">Value to check if is a C# keyword.</param>
		public static bool IsKeyword([NotNullWhen(true)] string? value)
		{
			return _keywordsHashed.Contains(value!);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="value"/> can be used as an identifier.
		/// </summary>
		/// <param name="value">Value to check if can be used as an identifier.</param>
		public static bool IsValidIdentifier([NotNullWhen(true)] string? value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return false;
			}

			string str = value!.Trim();

			if (str[0] == '@')
			{
				str = str.Substring(1, str.Length - 1);
				return SyntaxFacts.IsValidIdentifier(str);
			}

			return SyntaxFacts.IsValidIdentifier(str) && !_keywordsHashed.Contains(str);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="value"/> can be used as an identifier of a namespace.
		/// </summary>
		/// <param name="value">Value to check if can be used as an identifier of a namespace.</param>
		public static bool IsValidNamespaceIdentifier([NotNullWhen(true)] string? value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return false;
			}

			foreach (string st in value!.Split('.'))
			{
				if (!IsValidIdentifier(st))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Determines whether the <paramref name="first"/> <see cref="RefKind"/> is valid on a method when there is an overload that takes the same parameter, but with the <paramref name="second"/> <see cref="RefKind"/>.
		/// </summary>
		/// <param name="first">First <see cref="RefKind"/>.</param>
		/// <param name="second">Second <see cref="RefKind"/>.</param>
		public static bool IsValidRefKindForOverload(RefKind first, RefKind second)
		{
			return first == RefKind.None
				? second != RefKind.None
				: second == RefKind.None;
		}

		/// <summary>
		/// Returns a <see cref="string"/> that is created by joining the provided <paramref name="namespaces"/> using the dot (".") character.
		/// </summary>
		/// <param name="namespaces">Namespaces to join.</param>
		/// <exception cref="ArgumentNullException"><paramref name="namespaces"/> is <see langword="null"/>.</exception>
		public static string JoinNamespaces(IEnumerable<string> namespaces)
		{
			if (namespaces is null)
			{
				throw new ArgumentNullException(nameof(namespaces));
			}

			return string.Join(".", namespaces);
		}

		/// <summary>
		/// Converts the type keyword to its proper .NET type (<see langword="int"/> to <c>Int32</c>, <see langword="float"/> to <c>Single</c> etc.).
		/// </summary>
		/// <param name="keyword">C# keyword to convert.</param>
		/// <param name="applyNamespace">Determines whether to apply the <c>System</c> namespace in the returned type name.</param>
		/// <returns>Name of the type behind the given <paramref name="keyword"/>. -or- <paramref name="keyword"/> if it's not a C# type keyword. -or- <see cref="string.Empty"/> of the <paramref name="keyword"/> is <see langword="null"/> or empty.</returns>
		public static string KeywordToType(string? keyword, bool applyNamespace = false)
		{
			if (string.IsNullOrWhiteSpace(keyword))
			{
				return string.Empty;
			}

			string? value = keyword switch
			{
				"int" => "Int32",
				"string" => "String",
				"bool" => "Boolean",
				"float" => "Single",
				"double" => "Double",
				"decimal" => "Decimal",
				"char" => "Char",
				"long" => "Int64",
				"short" => "Int16",
				"byte" => "Byte",
				"uint" => "UInt32",
				"ulong" => "UInt64",
				"ushort" => "UInt16",
				"sbyte" => "SByte",
				"nint" => "IntPtr",
				"nuint" => "UIntPtr",
				"object" => "Object",
				"void" => "Void",
				_ => default
			};

			if (value is null)
			{
				return keyword!;
			}

			if (applyNamespace)
			{
				value = "System." + value;
			}

			return value;
		}

		/// <summary>
		/// Converts a <see cref="RefKind"/> value to an associated <see cref="SyntaxKind"/> value.
		/// </summary>
		/// <param name="kind"><see cref="RefKind"/> to convert.</param>
		public static SyntaxKind RefKindToSyntax(RefKind kind)
		{
			return kind switch
			{
				RefKind.Ref => SyntaxKind.RefKeyword,
				RefKind.Out => SyntaxKind.OutKeyword,
				RefKind.In => SyntaxKind.InKeyword,
				_ => SyntaxKind.None
			};
		}

		/// <summary>
		/// Sorts the collection of namespace names.
		/// </summary>
		/// <param name="collection">A collection of namespace names.</param>
		/// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
		public static IEnumerable<string> SortUsings(IEnumerable<string> collection)
		{
			if (collection is null)
			{
				throw new ArgumentNullException(nameof(collection));
			}

			return collection.OrderBy(n =>
			{
				if (n == "System")
				{
					return 0;
				}
				else if (n.StartsWith("System."))
				{
					return 1;
				}
				else
				{
					return 2;
				}
			}).ThenBy(n => n);
		}

		/// <summary>
		/// Converts a <see cref="SyntaxKind"/> value to an associated <see cref="RefKind"/> value.
		/// </summary>
		/// <param name="kind"><see cref="SyntaxKind"/> to convert.</param>
		public static RefKind SyntaxKindToRef(SyntaxKind kind)
		{
			return kind switch
			{
				SyntaxKind.RefKeyword => RefKind.Ref,
				SyntaxKind.InKeyword => RefKind.In,
				SyntaxKind.OutKeyword => RefKind.Out,
				_ => RefKind.None
			};
		}

		/// <summary>
		/// Converts the specified <paramref name="value"/> to an XML compatible value.
		/// </summary>
		/// <param name="value">Original value.</param>
		public static string ToXmlCompatible(string? value)
		{
			return value?.Replace('<', '{').Replace('>', '}') ?? string.Empty;
		}

		/// <summary>
		/// Converts the type name to its proper C# keyword (<c>Int32</c> to <see langword="int"/>, <c>Single</c> to <see langword="float"/> etc.).
		/// </summary>
		/// <param name="type">Type to get the associated C# keyword of.</param>
		/// <returns>Keyword that represents the given <paramref name="type"/>. -or- <paramref name="type"/> if the type name is not associated with a C# keyword. -or- <see cref="string.Empty"/> of the <paramref name="type"/> is <see langword="null"/>.</returns>
		public static string TypeToKeyword(string? type)
		{
			if (string.IsNullOrWhiteSpace(type))
			{
				return string.Empty;
			}

			return type switch
			{
				"Int32" => "int",
				"String" => "string",
				"Boolean" => "bool",
				"Single" => "float",
				"Double" => "double",
				"Decimal" => "decimal",
				"Char" => "char",
				"Int64" => "long",
				"Int16" => "short",
				"Byte" => "byte",
				"UInt32" => "uint",
				"UInt64" => "ulong",
				"UInt16" => "ushort",
				"SByte" => "sbyte",
				"IntPtr" => "nint",
				"UIntPtr" => "nuint",
				"Object" => "object",
				"Void" => "void",
				_ => type!,
			};
		}

		internal static bool ArraysAreEqual<T>(T[]? left, T[]? right)
		{
			if (left == right)
			{
				return true;
			}

			if (left is null)
			{
				return right is null;
			}

			if (right is null || left.Length != right.Length)
			{
				return false;
			}

			for (int i = 0; i < left.Length; i++)
			{
				if (!left[i]!.Equals(right[i]))
				{
					return false;
				}
			}

			return true;
		}

		internal static IEnumerable<T> ByOrder<T>(IEnumerable<T> collection, ReturnOrder order)
		{
			if (order == ReturnOrder.Root)
			{
				return collection.Reverse();
			}

			return collection;
		}

		internal static int GetArrayHashCode<T>(T[]? array)
		{
			if (array is null || array.Length == 0)
			{
				return 0;
			}

			int hashCode = 565389259;

			foreach (T item in array)
			{
				hashCode = (hashCode * -1521134295) + item?.GetHashCode() ?? 0;
			}

			return hashCode;
		}
	}
}
