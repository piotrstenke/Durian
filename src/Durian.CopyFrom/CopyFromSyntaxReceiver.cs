// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Durian.Analysis.CopyFrom
{
	/// <summary>
	/// Collects <see cref="CSharpSyntaxNode"/>s that are potential targets for the <see cref="CopyFromGenerator"/>.
	/// </summary>
	public sealed class CopyFromSyntaxReceiver : IDurianSyntaxReceiver
	{
		/// <summary>
		/// <see cref="MethodDeclarationSyntax"/>es that potentially have the <c>Durian.CopyFromAttribute</c> applied.
		/// </summary>
		public List<MethodDeclarationSyntax> CandidateMethods { get; }

		/// <summary>
		/// <see cref="TypeDeclarationSyntax"/>es that potentially have the <c>Durian.CopyFromAttribute</c> applied.
		/// </summary>
		public List<TypeDeclarationSyntax> CandidateTypes { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CopyFromSyntaxReceiver"/> class.
		/// </summary>
		public CopyFromSyntaxReceiver()
		{
			CandidateMethods = new();
			CandidateTypes = new();
		}

		/// <inheritdoc/>
		public bool IsEmpty()
		{
			return CandidateMethods.Count == 0 && CandidateTypes.Count == 0;
		}

		/// <inheritdoc/>
		public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
		{
			if (syntaxNode is MethodDeclarationSyntax method)
			{
				if (method.AttributeLists.Any())
				{
					CandidateMethods.Add(method);
				}
			}
			else if (syntaxNode is TypeDeclarationSyntax type)
			{
				if (type.AttributeLists.Any())
				{
					CandidateTypes.Add(type);
				}
			}
		}

		IEnumerable<CSharpSyntaxNode> INodeProvider.GetNodes()
		{
			return CandidateMethods.Cast<CSharpSyntaxNode>().Concat(CandidateTypes);
		}
	}
}
