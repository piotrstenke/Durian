// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.CopyFrom.Types
{
	/// <summary>
	/// Contains data of a <c>CopyFrom</c> type target.
	/// </summary>
	public sealed class TypeTargetData : IEquatable<TypeTargetData>
	{
		/// <summary>
		/// Determines whether to copy attributes applied to the target.
		/// </summary>
		public bool CopyAttributes { get; }

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
		/// <see cref="INamedTypeSymbol"/> of the target member.
		/// </summary>
		public INamedTypeSymbol Symbol { get; }

		/// <summary>
		/// Array of usings that should be used when generating syntax tree.
		/// </summary>
		public string[]? Usings { get; }

		/// <inheritdoc cref="TypeTargetData(INamedTypeSymbol, int, string[], TypeDeclarationSyntax?, string?, bool, bool)"/>
		public TypeTargetData(INamedTypeSymbol symbol) : this(symbol, default, default)
		{
		}

		/// <inheritdoc cref="TypeTargetData(INamedTypeSymbol, int, string[], TypeDeclarationSyntax?, string?, bool, bool)"/>
		public TypeTargetData(
			INamedTypeSymbol symbol,
			int order,
			string[]? usings
		) : this(symbol, order, usings, default, default, default, default)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeTargetData"/> struct.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> of the target member.</param>
		/// <param name="order">Order in which this target should be applied when comparing to other targets of the member.</param>
		/// <param name="usings">Array of usings that should be used when generating syntax tree.</param>
		/// <param name="partialPart">Partial part of the source type to copy the implementation from.</param>
		/// <param name="partialPartName">Name of the partial part.</param>
		/// <param name="handleSpecialMembers">Determines whether to automatically replace name of the target type in constructor, destructor and operator declarations.</param>
		/// <param name="copyAttributes">Determines whether to copy attributes applied to the target.</param>
		public TypeTargetData(
			INamedTypeSymbol symbol,
			int order,
			string[]? usings,
			TypeDeclarationSyntax? partialPart,
			string? partialPartName,
			bool handleSpecialMembers,
			bool copyAttributes
		)
		{
			Symbol = symbol;
			PartialPart = partialPart;
			PartialPartName = partialPartName;
			Order = order;
			HandleSpecialMembers = handleSpecialMembers;
			Usings = usings;
			CopyAttributes = copyAttributes;
		}

		/// <inheritdoc/>
		public static bool operator !=(TypeTargetData left, TypeTargetData right)
		{
			return !(left == right);
		}

		/// <inheritdoc/>
		public static bool operator ==(TypeTargetData left, TypeTargetData right)
		{
			return left.Equals(right);
		}

		/// <inheritdoc/>
		public override bool Equals(object? obj)
		{
			return obj is TypeTargetData other && Equals(other);
		}

		/// <inheritdoc/>
		public bool Equals(TypeTargetData other)
		{
			return
				other.Order == Order &&
				other.HandleSpecialMembers == HandleSpecialMembers &&
				other.CopyAttributes == CopyAttributes &&
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
			hashCode = (hashCode * -1521134295) + CopyAttributes.GetHashCode();
			hashCode = (hashCode * -1521134295) + PartialPartName?.GetHashCode() ?? 0;
			hashCode = (hashCode * -1521134295) + PartialPart?.GetHashCode() ?? 0;
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

		/// <summary>
		/// Returns a new instance of the <see cref="TypeTargetData"/> class being a copy of the current instance, but with the specified <paramref name="symbol"/> instead.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> of the target member.</param>
		public TypeTargetData WithSymbol(INamedTypeSymbol symbol)
		{
			return new(symbol, Order, Usings, PartialPart, PartialPartName, HandleSpecialMembers, CopyAttributes);
		}
	}
}
