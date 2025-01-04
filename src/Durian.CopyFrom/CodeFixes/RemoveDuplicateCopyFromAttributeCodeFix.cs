using Durian.Analysis.CodeFixes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.CopyFrom.CodeFixes;

/// <summary>
/// Code fix that removes a duplicate <c>Durian.CopyFromTypeAttribute</c>s.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveDuplicateCopyFromAttributeCodeFix))]
public sealed class RemoveDuplicateCopyFromAttributeCodeFix : RemoveNodeCodeFix<AttributeSyntax>
{
	/// <inheritdoc/>
	public override string Id => $"{Title} [{nameof(CopyFrom)}]";

	/// <inheritdoc/>
	public override string Title => $"Remove duplicate {CopyFromTypeAttributeProvider.TypeName}";

	/// <summary>
	/// Initializes a new instance of the <see cref="RemoveDuplicateCopyFromAttributeCodeFix"/> class.
	/// </summary>
	public RemoveDuplicateCopyFromAttributeCodeFix()
	{
	}

	/// <inheritdoc/>
	protected override DiagnosticDescriptor[] GetSupportedDiagnostics()
	{
		return new[]
		{
			CopyFromDiagnostics.DUR0206_EquivalentTarget
		};
	}
}
