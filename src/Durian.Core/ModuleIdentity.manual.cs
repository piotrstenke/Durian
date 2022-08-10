// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian.Info
{
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
				DurianModule.None => throw new ArgumentException($"{nameof(DurianModule)}.{nameof(DurianModule.None)} is not a valid Durian module!"),
				_ => throw new ArgumentException($"Unknown {nameof(DurianModule)} value: {module}!")
			};
		}
	}
}
