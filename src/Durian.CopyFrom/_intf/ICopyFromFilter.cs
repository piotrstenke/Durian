// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Cache;
using Durian.Analysis.Filters;

namespace Durian.Analysis.CopyFrom
{
	/// <summary>
	/// Filtrates and validates nodes collected by a <see cref="CopyFromSyntaxReceiver"/>.
	/// </summary>
	public interface ICopyFromFilter : ICachedGeneratorSyntaxFilterWithDiagnostics<ICopyFromMember>, ISyntaxValidatorWithDiagnostics<SyntaxValidatorContext>
	{
	}
}
