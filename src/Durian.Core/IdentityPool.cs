// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Concurrent;

namespace Durian.Info
{
	internal static class IdentityPool
	{
		public static ConcurrentDictionary<string, ModuleIdentity> Modules { get; } = new ConcurrentDictionary<string, ModuleIdentity>();

		public static ConcurrentDictionary<string, PackageIdentity> Packages { get; } = new ConcurrentDictionary<string, PackageIdentity>();

		public static ConcurrentDictionary<string, TypeIdentity> Types { get; } = new ConcurrentDictionary<string, TypeIdentity>();
	}
}