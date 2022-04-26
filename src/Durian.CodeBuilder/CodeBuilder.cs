// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Builder
{
	/// <summary>
	/// Generates C# code using fluent API.
	/// </summary>
	public sealed class CodeBuilder
	{
		/// <summary>
		/// <see cref="ICodeReceiver"/> the generated code is written to.
		/// </summary>
		public ICodeReceiver CodeReceiver { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CodeBuilder"/> class.
		/// </summary>
		/// <param name="codeReceiver"><see cref="ICodeReceiver"/> the generated code is written to.</param>
		/// <exception cref="ArgumentNullException"><paramref name="codeReceiver"/> is <see langword="null"/>.</exception>
		public CodeBuilder(ICodeReceiver codeReceiver)
		{
			if (codeReceiver is null)
			{
				throw new ArgumentNullException(nameof(codeReceiver));
			}

			CodeReceiver = codeReceiver;
		}
	}
}
