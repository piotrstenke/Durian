using System.Threading;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Filtration;

/// <summary>
/// Defines data that is passed to the <see cref="ISyntaxValidator{T}.ValidateAndCreate(in T, out IMemberData?)"/> method.
/// </summary>
public readonly struct SyntaxValidationContext : ISyntaxValidationContext
{
	private readonly bool _isInitialized;

	/// <inheritdoc/>
	public CancellationToken CancellationToken { get; }

	/// <summary>
	/// Determines whether this context was created using the default constructor.
	/// </summary>
	public bool IsDefault => !_isInitialized;

	/// <inheritdoc/>
	public SyntaxNode Node { get; }

	/// <inheritdoc/>
	public SemanticModel SemanticModel { get; }

	/// <inheritdoc/>
	public ISymbol Symbol { get; }

	/// <inheritdoc/>
	public ICompilationData TargetCompilation { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="SyntaxValidationContext"/> structure.
	/// </summary>
	/// <param name="compilation">Parent <see cref="ICompilationData"/> of the target <paramref name="node"/>.</param>
	/// <param name="semanticModel"><see cref="Microsoft.CodeAnalysis.SemanticModel"/> of the <paramref name="node"/>.</param>
	/// <param name="symbol"><see cref="ISymbol"/> that is represented by the <paramref name="node"/>.</param>
	/// <param name="node"><see cref="SyntaxNode"/> to validate.</param>
	/// <param name="cancellationToken"><see cref="System.Threading.CancellationToken"/> that specifies if the operation should be canceled.</param>
	public SyntaxValidationContext(
		ICompilationData compilation,
		SemanticModel semanticModel,
		ISymbol symbol,
		SyntaxNode node,
		CancellationToken cancellationToken = default
	)
	{
		TargetCompilation = compilation;
		SemanticModel = semanticModel;
		Node = node;
		Symbol = symbol;
		CancellationToken = cancellationToken;
		_isInitialized = true;
	}

	/// <summary>
	/// Converts the current <see cref="SyntaxValidationContext"/> to a <see cref="PreValidationContext"/>.
	/// </summary>
	public PreValidationContext ToValidationContext()
	{
		return new(Node, TargetCompilation, CancellationToken);
	}
}
