using System.Collections.Generic;
using System.Threading;
using Durian.Data;

namespace Durian.DefaultParam
{
	public interface IDefaultParamFilter : ISyntaxFilterWithDiagnostics
	{
		IDefaultParamDeclarationBuilder GetDeclarationBuilder(IDefaultParamTarget target, CancellationToken cancellationToken = default);
		IEnumerable<IDefaultParamTarget> Filtrate();
	}
}
