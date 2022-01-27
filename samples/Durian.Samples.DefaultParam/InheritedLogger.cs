// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Durian.Configuration;

namespace Durian.Samples.DefaultParam
{
	[DefaultParamConfiguration(TypeConvention = DPTypeConvention.Inherit)]
	public class InheritedLogger<[DefaultParam(typeof(string))]T> : Logger<T> where T : IEquatable<string>
	{
	}
}