using System;
using System.Collections.Generic;
using System.Text;
using Durian.Info;
using Durian.Generator;

namespace Durian
{
	/// <summary>
	/// Applies a default type for the generic parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.GenericParameter, AllowMultiple = false, Inherited = true)]
	public sealed class DefaultParamAttribute : Attribute
	{
		/// <summary>
		/// Type that is used as the default type for this generic parameter.
		/// </summary>
		public Type Type { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamAttribute"/> class.
		/// </summary>
		/// <param name="type">Type that is used as the default type for this generic parameter.</param>
		public DefaultParamAttribute(Type type)
		{
			Type = type;
		}
	}
}
