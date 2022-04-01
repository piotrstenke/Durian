// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Cache;

namespace Durian.Analysis.CopyFrom
{
	/// <summary>
	/// Filtrates and validates nodes collected by a <see cref="CopyFromSyntaxReceiver"/>.
	/// </summary>
	public interface ICopyFromFilter : ICachedGeneratorSyntaxFilterWithDiagnostics<ICopyFromMember>, INodeValidatorWithDiagnostics<ICopyFromMember>, INodeProvider
	{
		/// <summary>
		/// <see cref="CopyFromGenerator"/> that created this filter.
		/// </summary>
		new CopyFromGenerator Generator { get; }
	}
}
