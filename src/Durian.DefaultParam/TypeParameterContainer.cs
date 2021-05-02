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
	[DebuggerDisplay("Length = {GetLength()}, NumDefaultParam = {NumDefaultParam}")]
	public readonly struct TypeParameterContainer : IEquatable<TypeParameterContainer>, IEnumerable<TypeParameterData>
	{
		private readonly TypeParameterData[] _parameters;

		public readonly bool HasDefaultParams { get; }
		public readonly int NumDefaultParam { get; }
		public readonly int NumNonDefaultParam => _parameters.Length - NumDefaultParam;
		public readonly int Length => _parameters.Length;
		public readonly int FirstDefaultParamIndex { get; }

		public readonly ref readonly TypeParameterData this[int index] => ref _parameters[index];

		public ref readonly TypeParameterData GetDefaultParamAtIndex(int index)
		{
			return ref _parameters[FirstDefaultParamIndex + index];
		}

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

		private int GetLength()
		{
			return _parameters?.Length ?? 0;
		}

		bool IEquatable<TypeParameterContainer>.Equals(TypeParameterContainer other)
		{
			return IsEquivalentTo(in other);
		}

		public override bool Equals(object obj)
		{
			if (obj is TypeParameterContainer p)
			{
				return IsEquivalentTo(in p);
			}

			return false;
		}

		public override int GetHashCode()
		{
			int hashCode = -1158322089;
			hashCode = (hashCode * -1521134295) + EqualityComparer<TypeParameterData[]>.Default.GetHashCode(_parameters);
			hashCode = (hashCode * -1521134295) + FirstDefaultParamIndex.GetHashCode();
			return hashCode;
		}

		public static TypeParameterContainer Combine(in TypeParameterContainer first, in TypeParameterContainer second)
		{
			return first.Combine(in second);
		}

		public static bool IsEquivalentTo(in TypeParameterContainer first, in TypeParameterContainer second, bool includeNonDefaultParam = false)
		{
			return first.IsEquivalentTo(in second, includeNonDefaultParam);
		}

		public static TypeParameterContainer CreateFrom(MemberDeclarationSyntax member, SemanticModel semanticModel, INamedTypeSymbol defaultParamAttribute, CancellationToken cancellationToken = default)
		{
			TypeParameterListSyntax? list = member.GetTypeParameterList();

			if (list is not null)
			{
				return new TypeParameterContainer(list.Parameters.Select(p => TypeParameterData.CreateFrom(p, semanticModel, defaultParamAttribute, cancellationToken)));
			}

			return new TypeParameterContainer(null);
		}

		public static TypeParameterContainer CreateFrom(MemberDeclarationSyntax member, SemanticModel semanticModel, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			return CreateFrom(member, semanticModel, compilation.Attribute!, cancellationToken);
		}

		public static TypeParameterContainer CreateFrom(MemberDeclarationSyntax member, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			return CreateFrom(member, compilation.Compilation.GetSemanticModel(member.SyntaxTree), compilation, cancellationToken);
		}

		public static TypeParameterContainer CreateFrom(IMethodSymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			return new TypeParameterContainer(symbol.TypeParameters.Select(p => TypeParameterData.CreateFrom(p, compilation, cancellationToken)));
		}

		public static TypeParameterContainer CreateFrom(INamedTypeSymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			return new TypeParameterContainer(symbol.TypeParameters.Select(p => TypeParameterData.CreateFrom(p, compilation, cancellationToken)));
		}

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

		public IEnumerator<TypeParameterData> GetEnumerator()
		{
			return ((IEnumerable<TypeParameterData>)_parameters).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _parameters.GetEnumerator();
		}

		public static bool operator ==(in TypeParameterContainer first, in TypeParameterContainer second)
		{
			return first.IsEquivalentTo(second);
		}

		public static bool operator !=(in TypeParameterContainer first, in TypeParameterContainer second)
		{
			return !(first == second);
		}

		public static implicit operator TypeParameterData[](in TypeParameterContainer obj)
		{
			TypeParameterData[] parameters = new TypeParameterData[obj.Length];
			Array.Copy(obj._parameters, parameters, obj.Length);
			return parameters;
		}

		public static explicit operator TypeParameterContainer(TypeParameterData[] array)
		{
			return new TypeParameterContainer(array);
		}
	}
}
