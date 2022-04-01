// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Threading;

namespace Durian.Analysis.Filters
{
	/// <summary>
	/// <see cref="ISyntaxValidatorContextWithDiagnostics"/> that wraps a child <see cref="ISyntaxValidatorContext"/>.
	/// </summary>
	/// <typeparam name="T">Type of child <see cref="ISyntaxValidatorContext"/>.</typeparam>
	public readonly struct SyntaxValidatorContextWrapper<T> : ISyntaxValidatorContextWithDiagnostics where T : struct, ISyntaxValidatorContext
	{
		internal readonly T _context;

		/// <inheritdoc/>
		public IDiagnosticReceiver DiagnosticReceiver { get; }

		CancellationToken ISyntaxValidatorContext.CancellationToken => _context.CancellationToken;

		ICompilationData ISyntaxValidatorContext.Compilation => _context.Compilation;

		CSharpSyntaxNode ISyntaxValidatorContext.Node => _context.Node;

		SemanticModel ISyntaxValidatorContext.SemanticModel => _context.SemanticModel;

		ISymbol ISyntaxValidatorContext.Symbol => _context.Symbol;

		/// <summary>
		/// Initializes a new instance of the <see cref="SyntaxValidatorContextWrapper{T}"/> structure.
		/// </summary>
		/// <param name="context">Child <see cref="ISyntaxValidatorContext"/>.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		public SyntaxValidatorContextWrapper(in T context, IDiagnosticReceiver diagnosticReceiver)
		{
			_context = context;
			DiagnosticReceiver = diagnosticReceiver;
		}
	}
}
