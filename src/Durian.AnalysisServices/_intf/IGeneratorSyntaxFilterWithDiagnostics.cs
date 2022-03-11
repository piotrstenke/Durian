// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis
{
	/// <summary>
	/// <see cref="IGeneratorSyntaxFilter"/> that reports diagnostics about the received <see cref="CSharpSyntaxNode"/>s.
	/// </summary>
	public interface IGeneratorSyntaxFilterWithDiagnostics : IGeneratorSyntaxFilter, ISyntaxFilterWithDiagnostics
	{
		/// <summary>
		/// <see cref="IHintNameProvider"/> that is used to create a hint name for the generated source.
		/// </summary>
		IHintNameProvider HintNameProvider { get; }

		/// <summary>
		/// <see cref="FilterMode"/> of this <see cref="IGeneratorSyntaxFilterWithDiagnostics"/>.
		/// </summary>
		FilterMode Mode { get; }
	}
}
