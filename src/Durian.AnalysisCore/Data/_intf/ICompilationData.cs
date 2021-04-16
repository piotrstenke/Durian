using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Data
{
	/// <summary>
	/// Base interface for all classes that encapsulate the data needed during a single generator pass.
	/// </summary>
	public interface ICompilationData
	{
		/// <summary>
		/// Current <see cref="Microsoft.CodeAnalysis.Compilation"/>.
		/// </summary>
		CSharpCompilation Compilation { get; }

		/// <summary>
		/// Determines whether there were any errors detected when validation the <see cref="Compilation"/>.
		/// </summary>
		bool HasErrors { get; }

		/// <summary>
		/// Adds the <paramref name="tree"/> to the <see cref="Compilation"/>.
		/// </summary>
		/// <param name="tree"><see cref="CSharpSyntaxTree"/> to add.</param>
		void UpdateCompilation(CSharpSyntaxTree? tree);

		/// <summary>
		/// Replaces the <paramref name="original"/> <see cref="SyntaxTree"/> with the <paramref name="updated"/> one.
		/// </summary>
		/// <param name="original"><see cref="CSharpSyntaxTree"/> to replace.</param>
		/// <param name="updated"><see cref="CSharpSyntaxTree"/> to replace the <paramref name="original"/> by.</param>
		void UpdateCompilation(CSharpSyntaxTree? original, CSharpSyntaxTree? updated);

		/// <summary>
		/// Adds the following <paramref name="trees"/> to the <see cref="Compilation"/>.
		/// </summary>
		/// <param name="trees"><see cref="CSharpSyntaxTree"/>s to add.</param>
		void UpdateCompilation(IEnumerable<CSharpSyntaxTree>? trees);

		/// <summary>
		/// Adds the <paramref name="reference"/> to the <see cref="Compilation"/>.
		/// </summary>
		/// <param name="reference"><see cref="MetadataReference"/> to add.</param>
		void UpdateCompilation(MetadataReference? reference);

		/// <summary>
		/// Replaces the <paramref name="original"/> <see cref="MetadataReference"/> with the <paramref name="updated"/> one.
		/// </summary>
		/// <param name="original"><see cref="MetadataReference"/> to replace.</param>
		/// <param name="updated"><see cref="MetadataReference"/> to replace the <paramref name="original"/> by.</param>
		void UpdateCompilation(MetadataReference? original, MetadataReference? updated);

		/// <summary>
		/// Adds the following <paramref name="references"/> to the <see cref="Compilation"/>.
		/// </summary>
		/// <param name="references"><see cref="MetadataReference"/>s to add.</param>
		void UpdateCompilation(IEnumerable<MetadataReference>? references);
	}
}
