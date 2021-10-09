// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace Durian.Analysis.EnumServices
{
	/// <summary>
	/// Configures how <see cref="EnumServicesGenerator"/> generates sources for the target <see langword="enum"/>.
	/// </summary>
	public class EnumServicesConfiguration : IEquatable<EnumServicesConfiguration>, ICloneable
	{
		/// <summary>
		/// Returns a new instance of <see cref="EnumServicesConfiguration"/> with all values set to default.
		/// </summary>
		public static EnumServicesConfiguration Default => new()
		{
			ClassName = "EnumExtensions"
		};

		/// <summary>
		/// Determines the accessibility of the generated extension methods.
		/// </summary>
		public GeneratedTypeAccess Accessibility { get; set; }

		/// <summary>
		/// Determines whether to allow custom implementation of a generated extension method.
		/// If <see langword="true"/>, no diagnostic will be reported if a method with the same signature
		/// as the generated one already exists in the type.
		/// </summary>
		public bool AllowCustomization { get; set; }

		/// <summary>
		/// Name of class that should contain the generated extension methods.
		/// If the class already exists, it must be <see langword="partial"/>.
		/// </summary>
		public string? ClassName { get; set; }

		/// <summary>
		/// Namespace where the generated extension class will be placed.
		/// </summary>
		/// <remarks>If this property is set to <see langword="null"/>, the namespace of the original enum is used.</remarks>
		public string? Namespace { get; set; }

		/// <summary>
		/// Prefix that should be applied to generated extension methods.
		/// </summary>
		public string? Prefix { get; set; }

		/// <summary>
		/// Specifies extension methods that should be generated for the target <see langword="enum"/>.
		/// </summary>
		public Durian.EnumServices EnumServices { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="EnumServicesConfiguration"/> class.
		/// </summary>
		public EnumServicesConfiguration()
		{
		}

		/// <inheritdoc/>
		public static bool operator !=(EnumServicesConfiguration left, EnumServicesConfiguration right)
		{
			return !(left == right);
		}

		/// <inheritdoc/>
		public static bool operator ==(EnumServicesConfiguration left, EnumServicesConfiguration right)
		{
			return
				left.AllowCustomization == right.AllowCustomization &&
				left.Accessibility == right.Accessibility &&
				left.EnumServices == right.EnumServices &&
				left.ClassName == right.ClassName &&
				left.Namespace == right.Namespace &&
				left.Prefix == right.Prefix;
		}

		/// <inheritdoc cref="ICloneable.Clone"/>
		public EnumServicesConfiguration Clone()
		{
			return new()
			{
				Accessibility = Accessibility,
				AllowCustomization = AllowCustomization,
				ClassName = ClassName,
				Prefix = Prefix,
				EnumServices = EnumServices,
				Namespace = Namespace
			};
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (obj is EnumServicesConfiguration other)
			{
				return other == this;
			}

			return false;
		}

		/// <inheritdoc/>
		public bool Equals(EnumServicesConfiguration other)
		{
			return other == this;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int hashCode = -476904749;
			hashCode = (hashCode * -1521134295) + Accessibility.GetHashCode();
			hashCode = (hashCode * -1521134295) + AllowCustomization.GetHashCode();
			hashCode = (hashCode * -1521134295) + EqualityComparer<string?>.Default.GetHashCode(ClassName);
			hashCode = (hashCode * -1521134295) + EqualityComparer<string?>.Default.GetHashCode(Namespace);
			hashCode = (hashCode * -1521134295) + EqualityComparer<string?>.Default.GetHashCode(Prefix);
			hashCode = (hashCode * -1521134295) + EnumServices.GetHashCode();
			return hashCode;
		}

		object ICloneable.Clone()
		{
			return Clone();
		}
	}
}
