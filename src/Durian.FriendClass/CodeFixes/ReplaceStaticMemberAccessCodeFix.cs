// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Linq;
using Durian.Analysis.CodeFixes;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Durian.Analysis.FriendClass.FriendClassDiagnostics;

namespace Durian.Analysis.FriendClass.CodeFixes
{
	/// <summary>
	/// Code fix for the <see cref="DUR0314_DoNotAccessInheritedStaticMembers"/> diagnostic.
	/// </summary>
	public sealed class ReplaceStaticMemberAccessCodeFix : ReplaceNodeCodeFix<MemberAccessExpressionSyntax>
	{
		/// <inheritdoc/>
		public override string Id => $"{Title} [{nameof(FriendClass)}]";

		/// <inheritdoc/>
		public override string Title => "Replace inherited static member access";

		/// <summary>
		/// Initializes a new instance of the <see cref="ReplaceStaticMemberAccessCodeFix"/> class.
		/// </summary>
		public ReplaceStaticMemberAccessCodeFix()
		{
		}

		/// <inheritdoc/>
		protected override SyntaxNode GetNewNode(MemberAccessExpressionSyntax currentNode, CSharpCompilation compilation, SemanticModel semanticModel, out INamespaceSymbol[]? requiredNamespaces)
		{
			if (semanticModel.GetSymbolInfo(currentNode.Expression).Symbol is not INamedTypeSymbol currentType)
			{
				requiredNamespaces = default;
				return currentNode;
			}

			if (semanticModel.GetSymbolInfo(currentNode.Name).Symbol is not ISymbol currentSymbol || currentSymbol.ContainingType is not INamedTypeSymbol parentType)
			{
				requiredNamespaces = default;
				return currentNode;
			}

			requiredNamespaces = new INamespaceSymbol[] { parentType.ContainingNamespace };

			NameSyntax name;

			if (parentType.IsInnerType())
			{
				name = AnalysisUtilities.GetQualifiedName(parentType.GetContainingTypes(true).Select(t => t.Name))!;
			}
			else
			{
				name = SyntaxFactory.IdentifierName(parentType.Name);
			}

			return currentNode.WithExpression(name);
		}

		/// <inheritdoc/>
		protected override DiagnosticDescriptor[] GetSupportedDiagnostics()
		{
			return new[]
			{
				DUR0314_DoNotAccessInheritedStaticMembers
			};
		}
	}
}
