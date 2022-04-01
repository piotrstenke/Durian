// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics.CodeAnalysis;

namespace Durian.Analysis.Filters
{
	/// <summary>
	/// Validates <see cref="CSharpSyntaxNode"/>s and creates <see cref="IMemberData"/>s.
	/// </summary>
	/// <typeparam name="T">Type of target <see cref="ISyntaxValidatorContext"/>.</typeparam>
	public interface ISyntaxValidator<T> : IValidationDataProvider where T : ISyntaxValidatorContext
	{
		/// <summary>
		/// Validates a <see cref="CSharpSyntaxNode"/> and returns a new instance of <see cref="IMemberData"/> if the validation was a success.
		/// </summary>
		/// <param name="context">Target <see cref="ISyntaxValidatorContext"/>.</param>
		/// <param name="data"><see cref="IMemberData"/> that is returned if the validation succeeds.</param>
		bool ValidateAndCreate(in T context, out IMemberData? data);
	}
}
