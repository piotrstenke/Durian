// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian
{
	/// <summary>
	/// Specifies that the class is a specialization of the specified generic type.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public sealed class GenericSpecializationAttribute : Attribute
	{
		/// <summary>
		/// Unbound generic type to specialize.
		/// </summary>
		public Type Type { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="AllowSpecializationAttribute"/> class.
		/// </summary>
		/// <param name="type">Unbound generic type to specialize.</param>
		public GenericSpecializationAttribute(Type type)
		{
			Type = type;
		}
	}
}
