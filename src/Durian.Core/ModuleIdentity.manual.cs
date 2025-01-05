using System;
using System.Diagnostics.CodeAnalysis;

namespace Durian.Info;

public partial class ModuleIdentity
{
	/// <summary>
	/// Returns a new instance of <see cref="ModuleIdentity"/> corresponding with the specified <see cref="DurianModule"/>.
	/// </summary>
	/// <param name="module"><see cref="DurianModule"/> to get <see cref="ModuleIdentity"/> for.</param>
	/// <exception cref="ArgumentException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
	public static ModuleIdentity GetModule(DurianModule module)
	{
		return module switch
		{
			DurianModule.Core => ModuleRepository.Core,
			DurianModule.DefaultParam => ModuleRepository.DefaultParam,
			DurianModule.FriendClass => ModuleRepository.FriendClass,
			DurianModule.InterfaceTargets => ModuleRepository.InterfaceTargets,
			DurianModule.Development => ModuleRepository.Development,
			DurianModule.CopyFrom => ModuleRepository.CopyFrom,
			DurianModule.GlobalScope => ModuleRepository.GlobalScope,
			DurianModule.None => throw new ArgumentException($"{nameof(DurianModule)}.{nameof(DurianModule.None)} is not a valid Durian module!"),
			_ => throw new ArgumentException($"Unknown {nameof(DurianModule)} value: {module}!")
		};
	}

	/// <summary>
	/// Attempts to return a name of the specified <paramref name="module"/>.
	/// </summary>
	/// <param name="module"><see cref="DurianModule"/> to get the name of.</param>
	/// <param name="moduleName">Name of the module.</param>
	public static bool TryGetName(DurianModule module, [NotNullWhen(true)] out string? moduleName)
	{
		moduleName = module switch
		{
			DurianModule.Core => ModuleNames.Core,
			DurianModule.Development => ModuleNames.Development,
			DurianModule.DefaultParam => ModuleNames.DefaultParam,
			DurianModule.FriendClass => ModuleNames.FriendClass,
			DurianModule.InterfaceTargets => ModuleNames.InterfaceTargets,
			DurianModule.CopyFrom => ModuleNames.CopyFrom,
			DurianModule.GlobalScope => ModuleNames.GlobalScope,
			_ => null
		};

		return moduleName is not null;
	}

	/// <summary>
	/// Attempts to convert the specified <paramref name="moduleName"/> into a value of the <see cref="DurianModule"/> enum.
	/// </summary>
	/// <param name="moduleName"><see cref="string"/> to convert to a value of the <see cref="DurianModule"/> enum.</param>
	/// <param name="module">Value of the <see cref="DurianModule"/> enum created from the <paramref name="moduleName"/>.</param>
	public static bool TryParse([NotNullWhen(true)] string? moduleName, out DurianModule module)
	{
		if (string.IsNullOrWhiteSpace(moduleName))
		{
			module = default;
			return false;
		}

		string name = Utilities.GetParsableIdentityName(moduleName!);

		// Switch expression gives a compilation error here, weird.

		if (name.Equals(ModuleNames.Core, StringComparison.OrdinalIgnoreCase))
		{
			module = DurianModule.Core;
			return true;
		}

		if (name.Equals(ModuleNames.Development, StringComparison.OrdinalIgnoreCase))
		{
			module = DurianModule.Development;
			return true;
		}

		if (name.Equals(ModuleNames.DefaultParam, StringComparison.OrdinalIgnoreCase))
		{
			module = DurianModule.DefaultParam;
			return true;
		}

		if (name.Equals(ModuleNames.FriendClass, StringComparison.OrdinalIgnoreCase))
		{
			module = DurianModule.FriendClass;
			return true;
		}

		if (name.Equals(ModuleNames.InterfaceTargets, StringComparison.OrdinalIgnoreCase))
		{
			module = DurianModule.InterfaceTargets;
			return true;
		}

		if (name.Equals(ModuleNames.CopyFrom, StringComparison.OrdinalIgnoreCase))
		{
			module = DurianModule.CopyFrom;
			return true;
		}

		if (name.Equals(ModuleNames.GlobalScope, StringComparison.OrdinalIgnoreCase))
		{
			module = DurianModule.CopyFrom;
			return true;
		}

		module = default;
		return false;
	}
}
