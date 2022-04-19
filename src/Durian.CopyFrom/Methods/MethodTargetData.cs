// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.CopyFrom.Methods
{
	/// <summary>
	/// Contains data of a <c>CopyFrom</c> method target.
	/// </summary>
	public sealed class MethodTargetData : IEquatable<MethodTargetData>
	{
		/// <summary>
		/// Determines whether to copy attributes applied to the target.
		/// </summary>
		public bool CopyAttributes { get; }

		/// <summary>
		/// <see cref="IMethodSymbol"/> of the target member.
		/// </summary>
		public IMethodSymbol Symbol { get; }

		/// <summary>
		/// Array of usings that should be used when generating syntax tree.
		/// </summary>
		public string[]? Usings { get; }

		/// <inheritdoc cref="MethodTargetData(IMethodSymbol, bool, string[])"/>
		public MethodTargetData(IMethodSymbol symbol) : this(symbol, default)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MethodTargetData"/> class.
		/// </summary>
		/// <param name="symbol"><see cref="IMethodSymbol"/> of the target member.</param>
		/// <param name="copyAttributes">Determines whether to copy attributes applied to the target.</param>
		/// <param name="usings">Array of usings that should be used when generating syntax tree.</param>
		public MethodTargetData(IMethodSymbol symbol, bool copyAttributes = default, string[]? usings = default)
		{
			Symbol = symbol;
			CopyAttributes = copyAttributes;
			Usings = usings;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return Equals(obj as MethodTargetData);
		}

		/// <inheritdoc/>
		public bool Equals(MethodTargetData? other)
		{
			return this == other;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int hashCode = 565389259;
			hashCode = (hashCode * -1521134295) + CopyAttributes.GetHashCode();
			hashCode = (hashCode * -1521134295) + SymbolEqualityComparer.Default.GetHashCode(Symbol);
			hashCode = (hashCode * -1521134295) + AnalysisUtilities.GetArrayHashCode(Usings);
			return hashCode;
		}

		/// <inheritdoc/>
		public static bool operator ==(MethodTargetData? left, MethodTargetData? right)
		{
			if (ReferenceEquals(left, right))
			{
				return true;
			}

			if (left is null || right is null)
			{
				return false;
			}

			return
				left.CopyAttributes == right.CopyAttributes &&
				SymbolEqualityComparer.Default.Equals(left.Symbol, right.Symbol) &&
				AnalysisUtilities.ArraysAreEqual(left.Usings, right.Usings);
		}

		/// <inheritdoc/>
		public static bool operator !=(MethodTargetData? a, MethodTargetData? b)
		{
			return !(a == b);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return Symbol.ToString();
		}

		/// <summary>
		/// Returns a new instance of the <see cref="MethodTargetData"/> class being a copy of the current instance, but with the specified <paramref name="symbol"/> instead.
		/// </summary>
		/// <param name="symbol"><see cref="IMethodSymbol"/> of the target member.</param>
		public MethodTargetData WithSymbol(IMethodSymbol symbol)
		{
			return new(symbol, CopyAttributes, Usings);
		}
	}
}
