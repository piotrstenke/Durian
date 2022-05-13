// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="LocalDeclarationStatementSyntax"/>.
	/// </summary>
	public class LocalData : MemberData
	{
		/// <summary>
		/// Target <see cref="LocalDeclarationStatementSyntax"/>.
		/// </summary>
		public new LocalDeclarationStatementSyntax Declaration => (base.Declaration as LocalDeclarationStatementSyntax)!;

		/// <summary>
		/// Index of this local in the <see cref="Declaration"/>.
		/// </summary>
		public int Index { get; }

		/// <summary>
		/// <see cref="ILocalSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		public new ILocalSymbol Symbol => (base.Symbol as ILocalSymbol)!;

		/// <summary>
		/// <see cref="VariableDeclaratorSyntax"/> used to declare this field. Equivalent to using <c>Declaration.Declaration.Variables[Index]</c>.
		/// </summary>
		public VariableDeclaratorSyntax Variable { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="LocalData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="LocalDeclarationStatementSyntax"/> this <see cref="LocalData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="LocalData"/>.</param>
		/// <param name="index">Index of this field in the <paramref name="declaration"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		/// <exception cref="IndexOutOfRangeException">
		/// <paramref name="index"/> was out of range.
		/// </exception>
		public LocalData(LocalDeclarationStatementSyntax declaration, ICompilationData compilation, int index = 0) : this(
			declaration,
			compilation,
			FieldData.GetSemanticModel(compilation, declaration.Declaration),
			FieldData.GetVariable(declaration.Declaration, index))
		{
			Index = index;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LocalData"/> class.
		/// </summary>
		/// <param name="symbol"><see cref="ILocalSymbol"/> this <see cref="LocalData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="LocalData"/>.</param>
		internal LocalData(ILocalSymbol symbol, ICompilationData compilation) : base(
			GetLocalDeclarationFromSymbol(
				symbol,
				compilation,
				out SemanticModel semanticModel,
				out VariableDeclaratorSyntax var,
				out int index),
			compilation,
			symbol,
			semanticModel
		)
		{
			Variable = var;
			Index = index;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LocalData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="LocalDeclarationStatementSyntax"/> this <see cref="LocalData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="LocalData"/>.</param>
		/// <param name="symbol"><see cref="ILocalSymbol"/> this <see cref="LocalData"/> represents.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="variable"><see cref="VariableDeclaratorSyntax"/> that represents the target variable.</param>
		/// <param name="index">Index of this field in the <paramref name="declaration"/>.</param>
		/// <param name="modifiers">A collection of all modifiers applied to the <paramref name="symbol"/>.</param>
		/// <param name="containingTypes">A collection of <see cref="ITypeData"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="containingNamespaces">A collection of <see cref="ILocalSymbol"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>s representing the <paramref name="symbol"/> attributes.</param>
		protected internal LocalData(
			LocalDeclarationStatementSyntax declaration,
			ICompilationData compilation,
			ILocalSymbol symbol,
			SemanticModel semanticModel,
			VariableDeclaratorSyntax variable,
			int index,
			string[]? modifiers = null,
			IEnumerable<ITypeData>? containingTypes = null,
			IEnumerable<INamespaceSymbol>? containingNamespaces = null,
			IEnumerable<AttributeData>? attributes = null
		) : base(
			declaration,
			compilation,
			symbol,
			semanticModel,
			modifiers,
			containingTypes,
			containingNamespaces,
			attributes
		)
		{
			Index = index;
			Variable = variable;
		}

		private LocalData(
			LocalDeclarationStatementSyntax declaration,
			ICompilationData compilation,
			ILocalSymbol symbol,
			SemanticModel semanticModel,
			VariableDeclaratorSyntax variable,
			int index
		) : base(declaration, compilation, symbol, semanticModel)
		{
			Variable = variable;
			Index = index;
		}

		private LocalData(
			LocalDeclarationStatementSyntax declaration,
			ICompilationData compilation,
			SemanticModel semanticModel,
			VariableDeclaratorSyntax variable
		) : base(
			declaration,
			compilation,
			(semanticModel.GetDeclaredSymbol(variable) as IFieldSymbol)!,
			semanticModel)
		{
			Variable = variable;
		}

		/// <summary>
		/// Returns a collection of new <see cref="LocalData"/>s of all variables defined in the <see cref="Declaration"/>.
		/// </summary>
		public IEnumerable<LocalData> GetUnderlayingLocals()
		{
			int index = Index;
			int length = Declaration.Declaration.Variables.Count;

			for (int i = 0; i < length; i++)
			{
				if (i == index)
				{
					yield return this;
					continue;
				}

				VariableDeclaratorSyntax variable = Declaration.Declaration.Variables[i];

				yield return new LocalData(
					Declaration,
					ParentCompilation,
					(ILocalSymbol)SemanticModel.GetDeclaredSymbol(variable)!,
					SemanticModel,
					variable,
					index
				);
			}
		}

		private static LocalDeclarationStatementSyntax GetLocalDeclarationFromSymbol(
			ILocalSymbol symbol,
			ICompilationData compilation,
			out SemanticModel semanticModel,
			out VariableDeclaratorSyntax variable,
			out int index
		)
		{
			if (symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is not VariableDeclaratorSyntax decl || decl?.Parent?.Parent is not LocalDeclarationStatementSyntax field)
			{
				throw Exc_NoSyntaxReference(symbol);
			}

			SeparatedSyntaxList<VariableDeclaratorSyntax> variables = field.Declaration.Variables;
			int length = variables.Count;

			for (int i = 0; i < length; i++)
			{
				if (variables[i].IsEquivalentTo(decl))
				{
					index = i;
					variable = decl;
					semanticModel = compilation.Compilation.GetSemanticModel(decl.SyntaxTree);
					return field;
				}
			}

			throw Exc_NoSyntaxReference(symbol);
		}
	}
}
