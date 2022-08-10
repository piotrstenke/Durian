// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.FriendClass
{
	/// <summary>
	/// <see cref="CompilationData"/> that contains all <see cref="ISymbol"/>s needed to properly analyze types marked with the <c>Durian.FriendClassAttribute</c>.
	/// </summary>
	public class FriendClassCompilationData : CompilationData
	{
		/// <summary>
		/// <see cref="INamedTypeSymbol"/> representing the <c>Durian.FriendClassAttribute</c> class.
		/// </summary>
		public INamedTypeSymbol? FriendClassAttribute { get; private set; }

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> representing the <c>Durian.Configuration.FriendClassConfigurationAttribute</c> class.
		/// </summary>
		public INamedTypeSymbol? FriendClassConfigurationAttribute { get; private set; }

		/// <inheritdoc/>
		[MemberNotNullWhen(false, nameof(FriendClassAttribute), nameof(FriendClassConfigurationAttribute))]
		public override bool HasErrors
		{
			get => base.HasErrors;
			protected set => base.HasErrors = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FriendClassCompilationData"/> class.
		/// </summary>
		/// <param name="compilation">Current <see cref="CSharpCompilation"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		public FriendClassCompilationData(CSharpCompilation compilation) : base(compilation)
		{
			Reset();
		}

		/// <inheritdoc/>
		public override void Reset()
		{
			FriendClassAttribute = IncludeType(FriendClassAttributeProvider.FullName);
			FriendClassConfigurationAttribute = IncludeType(FriendClassConfigurationAttributeProvider.FullName);
		}
	}
}
