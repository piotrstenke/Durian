using System.Threading;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Filtration
{
	/// <summary>
	/// Defines data that is passed to the <see cref="ISyntaxValidator{T}.ValidateAndCreate(in T, out IMemberData?)"/> method.
	/// </summary>
	public interface ISyntaxValidationContext
	{
		/// <summary>
		/// <see cref="System.Threading.CancellationToken"/> that specifies if the operation should be canceled.
		/// </summary>
		CancellationToken CancellationToken { get; }

		/// <summary>
		/// <see cref="SyntaxNode"/> to validate.
		/// </summary>
		SyntaxNode Node { get; }

		/// <summary>
		/// <see cref="Microsoft.CodeAnalysis.SemanticModel"/> of the <see cref="Node"/>.
		/// </summary>
		SemanticModel SemanticModel { get; }

		/// <summary>
		/// <see cref="ISymbol"/> that is represented by the <see cref="Node"/>.
		/// </summary>
		ISymbol Symbol { get; }

		/// <summary>
		/// Parent <see cref="ICompilationData"/> of the target <see cref="Node"/>.
		/// </summary>
		ICompilationData TargetCompilation { get; }
	}
}
