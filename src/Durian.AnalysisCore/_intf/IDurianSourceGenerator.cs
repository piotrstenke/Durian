using Durian.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian
{
	/// <summary>
	/// <see cref="ISourceGenerator"/> that provides additional information about the current generator pass.
	/// </summary>
	public interface IDurianSourceGenerator : ISourceGenerator
	{
		/// <summary>
		/// <see cref="ICompilationData"/> this <see cref="IDurianSourceGenerator"/> operates on.
		/// </summary>
		ICompilationData TargetCompilation { get; }

		/// <summary>
		/// <see cref="IDurianSyntaxReceiver"/> that provides the <see cref="SyntaxNode"/>es that will take part in the generation.
		/// </summary>
		IDurianSyntaxReceiver SyntaxReceiver { get; }

		/// <summary>
		/// <see cref="CSharpParseOptions"/> that will be used to parse any added sources.
		/// </summary>
		CSharpParseOptions ParseOptions { get; }

		/// <summary>
		/// Version of this <see cref="IDurianSourceGenerator"/>.
		/// </summary>
		string Version { get; }

		/// <summary>
		/// Name of this <see cref="IDurianSourceGenerator"/>.
		/// </summary>
		string GeneratorName { get; }

		/// <summary>
		/// Determines whether this <see cref="IDurianSourceGenerator"/> can report any diagnostics.
		/// </summary>
		bool SupportsDiagnostics { get; }

		/// <summary>
		/// Enables diagnostics if this <see cref="IDurianSourceGenerator"/> supports any.
		/// </summary>
		bool EnableDiagnostics { get; set; }

		/// <summary>
		/// Creates a new <see cref="IDurianSyntaxReceiver"/> to be used during the generator execution pass.
		/// </summary>
		IDurianSyntaxReceiver CreateSyntaxReceiver();
	}
}
