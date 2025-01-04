using Durian.Analysis.CodeFixes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Durian.Analysis.FriendClass.FriendClassDiagnostics;

namespace Durian.Analysis.FriendClass.CodeFixes;

/// <summary>
/// Code fix for diagnostics indicating that value of <c>Durian.FriendClassAttribute</c> is redundant.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveRedundantFriendClassCodeFix))]
public sealed class RemoveRedundantFriendClassCodeFix : RemoveNodeCodeFix<AttributeSyntax>
{
	/// <inheritdoc/>
	public override string Id => $"{Title} [{nameof(FriendClass)}]";

	/// <inheritdoc/>
	public override string Title => $"Remove redundant {FriendClassAttributeProvider.TypeName}";

	/// <summary>
	/// Initializes a new instance of the <see cref="RemoveRedundantFriendClassCodeFix"/> class.
	/// </summary>
	public RemoveRedundantFriendClassCodeFix()
	{
	}

	/// <inheritdoc/>
	protected override DiagnosticDescriptor[] GetSupportedDiagnostics()
	{
		return new[]
		{
			DUR0305_TypeDoesNotDeclareInternalMembers,
			DUR0312_InnerTypeIsImplicitFriend
		};
	}
}
