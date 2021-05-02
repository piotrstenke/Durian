using System.Collections.Generic;
using System.Threading;
using Durian.Data;

namespace Durian.DefaultParam
{
	public interface IDefaultParamTarget : IMemberData
	{
		ref readonly TypeParameterContainer TypeParameters { get; }

		IEnumerable<string> GetUsedNamespaces();
		IDefaultParamDeclarationBuilder GetDeclarationBuilder(CancellationToken cancellationToken = default);
	}
}
