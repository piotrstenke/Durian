// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using Durian.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// <see cref="CompilationData"/> with <see cref="INamedTypeSymbol"/> of code generation attributes.
	/// </summary>
	public class CompilationWithEssentialSymbols : CompilationData, ICompilationDataWithSymbols
	{
		private string? _durianGeneratedAttributeName;
		private string? _enableModuleAttributeName;
		private string? _generatedCodeAttributeName;

		private INamedTypeSymbol? _durianGeneratedAttribute;
		private INamedTypeSymbol? _enableModuleAttribute;
		private INamedTypeSymbol? _generatedCodeAttribute;

		/// <inheritdoc/>
		public INamedTypeSymbol? DurianGeneratedAttribute => IncludeType(GetDurianGeneratedAttributeName(), ref _durianGeneratedAttribute);

		/// <inheritdoc/>
		public INamedTypeSymbol? EnableModuleAttribute => IncludeType(GetEnableModuleAttributeName(), ref _enableModuleAttribute);

		/// <inheritdoc/>
		public INamedTypeSymbol? GeneratedCodeAttribute => IncludeType(GetGeneratedCodeAttributeName(), ref _generatedCodeAttribute);

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
		/// Initializes a new instance of the <see cref="CompilationWithEssentialSymbols"/> class.
		/// </summary>
		/// <param name="compilation">Current <see cref="CSharpCompilation"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		public CompilationWithEssentialSymbols(CSharpCompilation compilation) : base(compilation, false)
		{
		}

		/// <inheritdoc/>
		public override void Reset()
		{
			_durianGeneratedAttribute = default;
			_generatedCodeAttribute = default;
			_enableModuleAttribute = default;
		}

		/// <inheritdoc/>
		public override void ForceReset()
		{
			_durianGeneratedAttribute = IncludeType(GetDurianGeneratedAttributeName());
			_generatedCodeAttribute = IncludeType(GetGeneratedCodeAttributeName());
			_enableModuleAttribute = IncludeType(GetEnableModuleAttributeName());
		}

		private string GetDurianGeneratedAttributeName()
		{
			return _durianGeneratedAttributeName ??= typeof(DurianGeneratedAttribute).ToString();
		}

		private string GetGeneratedCodeAttributeName()
		{
			return _generatedCodeAttributeName ??= typeof(GeneratedCodeAttribute).ToString();
		}

		private string GetEnableModuleAttributeName()
		{
			return _enableModuleAttributeName ??= typeof(EnableModuleAttribute).ToString();
		}
	}
}
