using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.CopyFrom;

/// <summary>
/// <see cref="IMemberData"/> that is used to generate new sources by the <see cref="CopyFromGenerator"/>.
/// </summary>
public interface ICopyFromMember : IMemberData
{
	/// <summary>
	/// <see cref="ISymbol"/>s generation of this member depends on.
	/// </summary>
	ISymbol[]? Dependencies { get; }

	/// <summary>
	/// A collection of patterns applied to the member using <c>Durian.PatternAttribute</c>.
	/// </summary>
	PatternData[]? Patterns { get; }
}
