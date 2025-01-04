using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.CopyFrom
{
	/// <summary>
	/// Collects <see cref="CSharpSyntaxNode"/>s that are potential targets for the <see cref="CopyFromGenerator"/>.
	/// </summary>
	public sealed class CopyFromSyntaxReceiver : DurianSyntaxReceiver
	{
		/// <summary>
		/// <see cref="BaseMethodDeclarationSyntax"/>es that potentially have the <c>Durian.CopyFromAttribute</c> applied.
		/// </summary>
		public List<CSharpSyntaxNode> CandidateMethods { get; }

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
		public override bool IsEmpty()
		{
			return CandidateMethods.Count == 0 && CandidateTypes.Count == 0;
		}

		/// <inheritdoc/>
		public override IEnumerable<SyntaxNode> GetNodes()
		{
			return CandidateMethods.Cast<CSharpSyntaxNode>().Concat(CandidateTypes);
		}

		/// <inheritdoc/>
		public override bool OnVisitSyntaxNode(SyntaxNode syntaxNode)
		{
			switch (syntaxNode)
			{
				case BaseMethodDeclarationSyntax method:

					if (method is ConstructorDeclarationSyntax)
					{
						return false;
					}

					if (HasValidAttributeList(method.AttributeLists, SyntaxKind.MethodKeyword))
					{
						CandidateMethods.Add(method);
						return true;
					}

					break;

				case AccessorDeclarationSyntax accessor:

					if (HasValidAttributeList(accessor.AttributeLists, SyntaxKind.MethodKeyword))
					{
						CandidateMethods.Add(accessor);
						return true;
					}

					break;

				case LocalFunctionStatementSyntax localFunction:

					if (HasValidAttributeList(localFunction.AttributeLists, SyntaxKind.MethodKeyword))
					{
						CandidateMethods.Add(localFunction);
						return true;
					}

					break;

				case LambdaExpressionSyntax lambda:

					if (HasValidAttributeList(lambda.AttributeLists, SyntaxKind.MethodKeyword))
					{
						CandidateMethods.Add(lambda);
						return true;
					}

					break;

				case TypeDeclarationSyntax type:

					if (HasValidAttributeList(type.AttributeLists, SyntaxKind.TypeKeyword))
					{
						CandidateTypes.Add(type);
						return true;
					}

					break;
			}

			return false;

			static bool HasValidAttributeList(SyntaxList<AttributeListSyntax> list, SyntaxKind keyword)
			{
				return list.IndexOf(attr => attr.Target is null || attr.Target.Identifier.IsKind(keyword)) != -1;
			}
		}
	}
}
