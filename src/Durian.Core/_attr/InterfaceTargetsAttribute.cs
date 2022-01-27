// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian
{
	/// <summary>
	/// Specifies that an <see langword="interface"/> can be implemented only by members of certain kind.
	/// </summary>
	[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
	public sealed class InterfaceTargetsAttribute : Attribute
	{
		/// <summary>
		/// Specifies member kinds this interface is valid on.
		/// </summary>
		public InterfaceTargets Targets { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="InterfaceTargetsAttribute"/> class.
		/// </summary>
		/// <param name="targets">Specifies member kinds this interface is valid on.</param>
		public InterfaceTargetsAttribute(InterfaceTargets targets)
		{
			Targets = targets;
		}
	}
}