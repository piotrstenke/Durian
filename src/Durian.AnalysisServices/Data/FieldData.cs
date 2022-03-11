// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="FieldDeclarationSyntax"/>.
	/// </summary>
	public class FieldData : MemberData
	{
		/// <summary>
		/// Target <see cref="FieldDeclarationSyntax"/>.
		/// </summary>
		public new FieldDeclarationSyntax Declaration => (base.Declaration as FieldDeclarationSyntax)!;

		/// <summary>
		/// Index of this field in the <see cref="Declaration"/>.
		/// </summary>
		public int Index { get; }

		/// <summary>
		/// <see cref="IFieldSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		public new IFieldSymbol Symbol => (base.Symbol as IFieldSymbol)!;

		/// <summary>
		/// <see cref="VariableDeclaratorSyntax"/> used to declare this field. Equivalent to using <c>Declaration.Declaration.Variables[Index]</c>.
		/// </summary>
		public VariableDeclaratorSyntax Variable { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="FieldData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="FieldDeclarationSyntax"/> this <see cref="FieldData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="FieldData"/>.</param>
		/// <param name="index">Index of this field in the <paramref name="declaration"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		/// <exception cref="IndexOutOfRangeException">
		/// <paramref name="index"/> was out of range.
		/// </exception>
		public FieldData(FieldDeclarationSyntax declaration, ICompilationData compilation, int index = 0) : this(
			declaration,
			compilation,
			GetSemanticModel(compilation, declaration),
			GetVariable(declaration, index))
		{
			Index = index;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FieldData"/> class.
		/// </summary>
		/// <param name="symbol"><see cref="IFieldSymbol"/> this <see cref="FieldData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="FieldData"/>.</param>
		internal FieldData(IFieldSymbol symbol, ICompilationData compilation) : base(
			GetFieldDeclarationFromSymbol(
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
		/// Initializes a new instance of the <see cref="FieldData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="FieldDeclarationSyntax"/> this <see cref="FieldData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="FieldData"/>.</param>
		/// <param name="symbol"><see cref="IFieldSymbol"/> this <see cref="FieldData"/> represents.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="variable"><see cref="VariableDeclaratorSyntax"/> that represents the target variable.</param>
		/// <param name="index">Index of this field in the <paramref name="declaration"/>.</param>
		/// <param name="containingTypes">A collection of <see cref="ITypeData"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="containingNamespaces">A collection of <see cref="IFieldSymbol"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>s representing the <paramref name="symbol"/> attributes.</param>
		protected internal FieldData(
			FieldDeclarationSyntax declaration,
			ICompilationData compilation,
			IFieldSymbol symbol,
			SemanticModel semanticModel,
			VariableDeclaratorSyntax variable,
			int index,
			IEnumerable<ITypeData>? containingTypes = null,
			IEnumerable<INamespaceSymbol>? containingNamespaces = null,
			IEnumerable<AttributeData>? attributes = null
		) : base(
			declaration,
			compilation,
			symbol,
			semanticModel,
			containingTypes,
			containingNamespaces,
			attributes
		)
		{
			Index = index;
			Variable = variable;
		}

		private FieldData(
					FieldDeclarationSyntax declaration,
					ICompilationData compilation,
					IFieldSymbol symbol,
					SemanticModel semanticModel,
					VariableDeclaratorSyntax variable,
					int index
				) : base(declaration, compilation, symbol, semanticModel)
		{
			Variable = variable;
			Index = index;
		}

		private FieldData(
					FieldDeclarationSyntax declaration,
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
		/// Returns a collection of new <see cref="FieldData"/>s of all variables defined in the <see cref="Declaration"/>.
		/// </summary>
		public IEnumerable<FieldData> GetUnderlayingFields()
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

				yield return new FieldData(
					Declaration,
					ParentCompilation,
					(IFieldSymbol)SemanticModel.GetDeclaredSymbol(variable)!,
					SemanticModel,
					variable,
					index
				);
			}
		}

		private static FieldDeclarationSyntax GetFieldDeclarationFromSymbol(
			IFieldSymbol symbol,
			ICompilationData compilation,
			out SemanticModel semanticModel,
			out VariableDeclaratorSyntax variable,
			out int index
		)
		{
			if (symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is not VariableDeclaratorSyntax decl || decl?.Parent?.Parent is not FieldDeclarationSyntax field)
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

		private static SemanticModel GetSemanticModel(ICompilationData compilation, FieldDeclarationSyntax declaration)
		{
			if (declaration is null)
			{
				throw new ArgumentNullException(nameof(declaration));
			}

			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			return compilation.Compilation.GetSemanticModel(declaration.SyntaxTree);
		}

		private static VariableDeclaratorSyntax GetVariable(FieldDeclarationSyntax declaration, int index)
		{
			if (index < 0 || index >= declaration.Declaration.Variables.Count)
			{
				throw new IndexOutOfRangeException(nameof(index));
			}

			return declaration.Declaration.Variables[index];
		}
	}
}
