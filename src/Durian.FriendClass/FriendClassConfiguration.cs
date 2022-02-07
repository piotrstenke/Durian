// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.FriendClass
{
	/// <summary>
	/// Represents data specified by an instance of the <c>Durian.Configuration.FriendClassConfigurationAttribute</c>.
	/// </summary>
	public sealed class FriendClassConfiguration
	{
		/// <summary>
		/// Returns a new instance of <see cref="FriendClassConfiguration"/> with values set to default.
		/// </summary>
		public static FriendClassConfiguration Default => new();

		/// <summary>
		/// Determines whether sub-classes of the current type should be treated like friend types. Defaults to <see langword="false"/>.
		/// </summary>
		[MemberNotNullWhen(true, nameof(Syntax))]
		public bool AllowChildren { get; set; }

		/// <summary>
		/// Determines whether to include inherited <see langword="internal"/> members in the analysis. Defaults to <see langword="false"/>.
		/// </summary>
		[MemberNotNullWhen(true, nameof(Syntax))]
		public bool IncludeInherited { get; set; }

		/// <summary>
		/// Determines whether this configuration was created from a <see cref="AttributeSyntax"/> with no properties specified.
		/// </summary>
		[MemberNotNullWhen(true, nameof(Syntax))]
		public bool IsZeroed { get; set; }

		/// <summary>
		/// <see cref="AttributeSyntax"/> where the <c>Durian.Configuration.FriendClassConfigurationAttribute</c> is specified.
		/// </summary>
		public AttributeSyntax? Syntax { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="FriendClassConfiguration"/> class.
		/// </summary>
		public FriendClassConfiguration()
		{
		}
	}
}
