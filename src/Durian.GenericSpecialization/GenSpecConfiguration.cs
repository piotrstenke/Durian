// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Durian.Configuration;

namespace Durian.Analysis.GenericSpecialization
{
	/// <summary>
	/// Configures optional features of the <see cref="GenericSpecializationGenerator"/>.
	/// </summary>
	/// <remarks><para>NOTE: This class implements the <see cref="IEquatable{T}"/> - two values are compared by their values, not references.</para></remarks>
	public sealed class GenSpecConfiguration : IEquatable<GenSpecConfiguration>, ICloneable
	{
		/// <summary>
		/// Represents the default value of <see cref="InterfaceName"/>.
		/// </summary>
		public const string DefaultInterfaceName = "ISpecialize";

		/// <summary>
		/// Represents the default value of <see cref="TemplateName"/>.
		/// </summary>
		public const string DefaultTemplateName = "Spec";

		private GenSpecImportOptions _importOptions;
		private string _interfaceName;
		private string _templateName;

		/// <summary>
		/// Returns a new instance of <see cref="GenSpecConfiguration"/> with all values set to default.
		/// </summary>
		public static GenSpecConfiguration Default => new(DefaultTemplateName, DefaultInterfaceName);

		/// <summary>
		/// Determines whether to force the specialization class to inherit the main implementation class.
		/// </summary>
		public bool ForceInherit { get; set; }

		/// <inheritdoc cref="GenSpecImportOptions"/>
		public GenSpecImportOptions ImportOptions
		{
			get => _importOptions;
			set
			{
				if (value < default(GenSpecImportOptions) || value > GenSpecImportOptions.OverrideAny)
				{
					_importOptions = default;
				}
				else
				{
					_importOptions = value;
				}
			}
		}

		/// <summary>
		/// Name of the generated specialization interface
		/// </summary>
		/// <exception cref="ArgumentException">Value is not a valid identifier.</exception>
		public string InterfaceName
		{
			get => _interfaceName;
			set
			{
				ThrowIfIsNotValidIdentifier(value);
				_interfaceName = value;
			}
		}

		/// <summary>
		/// Name of the class that is the main implementation of the type.
		/// </summary>
		/// <exception cref="ArgumentException">Value is not a valid identifier.</exception>
		public string TemplateName
		{
			get => _templateName;
			set
			{
				ThrowIfIsNotValidIdentifier(value);
				_templateName = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GenSpecConfiguration"/> class.
		/// </summary>
		/// <exception cref="ArgumentException"><paramref name="templateName"/> is not a valid identifier. -or- <paramref name="interfaceName"/> is not a valid identifier.</exception>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		public GenSpecConfiguration(string templateName, string interfaceName)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		{
			TemplateName = templateName;
			InterfaceName = interfaceName;
		}

		/// <inheritdoc/>
		public static bool operator !=(GenSpecConfiguration left, GenSpecConfiguration right)
		{
			return !(left == right);
		}

		/// <inheritdoc/>
		public static bool operator ==(GenSpecConfiguration left, GenSpecConfiguration right)
		{
			return
				left._interfaceName == right._interfaceName &&
				left._templateName == right._templateName &&
				left.ForceInherit == right.ForceInherit &&
				left._importOptions == right._importOptions;
		}

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>A new object that is a copy of this instance.</returns>
		public GenSpecConfiguration Clone()
		{
			return new GenSpecConfiguration(_templateName, _interfaceName)
			{
				ForceInherit = ForceInherit,
				ImportOptions = ImportOptions
			};
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (obj is not GenSpecConfiguration c)
			{
				return false;
			}

			return c == this;
		}

		/// <inheritdoc/>
		public bool Equals(GenSpecConfiguration other)
		{
			return other == this;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int hashCode = -1810266055;
			hashCode = (hashCode * -1521134295) + _interfaceName.GetHashCode();
			hashCode = (hashCode * -1521134295) + _templateName.GetHashCode();
			hashCode = (hashCode * -1521134295) + ForceInherit.GetHashCode();
			hashCode = (hashCode * -1521134295) + ImportOptions.GetHashCode();
			return hashCode;
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		internal static void ThrowIfIsNotValidIdentifier(string value)
		{
			if (!AnalysisUtilities.IsValidIdentifier(value))
			{
				throw new ArgumentException("Value '{value}' is not a valid identifier!");
			}
		}
	}
}
