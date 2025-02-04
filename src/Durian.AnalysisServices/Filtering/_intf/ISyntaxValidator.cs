﻿using System.Diagnostics.CodeAnalysis;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Filtering;

/// <summary>
/// Validates <see cref="SyntaxNode"/>s and creates <see cref="IMemberData"/>s.
/// </summary>
public interface ISyntaxValidator
{
	/// <summary>
	/// Validates a <see cref="SyntaxNode"/> and returns a new instance of <see cref="IMemberData"/> if the validation was a success.
	/// </summary>
	/// <param name="context">Target <see cref="PreValidationContext"/>.</param>
	/// <param name="data"><see cref="IMemberData"/> that is returned if the validation succeeds.</param>
	bool ValidateAndCreate(in PreValidationContext context, [NotNullWhen(true)] out IMemberData? data);
}

/// <summary>
/// Validates <see cref="SyntaxNode"/>s and creates <see cref="IMemberData"/>s.
/// </summary>
/// <typeparam name="T">Type of target <see cref="ISyntaxValidationContext"/>.</typeparam>
public interface ISyntaxValidator<T> : ISyntaxValidator, IValidationContextProvider<T> where T : ISyntaxValidationContext
{
	/// <summary>
	/// Validates a <see cref="SyntaxNode"/> and returns a new instance of <see cref="IMemberData"/> if the validation was a success.
	/// </summary>
	/// <param name="context">Target <see cref="ISyntaxValidationContext"/>.</param>
	/// <param name="data"><see cref="IMemberData"/> that is returned if the validation succeeds.</param>
	bool ValidateAndCreate(in T context, out IMemberData? data);
}
