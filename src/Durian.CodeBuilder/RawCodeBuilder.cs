// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian.Builder
{
	/// <summary>
	/// Generates C# code using fluent API with full control over tokens and formatting.
	/// </summary>
	public partial class RawCodeBuilder : ICodeBuilder
	{
		internal sealed partial class Adapter : RawCodeBuilder
		{
			public Adapter(ICodeReceiver builder) : base(builder)
			{
			}
		}

		/// <inheritdoc/>
		public ICodeReceiver CodeReceiver { get; }

		internal RawCodeBuilder(ICodeReceiver codeReceiver)
		{
			if (codeReceiver is null)
			{
				throw new ArgumentNullException(nameof(codeReceiver));
			}

			CodeReceiver = codeReceiver;
		}
	}
}
