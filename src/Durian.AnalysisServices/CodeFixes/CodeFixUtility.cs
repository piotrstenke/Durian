using System.Collections.Generic;
using System.Threading;
using Durian.Generator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Generator.CodeFixes
{
	/// <summary>
	/// Contains some utility methods for dealing with code fixes.
	/// </summary>
	public static class CodeFixUtility
	{
		/// <summary>
		/// Determines whether the namespace of the <paramref name="targetType"/> is present in the specified <paramref name="usings"/>.
		/// </summary>
		/// <param name="semanticModel">Target <see cref="SemanticModel"/>.</param>
		/// <param name="usings">A collection of <see cref="UsingDirectiveSyntax"/>es to check.</param>
		/// <param name="targetType">Target <see cref="ITypeSymbol"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static bool HasUsingDirective(SemanticModel semanticModel, IEnumerable<UsingDirectiveSyntax> usings, ITypeSymbol targetType, CancellationToken cancellationToken)
		{
			if (targetType.ContainingNamespace is null || targetType.IsPredefinedOrDynamic((CSharpCompilation)semanticModel.Compilation))
			{
				return true;
			}

			foreach (UsingDirectiveSyntax u in usings)
			{
				if (semanticModel!.GetSymbolInfo(u, cancellationToken).Symbol is not INamedTypeSymbol n)
				{
					continue;
				}

				if (SymbolEqualityComparer.Default.Equals(n, targetType.ContainingNamespace))
				{
					return true;
				}
			}

			return false;
		}
	}
}
