using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Durian.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.DefaultParam
{
	/// <summary>
	/// Contains information about all type parameters of a member.
	/// </summary>
	[DebuggerDisplay("Length = {_parameters?.Length ?? 0}, NumDefaultParam = {NumDefaultParam}")]
	public readonly struct TypeParameterContainer : IEquatable<TypeParameterContainer>, IEnumerable<TypeParameterData>
	{
		private readonly TypeParameterData[] _parameters;

		/// <summary>
		/// Determines whether this <see cref="TypeParameterContainer"/> contains any <see cref="TypeParameterData"/> that has the <see cref="TypeParameterData.IsDefaultParam"/> property set to <see langword="true"/>.
		/// </summary>
		public readonly bool HasDefaultParams { get; }

		/// <summary>
		/// Returns a number of <see cref="TypeParameterData"/>s with a valid <see cref="DefaultParamAttribute"/>.
		/// </summary>
		public readonly int NumDefaultParam { get; }

		/// <summary>
		/// Returns a number of <see cref="TypeParameterData"/>s that aren't DefaultParam.
		/// </summary>
		public readonly int NumNonDefaultParam => _parameters.Length - NumDefaultParam;

		/// <summary>
		/// Returns a number of all <see cref="TypeParameterData"/>s.
		/// </summary>
		public readonly int Length => _parameters.Length;

		/// <summary>
		/// Returns an index of the first <see cref="TypeParameterData"/> that has the <see cref="TypeParameterData.IsDefaultParam"/> property set to <see langword="true"/>.
		/// </summary>
		public readonly int FirstDefaultParamIndex { get; }

		/// <summary>
		/// Returns a <see cref="TypeParameterData"/> at the specified <paramref name="index"/>.
		/// </summary>
		/// <param name="index">Index to get the <see cref="TypeParameterData"/> at.</param>
		/// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is out of range.</exception>
		public readonly ref readonly TypeParameterData this[int index] => ref _parameters[index];

		/// <summary>
		/// Returns a <see cref="TypeParameterData"/> at the specified <paramref name="index"/> relative to the <see cref="FirstDefaultParamIndex"/>.
		/// </summary>
		/// <param name="index">Index relative to the <see cref="FirstDefaultParamIndex"/> to get the <see cref="TypeParameterData"/> at.</param>
		public ref readonly TypeParameterData GetDefaultParamAtIndex(int index)
		{
			return ref _parameters[FirstDefaultParamIndex + index];
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeParameterContainer"/> struct.
		/// </summary>
		/// <param name="parameters">A collection of <see cref="TypeParameterData"/> this <see cref="TypeParameterContainer"/> is about to store.</param>
		public TypeParameterContainer(IEnumerable<TypeParameterData>? parameters)
		{
			if (parameters is null)
			{
				_parameters = Array.Empty<TypeParameterData>();
				FirstDefaultParamIndex = -1;
				NumDefaultParam = 0;
				HasDefaultParams = false;
			}
			else
			{
				_parameters = parameters.ToArray();
				FirstDefaultParamIndex = FindFirstDefaultParamIndex(_parameters);
				HasDefaultParams = FirstDefaultParamIndex > -1;
				NumDefaultParam = HasDefaultParams ? _parameters.Length - FirstDefaultParamIndex : 0;
			}
		}

		/// <summary>
		/// Combines <see langword="this"/> <see cref="TypeParameterContainer"/> with the specified <paramref name="target"/>.
		/// </summary>
		/// <param name="target"><see cref="TypeParameterContainer"/> to combine with <see cref="this"/> <see cref="TypeParameterContainer"/>.</param>
		public TypeParameterContainer Combine(in TypeParameterContainer target)
		{
			if (Length != target.Length)
			{
				throw new InvalidOperationException($"Both {nameof(TypeParameterContainer)}s must be the same length!");
			}

			int length = Length;

			TypeParameterData[] parameters = new TypeParameterData[length];

			for (int i = 0; i < length; i++)
			{
				ref readonly TypeParameterData thisData = ref this[i];

				if (thisData.IsDefaultParam)
				{
					parameters[i] = thisData;
				}
				else if (target[i].IsDefaultParam)
				{
					ref readonly TypeParameterData targetData = ref target[i];
					parameters[i] = new(thisData.Syntax, thisData.Symbol, thisData.SemanticModel, targetData.Attribute, targetData.TargetType);
				}
				else
				{
					parameters[i] = thisData;
				}
			}

			return new TypeParameterContainer(parameters);
		}

		/// <summary>
		/// Determines whether <see langword="this"/> <see cref="TypeParameterContainer"/> has equivalent <see cref="TypeParameterData"/>s to the specified <paramref name="target"/>.
		/// </summary>
		/// <param name="target"><see cref="TypeParameterContainer"/> to compare to <see langword="this"/> <see cref="TypeParameterContainer"/>,</param>
		/// <param name="includeNonDefaultParam">Determines whether non-DefaultParam type parameters should be included as well.</param>
		public bool IsEquivalentTo(in TypeParameterContainer target, bool includeNonDefaultParam = false)
		{
			int i;
			int length = Length;

			if (includeNonDefaultParam)
			{
				if (length != target.Length)
				{
					return false;
				}

				i = 0;
			}
			else
			{
				i = FirstDefaultParamIndex;
			}

			if (FirstDefaultParamIndex != target.FirstDefaultParamIndex)
			{
				return false;
			}

			for (; i < length; i++)
			{
				ref readonly TypeParameterData first = ref _parameters[i];
				ref readonly TypeParameterData second = ref target._parameters[i];

				if (first != second)
				{
					return false;
				}
			}

			return true;
		}

		bool IEquatable<TypeParameterContainer>.Equals(TypeParameterContainer other)
		{
			return IsEquivalentTo(in other);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (obj is TypeParameterContainer p)
			{
				return IsEquivalentTo(in p);
			}

			return false;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int hashCode = -1158322089;
			hashCode = (hashCode * -1521134295) + EqualityComparer<TypeParameterData[]>.Default.GetHashCode(_parameters);
			hashCode = (hashCode * -1521134295) + FirstDefaultParamIndex.GetHashCode();
			return hashCode;
		}

		/// <inheritdoc/>
		public IEnumerator<TypeParameterData> GetEnumerator()
		{
			return ((IEnumerable<TypeParameterData>)_parameters).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _parameters.GetEnumerator();
		}

		/// <summary>
		/// Combines two <see cref="TypeParameterContainer"/> into one.
		/// </summary>
		/// <param name="first">First <see cref="TypeParameterContainer"/>.</param>
		/// <param name="second">Second <see cref="TypeParameterContainer"/>.</param>
		public static TypeParameterContainer Combine(in TypeParameterContainer first, in TypeParameterContainer second)
		{
			return first.Combine(in second);
		}

		/// <summary>
		/// Determines whether the <paramref name="first"/> <see cref="TypeParameterContainer"/> has equivalent <see cref="TypeParameterData"/>s to the <paramref name="second"/> <see cref="TypeParameterContainer"/>.
		/// </summary>
		/// <param name="first">First <see cref="TypeParameterContainer"/>.</param>
		/// <param name="second">Second <see cref="TypeParameterContainer"/>.</param>
		/// <param name="includeNonDefaultParam">Determines whether non-DefaultParam type parameters should be included as well.</param>
		public static bool IsEquivalentTo(in TypeParameterContainer first, in TypeParameterContainer second, bool includeNonDefaultParam = false)
		{
			return first.IsEquivalentTo(in second, includeNonDefaultParam);
		}

		/// <summary>
		/// Creates a new <see cref="TypeParameterContainer"/> for the specified <paramref name="member"/>.
		/// </summary>
		/// <param name="member"><see cref="MemberDeclarationSyntax"/> to create the <see cref="TypeParameterContainer"/> for.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="member"/>.</param>
		/// <param name="defaultParamAttribute"><see cref="INamedTypeSymbol"/> representing the <see cref="DefaultParamAttribute"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static TypeParameterContainer CreateFrom(MemberDeclarationSyntax member, SemanticModel semanticModel, INamedTypeSymbol defaultParamAttribute, CancellationToken cancellationToken = default)
		{
			TypeParameterListSyntax? list = member.GetTypeParameterList();

			if (list is not null)
			{
				return new TypeParameterContainer(list.Parameters.Select(p => TypeParameterData.CreateFrom(p, semanticModel, defaultParamAttribute, cancellationToken)));
			}

			return new TypeParameterContainer(null);
		}

		/// <summary>
		/// Creates a new <see cref="TypeParameterContainer"/> for the specified <paramref name="member"/>.
		/// </summary>
		/// <param name="member"><see cref="MemberDeclarationSyntax"/> to create the <see cref="TypeParameterContainer"/> for.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="member"/>.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static TypeParameterContainer CreateFrom(MemberDeclarationSyntax member, SemanticModel semanticModel, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			return CreateFrom(member, semanticModel, compilation.MainAttribute!, cancellationToken);
		}

		/// <summary>
		/// Creates a new <see cref="TypeParameterContainer"/> for the specified <paramref name="member"/>.
		/// </summary>
		/// <param name="member"><see cref="MemberDeclarationSyntax"/> to create the <see cref="TypeParameterContainer"/> for.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static TypeParameterContainer CreateFrom(MemberDeclarationSyntax member, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			return CreateFrom(member, compilation.Compilation.GetSemanticModel(member.SyntaxTree), compilation, cancellationToken);
		}

		/// <summary>
		/// Creates a new <see cref="TypeParameterContainer"/> for the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="IMethodSymbol"/> to create the <see cref="TypeParameterContainer"/> for.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static TypeParameterContainer CreateFrom(IMethodSymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			return new TypeParameterContainer(symbol.TypeParameters.Select(p => TypeParameterData.CreateFrom(p, compilation, cancellationToken)));
		}

		/// <summary>
		/// Creates a new <see cref="TypeParameterContainer"/> for the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to create the <see cref="TypeParameterContainer"/> for.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static TypeParameterContainer CreateFrom(INamedTypeSymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			return new TypeParameterContainer(symbol.TypeParameters.Select(p => TypeParameterData.CreateFrom(p, compilation, cancellationToken)));
		}

		/// <summary>
		/// Creates a new <see cref="TypeParameterContainer"/> from the specified collection of <see cref="ITypeParameterSymbol"/>s.
		/// </summary>
		/// <param name="typeParameters">A collection of <see cref="ITypeParameterSymbol"/>s to create the <see cref="TypeParameterContainer"/> from.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static TypeParameterContainer CreateFrom(IEnumerable<ITypeParameterSymbol> typeParameters, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			return new TypeParameterContainer(typeParameters.Select(p => TypeParameterData.CreateFrom(p, compilation, cancellationToken)));
		}

		private static int FindFirstDefaultParamIndex(TypeParameterData[] parameters)
		{
			int length = parameters.Length;

			for (int i = 0; i < length; i++)
			{
				ref TypeParameterData data = ref parameters[i];

				if (data.IsDefaultParam)
				{
					return i;
				}
			}

			return -1;
		}

		/// <inheritdoc/>
		public static bool operator ==(in TypeParameterContainer first, in TypeParameterContainer second)
		{
			return first.IsEquivalentTo(second);
		}

		/// <inheritdoc/>
		public static bool operator !=(in TypeParameterContainer first, in TypeParameterContainer second)
		{
			return !(first == second);
		}

		/// <inheritdoc/>
		public static implicit operator TypeParameterData[](in TypeParameterContainer obj)
		{
			TypeParameterData[] parameters = new TypeParameterData[obj.Length];
			Array.Copy(obj._parameters, parameters, obj.Length);
			return parameters;
		}

		/// <inheritdoc/>
		public static explicit operator TypeParameterContainer(TypeParameterData[] array)
		{
			return new TypeParameterContainer(array);
		}
	}
}
