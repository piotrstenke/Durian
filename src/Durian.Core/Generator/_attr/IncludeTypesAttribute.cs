// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Durian.Info;

namespace Durian.Generator
{
	/// <summary>
	/// Informs that the specified types from the <c>Durian.Core</c> module are part of this <see cref="DurianModule"/>.
	/// </summary>
	[Conditional("DEBUG")]
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
	public sealed class IncludeTypesAttribute : Attribute
	{
		/// <summary>
		/// Names of types to include.
		/// </summary>
		public string[] Types { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="IncludeTypesAttribute"/> class.
		/// </summary>
		/// <param name="types">Names of types to include.</param>
		public IncludeTypesAttribute(params string[]? types)
		{
			Types = types ?? Array.Empty<string>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IncludeTypesAttribute"/> class.
		/// </summary>
		/// <param name="types"><see cref="Type"/> to include.</param>
		public IncludeTypesAttribute(params Type[]? types)
		{
			if (types is null)
			{
				Types = Array.Empty<string>();
				return;
			}

			int length = types.Length;
			List<string> names = new(length);

			for (int i = 0; i < length; i++)
			{
				names.Add(types[i].Name);
			}

			Types = names.ToArray();
		}
	}
}
