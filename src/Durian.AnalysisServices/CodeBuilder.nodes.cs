// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis
{
	public partial class CodeBuilder2
	{
		/// <summary>
		/// Writes declaration of a method.
		/// </summary>
		/// <param name="method"><see cref="MethodDeclarationSyntax"/> to copy the method signature from.</param>
		/// <param name="methodBody">Determines whether to begin a block body ('{') or an expression body ('=>').</param>
		/// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>.</exception>
		public void BeginMethodDeclaration(MethodDeclarationSyntax method, MethodBody methodBody = MethodBody.Block)
		{
			if (method is null)
			{
				throw new ArgumentNullException(nameof(method));
			}

			foreach (SyntaxToken token in method.Modifiers)
			{
				TextBuilder.Append(token.ToString());
				TextBuilder.Append(' ');
			}

			TextBuilder.Append(method.ReturnType.ToString());
			TextBuilder.Append(' ');

			TextBuilder.Append(method.Identifier.ToString());

			WriteTypeParameters(method.TypeParameterList);
			WriteParameters(method.ParameterList);

			if(method.ConstraintClauses.Any())
			{
				TextBuilder.Append(' ');
				WriteConstraints(method.ConstraintClauses);
			}

			switch (methodBody)
			{
				case MethodBody.Block:
					TextBuilder.AppendLine();
					BeginScope();
					break;

				case MethodBody.Expression:
					TextBuilder.Append(" => ");
					break;

				case MethodBody.None:
					TextBuilder.Append(';');
					TextBuilder.AppendLine();
					break;

				default:
					goto case MethodBody.Block;
			}
		}

		/// <summary>
		/// Writes a list of parameters.
		/// </summary>
		/// <param name="parameterList"><see cref="ParameterListSyntax"/> to get the parameters from.</param>
		public void WriteParameters(ParameterListSyntax? parameterList)
		{
			if (parameterList is null)
			{
				TextBuilder.Append('(');
				TextBuilder.Append(')');
				return;
			}

			WriteParameters(parameterList.Parameters);
		}

		/// <summary>
		/// Writes a list of parameters.
		/// </summary>
		/// <param name="parameters">List of parameters to write.</param>
		public void WriteParameters(SeparatedSyntaxList<ParameterSyntax> parameters)
		{
			TextBuilder.Append('(');

			if (parameters.Any())
			{
				TextBuilder.Append(parameters[0].ToString());

				for (int i = 1; i < parameters.Count; i++)
				{
					TextBuilder.Append(',');
					TextBuilder.Append(' ');
					TextBuilder.Append(parameters[i].ToString());
				}
			}

			TextBuilder.Append(')');
		}

		/// <summary>
		/// Writes a list of type parameters.
		/// </summary>
		/// <param name="typeParameterList"><see cref="TypeParameterListSyntax"/> to get the type parameters from.</param>
		public void WriteTypeParameters(TypeParameterListSyntax? typeParameterList)
		{
			if (typeParameterList is null)
			{
				return;
			}

			WriteTypeParameters(typeParameterList.Parameters);
		}

		/// <summary>
		/// Writes a list of type parameters.
		/// </summary>
		/// <param name="typeParameters">List of type parameters to write.</param>
		public void WriteTypeParameters(SeparatedSyntaxList<TypeParameterSyntax> typeParameters)
		{
			if (!typeParameters.Any())
			{
				return;
			}

			TextBuilder.Append('<');
			TextBuilder.Append(typeParameters[0].ToString());

			for (int i = 1; i < typeParameters.Count; i++)
			{
				TextBuilder.Append(',');
				TextBuilder.Append(' ');
				TextBuilder.Append(typeParameters[i].ToString());
			}

			TextBuilder.Append('>');
		}

		/// <summary>
		/// Writes a list of constraint clauses.
		/// </summary>
		/// <param name="constraintClauses">Collection of constraint clauses to write.</param>
		public void WriteConstraints(IEnumerable<TypeParameterConstraintClauseSyntax> constraintClauses)
		{
			if(constraintClauses is null)
			{
				throw new ArgumentNullException(nameof(constraintClauses));
			}

			foreach (TypeParameterConstraintClauseSyntax constraint in constraintClauses)
			{
				WriteConstraint(constraint);
			}
		}


		public void WriteConstraints(TypeParameterConstraintClauseSyntax constraintClause)
		{
			if(constraintClause is null)
			{
				throw new ArgumentNullException(nameof(constraintClause));
			}

			TextBuilder.Append("where ");
			TextBuilder.Append(constraintClause.Name.ToString());
			TextBuilder.Append(" : ");

			SeparatedSyntaxList<TypeParameterConstraintSyntax> c = constraintClause.Constraints;

			TextBuilder.Append(c[0].ToString());

			for (int i = 1; i < c.Count; i++)
			{
				TextBuilder.Append(',');
				TextBuilder.Append(' ');
				TextBuilder.Append(c[i].ToString());
			}
		}

		public void WriteConstraint(TypeParameterConstraintSyntax constraint)
		{

		}
	}
}