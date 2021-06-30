// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Immutable;

namespace Durian.Info
{
	/// <summary>
	/// Contains basic information about a Durian module.
	/// </summary>
	/// <remarks><para>NOTE: This class implements the <see cref="IEquatable{T}"/> - two values are compared by their values, not references.</para></remarks>
	public sealed partial class ModuleIdentity : IEquatable<ModuleIdentity>, ICloneable
	{
		private ImmutableArray<DiagnosticData> _diagnostics;
		private ImmutableArray<PackageIdentity> _packages;
		private ImmutableArray<TypeIdentity> _types;

		/// <summary>
		/// A two-digit number that precedes the id of a diagnostic.
		/// </summary>
		public IdSection AnalysisId { get; }

		/// <summary>
		/// A collection of diagnostics that can be reported by this module.
		/// </summary>
		public ImmutableArray<DiagnosticData> Diagnostics => _diagnostics;

		/// <summary>
		/// Link to documentation regarding this module. -or- empty <see cref="string"/> if the module has no documentation.
		/// </summary>
		public string Documentation { get; }

		/// <summary>
		/// Enum representation of this module.
		/// </summary>
		public DurianModule Module { get; }

		/// <summary>
		/// A collection of packages that are part of this module.
		/// </summary>
		public ImmutableArray<PackageIdentity> Packages => _packages;

		/// <summary>
		/// A collection of types that are part of this module.
		/// </summary>
		public ImmutableArray<TypeIdentity> Types => _types;

		internal ModuleIdentity(
			DurianModule module,
			int id,
			PackageIdentity[]? packages,
			string? docsPath,
			DiagnosticData[]? diagnostics,
			TypeIdentity[]? types
		)
		{
			Module = module;
			AnalysisId = (IdSection)id;
			Documentation = docsPath is not null ? @$"{DurianInfo.Repository}\{docsPath}" : string.Empty;

			_packages = packages is null ? ImmutableArray.Create<PackageIdentity>() : ImmutableArray.Create(packages);

			_types = packages is null ? ImmutableArray.Create<TypeIdentity>() : ImmutableArray.Create(types);

			if (diagnostics is null)
			{
				_diagnostics = ImmutableArray.Create<DiagnosticData>();
			}
			else
			{
				foreach (DiagnosticData diag in diagnostics)
				{
					diag.SetModule(this);
				}

				_diagnostics = diagnostics.ToImmutableArray();
			}
		}

		private ModuleIdentity(
			DurianModule module,
			in IdSection id,
			ImmutableArray<PackageIdentity> packages,
			string docsPath,
			ImmutableArray<DiagnosticData> diagnostics,
			ImmutableArray<TypeIdentity> types
		)
		{
			Module = module;
			AnalysisId = id;
			Documentation = docsPath;
			_packages = packages;
			_diagnostics = diagnostics;
			_types = types;
		}

		/// <inheritdoc/>
		public static bool operator !=(ModuleIdentity a, ModuleIdentity b)
		{
			return !(a == b);
		}

		/// <inheritdoc/>
		public static bool operator ==(ModuleIdentity a, ModuleIdentity b)
		{
			return
				a.Module == b.Module &&
				a.Documentation == b.Documentation &&
				a.AnalysisId == b.AnalysisId &&
				Utilities.CompareImmutableArrays(ref a._types, ref b._types) &&
				Utilities.CompareImmutableArrays(ref a._diagnostics, ref b._diagnostics) &&
				Utilities.CompareImmutableArrays(ref a._packages, ref b._packages);
		}

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>A new object that is a copy of this instance.</returns>
		public ModuleIdentity Clone()
		{
			return new ModuleIdentity(Module, AnalysisId, _packages, Documentation, _diagnostics, _types);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (obj is not ModuleIdentity other)
			{
				return false;
			}

			return other == this;
		}

		/// <inheritdoc/>
		public bool Equals(ModuleIdentity other)
		{
			return other == this;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int hashCode = -726504116;
			hashCode = (hashCode * -1521134295) + Documentation.GetHashCode();
			hashCode = (hashCode * -1521134295) + AnalysisId.GetHashCode();
			hashCode = (hashCode * -1521134295) + Module.GetHashCode();
			hashCode = (hashCode * -1521134295) + Utilities.GetHashCodeOfImmutableArray(ref _types);
			hashCode = (hashCode * -1521134295) + Utilities.GetHashCodeOfImmutableArray(ref _packages);
			hashCode = (hashCode * -1521134295) + Utilities.GetHashCodeOfImmutableArray(ref _diagnostics);

			return hashCode;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return Module.ToString();
		}

		object ICloneable.Clone()
		{
			return Clone();
		}
	}
}
