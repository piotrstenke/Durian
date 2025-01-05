using System;
using System.Diagnostics.CodeAnalysis;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.GlobalScope;

/// <summary>
/// <see cref="CompilationData"/> that contains all <see cref="ISymbol"/>s needed to properly analyze types marked with the <c>Durian.FriendClassAttribute</c>.
/// </summary>
public class GlobalScopeCompilationData : CompilationData
{
	/// <summary>
	/// <see cref="INamedTypeSymbol"/> representing the <c>Durian.GlobalScopeAttribute</c> class.
	/// </summary>
	public INamedTypeSymbol GlobalScopeAttribute { get; private set; } = default!;

	/// <inheritdoc/>
	[MemberNotNullWhen(false, nameof(GlobalScopeAttribute))]
	public override bool HasErrors
	{
		get => base.HasErrors;
		protected set => base.HasErrors = value;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="GlobalScopeCompilationData"/> class.
	/// </summary>
	/// <param name="compilation">Current <see cref="CSharpCompilation"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
	public GlobalScopeCompilationData(CSharpCompilation compilation) : base(compilation)
	{
	}

	/// <inheritdoc/>
	public override void Reset()
	{
		GlobalScopeAttribute = IncludeType(GlobalScopeAttributeProvider.FullName)!;
	}
}
