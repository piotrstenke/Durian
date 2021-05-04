using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Data
{
	/// <summary>
	/// <see cref="CompilationData"/> with <see cref="INamedTypeSymbol"/> of code generation attributes.
	/// </summary>
	public class CompilationDataWithSymbols : CompilationData
	{
		/// <summary>
		/// <see cref="INamedTypeSymbol"/> that represents the <c>GeneratedFromAttribute</c> class.
		/// </summary>
		public INamedTypeSymbol? GeneratedFromAttribute { get; private set; }

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> that represents the <c>DurianGeneratedAttribute</c> class.
		/// </summary>
		public INamedTypeSymbol? DurianGeneratedAttribute { get; private set; }

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> that represents the <see cref="System.CodeDom.Compiler.GeneratedCodeAttribute"/>.
		/// </summary>
		public INamedTypeSymbol? GeneratedCodeAttribute { get; private set; }

		/// <inheritdoc/>
		[MemberNotNullWhen(false, nameof(GeneratedFromAttribute), nameof(GeneratedCodeAttribute), nameof(DurianGeneratedAttribute))]
		public override bool HasErrors { get; protected set; }

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
			DurianGeneratedAttribute = Compilation.GetTypeByMetadataName($"{DurianStrings.GeneratorAttributesNamespace}.DurianGeneratedAttribute");
			GeneratedFromAttribute = Compilation.GetTypeByMetadataName($"{DurianStrings.GeneratorAttributesNamespace}.GeneratedFromAttribute");
			GeneratedCodeAttribute = Compilation.GetTypeByMetadataName("System.CodeDom.Compiler.GeneratedCodeAttribute");

			HasErrors = DurianGeneratedAttribute is null || GeneratedFromAttribute is null || GeneratedCodeAttribute is null;
		}

		/// <inheritdoc/>
		protected override void OnUpdate(CSharpCompilation oldCompilation)
		{
			Reset();
		}
	}
}
