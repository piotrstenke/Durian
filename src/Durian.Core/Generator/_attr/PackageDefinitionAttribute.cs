using System;
using System.Diagnostics;
using Durian.Info;

namespace Durian.Generator
{
	/// <summary>
	/// Defines basic information about this Durian module.
	/// </summary>
	[Conditional("DEBUG")]
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
	public sealed class PackageDefinitionAttribute : Attribute
	{
		/// <summary>
		/// Target <see cref="DurianPackage"/>.
		/// </summary>
		public DurianPackage Package { get; }

		/// <summary>
		/// <see cref="DurianModule"/> this package is part of.
		/// </summary>
		public DurianModule Module { get; }

		/// <summary>
		/// Target <see cref="PackageType"/>.
		/// </summary>
		public PackageType Type { get; }

		/// <summary>
		/// Version of the module.
		/// </summary>
		public string Version { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PackageDefinitionAttribute"/> class.
		/// </summary>
		/// <param name="module"><see cref="DurianModule"/> this package is part of.</param>
		/// <param name="package">Target <see cref="DurianPackage"/>.</param>
		/// <param name="type">Target <see cref="PackageType"/>.</param>
		/// <param name="version">Version of the module.</param>
		public PackageDefinitionAttribute(DurianModule module, DurianPackage package, PackageType type, string version)
		{
			Module = module;
			Package = package;
			Type = type;
			Version = version;
		}
	}
}
