﻿using Microsoft.CodeAnalysis.CSharp;

namespace Durian
{
	/// <summary>
	/// <see cref="IGeneratorSyntaxFilter"/> that reports diagnostics about the received <see cref="CSharpSyntaxNode"/>s.
	/// </summary>
	public interface IGeneratorSyntaxFilterWithDiagnostics : IGeneratorSyntaxFilter, ISyntaxFilterWithDiagnostics
	{
		/// <summary>
		/// <see cref="FilterMode"/> of this <see cref="IGeneratorSyntaxFilterWithDiagnostics"/>.
		/// </summary>
		FilterMode Mode { get; }
	}
}
