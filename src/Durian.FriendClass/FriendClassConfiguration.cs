﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Runtime.CompilerServices;
using Durian.Configuration;

namespace Durian.Analysis.FriendClass
{
	/// <summary>
	/// Represents data specified by an instance of the <see cref="FriendClassConfigurationAttribute"/>.
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
		public bool AllowsChildren { get; init; }

		/// <summary>
		/// Determines whether <see langword="internal"/> members of the current <see cref="Type"/> can be accessed outside of the current assembly if an appropriate <see cref="InternalsVisibleToAttribute"/> is present.
		/// </summary>
		public bool AllowsExternalAssembly { get; init; }

		/// <summary>
		/// <see cref="AttributeSyntax"/> where the <see cref="FriendClassConfigurationAttribute"/> was specified.
		/// </summary>
		public AttributeSyntax? Syntax { get; init; }

		/// <summary>
		/// Initializes a new instance of the <see cref="FriendClassConfiguration"/> class.
		/// </summary>
		public FriendClassConfiguration()
		{
		}
	}
}