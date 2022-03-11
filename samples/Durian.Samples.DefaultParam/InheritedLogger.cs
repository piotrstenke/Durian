// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Configuration;
using System;

namespace Durian.Samples.DefaultParam
{
	[DefaultParamConfiguration(TypeConvention = DPTypeConvention.Inherit)]
	public class InheritedLogger<[DefaultParam(typeof(string))]T> : Logger<T> where T : IEquatable<string>
	{
	}
}
