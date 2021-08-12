// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Durian.Analysis.Data;
using System;
using Durian.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace Durian.Analysis.FriendClass
{
	/// <summary>
	/// <see cref="CompilationData"/> that contains all <see cref="ISymbol"/>s needed to properly analyze types marked with the <see cref="FriendClassAttribute"/>.
	/// </summary>
	public class FriendClassCompilationData : CompilationData
	{
		/// <summary>
		/// <see cref="INamedTypeSymbol"/> representing the <see cref="Durian.FriendClassAttribute"/> class.
		/// </summary>
		public INamedTypeSymbol? FriendClassAttribute { get; private set; }

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> representing the <see cref="Configuration.FriendClassConfigurationAttribute"/> class.
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
		/// Initializes a new instance of the <see cref="CompilationData"/> class.
		/// </summary>
		/// <param name="compilation">Current <see cref="CSharpCompilation"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		public FriendClassCompilationData(CSharpCompilation compilation) : base(compilation)
		{
			Reset();
		}

		/// <inheritdoc cref="ICompilationDataWithSymbols.Reset"/>
		public void Reset()
		{
			FriendClassAttribute = Compilation.GetTypeByMetadataName(typeof(FriendClassAttribute).ToString());
			FriendClassConfigurationAttribute = Compilation.GetTypeByMetadataName(typeof(FriendClassConfigurationAttribute).ToString());

			HasErrors =
				FriendClassConfigurationAttribute is null ||
				FriendClassAttribute is null;
		}

		/// <inheritdoc/>s
		protected override void OnUpdate(CSharpCompilation oldCompilation)
		{
			Reset();
		}
	}
}
