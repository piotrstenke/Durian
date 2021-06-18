// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis.DefaultParam
{
	/// <summary>
	/// Filtrates and validates nodes collected by a <see cref="DefaultParamSyntaxReceiver"/>.
	/// </summary>
	public interface IDefaultParamFilter : IDefaultParamFilter<IDefaultParamTarget>
	{
	}
}
