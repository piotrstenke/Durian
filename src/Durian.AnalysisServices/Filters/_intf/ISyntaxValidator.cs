// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.Filters
{
	/// <summary>
	/// Validates <see cref="CSharpSyntaxNode"/>s and creates <see cref="IMemberData"/>s.
	/// </summary>
	public interface ISyntaxValidator
	{
		/// <summary>
		/// Validates a <see cref="CSharpSyntaxNode"/> and returns a new instance of <see cref="IMemberData"/> if the validation was a success.
		/// </summary>
		/// <param name="context">Target <see cref="ValidationDataContext"/>.</param>
		/// <param name="data"><see cref="IMemberData"/> that is returned if the validation succeeds.</param>
		bool ValidateAndCreate(in ValidationDataContext context, [NotNullWhen(true)] out IMemberData? data);
	}

	/// <summary>
	/// Validates <see cref="CSharpSyntaxNode"/>s and creates <see cref="IMemberData"/>s.
	/// </summary>
	/// <typeparam name="T">Type of target <see cref="ISyntaxValidationContext"/>.</typeparam>
	public interface ISyntaxValidator<T> : ISyntaxValidator, IValidationContextProvider<T> where T : ISyntaxValidationContext
	{
		/// <summary>
		/// Validates a <see cref="CSharpSyntaxNode"/> and returns a new instance of <see cref="IMemberData"/> if the validation was a success.
		/// </summary>
		/// <param name="context">Target <see cref="ISyntaxValidationContext"/>.</param>
		/// <param name="data"><see cref="IMemberData"/> that is returned if the validation succeeds.</param>
		bool ValidateAndCreate(in T context, out IMemberData? data);
	}
}
