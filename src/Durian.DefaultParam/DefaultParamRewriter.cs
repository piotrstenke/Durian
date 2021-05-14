using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Generator.DefaultParam
{
	/// <summary>
	/// Creates new <see cref="CSharpSyntaxNode"/>s based on the specified DefualtParam <see cref="CSharpSyntaxNode"/>s.
	/// </summary>
	public class DefaultParamRewriter
	{
		private readonly List<TypeParameterConstraintClauseSyntax> _includedConstraints;
		private readonly List<ITypeParameterSymbol> _includedConstraintSymbols;
		private readonly TypeParameterReplacer _replacer;
		private readonly TypeParameterIdentifierCollector _collector;
		private int _numParameters;
		private int _numConstraints;

		/// <summary>
		/// <see cref="OriginalNode"/> after modification.
		/// </summary>
		public CSharpSyntaxNode CurrentNode => DeclarationBuilder.CurrentNode;

		/// <summary>
		/// Original <see cref="CSharpSyntaxNode"/>.
		/// </summary>
		public CSharpSyntaxNode OriginalNode => DeclarationBuilder.OriginalNode;

		/// <summary>
		/// <see cref="Microsoft.CodeAnalysis.SemanticModel"/> of the <see cref="OriginalNode"/>.
		/// </summary>
		public SemanticModel SemanticModel => DeclarationBuilder.SemanticModel;

		/// <summary>
		/// <see cref="IDefaultParamDeclarationBuilder"/> that is used to generate new <see cref="CSharpSyntaxNode"/>s.
		/// </summary>
		public IDefaultParamDeclarationBuilder DeclarationBuilder { get; private set; }

		/// <summary>
		/// <see cref="DefaultParamCompilationData"/> the <see cref="OriginalNode"/> is to be found in.
		/// </summary>
		public DefaultParamCompilationData? ParentCompilation
		{
			get => _collector.ParentCompilation;
			set => _collector.ParentCompilation = value;
		}

		/// <inheritdoc cref="DefaultParamRewriter(DefaultParamCompilationData?, IDefaultParamDeclarationBuilder?)"/>
		public DefaultParamRewriter() : this(null, null)
		{
		}

		/// <inheritdoc cref="DefaultParamRewriter(DefaultParamCompilationData?, IDefaultParamDeclarationBuilder?)"/>
		public DefaultParamRewriter(DefaultParamCompilationData? compilation) : this(compilation, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamRewriter"/> class.
		/// </summary>
		/// <param name="compilation">Default <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="builder"><see cref="IDefaultParamDeclarationBuilder"/> that is used to generate new <see cref="CSharpSyntaxNode"/>s.</param>
		public DefaultParamRewriter(DefaultParamCompilationData? compilation, IDefaultParamDeclarationBuilder? builder)
		{
			_includedConstraints = new List<TypeParameterConstraintClauseSyntax>();
			_includedConstraintSymbols = new List<ITypeParameterSymbol>();
			DeclarationBuilder = builder!;
			_collector = new(compilation);
			_replacer = new(_collector.OutputSymbols);
		}

		/// <summary>
		/// Resets the <see cref="DefaultParamRewriter"/>.
		/// </summary>
		public void Reset()
		{
			DeclarationBuilder = null!;
			_numParameters = 0;
			_includedConstraints.Clear();
			_includedConstraintSymbols.Clear();
			_numConstraints = 0;
			_replacer.Reset();
			// The collector.Reset() method only clears the list of symbols, and replacer.Reset() already does that
			//_collector.Reset();
		}

		/// <summary>
		/// Sets the specified <paramref name="node"/> as the <see cref="CurrentNode"/> without changing the <see cref="OriginalNode"/>.
		/// </summary>
		/// <param name="node"><see cref="CSharpSyntaxNode"/> to set as <see cref="CurrentNode"/>.</param>
		public void Emplace(CSharpSyntaxNode node)
		{
			DeclarationBuilder.Emplace(node);
		}

		/// <summary>
		/// Performs actions that are necessary to properly set the <see cref="DeclarationBuilder"/> to a new value.
		/// </summary>
		/// <param name="declBuilder"><see cref="IDefaultParamDeclarationBuilder"/> to be used as the target <see cref="DeclarationBuilder"/> from now on.</param>
		public void Acquire(IDefaultParamDeclarationBuilder declBuilder)
		{
			_includedConstraints.Clear();
			_includedConstraintSymbols.Clear();

			DeclarationBuilder = declBuilder;
			_numParameters = declBuilder.GetOriginalTypeParameterCount();

			SyntaxList<TypeParameterConstraintClauseSyntax> constraints = declBuilder.GetOriginalConstraintClauses();
			_numConstraints = constraints.Count;

			for (int i = 0; i < _numConstraints; i++)
			{
				TypeParameterConstraintClauseSyntax constraint = constraints[i];
				ISymbol? symbol = SemanticModel.GetSymbolInfo(constraint.Name).Symbol;

				_includedConstraints.Add(constraint);
				_includedConstraintSymbols.Add((symbol as ITypeParameterSymbol)!);
			}

			_replacer.Reset();
			_replacer.VisitDeclarationBody = declBuilder.VisitDeclarationBody;
			_collector.VisitDeclarationBody = declBuilder.VisitDeclarationBody;
			_collector.SemanticModel = declBuilder.SemanticModel;
			_collector.Visit(declBuilder.OriginalNode);
		}

		/// <summary>
		/// Replaces the old <see cref="ITypeParameterSymbol"/> <see cref="CSharpSyntaxNode"/>s with the <paramref name="newType"/>.
		/// </summary>
		/// <param name="oldType"><see cref="ITypeParameterSymbol"/> to be replaced.</param>
		/// <param name="newType"><see cref="string"/> to replace the encountered references to the target <see cref="ITypeParameterSymbol"/> with.</param>
		public void ReplaceType(ITypeParameterSymbol oldType, string newType)
		{
			_replacer.ParameterToReplace = oldType;
			_replacer.Replacement = SyntaxFactory.IdentifierName(newType ?? string.Empty);
			_replacer.ResetCounter();

			DeclarationBuilder.AcceptTypeParameterReplacer(_replacer);

			if (_replacer.HasModifiedConstraints)
			{
				int length = _replacer.ChangedConstraintIndices.Count;

				for (int i = 0; i < length; i++)
				{
					_includedConstraints[i] = DeclarationBuilder.GetCurrentConstraintClause(_replacer.ChangedConstraintIndices[i]);
				}
			}
		}

		/// <summary>
		/// Removes all constrains that are applied for the target <paramref name="symbol"/> in the <see cref="CurrentNode"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ITypeParameterSymbol"/> to remove the constraints of.</param>
		public void RemoveConstraintsOf(ITypeParameterSymbol symbol)
		{
			if (_numConstraints < 1)
			{
				return;
			}

			int length = _includedConstraints.Count;

			for (int i = 0; i < length; i++)
			{
				if (SymbolEqualityComparer.Default.Equals(_includedConstraintSymbols[i], symbol))
				{
					_includedConstraints.RemoveAt(i);
					_includedConstraintSymbols.RemoveAt(i);
					break;
				}
			}

			DeclarationBuilder.WithConstraintClauses(_includedConstraints);
		}

		/// <summary>
		/// Removes last (right-most) type parameters of the <see cref="CurrentNode"/>.
		/// </summary>
		public void RemoveLastTypeParameter()
		{
			if (_numParameters < 0)
			{
				return;
			}

			_numParameters--;
			DeclarationBuilder.WithTypeParameters(_numParameters);
		}
	}
}
