// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;

namespace Durian.Analysis
{
	/// <summary>
	/// Contains some utility methods for code analysis.
	/// </summary>
	public static class AnalysisUtilities
	{
		private static readonly string[] _keywords = new string[]
		{
			"__arglist",	"__makeref",	"__reftype",	"__refvalue",
			"abstract",	 "as",		   "base",		 "bool",
			"break",		"byte",		 "case",		 "catch",
			"char",		 "checked",	  "class",		"const",
			"continue",	 "decimal",	  "default",	  "delegate",
			"do",		   "double",	   "else",		 "enum",
			"event",		"explicit",	 "extern",	   "false",
			"finally",	  "fixed",		"float",		"for",
			"foreach",	  "goto",		 "if",		   "implicit",
			"in",		   "int",		  "interface",	"internal",
			"is",		   "lock",		 "long",		 "namespace",
			"new",		  "null",		 "object",	   "operator",
			"out",		  "override",	 "params",	   "private",
			"protected",	"public",	   "readonly",	 "ref",
			"return",	   "sbyte",		"sealed",	   "short",
			"sizeof",	   "stackalloc",   "static",	   "string",
			"struct",	   "switch",	   "this",		 "throw",
			"true",		 "try",		  "typeof",	   "uint",
			"ulong",		"unchecked",	"unsafe",	   "ushort",
			"using",		"virtual",	  "volatile",	 "void",
			"while"
		};

		private static readonly HashSet<string> _keywordsHashed = new(_keywords);

		/// <summary>
		/// Modifiers a modified version of the specified <paramref name="fullyQualifiedName"/> that can be used in the XML documentation.
		/// </summary>
		/// <param name="fullyQualifiedName">Original fully qualified name.</param>
		public static string ConvertFullyQualifiedNameToXml(string? fullyQualifiedName)
		{
			return fullyQualifiedName?.Replace('<', '{').Replace('>', '}') ?? string.Empty;
		}

		/// <summary>
		/// Returns a <see cref="string"/> representing a dot-separated name.
		/// </summary>
		/// <param name="parts">Parts of the name. Each part will be separated by a dot.</param>
		/// <returns>A <see cref="string"/> representing a dot-separated name. -or-. <see cref="string.Empty"/> if the <paramref name="parts"/> were null or empty or white-space only.</returns>
		public static string CreateName(params string[]? parts)
		{
			if (parts is null || parts.Length == 0)
			{
				return string.Empty;
			}

			StringBuilder builder = new();

			foreach (string part in parts)
			{
				if (string.IsNullOrWhiteSpace(part))
				{
					continue;
				}

				builder.Append(part).Append('.');
			}

			builder.Remove(builder.Length - 2, 1);
			return builder.ToString();
		}

		/// <summary>
		/// Returns a <see cref="string"/> containing generic identifier combined of the specified <paramref name="name"/> and the collection of <paramref name="typeParameters"/>.
		/// </summary>
		/// <param name="typeParameters">Type parameters.</param>
		/// <param name="name">Actual member identifier.</param>
		/// <exception cref="ArgumentNullException"><paramref name="typeParameters"/> is <see langword="null"/>.</exception>
		public static string GetGenericName(IEnumerable<string> typeParameters, string? name)
		{
			return (name ?? string.Empty) + GetGenericName(typeParameters);
		}

		/// <summary>
		/// Returns a <see cref="string"/> containing the generic part of an identifier created from the collection of <paramref name="typeParameters"/>.
		/// </summary>
		/// <param name="typeParameters">Type parameters.</param>
		/// <exception cref="ArgumentNullException"><paramref name="typeParameters"/> is <see langword="null"/>.</exception>
		public static string GetGenericName(IEnumerable<string> typeParameters)
		{
			if (typeParameters is null)
			{
				throw new ArgumentNullException(nameof(typeParameters));
			}

			string[] parameters = typeParameters.ToArray();
			int length = parameters.Length;

			if (length == 0)
			{
				return string.Empty;
			}

			StringBuilder sb = new();
			sb.Append('<');
			sb.Append(parameters[0]);

			for (int i = 1; i < length; i++)
			{
				sb.Append(", ").Append(parameters[i]);
			}

			sb.Append('>');

			return sb.ToString();
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
		/// Determines whether the specified <paramref name="value"/> is a reserved C# keyword.
		/// </summary>
		/// <param name="value">Value to check if is a C# keyword.</param>
		public static bool IsKeyword([NotNullWhen(true)] string? value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return false;
			}

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
			if (first == RefKind.None)
			{
				return second != RefKind.None;
			}

			if (first is RefKind.In or RefKind.Ref or RefKind.Out)
			{
				return second is not RefKind.In and not RefKind.Ref and not RefKind.Out;
			}

			return false;
		}

