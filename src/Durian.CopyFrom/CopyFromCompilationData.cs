using System;
using System.Diagnostics.CodeAnalysis;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.CopyFrom;

/// <summary>
/// <see cref="CompilationData"/> that contains all <see cref="ISymbol"/>s needed to properly analyze types marked with the <c>Durian.CopyFromTypeAttribute</c> or <c>Durian.CopyFromMethodAttribute</c>.
/// </summary>
public sealed class CopyFromCompilationData : CompilationWithEssentialSymbols
{
	private INamedTypeSymbol? _copyFromMethodAttribute;
	private INamedTypeSymbol? _copyFromTypeAttribute;
	private INamedTypeSymbol? _partialNameAttribute;
	private string? _partialNameAttributeName;
	private INamedTypeSymbol? _patternAttribute;

	/// <summary>
	/// <see cref="INamedTypeSymbol"/> representing the <c>Durian.CopyFromMethodAttribute</c> class.
	/// </summary>
	public INamedTypeSymbol? CopyFromMethodAttribute => IncludeType(CopyFromMethodAttributeProvider.FullName, ref _copyFromMethodAttribute);

	/// <summary>
	/// <see cref="INamedTypeSymbol"/> representing the <c>Durian.CopyFromTypeAttribute</c> class.
	/// </summary>
	public INamedTypeSymbol? CopyFromTypeAttribute => IncludeType(CopyFromTypeAttributeProvider.FullName, ref _copyFromTypeAttribute);

	/// <inheritdoc/>
	[MemberNotNullWhen(false,
		nameof(CopyFromTypeAttribute),
		nameof(CopyFromMethodAttribute),
		nameof(PatternAttribute),
		nameof(PartialNameAttribute))]
	public override bool HasErrors
	{
		get => base.HasErrors;
		protected set => base.HasErrors = value;
	}

	/// <summary>
	/// <see cref="CSharpCompilation"/> that was passed to the constructor.
	/// </summary>
	public CSharpCompilation OriginalCompilation { get; }

	/// <summary>
	/// <see cref="INamedTypeSymbol"/> representing the <see cref="Durian.PartialNameAttribute"/> class.
	/// </summary>
	public INamedTypeSymbol? PartialNameAttribute => IncludeType(GetPartialNameAttributeName(), ref _partialNameAttribute);

	/// <summary>
	/// <see cref="INamedTypeSymbol"/> representing the <c>Durian.PatternAttribute</c> class.
	/// </summary>
	public INamedTypeSymbol? PatternAttribute => IncludeType(PatternAttributeProvider.FullName, ref _patternAttribute);

	/// <summary>
	/// Initializes a new instance of the <see cref="CopyFromCompilationData"/> class.
	/// </summary>
	/// <param name="compilation">Current <see cref="CSharpCompilation"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
	public CopyFromCompilationData(CSharpCompilation compilation) : base(compilation)
	{
		OriginalCompilation = compilation;
	}

	/// <inheritdoc/>
	public override void ForceReset()
	{
		base.ForceReset();

		_copyFromTypeAttribute = IncludeType(CopyFromTypeAttributeProvider.TypeName);
		_copyFromMethodAttribute = IncludeType(CopyFromMethodAttributeProvider.TypeName);
		_patternAttribute = IncludeType(PatternAttributeProvider.TypeName);
		_partialNameAttribute = IncludeType(GetPartialNameAttributeName());
	}

	/// <inheritdoc/>
	public override void Reset()
	{
		base.Reset();

		_copyFromTypeAttribute = default;
		_copyFromMethodAttribute = default;
		_patternAttribute = default;
		_partialNameAttribute = default;
	}

	private string GetPartialNameAttributeName()
	{
		return _partialNameAttributeName ??= typeof(PartialNameAttribute).ToString();
	}
}
