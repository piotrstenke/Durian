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
	public sealed class ModuleDefinitionAttribute : Attribute
	{
		/// <summary>
		/// Target <see cref="DurianModule"/>.
		/// </summary>
		public DurianModule Module { get; }

		/// <summary>
		/// Target <see cref="ModuleType"/>.
		/// </summary>
		public ModuleType Type { get; }

		/// <summary>
		/// Version of the module.
		/// </summary>
		public string Version { get; }

		/// <summary>
		/// Two-digit id of the module.
		/// </summary>
		public int Id { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ModuleDefinitionAttribute"/> class.
		/// </summary>
		/// <param name="module">Target <see cref="DurianModule"/>.</param>
		/// <param name="type">Target <see cref="ModuleType"/>.</param>
		/// <param name="version">Version of the module.</param>
		/// <param name="diagnosticId">Two-digit id of the module that is added before id of a diagnostic.</param>
		public ModuleDefinitionAttribute(DurianModule module, ModuleType type, string version, int diagnosticId = default)
		{
			Module = module;
			Type = type;
			Version = version;
			Id = diagnosticId;
		}
	}
}
