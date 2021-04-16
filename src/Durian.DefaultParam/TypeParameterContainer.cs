using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Durian.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.DefaultParam
{
	[DebuggerDisplay("{_parameters}")]
	public readonly struct TypeParameterContainer : IEquatable<TypeParameterContainer>
	{
		private readonly TypeParameterData[] _parameters;

		public readonly bool HasDefaultParams { get; }
		public readonly int NumDefaultParam { get; }
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
				NumDefaultParam = _parameters.Length - FirstDefaultParamIndex;
				HasDefaultParams = FirstDefaultParamIndex > -1;
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
				if (this[i].IsDefaultParam)
				{
					parameters[i] = this[i];
				}
				else if (target[i].IsDefaultParam)
				{
					parameters[i] = target[i];
				}
				else
				{
					parameters[i] = this[i];
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
			return CreateFrom(member, semanticModel, compilation.Attribute, cancellationToken);
		}

		public static TypeParameterContainer CreateFrom(MemberDeclarationSyntax member, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			return CreateFrom(member, compilation.Compilation.GetSemanticModel(member.SyntaxTree), compilation, cancellationToken);
		}

		public static TypeParameterContainer CreateFrom(ISymbol? symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			if (symbol is INamedTypeSymbol type)
			{
				return new TypeParameterContainer(type.TypeParameters.Select(p => TypeParameterData.CreateFrom(p, compilation, cancellationToken)));
			}
			else if (symbol is IMethodSymbol method)
			{
				return new TypeParameterContainer(method.TypeParameters.Select(p => TypeParameterData.CreateFrom(p, compilation, cancellationToken)));
			}

			return default;
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

		public static bool operator ==(in TypeParameterContainer first, in TypeParameterContainer second)
		{
			return first.IsEquivalentTo(second);
		}

		public static bool operator !=(in TypeParameterContainer first, in TypeParameterContainer second)
		{
			return !(first == second);
		}
	}
}
