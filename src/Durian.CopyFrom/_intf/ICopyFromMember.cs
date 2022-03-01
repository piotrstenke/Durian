// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Durian.Analysis.CopyFrom
{
    /// <summary>
    /// <see cref="IMemberData"/> that is used to generate new sources by the <see cref="CopyFromGenerator"/>.
    /// </summary>
    public interface ICopyFromMember : IMemberData
	{
		/// <summary>
		/// A collection of patterns applied to the member using <c>Durian.PatternAttribute</c>.
		/// </summary>
		public PatternData[]? Patterns { get; }

		/// <summary>
		/// Target members.
		/// </summary>
		public IEnumerable<ISymbol> Targets { get; }
	}
}