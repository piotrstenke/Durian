using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Durian.Generator.DefaultParam
{
	/// <summary>
	/// Represents a member that collides with generated DefaultParam member.
	/// </summary>
	public readonly struct CollidingMember
	{
		/// <summary>
		/// <see cref="ISymbol"/> of the member.
		/// </summary>
		public readonly ISymbol Symbol { get; }

		/// <summary>
		/// Parameters of the member.
		/// </summary>
		public readonly IParameterSymbol[]? Parameters { get; }

		/// <summary>
		/// Type parameters of the member.
		/// </summary>
		public readonly ITypeParameterSymbol[]? TypeParameters { get; }

		/// <summary>
		/// Determines whether this member is a method. If so, its full signature should be investigated when checking if in fact there is a collision.
		/// </summary>
		[MemberNotNullWhen(true, nameof(Parameters), nameof(TypeParameters))]
		public bool IsMethod => Parameters is not null && TypeParameters is not null;

		/// <summary>
		/// Determines whether this member is contained within a type.
		/// </summary>
		public bool IsChild => Symbol.ContainingType is not null;

		/// <summary>
		/// Initializes a new instance of the <see cref="CollidingMember"/> struct.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> of the member.</param>
		/// <param name="typeParameters">Type parameters of the member.</param>
		/// <param name="parameters">Parameters of the member.</param>
		public CollidingMember(ISymbol symbol, ITypeParameterSymbol[]? typeParameters = null, IParameterSymbol[]? parameters = null)
		{
			Symbol = symbol;
			Parameters = parameters;
			TypeParameters = typeParameters;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return Symbol.ToString();
		}
	}
}
