using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="IMethodSymbol"/> representing an accessor.
	/// </summary>
	public interface IAccessorData : IMethodData, ISymbolOrMember<IMethodSymbol, IAccessorData>
	{
		/// <summary>
		/// Kind of this accessor.
		/// </summary>
		AccessorKind AccessorKind { get; }

		/// <summary>
		/// Target <see cref="AccessorDeclarationSyntax"/>.
		/// </summary>
		new AccessorDeclarationSyntax Declaration { get; }

		/// <summary>
		/// The <see langword="value"/> parameter of the accessor.
		/// </summary>
		ISymbolOrMember<IParameterSymbol, IParameterData>? Parameter { get; }

		/// <summary>
		/// Creates a shallow copy of the current data.
		/// </summary>
		new IAccessorData Clone();
	}
}