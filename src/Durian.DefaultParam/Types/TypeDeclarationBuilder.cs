// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace Durian.Analysis.DefaultParam.Types
{
	/// <summary>
	/// Builds a new <see cref="TypeDeclarationSyntax"/> based on the value specified in the <c>Durian.DefaultParamAttribute</c>.
	/// </summary>
	public sealed class TypeDeclarationBuilder : IDefaultParamDeclarationBuilder
	{
		private readonly List<(ConstructorDeclarationSyntax syntax, ParameterGeneration[] parameters)> _currentConstructors = new(16);

		private readonly Queue<IdentifierNameSyntax> _inheritTypeArguments = new(16);

		private GenericNameSyntax? _inheritedType;

		private HashSet<int>? _newModifierIndexes;

		private int _numNonDefaultParam;

		private int _numOriginalConstraints;

		private int _numOriginalTypeParameters;

		/// <summary>
		/// <see cref="OriginalDeclaration"/> after modification.
		/// </summary>
		public TypeDeclarationSyntax CurrentDeclaration { get; private set; }

		/// <summary>
		/// Original <see cref="TypeDeclarationSyntax"/>.
		/// </summary>
		public TypeDeclarationSyntax OriginalDeclaration { get; private set; }

		/// <summary>
		/// <see cref="Microsoft.CodeAnalysis.SemanticModel"/> of the <see cref="OriginalDeclaration"/>.
		/// </summary>
		public SemanticModel SemanticModel { get; private set; }

		CSharpSyntaxNode IDefaultParamDeclarationBuilder.CurrentNode => CurrentDeclaration;
		CSharpSyntaxNode IDefaultParamDeclarationBuilder.OriginalNode => OriginalDeclaration;
		bool IDefaultParamDeclarationBuilder.VisitDeclarationBody => true;

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeDeclarationBuilder"/> class.
		/// </summary>
		public TypeDeclarationBuilder()
		{
			CurrentDeclaration = null!;
			OriginalDeclaration = null!;
			SemanticModel = null!;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeDeclarationBuilder"/> class.
		/// </summary>
		/// <param name="data"><see cref="DefaultParamTypeData"/> to set as the <see cref="OriginalDeclaration"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public TypeDeclarationBuilder(DefaultParamTypeData data, CancellationToken cancellationToken = default)
		{
			SetData(data, cancellationToken);
		}

		/// <inheritdoc/>
		public void AcceptTypeParameterReplacer(TypeParameterReplacer replacer)
		{
			if (_inheritedType is not null)
			{
				_inheritTypeArguments.Enqueue(replacer.Replacement!);
				UpdateConstructors(replacer.ParameterToReplace!, replacer.NewType!, replacer.Replacement!);
			}

			CurrentDeclaration = (TypeDeclarationSyntax)replacer.Visit(CurrentDeclaration);
		}

		/// <summary>
		/// Sets the specified <paramref name="declaration"/> as the <see cref="CurrentDeclaration"/> without changing the <see cref="OriginalDeclaration"/>.
		/// </summary>
		/// <param name="declaration"><see cref="TypeDeclarationSyntax"/> to set as <see cref="CurrentDeclaration"/>.</param>
		public void Emplace(TypeDeclarationSyntax declaration)
		{
			CurrentDeclaration = declaration;
		}

		/// <summary>
		/// Returns the <see cref="TypeParameterConstraintClauseSyntax"/> at the specified <paramref name="index"/> in the <see cref="CurrentDeclaration"/>.
		/// </summary>
		/// <param name="index">Index of the <see cref="TypeParameterConstraintClauseSyntax"/> to get.</param>
		public TypeParameterConstraintClauseSyntax GetCurrentConstraintClause(int index)
		{
			return CurrentDeclaration.ConstraintClauses[index];
		}

		/// <summary>
		/// Returns a <see cref="SyntaxList{TNode}"/> of the <see cref="OriginalDeclaration"/>'s <see cref="TypeParameterConstraintClauseSyntax"/>es.
		/// </summary>
		public SyntaxList<TypeParameterConstraintClauseSyntax> GetOriginalConstraintClauses()
		{
			return OriginalDeclaration.ConstraintClauses;
		}

		/// <summary>
		/// Returns number of type parameters in the <see cref="OriginalDeclaration"/>.
		/// </summary>
		public int GetOriginalTypeParameterCount()
		{
			return _numOriginalTypeParameters;
		}

		/// <summary>
		/// Sets the value of <see cref="CurrentDeclaration"/> to the value of <see cref="OriginalDeclaration"/>.
		/// </summary>
		public void Reset()
		{
			CurrentDeclaration = OriginalDeclaration;
		}

		/// <summary>
		/// Sets the specified <paramref name="data"/> as the new <see cref="OriginalDeclaration"/>.
		/// </summary>
		/// <param name="data"><see cref="DefaultParamTypeData"/> to set as the new <see cref="OriginalDeclaration"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		[MemberNotNull(nameof(OriginalDeclaration), nameof(CurrentDeclaration), nameof(SemanticModel))]
		public void SetData(DefaultParamTypeData data, CancellationToken cancellationToken = default)
		{
			SemanticModel = data.SemanticModel;
			OriginalDeclaration = data.Declaration;
			_newModifierIndexes = data.NewModifierIndexes;
			_numNonDefaultParam = data.TypeParameters.NumNonDefaultParam;

			InitializeDeclaration(data, cancellationToken);

			if (data.Inherit)
			{
				InitializeInheritData(data, cancellationToken);
			}
			else
			{
				_inheritedType = null;
				_inheritTypeArguments.Clear();
				_currentConstructors.Clear();
			}
		}

		/// <summary>
		/// Replaces <see cref="TypeParameterConstraintClauseSyntax"/>es of the <see cref="CurrentDeclaration"/> with the specified collection of <see cref="TypeParameterConstraintClauseSyntax"/>es.
		/// </summary>
		/// <param name="constraintClauses">Collection of <see cref="TypeParameterConstraintClauseSyntax"/> to apply to the <see cref="CurrentDeclaration"/>.</param>
		public void WithConstraintClauses(IEnumerable<TypeParameterConstraintClauseSyntax> constraintClauses)
		{
			SyntaxList<TypeParameterConstraintClauseSyntax> clauses = DefaultParamUtilities.ApplyConstraints(constraintClauses, _numOriginalConstraints);

			if (clauses.Any())
			{
				clauses = clauses.Replace(clauses.Last(), clauses.Last().WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed));

				if (CurrentDeclaration.BaseList is not null)
				{
					CurrentDeclaration = CurrentDeclaration.WithBaseList(CurrentDeclaration.BaseList.WithTrailingTrivia(SyntaxFactory.Space));
				}
			}
			else if (CurrentDeclaration.TypeParameterList is null)
			{
				if (CurrentDeclaration.BaseList is null)
				{
					CurrentDeclaration = CurrentDeclaration.WithIdentifier(CurrentDeclaration.Identifier.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed));
				}
				else
				{
					CurrentDeclaration = CurrentDeclaration.WithIdentifier(CurrentDeclaration.Identifier.WithTrailingTrivia(SyntaxFactory.Space));
				}
			}
			else
			{
				CurrentDeclaration = CurrentDeclaration.WithTypeParameterList(CurrentDeclaration.TypeParameterList.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed));
			}

			CurrentDeclaration = CurrentDeclaration.WithConstraintClauses(clauses);
		}

		/// <summary>
		/// Determines how many type parameters of the <see cref="OriginalDeclaration"/> should the <see cref="CurrentDeclaration"/> have.
		/// </summary>
		/// <param name="count">Number of type parameters to take.</param>
		public void WithTypeParameters(int count)
		{
			if (DefaultParamUtilities.TryUpdateTypeParameters(CurrentDeclaration.TypeParameterList, count, out TypeParameterListSyntax? updated))
			{
				if (updated is null)
				{
					CurrentDeclaration = CurrentDeclaration.WithTypeParameterList(updated).WithIdentifier(CurrentDeclaration.Identifier.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed));
				}
				else
				{
					CurrentDeclaration = CurrentDeclaration.WithTypeParameterList(updated.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed));
				}
			}

			CheckInherit(count);
			SyntaxTokenList modifiers = CurrentDeclaration.Modifiers;

			if (!modifiers.Any())
			{
				SyntaxTriviaList trivia = CurrentDeclaration.Keyword.LeadingTrivia;

				if (trivia.Any())
				{
					trivia = trivia.RemoveAt(trivia.Count - 1);
					CurrentDeclaration = CurrentDeclaration.WithKeyword(CurrentDeclaration.Keyword.WithLeadingTrivia(trivia));
				}
			}

			if (DefaultParamUtilities.TryAddNewModifierForType(_newModifierIndexes, count, _numNonDefaultParam, ref modifiers))
			{
				CurrentDeclaration = CurrentDeclaration.WithModifiers(modifiers);
			}
		}

		void IDefaultParamDeclarationBuilder.Emplace(CSharpSyntaxNode node)
		{
			CurrentDeclaration = (TypeDeclarationSyntax)node;
		}

		private static ParameterGeneration[] GetParameterGeneration(IMethodSymbol symbol)
		{
			ImmutableArray<IParameterSymbol> parameters = symbol.Parameters;
			List<ParameterGeneration> list = new(parameters.Length);

			foreach (IParameterSymbol parameter in parameters)
			{
				if (parameter.Type is ITypeParameterSymbol s && s.DeclaringType is not null)
				{
					list.Add(new ParameterGeneration(s, parameter.RefKind, s.Ordinal));
				}
				else
				{
					list.Add(new ParameterGeneration(parameter.Type, parameter.RefKind));
				}
			}

			return list.ToArray();
		}

		private void CheckInherit(int count)
		{
			if (_inheritedType is not null)
			{
				TypeSyntax[] typeArguments = _inheritedType.TypeArgumentList.Arguments.ToArray();
				typeArguments[count] = _inheritTypeArguments.Dequeue();
				_inheritedType = _inheritedType.WithTypeArgumentList(SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(typeArguments)));

				if (count == 0)
				{
					CurrentDeclaration = CurrentDeclaration.WithIdentifier(CurrentDeclaration.Identifier.WithTrailingTrivia(SyntaxFactory.Space));
				}
				else
				{
					CurrentDeclaration = CurrentDeclaration.WithTypeParameterList(CurrentDeclaration.TypeParameterList!.WithTrailingTrivia(SyntaxFactory.Space));
				}

				CurrentDeclaration = CurrentDeclaration.WithBaseList(SyntaxFactory.BaseList(
					SyntaxFactory.Token(SyntaxKind.ColonToken).WithTrailingTrivia(SyntaxFactory.Space),
					SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
						SyntaxFactory.SimpleBaseType(_inheritedType).WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed))));

				CurrentDeclaration = CurrentDeclaration.WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>(GetValidConstructors()));
			}
		}

		private ConstructorDeclarationSyntax[] GetValidConstructors()
		{
			if (_currentConstructors.Count == 0)
			{
				return Array.Empty<ConstructorDeclarationSyntax>();
			}

			List<ConstructorDeclarationSyntax> constructors = new(_currentConstructors.Count);
			List<int> included = new(_currentConstructors.Count);

			int length = _currentConstructors.Count;

			for (int i = 0; i < length; i++)
			{
				ParameterGeneration[] parameters = _currentConstructors[i].parameters;
				bool include = true;

				foreach (int index in included)
				{
					if (!ShouldIncludeConstructor(index, parameters))
					{
						include = false;
						break;
					}
				}

				if (include)
				{
					included.Add(i);
					constructors.Add(_currentConstructors[i].syntax);
				}
			}

			return constructors.ToArray();

			bool ShouldIncludeConstructor(int current, ParameterGeneration[] parameters)
			{
				int length = parameters.Length;
				ParameterGeneration[] inc = _currentConstructors[current].parameters;

				if (inc.Length != parameters.Length)
				{
					return true;
				}

				for (int i = 0; i < length; i++)
				{
					ref readonly ParameterGeneration gen = ref parameters[i];
					ref readonly ParameterGeneration other = ref inc[i];

					if (!SymbolEqualityComparer.Default.Equals(gen.Type, other.Type) || AnalysisUtilities.IsValidRefKindForOverload(gen.RefKind, other.RefKind))
					{
						return true;
					}
				}

				return false;
			}
		}

		private void InitializeConstructorList(TypeDeclarationSyntax type, SemanticModel semanticModel, CancellationToken cancellationToken)
		{
			_currentConstructors.Clear();

			ConstructorDeclarationSyntax[] constructors = type.Members.OfType<ConstructorDeclarationSyntax>().ToArray();

			if (constructors.Length == 0)
			{
				return;
			}

			foreach (ConstructorDeclarationSyntax ctor in constructors)
			{
				if (ShouldIncludeConstructor(ctor))
				{
					if (semanticModel.GetDeclaredSymbol(ctor, cancellationToken) is not IMethodSymbol symbol)
					{
						continue;
					}

#pragma warning disable CA1826 // Do not use Enumerable methods on indexable collections
					ArgumentListSyntax arguments = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(
						ctor.ParameterList.Parameters.Select(p =>
							SyntaxFactory.Argument(null, p.Modifiers.FirstOrDefault(), SyntaxFactory.IdentifierName(p.Identifier)))));
#pragma warning restore CA1826 // Do not use Enumerable methods on indexable collections

					ConstructorDeclarationSyntax decl = ctor
						.WithParameterList(ctor.ParameterList.WithTrailingTrivia(SyntaxFactory.Space))
						.WithBody(SyntaxFactory.Block().WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed))
						.WithAttributeLists(SyntaxFactory.List<AttributeListSyntax>())
						.WithInitializer(SyntaxFactory.ConstructorInitializer(SyntaxKind.BaseConstructorInitializer, arguments)
							.WithColonToken(SyntaxFactory.Token(SyntaxKind.ColonToken).WithTrailingTrivia(SyntaxFactory.Space))
							.WithTrailingTrivia(SyntaxFactory.Space));

					_currentConstructors.Add((decl, GetParameterGeneration(symbol)));
				}
			}

			static bool ShouldIncludeConstructor(ConstructorDeclarationSyntax ctor)
			{
				foreach (SyntaxToken modifier in ctor.Modifiers)
				{
					if (modifier.IsKind(SyntaxKind.PublicKeyword) || modifier.IsKind(SyntaxKind.ProtectedKeyword) || modifier.IsKind(SyntaxKind.InternalKeyword))
					{
						return true;
					}
				}

				return false;
			}
		}

		[MemberNotNull(nameof(CurrentDeclaration))]
		private void InitializeDeclaration(DefaultParamTypeData data, CancellationToken cancellationToken)
		{
			TypeDeclarationSyntax type = data.Declaration;

			if (type.TypeParameterList is null || !type.TypeParameterList.Parameters.Any())
			{
				_numOriginalTypeParameters = 0;
				CurrentDeclaration = data.Declaration;
				return;
			}

			type = (TypeDeclarationSyntax)DefaultParamUtilities.InitializeDeclaration(
				type,
				data.SemanticModel,
				data.ParentCompilation,
				type.TypeParameterList,
				cancellationToken,
				out TypeParameterListSyntax updatedParameters
			);

			_numOriginalConstraints = type.ConstraintClauses.Count;
			_numOriginalTypeParameters = updatedParameters.Parameters.Count;

			CurrentDeclaration = type.WithTypeParameterList(updatedParameters);
		}

		private void InitializeInheritData(DefaultParamTypeData data, CancellationToken cancellationToken)
		{
			ref readonly TypeParameterContainer typeParameters = ref data.TypeParameters;
			TypeSyntax[] typeArguments = new TypeSyntax[typeParameters.Length];

			for (int i = 0; i < typeParameters.Length; i++)
			{
				ref readonly TypeParameterData param = ref typeParameters[i];

				typeArguments[i] = SyntaxFactory.IdentifierName(param.Symbol.Name);
			}

			_inheritedType = SyntaxFactory.GenericName(data.Declaration.Identifier, SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(typeArguments)));

			InitializeConstructorList(data.Declaration, data.SemanticModel, cancellationToken);

			CurrentDeclaration = CurrentDeclaration
				.WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>())
				.WithBaseList(null);
		}

		private void UpdateConstructors(ITypeParameterSymbol parameter, ITypeSymbol type, NameSyntax replacement)
		{
			if (_currentConstructors.Count == 0)
			{
				return;
			}

			int length = _currentConstructors.Count;

			for (int i = 0; i < length; i++)
			{
				ParameterGeneration[] parameters = _currentConstructors[i].parameters;
				int numParameters = parameters.Length;

				ConstructorDeclarationSyntax ctor = _currentConstructors[i].syntax;

				for (int j = 0; j < numParameters; j++)
				{
					ref readonly ParameterGeneration gen = ref parameters[j];

					if (gen.GenericParameterIndex == parameter.Ordinal)
					{
						parameters[j] = new ParameterGeneration(type, gen.RefKind);
						ParameterSyntax p = ctor.ParameterList.Parameters[j];

						ctor = ctor.WithParameterList(
							ctor.ParameterList.WithParameters(
								ctor.ParameterList.Parameters.Replace(p, p.WithType(replacement.WithTriviaFrom(p.Type!)).WithTriviaFrom(p))));
					}
				}

				_currentConstructors[i] = (ctor, parameters);
			}
		}
	}
}
