// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.CopyFrom.Methods
{
	/// <summary>
	/// Contains data of a <c>CopyFrom</c> method target.
	/// </summary>
	public sealed class TargetMethodData : IEquatable<TargetMethodData>, ICloneable
	{
		/// <summary>
		/// Specifies which non-standard nodes should also be copied.
		/// </summary>
		public AdditionalNodes AdditionalNodes { get; }

		/// <summary>
		/// <see cref="IMethodSymbol"/> of the target member.
		/// </summary>
		public IMethodSymbol Symbol { get; }

		/// <summary>
		/// Array of usings that should be used when generating syntax tree.
		/// </summary>
		public string[]? Usings { get; }

		/// <inheritdoc cref="TargetMethodData(IMethodSymbol, AdditionalNodes, string[])"/>
		public TargetMethodData(IMethodSymbol symbol) : this(symbol, default)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TargetMethodData"/> class.
		/// </summary>
		/// <param name="symbol"><see cref="IMethodSymbol"/> of the target member.</param>
		/// <param name="additionalNodes">Specifies which non-standard nodes should also be copied.</param>
		/// <param name="usings">Array of usings that should be used when generating syntax tree.</param>
		public TargetMethodData(IMethodSymbol symbol, AdditionalNodes additionalNodes = default, string[]? usings = default)
		{
			Symbol = symbol;
			AdditionalNodes = additionalNodes;
			Usings = usings;
		}

		/// <inheritdoc/>
		public static bool operator !=(TargetMethodData? a, TargetMethodData? b)
		{
			return !(a == b);
		}

		/// <inheritdoc/>
		public static bool operator ==(TargetMethodData? left, TargetMethodData? right)
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
				left.AdditionalNodes == right.AdditionalNodes &&
				SymbolEqualityComparer.Default.Equals(left.Symbol, right.Symbol) &&
				AnalysisUtilities.ArraysAreEqual(left.Usings, right.Usings);
		}

		/// <inheritdoc cref="ICloneable.Clone"/>
		public TargetMethodData Clone()
		{
			return WithSymbol(Symbol);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return Equals(obj as TargetMethodData);
		}

		/// <inheritdoc/>
		public bool Equals(TargetMethodData? other)
		{
			return this == other;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int hashCode = 565389259;
			hashCode = (hashCode * -1521134295) + AdditionalNodes.GetHashCode();
			hashCode = (hashCode * -1521134295) + SymbolEqualityComparer.Default.GetHashCode(Symbol);
			hashCode = (hashCode * -1521134295) + AnalysisUtilities.GetArrayHashCode(Usings);
			return hashCode;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return Symbol.ToString();
		}

		/// <summary>
		/// Returns a new instance of the <see cref="TargetMethodData"/> class being a copy of the current instance, but with the specified <paramref name="symbol"/> instead.
		/// </summary>
		/// <param name="symbol"><see cref="IMethodSymbol"/> of the target member.</param>
		public TargetMethodData WithSymbol(IMethodSymbol symbol)
		{
			return new(symbol, AdditionalNodes, Usings);
		}

		object ICloneable.Clone()
		{
			return Clone();
		}
	}
}
