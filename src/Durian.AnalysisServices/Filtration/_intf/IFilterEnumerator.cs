// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.Filtration
{
	/// <summary>
	/// Enumerates through a collection of <see cref="IMemberData"/>s of type <typeparamref name="T"/> created by the provided <see cref="ISyntaxValidator{T}"/>.
	/// </summary>
	/// <typeparam name="T">Type of target <see cref="ISyntaxValidationContext"/>.</typeparam>
	public interface IFilterEnumerator<T> where T : ISyntaxValidationContext
	{
		/// <summary>
		/// Parent <see cref="ICompilationData"/> of the provided <see cref="CSharpSyntaxNode"/>s.
		/// </summary>
		ICompilationData Compilation { get; }

		/// <summary>
		/// <see cref="IMemberData"/> at the current position in the enumerator.
		/// </summary>
		IMemberData? Current { get; }

		/// <summary>
		/// <see cref="ISyntaxValidator{T}"/> that is used to validate and create the <see cref="IMemberData"/>s to enumerate through.
		/// </summary>
		ISyntaxValidator<T> Validator { get; }

		/// <summary>
		/// Creates and validates the next <see cref="IMemberData"/>.
		/// </summary>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> is the <see cref="IMemberData"/> is valid, <see langword="false"/> otherwise.</returns>
		[MemberNotNullWhen(true, nameof(Current))]
		bool MoveNext(CancellationToken cancellationToken = default);
	}
}