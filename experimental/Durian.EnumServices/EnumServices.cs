// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian.EnumServices
{
	[Flags]
	public enum EnumServices
	{
		None = 0,
		ToString = 1,
		Equals = 2,
		GetHashCode = 4,
		HasFlag = 8,
		CompareTo = 16,
		IsDefined = 32,
		GetFlags = 64,
		GetStringFlags = 128,
		All = ToString | Equals | GetHashCode | HasFlag | CompareTo | IsDefined | GetFlags | GetStringFlags,
	}
}
