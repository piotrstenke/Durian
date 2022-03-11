// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using System;

namespace Durian.Analysis.DefaultParam
{
	/// <summary>
	/// Represents a parameter of type that is a generic argument.
	/// </summary>
	public readonly struct ParameterGeneration : IEquatable<ParameterGeneration>
	{
		/// <summary>
		/// Index of generic parameter this parameter is of type of.
		/// </summary>
		public readonly int GenericParameterIndex { get; }

		/// <summary>
		/// <see cref="Microsoft.CodeAnalysis.RefKind"/> of this parameter.
		/// </summary>
		public readonly RefKind RefKind { get; }

		/// <summary>
		/// <see cref="ITypeSymbol"/> representing type of this parameter.
		/// </summary>
		public readonly ITypeSymbol Type { get; }

		/// <inheritdoc cref="ParameterGeneration(ITypeSymbol, RefKind, int)"/>
		public ParameterGeneration(ITypeSymbol type, RefKind refKind)
		{
			Type = type;
			RefKind = refKind;
			GenericParameterIndex = -1;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ParameterGeneration"/> struct.
		/// </summary>
		/// <param name="type"><see cref="ITypeSymbol"/> representing type of this parameter.</param>
		/// <param name="refKind"><see cref="Microsoft.CodeAnalysis.RefKind"/> of this parameter.</param>
		/// <param name="genericParameterIndex">Index of generic parameter this parameter is of type of.</param>
		public ParameterGeneration(ITypeSymbol type, RefKind refKind, int genericParameterIndex)
		{
			Type = type;
			RefKind = refKind;
			GenericParameterIndex = genericParameterIndex;
		}

		/// <inheritdoc/>
		public static bool operator !=(ParameterGeneration a, ParameterGeneration b)
		{
			return !(a == b);
		}

		/// <inheritdoc/>
		public static bool operator ==(ParameterGeneration a, ParameterGeneration b)
		{
			return
				a.GenericParameterIndex == b.GenericParameterIndex &&
				a.RefKind == b.RefKind &&
				SymbolEqualityComparer.Default.Equals(a.Type, b.Type);
		}

		/// <inheritdoc/>
		public readonly bool Equals(ParameterGeneration other)
		{
			return this == other;
		}

		/// <inheritdoc/>
		public override readonly bool Equals(object obj)
		{
			return obj is ParameterGeneration other && other == this;
		}

		/// <inheritdoc/>
		public override readonly int GetHashCode()
		{
			int hashCode = 2061023908;
			hashCode = (hashCode * -1521134295) + SymbolEqualityComparer.Default.GetHashCode(Type);
			hashCode = (hashCode * -1521134295) + RefKind.GetHashCode();
			hashCode = (hashCode * -1521134295) + GenericParameterIndex.GetHashCode();
			return hashCode;
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return (RefKind != RefKind.None ? $"{RefKind} " : "") + Type.Name;
		}
	}
}
