// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Immutable;

namespace Durian.Info
{
	/// <summary>
	/// Contains basic information about a Durian module.
	/// </summary>
	/// <remarks>This class implements the <see cref="IEquatable{T}"/> interface - two instances are compared by their values, not references.
	/// <para>This class implements the <see cref="IDisposable"/> interface - instance should be disposed using the <see cref="Dispose"/> method if its no longer needed.</para></remarks>
	public sealed partial class ModuleIdentity : IDurianIdentity, IEquatable<ModuleIdentity>, IDisposable
	{
		private bool _disposed;

		/// <summary>
		/// A two-digit number that precedes the id of a diagnostic.
		/// </summary>
		public IdSection? AnalysisId { get; }

		/// <summary>
		/// A collection of diagnostics that can be reported by this module.
		/// </summary>
		public ImmutableArray<DiagnosticData> Diagnostics { get; }

		/// <summary>
		/// Link to documentation regarding this module. -or- empty <see cref="string"/> if the module has no documentation.
		/// </summary>
		public string Documentation { get; }

		/// <summary>
		/// Enum representation of this module.
		/// </summary>
		public DurianModule Module { get; }

		/// <summary>
		/// Name of the module.
		/// </summary>
		public string Name => Module.ToString();

		/// <summary>
		/// A collection of packages that are part of this module.
		/// </summary>
		public ImmutableArray<PackageReference> Packages { get; }

		/// <summary>
		/// A collection of types that are part of this module.
		/// </summary>
		public ImmutableArray<TypeIdentity> Types { get; }

		internal ModuleIdentity(
			DurianModule module,
			int? id,
			DurianPackage[]? packages,
			string? docsPath,
			DiagnosticData[]? diagnostics,
			TypeIdentity[]? types
		)
		{
			Module = module;
			Documentation = docsPath ?? string.Empty;

			if(id.HasValue)
			{
				AnalysisId = new IdSection(id.Value);
			}

			if (packages is null || packages.Length == 0)
			{
				Packages = ImmutableArray.Create<PackageReference>();
			}
			else
			{
				int length = packages.Length;
				ImmutableArray<PackageReference>.Builder b = ImmutableArray.CreateBuilder<PackageReference>(length);

				for (int i = 0; i < length; i++)
				{
					b.Add(new PackageReference(packages[i], this));
				}

				Packages = b.ToImmutable();
			}

			if (types is null)
			{
				Types = ImmutableArray.Create<TypeIdentity>();
			}
			else
			{
				foreach (TypeIdentity type in types)
				{
					type.SetModule(this);
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

			IdentityPool.Modules.TryAdd(Name, this);
		}

		private ModuleIdentity(
			DurianModule module,
			IdSection? id,
			ImmutableArray<PackageReference> packages,
			string docsPath,
			ImmutableArray<DiagnosticData> diagnostics,
			ImmutableArray<TypeIdentity> types
		)
		{
			Module = module;
			AnalysisId = id;
			Documentation = docsPath;
			Packages = packages;
			Diagnostics = diagnostics;
			Types = types;

			// This constructor is called only when a clone is created.
			// Since this instance is a clone, it shouldn't have access to the IdentityPool.
			_disposed = true;
		}

		/// <inheritdoc/>
		public static bool operator !=(ModuleIdentity? a, ModuleIdentity? b)
		{
			return !(a == b);
		}

		/// <inheritdoc/>
		public static bool operator ==(ModuleIdentity? a, ModuleIdentity? b)
		{
			if (a is null)
			{
				return b is null;
			}

			if (b is null)
			{
				return false;
			}

			return
				a.Module == b.Module &&
				a.Documentation == b.Documentation &&
				a.AnalysisId == b.AnalysisId &&
				Utilities.CompareImmutableArrays(a.Types, b.Types) &&
				Utilities.CompareImmutableArrays(a.Diagnostics, b.Diagnostics) &&
				Utilities.CompareImmutableArrays(a.Packages, b.Packages);
		}

		/// <inheritdoc cref="ICloneable.Clone"/>
		public ModuleIdentity Clone()
		{
			return new ModuleIdentity(Module, AnalysisId, Packages, Documentation, Diagnostics, Types);
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			if (!_disposed)
			{
				IdentityPool.Modules.TryRemove(Name, out _);
				_disposed = true;
			}
		}

		/// <inheritdoc/>
		public override bool Equals(object? obj)
		{
			if (obj is not ModuleIdentity other)
			{
				return false;
			}

			return other == this;
		}

		/// <inheritdoc/>
		public bool Equals(ModuleIdentity? other)
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
			hashCode = (hashCode * -1521134295) + Utilities.GetHashCodeOfImmutableArray(Types);
			hashCode = (hashCode * -1521134295) + Utilities.GetHashCodeOfImmutableArray(Packages);
			hashCode = (hashCode * -1521134295) + Utilities.GetHashCodeOfImmutableArray(Diagnostics);

			return hashCode;
		}

		/// <summary>
		/// Returns a new instance of <see cref="ModuleReference"/> pointing to the this <see cref="ModuleIdentity"/>.
		/// </summary>
		public ModuleReference GetReference()
		{
			return new ModuleReference(this);
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

		IDurianIdentity IDurianIdentity.Clone()
		{
			return Clone();
		}

		internal void SetPackage(PackageIdentity package)
		{
			foreach (PackageReference p in Packages)
			{
				if (p.EnumValue == package.EnumValue)
				{
					p.Accept(package);
					return;
				}
			}
		}
	}
}