using System.Collections.Generic;
using Durian.Data;

namespace Durian.DefaultParam
{
	public interface IDefaultParamTarget : IMemberData
	{
		string GetHintName();
		IEnumerable<string> GetUsedNamespaces();
		ref readonly TypeParameterContainer GetTypeParameters();
	}
}
