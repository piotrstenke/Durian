// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.DefaultParam
{
	/// <summary>
	/// Collects <see cref="CSharpSyntaxNode"/>s that are potential targets for the <see cref="DefaultParamGenerator"/>.
	/// </summary>
	public class DefaultParamSyntaxReceiver : IDurianSyntaxReceiver
	{
		private bool _allowsCollectingLocalFunctions;

		/// <summary>
		/// Determines whether to allow collecting <see cref="LocalFunctionStatementSyntax"/>es.
		/// </summary>
		/// <remarks>If this property is set to <see langword="false"/> and <see cref="CandidateLocalFunctions"/> is not empty, it is cleared using the <see cref="List{T}.Clear"/> method.</remarks>
		public bool AllowsCollectingLocalFunctions
		{
			get => _allowsCollectingLocalFunctions;
			set
			{
				if (!value)
				{
					CandidateLocalFunctions.Clear();
				}

				_allowsCollectingLocalFunctions = value;
			}
		}

		/// <summary>
		/// <see cref="TypeDeclarationSyntax"/>es that potentially have the <see cref="DefaultParamAttribute"/> applied.
		/// </summary>
		public List<DelegateDeclarationSyntax> CandidateDelegates { get; }

		/// <summary>
		/// <see cref="TypeDeclarationSyntax"/>es that potentially have the <see cref="DefaultParamAttribute"/> applied. -or- empty <see cref="List{T}"/> if <see cref="AllowsCollectingLocalFunctions"/> is <see langword="false"/>.
		/// </summary>
		public List<LocalFunctionStatementSyntax> CandidateLocalFunctions { get; }

		/// <summary>
		/// <see cref="TypeDeclarationSyntax"/>es that potentially have the <see cref="DefaultParamAttribute"/> applied.
		/// </summary>
		public List<MethodDeclarationSyntax> CandidateMethods { get; }

		/// <summary>
		/// <see cref="TypeDeclarationSyntax"/>es that potentially have the <see cref="DefaultParamAttribute"/> applied.
		/// </summary>
		public List<TypeDeclarationSyntax> CandidateTypes { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamSyntaxReceiver"/> class.
		/// </summary>
		public DefaultParamSyntaxReceiver()
		{
			CandidateTypes = new List<TypeDeclarationSyntax>();
			CandidateDelegates = new List<DelegateDeclarationSyntax>();
			CandidateMethods = new List<MethodDeclarationSyntax>();
			CandidateLocalFunctions = new();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamSyntaxReceiver"/> class.
		/// </summary>
		/// <param name="collectLocalFunctions">Determines whether to allow collecting <see cref="LocalFunctionStatementSyntax"/>es.</param>
		public DefaultParamSyntaxReceiver(bool collectLocalFunctions) : this()
		{
			AllowsCollectingLocalFunctions = collectLocalFunctions;
		}

		/// <inheritdoc/>
		public bool IsEmpty()
		{
			return
				CandidateTypes.Count == 0 &&
				CandidateDelegates.Count == 0 &&
				CandidateMethods.Count == 0 &&
				(CandidateLocalFunctions is null || CandidateLocalFunctions.Count == 0);
		}

		/// <inheritdoc/>
		public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
		{
			if (syntaxNode is TypeDeclarationSyntax t)
			{
				CollectType(t);
			}
			else if (syntaxNode is MethodDeclarationSyntax m)
			{
				CollectMethod(m);
			}
			else if (syntaxNode is DelegateDeclarationSyntax d)
			{
				CollectDelegate(d);
			}
			else if (AllowsCollectingLocalFunctions && syntaxNode is LocalFunctionStatementSyntax f)
			{
				CollectLocalFunction(f);
			}
		}

		IEnumerable<CSharpSyntaxNode> INodeProvider.GetNodes()
		{
			foreach (MethodDeclarationSyntax m in CandidateMethods)
			{
				yield return m;
			}

			foreach (DelegateDeclarationSyntax d in CandidateDelegates)
			{
				yield return d;
			}

			foreach (TypeDeclarationSyntax t in CandidateTypes)
			{
				yield return t;
			}

			if (CandidateLocalFunctions is not null)
			{
				foreach (LocalFunctionStatementSyntax fn in CandidateLocalFunctions)
				{
					yield return fn;
				}
			}
		}

		private void CollectDelegate(DelegateDeclarationSyntax decl)
		{
			if (decl.TypeParameterList is null)
			{
				return;
			}

			SeparatedSyntaxList<TypeParameterSyntax> parameters = decl.TypeParameterList.Parameters;

			if (parameters.Any() && parameters.Any(p => p.AttributeLists.Any()))
			{
				CandidateDelegates.Add(decl);
			}
		}

		private void CollectLocalFunction(LocalFunctionStatementSyntax decl)
		{
			if (decl.TypeParameterList is null)
			{
				return;
			}

			SeparatedSyntaxList<TypeParameterSyntax> parameters = decl.TypeParameterList.Parameters;

			if (parameters.Any() && parameters.Any(p => p.AttributeLists.Any()))
			{
				CandidateLocalFunctions!.Add(decl);
			}
		}

		private void CollectMethod(MethodDeclarationSyntax decl)
		{
			if (decl.TypeParameterList is not null)
			{
				SeparatedSyntaxList<TypeParameterSyntax> parameters = decl.TypeParameterList.Parameters;

				if (parameters.Any() && parameters.Any(p => p.AttributeLists.Any()))
				{
					CandidateMethods.Add(decl);
					return;
				}
			}

			if (decl.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword)))
			{
				CandidateMethods.Add(decl);
			}
		}

		private void CollectType(TypeDeclarationSyntax decl)
		{
			if (decl.TypeParameterList is null)
			{
				return;
			}

			SeparatedSyntaxList<TypeParameterSyntax> parameters = decl.TypeParameterList.Parameters;

			if (parameters.Any() && parameters.Any(p => p.AttributeLists.Any()))
			{
				CandidateTypes.Add(decl);
			}
		}
	}
}
