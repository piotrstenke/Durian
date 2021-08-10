// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.Analysis.FriendClass.FriendClassDiagnostics;
using Microsoft.CodeAnalysis.CSharp;
using System;

namespace Durian.Analysis.FriendClass
{
	/// <summary>
	/// Analyzes expressions that attempt to access members of a <see cref="Type"/> with at least one <see cref="FriendClassAttribute"/> specified.
	/// </summary>
#if !MAIN_PACKAGE

	[DiagnosticAnalyzer(LanguageNames.CSharp)]
#endif
#pragma warning disable RS1001 // Missing diagnostic analyzer attribute.

	public class FriendClassAccessAnalyzer : DurianAnalyzer<FriendClassCompilationData>
#pragma warning restore RS1001 // Missing diagnostic analyzer attribute.
	{
		/// <inheritdoc/>
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			DUR0302_MemberCannotBeAccessedOutsideOfFriendClass,
			DUR0308_MemberCannotBeAccessedByChildClass,
			DUR0309_TypeCannotBeAccessedByNonFriendType,
			DUR0313_MemberCannotBeAccessedByChildClassOfFriend
		);

		/// <summary>
		/// Initializes a new instance of the <see cref="FriendClassAccessAnalyzer"/> class.
		/// </summary>
		public FriendClassAccessAnalyzer()
		{
		}

		/// <inheritdoc/>
		public override void Register(IDurianAnalysisContext context, FriendClassCompilationData compilation)
		{
		}

		/// <inheritdoc/>
		protected override FriendClassCompilationData CreateCompilation(CSharpCompilation compilation)
		{
			return new FriendClassCompilationData(compilation);
		}
	}
}
