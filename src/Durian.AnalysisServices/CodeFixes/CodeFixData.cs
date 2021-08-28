// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.CodeFixes
{
	/// <summary>
	/// Represents data that is used when creating a <see cref="CodeAction"/> for the code fix.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="CSharpSyntaxNode"/> this <see cref="CodeFixData{T}"/> can store.</typeparam>
	public readonly struct CodeFixData<T> where T : CSharpSyntaxNode
	{
		/// <summary>
		/// <see cref="System.Threading.CancellationToken"/> that specifies if the operation should be canceled.
		/// </summary>
		public readonly CancellationToken CancellationToken { get; }

		/// <summary>
		/// <see cref="Microsoft.CodeAnalysis.Diagnostic"/> that the code fix is being proposed for.
		/// </summary>
		public readonly Diagnostic? Diagnostic { get; }

		/// <summary>
		/// <see cref="Microsoft.CodeAnalysis.Document"/> where the <see cref="Diagnostic"/> is to be found.
		/// </summary>
		public readonly Document? Document { get; }

		/// <summary>
		/// Determines whether this <see cref="CodeFixData{T}"/> contains a <see cref="CSharpSyntaxNode"/>.
		/// </summary>
		[MemberNotNullWhen(true, nameof(Node))]
		public readonly bool HasNode => Node is not null;

		/// <summary>
		/// Determines whether this <see cref="CodeFixData{T}"/> contains a <see cref="Microsoft.CodeAnalysis.SemanticModel"/>.
		/// </summary>
		[MemberNotNullWhen(true, nameof(SemanticModel))]
		public readonly bool HasSemanticModel => SemanticModel is not null;

		/// <summary>
		/// Target <see cref="CSharpSyntaxNode"/>.
		/// </summary>
		public readonly T? Node { get; }

		/// <summary>
		/// Root node of the analyzed tree.
		/// </summary>
		public readonly CompilationUnitSyntax? Root { get; }

		/// <summary>
		/// <see cref="Microsoft.CodeAnalysis.SemanticModel"/> of the <see cref="Node"/>.
		/// </summary>
		public readonly SemanticModel? SemanticModel { get; }

		/// <summary>
		/// Determines whether this data was successfully retrieved.
		/// </summary>
		[MemberNotNullWhen(true, nameof(Root), nameof(Diagnostic), nameof(Document))]
		public readonly bool Success { get; }

		private CodeFixData(
			Diagnostic diagnostic,
			Document document,
			SemanticModel? semanticModel,
			CompilationUnitSyntax root,
			T? node,
			bool success,
			CancellationToken cancellationToken)
		{
			Diagnostic = diagnostic;
			Document = document;
			Root = root;
			Node = node;
			Success = success;
			CancellationToken = cancellationToken;
			SemanticModel = semanticModel;
		}

		/// <summary>
		/// Creates a new <see cref="CodeFixData{T}"/> from the specified <paramref name="context"/>.
		/// </summary>
		/// <param name="context"><see cref="CodeFixContext"/> to create the <see cref="CodeFixData{T}"/> from.</param>
		/// <param name="includeSemanticModel">Determines whether to include the <see cref="Microsoft.CodeAnalysis.SemanticModel"/> when creating this <see cref="CodeFixData{T}"/>.</param>
		public static async Task<CodeFixData<T>> FromAsync(CodeFixContext context, bool includeSemanticModel = false)
		{
			Diagnostic? diagnostic = context.Diagnostics.FirstOrDefault();

			if (diagnostic is null)
			{
				return default;
			}

			if (await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false) is not CompilationUnitSyntax root)
			{
				return default;
			}

			SyntaxNode? parent = root.FindNode(diagnostic.Location.SourceSpan);
			T? node;

			if (parent is null)
			{
				node = null;
			}
			else
			{
				node = parent.FirstAncestorOrSelf<T>() ?? parent.DescendantNodes().OfType<T>().FirstOrDefault();
			}

			SemanticModel? semanticModel;

			if (includeSemanticModel)
			{
				semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
			}
			else
			{
				semanticModel = null;
			}

			return new CodeFixData<T>(
				diagnostic,
				context.Document,
				semanticModel,
				root,
				node,
				true,
				context.CancellationToken);
		}
	}
}
