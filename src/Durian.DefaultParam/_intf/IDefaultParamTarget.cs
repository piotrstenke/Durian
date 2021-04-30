using System.Collections.Generic;
using Durian.Data;

namespace Durian.DefaultParam
{
	public interface IDefaultParamTarget : IMemberData
	{
		ref readonly TypeParameterContainer TypeParameters { get; }
		IEnumerable<string> GetUsedNamespaces();
	}
}