		/// <summary>
		/// Joins the collection of <see cref="string"/>s into a <see cref="QualifiedNameSyntax"/>.
		/// </summary>
		/// <param name="names">A collection of <see cref="string"/>s to join into a <see cref="QualifiedNameSyntax"/>.</param>
		/// <returns>A <see cref="QualifiedNameSyntax"/> created by combining the <paramref name="names"/>. -or- <see langword="null"/> if there were less then 2 <paramref name="names"/> provided.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="names"/> is <see langword="null"/>.</exception>
		public static QualifiedNameSyntax? JoinIntoQualifiedName(IEnumerable<string> names)
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
				if(!left[i]!.Equals(right[i]))
				{
					return false;
				}
			}

			return true;
		}

		internal static int GetArrayHashCode<T>(T[]? array)
		{
			if(array is null || array.Length == 0)
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

		internal static IEnumerable<T> ReturnByOrder<T>(IEnumerable<T> collection, ReturnOrder order)
		{
			if (order == ReturnOrder.Root)
			{
				return collection.Reverse();
			}

			return collection;
		}

		internal static void WriteTypeNameOfParameter(ITypeSymbol type, StringBuilder sb)
		{
			if (type is INamedTypeSymbol n)
			{
				WriteParameterAsNamedType(n, sb);
			}
			else
			{
				if (type is IArrayTypeSymbol array)
				{
					WriteParameterAsArray(array, sb);
					return;
				}
				else if (type is IDynamicTypeSymbol)
				{
					sb.Append("dynamic");
				}
				else if (type is IPointerTypeSymbol pointer)
				{
					WriteTypeNameOfParameter(pointer.PointedAtType, sb);
					sb.Append('*');
					return;
				}
				else if (type is IFunctionPointerTypeSymbol)
				{
					throw new InvalidOperationException("Function pointers are not supported!");
				}
				else
				{
					sb.Append(type.Name);
				}

				if (type.NullableAnnotation == NullableAnnotation.Annotated)
				{
					sb.Append('?');
				}
			}
		}

		private static void WriteParameterAsArray(IArrayTypeSymbol array, StringBuilder sb)
		{
			ITypeSymbol element = array.ElementType;

			if (element is IArrayTypeSymbol elementArray)
			{
				Queue<IArrayTypeSymbol> childArrays = new();

				while (elementArray is not null)
				{
					childArrays.Enqueue(elementArray);
					element = elementArray.ElementType;
					elementArray = (element as IArrayTypeSymbol)!;
				}

				WriteTypeNameOfParameter(element, sb);
				WriteArrayBrackets(array);

				while (childArrays.Count > 0)
				{
					elementArray = childArrays.Dequeue();
					CheckNullable(elementArray);
					WriteArrayBrackets(elementArray);
				}
			}
			else
			{
				WriteTypeNameOfParameter(element, sb);
				WriteArrayBrackets(array);
			}

			CheckNullable(array);

			void CheckNullable(IArrayTypeSymbol a)
			{
				if (a.NullableAnnotation == NullableAnnotation.Annotated)
				{
					sb.Append('?');
				}
			}

			void WriteArrayBrackets(IArrayTypeSymbol a)
			{
				int rank = a.Rank;
				sb.Append('[');

				for (int i = 1; i < rank; i++)
				{
					sb.Append(',');
				}

				sb.Append(']');
			}
		}

		private static void WriteParameterAsNamedType(INamedTypeSymbol n, StringBuilder sb)
		{
			string name;

			if (n.IsValueType && n.Name == "Nullable" && n.TypeArguments.Length > 0)
			{
				name = n.TypeArguments[0].GetGenericName(GenericSubstitution.Arguments);
				sb.Append(TypeToKeyword(name));
				sb.Append('?');
			}
			else
			{
				name = n.TypeArguments.Length > 0 ? n.GetGenericName(GenericSubstitution.Arguments) : n.Name;
				sb.Append(TypeToKeyword(name));

				if (n.NullableAnnotation == NullableAnnotation.Annotated)
				{
					sb.Append('?');
				}
			}
		}
	}
}