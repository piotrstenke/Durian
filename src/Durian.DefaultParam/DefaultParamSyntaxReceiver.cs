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
	public sealed class DefaultParamSyntaxReceiver : DurianSyntaxReceiver
	{
		private bool _allowsCollectingLocalFunctions;

		/// <summary>
		/// Determines whether to allow collecting <see cref="LocalFunctionStatementSyntax"/>es.
		/// </summary>
		/// <remarks>If this property is set to <see langword="false"/> and <see cref="CandidateLocalFunctions"/> is not empty, it is cleared using the <see cref="List{T}.Clear()"/> method.</remarks>
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
		/// <see cref="TypeDeclarationSyntax"/>es that potentially have the <c>Durian.DefaultParamAttribute</c> applied.
		/// </summary>
		public List<DelegateDeclarationSyntax> CandidateDelegates { get; }

		/// <summary>
		/// <see cref="TypeDeclarationSyntax"/>es that potentially have the <c>Durian.DefaultParamAttribute</c> applied. -or- empty <see cref="List{T}"/> if <see cref="AllowsCollectingLocalFunctions"/> is <see langword="false"/>.
		/// </summary>
		public List<LocalFunctionStatementSyntax> CandidateLocalFunctions { get; }

		/// <summary>
		/// <see cref="TypeDeclarationSyntax"/>es that potentially have the <c>Durian.DefaultParamAttribute</c> applied.
		/// </summary>
		public List<MethodDeclarationSyntax> CandidateMethods { get; }

		/// <summary>
		/// <see cref="TypeDeclarationSyntax"/>es that potentially have the <c>Durian.DefaultParamAttribute</c> applied.
		/// </summary>
		public List<TypeDeclarationSyntax> CandidateTypes { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamSyntaxReceiver"/> class.
		/// </summary>
		public DefaultParamSyntaxReceiver()
		{
			CandidateTypes = new();
			CandidateDelegates = new();
			CandidateMethods = new();
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
		public override bool IsEmpty()
		{
			return
				CandidateTypes.Count == 0 &&
				CandidateDelegates.Count == 0 &&
				CandidateMethods.Count == 0 &&
				(CandidateLocalFunctions is null || CandidateLocalFunctions.Count == 0);
		}

		/// <inheritdoc/>
		public override IEnumerable<SyntaxNode> GetNodes()
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

		/// <inheritdoc/>
		public override bool OnVisitSyntaxNode(SyntaxNode syntaxNode)
		{
			return syntaxNode switch
			{
				TypeDeclarationSyntax t => CollectType(t),
				MethodDeclarationSyntax m => CollectMethod(m),
				DelegateDeclarationSyntax d => CollectDelegate(d),
				LocalFunctionStatementSyntax f when AllowsCollectingLocalFunctions => CollectLocalFunction(f),
				_ => false
			};
		}

		private bool CollectDelegate(DelegateDeclarationSyntax decl)
		{
			if (decl.TypeParameterList is null)
			{
				return false;
			}

			SeparatedSyntaxList<TypeParameterSyntax> parameters = decl.TypeParameterList.Parameters;

			if (parameters.Any() && parameters.Any(p => p.AttributeLists.Any()))
			{
				CandidateDelegates.Add(decl);
				return true;
			}

			return false;
		}

		private bool CollectLocalFunction(LocalFunctionStatementSyntax decl)
		{
			if (decl.TypeParameterList is null)
			{
				return false;
			}

			SeparatedSyntaxList<TypeParameterSyntax> parameters = decl.TypeParameterList.Parameters;

			if (parameters.Any() && parameters.Any(p => p.AttributeLists.Any()))
			{
				CandidateLocalFunctions!.Add(decl);
				return true;
			}

			return false;
		}

		private bool CollectMethod(MethodDeclarationSyntax decl)
		{
			if (decl.TypeParameterList is not null)
			{
				SeparatedSyntaxList<TypeParameterSyntax> parameters = decl.TypeParameterList.Parameters;

				if (parameters.Any() && parameters.Any(p => p.AttributeLists.Any()))
				{
					CandidateMethods.Add(decl);
					return true;
				}
			}

			if (decl.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword)))
			{
				CandidateMethods.Add(decl);
				return true;
			}

			return false;
		}

		private bool CollectType(TypeDeclarationSyntax decl)
		{
			if (decl.TypeParameterList is null)
			{
				return false;
			}

			SeparatedSyntaxList<TypeParameterSyntax> parameters = decl.TypeParameterList.Parameters;

			if (parameters.Any() && parameters.Any(p => p.AttributeLists.Any()))
			{
				CandidateTypes.Add(decl);
				return true;
			}

			return false;
		}
	}
}
