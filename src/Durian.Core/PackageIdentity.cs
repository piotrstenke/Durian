﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;

namespace Durian.Generator
{
	/// <summary>
	/// Contains basic information about a Durian package.
	/// </summary>
	[DebuggerDisplay("Name = {Name}, Version = {Version}")]
	public sealed class PackageIdentity : IEquatable<PackageIdentity>
	{
		/// <summary>
		/// Name of the package.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Version of the package.
		/// </summary>
		public string Version { get; }

		/// <summary>
		/// <see cref="DurianModule"/> this package is part of.
		/// </summary>
		public DurianModule Module { get; }

		/// <summary>
		/// Type of this package's module.
		/// </summary>
		public ModuleType Type { get; }

		/// <summary>
		/// A collection of syntax trees that are statically generated by this package.
		/// </summary>
		public ImmutableArray<StaticTreeIdentity> StaticTrees { get; }

		internal PackageIdentity(string name, string version, DurianModule module, ModuleType type)
		{
			Name = name;
			Version = version;
			Module = module;
			Type = type;
			StaticTrees = ImmutableArray.Create<StaticTreeIdentity>();
		}

		internal PackageIdentity(string name, string version, DurianModule module, ModuleType type, StaticTreeIdentity[] staticTrees)
		{
			Name = name;
			Version = version;
			Module = module;
			Type = type;

			foreach (StaticTreeIdentity tree in staticTrees)
			{
				tree._package = this;
			}

			StaticTrees = staticTrees.ToImmutableArray();
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Name}, {Version}";
		}

		/// <inheritdoc/>
		public override bool Equals(object? obj)
		{
			if (obj is not PackageIdentity other)
			{
				return false;
			}

			return other == this;
		}

		/// <inheritdoc/>
		public bool Equals(PackageIdentity? other)
		{
			return other is not null && other == this;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int hashCode = -726504116;
			hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(Name);
			hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(Version);
			hashCode = (hashCode * -1521134295) + Module.GetHashCode();
			hashCode = (hashCode * -1521134295) + Type.GetHashCode();
			hashCode = (hashCode * -1521134295) + StaticTrees.GetHashCode();
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
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected.</exception>
		public static bool HasReference(DurianModule module, Assembly assembly)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			return module switch
			{
				DurianModule.Core => assembly.GetType(typeof(PackageIdentity).ToString()) is not null,
				DurianModule.AnalysisCore => assembly.GetType("Durian.CodeBuilder") is not null,
				DurianModule.DefaultParam => assembly.GetType("Durian.DefaultParam.DefaultParamGenerator") is not null,
				_ => throw new InvalidOperationException($"Unknown {nameof(DurianModule)} value: {module}!"),
			};
		}

		/// <summary>
		/// Checks, if the calling <see cref="Assembly"/> references a Durian package with the specified <paramref name="packageName"/>.
		/// </summary>
		/// <param name="packageName">Name of the Durian package to check for.</param>
		/// <returns></returns>
		public static bool References(string packageName)
		{
			return References(packageName, Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Checks, if the specified <paramref name="assembly"/> references a Durian package with the specified <paramref name="packageName"/>.
		/// </summary>
		/// <param name="packageName">Name of the Durian package to check for.</param>
		/// <param name="assembly"><see cref="Assembly"/> to check if contains the reference.</param>
		/// <exception cref="ArgumentNullException"><paramref name="packageName"/> is <see langword="null"/>. -or- <paramref name="assembly"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="packageName"/> cannot be empty or white space only. -or- Unknown Durian package name: <paramref name="packageName"/>.</exception>
		public static bool References(string packageName, Assembly assembly)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			DurianModule module = ParseModule(packageName);
			return HasReference(module, assembly);
		}

		/// <summary>
		/// Returns a new instance of <see cref="PackageIdentity"/> corresponding with the specified <see cref="DurianModule"/>.
		/// </summary>
		/// <param name="module"><see cref="DurianModule"/> to get <see cref="PackageIdentity"/> for.</param>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected.</exception>
		public static PackageIdentity Find(DurianModule module)
		{
			return module switch
			{
				DurianModule.Core => PackageFactory.Core,
				DurianModule.AnalysisCore => PackageFactory.AnalysisCore,
				DurianModule.DefaultParam => PackageFactory.DefaultParam,
				_ => throw new InvalidOperationException($"Unknown {nameof(DurianModule)} value: {module}!")
			};
		}

		/// <summary>
		/// Returns a new instance of <see cref="PackageIdentity"/> of Durian package with the specified <paramref name="packageName"/>.
		/// </summary>
		/// <param name="packageName">Name of the Durian package to get the <see cref="PackageIdentity"/> of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="packageName"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="packageName"/> cannot be empty or white space only. -or- Unknown Durian package name: <paramref name="packageName"/>.</exception>
		public static PackageIdentity Find(string packageName)
		{
			DurianModule module = ParseModule(packageName);
			return Find(module);
		}

		/// <inheritdoc/>
		public static bool operator ==(PackageIdentity first, PackageIdentity second)
		{
			return first.Name == second.Name && first.Version == second.Version && first.Module == second.Module && first.Type == second.Type && first.StaticTrees == second.StaticTrees;
		}

		/// <inheritdoc/>
		public static bool operator !=(PackageIdentity first, PackageIdentity second)
		{
			return !(first == second);
		}

		private static DurianModule ParseModule(string packageName)
		{
			if (packageName is null)
			{
				throw new ArgumentNullException(nameof(packageName));
			}

			if (string.IsNullOrWhiteSpace(packageName))
			{
				throw new ArgumentException($"{nameof(packageName)} cannot be empty or white space only.");
			}

			string name = packageName.Replace("Durian.", "");

			try
			{
				return (DurianModule)Enum.Parse(typeof(DurianModule), name);
			}
			catch
			{
				throw new ArgumentException($"Unknown Durian package name: {packageName}", nameof(packageName));
			}
		}
	}
}
