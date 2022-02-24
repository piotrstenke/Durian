// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.CopyFrom
{
	/// <summary>
	/// <see cref="CompilationData"/> that contains all <see cref="ISymbol"/>s needed to properly analyze types marked with the <c>Durian.CopyFromAttribute</c>.
	/// </summary>
	public sealed class CopyFromCompilationData : CompilationDataWithSymbols
	{
		/// <summary>
		/// <see cref="INamedTypeSymbol"/> representing the <c>Durian.CopyFromMethodAttribute</c> class.
		/// </summary>
		public INamedTypeSymbol? CopyFromMethodAttribute { get; private set; }

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> representing the <c>Durian.CopyFromTypeAttribute</c> class.
		/// </summary>
		public INamedTypeSymbol? CopyFromTypeAttribute { get; private set; }

		/// <inheritdoc/>
		[MemberNotNullWhen(false, nameof(CopyFromTypeAttribute), nameof(CopyFromTypeAttribute))]
		public override bool HasErrors
		{
			get => base.HasErrors;
			protected set => base.HasErrors = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CopyFromCompilationData"/> class.
		/// </summary>
		/// <param name="compilation">Current <see cref="CSharpCompilation"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		public CopyFromCompilationData(CSharpCompilation compilation) : base(compilation)
		{
		}

		/// <inheritdoc/>
		public override void Reset()
		{
			base.Reset();

			CopyFromTypeAttribute = Compilation.GetTypeByMetadataName(CopyFromTypeAttributeProvider.FullName);
			CopyFromMethodAttribute = Compilation.GetTypeByMetadataName(CopyFromMethodAttributeProvider.FullName);

			HasErrors = base.HasErrors || CopyFromTypeAttribute is null || CopyFromMethodAttribute is null;
		}
	}
}