using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.DefaultParam
{
	public class DefaultParamSyntaxReceiver : IDurianSyntaxReceiver
	{
		public List<TypeDeclarationSyntax> CandidateTypes { get; }
		public List<DelegateDeclarationSyntax> CandidateDelegates { get; }
		public List<MethodDeclarationSyntax> CandidateMethods { get; }
		public List<LocalFunctionStatementSyntax>? CandidateLocalFunctions { get; }

		public DefaultParamSyntaxReceiver()
		{
			CandidateTypes = new List<TypeDeclarationSyntax>();
			CandidateDelegates = new List<DelegateDeclarationSyntax>();
			CandidateMethods = new List<MethodDeclarationSyntax>();
		}

		public DefaultParamSyntaxReceiver(bool collectLocalFunctions) : this()
		{
			if (collectLocalFunctions)
			{
				CandidateLocalFunctions = new List<LocalFunctionStatementSyntax>();
			}
		}

		public bool IsEmpty()
		{
			return
				CandidateTypes.Count == 0 &&
				CandidateDelegates.Count == 0 &&
				CandidateMethods.Count == 0 &&
				(CandidateLocalFunctions is null || CandidateLocalFunctions.Count == 0);
		}

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
			else if (syntaxNode is LocalFunctionStatementSyntax f && CandidateLocalFunctions is not null)
			{
				CollectLocalFunction(f);
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

		IEnumerable<CSharpSyntaxNode> IDurianSyntaxReceiver.GetCollectedNodes()
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
	}
}
