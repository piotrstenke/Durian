// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.GenericSpecialization
{
	/// <summary>
	/// Collects <see cref="CSharpSyntaxNode"/>s that are potential targets for the <see cref="GenericSpecializationGenerator"/>.
	/// </summary>
	public sealed class GenSpecSyntaxReceiver : IDurianSyntaxReceiver
	{
		/// <summary>
		/// <see cref="ClassDeclarationSyntax"/>es that potentially have the <see cref="AllowSpecializationAttribute"/> applied.
		/// </summary>
		public List<ClassDeclarationSyntax> CandidateClasses { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="GenSpecSyntaxReceiver"/> class.
		/// </summary>
		public GenSpecSyntaxReceiver()
		{
			CandidateClasses = new(64);
		}

		/// <inheritdoc/>
		public bool IsEmpty()
		{
			return CandidateClasses.Count == 0;
		}

		/// <inheritdoc/>
		public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
		{
			if (syntaxNode is ClassDeclarationSyntax decl &&
				decl.TypeParameterList is not null &&
				decl.TypeParameterList.Parameters.Any() &&
				decl.AttributeLists.Any()
			)
			{
				CandidateClasses.Add(decl);
			}
		}

		IEnumerable<CSharpSyntaxNode> INodeProvider.GetNodes()
		{
			return CandidateClasses;
		}
	}
}
