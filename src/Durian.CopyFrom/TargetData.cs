// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.CopyFrom
{
	/// <summary>
	/// Contains data of a <c>CopyFrom</c> target.
	/// </summary>
	public sealed class TargetData : IEquatable<TargetData>
	{
		/// <summary>
		/// Determines whether to copy usings from the target type's source file.
		/// </summary>
		public bool CopyUsings { get; }

		/// <summary>
		/// Determines whether to automatically replace name of the target type in constructor, destructor and operator declarations.
		/// </summary>
		public bool HandleSpecialMembers { get; }

		/// <summary>
		/// Order in which this target should be applied when comparing to other targets of the member.
		/// </summary>
		public int Order { get; }

		/// <summary>
		/// Partial part of the source type to copy the implementation from.
		/// </summary>
		public TypeDeclarationSyntax? PartialPart { get; }

		/// <summary>
		/// Name of the partial part.
		/// </summary>
		public string? PartialPartName { get; }

		/// <summary>
		/// <see cref="ISymbol"/> of the target member.
		/// </summary>
		public INamedTypeSymbol Symbol { get; }

		/// <summary>
		/// Array of usings that should be used when generating syntax tree.
		/// </summary>
		public string[]? Usings { get; }

		/// <inheritdoc cref="TargetData(INamedTypeSymbol, int, TypeDeclarationSyntax?, string?, bool, string[], bool)"/>
		public TargetData(INamedTypeSymbol symbol) : this(symbol, default, default, default)
		{
		}

		/// <inheritdoc cref="TargetData(INamedTypeSymbol, int, TypeDeclarationSyntax?, string?, bool, string[], bool)"/>
		public TargetData(
			INamedTypeSymbol symbol,
			int order,
			bool copyUsings = true,
			string[]? usings = default
		) : this(symbol, order, default, default, copyUsings, usings)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TargetData"/> struct.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> of the target member.</param>
		/// <param name="order">Order in which this target should be applied when comparing to other targets of the member.</param>
		/// <param name="partialPart">Partial part of the source type to copy the implementation from.</param>
		/// <param name="partialPartName">Name of the partial part.</param>
		/// <param name="copyUsings">Determines whether to copy usings from the target type's source file.</param>
		/// <param name="usings">Array of usings that should be used when generating syntax tree.</param>
		/// <param name="handleSpecialMembers">Determines whether to automatically replace name of the target type in constructor, destructor and operator declarations.</param>
		public TargetData(
			INamedTypeSymbol symbol,
			int order,
			TypeDeclarationSyntax? partialPart,
			string? partialPartName,
			bool copyUsings = true,
			string[]? usings = default,
			bool handleSpecialMembers = true
		)
		{
			Symbol = symbol;
			PartialPart = partialPart;
			PartialPartName = partialPartName;
			Order = order;
			HandleSpecialMembers = handleSpecialMembers;
			CopyUsings = copyUsings;
			Usings = usings;
		}

		/// <inheritdoc/>
		public static bool operator ==(TargetData left, TargetData right)
		{
			return left.Equals(right);
		}

		/// <inheritdoc/>
		public static bool operator !=(TargetData left, TargetData right)
		{
			return !(left == right);
		}

		/// <inheritdoc/>
		public override bool Equals(object? obj)
		{
			return obj is TargetData other && Equals(other);
		}

		/// <inheritdoc/>
		public bool Equals(TargetData other)
		{
			return
				other.Order == Order &&
				other.HandleSpecialMembers == HandleSpecialMembers &&
				other.CopyUsings == CopyUsings &&
				other.PartialPartName == PartialPartName &&
				other.PartialPart == PartialPart &&
				SymbolEqualityComparer.Default.Equals(other.Symbol, Symbol) &&
				AnalysisUtilities.ArraysAreEqual(other.Usings, Usings);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int hashCode = 565389259;
			hashCode = (hashCode * -1521134295) + Order.GetHashCode();
			hashCode = (hashCode * -1521134295) + HandleSpecialMembers.GetHashCode();
			hashCode = (hashCode * -1521134295) + PartialPartName?.GetHashCode() ?? 0;
			hashCode = (hashCode * -1521134295) + PartialPart?.GetHashCode() ?? 0;
			hashCode = (hashCode * -1521134295) + CopyUsings.GetHashCode();
			hashCode = (hashCode * -1521134295) + SymbolEqualityComparer.Default.GetHashCode(Symbol);
			hashCode = (hashCode * -1521134295) + AnalysisUtilities.GetArrayHashCode(Usings);
			return hashCode;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			if (string.IsNullOrWhiteSpace(PartialPartName))
			{
				return Symbol.ToString();
			}

			return $"{Symbol} (\"{PartialPart}\")";
		}
	}
}
