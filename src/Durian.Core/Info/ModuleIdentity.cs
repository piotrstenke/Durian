using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Durian.Generator;

namespace Durian.Info
{
	/// <summary>
	/// Contains basic information about a Durian module.
	/// </summary>
	[DebuggerDisplay("Name = {Name}, Version = {Version}")]
	public sealed record ModuleIdentity
	{
		/// <summary>
		/// Name of the module.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Version of the module.
		/// </summary>
		public string Version { get; }

		/// <summary>
		/// Link to documentation regarding this module. -or- empty <see cref="string"/> if the module has no documentation.
		/// </summary>
		public string Documentation { get; }

		/// <summary>
		/// Enum representation of this module.
		/// </summary>
		public DurianModule Module { get; }

		/// <summary>
		/// Type of this module.
		/// </summary>
		public ModuleType Type { get; }

		/// <summary>
		/// A two-digit number that precedes the id of a diagnostic.
		/// </summary>
		public IdSection AnalysisId { get; }

		/// <summary>
		/// A collection of types that are part of this module.
		/// </summary>
		public ImmutableArray<TypeIdentity> Types { get; }

		/// <summary>
		/// A collection of diagnostics that can be reported by this module.
		/// </summary>
		public ImmutableArray<DiagnosticData> Diagnostics { get; }

		internal ModuleIdentity(string name, string version, DurianModule module, ModuleType type, int id, string? docPath = null, TypeIdentity[]? types = null, DiagnosticData[]? diagnostics = null)
		{
			Name = name;
			Version = version;
			Module = module;
			Type = type;
			AnalysisId = (IdSection)id;
			Documentation = docPath is not null ? @$"{DurianInfo.Repository}\{docPath}" : string.Empty;

			if (types is null)
			{
				Types = ImmutableArray.Create<TypeIdentity>();
			}
			else
			{
				foreach (TypeIdentity tree in types)
				{
					tree.SetModule(this);
				}

				Types = types.ToImmutableArray();
			}

			if (diagnostics is null)
			{
				Diagnostics = ImmutableArray.Create<DiagnosticData>();
			}
			else
			{
				foreach (DiagnosticData diag in diagnostics)
				{
					diag.SetModule(this);
				}

				Diagnostics = diagnostics.ToImmutableArray();
			}
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Name}, {Version}";
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int hashCode = -726504116;
			hashCode = (hashCode * -1521134295) + Name.GetHashCode();
			hashCode = (hashCode * -1521134295) + Version.GetHashCode();
			hashCode = (hashCode * -1521134295) + Documentation.GetHashCode();
			hashCode = (hashCode * -1521134295) + AnalysisId.GetHashCode();
			hashCode = (hashCode * -1521134295) + Module.GetHashCode();
			hashCode = (hashCode * -1521134295) + Type.GetHashCode();
			hashCode = (hashCode * -1521134295) + Types.GetHashCode();
			hashCode = (hashCode * -1521134295) + Diagnostics.GetHashCode();
			return hashCode;
		}

		/// <summary>
		/// Checks, if the calling <see cref="Assembly"/> references the specified Durian <paramref name="module"/>.
		/// </summary>
		/// <param name="module"><see cref="DurianModule"/> to check for.</param>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected.</exception>
		public static bool HasReference(DurianModule module)
		{
			if (module == DurianModule.Core)
			{
				return true;
			}

			return HasReference(module, Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Checks, if the specified <paramref name="assembly"/> references the specified Durian <paramref name="module"/>.
		/// </summary>
		/// <param name="module"><see cref="DurianModule"/> to check for.</param>
		/// <param name="assembly"><see cref="Assembly"/> to check if contains the reference.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static bool HasReference(DurianModule module, Assembly assembly)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			IEnumerable<EnableModuleAttribute> attributes = assembly.GetCustomAttributes<EnableModuleAttribute>();

			return attributes.Any(attr => attr.Module == module);
		}

		/// <summary>
		/// Checks, if the calling <see cref="Assembly"/> references a Durian module with the specified <paramref name="moduleName"/>.
		/// </summary>
		/// <param name="moduleName">Name of the Durian module to check for.</param>
		public static bool HasReference(string moduleName)
		{
			return HasReference(moduleName, Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Checks, if the specified <paramref name="assembly"/> references a Durian module with the specified <paramref name="moduleName"/>.
		/// </summary>
		/// <param name="moduleName">Name of the Durian module to check for.</param>
		/// <param name="assembly"><see cref="Assembly"/> to check if contains the reference.</param>
		/// <exception cref="ArgumentNullException"><paramref name="moduleName"/> is <see langword="null"/>. -or- <paramref name="assembly"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="moduleName"/> cannot be empty or white space only. -or- Unknown Durian module name: <paramref name="moduleName"/>.</exception>
		public static bool HasReference(string moduleName, Assembly assembly)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			DurianModule module = ParseModule(moduleName);
			return HasReference(module, assembly);
		}

		/// <summary>
		/// Returns a new instance of <see cref="ModuleIdentity"/> corresponding with the specified <see cref="DurianModule"/>.
		/// </summary>
		/// <param name="module"><see cref="DurianModule"/> to get <see cref="ModuleIdentity"/> for.</param>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected.</exception>
		public static ModuleIdentity GetModule(DurianModule module)
		{
			return module switch
			{
				DurianModule.Core => ModuleRepository.Core,
				DurianModule.AnalysisServices => ModuleRepository.AnalysisServices,
				DurianModule.DefaultParam => ModuleRepository.DefaultParam,
				DurianModule.CoreAnalyzer => ModuleRepository.CoreAnalyzer,
				DurianModule.TestServices => ModuleRepository.TestServices,
				_ => throw new InvalidOperationException($"Unknown {nameof(DurianModule)} value: {module}!")
			};
		}

		/// <summary>
		/// Returns a new instance of <see cref="ModuleIdentity"/> of Durian module with the specified <paramref name="moduleName"/>.
		/// </summary>
		/// <param name="moduleName">Name of the Durian module to get the <see cref="ModuleIdentity"/> of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="moduleName"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="moduleName"/> cannot be empty or white space only. -or- Unknown Durian module name: <paramref name="moduleName"/>.</exception>
		public static ModuleIdentity GetModule(string moduleName)
		{
			DurianModule module = ParseModule(moduleName);
			return GetModule(module);
		}

		private static DurianModule ParseModule(string moduleName)
		{
			if (moduleName is null)
			{
				throw new ArgumentNullException(nameof(moduleName));
			}

			if (string.IsNullOrWhiteSpace(moduleName))
			{
				throw new ArgumentException($"{nameof(moduleName)} cannot be empty or white space only.");
			}

			string name = moduleName.Replace("Durian.", "");

			try
			{
				return (DurianModule)Enum.Parse(typeof(DurianModule), name);
			}
			catch
			{
				throw new ArgumentException($"Unknown Durian module name: {moduleName}", nameof(moduleName));
			}
		}
	}
}
