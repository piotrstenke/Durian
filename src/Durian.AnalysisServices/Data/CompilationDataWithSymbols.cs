// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// <see cref="CompilationData"/> with <see cref="INamedTypeSymbol"/> of code generation attributes.
	/// </summary>
	public class CompilationDataWithSymbols : CompilationData, ICompilationDataWithSymbols
	{
		/// <inheritdoc/>
		public INamedTypeSymbol? DurianGeneratedAttribute { get; private set; }

		/// <inheritdoc/>
		public INamedTypeSymbol? EnableModuleAttribute { get; private set; }

		/// <inheritdoc/>
		public INamedTypeSymbol? GeneratedCodeAttribute { get; private set; }

		/// <inheritdoc/>
		[MemberNotNullWhen(false, nameof(GeneratedCodeAttribute), nameof(DurianGeneratedAttribute), nameof(EnableModuleAttribute))]
		public override bool HasErrors
		{
			get => base.HasErrors;
			protected set => base.HasErrors = value;
		}

		INamedTypeSymbol ICompilationDataWithSymbols.DurianGeneratedAttribute => DurianGeneratedAttribute!;

		INamedTypeSymbol ICompilationDataWithSymbols.EnableModuleAttribute => EnableModuleAttribute!;

		INamedTypeSymbol ICompilationDataWithSymbols.GeneratedCodeAttribute => GeneratedCodeAttribute!;

		/// <summary>
		/// Initializes a new instance of the <see cref="CompilationDataWithSymbols"/> class.
		/// </summary>
		/// <param name="compilation">Current <see cref="CSharpCompilation"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		public CompilationDataWithSymbols(CSharpCompilation compilation) : base(compilation)
		{
		}

		/// <inheritdoc/>
		public override void Reset()
		{
			DurianGeneratedAttribute = IncludeType(typeof(DurianGeneratedAttribute).ToString());
			GeneratedCodeAttribute = IncludeType(typeof(GeneratedCodeAttribute).ToString());
			EnableModuleAttribute = IncludeType(typeof(EnableModuleAttribute).ToString());
		}
	}
}
