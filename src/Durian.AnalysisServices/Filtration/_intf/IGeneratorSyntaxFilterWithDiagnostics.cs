﻿using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Filtration
{
	/// <summary>
	/// <see cref="IGeneratorSyntaxFilter"/> that reports diagnostics about the received <see cref="SyntaxNode"/>s.
	/// </summary>
	public interface IGeneratorSyntaxFilterWithDiagnostics : IGeneratorSyntaxFilter, ISyntaxFilterWithDiagnostics
	{
	}
}
