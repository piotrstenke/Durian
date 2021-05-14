using System.Diagnostics;

namespace Durian.Info
{
	/// <summary>
	/// Contains information about a specific diagnostic.
	/// </summary>
	[DebuggerDisplay("{GetFullId}: {Title}")]
	public sealed record DiagnosticData
	{
		private ModuleIdentity? _module;
		private ModuleIdentity? _originalModule;
		private string? _docsPath;

		/// <summary>
		/// Title of the diagnostic.
		/// </summary>
		public string Title { get; }

		/// <summary>
		/// Link to the documentation regarding this diagnostic.
		/// </summary>
		public string Documentation => _docsPath!;

		/// <summary>
		/// If <see cref="IsExtern"/> is <see langword="true"/>, returns <see cref="ModuleIdentity"/> of the module this diagnostic was created in, otherwise returns <see cref="Module"/>.
		/// </summary>
		public ModuleIdentity OriginalModule => _originalModule!;

		/// <summary>
		/// Durian module this diagnostic is reported in.
		/// </summary>
		public ModuleIdentity Module => _module!;

		/// <summary>
		/// Two-digit number that precedes the actual <see cref="Id"/> of the diagnostic. Diagnostics created in the same module will have the same <see cref="ModuleId"/>.
		/// </summary>
		public IdSection ModuleId => _originalModule!.AnalysisId;

		/// <summary>
		/// Two-digit number representing the actual id of the diagnostic.
		/// </summary>
		public IdSection Id { get; }

		/// <summary>
		/// Determines whether the current generation pass will be stopped once this diagnostic is detected.
		/// </summary>
		public bool IsFatal { get; }

		/// <summary>
		/// Determines whether this diagnostic was created in different module than it was reported in.
		/// </summary>
		public bool IsExtern { get; }

		/// <summary>
		/// Determines whether this diagnostic is reported on a specific location or compilation-wide.
		/// </summary>
		public bool HasLocation { get; }

		internal DiagnosticData(string title, int id, string docsPath, bool fatal, bool hasLocation, ModuleIdentity? originalModule = null)
		{
			Title = title;
			Id = (IdSection)id;
			IsFatal = fatal;
			HasLocation = hasLocation;

			if (originalModule is not null)
			{
				_originalModule = originalModule;
				_docsPath = $@"{originalModule.Documentation}\{docsPath}";
				IsExtern = true;
			}
			else
			{
				_docsPath = docsPath;
			}
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

		/// <summary>
		/// Returns a <see cref="string"/> that represents the full id of the diagnostic, composed of the <see cref="DurianInfo.IdPrefix"/>, <see cref="ModuleId"/> and <see cref="Id"/>.
		/// </summary>
		public string GetFullId()
		{
			return $"{DurianInfo.IdPrefix}{Module.AnalysisId}{Id}";
		}

		internal void SetModule(ModuleIdentity module)
		{
			if (_originalModule is null)
			{
				_originalModule = module;
				_docsPath = $@"{module.Documentation}\{_docsPath}";
			}

			_module = module;
		}
	}
}
