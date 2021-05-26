using System;
using Microsoft.CodeAnalysis;

namespace Durian.Generator.DefaultParam
{
	/// <summary>
	/// Represents a parameter of type that is a generic argument.
	/// </summary>
	public readonly struct ParameterGeneration : IEquatable<ParameterGeneration>
	{
		/// <summary>
		/// <see cref="ITypeSymbol"/> representing type of this parameter.
		/// </summary>
		public readonly ITypeSymbol Type { get; }

		/// <summary>
		/// <see cref="Microsoft.CodeAnalysis.RefKind"/> of this parameter.
		/// </summary>
		public readonly RefKind RefKind { get; }

		/// <summary>
		/// Index of generic parameter this parameter is of type of.
		/// </summary>
		public readonly int GenericParameterIndex { get; }

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
		public override string ToString()
		{
			return (RefKind != RefKind.None ? $"{RefKind} " : "") + Type.Name;
		}

		/// <inheritdoc/>
		public bool Equals(ParameterGeneration other)
		{
			return this == other;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is ParameterGeneration other && other == this;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int hashCode = 2061023908;
			hashCode = (hashCode * -1521134295) + Type.GetHashCode();
			hashCode = (hashCode * -1521134295) + RefKind.GetHashCode();
			hashCode = (hashCode * -1521134295) + GenericParameterIndex.GetHashCode();
			return hashCode;
		}

		/// <inheritdoc/>
		public static bool operator ==(ParameterGeneration a, ParameterGeneration b)
		{
			return a.GenericParameterIndex == b.GenericParameterIndex && a.RefKind == b.RefKind && SymbolEqualityComparer.Default.Equals(a.Type, b.Type);
		}

		/// <inheritdoc/>
		public static bool operator !=(ParameterGeneration a, ParameterGeneration b)
		{
			return !(a == b);
		}
	}
}
