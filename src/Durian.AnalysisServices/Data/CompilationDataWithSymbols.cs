using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Generator.Data
{
	/// <summary>
	/// <see cref="CompilationData"/> with <see cref="INamedTypeSymbol"/> of code generation attributes.
	/// </summary>
	public class CompilationDataWithSymbols : CompilationData, ICompilationDataWithSymbols
	{
		/// <inheritdoc/>
		public INamedTypeSymbol? DurianGeneratedAttribute { get; private set; }

		/// <inheritdoc/>
		public INamedTypeSymbol? GeneratedCodeAttribute { get; private set; }

		/// <inheritdoc/>
		public INamedTypeSymbol? EnableModuleAttribute { get; private set; }

		/// <inheritdoc/>
		[MemberNotNullWhen(false, nameof(GeneratedCodeAttribute), nameof(DurianGeneratedAttribute), nameof(EnableModuleAttribute))]
		public override bool HasErrors { get; protected set; }

		INamedTypeSymbol ICompilationDataWithSymbols.DurianGeneratedAttribute => DurianGeneratedAttribute!;
		INamedTypeSymbol ICompilationDataWithSymbols.GeneratedCodeAttribute => GeneratedCodeAttribute!;
		INamedTypeSymbol ICompilationDataWithSymbols.EnableModuleAttribute => EnableModuleAttribute!;

		/// <summary>
		/// Initializes a new instance of the <see cref="CompilationDataWithSymbols"/> class.
		/// </summary>
		/// <param name="compilation">Current <see cref="CSharpCompilation"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		public CompilationDataWithSymbols(CSharpCompilation compilation) : base(compilation)
		{
			Reset();
		}

		/// <summary>
		/// Resets all collected <see cref="ISymbol"/>s.
		/// </summary>
		public virtual void Reset()
		{
			DurianGeneratedAttribute = Compilation.GetTypeByMetadataName(typeof(DurianGeneratedAttribute).ToString());
			GeneratedCodeAttribute = Compilation.GetTypeByMetadataName(typeof(GeneratedCodeAttribute).ToString());
			EnableModuleAttribute = Compilation.GetTypeByMetadataName(typeof(EnableModuleAttribute).ToString());

			HasErrors = DurianGeneratedAttribute is null || GeneratedCodeAttribute is null || EnableModuleAttribute is null;
		}

		/// <inheritdoc/>
		protected override void OnUpdate(CSharpCompilation oldCompilation)
		{
			Reset();
		}
	}
}
