using System.Collections.Generic;
using System.Threading;

namespace Durian.DefaultParam
{
	public interface IDefaultParamFilter : IGeneratorSyntaxFilterWithDiagnostics
	{
		IDefaultParamDeclarationBuilder GetDeclarationBuilder(IDefaultParamTarget target, CancellationToken cancellationToken = default);
		new IEnumerable<IDefaultParamTarget> Filtrate();
	}
}
