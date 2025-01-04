using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data;

/// <summary>
/// Encapsulates data associated with a single <see cref="IMethodSymbol"/> representing a destructor.
/// </summary>
public interface IDestructorData : IMethodData, ISymbolOrMember<IMethodSymbol, IDestructorData>
{
	/// <summary>
	/// Target <see cref="DestructorDeclarationSyntax"/>.
	/// </summary>
	new DestructorDeclarationSyntax Declaration { get; }

	/// <summary>
	/// Creates a shallow copy of the current data.
	/// </summary>
	new IDestructorData Clone();
}