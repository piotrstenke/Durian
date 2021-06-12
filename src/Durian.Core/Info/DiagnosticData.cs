// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;

namespace Durian.Info
{
	/// <summary>
	/// Contains information about a specific diagnostic.
	/// </summary>
	/// <remarks><para>NOTE: This class implements the <see cref="IEquatable{T}"/> - two values are compared by their values, not references.</para></remarks>
	[DebuggerDisplay("{GetFullId}: {Title}")]
	public sealed class DiagnosticData : IEquatable<DiagnosticData>
	{
		private string? _docsPath;
		private ModuleReference? _module;
		private ModuleReference? _originalModule;

		/// <summary>
		/// Link to the documentation regarding this diagnostic.
		/// </summary>
		public string Documentation => _docsPath!;

		/// <summary>
		/// Determines whether this diagnostic is reported on a specific location or compilation-wide.
		/// </summary>
		public bool HasLocation { get; }

		/// <summary>
		/// Two-digit number representing the actual id of the diagnostic.
		/// </summary>
		public IdSection Id { get; }

		/// <summary>
		/// Determines whether this diagnostic was created in different module than it was reported in.
		/// </summary>
		public bool IsExtern { get; }

		/// <summary>
		/// Determines whether the current generation pass will be stopped once this diagnostic is detected.
		/// </summary>
		public bool IsFatal { get; }

		/// <summary>
		/// Durian module this diagnostic is reported in.
		/// </summary>
		public ModuleReference Module => _module!;

		/// <summary>
		/// Two-digit number that precedes the actual <see cref="Id"/> of the diagnostic. Diagnostics created in the same module will have the same <see cref="ModuleId"/>.
		/// </summary>
		/// <remarks>NOTE: Using this property will call the <see cref="ModuleReference.GetModule"/> on the <see cref="OriginalModule"/>.</remarks>
		public IdSection ModuleId => _originalModule!.GetModule().AnalysisId;

		/// <summary>
		/// If <see cref="IsExtern"/> is <see langword="true"/>, returns <see cref="ModuleReference"/> to the module this diagnostic was created in, otherwise returns <see cref="Module"/>.
		/// </summary>
		public ModuleReference OriginalModule => _originalModule!;

		/// <summary>
		/// Title of the diagnostic.
		/// </summary>
		public string Title { get; }

		internal DiagnosticData(string title, int id, string docsPath, bool fatal, bool hasLocation, ModuleIdentity? originalModule = null)
		{
			Title = title;
			Id = (IdSection)id;
			IsFatal = fatal;
			HasLocation = hasLocation;

			if (originalModule is not null)
			{
				_originalModule = new ModuleReference(originalModule);
				_docsPath = $@"{originalModule.Documentation}\{docsPath}";
				IsExtern = true;
			}
			else
			{
				_docsPath = docsPath;
			}
		}

		/// <inheritdoc/>
		public static bool operator !=(DiagnosticData a, DiagnosticData b)
		{
			return !(a == b);
		}

		/// <inheritdoc/>
		public static bool operator ==(DiagnosticData a, DiagnosticData b)
		{
			return
				a.Title == b.Title &&
				a.Documentation == b.Documentation &&
				a.HasLocation == b.HasLocation &&
				a.Id == b.Id &&
				a.IsExtern == b.IsExtern &&
				a.IsFatal == b.IsFatal &&
				a.Module == b.Module &&
				a.OriginalModule == b.OriginalModule;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (obj is not DiagnosticData d)
			{
				return false;
			}

			return d == this;
		}

		/// <inheritdoc/>
		public bool Equals(DiagnosticData other)
		{
			return other == this;
		}

		/// <summary>
		/// Returns a <see cref="string"/> that represents the full id of the diagnostic, composed of the <see cref="DurianInfo.IdPrefix"/>, <see cref="ModuleId"/> and <see cref="Id"/>.
		/// </summary>
		/// <remarks>NOTE: Using this property will call the <see cref="ModuleReference.GetModule"/> on the <see cref="OriginalModule"/>.</remarks>
		public string GetFullId()
		{
			return $"{DurianInfo.IdPrefix}{Module}{Id}";
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int hashCode = -726504116;
			hashCode = (hashCode * -1521134295) + Title.GetHashCode();
			hashCode = (hashCode * -1521134295) + Documentation.GetHashCode();
			hashCode = (hashCode * -1521134295) + Id.GetHashCode();
			hashCode = (hashCode * -1521134295) + IsFatal.GetHashCode();
			hashCode = (hashCode * -1521134295) + HasLocation.GetHashCode();
			hashCode = (hashCode * -1521134295) + Module.GetHashCode();
			hashCode = (hashCode * -1521134295) + OriginalModule.GetHashCode();
			return hashCode;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return GetFullId();
		}

		internal void SetModule(ModuleIdentity module)
		{
			ModuleReference reference = new(module);

			if (_originalModule is null)
			{
				_originalModule = reference;
				_docsPath = $@"{module.Documentation}\{_docsPath}";
			}

			_module = reference;
		}
	}
}
