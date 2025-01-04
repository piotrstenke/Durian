using Durian.Analysis.CodeFixes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.CopyFrom.CodeFixes;

/// <summary>
/// Code fix that removes a duplicate or redundant <c>Durian.PatternAttribute</c>s.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveRedundantPatternAttributeCodeFix))]
public sealed class RemoveRedundantPatternAttributeCodeFix : RemoveNodeCodeFix<AttributeSyntax>
{
	/// <inheritdoc/>
	public override string Id => $"{Title} [{nameof(CopyFrom)}]";

	/// <inheritdoc/>
	public override string Title => $"Remove redundant {PatternAttributeProvider.TypeName}";

	/// <summary>
	/// Initializes a new instance of the <see cref="RemoveRedundantPatternAttributeCodeFix"/> class.
	/// </summary>
	public RemoveRedundantPatternAttributeCodeFix()
	{
	}

	/// <inheritdoc/>
	protected override DiagnosticDescriptor[] GetSupportedDiagnostics()
	{
		return new[]
		{
			CopyFromDiagnostics.DUR0215_RedundantPatternAttribute,
			CopyFromDiagnostics.DUR0216_EquivalentPatternAttribute
		};
	}
}
